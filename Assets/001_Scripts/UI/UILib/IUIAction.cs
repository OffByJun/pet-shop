using System.Threading;
using System.Threading.Tasks;

namespace _001_Scripts.UI.UILib
{
    public interface IUIAction
    {
        int Order { get; }

        bool RunsAt(UIActionTiming timing);

        Task ExecuteAsync(UIActionContext context, CancellationToken cancellationToken);
    }
}
