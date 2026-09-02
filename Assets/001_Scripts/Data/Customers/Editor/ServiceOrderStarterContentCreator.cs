using System.Collections.Generic;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Pets.Editor;
using _001_Scripts.Data.Tools;
using UnityEditor;
using UnityEngine;

namespace _001_Scripts.Data.Customers.Editor
{
    public static class ServiceOrderStarterContentCreator
    {
        private const string Root = "Assets/002_Resources/ServiceOrders";
        private const string PetCatalogPath = "Assets/002_Resources/Pets/PetCatalog.asset";

        static ServiceOrderStarterContentCreator()
        {
            EditorApplication.delayCall += CreateOnFirstImport;
        }

        [MenuItem("Tools/PetShop/Customers/Create Starter Service Orders")]
        public static void CreateStarterContent()
        {
            if (AssetDatabase.LoadAssetAtPath<PetCatalog>(PetCatalogPath) == null)
                PetStarterContentCreator.CreateStarterContent();

            EnsureFolder("Assets/002_Resources", "ServiceOrders");
            EnsureFolder(Root, "Conditions");
            EnsureFolder(Root, "CustomerTypes");

            var dirty = CreateCondition("dirty", "더러움", PetConditionCategory.Contamination, PetCareAction.Wash, PetToolCapability.Clean, PetToolInteractionMode.Hold, 1);
            var muddy = CreateCondition("muddy", "심한 오염", PetConditionCategory.Contamination, PetCareAction.Wash, PetToolCapability.Clean, PetToolInteractionMode.Hold, 2);
            var wounded = CreateCondition("wounded", "상처", PetConditionCategory.Injury, PetCareAction.Treat, PetToolCapability.Treat, PetToolInteractionMode.Hold, 2);
            var hungry = CreateCondition("hungry", "배고픔", PetConditionCategory.Hunger, PetCareAction.Feed, PetToolCapability.None, PetToolInteractionMode.Instant, 1);
            var stressed = CreateCondition("stressed", "스트레스", PetConditionCategory.Stress, PetCareAction.Play, PetToolCapability.None, PetToolInteractionMode.Instant, 1);
            var tangled = CreateCondition("tangled_coat", "엉킨 털", PetConditionCategory.Coat, PetCareAction.Brush, PetToolCapability.Groom, PetToolInteractionMode.Hold, 1);
            var hardGrowth = CreateCondition("hard_growth", "단단한 부착물", PetConditionCategory.Growth, PetCareAction.Extract, PetToolCapability.Extract, PetToolInteractionMode.Pull, 2, "special_conditions");
            var foreignObject = CreateCondition("foreign_object", "박힌 이물질", PetConditionCategory.ForeignObject, PetCareAction.Extract, PetToolCapability.Extract, PetToolInteractionMode.Pull, 2, "special_conditions");
            var overgrowth = CreateCondition("plant_overgrowth", "과성장", PetConditionCategory.Growth, PetCareAction.Trim, PetToolCapability.Trim, PetToolInteractionMode.Cut, 2, "special_conditions");
            var longNails = CreateCondition("long_nails", "긴 발톱", PetConditionCategory.Nails, PetCareAction.Clip, PetToolCapability.Clip, PetToolInteractionMode.Cut, 1);

            var resident = CreateCustomer("resident", "일반 주민", CustomerArchetype.Resident, 1, 2, 0, 1, .15f, .05f, "standard",
                Pref(dirty, 3), Pref(hungry, 2), Pref(stressed, 1), Pref(tangled, 1));
            var adventurer = CreateCustomer("adventurer", "모험가", CustomerArchetype.Adventurer, 2, 3, 0, 1, .35f, .15f, "adventurer",
                Pref(muddy, 4), Pref(wounded, 4), Pref(foreignObject, 3), Pref(dirty, 2), Pref(stressed, 1));
            var wizard = CreateCustomer("wizard", "마법사", CustomerArchetype.Wizard, 1, 3, 1, 2, .85f, .25f, "arcane",
                Pref(stressed, 3), Pref(hardGrowth, 3), Pref(dirty, 2), Pref(wounded, 1), Pref(tangled, 1));
            var merchant = CreateCustomer("merchant", "상인", CustomerArchetype.Merchant, 1, 2, 1, 2, .45f, .75f, "merchant",
                Pref(tangled, 3), Pref(hardGrowth, 3), Pref(dirty, 2), Pref(stressed, 2), Pref(hungry, 1));
            var noble = CreateCustomer("noble", "귀족", CustomerArchetype.Noble, 3, 4, 2, 3, .6f, .4f, "premium",
                Pref(tangled, 3), Pref(stressed, 3), Pref(overgrowth, 2), Pref(longNails, 2), Pref(dirty, 2), Pref(hungry, 2), Pref(wounded, 1));

            var petCatalog = AssetDatabase.LoadAssetAtPath<PetCatalog>(PetCatalogPath);
            var catalog = LoadOrCreate<ServiceOrderCatalog>($"{Root}/ServiceOrderCatalog.asset");
            var serialized = new SerializedObject(catalog);
            SetObjects(serialized.FindProperty("customerTypes"), new Object[] { resident, adventurer, wizard, merchant, noble });
            SetObjects(serialized.FindProperty("petVariants"), petCatalog.Variants);
            SetObjects(serialized.FindProperty("conditions"), new Object[] { dirty, muddy, wounded, hungry, stressed, tangled, hardGrowth, foreignObject, overgrowth, longNails });
            serialized.FindProperty("perfectOptionalCompletionRatio").floatValue = 1f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = catalog;
            Debug.Log($"Created PetShop starter service-order content at {Root}");
        }

