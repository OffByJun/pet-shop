using UnityEngine;

namespace _001_Scripts.Data
{
    public enum CareFlowFeedback
    {
        None,
        Good,
        Great,
        Perfect,
        Fever,
        Resolved,
        Broken
    }

    /// <summary>
    /// Session-local mastery loop for care interactions. Continuous, accurate strokes build
    /// flow; high flow starts a short fever that accelerates care progress.
    /// </summary>
    public sealed class CareFlowState
    {
        private const float BeatDistance = 56f;
        private const float ComboGraceSeconds = 1.15f;
        private const int FeverCombo = 12;
        private const float FeverDuration = 5.5f;

        private float beatCharge;
        private float lastSuccessTime = float.NegativeInfinity;
        private float feverRemaining;

        public int Combo { get; private set; }
        public int BestCombo { get; private set; }
        public int Score { get; private set; }
        public bool IsFever => feverRemaining > 0f;
        public float FeverRemaining => feverRemaining;
        public float Meter => IsFever ? 1f : Mathf.Clamp01(Combo / (float)FeverCombo);
        public float ProgressMultiplier => IsFever ? 1.75f : 1f + Mathf.Min(Combo, 10) * .035f;

        public string Grade => BestCombo switch
        {
            >= 16 => "S · 반짝 케어 마스터",
            >= 12 => "A · 환상의 손길",
            >= 8 => "B · 능숙한 돌봄",
            >= 4 => "C · 따뜻한 손길",
            _ => "D · 첫걸음"
        };

        public CareFlowFeedback RegisterSuccess(float pointerDistance, float now, bool resolved)
        {
            if (pointerDistance <= 0f || float.IsNaN(pointerDistance) || float.IsInfinity(pointerDistance))
                return resolved ? CareFlowFeedback.Resolved : CareFlowFeedback.None;

            if (!IsFever && now - lastSuccessTime > ComboGraceSeconds)
            {
                Combo = 0;
                beatCharge = 0f;
            }

            lastSuccessTime = now;
            beatCharge += Mathf.Min(pointerDistance, BeatDistance);
            var feedback = CareFlowFeedback.None;
            while (beatCharge >= BeatDistance)
            {
                beatCharge -= BeatDistance;
                Combo++;
                BestCombo = Mathf.Max(BestCombo, Combo);
                Score += Mathf.RoundToInt(100f * ProgressMultiplier);
                feedback = Combo % 8 == 0 ? CareFlowFeedback.Perfect
                    : Combo % 4 == 0 ? CareFlowFeedback.Great
                    : CareFlowFeedback.None;

                if (Combo >= FeverCombo && !IsFever)
                {
                    feverRemaining = FeverDuration;
                    feedback = CareFlowFeedback.Fever;
                }
            }

            if (IsFever)
                feverRemaining = Mathf.Min(FeverDuration, feverRemaining + pointerDistance * .0025f);
            return resolved ? CareFlowFeedback.Resolved : feedback;
        }

        public CareFlowFeedback BreakCombo()
        {
            if (IsFever) return CareFlowFeedback.None;
            var hadCombo = Combo > 0;
            Combo = 0;
            beatCharge = 0f;
            return hadCombo ? CareFlowFeedback.Broken : CareFlowFeedback.None;
        }

        public CareFlowFeedback GrantMomentum(int beats, float now)
        {
            var feedback = CareFlowFeedback.None;
            for (var i = 0; i < Mathf.Max(0, beats); i++)
                feedback = RegisterSuccess(56f, now, false);
            return feedback;
        }

        public void Tick(float unscaledDeltaTime, float now)
        {
            if (feverRemaining > 0f)
            {
                feverRemaining = Mathf.Max(0f, feverRemaining - Mathf.Max(0f, unscaledDeltaTime));
                if (feverRemaining <= 0f)
                {
                    Combo = 0;
                    beatCharge = 0f;
                }
                return;
            }

            if (Combo > 0 && now - lastSuccessTime > ComboGraceSeconds)
            {
                Combo = 0;
                beatCharge = 0f;
            }
        }

        public void Reset()
        {
            Combo = 0;
            BestCombo = 0;
            Score = 0;
            beatCharge = 0f;
            feverRemaining = 0f;
            lastSuccessTime = float.NegativeInfinity;
        }
    }
}
