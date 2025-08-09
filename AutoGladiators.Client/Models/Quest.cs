namespace AutoGladiators.Client.Models
{
    public class Quest
    {
        public string Id { get; set; } // Unique identifier for the quest
        public string Title { get; set; } // Title of the quest
        public string Description { get; set; } // Detailed description of the quest
        public string Reward { get; set; } // Reward for completing the quest
        public bool IsCompleted { get; set; } // Status of the quest
        public int ExperiencePoints { get; set; } // Experience points awarded for completion

        public Quest(string id, string title, string description, string reward, int experiencePoints)
        {
            Id = id;
            Title = title;
            Description = description;
            Reward = reward;
            ExperiencePoints = experiencePoints;
            IsCompleted = false;
        }
    }
}