
namespace AutoGladiators.Client.Models
{
    public class Dialogue
    {
        public string Speaker { get; set; }
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public Dictionary<string, string> Responses { get; set; } // Option -> Response text

        public Dialogue()
        {
            Options = new List<string>();
            Responses = new Dictionary<string, string>();
        }
        public Dialogue Clone()
        {
            return new Dialogue
            {
                Speaker = this.Speaker,
                Text = this.Text,
                Options = new List<string>(this.Options),
                Responses = new Dictionary<string, string>(this.Responses)
            };
        }
        public bool IsEqual(Dialogue? other)
        {
            if (other == null) return false;
            return this.Speaker == other.Speaker &&
                   this.Text == other.Text &&
                   this.Options.SequenceEqual(other.Options) &&
                   this.Responses.SequenceEqual(other.Responses);
        }
        public override bool Equals(object obj)
        {
            if (obj is Dialogue other)
            {
                return IsEqual(other);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Speaker, Text, Options, Responses);
        }
        public static bool operator ==(Dialogue left, Dialogue right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }
        public static bool operator !=(Dialogue left, Dialogue right)
        {
            return !(left == right);
        }
        public static Dialogue operator +(Dialogue dialogue, (string option, string response) choice)
        {
            dialogue.Options.Add(choice.option);
            dialogue.Responses[choice.option] = choice.response;
            return dialogue;
        }
        public static Dialogue operator -(Dialogue dialogue, string option)
        {
            dialogue.Options.Remove(option);
            dialogue.Responses.Remove(option);
            return dialogue;
        }
    }
}


