// Ignore Spelling: Validator Api

using CopyWords.Core.Services.Wrappers;
using CopyWords.Core.ViewModels;
using FluentValidation;

namespace CopyWords.Core.Validators
{
    public class SettingsViewModelValidator : AbstractValidator<SettingsViewModel>
    {
        public static string AnkiSoundsFolderProperty => nameof(SettingsViewModel.AnkiSoundsFolder);

        public SettingsViewModelValidator(IFileIOService fileIOService)
        {
            RuleFor(model => model.AnkiSoundsFolder).NotEmpty()
                .WithMessage("Path to Anki media collection cannot be empty")
                .Must(fileIOService.DirectoryExists)
                .WithMessage("The specified directory does not exist");
        }
    }
}
