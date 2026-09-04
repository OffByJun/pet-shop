using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.Core.Composition;

namespace _001_Scripts.UI.UILib
{
    public interface IUIAnimator : IModule
    {
        Task PlayAsync(UIAnimationContext context, CancellationToken cancellationToken);

        void ApplyInstant(UIAnimationContext context);
    }
}
