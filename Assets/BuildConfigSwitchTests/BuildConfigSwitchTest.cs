using System.Linq;
using AFJK.BuildConfigSwitch;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
 * * ProjectSettinsへの変更反映
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
    }
}
