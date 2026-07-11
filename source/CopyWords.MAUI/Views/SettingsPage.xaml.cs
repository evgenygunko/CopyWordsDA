using CopyWords.Core.ViewModels;

namespace CopyWords.MAUI.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;

        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            Command = _viewModel.NavigateBackCommand
        });
    }

    protected override bool OnBackButtonPressed()
    {
        if (_viewModel.NavigateBackCommand.CanExecute(null))
        {
            Dispatcher.Dispatch(async () => await _viewModel.NavigateBackAsync());
            return true;
        }

        return base.OnBackButtonPressed();
    }
}
