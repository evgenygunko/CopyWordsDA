using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Services;

namespace CopyWords.Core.ViewModels
{
    public partial class GetUpdateViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;
        private readonly IShellService _shellService;

        public GetUpdateViewModel(
            IUpdateService updateService,
            IShellService shellService)
        {
            _updateService = updateService;
            _shellService = shellService;
        }

        [ObservableProperty]
        private string latestVersion;

        [RelayCommand]
        public async Task GetLatestReleaseAsync()
        {
            LatestVersion = (await _updateService.GetLatestReleaseVersionAsync()).ToString();
        }

        [RelayCommand]
        public async Task CloseDialogAsync()
        {
            await _shellService.PopModalAsync();
        }
    }
}
