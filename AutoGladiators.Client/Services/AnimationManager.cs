using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Animation = Microsoft.Maui.Controls.Animation;

namespace AutoGladiators.Client.Services
{
    /// <summary>
    /// Provides smooth animations and visual effects for the game UI
    /// Handles page transitions, bot interactions, and battle effects
    /// </summary>
    public class AnimationManager
    {
        private readonly IDispatcher _dispatcher;
        
        public AnimationManager(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        #region Page Transitions

        /// <summary>
        /// Animates page entry with slide and fade effect
        /// </summary>
        public async Task AnimatePageEntry(VisualElement element)
        {
            element.Opacity = 0;
            element.TranslationX = 50;
            element.Scale = 0.9;

            var fadeIn = element.FadeTo(1, 300, Easing.CubicOut);
            var slideIn = element.TranslateTo(0, 0, 300, Easing.CubicOut);
            var scaleIn = element.ScaleTo(1, 300, Easing.CubicOut);

            await Task.WhenAll(fadeIn, slideIn, scaleIn);
        }

        /// <summary>
        /// Animates page exit with slide and fade effect
        /// </summary>
        public async Task AnimatePageExit(VisualElement element)
        {
            var fadeOut = element.FadeTo(0, 200, Easing.CubicIn);
            var slideOut = element.TranslateTo(-50, 0, 200, Easing.CubicIn);
            var scaleOut = element.ScaleTo(0.9, 200, Easing.CubicIn);

            await Task.WhenAll(fadeOut, slideOut, scaleOut);
        }

        #endregion

        #region Bot Interactions

        /// <summary>
        /// Animates bot card selection with pulse effect
        /// </summary>
        public async Task AnimateBotCardTap(VisualElement card)
        {
            var scaleDown = card.ScaleTo(0.95, 100, Easing.CubicIn);
            await scaleDown;
            
            var scaleUp = card.ScaleTo(1.05, 150, Easing.BounceOut);
            await scaleUp;
            
            var scaleNormal = card.ScaleTo(1, 100, Easing.CubicOut);
            await scaleNormal;
        }

        /// <summary>
        /// Animates bot level up with celebration effect
        /// </summary>
        public async Task AnimateBotLevelUp(VisualElement botCard)
        {
            // Create glow effect
            await AnimateGlow(botCard, Color.FromArgb("#FFD700"), 500);
            
            // Bounce animation
            var bounce = botCard.ScaleTo(1.2, 200, Easing.BounceOut);
            await bounce;
            
            var normalize = botCard.ScaleTo(1, 200, Easing.CubicOut);
            await normalize;
        }

        /// <summary>
        /// Animates bot collection discovery with sparkle effect
        /// </summary>
        public async Task AnimateBotDiscovery(VisualElement element)
        {
            element.Opacity = 0;
            element.Scale = 0;
            element.Rotation = -180;

            var fadeIn = element.FadeTo(1, 400, Easing.BounceOut);
            var scaleIn = element.ScaleTo(1, 400, Easing.BounceOut);
            var rotateIn = element.RotateTo(0, 400, Easing.BounceOut);

            await Task.WhenAll(fadeIn, scaleIn, rotateIn);
        }

        #endregion

        #region Battle Effects

        /// <summary>
        /// Animates damage effect with screen shake and flash
        /// </summary>
        public async Task AnimateDamageEffect(VisualElement target)
        {
            // Flash red
            var originalColor = target.BackgroundColor;
            target.BackgroundColor = Color.FromArgb("#FF4444");
            
            // Shake animation
            await AnimateShake(target);
            
            // Return to original color
            target.BackgroundColor = originalColor;
        }

        /// <summary>
        /// Animates healing effect with green glow
        /// </summary>
        public async Task AnimateHealEffect(VisualElement target)
        {
            await AnimateGlow(target, Color.FromArgb("#00FF88"), 300);
        }

        /// <summary>
        /// Animates shield effect with blue pulse
        /// </summary>
        public async Task AnimateShieldEffect(VisualElement target)
        {
            await AnimatePulse(target, Color.FromArgb("#4A9EFF"), 400);
        }

        #endregion

        #region UI Feedback

        /// <summary>
        /// Animates button press with scale and color feedback
        /// </summary>
        public async Task AnimateButtonPress(Button button)
        {
            var originalColor = button.BackgroundColor;
            var pressedColor = Color.FromArgb("#3A3A4E");
            
            button.BackgroundColor = pressedColor;
            var scaleDown = button.ScaleTo(0.95, 50, Easing.Linear);
            await scaleDown;
            
            button.BackgroundColor = originalColor;
            var scaleUp = button.ScaleTo(1, 100, Easing.CubicOut);
            await scaleUp;
        }

        /// <summary>
        /// Animates progress bar fill with smooth easing
        /// </summary>
        public async Task AnimateProgressBar(ProgressBar progressBar, double targetProgress, uint duration = 500)
        {
            var animation = new Animation(v => progressBar.Progress = v, progressBar.Progress, targetProgress);
            progressBar.Animate("ProgressAnimation", animation, 16, duration, Easing.CubicOut);
            
            await Task.Delay((int)duration);
        }

        /// <summary>
        /// Animates stat increase with number count-up effect
        /// </summary>
        public async Task AnimateStatIncrease(Label label, int fromValue, int toValue, uint duration = 800)
        {
            var animation = new Animation(v => 
            {
                _dispatcher.Dispatch(() => 
                {
                    label.Text = ((int)v).ToString();
                });
            }, fromValue, toValue);
            
            label.Animate("StatCountUp", animation, 16, duration, Easing.CubicOut);
            await Task.Delay((int)duration);
        }

        #endregion

        #region Loading Effects

        /// <summary>
        /// Creates a subtle loading pulse animation
        /// </summary>
        public async Task StartLoadingPulse(VisualElement element)
        {
            while (element != null && element.IsVisible)
            {
                await element.FadeTo(0.5, 800, Easing.SinInOut);
                await element.FadeTo(1, 800, Easing.SinInOut);
            }
        }

        /// <summary>
        /// Stops loading animation and resets element
        /// </summary>
        public void StopLoadingPulse(VisualElement element)
        {
            element?.AbortAnimation("Pulse");
            element.Opacity = 1;
        }

        #endregion

        #region Helper Animations

        /// <summary>
        /// Creates a glow effect by animating background color
        /// </summary>
        private async Task AnimateGlow(VisualElement element, Color glowColor, uint duration)
        {
            var originalColor = element.BackgroundColor;
            
            var glowIn = element.ColorTo(glowColor, duration / 2, Easing.CubicOut);
            await glowIn;
            
            var glowOut = element.ColorTo(originalColor, duration / 2, Easing.CubicIn);
            await glowOut;
        }

        /// <summary>
        /// Creates a shake effect for impact feedback
        /// </summary>
        private async Task AnimateShake(VisualElement element)
        {
            var shake1 = element.TranslateTo(-5, 0, 50, Easing.Linear);
            await shake1;
            
            var shake2 = element.TranslateTo(5, 0, 50, Easing.Linear);
            await shake2;
            
            var shake3 = element.TranslateTo(-3, 0, 50, Easing.Linear);
            await shake3;
            
            var reset = element.TranslateTo(0, 0, 50, Easing.Linear);
            await reset;
        }

        /// <summary>
        /// Creates a pulse effect for status indicators
        /// </summary>
        private async Task AnimatePulse(VisualElement element, Color pulseColor, uint duration)
        {
            var originalColor = element.BackgroundColor;
            var originalScale = element.Scale;
            
            var scaleUp = element.ScaleTo(originalScale * 1.1, duration / 2, Easing.CubicOut);
            var colorChange = element.ColorTo(pulseColor, duration / 2, Easing.CubicOut);
            await Task.WhenAll(scaleUp, colorChange);
            
            var scaleDown = element.ScaleTo(originalScale, duration / 2, Easing.CubicIn);
            var colorRevert = element.ColorTo(originalColor, duration / 2, Easing.CubicIn);
            await Task.WhenAll(scaleDown, colorRevert);
        }
    }

    /// <summary>
    /// Extension methods for color animations
    /// </summary>
    public static class ColorAnimationExtensions
    {
        public static Task<bool> ColorTo(this VisualElement element, Color targetColor, uint duration, Easing easing = null)
        {
            var originalColor = element.BackgroundColor;
            var taskCompletionSource = new TaskCompletionSource<bool>();
            
            var animation = new Animation(v =>
            {
                var r = originalColor.Red + (targetColor.Red - originalColor.Red) * v;
                var g = originalColor.Green + (targetColor.Green - originalColor.Green) * v;
                var b = originalColor.Blue + (targetColor.Blue - originalColor.Blue) * v;
                var a = originalColor.Alpha + (targetColor.Alpha - originalColor.Alpha) * v;
                
                element.BackgroundColor = Color.FromRgba(r, g, b, a);
            });
            
            animation.Commit(element, "ColorAnimation", 16, duration, easing, (v, c) =>
            {
                taskCompletionSource.SetResult(c);
            });
            
            return taskCompletionSource.Task;
        }

        #endregion
    }
}