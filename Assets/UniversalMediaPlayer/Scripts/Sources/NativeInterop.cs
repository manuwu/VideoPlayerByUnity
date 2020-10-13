using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UMP.Wrappers;
using UnityEngine;

namespace UMP
{
    internal class NativeInterop
    {
        private const string LIB_WIN_KERNEL = "kernel32";
        private const string LIB_WIN_ADVAPI = "advapi32";
        private const string LIB_UNX = "libdl";
        private const string LIB_LIN = "__Internal";

        private const int LIN_RTLD_NOW = 2;
        private const string EXT_PLUGINS_FOLDER_NAME = "/vlc/plugins";

        private const string MAC_APPS_FOLDER_NAME = "/Applications";
        private const string LIN_86_APPS_FOLDER_NAME = "/usr/lib";
        private const string LIN_64_APPS_FOLDER_NAME = "/usr/lib64";

        private const string MAC_BUNDLE_NAME = "/libvlc.bundle";
        private const string MAC_PACKAGE_NAME = "/vlc.app";
        private const string MAC_PACKAGE_LIB_PATH = @"/Contents/MacOS/lib";
        private const string VLC_EXT_ENV = "VLC_PLUGIN_PATH";

        private static readonly Dictionary<string, Delegate> _interopDelegates = new Dictionary<string, Delegate>();

        private static class WindowsInterop
        {
            [DllImport(LIB_WIN_KERNEL, SetLastError = true, CharSet = CharSet.Ansi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetDllDirectory([MarshalAs(UnmanagedType.LPStr)]string lpPathName);

            [DllImport(LIB_WIN_KERNEL, SetLastError = true, CharSet = CharSet.Ansi)]
            internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

            [DllImport(LIB_WIN_KERNEL, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport(LIB_WIN_KERNEL, SetLastError = true)]
            internal static extern bool FreeLibrary(IntPtr hModule);

            [DllImport(LIB_WIN_ADVAPI, SetLastError = true)]
            internal static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, int ulOptions, int samDesired, out UIntPtr hkResult);

            [DllImport(LIB_WIN_ADVAPI, SetLastError = true, CharSet = CharSet.Ansi)]
            internal static extern uint RegQueryValueEx(UIntPtr hKey, [MarshalAs(UnmanagedType.LPStr)]string lpValueName, int lpReserved, out uint lpType, StringBuilder lpData, ref uint lpcbData);

            [DllImport(LIB_WIN_ADVAPI, SetLastError = true)]
            internal static extern int RegCloseKey(UIntPtr hKey);
        }
        
        private static class MacInterop
        {
            [DllImport(LIB_UNX, SetLastError = true)]
            internal static extern IntPtr dlopen(string fileName, int flags);

            [DllImport(LIB_UNX, SetLastError = true)]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport(LIB_UNX)]
            internal static extern int dlclose(IntPtr handle);
        }

        private static class LinuxInterop
        {
            [DllImport(LIB_LIN, SetLastError = true)]
            internal static extern IntPtr dlopen(string fileName, int flags);

            [DllImport(LIB_LIN, SetLastError = true)]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport(LIB_LIN)]
            internal static extern int dlclose(IntPtr handle);
        }

        private static bool SetLibraryDirectory(string path)
        {
            var supportedPlatform = UMPSettings.RuntimePlatform;

            if (supportedPlatform == UMPSettings.Platforms.Win)
                return WindowsInterop.SetDllDirectory(path);

            if (supportedPlatform == UMPSettings.Platforms.Mac)
            {
                var pluginPath = Directory.GetParent(path.TrimEnd(Path.DirectorySeparatorChar)).FullName;
                pluginPath = Path.Combine(pluginPath, "plugins");

                if (Directory.Exists(pluginPath))
                {
                    Environment.SetEnvironmentVariable(VLC_EXT_ENV, pluginPath);
                    return true;
                }
            }

            if (supportedPlatform == UMPSettings.Platforms.Linux)
            {
                Environment.SetEnvironmentVariable(VLC_EXT_ENV, path);
                Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", path);
                return true;
            }

            return false;
        }

