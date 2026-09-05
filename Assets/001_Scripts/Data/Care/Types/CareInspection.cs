using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data
{
    public enum InspectHeat { None, Cold, Cool, Warm, Hot }

    /// <summary>
    /// 살펴보기 미니게임 상태입니다. 펫 위를 훑으면 가까운 증상에 반응이 오고,
    /// 뜨거운 자리에 머물면 확신이 차올라 증상을 찾아냅니다. 진찰력은 한정되어 있습니다.
    /// </summary>
    public sealed class CareInspection
    {
        private readonly float warmRadius;
        private readonly float hotRadius;
        private readonly float staminaPerUnit;
        private readonly float confidencePerSecond;
        private readonly float confidenceDecayPerSecond;

        public float Stamina { get; private set; }
        public float MaxStamina { get; }
        public float Stamina01 => MaxStamina <= 0f ? 0f : Mathf.Clamp01(Stamina / MaxStamina);
        public float Confidence { get; private set; }
        public InspectHeat Heat { get; private set; }
        /// <summary>지금 반응하고 있는 증상입니다. 확신이 차면 이 증상을 찾아냅니다.</summary>
        public CareConditionState Focus { get; private set; }
        public bool Exhausted => Stamina <= 0f;

        public CareInspection(float maxStamina, float warmRadius, float hotRadius,
            float staminaPerUnit, float confidencePerSecond, float confidenceDecayPerSecond)
        {
            MaxStamina = Mathf.Max(1f, maxStamina);
            Stamina = MaxStamina;
            this.warmRadius = Mathf.Max(.01f, warmRadius);
            this.hotRadius = Mathf.Clamp(hotRadius, .001f, this.warmRadius);
            this.staminaPerUnit = Mathf.Max(0f, staminaPerUnit);
            this.confidencePerSecond = Mathf.Max(.01f, confidencePerSecond);
            this.confidenceDecayPerSecond = Mathf.Max(0f, confidenceDecayPerSecond);
        }

        public void Reset()
        {
            Stamina = MaxStamina;
            Confidence = 0f;
            Heat = InspectHeat.None;
            Focus = null;
        }

        /// <summary>확신이 가득 차 증상을 찾아냈으면 그 증상을 돌려줍니다.</summary>
        public CareConditionState Scan(Vector2 stagePoint, float travel, float deltaSeconds,
            IReadOnlyList<CareConditionState> conditions)
        {
            if (Exhausted || conditions == null) { Heat = InspectHeat.None; return null; }

            var nearest = Nearest(stagePoint, conditions, out var distance);
            if (nearest == null) { Focus = null; Heat = InspectHeat.None; Confidence = 0f; return null; }

            // Moving the magnifier is what costs effort; resting on a spot is free.
            Stamina = Mathf.Max(0f, Stamina - travel * staminaPerUnit);

            if (!ReferenceEquals(nearest, Focus)) { Focus = nearest; Confidence = 0f; }
            Heat = HeatFor(distance);

            if (Heat == InspectHeat.Hot)
            {
                Confidence = Mathf.Clamp01(Confidence + confidencePerSecond * deltaSeconds);
                if (Confidence < 1f) return null;
                Confidence = 0f;
                var found = Focus;
                Focus = null;
                Heat = InspectHeat.None;
                return found;
            }

            Confidence = Mathf.Max(0f, Confidence - confidenceDecayPerSecond * deltaSeconds);
            return null;
        }

        private InspectHeat HeatFor(float distance)
        {
            if (distance <= hotRadius) return InspectHeat.Hot;
            if (distance <= warmRadius * .6f) return InspectHeat.Warm;
            if (distance <= warmRadius) return InspectHeat.Cool;
            return InspectHeat.Cold;
        }

        private static CareConditionState Nearest(Vector2 point,
            IReadOnlyList<CareConditionState> conditions, out float distance)
        {
            CareConditionState best = null;
            distance = float.MaxValue;
            for (var i = 0; i < conditions.Count; i++)
            {
                var condition = conditions[i];
                if (condition.IsDiscovered || condition.Resolved) continue;
                var zone = condition.Zone;
                // Zone y is measured from the top of the stage; the pointer is too.
                var centre = new Vector2(zone.x + zone.width * .5f, zone.y + zone.height * .5f);
                var gap = Vector2.Distance(point, centre);
                if (gap >= distance) continue;
                distance = gap;
                best = condition;
            }
            return best;
        }

        public static string HeatLabel(InspectHeat heat) => heat switch
        {
            InspectHeat.Hot => "여기예요! 잠시 이대로",
            InspectHeat.Warm => "이 근처가 이상해요",
            InspectHeat.Cool => "조금 더 가까이",
            InspectHeat.Cold => "이 부근은 괜찮아 보여요",
            _ => "펫을 천천히 훑어보세요"
        };
    }
}
