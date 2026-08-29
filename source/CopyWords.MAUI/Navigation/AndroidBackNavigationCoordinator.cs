namespace CopyWords.MAUI.Navigation;

internal sealed class AndroidBackNavigationCoordinator : IBackNavigationCoordinator
{
    private Func<bool>? _canNavigateBack;
    private Func<Task<bool>>? _navigateBackAsync;
    private int _isNavigating;

    public event EventHandler? StateChanged;

    public bool ShouldInterceptBack
    {
        get
        {
            Func<bool>? canNavigateBack = _canNavigateBack;
            if (canNavigateBack is null)
            {
                return false;
            }

            return Volatile.Read(ref _isNavigating) != 0 || canNavigateBack();
        }
    }

    public void Activate(Func<bool> canNavigateBack, Func<Task<bool>> navigateBackAsync)
    {
        ArgumentNullException.ThrowIfNull(canNavigateBack);
        ArgumentNullException.ThrowIfNull(navigateBackAsync);

        _canNavigateBack = canNavigateBack;
        _navigateBackAsync = navigateBackAsync;
        OnStateChanged();
    }

    public void Deactivate()
    {
        _canNavigateBack = null;
        _navigateBackAsync = null;
        OnStateChanged();
    }

    public void Refresh()
    {
        OnStateChanged();
    }

    public async Task<bool> TryNavigateBackAsync()
    {
        Func<bool>? canNavigateBack = _canNavigateBack;
        Func<Task<bool>>? navigateBackAsync = _navigateBackAsync;
        if (canNavigateBack is null || navigateBackAsync is null)
        {
            return false;
        }

        if (Interlocked.CompareExchange(ref _isNavigating, 1, 0) != 0)
        {
            // Consume repeated presses while the first navigation is still running.
            return true;
        }

        try
        {
            if (!canNavigateBack())
            {
                return false;
            }

            OnStateChanged();
            return await navigateBackAsync();
        }
        finally
        {
            Interlocked.Exchange(ref _isNavigating, 0);
            OnStateChanged();
        }
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
