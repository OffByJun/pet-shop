using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Pets;

namespace PetShop.Care
{
    /// <summary>Default dialogue policy. Alternate tone/localization can replace this policy.</summary>
    public sealed class DefaultReceptionDialogueComposer : IReceptionDialogueComposer
    {
        public string Greeting(ServiceOrder order)
        {
            var pet = order.Pet.DisplayName;
            var clue = order.Requests.Count == 0 ? "평소와 조금 달라 보여요" : Clue(order.Requests[0].Condition);
            return order.Customer.Archetype switch
            {
                CustomerArchetype.Adventurer => $"긴 여행을 마치고 왔어요. {pet}이(가) {clue}.",
                CustomerArchetype.Wizard => $"요즘 {pet}이(가) {clue}. 원인을 살펴봐 주세요.",
                CustomerArchetype.Merchant => $"깔끔한 관리가 필요해요. {pet}이(가) {clue}.",
                CustomerArchetype.Noble => $"우리 {pet}이(가) {clue}. 세심히 부탁드려요.",
                _ => $"안녕하세요. {pet}이(가) {clue}. 한번 봐주시겠어요?"
            };
        }

        public string Question(PetConditionDefinition condition) => condition.Category switch
        {
            PetConditionCategory.Contamination => "어디가 가장 더러워졌나요?",
            PetConditionCategory.Injury => "다친 곳을 자세히 보셨나요?",
            PetConditionCategory.Coat => "털 상태를 조금 더 설명해 주세요.",
            PetConditionCategory.Growth => "몸에 자란 것이 정확히 무엇인가요?",
            PetConditionCategory.ForeignObject => "붙어 있는 물체가 무엇인가요?",
            PetConditionCategory.Nails => "발톱 상태는 어떤가요?",
            _ => $"{condition.DisplayName}에 대해 더 알려주세요."
        };

        public string Reply(PetConditionDefinition condition) =>
            $"자세히 보니 ‘{condition.DisplayName}’ 상태예요. {ActionLabel(condition.ResolvedBy)} 케어가 필요해 보여요.";

        private static string Clue(PetConditionDefinition condition) => condition.Category switch
        {
            PetConditionCategory.Contamination => "몸이 무겁고 바닥에 자국을 남겨요",
            PetConditionCategory.Injury => "한쪽을 만지면 움찔해요",
            PetConditionCategory.Coat => "털이 빗자루처럼 엉켜 있어요",
            PetConditionCategory.Growth => "몸에 작은 것이 자란 것 같아요",
            PetConditionCategory.ForeignObject => "몸 한쪽이 유난히 반짝여요",
            PetConditionCategory.Nails => "걸을 때 바닥을 긁는 소리가 나요",
            PetConditionCategory.Hunger => "기운이 없어 보여요",
            PetConditionCategory.Stress => "구석에 오래 숨어 있어요",
            _ => "평소와 조금 달라 보여요"
        };

        private static string ActionLabel(PetCareAction action) => action switch
        {
            PetCareAction.Wash => "세척",
            PetCareAction.Brush => "빗질",
            PetCareAction.Treat => "치료",
            PetCareAction.Extract => "제거",
            PetCareAction.Trim => "손질",
            PetCareAction.Clip => "발톱 손질",
            PetCareAction.Feed => "급식",
            PetCareAction.Play => "놀이",
            _ => "상태에 맞는"
        };
    }
}
