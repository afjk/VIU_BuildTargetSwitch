using UnityEditor;
using UnityEngine;

namespace AFJK.BuildConfigSwitch
{
    [CreateAssetMenu(fileName = "BuildConfig", menuName = "ScriptableObjects/BuildConfig", order = 1)]
    public class BuildConfigScriptableObject : ScriptableObject
    {
        public enum TargetDevice
        {
            Simurator,
            OpenVR,
            OculusDesktop,
            WindowsMR,
            Daydream,
            WaveVR,
            OculusAndroid,
        }

        public TargetDevice supportDevice;
        public string[] addDefines;
        public string[] removeDefines;
        public string[] addPackages;
        public string[] removePackages;
        public string[] evacuateFiles;
        public bool legacyVRSupported = false;
        public string androidPackageName;
        public string androidManifestPath;
        public BuildTarget buildTarget = BuildTarget.Android;
        public UnityEngine.Rendering.GraphicsDeviceType[] graphicsDeviceTypes;
        public ScriptingImplementation backend = ScriptingImplementation.IL2CPP;
        public AndroidSdkVersions minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        public AndroidSdkVersions targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        public AndroidArchitecture androidArchitecture = AndroidArchitecture.ARM64;
        public string[] sceneList;
        public bool appBundle_aab = false;
        public bool developmentBuild = false;
    }
}
