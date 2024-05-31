namespace CopyWords.Core.Services
{
    public interface IClipboardService
    {
        Task CopyTextToClipboardAsync(string text);
    }

    public class ClipboardService : IClipboardService
    {
        public async Task CopyTextToClipboardAsync(string text)
        {
            await Clipboard.Default.SetTextAsync(text);
        }
    }
}
