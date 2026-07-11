namespace CopyWords.Core.Services
{
    public interface ITranslationRefreshState
    {
        void SetRefreshRequired(bool refreshRequired);

        bool ConsumeRefreshRequired();
    }

    public class TranslationRefreshState : ITranslationRefreshState
    {
        private readonly object _syncRoot = new();
        private bool? _refreshRequired;

        public void SetRefreshRequired(bool refreshRequired)
        {
            lock (_syncRoot)
            {
                _refreshRequired = refreshRequired;
            }
        }

        public bool ConsumeRefreshRequired()
        {
            lock (_syncRoot)
            {
                bool refreshRequired = _refreshRequired ?? true;
                _refreshRequired = null;
                return refreshRequired;
            }
        }
    }
}
