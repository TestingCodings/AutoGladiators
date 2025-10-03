using System;
using Microsoft.Maui.Controls;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Core;

namespace AutoGladiators.Client.Pages
{
    public sealed class MainMenuPage : ContentPage
    {
        public MainMenuPage()
        {
            Title = "AutoGladiators";

            var newGame = new Button { Text = "New Game" };
            newGame.Clicked += async (_, __) =>
            {
                try
                {
                    await Navigation.PushAsync(new CreateCharacterPage());
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to start new game: {ex.Message}", "OK");
                }
            };

            var continueBtn = new Button { Text = "Continue" };
            continueBtn.Clicked += async (_, __) =>
            {
                try
                {
                    // Just ask the state machine to go exploring; if save/loading is needed, do it first.
                    await GameLoop.GoToAsync(GameStateId.Exploring, new StateArgs { Reason = "Continue" });
                    await DisplayAlert("Continue", "Resuming adventure…", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to continue game: {ex.Message}", "OK");
                }
            };

            var debugBtn = new Button { Text = "Debug Menu" };
            debugBtn.Clicked += async (_, __) =>
            {
                try
                {
                    await Navigation.PushAsync(new DebugMenuPage());
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to open debug menu: {ex.Message}", "OK");
                }
            };

            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 16,
                Children =
                {
                    new Label { Text = "AUTOGLADIATORS", FontSize = 24, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center },
                    newGame,
                    continueBtn,
                    debugBtn
                }
            };
        }
    }
}
