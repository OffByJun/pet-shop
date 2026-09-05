using System;
using System.Linq;
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
    /// <summary>시작 메뉴 씬을 만들고 빌드 설정의 첫 씬으로 올립니다.</summary>
    public static class MainMenuCreator
    {
        public const string ScenePath = "Assets/000_Scenes/MainMenuScene.unity";
        private const string PlayScene = "ShopRoutineScene";
        private static Font font;
        private static readonly Color Ink = new Color32(45, 65, 60, 255);
        private static readonly Color Green = new Color32(184, 215, 195, 255);
        private static readonly Color Paper = new Color32(245, 241, 228, 255);

        [MenuItem("Tools/PetShop/Routine/Create Main Menu")]
        public static void Create()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before authoring.");
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var camera = new GameObject("Menu Camera").AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Paper;
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                BuildUI();
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                SceneManager.SetActiveScene(previous);
            }
            PutFirstInBuild(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("PetShop main menu created: " + ScenePath);
        }

        /// <summary>메뉴 씬을 빌드 인덱스 0으로 두고 나머지 순서는 유지합니다.</summary>
        public static void PutFirstInBuild(string path)
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) }
                .Concat(EditorBuildSettings.scenes.Where(s => s.path != path)).ToArray();
        }

        private static void BuildUI()
        {
            var canvasObject = new GameObject("Menu Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = .5f;

            var panel = Box(canvasObject.transform, "Backdrop", Vector2.zero, new Vector2(1600, 900), Paper);
            panel.rectTransform.anchorMin = Vector2.zero;
            panel.rectTransform.anchorMax = Vector2.one;
            panel.rectTransform.sizeDelta = Vector2.zero;
            panel.raycastTarget = false;

            Label(panel.transform, "Brand", "PET SHOP  /  COZY CARE", new Vector2(0, 300), new Vector2(1300, 40), 19);
            Label(panel.transform, "Title", "포근포근 펫샵", new Vector2(0, 220), new Vector2(1400, 100), 56);
            Label(panel.transform, "Tagline", "작은 돌봄이 쌓여, 나만의 가게가 됩니다.", new Vector2(0, 150), new Vector2(1200, 44), 22);

            Art(panel.transform, "Pet Art", "Assets/002_Resources/light_phome/Parts/icon.png", new Vector2(-330, -20), 300f);
            // The customer frames carry ~15% transparent padding, so the box is taller than the figure.
            Art(panel.transform, "Customer Art", "Assets/002_Resources/Customer/캐4.png", new Vector2(340, -20), 400f);

            var play = Button(panel.transform, "Play", "영업 시작", new Vector2(0, -190), new Vector2(460, 72));
            var quit = Button(panel.transform, "Quit", "게임 종료", new Vector2(0, -280), new Vector2(300, 54));
            Label(panel.transform, "Flow", "시작 메뉴 → 가게 → 접수대 → 케어룸 → 정산", new Vector2(0, -360), new Vector2(1200, 36), 17);

            var view = canvasObject.AddComponent<MainMenuUI>();
            view.Configure(PlayScene, play, quit);
            EditorUtility.SetDirty(view);
        }

        /// <summary>스프라이트를 원본 비율 그대로 배치합니다.</summary>
        private static Image Art(Transform parent, string name, string spritePath, Vector2 position, float height)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null) { Debug.LogWarning("Main menu art missing: " + spritePath); return null; }
            var size = new Vector2(height * sprite.rect.width / sprite.rect.height, height);
            var image = Box(parent, name, position, size, Color.white);
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static Image Box(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var image = Rect(parent, name, position, size).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text Label(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize)
        {
            var text = Rect(parent, name, position, size).gameObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.color = Ink;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }

        private static Button Button(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            var image = Box(parent, name, position, size, Green);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Label(button.transform, "Label", label, Vector2.zero, size - new Vector2(16, 4), 24);
            return button;
        }
    }
}
