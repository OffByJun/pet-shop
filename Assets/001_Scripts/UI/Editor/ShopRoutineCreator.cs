using System;
using System.Linq;
using _001_Scripts.Data;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Economy;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Progression;
using _001_Scripts.Data.Tools;
using _001_Scripts.Managers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace _001_Scripts.UI.Editor
{
    public static class ShopRoutineCreator
    {
        public const string Root = "Assets/002_Resources/Routine";
        public const string ScenePath = "Assets/000_Scenes/ShopRoutineScene.unity";
        private static Font font;
        private static readonly Color Ink = new Color32(45, 65, 60, 255);
        private static readonly Color Green = new Color32(184, 215, 195, 255);

        [MenuItem("Tools/PetShop/Routine/Create Playable Routine")]
        public static void Create()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before authoring.");
            if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets/002_Resources", "Routine");
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var orderCatalog = AssetDatabase.LoadAssetAtPath<ServiceOrderCatalog>("Assets/002_Resources/ServiceOrders/ServiceOrderCatalog.asset");
            var economy = AssetDatabase.LoadAssetAtPath<ServiceEconomySettings>("Assets/002_Resources/Economy/ServiceEconomySettings.asset");
            var settings = Asset<ShopRoutineSettings>("ShopRoutineSettings");
            var days = Asset<GameSettings>("BusinessDaySettings");
            if (new SerializedObject(settings).FindProperty("careRules").arraySize == 0) SeedCare(settings, orderCatalog);
            var progression = Asset<ProgressionCatalog>("RoutineProgressionCatalog");
            if (progression.Unlocks.Count == 0) SeedProgression(progression);
            if (settings.Supplies.Count == 0) SeedSupplies(settings);
            if (settings.Decorations.Count == 0)
            {
                var mint = Decoration("mint_room", "민트빛 인테리어", 80, new Color32(173, 213, 196, 255));
                var rose = Decoration("rose_room", "장밋빛 인테리어", 120, new Color32(227, 180, 180, 255));
                var sky = Decoration("sky_room", "하늘빛 인테리어", 120, new Color32(172, 202, 224, 255));
                SetArray(settings, "decorations", new Object[] { mint, rose, sky });
            }

            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var root = new GameObject("Shop Routine");
                var game = root.AddComponent<GameManager>();
                var inventory = root.AddComponent<InventoryManager>();
                var routine = root.AddComponent<ShopRoutineManager>();
                Set(game, "settings", days); Set(game, "routineSettings", settings);
                Set(game, "orderCatalog", orderCatalog); Set(game, "progressionCatalog", progression); Set(game, "economyProvider", economy);
                Set(routine, "settings", settings); Set(routine, "game", game); Set(routine, "inventory", inventory);
                BuildUI(root.transform, routine);
                var camera = new GameObject("Menu Camera").AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Green;
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                EditorSceneManager.SaveScene(scene, ScenePath);
                PrefabUtility.SaveAsPrefabAsset(root, Root + "/ShopRoutine.prefab");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                SceneManager.SetActiveScene(previous);
            }
            WireDecoration("Assets/000_Scenes/CustomerReceptionScene.unity", "Desk Top", "Front Desk - replace sprite");
            WireDecoration("Assets/000_Scenes/CarePlayScene.unity", "Care Mat", "Stage Background");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }
                .Concat(EditorBuildSettings.scenes.Where(s => s.path != ScenePath)).ToArray();
            // The main menu stays the first scene the player lands on.
            MainMenuCreator.PutFirstInBuild(MainMenuCreator.ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("PetShop routine created: " + ScenePath);
        }

        private static void SeedCare(ShopRoutineSettings settings, ServiceOrderCatalog catalog)
        {
            var conditions = catalog.Conditions.Where(c => c.RequiredCapabilities != PetToolCapability.None).ToArray();
            var serialized = new SerializedObject(settings);
            var rules = serialized.FindProperty("careRules"); rules.arraySize = conditions.Length;
            for (var i = 0; i < conditions.Length; i++)
            {
                var c = conditions[i];
                var tool = Asset<PetToolDefinition>("tool_" + c.ConditionId);
                var t = new SerializedObject(tool);
                t.FindProperty("toolId").stringValue = c.ConditionId;
                t.FindProperty("displayName").stringValue = c.DisplayName + " 케어 도구";
                t.FindProperty("capabilities").intValue = (int)c.RequiredCapabilities;
                t.FindProperty("supportedInteractions").intValue = 1 << (int)c.InteractionMode;
                t.FindProperty("rewardAction").intValue = (int)c.ResolvedBy;
                t.ApplyModifiedPropertiesWithoutUndo();
                // Initial content defaults only; all runtime behavior reads the serialized rules below.
                var kind = c.ResolvedBy switch { PetCareAction.Wash => CareKind.Wash, PetCareAction.Brush => CareKind.Brush,
                    PetCareAction.Treat => CareKind.Treat, PetCareAction.Extract => CareKind.Remove, _ => CareKind.Trim };
                var uiTool = kind switch { CareKind.Wash => CareToolKind.WashBrush, CareKind.Brush => CareToolKind.Comb,
                    CareKind.Treat => CareToolKind.Medicine, CareKind.Remove => CareToolKind.Tweezers, _ => CareToolKind.Scissors };
                var rule = rules.GetArrayElementAtIndex(i);
                rule.FindPropertyRelative("condition").objectReferenceValue = c;
                rule.FindPropertyRelative("presentationKind").intValue = (int)kind;
                rule.FindPropertyRelative("tool").intValue = (int)uiTool;
                rule.FindPropertyRelative("needsWater").boolValue = kind == CareKind.Wash;
                rule.FindPropertyRelative("effort").floatValue = 420;
                rule.FindPropertyRelative("waterEffort").floatValue = 380;
                rule.FindPropertyRelative("domainTool").objectReferenceValue = tool;
                rule.FindPropertyRelative("zone").rectValue = CareZone(c.ConditionId, i);
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>케어 스테이지 기준 정규화 좌표입니다. 레이어드 펫 아트의 부위와 맞춥니다.</summary>
        private static Rect CareZone(string conditionId, int index) => conditionId switch
        {
            "dirty" or "muddy" => new Rect(.407f, .593f, .186f, .233f),
            "wounded" => new Rect(.523f, .798f, .079f, .121f),
            "tangled_coat" => new Rect(.477f, .202f, .139f, .140f),
            "hard_growth" => new Rect(.491f, .277f, .112f, .140f),
            "foreign_object" => new Rect(.314f, .130f, .105f, .222f),
            "plant_overgrowth" => new Rect(.314f, .481f, .099f, .335f),
            "long_nails" => new Rect(.547f, .854f, .084f, .084f),
            _ => new Rect(.13f + (index % 4) * .18f, .27f + (index / 4) * .28f, .16f, .20f)
        };

        /// <summary>보급품과 평판 구간, 그리고 케어별 소모 연결을 채웁니다.</summary>
        private static void SeedSupplies(ShopRoutineSettings settings)
        {
            var soap = Supply("supply_soap", "펫 전용 비누", 45, 10, 10);
            var salve = Supply("supply_salve", "상처 연고", 45, 5, 5);
            var tweezerTip = Supply("supply_tweezer_tip", "집게 팁", 50, 8, 8);
            var blade = Supply("supply_blade", "손질용 날", 50, 8, 8);
            SetArray(settings, "supplies", new Object[] { soap, salve, tweezerTip, blade });

            var serialized = new SerializedObject(settings);
            var tiers = serialized.FindProperty("reputationTiers");
            string[] titles = { "동네 신참", "입소문 나는 가게", "인기 펫샵", "명성 있는 펫샵" };
            int[] minimums = { 0, 8, 20, 40 };
            int[] extras = { 0, 1, 2, 3 };
            tiers.arraySize = titles.Length;
            for (var i = 0; i < titles.Length; i++)
            {
                var tier = tiers.GetArrayElementAtIndex(i);
                tier.FindPropertyRelative("title").stringValue = titles[i];
                tier.FindPropertyRelative("minimumPoints").intValue = minimums[i];
                tier.FindPropertyRelative("extraCustomers").intValue = extras[i];
            }

            // Brushing stays free so an empty shelf never blocks every care in an order.
            var byCondition = new System.Collections.Generic.Dictionary<string, Object>
            {
                { "dirty", soap }, { "muddy", soap }, { "wounded", salve },
                { "hard_growth", tweezerTip }, { "foreign_object", tweezerTip },
                { "plant_overgrowth", blade }, { "long_nails", blade }
            };
            var rules = serialized.FindProperty("careRules");
            for (var i = 0; i < rules.arraySize; i++)
            {
                var rule = rules.GetArrayElementAtIndex(i);
                var condition = rule.FindPropertyRelative("condition").objectReferenceValue;
                var supply = condition != null && byCondition.TryGetValue(condition.name, out var found) ? found : null;
                rule.FindPropertyRelative("supply").objectReferenceValue = supply;
                rule.FindPropertyRelative("supplyCost").intValue = supply == null ? 0 : 1;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static ShopSupplyDefinition Supply(string id, string label, int packCost, int packSize, int startingStock)
        {
            var asset = Asset<ShopSupplyDefinition>(id);
            var s = new SerializedObject(asset);
            s.FindProperty("supplyId").stringValue = id;
            s.FindProperty("displayName").stringValue = label;
            s.FindProperty("packCost").intValue = packCost;
            s.FindProperty("packSize").intValue = packSize;
            s.FindProperty("startingStock").intValue = startingStock;
            s.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        private static void SeedProgression(ProgressionCatalog catalog)
        {
            var speed = Upgrade("care_training", "케어 도구 개선 · 속도 +15%", 100, ProgressionBenefitType.ProcessingSpeed, .15f);
            var storage = Upgrade("storage_shelves", "보관 선반 · 용량 +20", 150, ProgressionBenefitType.StorageCapacity, 20);
            var expert = Upgrade("expert_tools", "전문 도구 · 속도 +25%", 300, ProgressionBenefitType.ProcessingSpeed, .25f);
            SetArray(expert, "prerequisites", new Object[] { speed });
            SetArray(catalog, "unlocks", new Object[] { speed, storage, expert });
        }

        private static ProgressionUnlockDefinition Upgrade(string id, string label, int cost, ProgressionBenefitType type, float value)
        {
            var asset = Asset<ProgressionUnlockDefinition>(id);
            var s = new SerializedObject(asset);
            s.FindProperty("unlockId").stringValue = id; s.FindProperty("displayName").stringValue = label;
            s.FindProperty("cost").intValue = cost;
            var benefits = s.FindProperty("benefits"); benefits.arraySize = 1;
            benefits.GetArrayElementAtIndex(0).FindPropertyRelative("type").intValue = (int)type;
            benefits.GetArrayElementAtIndex(0).FindPropertyRelative("value").floatValue = value;
            s.ApplyModifiedPropertiesWithoutUndo(); return asset;
        }

        private static ShopDecorationDefinition Decoration(string id, string label, int cost, Color color)
        {
            var asset = Asset<ShopDecorationDefinition>(id); var s = new SerializedObject(asset);
            s.FindProperty("decorationId").stringValue = id; s.FindProperty("displayName").stringValue = label;
            s.FindProperty("cost").intValue = cost; s.FindProperty("accentColor").colorValue = color;
            s.ApplyModifiedPropertiesWithoutUndo(); return asset;
        }

        private static void BuildUI(Transform parent, ShopRoutineManager routine)
        {
            var canvasObject = new GameObject("Routine Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = .5f;
            var view = canvasObject.AddComponent<ShopRoutineUI>(); Set(view, "routine", routine);
            var panel = Box(canvasObject.transform, "Routine Panel", Vector2.zero, new Vector2(1600, 900), new Color32(245, 241, 228, 255));
            // Stretch the backdrop; content uses the reference-resolution layout.
            panel.rectTransform.anchorMin = Vector2.zero; panel.rectTransform.anchorMax = Vector2.one; panel.rectTransform.sizeDelta = Vector2.zero;
            Set(view, "panel", panel.gameObject);
            Label(panel.transform, "Brand", "PET SHOP  /  DAILY JOURNAL", new Vector2(0, 365), new Vector2(1300, 40), 19);
            Set(view, "title", Label(panel.transform, "Title", "포근포근 펫샵", new Vector2(0, 285), new Vector2(1400, 90), 44));
            var body = Label(panel.transform, "Body", "", new Vector2(0, 65), new Vector2(1260, 310), 23); body.alignment = TextAnchor.UpperCenter;
            Set(view, "body", body);
            var primary = Button(panel.transform, "Continue", "영업 시작", new Vector2(0, -310), new Vector2(640, 66));
            Set(view, "primary", primary); Set(view, "primaryLabel", primary.GetComponentInChildren<Text>());
            Set(view, "sell", Button(panel.transform, "Sell Byproducts", "부산물 모두 판매", new Vector2(0, -227), new Vector2(440, 55)));
            Set(view, "mainMenu", Button(panel.transform, "Main Menu", "메인 메뉴로", new Vector2(620, 360), new Vector2(220, 48)));
            var upgrades = Rect(panel.transform, "Upgrades", new Vector2(-455, -65), new Vector2(430, 245));
            var supplies = Rect(panel.transform, "Supplies", new Vector2(0, -65), new Vector2(430, 245));
            var decorations = Rect(panel.transform, "Decorations", new Vector2(455, -65), new Vector2(430, 245));
            foreach (var list in new[] { upgrades, supplies, decorations })
            {
                var layout = list.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing = 12; layout.childControlHeight = true;
                layout.childForceExpandHeight = false; layout.childControlWidth = true; layout.childForceExpandWidth = true;
            }
            Set(view, "upgradeList", upgrades); Set(view, "decorationList", decorations);
            Set(view, "supplyList", supplies);
            var template = Button(canvasObject.transform, "Upgrade Card Template", "", Vector2.zero, new Vector2(600, 62));
            template.gameObject.AddComponent<LayoutElement>().preferredHeight = 62; template.gameObject.SetActive(false); Set(view, "cardTemplate", template);
            var bar = Box(canvasObject.transform, "Business HUD", new Vector2(0, -417), new Vector2(1600, 66), Green);
            Set(view, "accent", bar); Set(view, "hud", Label(bar.transform, "Clock and wallet", "", new Vector2(-180, 0), new Vector2(1200, 55), 19));
            Set(view, "returnPet", Button(bar.transform, "Return Pet", "펫 돌려주기", new Vector2(625, 0), new Vector2(280, 50)));
            var art = Box(bar.transform, "Decoration Artwork", new Vector2(-758, 0), new Vector2(44, 44), Color.white);
            art.gameObject.SetActive(false); Set(view, "decorationArt", art);
        }

        private static void WireDecoration(string path, params string[] names)
        {
            var existing = SceneManager.GetSceneByPath(path);
            if (existing.IsValid() && existing.isLoaded && existing.isDirty) throw new InvalidOperationException("Scene has unsaved changes: " + path);
            var opened = !existing.IsValid() || !existing.isLoaded;
            var scene = opened ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive) : existing;
            var images = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Image>(true)).ToArray();
            var targets = images.Where(i => names.Contains(i.name)).ToArray();
            if (targets.Length == 0) targets = images.Where(i => i.name == "Background" && i.transform.parent.GetComponent<Canvas>() != null).ToArray();
            if (targets.Length == 0) throw new InvalidOperationException("No decoration surfaces in " + path);
            var view = targets[0].GetComponent<ShopDecorationView>() ?? targets[0].gameObject.AddComponent<ShopDecorationView>();
            SetArray(view, "surfaces", targets.Cast<Object>().ToArray());
            EditorSceneManager.SaveScene(scene);
            if (opened) EditorSceneManager.CloseScene(scene, true);
        }

        private static T Asset<T>(string name) where T : ScriptableObject
        {
            var path = Root + "/" + name + ".asset"; var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
        }
        private static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        private static Image Box(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        { var image = Rect(parent, name, position, size).gameObject.AddComponent<Image>(); image.color = color; return image; }
        private static Text Label(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize)
        {
            var text = Rect(parent, name, position, size).gameObject.AddComponent<Text>(); text.font = font; text.text = value; text.fontSize = fontSize;
            text.color = Ink; text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false; return text;
        }
        private static Button Button(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            var image = Box(parent, name, position, size, Green); var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            Label(button.transform, "Label", label, Vector2.zero, size - new Vector2(16, 4), 22); return button;
        }
        private static void Set(Object target, string name, Object value)
        { var s = new SerializedObject(target); s.FindProperty(name).objectReferenceValue = value; s.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetArray(Object target, string name, Object[] values)
        {
            var s = new SerializedObject(target); var p = s.FindProperty(name); p.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; s.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
