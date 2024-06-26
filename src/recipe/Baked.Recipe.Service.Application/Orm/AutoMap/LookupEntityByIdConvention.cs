﻿using Baked.Business;
using Baked.Domain.Model;
using Baked.RestApi.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Baked.Orm.AutoMap;

public class LookupEntityByIdConvention(DomainModel _domain)
    : IApiModelConvention<ParameterModelContext>
{
    public void Apply(ParameterModelContext context)
    {
        if (context.Action.MappedMethod is null) { return; }
        if (context.Action.MappedMethod.Has<InitializerAttribute>()) { return; }
        if (!context.Parameter.IsInvokeMethodParameter) { return; }
        if (!context.Parameter.TypeModel.TryGetQueryContextType(_domain, out var queryContextType)) { return; }

        var notNull = context.Parameter.MappedParameter?.Has<NotNullAttribute>() == true;
        var queryContextParameter = context.Action.AddQueryContextAsService(queryContextType);

        context.Parameter.ConvertToId(nullable: !notNull);
        context.Parameter.LookupRenderer =
            p => queryContextParameter.BuildSingleBy(p,
                notNullValueExpression: $"(Guid){p}",
                nullable: !notNull
            );
    }
}