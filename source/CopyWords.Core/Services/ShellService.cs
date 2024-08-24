namespace CopyWords.Core.Services
{
    public interface IShellService
    {
        Task GoToAsync(ShellNavigationState state);
    }

    public class ShellService : IShellService
    {
        public async Task GoToAsync(ShellNavigationState state) => await Shell.Current.GoToAsync(state);
    }
}
