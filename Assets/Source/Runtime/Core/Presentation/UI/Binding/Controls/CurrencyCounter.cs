using LitMotion;
using R3;
using UnityEngine.Localization.Settings;

namespace Core.UI.Controls
{
    public static class CurrencyCounter
    {
        private const float CountSeconds = 0.6f;

        public static void BindCounter(this BaseText text, ReadOnlyReactiveProperty<int> amount, ref DisposableBag disposables)
        {
            var shown = amount.CurrentValue;
            var motion = default(MotionHandle);
            text.SetText(Format(shown));
            amount.Skip(1)
                .Subscribe(value =>
                {
                    Cancel(motion);
                    motion = LMotion.Create(shown, value, CountSeconds)
                        .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                        .WithEase(Ease.OutQuad)
                        .Bind(current =>
                        {
                            shown = current;
                            text.SetText(Format(current));
                        });
                })
                .AddTo(ref disposables);
            Disposable.Create(() => Cancel(motion)).AddTo(ref disposables);
        }

        private static void Cancel(MotionHandle motion)
        {
            if (motion.IsActive())
            {
                motion.Cancel();
            }
        }

        private static string Format(int value) => value.ToString("N0", LocalizationSettings.SelectedLocale.Formatter);
    }
}
