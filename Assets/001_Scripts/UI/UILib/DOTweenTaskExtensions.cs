using System;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;

namespace _001_Scripts.UI.UILib
{
    public static class DOTweenTaskExtensions
    {
        /// <summary>
        /// Awaits a tween and kills it when the supplied token is cancelled.
        /// DOTween operations are marshalled back to Unity's synchronization context.
        /// </summary>
        public static Task AwaitCompletionAsync(this Tween tween, CancellationToken cancellationToken)
        {
            if (tween == null)
            {
                return Task.CompletedTask;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                tween.Kill(false);
                return Task.FromCanceled(cancellationToken);
            }

            var completion = new TaskCompletionSource<bool>();
            SynchronizationContext unityContext = SynchronizationContext.Current;
            CancellationTokenRegistration registration = default;
            bool completed = false;

            tween.OnComplete(() =>
            {
                completed = true;
                registration.Dispose();
                completion.TrySetResult(true);
            });

            tween.OnKill(() =>
            {
                registration.Dispose();
                if (!completed)
                {
                    CancellationToken token = cancellationToken.IsCancellationRequested
                        ? cancellationToken
                        : new CancellationToken(true);
                    completion.TrySetCanceled(token);
                }
            });

            registration = cancellationToken.Register(() =>
            {
                void KillTween()
                {
                    if (tween.IsActive())
                    {
                        tween.Kill(false);
                    }
                    else
                    {
                        completion.TrySetCanceled(cancellationToken);
                    }
                }

                if (unityContext != null && SynchronizationContext.Current != unityContext)
                {
                    unityContext.Post(_ => KillTween(), null);
                }
                else
                {
                    KillTween();
                }
            });

            if (completion.Task.IsCompleted)
            {
                registration.Dispose();
            }

            return completion.Task;
        }
    }
}
