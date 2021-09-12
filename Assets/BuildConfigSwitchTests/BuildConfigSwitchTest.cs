using AFJK.BuildConfigSwitch;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/*
 * ToDo:
 * * SerializeFieldに設定したScriptableObjectが参照出来る
 * * ScriptableObjectを.assetファイルからロード出来る
 * * BuildConfigSwitcherクラスを生成出来る
 * * 
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
        
    }
}
