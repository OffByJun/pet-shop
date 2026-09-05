using _001_Scripts.Core;
using _001_Scripts.UI.UILib;
using UnityEngine;
using UnityEngine.UI;

namespace _001_Scripts.UI.Components
{
    /// <summary>부위 스프라이트로 조립된 펫입니다. 상태가 해결되면 해당 과성장 파츠를 숨깁니다.</summary>
    public sealed class PetLayeredVisual : GameBehaviour
    {
        [System.Serializable]
        private struct ConditionParts
        {
            [Tooltip("CareConditionState.Id 목록입니다. 하나라도 남아 있으면 파츠가 보입니다. " +
                     "기본 케어와 루틴 케어가 서로 다른 id를 쓰므로 동의어를 함께 적습니다.")]
            public string[] ConditionIds;
            public GameObject[] Parts;
        }

        [SerializeField] private Image face;
        [SerializeField] private Sprite calmFace;
        [SerializeField] private Sprite troubledFace;
        [SerializeField] private ConditionParts[] conditionParts = new ConditionParts[0];

        public void Render(CareViewModel model)
        {
            if (model == null) return;
            for (var i = 0; i < conditionParts.Length; i++)
            {
                var parts = conditionParts[i].Parts;
                if (parts == null) continue;
                var visible = HasUnresolved(model, conditionParts[i].ConditionIds);
                for (var p = 0; p < parts.Length; p++)
                    if (parts[p] != null) parts[p].SetActive(visible);
            }

            if (face == null) return;
            var next = model.Completed ? calmFace : troubledFace;
            if (next != null) face.sprite = next;
        }

        private static bool HasUnresolved(CareViewModel model, string[] conditionIds)
        {
            if (conditionIds == null) return false;
            for (var i = 0; i < model.Conditions.Count; i++)
            {
                var condition = model.Conditions[i];
                if (condition.Resolved) continue;
                for (var id = 0; id < conditionIds.Length; id++)
                    if (condition.Id == conditionIds[id]) return true;
            }
            return false;
        }
    }
}
