﻿namespace Do.Domain.Model;

public record ConstructorModel(
    string Name,
    TypeModel Type,
    ModelCollection<ParameterModel> Parameters,
    bool IsPublic
) : IModel
{
    public string Id { get; } = $"{Name}[{string.Join(',', Parameters.Select(p => p.Id))}]";
}