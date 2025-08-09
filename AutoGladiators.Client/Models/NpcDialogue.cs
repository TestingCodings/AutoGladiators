namespace AutoGladiators.Client.Models
{
    public class NPCDialogue
    {
        public string Speaker { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public List<string> Options { get; set; }
        public Dictionary<string, string> Responses { get; set; } // Option -> Response text

        public NPCDialogue()
        {
            Options = new List<string>();
            Responses = new Dictionary<string, string>();
        }
        public NPCDialogue Clone()
        {
            return new NPCDialogue
            {
                Speaker = this.Speaker,
                Text = this.Text,
                Options = new List<string>(this.Options),
                Responses = new Dictionary<string, string>(this.Responses)
            };
        }
        public bool IsEqual(NPCDialogue? other)
        {
            if (other == null) return false;
            return this.Speaker == other.Speaker &&
                   this.Text == other.Text &&
                   this.Options.SequenceEqual(other.Options) &&
                   this.Responses.SequenceEqual(other.Responses);
        }
        public override bool Equals(object obj)
        {
            if (obj is NPCDialogue other)
            {
                return IsEqual(other);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Speaker, Text, Options, Responses);
        }
        public static bool operator ==(NPCDialogue left, NPCDialogue right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }
        public static bool operator !=(NPCDialogue left, NPCDialogue right)
        {
            return !(left == right);
        }
        public static NPCDialogue operator +(NPCDialogue NPCDialogue, (string option, string response) choice)
        {
            NPCDialogue.Options.Add(choice.option);
            NPCDialogue.Responses[choice.option] = choice.response;
            return NPCDialogue;
        }
        public static NPCDialogue operator -(NPCDialogue NPCDialogue, string option)
        {
            NPCDialogue.Options.Remove(option);
            NPCDialogue.Responses.Remove(option);
            return NPCDialogue;
        }
    }
}


