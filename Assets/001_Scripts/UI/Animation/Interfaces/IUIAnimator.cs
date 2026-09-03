using System.Threading;
using System.Threading.Tasks;

namespace _001_Scripts.UI.UILib
{
    public interface IUIAnimator
    {
        Task PlayAsync(UIAnimationContext context, CancellationToken cancellationToken);

        void ApplyInstant(UIAnimationContext context);
    }
}
