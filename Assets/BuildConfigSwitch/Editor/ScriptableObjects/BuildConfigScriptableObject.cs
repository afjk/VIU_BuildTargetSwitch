using UnityEditor;
using UnityEngine;

namespace AFJK.BuildConfigSwitch
{
    [CreateAssetMenu(fileName = "BuildConfig", menuName = "ScriptableObjects/BuildConfig", order = 1)]
    public class BuildConfigScriptableObject : ScriptableObject
    {
        public string[] addDefines;
        public string[] removeDefines;
        public string[] addPackages;
        public string[] removePackages;
        public string[] evacuateFiles;
        public bool legacyVRSupported = false;
        public string androidManifestPath;
        public BuildTarget buildTarget = BuildTarget.Android;
        public UnityEngine.Rendering.GraphicsDeviceType[] graphicsDeviceTypes;
        public ScriptingImplementation backend = ScriptingImplementation.IL2CPP;
        public AndroidSdkVersions minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        public AndroidSdkVersions targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        public AndroidArchitecture androidArchitecture = AndroidArchitecture.ARM64;
        public bool appBundle_aab = false;
    }
}
