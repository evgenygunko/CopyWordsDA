// Ignore Spelling: Validator Api

using CopyWords.Core.ViewModels;
using FluentValidation;

namespace CopyWords.Core.Validators
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
