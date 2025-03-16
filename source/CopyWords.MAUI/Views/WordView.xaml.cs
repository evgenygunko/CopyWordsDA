using CommunityToolkit.Maui.Views;

namespace CopyWords.MAUI.Views;

public partial class WordView : ContentView
{
    public WordView()
    {
        InitializeComponent();

        // Workaround for a bug in MediaElement:
        // On MacOS, the app crashes at startup with the runtime exception:
        // "System.MissingMethodException: No parameterless constructor defined for type 'CommunityToolkit.Maui.Views.MediaElement'."
        // To resolve this, we create the MediaElement manually in the C# file.
        // However, it must be added to the Visual Tree; otherwise, there is no sound.
        // Reference: https://stackoverflow.com/a/75535084
        var mediaElement = MauiProgram.GetService<MediaElement>();

        stackSound.Children.Add(mediaElement);
    }
}