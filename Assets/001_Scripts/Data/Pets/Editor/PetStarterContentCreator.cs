using System.Collections.Generic;
using _001_Scripts.Data.Items;
using UnityEditor;
using UnityEngine;

namespace _001_Scripts.Data.Pets.Editor
{
    public static class PetStarterContentCreator
    {
        private const string Root = "Assets/002_Resources/Pets";

        static PetStarterContentCreator()
        {
            EditorApplication.delayCall += CreateOnFirstImport;
        }

        [MenuItem("Tools/PetShop/Pets/Create Starter Content")]
        public static void CreateStarterContent()
        {
            EnsureFolder("Assets/002_Resources", "Pets");
            EnsureFolder(Root, "BaseAnimals");
            EnsureFolder(Root, "Attributes");
            EnsureFolder(Root, "Variants");
            EnsureFolder(Root, "Byproducts");

            var cat = CreateBase("cat", "고양이", PetSpecies.Cat, "중형, 범용");
            var dog = CreateBase("dog", "개", PetSpecies.Dog, "중대형, 활동형");
            var rabbit = CreateBase("rabbit", "토끼", PetSpecies.Rabbit, "소형, 점프형");
            var hamster = CreateBase("hamster", "햄스터", PetSpecies.Hamster, "초소형, 수집형");
            var guineaPig = CreateBase("guinea_pig", "기니피그", PetSpecies.GuineaPig, "소형, 온순형");
            var parrot = CreateBase("parrot", "앵무새", PetSpecies.Parrot, "소형, 비행형");

            var normal = CreateAttribute("normal", "일반", PetElement.None);
            var obsidian = CreateAttribute("obsidian", "흑요석", PetElement.Obsidian);
            var fire = CreateAttribute("fire", "불", PetElement.Fire);
            var crystal = CreateAttribute("crystal", "수정", PetElement.Crystal);
            var moss = CreateAttribute("moss", "이끼", PetElement.Moss);
            var lightning = CreateAttribute("lightning", "번개", PetElement.Lightning);
            var armor = CreateAttribute("armor", "철갑", PetElement.Armor);
            var coral = CreateAttribute("coral", "산호", PetElement.Coral);

            var catFur = CreateItem("cat_fur", "고양이 털", 6);
            var dogFur = CreateItem("dog_fur", "개 털", 7);
            var rabbitFluff = CreateItem("rabbit_fluff", "토끼 솜털", 8);
            var hamsterFur = CreateItem("hamster_fur", "햄스터 털", 5);
            var guineaPigFur = CreateItem("guinea_pig_fur", "기니피그 털", 7);
            var parrotFeather = CreateItem("parrot_feather", "앵무새 깃털", 9);
            var obsidianShard = CreateItem("obsidian_shard", "흑요석 조각", 25, ItemRarity.Uncommon);
            var ashPowder = CreateItem("ash_powder", "잿가루", 12);
            var crystalFragment = CreateItem("crystal_fragment", "수정 파편", 30, ItemRarity.Rare);
            var herbMoss = CreateItem("herb_moss", "약초 이끼", 18, ItemRarity.Uncommon);
            var electricFur = CreateItem("electric_fur", "전기 털", 28, ItemRarity.Rare);
            var ironScrap = CreateItem("iron_scrap", "철 조각", 22);
            var coralFragment = CreateItem("coral_fragment", "산호 조각", 26, ItemRarity.Uncommon);

            var variants = new[]
            {
                CreateVariant("normal_cat", "고양이", cat, normal, PetCareAction.Brush, catFur),
                CreateVariant("normal_dog", "개", dog, normal, PetCareAction.Brush, dogFur),
                CreateVariant("normal_rabbit", "토끼", rabbit, normal, PetCareAction.Brush, rabbitFluff),
                CreateVariant("normal_hamster", "햄스터", hamster, normal, PetCareAction.Brush, hamsterFur),
                CreateVariant("normal_guinea_pig", "기니피그", guineaPig, normal, PetCareAction.Brush, guineaPigFur),
                CreateVariant("normal_parrot", "앵무새", parrot, normal, PetCareAction.Brush, parrotFeather),
                CreateVariant("obsidian_cat", "흑요석 고양이", cat, obsidian, PetCareAction.Extract, obsidianShard, "expanded_pet_attribute_condition_pool"),
                CreateVariant("fire_cat", "불 고양이", cat, fire, PetCareAction.Wash, ashPowder, "expanded_pet_attribute_condition_pool"),
                CreateVariant("crystal_cat", "수정 고양이", cat, crystal, PetCareAction.Extract, crystalFragment, "rare_attribute_pets"),
                CreateVariant("moss_dog", "이끼 개", dog, moss, PetCareAction.Trim, herbMoss, "expanded_pet_attribute_condition_pool"),
                CreateVariant("lightning_dog", "번개 개", dog, lightning, PetCareAction.Brush, electricFur, "rare_attribute_pets"),
                CreateVariant("armor_dog", "철갑 개", dog, armor, PetCareAction.Extract, ironScrap, "rare_attribute_pets"),
                CreateVariant("coral_rabbit", "산호 토끼", rabbit, coral, PetCareAction.Extract, coralFragment, "expanded_pet_attribute_condition_pool")
            };

            var catalog = LoadOrCreate<PetCatalog>($"{Root}/PetCatalog.asset");
            var catalogObject = new SerializedObject(catalog);
            SetObjectArray(catalogObject.FindProperty("baseAnimals"), new Object[] { cat, dog, rabbit, hamster, guineaPig, parrot });
            SetObjectArray(catalogObject.FindProperty("attributes"), new Object[] { normal, obsidian, fire, crystal, moss, lightning, armor, coral });
            SetObjectArray(catalogObject.FindProperty("variants"), variants);
            catalogObject.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = catalog;
            Debug.Log($"Created PetShop starter pet content at {Root}");
        }

