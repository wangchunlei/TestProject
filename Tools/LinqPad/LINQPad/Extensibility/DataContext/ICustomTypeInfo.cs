namespace LINQPad.Extensibility.DataContext
{
    using System;

    public interface ICustomTypeInfo
    {
        string GetCustomTypeDescription();
        string[] GetCustomTypesInAssembly();
        string[] GetCustomTypesInAssembly(string baseTypeName);
        bool IsEquivalent(ICustomTypeInfo other);

        string CustomAssemblyPath { get; set; }

        string CustomMetadataPath { get; set; }

        string CustomTypeName { get; set; }
    }
}

