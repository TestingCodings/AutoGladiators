using AutoGladiators.Client.ViewModels;

namespace AutoGladiators.Client.Pages;

public partial class VictoryPage : ContentPage
{
    public VictoryPage(VictoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}