        public static IntPtr LoadLibrary(string libName, string libraryPath)
        {
            var libHandler = IntPtr.Zero;
            var libNameWithExt = string.Empty;

            if (string.IsNullOrEmpty(libName) || string.IsNullOrEmpty(libraryPath))
                return libHandler;

            SetLibraryDirectory(libraryPath);

            var libraryFiles = Directory.GetFiles(libraryPath);
            foreach (var libraryFile in libraryFiles)
            {
                if (libraryFile.EndsWith(".meta"))
                    continue;

                var fileName = Path.GetFileName(libraryFile);

                if (fileName.StartsWith(libName + ".") && (string.IsNullOrEmpty(libNameWithExt) || fileName.Any(char.IsDigit)))
                    libNameWithExt = fileName;
            }

            switch (UMPSettings.RuntimePlatform)
            {
                case UMPSettings.Platforms.Win:
                    libHandler = WindowsInterop.LoadLibrary(Path.Combine(libraryPath, libNameWithExt));
                    break;

                case UMPSettings.Platforms.Mac:
                    libHandler = MacInterop.dlopen(Path.Combine(libraryPath, libNameWithExt), LIN_RTLD_NOW);
                    break;

                case UMPSettings.Platforms.Linux:
                    libHandler = LinuxInterop.dlopen(Path.Combine(libraryPath, libNameWithExt), LIN_RTLD_NOW);
                    break;
            }

            if (libHandler == IntPtr.Zero)
                throw new Exception(string.Format("[LoadLibrary] Can't load '{0}' library", libName));

            return libHandler;
        }

        public static T GetLibraryDelegate<T>(IntPtr handler)
        {
            string functionName = null;
            var procAddress = IntPtr.Zero;
            var supportedPlatform = UMPSettings.RuntimePlatform;

            try
            {
                var attrs = typeof(T).GetCustomAttributes(typeof(NativeFunctionAttribute), false);
                if (attrs.Length == 0)
                    throw new Exception("[GetLibraryDelegate] Could not find the native attribute type.");

                var attr = (NativeFunctionAttribute)attrs[0];
                functionName = attr.FunctionName;
                if (_interopDelegates.ContainsKey(functionName))
                    return (T)Convert.ChangeType(_interopDelegates[attr.FunctionName], typeof(T), null);

                if (supportedPlatform == UMPSettings.Platforms.Win)
                    procAddress = WindowsInterop.GetProcAddress(handler, attr.FunctionName);
                if (supportedPlatform == UMPSettings.Platforms.Mac)
                    procAddress = MacInterop.dlsym(handler, attr.FunctionName);
                if (supportedPlatform == UMPSettings.Platforms.Linux)
                    procAddress = LinuxInterop.dlsym(handler, attr.FunctionName);

                if (procAddress == IntPtr.Zero)
                    throw new Exception(string.Format("[GetLibraryDelegate] Can't get process address from '{0}'", handler));

                var delegateForFunctionPointer = Marshal.GetDelegateForFunctionPointer(procAddress, typeof(T));
                _interopDelegates[attr.FunctionName] = delegateForFunctionPointer;
                return (T)Convert.ChangeType(delegateForFunctionPointer, typeof(T), null);
            }
            catch (Exception e)
            {
                throw new MissingMethodException(string.Format("[GetLibraryDelegate] The address of the function '{0}' does not exist in '{1}' library.", functionName, handler), e);
            }
        }

        public static bool FreeLibrary(IntPtr handler)
        {
            switch (UMPSettings.RuntimePlatform)
            {
                case UMPSettings.Platforms.Win:
                    return WindowsInterop.FreeLibrary(handler);

                case UMPSettings.Platforms.Mac:
                    return MacInterop.dlclose(handler) == 0;

                case UMPSettings.Platforms.Linux:
                    return LinuxInterop.dlclose(handler) == 0;
            }

            return false;
        }

        public static string ReadLocalRegKey(string keyPath, string valueName)
        {
            var platform = UMPSettings.RuntimePlatform;
            var value = string.Empty;

            if (platform == UMPSettings.Platforms.Win)
            {
                var localMachine = new UIntPtr(0x80000002u);
                var readKeyRights = 131097;
                var hKey = UIntPtr.Zero;

                if (WindowsInterop.RegOpenKeyEx(localMachine, keyPath, 0, readKeyRights, out hKey) == 0)
                {
                    uint type;
                    uint size = 1024;
                    var keyBuffer = new StringBuilder((int)size);

                    if (WindowsInterop.RegQueryValueEx(hKey, valueName, 0, out type, keyBuffer, ref size) == 0)
                        value = keyBuffer.ToString();
                    else
                        Debug.LogWarning(string.Format("[ReadLocalRegKey] Can't read local reg key value: '{0}'", valueName));

                    WindowsInterop.RegCloseKey(hKey);
                }
                //else
                //    Debug.LogWarning(string.Format("[ReadLocalRegKey] Can't open local reg key: '{0}'", keyPath));
            }

            return value;
        }
    }
}