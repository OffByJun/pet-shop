namespace _001_Scripts.UI.Shell
{
    /// <summary>일시정지 상태입니다. Time.timeScale이 멈추지 않는 unscaled 로직이 이 값을 봅니다.</summary>
    public static class GamePause
    {
        public static bool IsPaused { get; private set; }

        public static void Set(bool paused)
        {
            IsPaused = paused;
            UnityEngine.Time.timeScale = paused ? 0f : 1f;
        }

        /// <summary>씬 이동이나 종료 전에 시간을 되돌려 놓습니다.</summary>
        public static void Clear() => Set(false);
    }
}
