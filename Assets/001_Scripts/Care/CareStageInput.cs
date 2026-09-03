using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PetShop.Care
{
    /// <summary>uGUI pointer adapter for the editable care stage.</summary>
    public sealed class CareStageInput : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField] private RectTransform[] conditionTargets;

        public event Action<int, float> StrokeRequested;

        public void OnPointerDown(PointerEventData eventData) => TryStroke(eventData, 0f);

        public void OnDrag(PointerEventData eventData)
        {
            var distance = eventData.delta.magnitude;
            if (distance >= 1f) TryStroke(eventData, distance);
        }

        private void TryStroke(PointerEventData eventData, float distance)
        {
            for (var i = 0; i < conditionTargets.Length; i++)
            {
                var target = conditionTargets[i];
                if (target != null && target.gameObject.activeInHierarchy &&
                    RectTransformUtility.RectangleContainsScreenPoint(target, eventData.position, eventData.pressEventCamera))
                {
                    StrokeRequested?.Invoke(i, distance);
                    return;
                }
            }
        }

        public void Configure(RectTransform[] targets) => conditionTargets = targets;
    }
}
