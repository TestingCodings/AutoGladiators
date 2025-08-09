using System;
using Microsoft.Maui.Controls;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Pages
{
    public sealed class MainMenuPage : ContentPage
    {
        public MainMenuPage()
        {
            Title = "AutoGladiators";

            var newGame = new Button { Text = "New Game" };
            newGame.Clicked += async (_, __) => await Navigation.PushAsync(new CreateCharacterPage());

            var continueBtn = new Button { Text = "Continue" };
            continueBtn.Clicked += async (_, __) =>
            {
                // Just ask the state machine to go exploring; if save/loading is needed, do it first.
                await GameLoop.GoToAsync(GameStateId.Exploring, new StateArgs { Reason = "Continue" });
                await DisplayAlert("Continue", "Resuming adventure…", "OK");
            };

            var debugBtn = new Button { Text = "Debug Menu" };
            debugBtn.Clicked += async (_, __) => await Navigation.PushAsync(new DebugMenuPage());

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
