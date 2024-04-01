using Do.Architecture;
using Do.Business.Default.RestApiConventions;
using Do.Domain.Configuration;
using Do.Orm;
using Do.RestApi.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Do.Business.Default;

public class DefaultBusinessFeature(List<Assembly> _domainAssemblies)
    : IFeature<BusinessConfigurator>
{
    const BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    public void Configure(LayerConfigurator configurator)
    {
        configurator.ConfigureDomainTypeCollection(types =>
        {
            foreach (var assembly in _domainAssemblies)
            {
                types.AddFromAssembly(assembly, except: type =>
                    (type.IsSealed && type.IsAbstract) || // if type is static
                    type.IsAssignableTo(typeof(Exception)) ||
                    type.IsAssignableTo(typeof(Attribute)) ||
                    type.IsAssignableTo(typeof(Delegate))
                );
            }
        });

        configurator.ConfigureDomainModelBuilder(builder =>
        {
            builder.BindingFlags.Constructor = _bindingFlags;
            builder.BindingFlags.Method = _bindingFlags;
            builder.BindingFlags.Property = _bindingFlags;

            builder.BuildLevels.Add(context => context.DomainTypesContain(context.Type), BuildLevels.Members);
            builder.BuildLevels.Add(context => context.Type.IsGenericType && context.DomainTypesContain(context.Type.GetGenericTypeDefinition()), BuildLevels.Members);
            builder.BuildLevels.Add(type => !type.IsValueType, BuildLevels.Inheritance);
            builder.BuildLevels.Add(type => type.IsGenericType, BuildLevels.Generics);

            builder.Index.Type.Add<ServiceAttribute>();
            builder.Index.Type.Add<TransientAttribute>();
            builder.Index.Type.Add<ScopedAttribute>();
            builder.Index.Type.Add<SingletonAttribute>();
            builder.Index.Method.Add<ApiMethodAttribute>();

            builder.Metadata.Type.Add(new DataClassAttribute(),
                when: type =>
                    type.TryGetMembers(out var members) &&
                    members.Methods.Contains("<Clone>$"), // if type is record
                order: int.MinValue
            );
            builder.Metadata.Type.Add(new ServiceAttribute(),
                when: type =>
                    type.IsPublic &&
                    !type.IsAssignableTo<IEnumerable>() &&
                    !type.IsValueType &&
                    !type.IsGenericMethodParameter &&
                    !type.IsGenericTypeParameter &&
                    !type.IsGenericTypeDefinition &&
                    type.TryGetMembers(out var members) &&
                    !members.Has<DataClassAttribute>()
            );
            builder.Metadata.Type.Add(new SingletonAttribute(),
               when: type =>
                   type.IsClass && !type.IsAbstract &&
                   type.TryGetMembers(out var members) &&
                   members.Has<ServiceAttribute>() &&
                   !members.Has<TransientAttribute>() &&
                   !members.Has<ScopedAttribute>() &&
                   members.Properties.All(p => !p.IsPublic),
               order: int.MaxValue
            );
            builder.Metadata.Type.Add(new TransientAttribute(),
                when: type =>
                    type.IsClass && !type.IsAbstract &&
                    type.TryGetMembers(out var members) &&
                    members.Has<ServiceAttribute>() &&
                    members.TryGetMethods("With", out var method) &&
                    method.Any(o =>
                        o.ReturnType is not null &&
                        (
                            o.ReturnType == type ||
                            (o.ReturnType.IsAssignableTo<Task>() && o.ReturnType.TryGetGenerics(out var returnTypeGenerics) && returnTypeGenerics.GenericTypeArguments.Contains(type))
                        )
                    )
            );
            builder.Metadata.Type.Add(new ScopedAttribute(),
                when: type =>
                    type.IsClass && !type.IsAbstract &&
                    type.TryGetMetadata(out var metadata) &&
                    metadata.Has<ServiceAttribute>() &&
                    type.IsAssignableTo<IScoped>()
            );

            builder.Metadata.Method.Add(new ApiMethodAttribute(),
                when: method => method.Overloads.Any(m => m.IsPublic)
            );
        });

        configurator.ConfigureServiceCollection(services =>
        {
            var domainModel = configurator.Context.GetDomainModel();
            foreach (var type in domainModel.Types.Having<TransientAttribute>())
            {
                type.Apply(t =>
                {
                    services.AddTransientWithFactory(t);
                    type.GetInheritance().Interfaces
                        .Where(i => i.Model.TryGetMetadata(out var metadata) && metadata.Has<ServiceAttribute>())
                        .Apply(i => services.AddTransientWithFactory(i, t));
                });
            }

            foreach (var type in domainModel.Types.Having<ScopedAttribute>())
            {
                type.Apply(t =>
                {
                    services.AddScopedWithFactory(t);
                    type.GetInheritance().Interfaces
                        .Where(i => i.Model.TryGetMetadata(out var metadata) && metadata.Has<ServiceAttribute>())
                        .Apply(i => services.AddScopedWithFactory(i, t));
                });
            }

            foreach (var type in domainModel.Types.Having<SingletonAttribute>())
            {
                type.Apply(t =>
                {
                    services.AddSingleton(t);
                    type.GetInheritance().Interfaces
                        .Where(i => i.Model.TryGetMetadata(out var metadata) && metadata.Has<ServiceAttribute>())
                        .Apply(i => services.AddSingleton(i, t, forward: true));
                });
            }
        });

        configurator.ConfigureApiModel(api =>
        {
            _domainAssemblies.ForEach(a => api.Reference.Add(a.GetName().FullName, a));

            var domainModel = configurator.Context.GetDomainModel();
            foreach (var type in domainModel.Types.Having<ServiceAttribute>())
            {
                if (type.FullName is null) { continue; }
                if (!type.GetMetadata().Has<SingletonAttribute>()) { continue; } // TODO for now only singleton

                var controller = new ControllerModel(type.Name);
                foreach (var method in type.GetMembers().Methods.Having<ApiMethodAttribute>())
                {
                    var overload = method.Overloads.OrderByDescending(o => o.Parameters.Count).First(); // overload with most parameters
                    if (overload.ReturnType is null) { continue; }

                    // TODO for now only primitive, list of primitive and entity parameters
                    if (overload.Parameters
                            .Count(p =>
                                !(p.ParameterType.IsValueType || p.ParameterType.IsAssignableTo<string>()) && // primitive
                                !(
                                    p.ParameterType.IsGenericType &&
                                    p.ParameterType.TryGetGenerics(out var parameterTypeGenerics) &&
                                    parameterTypeGenerics.GenericTypeDefinition?.IsAssignableTo(typeof(List<>)) == true &&
                                    (
                                        parameterTypeGenerics.GenericTypeArguments.First().Model.IsValueType ||
                                        parameterTypeGenerics.GenericTypeArguments.First().Model.IsAssignableTo(typeof(List<>))
                                    )
                                ) && // list of primitives
                                !(p.ParameterType.TryGetMetadata(out var parameterTypeMetadata) && parameterTypeMetadata.Has<EntityAttribute>()) // entity
                            ) > 0
                    ) { continue; }

                    if (!overload.ReturnType.IsAssignableTo(typeof(void)) &&
                        !overload.ReturnType.IsAssignableTo(typeof(Task))) { continue; } // TODO for now only void

                    controller.Action.Add(
                        method.Name,
                        new(
                            Name: method.Name,
                            Method: HttpMethod.Post,
                            Route: $"generated/{type.Name}/{method.Name}",
                            Return: new(async: overload.ReturnType.IsAssignableTo(typeof(Task))),
                            FindTargetStatement: "target",
                            InvokedMethodName: method.Name
                        )
                        {
                            Parameters = [
                                new(ParameterModelFrom.Services, type.FullName, "target"),
                                .. overload.Parameters.Select(p => new ParameterModel(ParameterModelFrom.Body, p.ParameterType.CSharpFriendlyFullName, p.Name))
                            ]
                        }
                    );
                }

                api.Controller.Add(controller.Name, controller);
            }
        });

        configurator.ConfigureApiModelConventions(conventions =>
        {
            conventions.Add(new LookupEntityByIdConvention(configurator.Context.GetDomainModel()));
        });

        configurator.ConfigureMvcNewtonsoftJsonOptions(options =>
        {
            options.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
        });
    }
}
