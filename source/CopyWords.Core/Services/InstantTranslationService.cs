#nullable enable

namespace CopyWords.Core.Services
{
    public interface IInstantTranslationService
    {
        void SetText(string text);

        string? GetTextAndClear();
    }

    public class InstantTranslationService : IInstantTranslationService
    {
        private string? _text;

        public string? GetTextAndClear()
        {
            var text = _text;
            _text = null;

            return text;
        }

        public void SetText(string text) => _text = text;
    }
}