        public static void CreateStarterContentBatch() => CreateStarterContent();

        private static void CreateOnFirstImport()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += CreateOnFirstImport;
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<PetCatalog>($"{Root}/PetCatalog.asset") == null)
                CreateStarterContent();
        }

        private static BaseAnimalDefinition CreateBase(string id, string label, PetSpecies species, string role)
        {
            var asset = LoadOrCreate<BaseAnimalDefinition>($"{Root}/BaseAnimals/{id}.asset", out var created);
            if (!created) return asset;
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("petBaseId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = label;
            serialized.FindProperty("species").enumValueIndex = (int)species;
            serialized.FindProperty("bodyRole").stringValue = role;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        private static PetAttributeDefinition CreateAttribute(string id, string label, PetElement element)
        {
            var asset = LoadOrCreate<PetAttributeDefinition>($"{Root}/Attributes/{id}.asset", out var created);
            if (!created) return asset;
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("attributeId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = label;
            serialized.FindProperty("element").enumValueIndex = (int)element;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        private static ItemDefinition CreateItem(string id, string label, int sellPrice, ItemRarity rarity = ItemRarity.Common)
        {
            var asset = LoadOrCreate<ItemDefinition>($"{Root}/Byproducts/{id}.asset", out var created);
            if (!created) return asset;
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("itemId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = label;
            serialized.FindProperty("category").enumValueIndex = (int)ItemCategory.Product;
            serialized.FindProperty("rarity").enumValueIndex = (int)rarity;
            serialized.FindProperty("baseSellPrice").intValue = sellPrice;
            serialized.FindProperty("maxStackSize").intValue = 99;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        private static PetVariantDefinition CreateVariant(
            string id, string label, PetBase baseAnimal, PetAttributeDefinition attribute,
            PetCareAction action, ItemBase byproduct, string requiredContentId = "")
        {
            var asset = LoadOrCreate<PetVariantDefinition>($"{Root}/Variants/{id}.asset", out _);
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("variantId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = label;
            serialized.FindProperty("baseAnimal").objectReferenceValue = baseAnimal;
            serialized.FindProperty("attribute").objectReferenceValue = attribute;
            serialized.FindProperty("requiredProgressionContentId").stringValue = requiredContentId;
            var rules = serialized.FindProperty("byproducts");
            rules.arraySize = 1;
            var rule = rules.GetArrayElementAtIndex(0);
            rule.FindPropertyRelative("CareAction").enumValueIndex = (int)action;
            rule.FindPropertyRelative("Item").objectReferenceValue = byproduct;
            rule.FindPropertyRelative("MinAmount").intValue = 1;
            rule.FindPropertyRelative("MaxAmount").intValue = 2;
            rule.FindPropertyRelative("Chance").floatValue = 1f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

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

        private static void SetObjectArray(SerializedProperty property, IReadOnlyList<Object> values)
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
