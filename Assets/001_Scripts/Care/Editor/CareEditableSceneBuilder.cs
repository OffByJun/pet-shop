using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PetShop.Care.Editor
{
    public static class CareEditableSceneBuilder
    {
        private const string ScenePath = "Assets/000_Scenes/CarePlayScene.unity";
        private static readonly Color Navy = new Color32(36, 51, 66, 255);
        private static readonly Color Cream = new Color32(250, 247, 238, 255);
        private static readonly Color Panel = new Color32(255, 253, 247, 255);
        private static Font font;

        [MenuItem("PetShop/Care/Rebuild Editable uGUI Care Scene")]
        public static void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var systems = new GameObject("Care Systems");
            var flow = systems.AddComponent<CarePlayScene>();

            var cameraObject = new GameObject("Care Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Cream;
            camera.orthographic = true;

            var canvasObject = new GameObject("Care Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = .5f;

            Image("Background", canvasObject.transform, Cream, Vector2.zero, new Vector2(1600, 900));
            var title = Text("Title", canvasObject.transform, "포근포근 케어룸", 32, FontStyle.Bold, Navy,
                new Vector2(38, 814), new Vector2(650, 52), TextAnchor.MiddleLeft);
            Text("Subtitle", canvasObject.transform, "상태를 보고, 도구를 골라 직접 해결해 주세요.", 14, FontStyle.Normal,
                new Color32(91, 103, 111, 255), new Vector2(40, 785), new Vector2(650, 28), TextAnchor.MiddleLeft);
            var remaining = Text("Remaining Status", canvasObject.transform, "남은 상태", 16, FontStyle.Bold, Color.white,
                new Vector2(1268, 812), new Vector2(286, 44), TextAnchor.MiddleCenter);
            Image("Remaining Background", remaining.transform.parent, Navy, new Vector2(1268, 812), new Vector2(286, 44))
                .transform.SetSiblingIndex(remaining.transform.GetSiblingIndex());
            remaining.transform.SetAsLastSibling();

            var statusPanel = Image("Status Panel", canvasObject.transform, Panel, new Vector2(36, 42), new Vector2(330, 744));
            Text("Heading", statusPanel.transform, "1. 펫 상태 확인", 20, FontStyle.Bold, Navy,
                new Vector2(20, 684), new Vector2(280, 34), TextAnchor.MiddleLeft);
            var conditionButtons = new Button[5];
            var conditionNames = new Text[5];
            var conditionCare = new Text[5];
            var conditionProgress = new Slider[5];
            for (var i = 0; i < 5; i++)
            {
                var rowY = 568 - i * 98;
                conditionButtons[i] = Button("Condition " + (i + 1), statusPanel.transform, "", new Vector2(18, rowY), new Vector2(294, 88), out _);
                conditionNames[i] = Text("Name", conditionButtons[i].transform, "상태", 16, FontStyle.Bold, Navy,
                    new Vector2(20, 48), new Vector2(164, 28), TextAnchor.MiddleLeft);
                conditionCare[i] = Text("Care", conditionButtons[i].transform, "케어", 13, FontStyle.Bold, Navy,
                    new Vector2(200, 50), new Vector2(76, 24), TextAnchor.MiddleCenter);
                conditionProgress[i] = Slider("Progress", conditionButtons[i].transform, new Vector2(20, 18), new Vector2(256, 12));
            }
            Text("Byproduct Heading", statusPanel.transform, "발생한 부산물", 18, FontStyle.Bold, Navy,
                new Vector2(20, 76), new Vector2(280, 30), TextAnchor.MiddleLeft);
            var byproducts = Text("Byproducts", statusPanel.transform, "아직 없음", 14, FontStyle.Normal, Navy,
                new Vector2(20, 22), new Vector2(280, 50), TextAnchor.UpperLeft);

            var workPanel = Image("Work Panel", canvasObject.transform, Panel, new Vector2(390, 42), new Vector2(790, 744));
            Text("Heading", workPanel.transform, "2. 도구로 직접 처리", 20, FontStyle.Bold, Navy,
                new Vector2(24, 684), new Vector2(360, 34), TextAnchor.MiddleLeft);
            var stage = Image("Pet Work Stage", workPanel.transform, new Color32(224, 240, 232, 255),
                new Vector2(28, 84), new Vector2(734, 580));
            stage.raycastTarget = true;
            var petBody = Image("Pet Body - replace sprite", stage.transform, new Color32(225, 180, 126, 255),
                new Vector2(157, 120), new Vector2(420, 300));
            petBody.raycastTarget = false;
            var petHead = Image("Pet Head - replace sprite", stage.transform, new Color32(241, 202, 152, 255),
                new Vector2(241, 330), new Vector2(252, 210));
            petHead.raycastTarget = false;

            var marks = new RectTransform[5];
            var markImages = new Image[5];
            var markLabels = new Text[5];
            for (var i = 0; i < 5; i++)
            {
                markImages[i] = Image("Condition Mark " + (i + 1), stage.transform, new Color32(74, 158, 210, 220),
                    new Vector2(20 + i * 112, 220), new Vector2(100, 100));
                marks[i] = markImages[i].rectTransform;
                markLabels[i] = Text("Glyph", marks[i], "?", 24, FontStyle.Bold, Color.white,
                    Vector2.zero, new Vector2(100, 100), TextAnchor.MiddleCenter);
                markLabels[i].raycastTarget = false;
            }
            var stageInput = stage.gameObject.AddComponent<CareStageInput>();
            stageInput.Configure(marks);
            var message = Text("Care Message", workPanel.transform, "상태를 확인하고 알맞은 도구를 선택하세요.", 16,
                FontStyle.Bold, Navy, new Vector2(36, 24), new Vector2(718, 44), TextAnchor.MiddleCenter);

            var toolPanel = Image("Tool Panel", canvasObject.transform, Panel, new Vector2(1204, 42), new Vector2(360, 744));
            Text("Heading", toolPanel.transform, "3. 케어 도구 선택", 20, FontStyle.Bold, Navy,
                new Vector2(20, 684), new Vector2(310, 34), TextAnchor.MiddleLeft);
            var toolButtons = new Button[6];
            var toolImages = new Image[6];
            var toolNames = new[] { "물뿌리개", "세척 브러시", "빗", "치료 도구", "집게", "가위" };
            var toolHints = new[] { "먼저 충분히 적시기", "젖은 오염 문지르기", "엉킨 털 빗기", "상처 부위 치료", "붙은 이물질 제거", "긴 털 정리" };
            for (var i = 0; i < 6; i++)
            {
                var y = 576 - i * 91;
                toolButtons[i] = Button("Tool " + toolNames[i], toolPanel.transform, "", new Vector2(20, y), new Vector2(320, 82), out _);
                toolImages[i] = toolButtons[i].GetComponent<Image>();
                Text("Name", toolButtons[i].transform, toolNames[i], 16, FontStyle.Bold, Navy,
                    new Vector2(28, 39), new Vector2(260, 27), TextAnchor.MiddleLeft);
                Text("Hint", toolButtons[i].transform, toolHints[i], 13, FontStyle.Normal, new Color32(91, 103, 111, 255),
                    new Vector2(28, 13), new Vector2(260, 24), TextAnchor.MiddleLeft);
            }
            var reset = Button("Reset Care", toolPanel.transform, "처음부터", new Vector2(84, 20), new Vector2(192, 40), out _);

            var completion = Image("Completion Panel", canvasObject.transform, new Color(0.15f, .2f, .24f, .96f),
                new Vector2(440, 250), new Vector2(720, 400));
            Text("Title", completion.transform, "케어 완료!", 44, FontStyle.Bold, Color.white,
                new Vector2(40, 264), new Vector2(640, 76), TextAnchor.MiddleCenter);
            Text("Description", completion.transform, "모든 상태의 진행도가 0이 되었습니다.", 19, FontStyle.Normal, Color.white,
                new Vector2(70, 190), new Vector2(580, 60), TextAnchor.MiddleCenter);
            var completionByproducts = Text("Byproducts", completion.transform, "획득 부산물", 17, FontStyle.Bold, Color.white,
                new Vector2(80, 115), new Vector2(560, 50), TextAnchor.MiddleCenter);
            completion.gameObject.SetActive(false);

            var ui = canvasObject.AddComponent<CareUIComponent>();
            ui.Configure(title, remaining, message, byproducts, stage.rectTransform,
                toolButtons, toolImages, conditionButtons, conditionNames, conditionCare, conditionProgress,
                marks, markImages, markLabels, reset, completion.gameObject, completionByproducts);
            flow.Configure(ui, stageInput);

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = systems;
            Debug.Log("Editable uGUI care scene rebuilt at " + ScenePath);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = Vector2.zero;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        private static Image Image(string name, Transform parent, Color color, Vector2 position, Vector2 size)
        {
            var image = Rect(name, parent, position, size).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text Text(string name, Transform parent, string value, int size, FontStyle style, Color color,
            Vector2 position, Vector2 dimensions, TextAnchor alignment)
        {
            var text = Rect(name, parent, position, dimensions).gameObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.resizeTextForBestFit = false;
            return text;
        }

        private static Button Button(string name, Transform parent, string label, Vector2 position, Vector2 size, out Text text)
        {
            var image = Image(name, parent, new Color32(247, 244, 235, 255), position, size);
            var button = image.gameObject.AddComponent<Button>();
            text = Text("Label", button.transform, label, 15, FontStyle.Bold, Navy, Vector2.zero, size, TextAnchor.MiddleCenter);
            return button;
        }

        private static Slider Slider(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var root = Rect(name, parent, position, size);
            var slider = root.gameObject.AddComponent<Slider>();
            var background = Image("Background", root, new Color32(221, 218, 208, 255), Vector2.zero, size);
            var fillArea = Rect("Fill Area", root, Vector2.zero, size);
            var fill = Image("Fill", fillArea, new Color32(95, 184, 151, 255), Vector2.zero, size);
            slider.fillRect = fill.rectTransform;
            slider.targetGraphic = background;
            slider.interactable = false;
            slider.value = 0f;
            return slider;
        }
    }
}
