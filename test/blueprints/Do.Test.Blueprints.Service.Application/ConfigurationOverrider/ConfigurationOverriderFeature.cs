﻿using Do.Architecture;
using Do.Authentication.FixedToken;
using Microsoft.OpenApi.Models;

namespace Do.Test.ConfigurationOverrider;

public class ConfigurationOverriderFeature : IFeature
{
    public void Configure(LayerConfigurator configurator)
    {
        configurator.ConfigureTypeCollection(types =>
        {
            types.Add<OperationWithGenericParameter<Entity>>();
        });

        configurator.ConfigureAutoPersistenceModel(model =>
        {
            model.Override<Entity>(x => x.Map(e => e.String).Length(500));
        });

        configurator.ConfigureSwaggerGenOptions(swaggerGenOptions =>
        {
            swaggerGenOptions.AddSecurityDefinition("AdditionalSecurity",
                new()
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = "X-Secret",
                    Description = "Enter secret information",
                }
            );

            swaggerGenOptions.AddSecurityRequirementToOperationsThatUse<Middleware>("AdditionalSecurity");
            swaggerGenOptions.AddParameterToOperationsThatUse<Middleware>("X-Security", @in: ParameterLocation.Header, required: true);
        });

        configurator.ConfigureCompilerOptions(options =>
        {
            options.AddReferenceFrom<Program>();
        });

        configurator.ConfigureCodeCollection(codes =>
        {
            codes.AddCode(Codes.Entities.Code, assemblyName: "Controllers");
            codes.AddCode(Codes.Parents.Code, assemblyName: "Controllers");
            codes.AddCode(Codes.Remote.Code, assemblyName: "Controllers");
            codes.AddCode(Codes.Singleton.Code, assemblyName: "Controllers");
        });

        configurator.ConfigureApplicationParts(applicationParts =>
        {
            applicationParts.Add(new(configurator.Context.GetGeneratedAssembly("Controllers")));
        });
    }
}
