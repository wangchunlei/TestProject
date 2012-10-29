namespace LINQPad
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal class PluginAssembly
    {
        private ProcessorArchitecture? _architecture;
        private static Dictionary<string, PluginAssembly> _assemblies = new Dictionary<string, PluginAssembly>(StringComparer.InvariantCultureIgnoreCase);
        private static string _folder;
        private DateTime _lastWriteTime;
        private string _path;

        public static IEnumerable<string> GetCompatibleAssemblies(bool myExtensions)
        {
            string pluginsFolder = UserOptions.Instance.GetPluginsFolder(true);
            if (pluginsFolder != _folder)
            {
                _folder = pluginsFolder;
                _assemblies.Clear();
            }
            if (_folder == null)
            {
                return new string[0];
            }
            Update();
            return (from a in _assemblies.Values
                where !myExtensions || !Path.GetFileNameWithoutExtension(a._path).StartsWith("myextensions.fw", StringComparison.InvariantCultureIgnoreCase)
                where a._architecture.HasValue && IsCompatible(a._architecture.Value)
                select a._path);
        }

        private static ProcessorArchitecture? GetProcessorArchitecture(string fileName)
        {
            ProcessorArchitecture? nullable2;
            FileStream input = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                byte[] buffer = new byte[300];
                new BinaryReader(input).Read(buffer, 0, 0x80);
                if ((buffer[0] != 0x4d) || (buffer[1] != 90))
                {
                    return null;
                }
                int num = BitConverter.ToInt32(buffer, 60);
                input.Seek((long) num, SeekOrigin.Begin);
                new BinaryReader(input).Read(buffer, 0, 0x18);
                if ((buffer[0] != 80) || (buffer[1] != 0x45))
                {
                    return null;
                }
                new BinaryReader(input).Read(buffer, 0, 0xe0);
                if ((buffer[0] != 11) || (buffer[1] != 1))
                {
                    return null;
                }
                if (BitConverter.ToInt32(buffer, 0xd0) == 0)
                {
                    return null;
                }
                nullable2 = new ProcessorArchitecture?(AssemblyName.GetAssemblyName(fileName).ProcessorArchitecture);
            }
            catch
            {
                nullable2 = null;
            }
            finally
            {
                if (input != null)
                {
                    input.Dispose();
                }
            }
            return nullable2;
        }

        private static bool IsCompatible(ProcessorArchitecture pa)
        {
            if ((pa == ProcessorArchitecture.X86) && (IntPtr.Size == 8))
            {
                return false;
            }
            if ((pa == ProcessorArchitecture.IA64) && (IntPtr.Size == 4))
            {
                return false;
            }
            if ((pa == ProcessorArchitecture.Amd64) && (IntPtr.Size == 4))
            {
                return false;
            }
            return true;
        }

        private static void Update()
        {
            List<PluginAssembly> source = new List<PluginAssembly>();
            foreach (FileInfo info in new DirectoryInfo(_folder).GetFiles())
            {
                if (((info.Name.ToLowerInvariant() != "linqpad.exe") && ((info.Extension.ToLowerInvariant() == ".exe") || (info.Extension.ToLowerInvariant() == ".dll"))) && !Path.GetFileNameWithoutExtension(info.Name).ToLowerInvariant().EndsWith(".fw35"))
                {
                    PluginAssembly assembly;
                    if (!(_assemblies.TryGetValue(info.FullName, out assembly) && !(assembly._lastWriteTime != info.LastWriteTimeUtc)))
                    {
                        assembly = new PluginAssembly {
                            _path = info.FullName,
                            _architecture = GetProcessorArchitecture(info.FullName),
                            _lastWriteTime = info.LastWriteTimeUtc
                        };
                    }
                    source.Add(assembly);
                }
            }
            _assemblies = source.ToDictionary<PluginAssembly, string>(a => a._path);
        }
    }
}

