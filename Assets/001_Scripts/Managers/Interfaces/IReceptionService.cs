using _001_Scripts.Core.Services;
using _001_Scripts.Data.Customers;

namespace _001_Scripts.Managers.Interfaces
{
    /// <summary>접수대의 진행 단계입니다. 구현이 아니라 계약 쪽에 두어야 화면이 매니저를 몰라도 됩니다.</summary>
    public enum ReceptionFlow { Arriving, Talking, Leaving }

    /// <summary>손님 접수와 대화 흐름입니다.</summary>
    public interface IReceptionService : IService
    {
        ServiceOrder CurrentOrder { get; }
        ReceptionFlow State { get; }
        float StateProgress { get; }
        bool CanInteract { get; }

        void Ask(int index);
        void Accept();
        void Reject();
        void SkipToNext();
        void EnterCare();
    }
}
