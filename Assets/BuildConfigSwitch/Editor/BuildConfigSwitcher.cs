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
        private readonly string ANDROID_MANIFEST_PATH = "Assets/Plugins/Android/AndroidManifest.xml";
        public BuildConfigSwitcher(BuildConfigScriptableObject buildParam)
        {
            this.buildParam = buildParam;
        }

        public void ApplyDefineSymbols()
        {
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(GetBuildTargetGroup(buildParam.buildTarget));

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
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(GetBuildTargetGroup(buildParam.buildTarget),newDefineSymbols );
        }

        private BuildTargetGroup GetBuildTargetGroup(BuildTarget buildTarget)
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
                var metaPath = path + ".meta";
                var ignorePath = path + "~";
                var ignoreMetaPath = metaPath + "~";

                if (Directory.Exists(path))
                {
                    if (Directory.Exists(ignorePath))
                    {
                        Directory.Delete(ignorePath, true);
                    }
                    Directory.Move(path, ignorePath);
                }

                if (File.Exists(path))
                {
                    if (File.Exists(ignorePath))
                    {
                        File.Delete(ignorePath);
                    }
                    
                    File.Move(path, ignorePath);
                }
                
                if (File.Exists(metaPath))
                {
                    if (File.Exists(ignoreMetaPath))
                    {
                        File.Delete(ignoreMetaPath);
                    }
                    
                    File.Move(metaPath, ignoreMetaPath);
                }
            }
        }

        public void RestoreFiles()
        {
            if (buildParam.evacuateFiles == null || buildParam.evacuateFiles.Length == 0) { return; }

            foreach (var path in buildParam.evacuateFiles)
            {
                
                var metaPath = path + ".meta";
                var ignorePath = path + "~";
                var ignoreMetaPath = metaPath + "~";

                if (Directory.Exists(ignorePath))
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    Directory.Move(ignorePath, path);    
                }

                if (File.Exists(ignorePath))
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    File.Move(ignorePath, path);
                }
                
                if (File.Exists(ignoreMetaPath))
                {
                    if (File.Exists(metaPath))
                    {
                        File.Delete(metaPath);
                    }
                    File.Move(ignoreMetaPath, metaPath );
                }
            }
        }

        public void ApplyProjectSettings()
        {
            PlayerSettings.SetScriptingBackend(GetBuildTargetGroup(buildParam.buildTarget), buildParam.backend);

            PlayerSettings.SetGraphicsAPIs(buildParam.buildTarget, buildParam.graphicsDeviceTypes);
            PlayerSettings.Android.minSdkVersion = buildParam.minSdkVersion;
            PlayerSettings.Android.targetSdkVersion = buildParam.targetSdkVersion;
            PlayerSettings.SetVirtualRealitySupported(GetBuildTargetGroup(buildParam.buildTarget), buildParam.legacyVRSupported);
        }

        public void ApplyBuildSettings()
        {
            EditorUserBuildSettings.buildAppBundle = buildParam.appBundle_aab;
            EditorUserBuildSettings.development = buildParam.developmentBuild;
            
            List<EditorBuildSettingsScene> sceneList = new List<EditorBuildSettingsScene>();
            
            if( buildParam.sceneList == null || buildParam.sceneList.Length == 0)
            {
                EditorBuildSettings.scenes = null;
                return;
            }
            
            foreach (var scene in buildParam.sceneList)
            {
                sceneList.Add(new EditorBuildSettingsScene(scene, true));                
            }
            EditorBuildSettings.scenes = sceneList.ToArray();
        }

        public void ApplyAndroidManifest()
        {
            if (File.Exists(ANDROID_MANIFEST_PATH))
            {
                File.Delete(ANDROID_MANIFEST_PATH);
            }

            if (!File.Exists(buildParam.androidManifestPath))
            {
                throw new Exception($"AndroidManifest.xml is not found path:{buildParam.androidManifestPath}");
            }

            if (!Directory.Exists(Path.GetDirectoryName(ANDROID_MANIFEST_PATH)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ANDROID_MANIFEST_PATH));
            }
            File.Copy(buildParam.androidManifestPath, ANDROID_MANIFEST_PATH);
        }
    }
}