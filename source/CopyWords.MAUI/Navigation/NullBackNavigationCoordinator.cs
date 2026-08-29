namespace CopyWords.MAUI.Navigation;

internal sealed class NullBackNavigationCoordinator : IBackNavigationCoordinator
{
    public void Activate(Func<bool> canNavigateBack, Func<Task<bool>> navigateBackAsync)
    {
    }

    public void Deactivate()
    {
    }

    public void Refresh()
    {
    }
}
