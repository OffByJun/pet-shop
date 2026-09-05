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
        private Camera lastCamera;

        /// <summary>스테이지의 픽셀 크기입니다. 훑은 거리를 비율로 바꿀 때 씁니다.</summary>
        public Vector2 StageSize => ((RectTransform)transform).rect.size;

        /// <summary>화면 좌표를 스테이지 비율(왼쪽 위가 0,0)로 바꿉니다.</summary>
        public bool TryStageNormalized(Vector2 screenPosition, out Vector2 normalized)
        {
            normalized = default;
            var rect = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect, screenPosition, lastCamera, out var local)) return false;
            var size = rect.rect.size;
            if (size.x <= 0f || size.y <= 0f) return false;
            normalized = new Vector2((local.x - rect.rect.xMin) / size.x,
                                     1f - (local.y - rect.rect.yMin) / size.y);
            return true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            lastCamera = eventData.pressEventCamera;
            TryInspect(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            lastCamera = eventData.pressEventCamera;
            var distance = eventData.delta.magnitude;
            if (distance >= 1f) TryStroke(eventData, distance);
        }

        private void TryInspect(PointerEventData eventData)
        {
            for (var i = 0; i < conditionTargets.Length; i++)
            {
                var target = conditionTargets[i];
                if (target != null && target.gameObject.activeInHierarchy &&
                    RectTransformUtility.RectangleContainsScreenPoint(target, eventData.position, eventData.pressEventCamera))
                {
                    GamePipe.Publish(new CareInputRequest(this, CareInput.Inspect, i, 0f, eventData.position));
                    return;
                }
            }

            GamePipe.Publish(new CareInputRequest(this, CareInput.Inspect, -1, 0f, eventData.position));
        }

        private void TryStroke(PointerEventData eventData, float distance)
        {
            for (var i = 0; i < conditionTargets.Length; i++)
            {
                var target = conditionTargets[i];
                if (target != null && target.gameObject.activeInHierarchy &&
                    RectTransformUtility.RectangleContainsScreenPoint(target, eventData.position, eventData.pressEventCamera))
                {
                    GamePipe.Publish(new CareInputRequest(this, CareInput.Stroke, i, distance, eventData.position));
                    return;
                }
            }

            // Away from any marker the drag is a search sweep, not a treatment.
            GamePipe.Publish(new CareInputRequest(this, CareInput.Stroke, -1, distance, eventData.position));
        }

        public void Configure(RectTransform[] targets) => conditionTargets = targets;
    }
}
