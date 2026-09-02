using System.Collections.Generic;
using _001_Scripts.Data.Pets;
using UnityEditor;
using UnityEngine;

namespace _001_Scripts.Data.Tools.Editor
{
    public static class PetToolStarterContentCreator
    {
        private const string Root = "Assets/002_Resources/Tools";

        static PetToolStarterContentCreator()
        {
            EditorApplication.delayCall += CreateOnFirstImport;
        }

        [MenuItem("Tools/PetShop/Tools/Create Starter Pet Tools")]
        public static void CreateStarterContent()
        {
            EnsureFolder("Assets/002_Resources", "Tools");

            var shower = CreateTool("shower", "샤워기", PetToolCapability.Clean,
                PetToolInteractionSupport.Hold, PetCareAction.Wash);
            var brush = CreateTool("brush", "브러시", PetToolCapability.Clean | PetToolCapability.Groom,
                PetToolInteractionSupport.Hold, PetCareAction.Brush);
            var tweezers = CreateTool("tweezers", "집게", PetToolCapability.Extract,
                PetToolInteractionSupport.Pull, PetCareAction.Extract);
            var treatmentKit = CreateTool("treatment_kit", "치료 키트", PetToolCapability.Treat,
                PetToolInteractionSupport.Hold, PetCareAction.Treat);
            var scissors = CreateTool("scissors", "가위", PetToolCapability.Trim,
                PetToolInteractionSupport.Cut, PetCareAction.Trim);
            var nailClipper = CreateTool("nail_clipper", "손톱깎이", PetToolCapability.Clip,
                PetToolInteractionSupport.Cut, PetCareAction.Clip);

            var catalog = LoadOrCreate<PetToolCatalog>($"{Root}/PetToolCatalog.asset");
            var serialized = new SerializedObject(catalog);
            SetObjects(serialized.FindProperty("tools"), new[] { shower, brush, tweezers, treatmentKit, scissors, nailClipper });
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = catalog;
            Debug.Log($"Created PetShop starter pet tools at {Root}");
        }

        public static void CreateStarterContentBatch() => CreateStarterContent();

        private static void CreateOnFirstImport()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += CreateOnFirstImport;
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<PetToolCatalog>($"{Root}/PetToolCatalog.asset") == null)
                CreateStarterContent();
        }

        private static PetToolDefinition CreateTool(
            string id, string label, PetToolCapability capabilities,
            PetToolInteractionSupport interactions, PetCareAction rewardAction)
        {
            var path = $"{Root}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<PetToolDefinition>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<PetToolDefinition>();
            AssetDatabase.CreateAsset(asset, path);
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("toolId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = label;
            serialized.FindProperty("capabilities").intValue = (int)capabilities;
            serialized.FindProperty("supportedInteractions").intValue = (int)interactions;
            serialized.FindProperty("rewardAction").enumValueIndex = (int)rewardAction;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void SetObjects<T>(SerializedProperty property, IReadOnlyList<T> values) where T : Object
        {
            property.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
