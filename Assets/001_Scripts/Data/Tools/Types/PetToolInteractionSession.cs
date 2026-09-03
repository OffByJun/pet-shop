using System;
using _001_Scripts.Core.Entity;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Pets;
using UnityEngine;

namespace _001_Scripts.Data.Tools
{
    /// <summary>한 도구가 한 요청 상태를 처리하는 동안의 직접 조작 진행도입니다.</summary>
    public sealed class PetToolInteractionSession
    {
        public PetToolDefinition Tool { get; }
        public ServiceOrder Order { get; }
        public PetInstance Pet { get; }
        public ServiceRequestState Request { get; }
        public float Progress { get; private set; }
        public bool IsCancelled { get; private set; }
        public bool IsCommitted { get; private set; }
        public bool IsReadyToComplete => !IsCancelled && !IsCommitted && Progress >= 1f;

        internal PetToolInteractionSession(
            PetToolDefinition tool, ServiceOrder order, PetInstance pet, ServiceRequestState request)
        {
            Tool = tool ?? throw new ArgumentNullException(nameof(tool));
            Order = order ?? throw new ArgumentNullException(nameof(order));
            Pet = pet ?? throw new ArgumentNullException(nameof(pet));
            Request = request ?? throw new ArgumentNullException(nameof(request));
            if (request.Condition.InteractionMode == PetToolInteractionMode.Instant) Progress = 1f;
        }

        public bool ApplyInput(PetToolInteractionMode inputMode, float normalizedAmount)
        {
            if (IsCancelled || IsCommitted || Progress >= 1f || normalizedAmount <= 0f) return false;
            if (Request.Condition.InteractionMode != inputMode || inputMode == PetToolInteractionMode.Instant) return false;
            Progress = Mathf.Clamp01(Progress + normalizedAmount);
            return true;
        }

        public void Cancel()
        {
            if (!IsCommitted) IsCancelled = true;
        }

        internal void Commit() => IsCommitted = true;
    }
}
