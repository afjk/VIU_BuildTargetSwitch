using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

namespace AFJK.BuildConfigSwitch
{
    public class BuildConfigSwitcher
    {
        readonly BuildConfigScriptableObject buildParam;
        public BuildConfigSwitcher(BuildConfigScriptableObject buildParam)
        {
            this.buildParam = buildParam;
        }

        public void ApplyDefineSymbols()
        {
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(GetBuildTarget(buildParam.buildTarget));

            var defineSymbolList = defineSymbols.Split(';').ToList();

            // Define追加
            if (buildParam.addDefines!=null &&  buildParam.addDefines.Length > 0)
            {
                defineSymbolList.AddRange(buildParam.addDefines.ToList().Distinct());
            }
            
            // Define削除
            if (buildParam.removeDefines != null && buildParam.removeDefines.Length > 0)
            {
                defineSymbolList.RemoveAll(x => buildParam.removeDefines.Contains(x));
            }

            var newDefineSymbols = string.Join(";", defineSymbolList.Distinct());
            
            Debug.Log(newDefineSymbols);
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(GetBuildTarget(buildParam.buildTarget),newDefineSymbols );
        }

        private BuildTargetGroup GetBuildTarget(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    return BuildTargetGroup.Android;
                case BuildTarget.StandaloneWindows64:
                    return BuildTargetGroup.Standalone;
                default:
                    throw new Exception($"Unknown BuildTarget:{buildTarget}");
            }
        }
    }
}