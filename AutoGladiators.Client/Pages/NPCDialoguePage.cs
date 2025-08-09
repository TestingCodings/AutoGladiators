using System;
using System.Linq;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.ViewModels;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;
using Microsoft.Maui.Controls;

namespace AutoGladiators.Client.Pages
{
    public sealed class NPCDialoguePage : ContentPage
    {
        private readonly Label _nameLabel;
        private readonly Label _textLabel;
        private readonly StackLayout _optionsLayout;
        private readonly NPCDialogueViewModel _vm;

        public NPCDialoguePage(string dialogueFile)
        {
            Title = "Dialogue";

            _nameLabel = new Label
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 8)
            };

            _textLabel = new Label
            {
                FontSize = 16,
                Margin = new Thickness(12, 8)
            };

            _optionsLayout = new StackLayout
            {
                Spacing = 8,
                Padding = new Thickness(12, 8)
            };

            Content = new StackLayout
            {
                Children = { _nameLabel, _textLabel, _optionsLayout }
            };

            _vm = new NPCDialogueViewModel("NPC");
            BindingContext = _vm;

            Appearing += async (_, __) =>
            {
                await _vm.InitializeFromFile(dialogueFile);
                UpdateUI();
            };
        }

        private void UpdateUI()
        {
            _nameLabel.Text = _vm.NPCName;
            _textLabel.Text = _vm.CurrentText;
            RenderOptions();
        }

        private void RenderOptions()
        {
            _optionsLayout.Children.Clear();
            if (_vm.CurrentOptions == null || !_vm.CurrentOptions.Any())
                return;

            foreach (var opt in _vm.CurrentOptions)
            {
                var btn = new Button { Text = opt.Text };
                btn.Clicked += (_, __) =>
                {
                    var cmd = _vm.SelectOptionCommand;
                    if (cmd?.CanExecute(opt) == true)
                        cmd.Execute(opt);

                    UpdateUI();
                };
                _optionsLayout.Children.Add(btn);
            }
        }
    }
}
