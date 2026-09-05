using System.Collections.Generic;

namespace _001_Scripts.Data
{
    /// <summary>Guarantees varied, one-time care events as treatment stages are completed.</summary>
    public sealed class CareEventDirector
    {
        private const int MaximumEventsPerVisit = 4;
        private readonly HashSet<string> triggeredConditions = new HashSet<string>();

        public int TriggeredCount { get; private set; }

        public bool TryCreate(CareConditionState condition, out CareEventEncounter encounter)
        {
            encounter = null;
            if (condition == null || condition.CompletedPasses < 1 || condition.Resolved ||
                TriggeredCount >= MaximumEventsPerVisit || !triggeredConditions.Add(condition.Id)) return false;

            encounter = Create(condition, TriggeredCount++);
            return true;
        }

        public void Reset()
        {
            triggeredConditions.Clear();
            TriggeredCount = 0;
        }

        private static CareEventEncounter Create(CareConditionState condition, int sequence)
        {
            return condition.Care switch
            {
                CareKind.Wash => new CareEventEncounter($"wash_shake_{sequence}", "갑작스러운 물 털기!",
                    "차가운 물에 놀란 펫이 온몸을 털려고 합니다. 어떻게 진정시킬까요?", condition,
                    new CareEventChoice("수건으로 감싸기", "다음 단계 18% 도움",
                        "포근한 수건으로 안심시켰습니다. 다음 세척이 수월해졌어요.", .18f, 0),
                    new CareEventChoice("분사 리듬 맞추기", "FLOW 3칸 획득",
                        "펫의 호흡에 맞춰 물을 뿌렸습니다. 손의 리듬이 살아납니다.", 0f, 3)),

                CareKind.Brush => new CareEventEncounter($"brush_tangle_{sequence}", "민감한 털뭉치",
                    "단단히 엉킨 털이 당겨지자 펫이 몸을 움츠립니다.", condition,
                    new CareEventChoice("끝에서부터 풀기", "다음 단계 20% 도움",
                        "털 끝부터 천천히 풀어 피부 자극을 줄였습니다.", .20f, 0),
                    new CareEventChoice("손으로 결을 잡기", "FLOW 2칸 + 8% 도움",
                        "한 손으로 털을 받치고 빗질해 안정적인 리듬을 찾았습니다.", .08f, 2)),

                CareKind.Treat => new CareEventEncounter($"treat_reaction_{sequence}", "약품 반응 확인",
                    "상처 주변이 잠깐 붉어졌습니다. 침착하게 다음 처치를 골라야 합니다.", condition,
                    new CareEventChoice("희석해서 다시 바르기", "다음 단계 16% 도움",
                        "농도를 낮춰 자극 없이 약이 스며들게 했습니다.", .16f, 0),
                    new CareEventChoice("호흡을 보며 기다리기", "FLOW 3칸 획득",
                        "서두르지 않고 반응을 살펴 펫의 신뢰를 얻었습니다.", 0f, 3)),

                CareKind.Remove => new CareEventEncounter($"remove_glint_{sequence}", "숨은 파편의 반짝임",
                    "큰 이물질 아래에서 작은 파편 하나가 더 빛납니다.", condition,
                    new CareEventChoice("주변부터 넓게 정리", "다음 단계 22% 도움",
                        "주변을 먼저 정리해 파편의 방향이 분명해졌습니다.", .22f, 0),
                    new CareEventChoice("빛의 각도에 맞춰 집기", "FLOW 3칸 + 5% 도움",
                        "반짝임을 따라 정확한 각도로 잡았습니다.", .05f, 3)),

                CareKind.Trim => new CareEventEncounter($"trim_movement_{sequence}", "꼬리가 살랑살랑",
                    "기분이 좋아진 펫이 꼬리를 흔들어 가위질이 어려워졌습니다.", condition,
                    new CareEventChoice("간격을 두고 기다리기", "다음 단계 15% 도움",
                        "움직임이 잦아들 때까지 기다려 안전한 각도를 만들었습니다.", .15f, 0),
                    new CareEventChoice("흔들림에 맞춰 손질", "FLOW 3칸 획득",
                        "꼬리의 박자에 맞춰 능숙하게 가위를 움직였습니다.", 0f, 3)),

                _ => new CareEventEncounter($"care_bond_{sequence}", "마음을 여는 순간",
                    "펫이 조심스럽게 손길을 기다립니다.", condition,
                    new CareEventChoice("천천히 쓰다듬기", "다음 단계 15% 도움",
                        "긴장이 풀려 다음 케어가 편안해졌습니다.", .15f, 0),
                    new CareEventChoice("케어 리듬 이어가기", "FLOW 2칸 획득",
                        "집중을 유지해 좋은 흐름을 이어갑니다.", 0f, 2))
            };
        }
    }
}
