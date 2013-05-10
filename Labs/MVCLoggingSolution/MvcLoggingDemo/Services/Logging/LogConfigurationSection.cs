using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcLoggingDemo.Services.Logging
{            
    // Use the following web.config file.
    // <?xml version="1.0" encoding="utf-8" ?>
    // <configuration>
    //    <configSections>
    //        <section name="logConfiguration" type="MvcLoggingDemo.Services.Logging.LogConfigurationSection" />
    //    </configSections>
    //    <logConfiguration>
    //        <logProviders>
    //            <clear />
    //            <add name="Elmah" type="MvcLoggingDemo.Models.Repository.ElmahRepository" />
    //            <add name="NLog" type="MvcLoggingDemo.Models.Repository.NLogRepository" />
    //        </logProviders>
    //    </logConfiguration>
    // </configuration>

    // Define a custom section named LogConfigurationSection containing a
    // LogProviderCollection collection of LogProviderConfigElement elements.
    // The collection is wrapped in an element named "logProviders" in the
    // web.config file.
    // LogProviderCollection and LogProviderConfigElement classes are defined below.
    // This is the key class that shows how to use the ConfigurationCollectionAttribute.
    public class LogConfigurationSection : ConfigurationSection
    {
        // Declare the urls collection property.
        // Note: the "IsDefaultCollection = false" instructs 
        // .NET Framework to build a nested section of 
        // the kind <logProviders>...</logProviders>.
        [ConfigurationProperty("logProviders", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(LogProviderCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public LogProviderCollection LogProviders
        {
            get
            {
                LogProviderCollection logProvidersCollection = (LogProviderCollection)base["logProviders"];
                return logProvidersCollection;
            }
        }

    }

    // Define the UrlsCollection that will contain the UrlsConfigElement
    // elements.
    public class LogProviderCollection : ConfigurationElementCollection
    {
        public LogProviderCollection()
        {
            // When the collection is created, always add one element 
            // with the default values. (This is not necessary; it is
            // here only to illustrate what can be done; you could 
            // also create additional elements with other hard-coded 
            // values here.)
            LogProviderConfigElement element = (LogProviderConfigElement)CreateNewElement();
            Add(element);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new LogProviderConfigElement();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((LogProviderConfigElement)element).Name;
        }

        public LogProviderConfigElement this[int index]
        {
            get
            {
                return (LogProviderConfigElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public LogProviderConfigElement this[string Name]
        {
            get
            {
                return (LogProviderConfigElement)BaseGet(Name);
            }
        }

        public int IndexOf(LogProviderConfigElement url)
        {
            return BaseIndexOf(url);
        }

        public void Add(LogProviderConfigElement url)
        {
            BaseAdd(url);
        }
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(LogProviderConfigElement url)
        {
            if (BaseIndexOf(url) >= 0)
                BaseRemove(url.Name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
        }
    }

    // Define the element type contained by the UrlsCollection
    // collection.
    public class LogProviderConfigElement : ConfigurationElement
    {
        public LogProviderConfigElement(String name, String type)
        {
            this.Name = name;
            this.Type = type;
        }

        public LogProviderConfigElement()
        {
            // Attributes on the properties provide default values.
        }

        [ConfigurationProperty("name", DefaultValue = "NLog", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("type", DefaultValue = "MvcLoggingDemo.Models.Repository.NLogRepository", IsRequired = true)]
        public string Type
        {
            get
            {
                return (string)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }

    }

}