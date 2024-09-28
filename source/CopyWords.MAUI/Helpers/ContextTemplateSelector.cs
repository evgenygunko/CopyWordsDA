using CopyWords.Core.ViewModels;

namespace CopyWords.MAUI.Helpers
{
    public class ContextTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ContextsTemplate { get; set; } = default!;
        public DataTemplate MeaningsTemplate { get; set; } = default!;

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item != null && item is ContextViewModel contextVM)
            {
                if (string.IsNullOrEmpty(contextVM.Position))
                {
                    return MeaningsTemplate;
                }
            }

            return ContextsTemplate;
        }
    }
}
