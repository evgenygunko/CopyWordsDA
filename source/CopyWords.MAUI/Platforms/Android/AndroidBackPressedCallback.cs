using AndroidX.Activity;
using CopyWords.MAUI.Navigation;

namespace CopyWords.MAUI;

internal sealed class AndroidBackPressedCallback : OnBackPressedCallback
{
    private readonly AndroidBackNavigationCoordinator _coordinator;
    private readonly OnBackPressedDispatcher _dispatcher;

    public AndroidBackPressedCallback(
        AndroidBackNavigationCoordinator coordinator,
        OnBackPressedDispatcher dispatcher)
        : base(coordinator.ShouldInterceptBack)
    {
        _coordinator = coordinator;
        _dispatcher = dispatcher;
        _coordinator.StateChanged += OnCoordinatorStateChanged;
    }

    public override async void HandleOnBackPressed()
    {
        bool handled = await _coordinator.TryNavigateBackAsync();
        if (!handled)
        {
            FallThroughToSystemBack();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _coordinator.StateChanged -= OnCoordinatorStateChanged;
        }

        base.Dispose(disposing);
    }

    private void OnCoordinatorStateChanged(object? sender, EventArgs e)
    {
        Enabled = _coordinator.ShouldInterceptBack;
    }

    private void FallThroughToSystemBack()
    {
        Enabled = false;
        _dispatcher.OnBackPressed();
        Enabled = _coordinator.ShouldInterceptBack;
    }
}
