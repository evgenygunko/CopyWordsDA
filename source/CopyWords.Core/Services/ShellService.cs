namespace CopyWords.Core.Services
{
    public interface IShellService
    {
        Task GoToAsync(ShellNavigationState state);

        Task PopModalAsync();
    }

    public class ShellService : IShellService
    {
        public async Task GoToAsync(ShellNavigationState state) => await Shell.Current.GoToAsync(state);

        public async Task PopModalAsync() => await Shell.Current.Navigation.PopModalAsync();
    }
}
