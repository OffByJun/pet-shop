using _001_Scripts.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _001_Scripts.UI
{
    /// <summary>시작 메뉴입니다. 씬 이동만 담당하고 게임 상태는 소유하지 않습니다.</summary>
    public sealed class MainMenuUI : GameBehaviour
    {
        [Tooltip("영업을 시작할 때 넘어갈 씬입니다. 하루 루프의 허브 씬을 지정하세요.")]
        [SerializeField] private string playScene = "ShopRoutineScene";
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;
        private bool loading;

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(Play);
            if (quitButton != null) quitButton.onClick.AddListener(Quit);
        }

        public void Play()
        {
            if (loading || string.IsNullOrWhiteSpace(playScene)) return;
            loading = true;
            if (playButton != null) playButton.interactable = false;
            SceneManager.LoadScene(playScene);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void Configure(string scene, Button play, Button quit)
        {
            playScene = scene;
            playButton = play;
            quitButton = quit;
        }
    }
}
