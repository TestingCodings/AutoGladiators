using AutoGladiators.Core.Models;

namespace AutoGladiators.Core.Models
{
    public static class DialogueCompatExtensions
    {
        // Use whatever your model actually has: Action, ActionId, OnSelect, etc.
        public static string? GetActionKey(this DialogueOption option)
        {
            // Try common property names you might already have
            return option.GetType().GetProperty("ActionKey")?.GetValue(option) as string
                ?? option.GetType().GetProperty("Action")?.GetValue(option) as string
                ?? option.GetType().GetProperty("ActionId")?.GetValue(option) as string
                ?? option.GetType().GetProperty("OnSelect")?.GetValue(option) as string;
        }
    }
}
