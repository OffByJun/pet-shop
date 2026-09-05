using System;
using System.Linq;
using _001_Scripts.UI.Theme;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace _001_Scripts.UI.Editor
{
    /// <summary>디자인 토큰을 실제 씬에 입힙니다. 여러 번 실행해도 결과가 같습니다.</summary>
    public static class PetShopUIThemer
    {
        public const string ThemePath = "Assets/002_Resources/UI/UITheme.asset";
        private const string MenuScene = "Assets/000_Scenes/MainMenuScene.unity";
        private const string HubScene = "Assets/000_Scenes/ShopRoutineScene.unity";
        private const string ReceptionScene = "Assets/000_Scenes/CustomerReceptionScene.unity";
        private const string CareScene = "Assets/000_Scenes/CarePlayScene.unity";

        private static UITheme theme;

        [MenuItem("Tools/PetShop/UI/Apply Theme To All Scenes")]
        public static void ApplyAll()
        {
            Guard();
            Run(MenuScene, ThemeMainMenu);
            Run(HubScene, ThemeHub);
            Run(ReceptionScene, ThemeReception);
            Run(CareScene, ThemeCare);
            AssetDatabase.SaveAssets();
            Debug.Log("PetShop UI theme applied to 4 scenes.");
        }

        [MenuItem("Tools/PetShop/UI/Apply Theme To Open Scene")]
        public static void ApplyOpen()
        {
            Guard();
            var path = EditorSceneManager.GetActiveScene().path;
            Action<Scene> pass = path switch
            {
                MenuScene => ThemeMainMenu,
                HubScene => ThemeHub,
                ReceptionScene => ThemeReception,
                CareScene => ThemeCare,
                _ => null
            };
            if (pass == null) throw new InvalidOperationException("No theme pass for " + path);
            pass(default);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("PetShop UI theme applied to " + path);
        }

        private struct Scene { }

        private static void Guard()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before theming.");
            theme = AssetDatabase.LoadAssetAtPath<UITheme>(ThemePath);
            if (theme == null) throw new InvalidOperationException("Missing theme asset: " + ThemePath);
        }

        private static void Run(string path, Action<Scene> pass)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            pass(default);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ---------- shared helpers ----------

        /// <summary>비활성 오브젝트까지 찾습니다. GameObject.Find는 활성만 봅니다.</summary>
        private static GameObject Find(string path)
        {
            var go = GameObject.Find(path);
            if (go != null) return go;
            var parts = path.TrimStart('/').Split('/');
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name != parts[0]) continue;
                var current = root.transform;
                for (var i = 1; i < parts.Length && current != null; i++) current = current.Find(parts[i]);
                if (current != null) return current.gameObject;
            }
            Debug.LogWarning("Themer: missing " + path);
            return null;
        }

        private static void Surface(GameObject go, Sprite sprite, Color color)
        {
            if (go == null) return;
            var image = go.GetComponent<Image>();
            if (image == null) return;
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = color;
        }

        private static void Ink(GameObject go, int size, Color color, bool display = false,
            TextAnchor? anchor = null)
        {
            if (go == null) return;
            var text = go.GetComponent<Text>();
            if (text == null) return;
            text.font = display ? theme.DisplayFont : theme.BodyFont;
            text.fontStyle = display ? FontStyle.Bold : FontStyle.Normal;
            text.fontSize = size;
            text.color = color;
            text.lineSpacing = 1.28f;
            if (anchor.HasValue) text.alignment = anchor.Value;
        }

        private enum Tier { Primary, Soft, Sage, Ghost, Muted }

        private static void Style(Button button, Tier tier, int fontSize, string label = null)
        {
            if (button == null) return;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                switch (tier)
                {
                    case Tier.Primary: Surface(button.gameObject, theme.CapsulePrimary, Color.white); break;
                    case Tier.Soft: Surface(button.gameObject, theme.CapsuleSoft, Color.white); break;
                    case Tier.Sage: Surface(button.gameObject, theme.CapsuleSage, Color.white); break;
                    case Tier.Muted: Surface(button.gameObject, theme.CapsuleMuted, Color.white); break;
                    case Tier.Ghost:
                        image.sprite = null;
                        image.color = new Color(0, 0, 0, 0);
                        break;
                }
                image.raycastTarget = true;
            }
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, .98f, .93f, 1f);
            colors.pressedColor = new Color(.90f, .85f, .76f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(1f, 1f, 1f, .45f);
            colors.fadeDuration = .08f;
            button.colors = colors;

            var text = button.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                var display = tier == Tier.Primary;
                Ink(text.gameObject, fontSize,
                    tier == Tier.Ghost ? theme.InkFaint : tier == Tier.Soft ? theme.InkSoft : theme.Ink,
                    display, TextAnchor.MiddleCenter);
                if (label != null) text.text = label;
            }
        }

        /// <summary>버튼 안에 아이콘을 하나 붙입니다. 이미 있으면 갱신만 합니다.</summary>
        private static void Leading(Button button, Sprite sprite, Color tint, float size, float inset)
        {
            if (button == null || sprite == null) return;
            var existing = button.transform.Find("Themed Icon");
            var icon = existing != null
                ? existing.GetComponent<Image>()
                : new GameObject("Themed Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            if (existing == null) icon.rectTransform.SetParent(button.transform, false);
            icon.sprite = sprite;
            icon.type = Image.Type.Simple;
            icon.color = tint;
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            var rect = icon.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, .5f);
            rect.pivot = new Vector2(0f, .5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(inset, 0f);
        }

        private static void Shadowed(GameObject go, float drop = 6f)
        {
            if (go == null) return;
            var shadow = go.GetComponent<Shadow>() ?? go.AddComponent<Shadow>();
            shadow.effectColor = theme.ShadowColor;
            shadow.effectDistance = new Vector2(0f, -drop);
            shadow.useGraphicAlpha = true;
        }

        private static void SliderColors(Slider slider, Color fill, Color track)
        {
            if (slider == null) return;
            if (slider.fillRect != null)
            {
                slider.fillRect.sizeDelta = Vector2.zero;
                var image = slider.fillRect.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = theme.Chip;
                    image.type = Image.Type.Sliced;
                    image.pixelsPerUnitMultiplier = 3.4f;
                    image.color = fill;
                }
            }
            var background = slider.transform.Find("Background");
            if (background != null)
            {
                var image = background.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = theme.Chip;
                    image.type = Image.Type.Sliced;
                    image.pixelsPerUnitMultiplier = 3.4f;
                    image.color = track;
                }
            }
        }

        /// <summary>배경 아트를 캔버스 맨 아래에 깔고 그 위에 크림 스크림을 얹습니다.</summary>
        private static void Backdrop(Transform canvas, string spritePath, float scrimAlpha)
        {
            var sprite = Resources.Load<Sprite>(spritePath);
            if (sprite == null) return;
            var art = canvas.Find("Themed Backdrop");
            if (art == null)
            {
                art = new GameObject("Themed Backdrop", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                art.SetParent(canvas, false);
            }
            var artImage = art.GetComponent<Image>();
            artImage.sprite = sprite;
            artImage.type = Image.Type.Simple;
            artImage.preserveAspect = false;
            artImage.color = Color.white;
            artImage.raycastTarget = false;
            Stretch((RectTransform)art);
            art.SetSiblingIndex(0);

            var scrim = canvas.Find("Themed Scrim");
            if (scrim == null)
            {
                scrim = new GameObject("Themed Scrim", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                scrim.SetParent(canvas, false);
            }
            var scrimImage = scrim.GetComponent<Image>();
            scrimImage.sprite = null;
            scrimImage.color = new Color(theme.Paper.r, theme.Paper.g, theme.Paper.b, scrimAlpha);
            scrimImage.raycastTarget = false;
            Stretch((RectTransform)scrim);
            scrim.SetSiblingIndex(1);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // ---------- per scene ----------

        private static void ThemeMainMenu(Scene _)
        {
            var canvas = Find("/Menu Canvas");
            if (canvas == null) return;
            Backdrop(canvas.transform, "UI/ShopTheme/cozy-shop-background", .40f);

            var backdrop = Find("/Menu Canvas/Backdrop");
            if (backdrop != null)
            {
                var image = backdrop.GetComponent<Image>();
                image.sprite = null;
                image.color = new Color(0, 0, 0, 0);
                image.raycastTarget = false;
            }

            Ink(Find("/Menu Canvas/Backdrop/Brand"), 16, theme.Oak);
            Ink(Find("/Menu Canvas/Backdrop/Title"), 76, theme.Ink, true);
            Ink(Find("/Menu Canvas/Backdrop/Tagline"), 20, theme.InkSoft);
            Ink(Find("/Menu Canvas/Backdrop/Flow"), 14, theme.InkFaint);

            var play = Find("/Menu Canvas/Backdrop/Play");
            if (play != null)
            {
                var rect = (RectTransform)play.transform;
                rect.sizeDelta = new Vector2(400f, 78f);
                Style(play.GetComponent<Button>(), Tier.Primary, 27);
                Leading(play.GetComponent<Button>(), theme.IconPaw, theme.Oak, 30f, 46f);
                Shadowed(play, 7f);
            }

            var quit = Find("/Menu Canvas/Backdrop/Quit");
            if (quit != null)
            {
                ((RectTransform)quit.transform).sizeDelta = new Vector2(244f, 54f);
                Style(quit.GetComponent<Button>(), Tier.Soft, 18);
            }
        }

        private static void ThemeHub(Scene _)
        {
            var panel = Find("Shop Routine/Routine Canvas/Routine Panel");
            if (panel == null) return;

            Ink(Find("Shop Routine/Routine Canvas/Routine Panel/Brand"), 15, theme.Oak);
            Ink(Find("Shop Routine/Routine Canvas/Routine Panel/Title"), 34, theme.Ink, true);
            Ink(Find("Shop Routine/Routine Canvas/Routine Panel/Body"), 18, theme.InkSoft);

            var primary = Find("Shop Routine/Routine Canvas/Routine Panel/Continue");
            if (primary != null)
            {
                ((RectTransform)primary.transform).sizeDelta = new Vector2(420f, 70f);
                Style(primary.GetComponent<Button>(), Tier.Primary, 22);
                Leading(primary.GetComponent<Button>(), theme.IconPaw, theme.Oak, 26f, 44f);
                Shadowed(primary, 6f);
            }
            StyleSecondary("Shop Routine/Routine Canvas/Routine Panel/Sell Byproducts", 16, new Vector2(440f, 52f));
            StyleSecondary("Shop Routine/Routine Canvas/Routine Panel/Main Menu", 15, new Vector2(220f, 48f));

            foreach (var name in new[] { "Upgrades", "Supplies", "Decorations" })
            {
                var list = panel.transform.Find(name);
                if (list == null) continue;
                foreach (var button in list.GetComponentsInChildren<Button>(true))
                    StyleCard(button);
            }
            var template = Find("Shop Routine/Routine Canvas/Upgrade Card Template");
            if (template != null) StyleCard(template.GetComponent<Button>());

            WireThemeField(Find("Shop Routine/Routine Canvas"), "ShopRoutineUI");
            AttachOverlays(Find("Shop Routine"));

            var bar = Find("Shop Routine/Routine Canvas/Business HUD");
            Surface(bar, theme.Card, theme.PaperWarm);
            Ink(Find("Shop Routine/Routine Canvas/Business HUD/Clock and wallet"), 16, theme.Ink);
            StyleSecondary("Shop Routine/Routine Canvas/Business HUD/Return Pet", 16, new Vector2(280f, 50f));
        }

        private static void StyleSecondary(string path, int size, Vector2 rect)
        {
            var go = GameObject.Find(path);
            if (go == null) return;
            ((RectTransform)go.transform).sizeDelta = rect;
            Style(go.GetComponent<Button>(), Tier.Soft, size);
        }

        /// <summary>목록 카드입니다. 좌측 정렬 라벨과 여백을 씁니다.</summary>
        private static void StyleCard(Button button)
        {
            if (button == null) return;
            Style(button, Tier.Soft, 15);
            var text = button.GetComponentInChildren<Text>(true);
            if (text == null) return;
            text.alignment = TextAnchor.MiddleLeft;
            var rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(20f, 0f);
            rect.offsetMax = new Vector2(-18f, 0f);
        }

        private static void ThemeReception(Scene _)
        {
            var canvas = Find("/Reception Canvas");
            if (canvas == null) return;

            Backdrop(canvas.transform, "UI/ShopTheme/cozy-shop-background", .34f);
            var flat = Find("/Reception Canvas/Background");
            if (flat != null)
            {
                var image = flat.GetComponent<Image>();
                image.sprite = null;
                image.color = new Color(0, 0, 0, 0);
                image.raycastTarget = false;
            }

            var carrier = Find("/Reception Canvas/Customer Root (enters from left)/Pet Carrier/Carrier Body");
            Surface(carrier, theme.Card, new Color32(206, 226, 213, 255));
            Shadowed(carrier, 5f);


            Surface(Find("/Reception Canvas/Header"), theme.Card, theme.PaperWarm);
            Ink(Find("/Reception Canvas/Title"), 26, theme.Ink, true, TextAnchor.MiddleLeft);
            Ink(Find("/Reception Canvas/Subtitle"), 14, theme.InkFaint, false, TextAnchor.MiddleLeft);

            Surface(Find("/Reception Canvas/Dialogue Panel"), theme.Card, theme.PaperWarm);
            Shadowed(Find("/Reception Canvas/Dialogue Panel"), 8f);
            var speaker = Find("/Reception Canvas/Dialogue Panel/Speaker");
            Ink(speaker, 21, theme.Ink, true, TextAnchor.MiddleLeft);
            // Name plus the relationship badge needs more room than the original 300px.
            if (speaker != null) ((RectTransform)speaker.transform).sizeDelta = new Vector2(620f, 35f);
            Ink(Find("/Reception Canvas/Dialogue Panel/Dialogue"), 19, new Color32(58, 81, 72, 255),
                false, TextAnchor.UpperLeft);

            for (var i = 1; i <= 3; i++)
            {
                var go = GameObject.Find("/Reception Canvas/Dialogue Panel/Question " + i);
                if (go == null) continue;
                Style(go.GetComponent<Button>(), Tier.Soft, 14);
                var text = go.GetComponentInChildren<Text>(true);
                if (text != null) text.alignment = TextAnchor.MiddleCenter;
            }

            var accept = Find("/Reception Canvas/Accept");
            if (accept != null)
            {
                ((RectTransform)accept.transform).sizeDelta = new Vector2(260f, 62f);
                Style(accept.GetComponent<Button>(), Tier.Primary, 20);
                Leading(accept.GetComponent<Button>(), theme.IconPaw, theme.Oak, 24f, 30f);
                Shadowed(accept, 6f);
            }
            StyleSecondary("/Reception Canvas/Reject", 16, new Vector2(260f, 52f));
            var next = Find("/Reception Canvas/Next Customer");
            if (next != null)
            {
                ((RectTransform)next.transform).sizeDelta = new Vector2(340f, 52f);
                Style(next.GetComponent<Button>(), Tier.Soft, 15);
            }
            var care = Find("/Reception Canvas/Start Care");
            if (care != null)
            {
                ((RectTransform)care.transform).sizeDelta = new Vector2(300f, 62f);
                Style(care.GetComponent<Button>(), Tier.Primary, 20);
                Leading(care.GetComponent<Button>(), theme.IconPaw, theme.Oak, 24f, 30f);
                Shadowed(care, 6f);
            }

            Surface(Find("/Reception Canvas/ServiceOrder Memo"), theme.Card, theme.Memo);
            Shadowed(Find("/Reception Canvas/ServiceOrder Memo"), 7f);
            Ink(Find("/Reception Canvas/ServiceOrder Memo/Memo Title"), 17, theme.Ink, true, TextAnchor.MiddleLeft);
            Ink(Find("/Reception Canvas/ServiceOrder Memo/Pet"), 15, theme.InkSoft, false, TextAnchor.MiddleLeft);
            Ink(Find("/Reception Canvas/ServiceOrder Memo/Request Count"), 13, theme.InkFaint, false, TextAnchor.MiddleRight);
            Ink(Find("/Reception Canvas/ServiceOrder Memo/Revealed Conditions"), 15, theme.InkSoft, false, TextAnchor.UpperLeft);

            Surface(Find("/Reception Canvas/Patience Panel"), theme.Card, theme.PaperWarm);
            Ink(Find("/Reception Canvas/Patience Panel/Label"), 12, theme.InkFaint, false, TextAnchor.MiddleLeft);
            var patience = Find("/Reception Canvas/Patience Panel/Patience");
            if (patience != null)
                SliderColors(patience.GetComponent<Slider>(), theme.SageDeep, new Color(theme.Ink.r, theme.Ink.g, theme.Ink.b, .10f));

            var desk = Find("/Reception Canvas/Front Desk - replace sprite");
            if (desk != null)
            {
                var image = desk.GetComponent<Image>();
                if (image != null) image.color = new Color32(178, 133, 88, 255);
            }
            Ink(Find("/Reception Canvas/Front Desk - replace sprite/Desk Sign"), 18, new Color32(247, 238, 219, 255),
                true, TextAnchor.MiddleRight);
        }

        private static void ThemeCare(Scene _)
        {
            var canvas = Find("/Care Canvas");
            if (canvas == null) return;

            Surface(Find("/Care Canvas/Background"), null, new Color32(242, 232, 213, 255));
            Ink(Find("/Care Canvas/Title"), 26, theme.Ink, true, TextAnchor.MiddleLeft);
            Ink(Find("/Care Canvas/Subtitle"), 14, theme.InkFaint, false, TextAnchor.MiddleLeft);
            Surface(Find("/Care Canvas/Remaining Background"), theme.Card, new Color32(45, 65, 60, 255));
            Ink(Find("/Care Canvas/Remaining Status"), 16, new Color32(247, 240, 224, 255), true);

            foreach (var name in new[] { "Status Panel", "Work Panel", "Tool Panel" })
            {
                var go = GameObject.Find("/Care Canvas/" + name);
                Surface(go, theme.Card, theme.PaperWarm);
                Shadowed(go, 6f);
                var heading = go == null ? null : go.transform.Find("Heading");
                if (heading != null) Ink(heading.gameObject, 18, theme.Ink, true, TextAnchor.MiddleLeft);
            }

            Surface(Find("/Care Canvas/Work Panel/Pet Work Stage"), theme.Card, new Color32(250, 243, 227, 255));
            Ink(Find("/Care Canvas/Work Panel/Care Message"), 16, theme.InkSoft);

            for (var i = 1; i <= 5; i++)
            {
                var row = GameObject.Find("/Care Canvas/Status Panel/Condition " + i);
                if (row == null) continue;
                Style(row.GetComponent<Button>(), Tier.Soft, 15);
                Ink(row.transform.Find("Name") == null ? null : row.transform.Find("Name").gameObject,
                    16, theme.Ink, false, TextAnchor.MiddleLeft);
                Ink(row.transform.Find("Care") == null ? null : row.transform.Find("Care").gameObject,
                    13, theme.InkFaint, false, TextAnchor.MiddleRight);
                var label = row.transform.Find("Label");
                if (label != null) label.gameObject.SetActive(false);
                var progress = row.transform.Find("Progress");
                if (progress != null)
                    SliderColors(progress.GetComponent<Slider>(), theme.SageDeep,
                        new Color(theme.Ink.r, theme.Ink.g, theme.Ink.b, .09f));
            }
            Ink(Find("/Care Canvas/Status Panel/Byproduct Heading"), 12, theme.InkFaint, false, TextAnchor.MiddleLeft);
            Ink(Find("/Care Canvas/Status Panel/Byproducts"), 14, theme.InkSoft, false, TextAnchor.UpperLeft);

            foreach (var tool in GameObject.Find("/Care Canvas/Tool Panel").GetComponentsInChildren<Button>(true))
            {
                if (tool.name == "Reset Care")
                {
                    Style(tool, Tier.Sage, 16);
                    continue;
                }
                Style(tool, Tier.Soft, 16);
                var name = tool.transform.Find("Name");
                if (name != null) Ink(name.gameObject, 16, theme.Ink, false, TextAnchor.MiddleLeft);
                var hint = tool.transform.Find("Hint");
                if (hint != null) Ink(hint.gameObject, 12, theme.InkFaint, false, TextAnchor.MiddleLeft);
                var label = tool.transform.Find("Label");
                if (label != null) label.gameObject.SetActive(false);
            }

            // Condition markers: round tinted badge with a line icon instead of a CJK glyph.
            for (var i = 1; i <= 5; i++)
            {
                var mark = GameObject.Find("/Care Canvas/Work Panel/Pet Work Stage/Condition Mark " + i);
                if (mark == null) continue;
                var image = mark.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = theme.Circle;
                    image.type = Image.Type.Simple;
                }
                var glyph = mark.transform.Find("Glyph");
                if (glyph != null) glyph.gameObject.SetActive(false);
                var iconTransform = mark.transform.Find("Icon");
                var icon = iconTransform != null
                    ? iconTransform.GetComponent<Image>()
                    : new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                if (iconTransform == null) icon.rectTransform.SetParent(mark.transform, false);
                icon.raycastTarget = false;
                icon.preserveAspect = true;
                icon.color = Color.white;
                // Markers are resized every frame from the condition zone, so the icon tracks them
                // with relative anchors rather than a fixed size.
                var rect = icon.rectTransform;
                rect.anchorMin = new Vector2(.30f, .30f);
                rect.anchorMax = new Vector2(.70f, .70f);
                rect.pivot = new Vector2(.5f, .5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            WireCareIcons();
            AttachPetSurface();

            var completion = Find("/Care Canvas/Completion Panel");
            Surface(completion, theme.Card, theme.PaperWarm);
            Ink(Find("/Care Canvas/Completion Panel/Title"), 28, theme.Ink, true);
            Ink(Find("/Care Canvas/Completion Panel/Description"), 17, theme.InkSoft);
            Ink(Find("/Care Canvas/Completion Panel/Byproducts"), 15, theme.InkSoft);
        }

        /// <summary>일시정지와 디버그 패널을 씬 이동에도 살아남는 루트에 붙입니다.</summary>
        private static void AttachOverlays(GameObject persistentRoot)
        {
            if (persistentRoot == null) return;
            Ensure<Shell.PauseMenuUI>(persistentRoot);
            Ensure<Shell.DebugCheatPanel>(persistentRoot);
        }

        private static void Ensure<T>(GameObject host) where T : MonoBehaviour
        {
            var component = host.GetComponent<T>();
            if (component == null) component = host.AddComponent<T>();
            var serialized = new SerializedObject(component);
            var field = serialized.FindProperty("theme");
            if (field == null) return;
            field.objectReferenceValue = theme;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>런타임에 UI를 만드는 컴포넌트에도 테마를 꽂아 줍니다.</summary>
        private static void WireThemeField(GameObject canvas, string componentName)
        {
            if (canvas == null) return;
            foreach (var component in canvas.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null || component.GetType().Name != componentName) continue;
                var serialized = new SerializedObject(component);
                var field = serialized.FindProperty("theme");
                if (field == null) continue;
                field.objectReferenceValue = theme;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                return;
            }
            Debug.LogWarning("Themer: no " + componentName + " under " + canvas.name);
        }

        /// <summary>펫 파츠에 표면 시뮬레이션을 붙입니다. 몸통·머리·얼굴만 대상으로 합니다.</summary>
        private static void AttachPetSurface()
        {
            var visual = Find("/Care Canvas/Work Panel/Pet Work Stage/Pet Visual");
            if (visual == null) return;
            var surface = visual.GetComponent<Core.Entity.Pets.PetSurface>()
                       ?? visual.AddComponent<Core.Entity.Pets.PetSurface>();

            // Overgrowth parts keep their own look; only the coat carries dirt and foam.
            string[] coat = { "body", "head", "face" };
            var targets = new System.Collections.Generic.List<Object>();
            foreach (var name in coat)
            {
                var part = visual.transform.Find(name);
                if (part == null) continue;
                var image = part.GetComponent<Image>();
                if (image != null) targets.Add(image);
            }
            SetArray(surface, "parts", targets.ToArray());
            Debug.Log("Themer: pet surface bound to " + targets.Count + " coat parts.");
        }

        private static void SetArray(Object target, string field, Object[] values)
        {
            var serialized = new SerializedObject(target);
            var array = serialized.FindProperty(field);
            array.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>마커 아이콘과 테마를 CareUIComponent에 연결합니다.</summary>
        private static void WireCareIcons()
        {
            var canvas = Find("/Care Canvas");
            if (canvas == null) return;
            var view = canvas.GetComponent<Components.CareUIComponent>();
            if (view == null) return;
            var icons = new Object[5];
            for (var i = 0; i < 5; i++)
            {
                var mark = Find("/Care Canvas/Work Panel/Pet Work Stage/Condition Mark " + (i + 1));
                var icon = mark == null ? null : mark.transform.Find("Icon");
                icons[i] = icon == null ? null : icon.GetComponent<Image>();
            }
            var serialized = new SerializedObject(view);
            var array = serialized.FindProperty("conditionMarkIcons");
            array.arraySize = icons.Length;
            for (var i = 0; i < icons.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = icons[i];
            serialized.FindProperty("theme").objectReferenceValue = theme;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
