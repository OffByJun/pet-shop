using UnityEditor;
using UnityEngine;

namespace _001_Scripts.Data.Business.Editor
{
    public static class BusinessDayStarterContentCreator
    {
        public const string SettingsPath = "Assets/002_Resources/Business/BusinessDaySettings.asset";

        static BusinessDayStarterContentCreator()
        {
            EditorApplication.delayCall += CreateOnFirstImport;
        }

        [MenuItem("Tools/PetShop/Business/Create Day Settings")]
        public static void Create()
        {
            EnsureFolder("Assets/002_Resources", "Business");
            var settings = AssetDatabase.LoadAssetAtPath<BusinessDaySettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<BusinessDaySettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            var serialized = new SerializedObject(settings);
            serialized.FindProperty("minimumCustomers").intValue = 5;
            serialized.FindProperty("maximumCustomers").intValue = 8;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Selection.activeObject = settings;
            Debug.Log($"Created PetShop business day settings at {SettingsPath}");
        }

        public static void CreateBatch() => Create();

        private static void CreateOnFirstImport()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += CreateOnFirstImport;
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<BusinessDaySettings>(SettingsPath) == null) Create();
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
