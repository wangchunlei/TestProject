namespace LINQPad
{
    using LINQPad.Extensibility.DataContext;
    using System;

    internal class ShadowAssemblyManager
    {
        public static bool IsShadowable(string assemPath)
        {
            return (((!assemPath.StartsWith(PathHelper.ProgramFilesX86, StringComparison.OrdinalIgnoreCase) && !assemPath.StartsWith(PathHelper.ProgramFilesX64, StringComparison.OrdinalIgnoreCase)) && !assemPath.StartsWith(DCDriverLoader.ThirdPartyDriverFolder, StringComparison.OrdinalIgnoreCase)) && !assemPath.StartsWith(PathHelper.Windows, StringComparison.OrdinalIgnoreCase));
        }
    }
}