        public static void CreateStarterContentBatch() => CreateStarterContent();

        private static void CreateOnFirstImport()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += CreateOnFirstImport;
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<ServiceOrderCatalog>($"{Root}/ServiceOrderCatalog.asset") == null)
                CreateStarterContent();
        }

        private static PetConditionDefinition CreateCondition(
            string id, string label, PetConditionCategory category, PetCareAction action,
            PetToolCapability capability, PetToolInteractionMode interactionMode, int severity,
            string requiredContentId = "")
        {
            var asset = LoadOrCreate<PetConditionDefinition>($"{Root}/Conditions/{id}.asset", out var created);
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("conditionId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = label;
            serialized.FindProperty("category").enumValueIndex = (int)category;
            serialized.FindProperty("resolvedBy").enumValueIndex = (int)action;
            serialized.FindProperty("requiredCapabilities").intValue = (int)capability;
            serialized.FindProperty("interactionMode").enumValueIndex = (int)interactionMode;
            serialized.FindProperty("severity").intValue = severity;
            serialized.FindProperty("requiredProgressionContentId").stringValue = requiredContentId;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        private static CustomerTypeDefinition CreateCustomer(
            string id, string label, CustomerArchetype archetype,
            int minRequired, int maxRequired, int minOptional, int maxOptional,
            float elementalChance, float rareChance, string economyTier,
            params CustomerConditionPreference[] preferences)
        {
            var asset = LoadOrCreate<CustomerTypeDefinition>($"{Root}/CustomerTypes/{id}.asset", out var created);
            if (!created) return asset;
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("customerTypeId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = label;
            serialized.FindProperty("archetype").enumValueIndex = (int)archetype;
            serialized.FindProperty("minimumRequiredRequests").intValue = minRequired;
            serialized.FindProperty("maximumRequiredRequests").intValue = maxRequired;
            serialized.FindProperty("minimumOptionalCare").intValue = minOptional;
            serialized.FindProperty("maximumOptionalCare").intValue = maxOptional;
            serialized.FindProperty("elementalPetChance").floatValue = elementalChance;
            serialized.FindProperty("rareByproductChance").floatValue = rareChance;
            serialized.FindProperty("economyTierId").stringValue = economyTier;
            var preferenceArray = serialized.FindProperty("conditionPreferences");
            preferenceArray.arraySize = preferences.Length;
            for (var i = 0; i < preferences.Length; i++)
            {
                var entry = preferenceArray.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("Condition").objectReferenceValue = preferences[i].Condition;
                entry.FindPropertyRelative("Weight").floatValue = preferences[i].Weight;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        private static CustomerConditionPreference Pref(PetConditionDefinition condition, float weight)
            => new CustomerConditionPreference { Condition = condition, Weight = weight };

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
            => LoadOrCreate<T>(path, out _);

        private static T LoadOrCreate<T>(string path, out bool created) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            created = asset == null;
            if (!created) return asset;
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
