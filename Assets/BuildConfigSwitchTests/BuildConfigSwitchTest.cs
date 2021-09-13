using System;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using AFJK.BuildConfigSwitch;
using HTC.UnityPlugin.Vive;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.PackageManager;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;

/*
 * ToDo:
 * * SerializeFieldに設定したScriptableObjectの参照
 * * ScriptableObjectを.assetファイルからロード
 * * BuildConfigSwitcherクラスの生成
 * * 指定Defineの追加
 * * 設定Defineの削除
 * * 指定Packageのインストール
 * * 指定PackageのRemove
 * * 指定フォルダの無効化
 * * 無効化フォルダの有効化
 * * ProjectSettinsへの設定反映
 *   * Packagename
 * * BuildSettingsへの設定反映
 * * AndriodManifestの置き換え
 * * 指定VIU Settings Support Deviceのチェック
 */

namespace Tests
{
    public class BuildConfigSwitchTest : ScriptableObject
    {
        [SerializeField]
        private  BuildConfigScriptableObject testScriptableObject;
        
        [Test]
        public void ReadScriptableObjectTest()
        {
            Assert.NotNull(testScriptableObject);
            Assert.AreEqual(testScriptableObject.addDefines[0], "TestDefine1");
        }

        [Test]
        public void LoadScriptableObjectTest()
        {
            var scriptableObject = AssetDatabase.LoadAssetAtPath<BuildConfigScriptableObject>("Assets/BuildConfigSwitchTests/TestData/BuildConfigTest.asset");
            Assert.NotNull(scriptableObject);
            Assert.AreEqual(scriptableObject.addDefines[0], "TestDefine1");
        }

        [Test]
        public void NewBuildConfigTest()
        {
            var buildConfig = new BuildConfigSwitcher(testScriptableObject);
            
            Assert.NotNull(buildConfig);
        }

        [Test]
        public void AddDefineTest()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.buildTarget = BuildTarget.Android;
            testConfigSO.addDefines = new[] {"TestDefine1, TestDefine2"};
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android,"" );

            var bulidConfig = new BuildConfigSwitcher(testConfigSO);
            bulidConfig.ApplyDefineSymbols();

            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            var defineSymbolList = defineSymbols.Split(';');

