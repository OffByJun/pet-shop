using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.UI.Components;

namespace _001_Scripts.UI.UILib
{
    public interface IUIService
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
