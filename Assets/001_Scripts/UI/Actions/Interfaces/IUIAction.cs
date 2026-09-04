using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.Core.Composition;

namespace _001_Scripts.UI.UILib
{
    public interface IUIAction : IOrderedModule
    {
        bool RunsAt(UIActionTiming timing);

        Task ExecuteAsync(UIActionContext context, CancellationToken cancellationToken);
    }
}
