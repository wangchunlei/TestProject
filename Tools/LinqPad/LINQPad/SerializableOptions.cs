namespace LINQPad
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    internal abstract class SerializableOptions
    {
        protected SerializableOptions()
        {
        }

        public void Deserialize()
        {
            if (File.Exists(this.FullPath))
            {
                XElement element = XElement.Load(this.FullPath);
                foreach (FieldInfo info in this.GetSerializableFields())
                {
                    string str = (string) element.Element((XName) (element.Name.Namespace + info.Name));
                    if (!string.IsNullOrEmpty(str))
                    {
                        try
                        {
                            info.SetValue(this, XmlHelper.FromXmlString(str, info.FieldType));
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private FieldInfo[] GetSerializableFields()
        {
            return (from f in base.GetType().GetFields()
                where f.GetCustomAttributes(false).OfType<SerializeAttribute>().Any<SerializeAttribute>()
                select f).ToArray<FieldInfo>();
        }

        public void Save()
        {
            string directoryName = Path.GetDirectoryName(this.FullPath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            XNamespace ns = "http://schemas.datacontract.org/2004/07/LINQPad";
            IEnumerable<XElement> content = from f in this.GetSerializableFields()
                let value = f.GetValue(this)
                where value != null
                where !(value is bool) || 1.Equals(value)
                orderby f.Name
                select new XElement((XName) (ns + f.Name), value);
            new XElement((XName) (ns + base.GetType().Name), content).Save(this.FullPath);
        }

        public abstract string FullPath { get; }
    }
}

