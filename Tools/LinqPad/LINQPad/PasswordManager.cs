namespace LINQPad
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    internal class PasswordManager
    {
        private static string _passwordFolder = Path.Combine(Program.LocalUserDataFolder, "Passwords");

        private static string Decrypt(byte[] encrypted)
        {
            byte[] optionalEntropy = encrypted.Take<byte>(0x20).ToArray<byte>();
            byte[] encryptedData = encrypted.Skip<byte>(0x20).ToArray<byte>();
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(encryptedData, optionalEntropy, DataProtectionScope.CurrentUser));
        }

        public static void DeletePassword(string name)
        {
            string filePath = GetFilePath(name);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static byte[] Encrypt(string value)
        {
            byte[] data = new byte[0x20];
            RandomNumberGenerator.Create().GetBytes(data);
            byte[] second = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), data, DataProtectionScope.CurrentUser);
            return data.Concat<byte>(second).ToArray<byte>();
        }

        public static string[] GetAllPasswordNames()
        {
            return (from f in Directory.GetFiles(PasswordFolder)
                select GetName(f) into f
                where f != null
                select f).ToArray<string>();
        }

        private static string GetFilePath(string name)
        {
            string str = string.Concat((from b in Encoding.UTF8.GetBytes(name.Trim().ToLowerInvariant()) select b.ToString("X2")).ToArray<string>());
            return Path.Combine(PasswordFolder, str);
        }

        private static string GetName(string filePath)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                byte[] bytes = new byte[fileName.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(fileName.Substring(i * 2, 2), 0x10);
                }
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return null;
            }
        }

        public static string GetPassword(string name)
        {
            string filePath = GetFilePath(name);
            if (!File.Exists(filePath))
            {
                return null;
            }
            try
            {
                return Decrypt(File.ReadAllBytes(filePath));
            }
            catch
            {
                return null;
            }
        }

        public static void SetPassword(string name, string password)
        {
            if (password == null)
            {
                DeletePassword(name);
            }
            else
            {
                File.WriteAllBytes(GetFilePath(name), Encrypt(password));
            }
        }

        private static string PasswordFolder
        {
            get
            {
                if (!Directory.Exists(_passwordFolder))
                {
                    Directory.CreateDirectory(_passwordFolder);
                }
                return _passwordFolder;
            }
        }
    }
}

