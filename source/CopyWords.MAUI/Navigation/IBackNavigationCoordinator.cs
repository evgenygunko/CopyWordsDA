namespace CopyWords.MAUI.Navigation;

public interface IBackNavigationCoordinator
{
    void Activate(Func<bool> canNavigateBack, Func<Task<bool>> navigateBackAsync);

    void Deactivate();

    void Refresh();
}
