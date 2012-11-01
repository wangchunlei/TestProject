using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace ReadRegistry
{
    class Program
    {
        static void Main(string[] args)
        {
            var key = @"SOFTWARE\Wow6432Node\Microsoft\Updates";

            Console.ReadKey(false);
        }
        static IEnumerable<RegistryKey> GetSubKeys(RegistryKey keyParentArg)
        {
            var keysFound = new List<RegistryKey>();

            try
            {
                if (keyParentArg.SubKeyCount > 0)
                {
                    foreach (var subKeyName in keyParentArg.GetSubKeyNames())
                    {
                        var keyChild = keyParentArg.OpenSubKey(subKeyName);
                        if (keyChild != null)
                        {
                            keysFound.Add(keyChild);
                        }

                        var keyGrandChildren = GetSubKeys(keyChild);

                        if (keyGrandChildren != null)
                        {
                            keysFound.AddRange(keyGrandChildren);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.GetBaseException());
                throw;
            }
            return keysFound;
        }
        private static void ReadReg(string path)
        {
            var register_key = path;
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(register_key))
            {
                if (key == null)
                    return;

                GetSubKey(key);
            }
        }

        private static void GetSubKey(RegistryKey key)
        {
            foreach (string skName in key.GetSubKeyNames())
            {
                using (RegistryKey sk = key.OpenSubKey(skName))
                {
                    if (sk == null)
                    {
                        continue;
                    }
                    if (sk.SubKeyCount > 0)
                    {
                        GetSubKey(sk);

                    }
                    if (sk.ValueCount > 0)
                    {
                        foreach (var valueName in sk.GetValueNames())
                        {
                            //Console.WriteLine(valueName);
                        }
                    }
                }
            }
        }
    }
}
