using System.Collections.Generic;

namespace AutoGladiators.Client.Models
{
    public class DialogueNode
    {
        public string Id { get; set; } = string.Empty;
        public string NPCName { get; set; } = "NPC";
        public string Text { get; set; } = string.Empty;
        public List<DialogueOption> Options { get; set; } = new();
        public bool IsEndNode { get; set; } = false;

        public DialogueNode() { }

        public DialogueNode(string id, string text, List<DialogueOption> options, bool isEnd = false)
        {
            Id = id;
            Text = text;
            Options = options;
            IsEndNode = isEnd;
        }
    }
}
