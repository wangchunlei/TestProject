namespace LINQPad
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.AccessControl;
    using System.Security.Principal;

    internal class FileUtil
    {
        internal static void AssignUserPermissionsFile(string path)
        {
            try
            {
                FileSecurity accessControl = File.GetAccessControl(path);
                if (!UserHasRights(accessControl))
                {
                    accessControl.AddAccessRule(new FileSystemAccessRule(GetUsersAccount(), FileSystemRights.FullControl, AccessControlType.Allow));
                    File.SetAccessControl(path, accessControl);
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (Exception exception)
            {
                Log.Write(exception, "AssignUserPermissions");
            }
        }

        internal static void AssignUserPermissionsToFolder(string path)
        {
            try
            {
                DirectorySecurity accessControl = Directory.GetAccessControl(path);
                if (!UserHasRights(accessControl))
                {
                    FileSystemAccessRule rule = new FileSystemAccessRule(GetUsersAccount().ToString(), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow);
                    accessControl.AddAccessRule(rule);
                    Directory.SetAccessControl(path, accessControl);
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (Exception exception)
            {
                Log.Write(exception, "AssignUserPermissions");
            }
        }

        internal static NTAccount GetUsersAccount()
        {
            SecurityIdentifier identifier = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            return (NTAccount) identifier.Translate(typeof(NTAccount));
        }

        internal static bool UserHasRights(FileSystemSecurity sec)
        {
            NTAccount usersAccount = GetUsersAccount();
            return sec.GetAccessRules(true, true, typeof(NTAccount)).OfType<FileSystemAccessRule>().Any<FileSystemAccessRule>(r => ((((r.FileSystemRights == FileSystemRights.FullControl) && (r.AccessControlType == AccessControlType.Allow)) && (r.InheritanceFlags == (InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit))) && (r.IdentityReference == usersAccount)));
        }
    }
}

