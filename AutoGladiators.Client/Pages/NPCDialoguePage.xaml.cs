using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace AutoGladiators.Client.Pages
{
    public partial class NPCDialoguePage : ContentPage
    {
        private List<string> dialogueLines = new()
        {
            "Hey there, traveler! Welcome to the city of GigaCore.",
            "Here, bots aren't just machines—they're champions.",
            "Train yours well, and maybe you’ll enter the Grand Circuit.",
            "Remember, the bond with your bot is just as important as its strength."
        };

        private int currentLineIndex = 0;

        public NPCDialoguePage()
        {
            InitializeComponent();
            DisplayNextLine();
        }

        private void DisplayNextLine()
        {
            if (currentLineIndex < dialogueLines.Count)
            {
                NPCTextLabel.Text = dialogueLines[currentLineIndex];
                currentLineIndex++;
            }
            else
            {
                NPCTextLabel.Text = "That's all I have to say. Good luck!";
            }
        }

        private void OnNextDialogueClicked(object sender, System.EventArgs e)
        {
            DisplayNextLine();
        }

        private async void OnEndConversationClicked(object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
