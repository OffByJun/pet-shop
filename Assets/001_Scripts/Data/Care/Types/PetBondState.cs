using UnityEngine;

namespace _001_Scripts.Data
{
    public enum PetTemperament { Friendly, Cautious, Shy, Curious, Calm, Playful }

    /// <summary>한 번의 케어 방문 동안 손길에 따라 달라지는 펫의 신뢰와 기분입니다.</summary>
    public sealed class PetBondState
    {
        private float gainScale = 1f;
        private float lossScale = 1f;

        public PetTemperament Temperament { get; private set; }
        public float Trust { get; private set; }
        public float Trust01 => Trust / 100f;
        public string TemperamentLabel => Temperament switch
        {
            PetTemperament.Friendly => "다정한 성격",
            PetTemperament.Cautious => "도도한 성격",
            PetTemperament.Shy => "겁이 많은 성격",
            PetTemperament.Curious => "호기심 많은 성격",
            PetTemperament.Calm => "느긋한 성격",
            PetTemperament.Playful => "장난기 많은 성격",
            _ => "보통 성격"
        };
        public string MoodLabel => Trust switch
        {
            >= 90f => "완전히 마음을 열었어요",
            >= 72f => "행복해요",
            >= 50f => "편안해요",
            >= 28f => "경계하고 있어요",
            _ => "많이 불안해요"
        };
        public float ProgressMultiplier => Trust switch
        {
            >= 90f => 1.18f,
            >= 72f => 1.10f,
            >= 50f => 1.03f,
            >= 28f => .92f,
            _ => .80f
        };
        public float RewardMultiplier => Mathf.Lerp(.88f, 1.18f, Trust01);

        public void Reset(string variantId)
        {
            var id = (variantId ?? string.Empty).ToLowerInvariant();
            if (id.Contains("cat")) Configure(PetTemperament.Cautious, 42f, .88f, 1.16f);
            else if (id.Contains("rabbit")) Configure(PetTemperament.Shy, 38f, .94f, 1.24f);
            else if (id.Contains("hamster")) Configure(PetTemperament.Curious, 48f, 1.12f, 1f);
            else if (id.Contains("guinea")) Configure(PetTemperament.Calm, 60f, 1f, .78f);
            else if (id.Contains("parrot")) Configure(PetTemperament.Playful, 50f, 1.18f, 1.08f);
            else Configure(PetTemperament.Friendly, 55f, 1.08f, .90f);
        }

        public string RegisterInteraction(CareInteractionStatus status, float pointerDistance)
        {
            var before = MoodBand();
            switch (status)
            {
                case CareInteractionStatus.WrongTool: Change(-8f * lossScale); break;
                case CareInteractionStatus.NeedsWater: Change(-3f * lossScale); break;
                case CareInteractionStatus.StageCompleted: Change(5f * gainScale); break;
                case CareInteractionStatus.Resolved: Change(9f * gainScale); break;
                case CareInteractionStatus.Wetting:
                case CareInteractionStatus.Progressed:
                    Change(Mathf.Min(1.1f, Mathf.Max(0f, pointerDistance) * .006f) * gainScale);
                    break;
            }

            if (before != MoodBand()) return Trust >= 72f ? "펫이 기분 좋게 몸을 맡깁니다!" : MoodLabel;
            return status switch
            {
                CareInteractionStatus.WrongTool => "펫이 깜짝 놀라 몸을 움츠렸어요.",
                CareInteractionStatus.NeedsWater => "마른 털이 당겨져 펫이 불편해해요.",
                CareInteractionStatus.StageCompleted => "펫이 한결 편안해졌어요.",
                CareInteractionStatus.Resolved => "펫이 고맙다는 듯 다가왔어요!",
                _ => string.Empty
            };
        }

        public string RegisterDiscovery()
        {
            Change(4f * gainScale);
            return "아픈 곳을 알아채자 펫의 경계가 조금 풀렸어요.";
        }

        public string RegisterEventChoice(float assistProgress, int flowBeats)
        {
            Change((5f + assistProgress * 16f + flowBeats * .75f) * gainScale);
            return Trust >= 90f ? "완전 신뢰 달성! 최종 보상이 크게 올라갑니다." : "세심한 대응으로 신뢰가 올랐어요.";
        }

        private int MoodBand() => Trust >= 90f ? 4 : Trust >= 72f ? 3 : Trust >= 50f ? 2 : Trust >= 28f ? 1 : 0;

        private void Configure(PetTemperament temperament, float trust, float gain, float loss)
        {
            Temperament = temperament;
            Trust = trust;
            gainScale = gain;
            lossScale = loss;
        }

        private void Change(float amount) => Trust = Mathf.Clamp(Trust + amount, 0f, 100f);
    }
}
