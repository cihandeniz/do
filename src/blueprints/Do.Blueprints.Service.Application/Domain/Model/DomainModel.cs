﻿namespace Do.Domain.Model;

public record DomainModel(List<AssemblyModel> AssemblyModels)
{
    public static DomainModel From(params Type[] args)
    {
        var descriptor = new DomainDescriptor();
        args.ToList().ForEach(t => descriptor.AddType(t));

        return DomainModelBuilder.CreateBuilder(descriptor).Build();
    }
}
