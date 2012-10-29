namespace LINQPad.Extensibility.DataContext
{
    using System;

    public interface IDynamicSchemaOptions
    {
        bool ExcludeRoutines { get; set; }

        bool NoCapitalization { get; set; }

        bool NoPluralization { get; set; }
    }
}

