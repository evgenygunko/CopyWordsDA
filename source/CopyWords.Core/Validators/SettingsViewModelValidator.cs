// Ignore Spelling: Validator Api

using CopyWords.Core.Services.Wrappers;
using CopyWords.Core.ViewModels;
using FluentValidation;

namespace CopyWords.Core.Validators
{
    public class SettingsViewModelValidator : AbstractValidator<SettingsViewModel>
    {
        public static string AnkiSoundsFolderProperty => nameof(SettingsViewModel.AnkiSoundsFolder);
        public static string DictionaryOptionsProperty => nameof(SettingsViewModel.DictionaryOptions);

        public SettingsViewModelValidator(IFileIOService fileIOService)
        {
            RuleFor(model => model.DictionaryOptions.Count(x => x.IsEnabled)).GreaterThan(0)
                .WithName(DictionaryOptionsProperty)
                .WithMessage("At least one dictionary must be enabled");

            RuleFor(model => model.AnkiSoundsFolder).NotEmpty()
                .WithMessage("Path to Anki media collection cannot be empty")
                .Must(fileIOService.DirectoryExists)
                .WithMessage("The specified directory does not exist");
        }
    }
}