            Assert.IsTrue(defineSymbolList.Contains("TestDefine1"));
            Assert.IsTrue(defineSymbolList.Contains("TestDefine2"));
        }

        [Test]
        public void RemoveDefineTest()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.buildTarget = BuildTarget.StandaloneWindows64;
            testConfigSO.removeDefines = new[] {"TestDefine2"};
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,"TestDefine1;TestDefine2");
            
            var buildConfig = new BuildConfigSwitcher(testConfigSO);
            buildConfig.ApplyDefineSymbols();
            
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            var defineSymbolList = defineSymbols.Split(';');
            
            Assert.IsTrue(defineSymbolList.Contains("TestDefine1"));
            Assert.IsFalse(defineSymbolList.Contains("TestDefine2"));
        }

        [Test]
        public void ApplyPackageTest()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.buildTarget = BuildTarget.Android;
            
            // パッケージ追加
            testConfigSO.addPackages = new[] {"com.unity.xr.oculus", "com.htc.upm.wave.xrsdk@4.1.1-r.3.1"};
            
            var buildConfig = new BuildConfigSwitcher(testConfigSO);
            buildConfig.ApplyPackage();

            var request1 = Client.List();

            while (! request1.IsCompleted)
            {
                Thread.Sleep(100);
            }

            var result = request1.Result;
            var oculusPackage = result.First(x => x.name == "com.unity.xr.oculus");
            Assert.AreEqual("com.unity.xr.oculus",  oculusPackage.name);
            var wavePackage = result.First(x => x.name == "com.htc.upm.wave.xrsdk");
            Assert.AreEqual("com.htc.upm.wave.xrsdk",  wavePackage.name);
            

            // パッケージ削除
            testConfigSO.addPackages = null;
            testConfigSO.removePackages = new[] {"com.unity.xr.oculus"};
            
            buildConfig.ApplyPackage();

            var request2 = Client.List();

            while (! request2.IsCompleted)
            {
                Thread.Sleep(100);
            }

            result = request2.Result;

            var oculusPackageEnumerable = result.Where(x => x.name == "com.unity.xr.oculus");
            Assert.IsEmpty(oculusPackageEnumerable);
            wavePackage = result.First(x => x.name == "com.htc.upm.wave.xrsdk");
            Assert.AreEqual("com.htc.upm.wave.xrsdk",  wavePackage.name);
        }

        
        [Test]
        public void EvacuateFileTest()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            var testDirPath = "Assets/BuildConfigSwitchTests/TestData/TestFolder/Dir1";
            var testFilePath = "Assets/BuildConfigSwitchTests/TestData/TestFolder/File1.txt";
            
            testConfigSO.evacuateFiles = new[] {testDirPath, testFilePath};
            var buildConfig = new BuildConfigSwitcher(testConfigSO);
            
            buildConfig.EvacuateFiles();
            
            Assert.IsFalse(Directory.Exists(testDirPath));
            Assert.IsTrue(Directory.Exists(testDirPath + "~"));
            Assert.IsFalse(File.Exists(testDirPath + ".meta"));
            Assert.IsTrue(File.Exists(testDirPath + ".meta~"));
            Assert.IsFalse(File.Exists(testFilePath));
            Assert.IsTrue(File.Exists(testFilePath + "~"));
            Assert.IsFalse(File.Exists(testFilePath + ".meta"));
            Assert.IsTrue(File.Exists(testFilePath + ".meta~"));

            buildConfig.RestoreFiles();

            Assert.IsTrue(Directory.Exists(testDirPath));
            Assert.IsFalse(Directory.Exists(testDirPath + "~"));
            
            Assert.IsTrue(File.Exists(testDirPath + ".meta"));
            Assert.IsFalse(File.Exists(testDirPath + ".meta~"));
            
            Assert.IsTrue(File.Exists(testFilePath));
            Assert.IsFalse(File.Exists(testFilePath + "~"));

            Assert.IsTrue(File.Exists(testFilePath + ".meta"));
            Assert.IsFalse(File.Exists(testFilePath + ".meta~"));

        }

        [Test]
        public void SetProjectSettingsCase1Test()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.buildTarget = BuildTarget.Android;
            testConfigSO.backend = ScriptingImplementation.IL2CPP;
            testConfigSO.graphicsDeviceTypes = new[] {GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.OpenGLES2};
            testConfigSO.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            testConfigSO.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
            testConfigSO.legacyVRSupported = false;
            testConfigSO.androidPackageName = "com.afjk.test1";

            var buildConfig = new BuildConfigSwitcher(testConfigSO);

            buildConfig.ApplyProjectSettings();
            
            Assert.AreEqual( ScriptingImplementation.IL2CPP,PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android));

            foreach (var graphicsDeviceType in testConfigSO.graphicsDeviceTypes)
            {
                Assert.IsTrue(PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).Contains(graphicsDeviceType));
            }
            Assert.AreEqual( AndroidSdkVersions.AndroidApiLevel24, PlayerSettings.Android.minSdkVersion);
            Assert.AreEqual( AndroidSdkVersions.AndroidApiLevel30, PlayerSettings.Android.targetSdkVersion);
            Assert.IsFalse(PlayerSettings.GetVirtualRealitySupported(BuildTargetGroup.Android));
            Assert.AreEqual("com.afjk.test1", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));
        }
        
        [Test]
        public void SetProjectSettingsCase2Test()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.buildTarget = BuildTarget.Android;
            testConfigSO.backend = ScriptingImplementation.Mono2x;
            testConfigSO.graphicsDeviceTypes = new[] {GraphicsDeviceType.Vulkan};
            testConfigSO.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
            testConfigSO.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            testConfigSO.legacyVRSupported = true;
            testConfigSO.androidPackageName = "com.afjk.test2";

            var buildConfig = new BuildConfigSwitcher(testConfigSO);

            buildConfig.ApplyProjectSettings();
            
            Assert.AreEqual( ScriptingImplementation.Mono2x,PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android));

            foreach (var graphicsDeviceType in testConfigSO.graphicsDeviceTypes)
            {
                Assert.IsTrue(PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).Contains(graphicsDeviceType));
            }
            Assert.AreEqual( AndroidSdkVersions.AndroidApiLevel19, PlayerSettings.Android.minSdkVersion);
            Assert.AreEqual( AndroidSdkVersions.AndroidApiLevelAuto, PlayerSettings.Android.targetSdkVersion);
            Assert.IsTrue(PlayerSettings.GetVirtualRealitySupported(BuildTargetGroup.Android));
            Assert.AreEqual("com.afjk.test2", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));
        }

        [Test]
        public void BuildSettingsCase1Test()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.buildTarget = BuildTarget.Android;
            testConfigSO.developmentBuild = true;
            testConfigSO.appBundle_aab = true;
            testConfigSO.sceneList = new[]
            {
                "Assets/BuildConfigSwitchTests/TestData/TestScene1.unity",
                "Assets/BuildConfigSwitchTests/TestData/TestScene2.unity"
            };
            
            var buildConfig = new BuildConfigSwitcher(testConfigSO);

            buildConfig.ApplyBuildSettings();
            
            Assert.IsTrue(EditorUserBuildSettings.buildAppBundle);
            Assert.IsTrue(EditorUserBuildSettings.development);
            Assert.AreEqual("Assets/BuildConfigSwitchTests/TestData/TestScene1.unity", EditorBuildSettings.scenes[0].path );
            Assert.AreEqual("Assets/BuildConfigSwitchTests/TestData/TestScene2.unity", EditorBuildSettings.scenes[1].path );
        }
        
        
        [Test]
        public void BuildSettingsCase2Test()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.buildTarget = BuildTarget.Android;
            testConfigSO.developmentBuild = false;
            testConfigSO.appBundle_aab = false;

            var buildConfig = new BuildConfigSwitcher(testConfigSO);

            buildConfig.ApplyBuildSettings();
            
            Assert.IsFalse(EditorUserBuildSettings.buildAppBundle);
            Assert.IsFalse(EditorUserBuildSettings.development);
            
            Assert.Zero(EditorBuildSettings.scenes.Length);
        }
        
        [Test]
        public void ApplyAndroidManifestCase1Test()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.androidManifestPath = "Assets/BuildConfigSwitchTests/TestData/AndroidManifest-test1.xml";
            var buildConfig = new BuildConfigSwitcher(testConfigSO);

            buildConfig.ApplyAndroidManifest();

            Assert.IsTrue(File.Exists("Assets/Plugins/Android/AndroidManifest.xml"));
            var expectManifest = File.ReadAllText(testConfigSO.androidManifestPath);
            var actualManifest = File.ReadAllText("Assets/Plugins/Android/AndroidManifest.xml");
            Assert.AreEqual(expectManifest, actualManifest);
        }
       
        [Test]
        public void ApplyAndroidManifestCase2Test()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.androidManifestPath = "Assets/BuildConfigSwitchTests/TestData/AndroidManifest-test2.xml";
            var buildConfig = new BuildConfigSwitcher(testConfigSO);

            buildConfig.ApplyAndroidManifest();

            Assert.IsTrue(File.Exists("Assets/Plugins/Android/AndroidManifest.xml"));
            var expectManifest = File.ReadAllText(testConfigSO.androidManifestPath);
            var actualManifest = File.ReadAllText("Assets/Plugins/Android/AndroidManifest.xml");
            Assert.AreEqual(expectManifest, actualManifest);
        }
        
        [Test]
        public void ApplyAndroidManifestCase3Test()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.androidManifestPath = null;
            var buildConfig = new BuildConfigSwitcher(testConfigSO);

            buildConfig.ApplyAndroidManifest();

            Assert.IsFalse(File.Exists("Assets/Plugins/Android/AndroidManifest.xml"));
        }

        [Test]
        public void SwitchPlatformAndroidTest()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.buildTarget = BuildTarget.Android;
            var buildConfig = new BuildConfigSwitcher(testConfigSO);
            
            buildConfig.SwitchPlatform();
            
            Assert.AreEqual(BuildTarget.Android, EditorUserBuildSettings.activeBuildTarget);
        }

        [Test]
        public void SwitchPlatformWindowsTest()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.buildTarget = BuildTarget.StandaloneWindows64;
            var buildConfig = new BuildConfigSwitcher(testConfigSO);
            
            buildConfig.SwitchPlatform();
            
            Assert.AreEqual(BuildTarget.StandaloneWindows64, EditorUserBuildSettings.activeBuildTarget);
        }

        
        [Test]
        public void SwitchPlatformWebGLTest()
        {
            var testConfigSO = ScriptableObject.CreateInstance<BuildConfigScriptableObject>();
            testConfigSO.buildTarget = BuildTarget.WebGL;
            var buildConfig = new BuildConfigSwitcher(testConfigSO);
            
            buildConfig.SwitchPlatform();
            
            Assert.AreEqual(BuildTarget.WebGL, EditorUserBuildSettings.activeBuildTarget);
        }
        
        [Test]
        public void VIUSettingSupportDeviceOculusAndroidTest()
        {
            var testConfigSO = AssetDatabase.LoadAssetAtPath<BuildConfigScriptableObject>("Assets/BuildConfigSwitchTests/TestData/BuildConfigOculusAndroid.asset");
            var buildConfig = new BuildConfigSwitcher(testConfigSO);

            buildConfig.SwitchPlatform();
            buildConfig.ApplyPackage();
            buildConfig.ApplyAndroidManifest();
            buildConfig.ApplyDefineSymbols();
            buildConfig.ApplyProjectSettings();
            buildConfig.ApplySupportDevice();
            
            Assert.IsTrue(VIUSettingsEditor.supportOculusGo );
            
        }
        
        [Test]
        public void VIUSettingSupportDeviceOculusDesktopTest()
        {
            var testConfigSO = AssetDatabase.LoadAssetAtPath<BuildConfigScriptableObject>("Assets/BuildConfigSwitchTests/TestData/BuildConfigOculusDesktop.asset");
            var buildConfig = new BuildConfigSwitcher(testConfigSO);

            buildConfig.SwitchPlatform();
            buildConfig.ApplyPackage();
            buildConfig.ApplyAndroidManifest();
            buildConfig.ApplyDefineSymbols();
            buildConfig.ApplyProjectSettings();
            buildConfig.ApplySupportDevice();
            
            Assert.IsTrue(VIUSettingsEditor.supportOculus );
        }
        
        
        [Test]
        public void VIUSettingSupportDeviceWaveVRTest()
        {
            var testConfigSO = AssetDatabase.LoadAssetAtPath<BuildConfigScriptableObject>("Assets/BuildConfigSwitchTests/TestData/BuildConfigWaveVR.asset");
            var buildConfig = new BuildConfigSwitcher(testConfigSO);

            buildConfig.SwitchPlatform();
            buildConfig.ApplyPackage();
            buildConfig.ApplyAndroidManifest();
            buildConfig.ApplyDefineSymbols();
            buildConfig.ApplyProjectSettings();
            buildConfig.ApplySupportDevice();
            
            Assert.IsTrue(VIUSettingsEditor.supportWaveVR );
        }
    }
}
