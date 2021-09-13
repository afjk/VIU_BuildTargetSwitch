using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEditor.PackageManager;
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

        public void ApplyPackage()
        {
            if (buildParam.addPackages != null && buildParam.addPackages.Length > 0)
            {
                foreach (var package in buildParam.addPackages)
                {
                    var request = Client.Add(package);
                    while (! request.IsCompleted)
                    {
                        Thread.Sleep(100);
                    }
                }
            }

            if (buildParam.removePackages != null && buildParam.removePackages.Length > 0)
            {
                foreach (var package in buildParam.removePackages)
                {
                    var request = Client.Remove(package);
                    while (! request.IsCompleted)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            
        }

        public void EvacuateFiles()
        {
            if (buildParam.evacuateFiles == null || buildParam.evacuateFiles.Length == 0) { return; }

            foreach (var path in buildParam.evacuateFiles)
            {
                if (Directory.Exists(path))
                {
                    if (Directory.Exists(path + "~"))
                    {
                        Directory.Delete(path + "~", true);
                    }
                    Directory.Move(path, path + "~");    
                }

                if (File.Exists(path))
                {
                    if (File.Exists(path + "~"))
                    {
                        File.Delete(path + "~");
                    }
                    
                    File.Move(path, path + "~");
                }
            }
        }
    }
}