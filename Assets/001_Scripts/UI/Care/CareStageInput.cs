using System;
using _001_Scripts.Core;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _001_Scripts.UI.Components
{
    /// <summary>uGUI pointer adapter for the editable care stage.</summary>
    public sealed class CareStageInput : GameBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField] private RectTransform[] conditionTargets;

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
                    GamePipe.Publish(new CareInputRequest(this, CareInput.Stroke, i, distance));
                    return;
                }
            }
        }

        public void Configure(RectTransform[] targets) => conditionTargets = targets;
    }
}
