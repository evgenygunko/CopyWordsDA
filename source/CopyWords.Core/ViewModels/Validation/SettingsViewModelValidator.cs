// Ignore Spelling: Validator Api

using FluentValidation;

namespace CopyWords.Core.ViewModels.Validation
{
    public class SettingsViewModelValidator : AbstractValidator<SettingsViewModel>
    {
        public static string AnkiSoundsFolderProperty => nameof(SettingsViewModel.AnkiSoundsFolder);
        public SettingsViewModelValidator()
        {
            RuleFor(model => model.AnkiSoundsFolder).NotEmpty();
        }
    }
}
