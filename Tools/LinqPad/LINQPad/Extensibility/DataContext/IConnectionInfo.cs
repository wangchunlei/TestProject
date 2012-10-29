namespace LINQPad.Extensibility.DataContext
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    public interface IConnectionInfo
    {
        string Decrypt(string data);
        string Encrypt(string data);

        string AppConfigPath { get; set; }

        ICustomTypeInfo CustomTypeInfo { get; }

        IDatabaseInfo DatabaseInfo { get; }

        string DisplayName { get; set; }

        XElement DriverData { get; set; }

        IDynamicSchemaOptions DynamicSchemaOptions { get; }

        bool Persist { get; set; }

        IDictionary<string, object> SessionData { get; }
    }
}

