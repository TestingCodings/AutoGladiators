using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services
{
    /// <summary>
    /// Manages story progression, narrative events, and configurable storylines
    /// </summary>
    public class StoryService
    {
        private static readonly IAppLogger Log = AppLog.For<StoryService>();
        
        // Singleton instance
        public static StoryService Instance { get; private set; } = new StoryService();
        private StoryService() { }
        
        // Current story state
        public string CurrentStoryId { get; private set; } = "MainStory";
        public int CurrentChapter { get; private set; } = 1;
        public int CurrentScene { get; private set; } = 1;
        public Dictionary<string, object> StoryFlags { get; private set; } = new();
        
        // Story configuration
        private readonly Dictionary<string, Story> _availableStories = new();
        private readonly Dictionary<string, StoryChapter> _currentChapters = new();
        
        // Events for story progression
        public event Action<StoryEvent>? OnStoryEvent;
        public event Action<string, int, int>? OnChapterChanged; // storyId, oldChapter, newChapter
        public event Action<QuestObjective>? OnQuestObjectiveUpdated;
        
        #region Initialization
        
        /// <summary>
        /// Initialize the story system with default storylines
        /// </summary>
        public void Initialize()
        {
            LoadDefaultStories();
            Log.Info("Story system initialized with default storylines");
        }
        
        /// <summary>
        /// Load the default main storyline
        /// </summary>
        private void LoadDefaultStories()
        {
            // Main storyline
            var mainStory = new Story
            {
                Id = "MainStory",
                Title = "The Gladiator's Journey",
                Description = "A young trainer embarks on an epic quest to become the ultimate Gladiator Champion",
                Chapters = new List<StoryChapter>
                {
                    CreateTutorialChapter(),
                    CreateEarlyGameChapter(),
                    CreateMidGameChapter(),
                    CreateLateGameChapter(),
                    CreateEndGameChapter()
                }
            };
            
            _availableStories["MainStory"] = mainStory;
            _currentChapters["MainStory"] = mainStory.Chapters[0];
            
            // Optional side storylines
            LoadSideStories();
        }
        
        private void LoadSideStories()
        {
            // Elemental Mastery storyline
            var elementalStory = new Story
            {
                Id = "ElementalMastery",
                Title = "Master of Elements",
                Description = "Discover the secrets of elemental combat and forge legendary bonds",
                Chapters = new List<StoryChapter>
                {
                    new StoryChapter
                    {
                        Id = 1,
                        Title = "The Spark Within",
                        Description = "Learn about elemental affinities and their power",
                        Objectives = new List<QuestObjective>
                        {
                            new QuestObjective("Capture a Fire-type bot", "capture_fire", false),
                            new QuestObjective("Capture a Water-type bot", "capture_water", false),
                            new QuestObjective("Win 3 battles using type advantages", "type_advantage_wins", false)
                        }
                    }
                }
            };
            
            _availableStories["ElementalMastery"] = elementalStory;
        }
        
        #endregion
        
        #region Story Progression
        
        /// <summary>
        /// Advance to the next scene in the current chapter
        /// </summary>
        public async Task<bool> AdvanceSceneAsync()
        {
            var currentChapter = GetCurrentChapter();
            if (currentChapter == null) return false;
            
            CurrentScene++;
            
            // Trigger scene events
            var sceneEvent = new StoryEvent
            {
                Type = StoryEventType.SceneChanged,
                StoryId = CurrentStoryId,
                Chapter = CurrentChapter,
                Scene = CurrentScene,
                Timestamp = DateTime.UtcNow
            };
            
            OnStoryEvent?.Invoke(sceneEvent);
            
            // Check if chapter is complete
            if (CurrentScene > currentChapter.MaxScenes)
            {
                return await AdvanceChapterAsync();
            }
            
            return true;
        }
        
        /// <summary>
        /// Advance to the next chapter
        /// </summary>
        public async Task<bool> AdvanceChapterAsync()
        {
            var story = _availableStories.GetValueOrDefault(CurrentStoryId);
            if (story == null) return false;
            
            int oldChapter = CurrentChapter;
            CurrentChapter++;
            CurrentScene = 1;
            
            // Check if story is complete
            if (CurrentChapter > story.Chapters.Count)
            {
                await CompleteStoryAsync();
                return false;
            }
            
            // Update current chapter
            _currentChapters[CurrentStoryId] = story.Chapters[CurrentChapter - 1];
            
            // Trigger chapter change event
            OnChapterChanged?.Invoke(CurrentStoryId, oldChapter, CurrentChapter);
            
            var chapterEvent = new StoryEvent
            {
                Type = StoryEventType.ChapterStarted,
                StoryId = CurrentStoryId,
                Chapter = CurrentChapter,
                Scene = CurrentScene,
                Data = GetCurrentChapter()?.Title,
                Timestamp = DateTime.UtcNow
            };
            
            OnStoryEvent?.Invoke(chapterEvent);
            
            Log.Info($"Advanced to chapter {CurrentChapter} of story {CurrentStoryId}");
            return true;
        }
        
        /// <summary>
        /// Complete the current story
        /// </summary>
        public async Task CompleteStoryAsync()
        {
            var completionEvent = new StoryEvent
            {
                Type = StoryEventType.StoryCompleted,
                StoryId = CurrentStoryId,
                Chapter = CurrentChapter,
                Scene = CurrentScene,
                Timestamp = DateTime.UtcNow
            };
            
            OnStoryEvent?.Invoke(completionEvent);
            
            // Award completion rewards
            var gameState = GameStateService.Instance;
            if (gameState.CurrentPlayer != null)
            {
                gameState.CurrentPlayer.ExplorationPoints += 1000; // Major XP reward
                Log.Info($"Story {CurrentStoryId} completed! Awarded 1000 XP.");
            }
            
            // Could trigger new story unlock here
            await Task.CompletedTask;
        }
        
        #endregion
        
        #region Quest Objectives
        
        /// <summary>
        /// Update progress on a quest objective
        /// </summary>
        public void UpdateObjective(string objectiveId, bool completed = true)
        {
            var currentChapter = GetCurrentChapter();
            if (currentChapter == null) return;
            
            var objective = currentChapter.Objectives.FirstOrDefault(o => o.Id == objectiveId);
            if (objective != null && !objective.IsCompleted)
            {
                objective.IsCompleted = completed;
                OnQuestObjectiveUpdated?.Invoke(objective);
                
                var objectiveEvent = new StoryEvent
                {
                    Type = StoryEventType.ObjectiveCompleted,
                    StoryId = CurrentStoryId,
                    Chapter = CurrentChapter,
                    Scene = CurrentScene,
                    Data = objective.Description,
                    Timestamp = DateTime.UtcNow
                };
                
                OnStoryEvent?.Invoke(objectiveEvent);
                
                // Check if all objectives in chapter are complete
                if (currentChapter.Objectives.All(o => o.IsCompleted))
                {
                    _ = Task.Run(AdvanceChapterAsync);
                }
            }
        }
        
        /// <summary>
        /// Get the current chapter
        /// </summary>
        public StoryChapter? GetCurrentChapter()
        {
            return _currentChapters.GetValueOrDefault(CurrentStoryId);
        }
        
        /// <summary>
        /// Get all available objectives in the current chapter
        /// </summary>
        public List<QuestObjective> GetCurrentObjectives()
        {
            return GetCurrentChapter()?.Objectives ?? new List<QuestObjective>();
        }
        
        #endregion
        
        #region Story Flags and Conditions
        
        /// <summary>
        /// Set a story flag value
        /// </summary>
        public void SetStoryFlag(string flagId, object value)
        {
            StoryFlags[flagId] = value;
            
            var flagEvent = new StoryEvent
            {
                Type = StoryEventType.FlagChanged,
                StoryId = CurrentStoryId,
                Chapter = CurrentChapter,
                Scene = CurrentScene,
                Data = $"{flagId}={value}",
                Timestamp = DateTime.UtcNow
            };
            
            OnStoryEvent?.Invoke(flagEvent);
        }
        
        /// <summary>
        /// Get a story flag value
        /// </summary>
        public T GetStoryFlag<T>(string flagId, T defaultValue = default(T))
        {
            if (StoryFlags.TryGetValue(flagId, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Check if a story condition is met
        /// </summary>
        public bool IsConditionMet(StoryCondition condition)
        {
            return condition.Type switch
            {
                StoryConditionType.FlagEquals => GetStoryFlag<object>(condition.FlagId) == condition.Value,
                StoryConditionType.FlagGreaterThan => CompareNumericFlag(condition.FlagId, condition.Value, (a, b) => a > b),
                StoryConditionType.FlagLessThan => CompareNumericFlag(condition.FlagId, condition.Value, (a, b) => a < b),
                StoryConditionType.PlayerLevel => GameStateService.Instance.CurrentPlayer?.PlayerLevel >= (int)(condition.Value ?? 0),
                StoryConditionType.BotCount => GameStateService.Instance.BotRoster.Count >= (int)(condition.Value ?? 0),
                StoryConditionType.HasBot => HasBotOfType((string?)condition.Value),
                _ => false
            };
        }
        
        private bool CompareNumericFlag(string flagId, object? targetValue, Func<double, double, bool> comparer)
        {
            var flagValue = GetStoryFlag<object>(flagId);
            if (flagValue is IConvertible convertibleFlag && targetValue is IConvertible convertibleTarget)
            {
                try
                {
                    double flagNum = Convert.ToDouble(convertibleFlag);
                    double targetNum = Convert.ToDouble(convertibleTarget);
                    return comparer(flagNum, targetNum);
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
        
        private bool HasBotOfType(string? elementalType)
        {
            if (string.IsNullOrEmpty(elementalType)) return false;
            
            var targetElement = Enum.TryParse<ElementalCore>(elementalType, true, out var element) ? element : ElementalCore.None;
            return GameStateService.Instance.BotRoster.Any(bot => bot.ElementalCore == targetElement);
        }
        
        #endregion
        
        #region Chapter Creation Helpers
        
        private StoryChapter CreateTutorialChapter()
        {
            return new StoryChapter
            {
                Id = 1,
                Title = "A New Beginning",
                Description = "Learn the basics of being a Gladiator trainer",
                MaxScenes = 5,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective("Choose your starter bot", "choose_starter", false),
                    new QuestObjective("Win your first battle", "first_victory", false),
                    new QuestObjective("Visit the training grounds", "visit_training", false),
                    new QuestObjective("Explore the town", "explore_town", false)
                }
            };
        }
        
        private StoryChapter CreateEarlyGameChapter()
        {
            return new StoryChapter
            {
                Id = 2,
                Title = "Building Your Team",
                Description = "Expand your roster and master the fundamentals",
                MaxScenes = 8,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective("Capture 3 different bots", "capture_three", false),
                    new QuestObjective("Win 5 battles", "win_five", false),
                    new QuestObjective("Reach trainer level 5", "level_five", false),
                    new QuestObjective("Learn about elemental advantages", "learn_elements", false)
                }
            };
        }
        
        private StoryChapter CreateMidGameChapter()
        {
            return new StoryChapter
            {
                Id = 3,
                Title = "The Elemental Trials",
                Description = "Face the elemental gym leaders and prove your worth",
                MaxScenes = 10,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective("Defeat the Fire Gym Leader", "defeat_fire_gym", false),
                    new QuestObjective("Defeat the Water Gym Leader", "defeat_water_gym", false),
                    new QuestObjective("Defeat the Electric Gym Leader", "defeat_electric_gym", false),
                    new QuestObjective("Master bot fusion techniques", "learn_fusion", false)
                }
            };
        }
        
        private StoryChapter CreateLateGameChapter()
        {
            return new StoryChapter
            {
                Id = 4,
                Title = "The Elite Challenge",
                Description = "Battle the most skilled trainers in the region",
                MaxScenes = 12,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective("Defeat all 4 Elite Trainers", "defeat_elite_four", false),
                    new QuestObjective("Reach trainer level 25", "level_twenty_five", false),
                    new QuestObjective("Capture a legendary bot", "capture_legendary", false)
                }
            };
        }
        
        private StoryChapter CreateEndGameChapter()
        {
            return new StoryChapter
            {
                Id = 5,
                Title = "Champion's Destiny",
                Description = "Face the ultimate challenge and claim your place as Champion",
                MaxScenes = 15,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective("Challenge the reigning Champion", "challenge_champion", false),
                    new QuestObjective("Prove your mastery in the final battle", "final_battle", false),
                    new QuestObjective("Become the new Champion", "become_champion", false)
                }
            };
        }
        
        #endregion
        
        #region Configuration
        
        /// <summary>
        /// Load a custom story configuration
        /// </summary>
        public async Task<bool> LoadCustomStoryAsync(string storyFilePath)
        {
            try
            {
                // TODO: Implement JSON story loading
                // This would allow users to create custom storylines
                await Task.Delay(100); // Placeholder
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load custom story from {storyFilePath}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Switch to a different story
        /// </summary>
        public bool SwitchToStory(string storyId, int chapter = 1, int scene = 1)
        {
            if (_availableStories.ContainsKey(storyId))
            {
                CurrentStoryId = storyId;
                CurrentChapter = chapter;
                CurrentScene = scene;
                
                var switchEvent = new StoryEvent
                {
                    Type = StoryEventType.StoryChanged,
                    StoryId = CurrentStoryId,
                    Chapter = CurrentChapter,
                    Scene = CurrentScene,
                    Timestamp = DateTime.UtcNow
                };
                
                OnStoryEvent?.Invoke(switchEvent);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get all available stories
        /// </summary>
        public List<Story> GetAvailableStories()
        {
            return _availableStories.Values.ToList();
        }
        
        #endregion
    }
    
    #region Supporting Classes
    
    /// <summary>
    /// Represents a complete story/campaign
    /// </summary>
    public class Story
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<StoryChapter> Chapters { get; set; } = new();
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedDate { get; set; }
    }
    
    /// <summary>
    /// Represents a chapter within a story
    /// </summary>
    public class StoryChapter
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int MaxScenes { get; set; } = 5;
        public List<QuestObjective> Objectives { get; set; } = new();
        public List<StoryCondition> UnlockConditions { get; set; } = new();
        public Dictionary<string, object> Rewards { get; set; } = new();
    }
    
    /// <summary>
    /// Represents a quest objective within a chapter
    /// </summary>
    public class QuestObjective
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int Progress { get; set; } = 0;
        public int MaxProgress { get; set; } = 1;
        
        public QuestObjective(string description, string id, bool completed = false)
        {
            Description = description;
            Id = id;
            IsCompleted = completed;
        }
    }
    
    /// <summary>
    /// Represents a story event that occurred
    /// </summary>
    public class StoryEvent
    {
        public StoryEventType Type { get; set; }
        public string StoryId { get; set; } = "";
        public int Chapter { get; set; }
        public int Scene { get; set; }
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Represents a condition that must be met for story progression
    /// </summary>
    public class StoryCondition
    {
        public StoryConditionType Type { get; set; }
        public string FlagId { get; set; } = "";
        public object? Value { get; set; }
        public string Description { get; set; } = "";
    }
    
    /// <summary>
    /// Types of story events
    /// </summary>
    public enum StoryEventType
    {
        StoryStarted,
        StoryCompleted,
        StoryChanged,
        ChapterStarted,
        ChapterCompleted,
        SceneChanged,
        ObjectiveCompleted,
        FlagChanged,
        DialogueTriggered,
        BattleTriggered,
        ItemReceived
    }
    
    /// <summary>
    /// Types of story conditions
    /// </summary>
    public enum StoryConditionType
    {
        FlagEquals,
        FlagGreaterThan,
        FlagLessThan,
        PlayerLevel,
        BotCount,
        HasBot,
        ItemCount,
        BattlesWon
    }
    
    #endregion
}