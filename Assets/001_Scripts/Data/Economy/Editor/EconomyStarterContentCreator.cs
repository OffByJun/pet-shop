using UnityEditor;
using UnityEngine;

namespace _001_Scripts.Data.Economy.Editor
{
    public static class EconomyStarterContentCreator
    {
        public const string SettingsPath = "Assets/002_Resources/Economy/ServiceEconomySettings.asset";

        static EconomyStarterContentCreator() => EditorApplication.delayCall += CreateOnFirstImport;

        [MenuItem("Tools/PetShop/Economy/Create Starter Economy Settings")]
        public static void Create()
        {
            EnsureFolder("Assets/002_Resources", "Economy");
            var settings = AssetDatabase.LoadAssetAtPath<ServiceEconomySettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<ServiceEconomySettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            var serialized = new SerializedObject(settings);
            SetTier(serialized.FindProperty("defaultTier"), "standard", 20, 10, 7);
            var tiers = serialized.FindProperty("tiers");
            tiers.arraySize = 5;
            SetTier(tiers.GetArrayElementAtIndex(0), "standard", 20, 10, 7);
            SetTier(tiers.GetArrayElementAtIndex(1), "adventurer", 25, 12, 8);
            SetTier(tiers.GetArrayElementAtIndex(2), "arcane", 30, 14, 10);
            SetTier(tiers.GetArrayElementAtIndex(3), "merchant", 35, 15, 12);
            SetTier(tiers.GetArrayElementAtIndex(4), "premium", 50, 20, 15);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Selection.activeObject = settings;
            Debug.Log($"Created PetShop economy settings at {SettingsPath}");
        }

        public static void CreateBatch() => Create();

        private static void CreateOnFirstImport()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += CreateOnFirstImport;
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<ServiceEconomySettings>(SettingsPath) == null) Create();
        }

        private static void SetTier(SerializedProperty property, string id, int visit, int required, int optional)
        {
            property.FindPropertyRelative("tierId").stringValue = id;
            property.FindPropertyRelative("visitFee").intValue = visit;
            property.FindPropertyRelative("requiredCareUnitPrice").intValue = required;
            property.FindPropertyRelative("optionalCareBonusUnitPrice").intValue = optional;
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
