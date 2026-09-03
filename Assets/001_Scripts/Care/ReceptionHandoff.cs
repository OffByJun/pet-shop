using UnityEngine;

namespace PetShop.Care
{
    /// <summary>Controls only the timed handoff state.</summary>
    public sealed class ReceptionHandoff : MonoBehaviour, IReceptionHandoff
    {
        [SerializeField, Min(.1f)] private float duration = 1.1f;

        public bool IsRunning { get; private set; }
        public bool IsReady { get; private set; }
        public float Progress => IsReady ? 1f : IsRunning ? Mathf.Clamp01((Time.unscaledTime - startedAt) / duration) : 0f;

        private float startedAt;

        public void Begin()
        {
            startedAt = Time.unscaledTime;
            IsRunning = true;
            IsReady = false;
        }

        private void Update()
        {
            if (IsRunning && Progress >= 1f)
            {
                IsRunning = false;
                IsReady = true;
            }
        }

        public void ResetState()
        {
            IsRunning = false;
            IsReady = false;
        }
    }
}
