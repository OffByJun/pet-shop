using _001_Scripts.Core.Services;
using _001_Scripts.UI.Components;
using _001_Scripts.UI.UILib;

namespace _001_Scripts.Managers.Interfaces
{
    /// <summary>케어 화면과 입력을 연결하는 계약입니다.</summary>
    public interface ICareService : IService
    {
        void Configure(CareUIComponent view, CareStageInput input);
    }
}
