
---

🖥️ **Advanced AutoGladiators.Client** (Xamarin.Forms App)

File: `AutoGladiators.Client/MainPage.xaml.cs`


using Xamarin.Forms;
using SkillTreeLib;
using System.Linq;
using System.Text;

namespace AutoGladiators.Client
{
    public partial class MainPage : ContentPage
    {
        private SkillTree skillTree;
        private List<Skill> selectedPhysicalSkills = new();
        private List<Skill> selectedWeaponrySkills = new();

        public MainPage()
        {
            InitializeComponent();
            skillTree = new SkillTree();
            LoadSkills();
        }

        private void LoadSkills()
        {
            foreach (var skill in skillTree.PhysicalSkills)
            {
                var button = new Button
                {
                    Text = skill.Name,
                    BackgroundColor = Color.Gray,
                    TextColor = Color.White
                };
                button.Clicked += (s, e) => ToggleSkill(skill, SkillCategory.Physical, button);
                PhysicalSkillsLayout.Children.Add(button);
            }

            foreach (var skill in skillTree.WeaponrySkills)
            {
                var button = new Button
                {
                    Text = skill.Name,
                    BackgroundColor = Color.DarkRed,
                    TextColor = Color.White
                };
                button.Clicked += (s, e) => ToggleSkill(skill, SkillCategory.Weaponry, button);
                WeaponrySkillsLayout.Children.Add(button);
            }

            UpdateStats();
        }

        private void ToggleSkill(Skill skill, SkillCategory category, Button button)
        {
            var list = category == SkillCategory.Physical ? selectedPhysicalSkills : selectedWeaponrySkills;

            if (list.Contains(skill))
            {
                list.Remove(skill);
                button.BackgroundColor = category == SkillCategory.Physical ? Color.Gray : Color.DarkRed;
            }
            else
            {
                list.Add(skill);
                button.BackgroundColor = Color.LimeGreen;
            }

            UpdateStats();
        }

        private void UpdateStats()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Physical:");
            foreach (var skill in selectedPhysicalSkills)
                builder.AppendLine($"- {skill.Name}");

            builder.AppendLine("Weaponry:");
            foreach (var skill in selectedWeaponrySkills)
                builder.AppendLine($"- {skill.Name}");

            StatsLabel.Text = builder.ToString();
        }

        private void RunSimulation_Clicked(object sender, EventArgs e)
        {
            DisplayAlert("Simulating...", "Battle simulator not implemented yet", "OK");
        }
    }
}
