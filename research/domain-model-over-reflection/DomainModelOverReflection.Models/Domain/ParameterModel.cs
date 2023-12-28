﻿namespace DomainModelOverReflection.Models.Domain;

#pragma warning disable IDE1006 // Naming Styles
public class ParameterModel
{
    public readonly string Name;
    public readonly string Type;
    public ParameterModel(string name, string type)
    {
        Name = name;
        Type = type;
    }
}
