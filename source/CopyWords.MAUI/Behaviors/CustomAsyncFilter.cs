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
            if (this._cancellationTokenSource != null)
            {
                this._cancellationTokenSource.Cancel();
                this._cancellationTokenSource.Dispose();
            }

            this._cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = this._cancellationTokenSource.Token;

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
