using System.Diagnostics;
using CopyWords.Core.ViewModels;
using Syncfusion.Maui.Inputs;

namespace CopyWords.MAUI.Behaviors
{
    public class CustomAsyncFilter : IAutocompleteFilterBehavior
    {
        private CancellationTokenSource? _cancellationTokenSource;

        public async Task<object?> GetMatchingItemsAsync(SfAutocomplete source, AutocompleteFilterInfo filterInfo)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            var vm = source.BindingContext as MainViewModel;
            if (vm != null)
            {
                Debug.WriteLine($"Getting suggested words for the input '{filterInfo.Text}'");
                return await vm.GetSuggestionsAsync(filterInfo.Text ?? string.Empty, token);
            }

            return new List<string>();
        }
    }
}
