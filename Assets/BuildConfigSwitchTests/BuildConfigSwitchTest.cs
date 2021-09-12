using System.Collections;
using System.Collections.Generic;
using AFJK.BuildConfigSwitch;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
    }
}
