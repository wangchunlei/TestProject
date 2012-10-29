namespace LINQPad
{
    using System;
    using System.IO;

    internal class DomainIsolator : IDisposable
    {
        private AppDomain _domain;

        public DomainIsolator(AppDomain domain)
        {
            this._domain = domain;
        }

        public DomainIsolator(string friendlyName) : this(friendlyName, null, null)
        {
        }

        public DomainIsolator(string friendlyName, string configFile, string appBase) : this(AppDomainUtil.CreateDomain(friendlyName, configFile, appBase))
        {
        }

        public void Dispose()
        {
            Util.UnloadAppDomain(this._domain);
        }

        public T GetInstance<T>() where T: MarshalByRefObject
        {
            return (T) this.GetInstance(typeof(T));
        }

        public object GetInstance(Type t)
        {
            try
            {
                return this._domain.CreateInstanceFromAndUnwrap(t.Assembly.Location, t.FullName);
            }
            catch (FileNotFoundException)
            {
                return this._domain.CreateInstanceAndUnwrap(t.Assembly.FullName, t.FullName);
            }
        }

        public AppDomain Domain
        {
            get
            {
                return this._domain;
            }
        }
    }
}

