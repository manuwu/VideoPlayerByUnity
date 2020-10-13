using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UMP
{
    [CreateAssetMenu(fileName = "UMPSettings", menuName = "UMP/UMPSettings")]
    [Serializable]
    public class UMPSettings : ScriptableObject
    {
        public enum Platforms
        {
            None = 1,
            Win = 2,
            Mac = 4,
            Linux = 8,
            WebGL = 16,
            Android = 32,
            iOS = 64
        }

        public enum BitModes
        {
            x86,
            x86_64
        }

        private const string MAC_APPS_FOLDER_NAME = "/Applications";
        private const string MAC_VLC_PACKAGE_NAME = "vlc.app";
        private const string MAC_LIBVLC_PACKAGE_NAME = "libvlc.bundle";
        private const string MAC_PACKAGE_LIB_PATH = @"Contents/MacOS/lib";

        private const string WIN_REG_KEY_X86 = @"SOFTWARE\WOW6432Node\VideoLAN\VLC";
        private const string WIN_REG_KEY_X86_64 = @"SOFTWARE\VideoLAN\VLC";

        private static string[] LIN_APPS_FOLDERS_PATHS = new string[] { "/usr/lib",
        "/usr/lib64",
        "/usr/lib/x86_64-linux-gnu/" };

        public const string SETTINGS_FILE_NAME = "UMPSettings";
        public const string ASSET_NAME = "UniversalMediaPlayer";
        public const string LIB_VLC_NAME = "libvlc";
        public const string LIB_VLC_CORE_NAME = "libvlccore";
        public const string DESKTOP_CATEGORY_NAME = "Desktop";
        public const string PLUGINS_FOLDER_NAME = "Plugins";
        private const string ASSETS_FOLDER_NAME = "Assets";

        #region Instance
        private static UMPSettings _instance;

        public static UMPSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<UMPSettings>(SETTINGS_FILE_NAME);

                    if (_instance == null)
                        Debug.LogError(string.Format("[UMPSetting] Could not find settings file '{0}' in UMP 'Resources' folder. " +
                            "Try to correctly import UMP asset to your project or create the new settings file by click with right mouse on UMP 'Resources' folder and choose: 'Create'->'UMP'->'UMPSettings'.", SETTINGS_FILE_NAME));

                    if (Application.isEditor)
                    {
                        if (!_instance.IsValidAssetPath)
                            Debug.LogError("[UMPSetting] Asset path is not correct, please check the settings file in UMP 'Resources' folder.");
                    }

                    if ((RuntimePlatform & Desktop) == RuntimePlatform)
                    {
                        if (!ContainsLibVLC(_instance.LibrariesPath))
                            Debug.LogError("[UMPSetting] Can't find LibVLC libraries, try to check the settings file in UMP 'Resources' folder.");
                    }
                }

                return _instance;
            }
        }
        #endregion

        #region Asset Path
        [SerializeField]
        private string _assetPath = Path.Combine(ASSETS_FOLDER_NAME, ASSET_NAME).Replace(@"\", "/");

        /// <summary>
        /// Get/Set path to main asset folder.
        /// </summary>
        public string AssetPath
        {
            get
            {
                return _assetPath;
            }
            set
            {
                _assetPath = value;
            }
        }

        /// <summary>
        /// Check if main asset folder is valid.
        /// </summary>
        public bool IsValidAssetPath
        {
            get
            {
                return Directory.Exists(_assetPath) && Directory.GetFiles(_assetPath).Length > 0;
            }
        }
        #endregion

        #region Audio Source
        [SerializeField]
        private bool _useAudioSource = false;

        /// <summary>
        /// Check if needed to use Unity 'AudioSource' component (only for Desktop platforms).
        /// </summary>
        public bool UseAudioSource
        {
            get { return _useAudioSource; }
        }
        #endregion

        #region External Libraries
        [SerializeField]
        private bool _useExternalLibraries = false;

        /// <summary>
        /// Check if needed to use external libraries path (only for Desktop platforms).
        /// </summary>
        public bool UseExternalLibraries
        {
            get { return _useExternalLibraries; }
        }
        #endregion

        #region Libraries Path
        [SerializeField]
        private string _librariesPath = string.Empty;

        /// <summary>
        /// Get path to the liraries that will be used for media player (only for Desktop platforms).
        /// </summary>
        public string LibrariesPath
        {
            get
            {
                var path = GetLibrariesPath(RuntimePlatform, _useExternalLibraries);

                if (ContainsLibVLC(path))
                    _librariesPath = path;
                else if (!ContainsLibVLC(_librariesPath))
                    _librariesPath = string.Empty;

                return _librariesPath;
            }
        }
        #endregion

        #region Players Android
        [SerializeField]
        private PlayerOptionsAndroid.PlayerTypes _playersAndroid = PlayerOptionsAndroid.PlayerTypes.Native | PlayerOptionsAndroid.PlayerTypes.LibVLC;

        public PlayerOptionsAndroid.PlayerTypes PlayersAndroid
        {
            get { return _playersAndroid; }
            set
            {
                _playersAndroid = value;
            }
        }
        #endregion

        #region Players IPhone
        [SerializeField]
        private PlayerOptionsIPhone.PlayerTypes _playersIPhone = PlayerOptionsIPhone.PlayerTypes.Native | PlayerOptionsIPhone.PlayerTypes.FFmpeg;

        public PlayerOptionsIPhone.PlayerTypes PlayersIPhone
        {
            get { return _playersIPhone; }
            set
            {
                _playersIPhone = value;
            }
        }
        #endregion

        #region Exported Paths
        [SerializeField]
        private string[] _androidExportedPaths = new string[0];

        public string[] AndroidExportedPaths
        {
            get { return _androidExportedPaths; }
            set
            {
                _androidExportedPaths = value;
            }
        }
        #endregion

        #region Youtube Function Pattern
        [SerializeField]
        private string _youtubeDecryptFunction = @"\bc\s*&&\s*d\.set\([^,]+\s*,[^(]*\(([a-zA-Z0-9$]+)\(";

        public string YoutubeDecryptFunction
        {
            get { return _youtubeDecryptFunction; }
        }
        #endregion

        /// <summary>
        /// Returns the libraries path for specific platform.
        /// </summary>
        /// <param name="platform">Runtime platform</param>
        /// <param name="externalSpace">Use external space (for libraries that previously installed on your system)</param>
        /// <returns></returns>
        public string GetLibrariesPath(Platforms platform, bool externalSpace)
        {
            string librariesPath = string.Empty;

            if (platform != Platforms.None)
            {
                if (!externalSpace)
                {
                    if (Application.isEditor)
                    {
                        librariesPath = Path.Combine(_assetPath, PLUGINS_FOLDER_NAME);
                        librariesPath = Path.Combine(librariesPath, PlatformFolderName(platform));

                        if (platform == Platforms.Win || platform == Platforms.Mac || platform == Platforms.Linux)
                            librariesPath = Path.Combine(librariesPath, EditorBitModeFolderName);
                    }
                    else
                    {
                        librariesPath = Path.Combine(Application.dataPath, PLUGINS_FOLDER_NAME);

                        if (platform == Platforms.Linux)
                            librariesPath = Path.Combine(librariesPath, EditorBitModeFolderName);
                    }

                    if (platform == Platforms.Mac)
                        librariesPath = Path.Combine(librariesPath, Path.Combine(MAC_LIBVLC_PACKAGE_NAME, MAC_PACKAGE_LIB_PATH));

                    if (!Directory.Exists(librariesPath))
                        librariesPath = string.Empty;
                }
                else
                {
                    if (platform == Platforms.Win)
                    {
                        librariesPath = NativeInterop.ReadLocalRegKey(EditorBitMode == BitModes.x86 ? WIN_REG_KEY_X86 : WIN_REG_KEY_X86_64, "InstallDir");
                    }

                    if (platform == Platforms.Mac)
                    {
                        var appsFolderInfo = new DirectoryInfo(MAC_APPS_FOLDER_NAME);
                        var packages = appsFolderInfo.GetDirectories();

                        foreach (var package in packages)
                        {
                            if (package.FullName.ToLower().Contains(MAC_VLC_PACKAGE_NAME))
                                librariesPath = Path.Combine(package.FullName, MAC_PACKAGE_LIB_PATH);
                        }
                    }

                    if (platform == Platforms.Linux)
                    {
                        DirectoryInfo appsFolderInfo = null;

                        foreach (var appFolder in LIN_APPS_FOLDERS_PATHS)
                        {
                            if (Directory.Exists(appFolder))
                                appsFolderInfo = new DirectoryInfo(appFolder);

                            if (appsFolderInfo != null)
                            {
                                var appsLibs = appsFolderInfo.GetFiles();

                                foreach (var lib in appsLibs)
                                {
                                    if (lib.FullName.ToLower().Contains(LIB_VLC_NAME))
                                        librariesPath = appFolder;
                                }
                            }
                        }
                    }
                }

                if (!librariesPath.Equals(string.Empty))
                    librariesPath = Path.GetFullPath(librariesPath + Path.AltDirectorySeparatorChar);
            }

            return librariesPath;
        }

        /// <summary>
        /// Returns installed platforms.
        /// </summary>
        /// <param name="platform">Platforms group (mobile/desktop)</param>
        /// <returns></returns>
        public string[] GetInstalledPlatforms(Platforms category)
        {
            var installedPlatforms = new List<string>();
            foreach (Platforms platform in Enum.GetValues(typeof(Platforms)))
            {
                var librariesPath = GetLibrariesPath(platform, false);

                if (!string.IsNullOrEmpty(librariesPath))
                {
                    foreach (var file in Directory.GetFiles(librariesPath))
                    {
                        if (Path.GetFileName(file).Contains(ASSET_NAME))
                        {
                            if ((category & Desktop) == Desktop &&
                                (platform == Platforms.Win || platform == Platforms.Mac || platform == Platforms.Linux) &&
                                !installedPlatforms.Contains(DESKTOP_CATEGORY_NAME))
                            {

                                installedPlatforms.Add(DESKTOP_CATEGORY_NAME);
                            }

                            if ((category & Mobile) == Mobile &&
                                platform == Platforms.Android &&
                                !installedPlatforms.Contains(Platforms.Android.ToString()))
                            {
                                installedPlatforms.Add(Platforms.Android.ToString());
                            }

                            if ((category & Mobile) == Mobile &&
                                platform == Platforms.iOS &&
                                !installedPlatforms.Contains(Platforms.iOS.ToString()))
                            {
                                installedPlatforms.Add(Platforms.iOS.ToString());
                            }

                            if ((category & Desktop) == Desktop &&
                                platform == Platforms.WebGL &&
                                !installedPlatforms.Contains(Platforms.WebGL.ToString()))
                            {
                                installedPlatforms.Add(Platforms.WebGL.ToString());
                            }

                            break;
                        }
                    }
                }
            }

            return installedPlatforms.ToArray();
        }

        #region Static Methods
        /// <summary>
        /// Returns the desktop platforms (Read Only).
        /// </summary>
        public static Platforms Desktop
        {
            get { return Platforms.Win | Platforms.Mac | Platforms.Linux; }
        }

        /// <summary>
        /// Returns the mobile platforms (Read Only).
        /// </summary>
        public static Platforms Mobile
        {
            get { return Platforms.Android | Platforms.iOS; }
        }

        /// <summary>
        /// Returns the Unity Editor bit mode (Read Only).
        /// </summary>
        public static BitModes EditorBitMode
        {
            get { return IntPtr.Size == 4 ? BitModes.x86 : BitModes.x86_64; }
        }

        /// <summary>
        /// Returns the folder name for current Unity Editor bit mode (Read Only).
        /// </summary>
        public static string EditorBitModeFolderName
        {
            get { return Enum.GetName(typeof(BitModes), EditorBitMode); }
        }

        /// <summary>
        /// Returns the current running platform that supported by UMP asset (Read Only).
        /// </summary>
        public static Platforms RuntimePlatform
        {
            get
            {
                var runtimePlatform = Platforms.None;
                var platform = Application.platform;

                if (platform == UnityEngine.RuntimePlatform.WindowsEditor ||
                            Application.platform == UnityEngine.RuntimePlatform.WindowsPlayer)
                    runtimePlatform = Platforms.Win;

                if (platform == UnityEngine.RuntimePlatform.OSXEditor ||
                            Application.platform == UnityEngine.RuntimePlatform.OSXPlayer)
                    runtimePlatform = Platforms.Mac;

                if (platform == UnityEngine.RuntimePlatform.LinuxPlayer ||
                            (int)Application.platform == 16)
                    runtimePlatform = Platforms.Linux;

                if (platform == UnityEngine.RuntimePlatform.WebGLPlayer)
                    runtimePlatform = Platforms.WebGL;

                if (platform == UnityEngine.RuntimePlatform.Android)
                    runtimePlatform = Platforms.Android;

                if (platform == UnityEngine.RuntimePlatform.IPhonePlayer)
                    runtimePlatform = Platforms.iOS;

                return runtimePlatform;
            }
        }

        /// <summary>
        /// Returns the platform folder name for specific platform.
        /// </summary>
        /// <param name="platform">Runtime platform</param>
        /// <returns></returns>
        public static string PlatformFolderName(Platforms platform)
        {
            if (platform != Platforms.None)
                return platform.ToString();

            return string.Empty;
        }

        /// <summary>
        /// Returns the folder name for current platform the game is running on (Read Only).
        /// </summary>
        public static string RuntimePlatformFolderName
        {
            get
            {
                return PlatformFolderName(RuntimePlatform);
            }
        }

        /// <summary>
        /// Checking if libVLC exists by the current file path (only for Desktop platforms).
        /// </summary>
        public static bool ContainsLibVLC(string path)
        {
            var result = false;

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                var includes = 0;
                var libExt = string.Empty;

                switch (RuntimePlatform)
                {
                    case Platforms.Win:
                        libExt = "dll";
                        break;

                    case Platforms.Mac:
                        libExt = "dylib";
                        break;

                    case Platforms.Linux:
                        libExt = "so";
                        break;
                }

                foreach (var file in files)
                {
                    if (file.EndsWith(string.Format("{0}.{1}", LIB_VLC_NAME, libExt)) ||
                        file.EndsWith(string.Format("{0}.{1}", LIB_VLC_CORE_NAME, libExt)))
                        includes++;
                }

                if (includes >= 2)
                    result = true;
            }

            return result;
        }
        #endregion
    }
}