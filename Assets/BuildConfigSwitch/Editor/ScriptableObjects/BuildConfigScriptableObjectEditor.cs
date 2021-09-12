using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AFJK.BuildConfigSwitch
{
    [CustomEditor(typeof(BuildConfigScriptableObject))]
    public class BuildConfigScriptableObjectEditor : Editor
    {
        private const string UxmlPath = "Assets/BuildConfigSwitch/Editor/ScriptableObjects/BuildConfigInspector.uxml";
        private Foldout androidFoldout;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Bind(serializedObject);

            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            root.Add(tree.CloneTree());

            androidFoldout = root.Q<Foldout>("android field");
            EnumField buildTarget = root.Query<EnumField>("buildTarget");

            var nowBuildTarget = serializedObject.FindProperty("buildTarget");

            if (nowBuildTarget.enumValueIndex == (int) BuildTarget.Android)
            {
                androidFoldout.visible = true;
            }
            else
            {
                androidFoldout.visible = false;
            }

            buildTarget.RegisterCallback<ChangeEvent<Enum>>((x) =>
            {
                if (x.newValue.Equals(BuildTarget.Android))
                {
                    androidFoldout.visible = true;
                }
                else
                {
                    androidFoldout.visible = false;
                }
            });

            return root;
        }
    }
}