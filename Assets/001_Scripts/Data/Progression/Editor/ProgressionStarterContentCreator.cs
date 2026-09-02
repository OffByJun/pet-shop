using System.Collections.Generic;
using _001_Scripts.Data.Economy;
using UnityEditor;
using UnityEngine;

namespace _001_Scripts.Data.Progression.Editor
{
    public static class ProgressionStarterContentCreator
    {
        private const string Root = "Assets/002_Resources/Progression";
        public const string CatalogPath = Root + "/ProgressionCatalog.asset";

        static ProgressionStarterContentCreator() => EditorApplication.delayCall += CreateOnFirstImport;

        [MenuItem("Tools/PetShop/Progression/Create Starter Progression")]
        public static void Create()
        {
            EnsureFolder("Assets/002_Resources", "Progression");
            EnsureFolder(Root, "Unlocks");
            EnsureFolder(Root, "Endings");

            var toolControl = CreateUnlock("tool_control", "기본 도구 제어 강화", ProgressionStageId.Early,
                ExpenseCategory.ToolUpgrade, 100, null,
                Benefit(ProgressionBenefitType.ProcessingSpeed, .15f),
                Benefit(ProgressionBenefitType.InteractionAssist, 1f, "steady_input"));
            var wideBrush = CreateUnlock("wide_brush", "넓은 브러시", ProgressionStageId.Early,
                ExpenseCategory.ToolUpgrade, 150, new[] { toolControl },
                Benefit(ProgressionBenefitType.EffectiveRange, .25f, "brush"));
            var workbench = CreateUnlock("improved_workbench", "개선된 작업대", ProgressionStageId.Middle,
                ExpenseCategory.StoreEquipment, 350, new[] { wideBrush },
                Benefit(ProgressionBenefitType.ShopFunction, 1f, "workbench_v2"),
                Benefit(ProgressionBenefitType.ShopVisual, 1f, "workbench_v2_visual"));
            var storage = CreateUnlock("material_storage", "부산물 보관 설비", ProgressionStageId.Middle,
                ExpenseCategory.StorageExpansion, 300, new[] { wideBrush },
                Benefit(ProgressionBenefitType.StorageCapacity, 20f, "byproduct_storage"),
                Benefit(ProgressionBenefitType.ShopVisual, 1f, "storage_shelf"));
            var expansion = CreateUnlock("shop_expansion", "가게 확장", ProgressionStageId.Expansion,
                ExpenseCategory.StoreExpansion, 800, new[] { workbench, storage },
                Benefit(ProgressionBenefitType.ShopVisual, 1f, "expanded_shop"),
                Benefit(ProgressionBenefitType.ShopFunction, 1f, "expanded_facilities"),
                Benefit(ProgressionBenefitType.ContentPool, 1f, "expanded_pet_attribute_condition_pool"));
            var advancedExtractor = CreateUnlock("advanced_extractor", "고급 제거 장비", ProgressionStageId.Late,
                ExpenseCategory.ToolUpgrade, 1200, new[] { expansion },
                Benefit(ProgressionBenefitType.ProcessingSpeed, .25f, "extract"),
                Benefit(ProgressionBenefitType.InteractionAssist, 1f, "extract_stability"),
                Benefit(ProgressionBenefitType.ContentPool, 1f, "rare_attribute_pets"));
            var arcaneTreatment = CreateUnlock("arcane_treatment", "속성 치료 장비", ProgressionStageId.Late,
                ExpenseCategory.ToolUpgrade, 1500, new[] { advancedExtractor },
                Benefit(ProgressionBenefitType.EffectiveRange, .2f, "treat"),
                Benefit(ProgressionBenefitType.InteractionAssist, 1f, "condition_hint"),
                Benefit(ProgressionBenefitType.ContentPool, 1f, "special_conditions"));

            var house = CreateEnding("settlement_house", "이세계의 집 구매", 5000, new[] { arcaneTreatment });
            var ownership = CreateEnding("settlement_shop_ownership", "가게 완전 매입", 6500, new[] { arcaneTreatment });

            var catalog = LoadOrCreate<ProgressionCatalog>(CatalogPath);
            var serialized = new SerializedObject(catalog);
            SetStages(serialized.FindProperty("stages"));
            SetObjects(serialized.FindProperty("unlocks"), new[]
            {
                toolControl, wideBrush, workbench, storage, expansion, advancedExtractor, arcaneTreatment
            });
            SetObjects(serialized.FindProperty("endingCandidates"), new[] { house, ownership });
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = catalog;
            Debug.Log($"Created PetShop starter progression at {Root}");
        }

        public static void CreateBatch() => Create();

        private static ProgressionUnlockDefinition CreateUnlock(
            string id, string label, ProgressionStageId stage, ExpenseCategory category, int cost,
            IReadOnlyList<ProgressionUnlockDefinition> prerequisites, params BenefitData[] benefits)
        {
            var asset = LoadOrCreate<ProgressionUnlockDefinition>($"{Root}/Unlocks/{id}.asset");
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("unlockId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = label;
            serialized.FindProperty("stage").enumValueIndex = (int)stage;
            serialized.FindProperty("expenseCategory").enumValueIndex = (int)category;
            serialized.FindProperty("cost").intValue = cost;
            SetObjects(serialized.FindProperty("prerequisites"), prerequisites);
            var values = serialized.FindProperty("benefits");
            values.arraySize = benefits.Length;
            for (var i = 0; i < benefits.Length; i++)
            {
                var value = values.GetArrayElementAtIndex(i);
                value.FindPropertyRelative("type").enumValueIndex = (int)benefits[i].Type;
                value.FindPropertyRelative("value").floatValue = benefits[i].Value;
                value.FindPropertyRelative("contentId").stringValue = benefits[i].ContentId;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        private static SettlementGoalDefinition CreateEnding(
            string id, string label, int cost, IReadOnlyList<ProgressionUnlockDefinition> requirements)
        {
            var asset = LoadOrCreate<SettlementGoalDefinition>($"{Root}/Endings/{id}.asset");
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("goalId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = label;
            serialized.FindProperty("cost").intValue = cost;
            serialized.FindProperty("continueBusinessAfterCompletion").boolValue = true;
            SetObjects(serialized.FindProperty("requiredUnlocks"), requirements);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        private static void SetStages(SerializedProperty stages)
        {
            var names = new[] { "초반", "중반", "중후반", "후반", "최종" };
            stages.arraySize = names.Length;
            for (var i = 0; i < names.Length; i++)
            {
                var stage = stages.GetArrayElementAtIndex(i);
                stage.FindPropertyRelative("stage").enumValueIndex = i;
                stage.FindPropertyRelative("displayName").stringValue = names[i];
            }
        }

        private static BenefitData Benefit(ProgressionBenefitType type, float value, string contentId = "")
            => new BenefitData(type, value, contentId);

        private readonly struct BenefitData
        {
            public readonly ProgressionBenefitType Type;
            public readonly float Value;
            public readonly string ContentId;
            public BenefitData(ProgressionBenefitType type, float value, string contentId)
            { Type = type; Value = value; ContentId = contentId; }
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void SetObjects<T>(SerializedProperty property, IReadOnlyList<T> values) where T : UnityEngine.Object
        {
            var count = values == null ? 0 : values.Count;
            property.arraySize = count;
            for (var i = 0; i < count; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        private static void CreateOnFirstImport()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += CreateOnFirstImport;
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<ProgressionCatalog>(CatalogPath) == null) Create();
        }
    }
}
