namespace LINQPad
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal abstract class OptionsLive
    {
        private Dictionary<string, object> _values = new Dictionary<string, object>();

        protected OptionsLive()
        {
        }

        private string GetFullPath(string key)
        {
            return Path.Combine(this.BaseFolder, key + ".txt");
        }

        protected T Read<T>(string key)
        {
            object obj2;
            if (!this._values.TryGetValue(key, out obj2))
            {
                string fullPath = this.GetFullPath(key);
                obj2 = default(T);
                if (File.Exists(fullPath))
                {
                    string str2 = File.ReadAllText(fullPath);
                    if (!string.IsNullOrEmpty(str2))
                    {
                        try
                        {
                            obj2 = Convert.ChangeType(str2, typeof(T));
                        }
                        catch
                        {
                        }
                    }
                }
                this._values[key] = obj2;
            }
            return (T) obj2;
        }

        protected bool Write(string key, object value)
        {
            object obj2;
            if (this._values.TryGetValue(key, out obj2) && object.Equals(obj2, value))
            {
                return false;
            }
            this.GetFullPath(key);
            try
            {
                File.WriteAllText(this.GetFullPath(key), (value == null) ? "" : value.ToString());
                this._values[key] = value;
            }
            catch
            {
            }
            return true;
        }

        public abstract string BaseFolder { get; }
    }
}

