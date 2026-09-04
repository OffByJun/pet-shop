using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.Core.Services;
using _001_Scripts.UI.Components;

namespace _001_Scripts.UI.UILib
{
    /// <summary>UI 전환 계약입니다. 호출자는 UIManager가 아니라 이 계약을 봅니다.</summary>
    public interface IUIService : IService
    {
        bool Register(UIComponent component);

        bool Unregister(UIComponent component);

        bool TryGet(string serviceId, out UIComponent component);

        Task ShowAsync(string serviceId, CancellationToken cancellationToken = default);

        Task HideAsync(string serviceId, CancellationToken cancellationToken = default);

        void Cancel(string serviceId);

        void SetInstant(string serviceId, bool visible);
    }
}
