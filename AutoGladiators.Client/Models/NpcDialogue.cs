
namespace AutoGladiators.Client.Models
{
    public class NpcDialogue
    {
        public string Speaker { get; set; }
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public Dictionary<string, string> Responses { get; set; } // Option -> Response text

        public NpcDialogue()
        {
            Options = new List<string>();
            Responses = new Dictionary<string, string>();
        }
        public NpcDialogue Clone()
        {
            return new NpcDialogue
            {
                Speaker = this.Speaker,
                Text = this.Text,
                Options = new List<string>(this.Options),
                Responses = new Dictionary<string, string>(this.Responses)
            };
        }
        public bool IsEqual(NpcDialogue? other)
        {
            if (other == null) return false;
            return this.Speaker == other.Speaker &&
                   this.Text == other.Text &&
                   this.Options.SequenceEqual(other.Options) &&
                   this.Responses.SequenceEqual(other.Responses);
        }
        public override bool Equals(object obj)
        {
            if (obj is NpcDialogue other)
            {
                return IsEqual(other);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Speaker, Text, Options, Responses);
        }
        public static bool operator ==(NpcDialogue left, NpcDialogue right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }
        public static bool operator !=(NpcDialogue left, NpcDialogue right)
        {
            return !(left == right);
        }
        public static NpcDialogue operator +(NpcDialogue NpcDialogue, (string option, string response) choice)
        {
            NpcDialogue.Options.Add(choice.option);
            NpcDialogue.Responses[choice.option] = choice.response;
            return NpcDialogue;
        }
        public static NpcDialogue operator -(NpcDialogue NpcDialogue, string option)
        {
            NpcDialogue.Options.Remove(option);
            NpcDialogue.Responses.Remove(option);
            return NpcDialogue;
        }
    }
}


