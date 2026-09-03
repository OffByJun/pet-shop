using System;
using UnityEngine;

namespace _001_Scripts.UI.UILib
{
    [Serializable]
    public sealed class UIAnimationSettings
    {
        [SerializeField, Min(0f)] private float duration = 0.2f;
        [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField, Range(0f, 1f)] private float alpha = 1f;
        [SerializeField] private Vector3 scale = Vector3.one;
        [SerializeField] private Vector2 anchoredPositionOffset = Vector2.zero;

        public float Duration => duration;

        public AnimationCurve Easing => easing;

        public bool UseUnscaledTime => useUnscaledTime;

        public float Alpha => alpha;

        public Vector3 Scale => scale;

        public Vector2 AnchoredPositionOffset => anchoredPositionOffset;

        public void SetDefaults(
            float newDuration,
            float newAlpha,
            Vector3 newScale,
            Vector2 newAnchoredPositionOffset)
        {
            duration = Mathf.Max(0f, newDuration);
            alpha = Mathf.Clamp01(newAlpha);
            scale = newScale;
            anchoredPositionOffset = newAnchoredPositionOffset;
        }
    }
}
