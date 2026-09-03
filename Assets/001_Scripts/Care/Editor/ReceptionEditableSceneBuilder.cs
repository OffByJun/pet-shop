using _001_Scripts.Data.Customers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PetShop.Care.Editor
{
    /// <summary>Editor-only authoring utility. The produced hierarchy is fully serialized and editable.</summary>
    public static class ReceptionEditableSceneBuilder
    {
        private const string ScenePath = "Assets/000_Scenes/CustomerReceptionScene.unity";
        private static readonly Color Navy = new Color32(39, 53, 67, 255);
        private static readonly Color Cream = new Color32(249, 244, 230, 255);
        private static readonly Color Paper = new Color32(255, 253, 246, 255);
        private static readonly Color Mint = new Color32(95, 184, 151, 255);
        private static Font font;

        [MenuItem("PetShop/Reception/Rebuild Editable Reception Scene")]
        public static void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var systems = new GameObject("Reception Systems");
            var source = systems.AddComponent<ReceptionOrderSource>();
            var dialogue = systems.AddComponent<ReceptionDialogueSession>();
            var handoff = systems.AddComponent<ReceptionHandoff>();
            var transition = systems.AddComponent<ReceptionCareSceneTransition>();
            var flow = systems.AddComponent<CustomerReceptionScene>();
            var catalog = AssetDatabase.LoadAssetAtPath<ServiceOrderCatalog>("Assets/002_Resources/ServiceOrders/ServiceOrderCatalog.asset");
            source.Configure(catalog);

            // Serialized camera prevents the Game view's "No cameras rendering" overlay.
            var cameraObject = new GameObject("Reception Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            var receptionCamera = cameraObject.GetComponent<Camera>();
            receptionCamera.clearFlags = CameraClearFlags.SolidColor;
            receptionCamera.backgroundColor = Cream;
            receptionCamera.orthographic = true;

            var canvasObject = new GameObject("Reception Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = .5f;

            var background = Image("Background", canvasObject.transform, Cream, Vector2.zero, new Vector2(1600, 900), Vector2.zero);
            background.transform.SetAsFirstSibling();
            Image("Header", canvasObject.transform, new Color32(244, 237, 218, 255), new Vector2(0, 782), new Vector2(1600, 118), Vector2.zero);
            Text("Title", canvasObject.transform, "포근포근 펫숍 · 접수대", 32, FontStyle.Bold, Navy, new Vector2(52, 815), new Vector2(620, 50), TextAnchor.MiddleLeft);
            Text("Subtitle", canvasObject.transform, "손님의 이야기를 듣고 케어 주문을 확인하세요", 14, FontStyle.Normal, Navy, new Vector2(54, 790), new Vector2(600, 28), TextAnchor.MiddleLeft);

            var patiencePanel = Image("Patience Panel", canvasObject.transform, Navy, new Vector2(1210, 810), new Vector2(320, 62), Vector2.zero);
            Text("Label", patiencePanel.transform, "손님 인내도", 14, FontStyle.Bold, Color.white, new Vector2(18, 31), new Vector2(150, 23), TextAnchor.MiddleLeft);
            var patience = Slider("Patience", patiencePanel.transform, new Vector2(20, 12), new Vector2(280, 14));

            var speech = Image("Dialogue Panel", canvasObject.transform, Paper, new Vector2(610, 500), new Vector2(900, 305), new Vector2(.5f, .5f));
            var speakerText = Text("Speaker", speech.transform, "손님", 21, FontStyle.Bold, Navy, new Vector2(28, 245), new Vector2(300, 35), TextAnchor.MiddleLeft);
            var lineText = Text("Dialogue", speech.transform, "", 22, FontStyle.Normal, Navy, new Vector2(28, 92), new Vector2(844, 140), TextAnchor.MiddleLeft);
            var questionButtons = new Button[3];
            var questionLabels = new Text[3];
            for (var i = 0; i < 3; i++)
            {
                questionButtons[i] = Button("Question " + (i + 1), speech.transform, "질문", new Vector2(28 + i * 269, 20), new Vector2(255, 62), out questionLabels[i]);
            }

            var accept = Button("Accept", canvasObject.transform, "맡아볼게요", new Vector2(326, 402), new Vector2(260, 58), out _);
            var reject = Button("Reject", canvasObject.transform, "오늘은 어려워요", new Vector2(606, 402), new Vector2(260, 58), out _);
            var next = Button("Next Customer", canvasObject.transform, "다음 무작위 손님", new Vector2(886, 402), new Vector2(360, 58), out _);
            var care = Button("Start Care", canvasObject.transform, "케어 시작하기  →", new Vector2(522, 436), new Vector2(300, 62), out _);

            var memoPanel = Image("ServiceOrder Memo", canvasObject.transform, new Color32(246, 239, 211, 255), new Vector2(56, 58), new Vector2(900, 292), Vector2.zero);
            Text("Memo Title", memoPanel.transform, "ServiceOrder 접수 메모", 21, FontStyle.Bold, Navy, new Vector2(28, 232), new Vector2(420, 38), TextAnchor.MiddleLeft);
            var petText = Text("Pet", memoPanel.transform, "", 17, FontStyle.Bold, Navy, new Vector2(28, 200), new Vector2(500, 30), TextAnchor.MiddleLeft);
            var requestCount = Text("Request Count", memoPanel.transform, "", 14, FontStyle.Bold, Navy, new Vector2(560, 235), new Vector2(300, 30), TextAnchor.MiddleRight);
            var memoText = Text("Revealed Conditions", memoPanel.transform, "", 16, FontStyle.Normal, Navy, new Vector2(28, 48), new Vector2(820, 135), TextAnchor.UpperLeft);

            // Customer enters from outside the LEFT edge. All visual children are editable placeholders.
            var customerRoot = Rect("Customer Root (enters from left)", canvasObject.transform, new Vector2(-260, -40), new Vector2(420, 600), new Vector2(0, 0));
            var body = Image("Customer Body - replace sprite", customerRoot, new Color32(92, 139, 181, 255), new Vector2(60, 40), new Vector2(300, 330), Vector2.zero);
            Image("Customer Head - replace sprite", customerRoot, new Color32(245, 210, 176, 255), new Vector2(125, 340), new Vector2(170, 170), Vector2.zero);
            var carrier = Rect("Pet Carrier", customerRoot, new Vector2(230, 120), new Vector2(220, 145), new Vector2(0, 0));
            Image("Carrier Body", carrier, new Color32(88, 104, 113, 255), Vector2.zero, new Vector2(220, 145), Vector2.zero);
            var petBody = Image("Pet Sprite - replace", carrier, new Color32(224, 176, 113, 255), new Vector2(62, 46), new Vector2(96, 92), Vector2.zero);

            // Desk is a real pre-authored object and stays in front of the customer.
            var desk = Image("Front Desk - replace sprite", canvasObject.transform, new Color32(135, 88, 59, 255), new Vector2(0, 0), new Vector2(1600, 210), Vector2.zero);
            Image("Desk Top", desk.transform, new Color32(102, 66, 48, 255), new Vector2(0, 190), new Vector2(1600, 20), Vector2.zero);
            Text("Desk Sign", desk.transform, "PET CARE · RECEPTION", 22, FontStyle.Bold, new Color32(238, 216, 174, 255), new Vector2(1030, 55), new Vector2(450, 70), TextAnchor.MiddleCenter);
            var deskAnchor = Rect("Pet Handoff Anchor", desk.transform, new Vector2(760, 160), new Vector2(10, 10), Vector2.zero);

            // Customer is behind the counter; dialogue and controls remain in the foreground.
            customerRoot.SetSiblingIndex(4);
            desk.rectTransform.SetSiblingIndex(5);

            var actor = customerRoot.gameObject.AddComponent<ReceptionCustomerActor>();
            actor.Configure(customerRoot, carrier, deskAnchor, body, petBody);
            var ui = canvasObject.AddComponent<ReceptionUIComponent>();
            ui.Configure(speakerText, lineText, patience, questionButtons, questionLabels,
                petText, requestCount, memoText, accept, reject, next, care);
            flow.Configure(source, dialogue, handoff, actor, transition, ui);

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = systems;
            Debug.Log("Editable reception scene rebuilt at " + ScenePath);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size, Vector2 anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = Vector2.zero;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        private static Image Image(string name, Transform parent, Color color, Vector2 position, Vector2 size, Vector2 anchor)
        {
            var rt = Rect(name, parent, position, size, anchor);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text Text(string name, Transform parent, string value, int size, FontStyle style, Color color, Vector2 position, Vector2 dimensions, TextAnchor alignment)
        {
            var rt = Rect(name, parent, position, dimensions, Vector2.zero);
            var text = rt.gameObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Button Button(string name, Transform parent, string label, Vector2 position, Vector2 size, out Text text)
        {
            var image = Image(name, parent, new Color32(239, 236, 226, 255), position, size, Vector2.zero);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            text = Text("Label", image.transform, label, 15, FontStyle.Bold, Navy, new Vector2(14, 0), new Vector2(size.x - 28, size.y), TextAnchor.MiddleLeft);
            return button;
        }

        private static Slider Slider(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var root = Rect(name, parent, position, size, Vector2.zero);
            var background = Image("Background", root, new Color32(91, 104, 114, 255), Vector2.zero, size, Vector2.zero);
            var fillArea = Rect("Fill Area", root, new Vector2(2, 2), new Vector2(size.x - 4, size.y - 4), Vector2.zero);
            var fill = Image("Fill", fillArea, Mint, Vector2.zero, fillArea.sizeDelta, Vector2.zero);
            var slider = root.gameObject.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;
            slider.fillRect = fill.rectTransform;
            slider.targetGraphic = background;
            slider.interactable = false;
            return slider;
        }
    }
}
