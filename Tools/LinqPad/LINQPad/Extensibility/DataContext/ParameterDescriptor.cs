namespace LINQPad.Extensibility.DataContext
{
    using System;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class ParameterDescriptor
    {
        public ParameterDescriptor(string parameterName, string fullTypeName)
        {
            this.ParameterName = parameterName;
            this.FullTypeName = fullTypeName;
        }

        public string FullTypeName { get; private set; }

        public string ParameterName { get; private set; }
    }
}

