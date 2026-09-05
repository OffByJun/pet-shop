using _001_Scripts.Data.Customers;
using UnityEngine;

namespace _001_Scripts.Core.Entity
{
    /// <summary>문구는 ReceptionDialogueTable이 갖고, 이 클래스는 토큰만 채웁니다.</summary>
    public sealed class DefaultReceptionDialogueComposer
    {
        private readonly ReceptionDialogueTable table;

        public DefaultReceptionDialogueComposer(ReceptionDialogueTable table = null) =>
            this.table = table == null ? ScriptableObject.CreateInstance<ReceptionDialogueTable>() : table;

        public ReceptionDialogueTable Table => table;

        public string Greeting(ServiceOrder order, CustomerRelationship relationship = null)
        {
            var condition = order.Requests.Count == 0 ? null : order.Requests[0].Condition;
            return ReceptionDialogueTable.Fill(
                Template(order, relationship),
                pet: order.Pet.DisplayName,
                customer: order.Customer.CharacterName,
                clue: table.Clue(condition));
        }

        /// <summary>방문 전용 대사 → 지난 결과 반응 → 기본 인사 순으로 고릅니다.</summary>
        private string Template(ServiceOrder order, CustomerRelationship relationship)
        {
            var customer = order.Customer;
            if (relationship != null)
            {
                var scripted = customer.GreetingForVisit(relationship.Visits + 1);
                if (!string.IsNullOrWhiteSpace(scripted)) return scripted;
                if (relationship.Visits > 0)
                {
                    var reaction = relationship.LastResult == ServiceOrderStatus.Failed
                        ? customer.ReturningUpsetLine
                        : customer.ReturningHappyLine;
                    if (!string.IsNullOrWhiteSpace(reaction)) return reaction;
                }
            }
            return table.Greeting(customer.Archetype);
        }

        public string Question(PetConditionDefinition condition) =>
            ReceptionDialogueTable.Fill(table.Question(condition), condition: condition.DisplayName);

        public string Reply(PetConditionDefinition condition) =>
            ReceptionDialogueTable.Fill(table.Reply(condition),
                condition: condition.DisplayName,
                action: table.ActionLabel(condition.ResolvedBy));
    }
}
