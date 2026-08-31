using _001_Scripts.UI.Components;
using _001_Scripts.UI.UILib;
using UnityEditor;
using UnityEngine;

namespace _001_Scripts.UI.Editor
{
    public static class UIDefaultPresetCreator
    {
        public const string PresetPath = "Assets/002_Resources/UI/Presets/DefaultUIAnimationPreset.asset";
        public const string PrefabPath = "Assets/002_Resources/UI/Presets/DefaultUIPanel.prefab";

        [MenuItem("Tools/PetShop/UI/Create Default UI Preset")]
        public static void Create()
        {
            EnsureFolder("Assets/002_Resources", "UI");
            EnsureFolder("Assets/002_Resources/UI", "Presets");

            UIAnimationPreset preset = AssetDatabase.LoadAssetAtPath<UIAnimationPreset>(PresetPath);
            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<UIAnimationPreset>();
                preset.SetDefaultValues();
                AssetDatabase.CreateAsset(preset, PresetPath);
            }

            GameObject panel = new GameObject(
                "Default UI Panel",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(UIFadeAnimator),
                typeof(UIScaleAnimator),
                typeof(UIPositionAnimator),
                typeof(UIComponent));

            try
            {
                RectTransform rectTransform = panel.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(640f, 360f);

                SerializedObject serializedComponent = new SerializedObject(panel.GetComponent<UIComponent>());
                serializedComponent.FindProperty("animationPreset").objectReferenceValue = preset;
                serializedComponent.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(panel, PrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(panel);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Debug.Log($"Created UI preset: {PresetPath}\nCreated UI prefab: {PrefabPath}");
        }

        public static void CreateBatch()
        {
            Create();
            UIAnimationPreset preset = AssetDatabase.LoadAssetAtPath<UIAnimationPreset>(PresetPath);
            preset.SetDefaultValues();
            EditorUtility.SetDirty(preset);
            AssetDatabase.SaveAssets();
            Create();
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
