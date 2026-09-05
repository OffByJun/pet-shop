using _001_Scripts.Core;
using _001_Scripts.Managers;
using _001_Scripts.UI.Theme;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _001_Scripts.UI.Shell
{
    /// <summary>ESC로 여는 일시정지 화면입니다. 자기 캔버스를 직접 만들어 어느 씬에서나 뜹니다.</summary>
    public sealed class PauseMenuUI : GameBehaviour
    {
        [SerializeField] private UITheme theme;
        [Tooltip("메인 메뉴로 돌아갈 때 쓰는 씬 이름입니다. 루틴이 없을 때만 사용합니다.")]
        [SerializeField] private string mainMenuScene = "MainMenuScene";

        private Canvas canvas;
        private GameObject root;
        private UnityEngine.UI.Button giveUp;
        private UnityEngine.UI.Text giveUpLabel;

        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            if (theme == null) return;
            Build();
            root.SetActive(false);
        }

        private void OnDestroy()
        {
            // Never leave the game frozen if this object goes away while paused.
            if (IsOpen) GamePause.Clear();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || root == null) return;
            if (keyboard.escapeKey.wasPressedThisFrame) Toggle();
        }

        public void Toggle()
        {
            if (root == null) return;
            var open = !root.activeSelf;
            root.SetActive(open);
            if (open) RefreshGiveUp();
            GamePause.Set(open);
        }

        /// <summary>영업을 접는 대가를 라벨에 그대로 적어 둡니다.</summary>
        private void RefreshGiveUp()
        {
            if (giveUp == null) return;
            var available = ShopRoutineManager.HasInstance && ShopRoutineManager.Instance.CanGiveUpDay;
            giveUp.gameObject.SetActive(available);
            if (!available || giveUpLabel == null) return;
            var remaining = ShopRoutineManager.Instance.RemainingCustomers;
            giveUpLabel.text = remaining > 0
                ? $"오늘 영업 접기 · 남은 손님 {remaining}명 미응대"
                : "오늘 영업 접기";
        }

        private void GiveUpDay()
        {
            if (!ShopRoutineManager.HasInstance) return;
            ShopRoutineManager.Instance.GiveUpDay();
            Close();
        }

        public void Close()
        {
            if (root == null || !root.activeSelf) return;
            root.SetActive(false);
            GamePause.Clear();
        }

        private void Build()
        {
            canvas = ThemedUIBuilder.Overlay("Pause Canvas", 900);
            canvas.transform.SetParent(transform, false);
            root = canvas.gameObject;

            var scrim = ThemedUIBuilder.Fill("Scrim", root.transform,
                new Color(theme.Ink.r * .5f, theme.Ink.g * .48f, theme.Ink.b * .42f, .70f));
            scrim.raycastTarget = true;

            var card = ThemedUIBuilder.Surface("Pause Card", root.transform, theme, theme.Card,
                theme.PaperWarm, Vector2.zero, new Vector2(520f, 520f));

            ThemedUIBuilder.Label("Eyebrow", card.transform, theme, "잠시 멈춤", 13, theme.Oak,
                TextAnchor.MiddleCenter, new Vector2(0f, 158f), new Vector2(440f, 26f));
            ThemedUIBuilder.Label("Title", card.transform, theme, "가게를 잠시 비웠어요", 28, theme.Ink,
                TextAnchor.MiddleCenter, new Vector2(0f, 112f), new Vector2(460f, 48f), true);
            ThemedUIBuilder.Label("Hint", card.transform, theme, "ESC를 다시 누르면 이어서 진행합니다", 14,
                theme.InkFaint, TextAnchor.MiddleCenter, new Vector2(0f, 70f), new Vector2(460f, 26f));

            var resume = ThemedUIBuilder.Capsule("Resume", card.transform, theme, "계속하기", true,
                new Vector2(0f, 6f), new Vector2(380f, 68f));
            resume.onClick.AddListener(Close);

            giveUp = ThemedUIBuilder.Capsule("Give Up Day", card.transform, theme, "오늘 영업 접기", false,
                new Vector2(0f, -70f), new Vector2(380f, 54f));
            giveUpLabel = giveUp.GetComponentInChildren<UnityEngine.UI.Text>(true);
            giveUp.onClick.AddListener(GiveUpDay);

            var menu = ThemedUIBuilder.Capsule("Main Menu", card.transform, theme, "메인 메뉴로", false,
                new Vector2(0f, -132f), new Vector2(380f, 54f));
            menu.onClick.AddListener(ToMainMenu);

            var quit = ThemedUIBuilder.Capsule("Quit", card.transform, theme, "게임 종료", false,
                new Vector2(0f, -194f), new Vector2(380f, 54f));
            quit.onClick.AddListener(Quit);
        }

        private void ToMainMenu()
        {
            Close();
            if (ShopRoutineManager.HasInstance) { ShopRoutineManager.Instance.ReturnToMainMenu(); return; }
            if (!string.IsNullOrWhiteSpace(mainMenuScene))
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }

        private void Quit()
        {
            GamePause.Clear();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
