// Ignore Spelling: Validator Ffmpeg Api

using FluentValidation;

namespace CopyWords.Core.ViewModels.Validation
{
    public class SettingsViewModelValidator : AbstractValidator<SettingsViewModel>
    {
        public static string AnkiSoundsFolderProperty => nameof(SettingsViewModel.AnkiSoundsFolder);
        public static string FfmpegBinFolderProperty => nameof(SettingsViewModel.FfmpegBinFolder);
        public static string Mp3gainPathProperty => nameof(SettingsViewModel.Mp3gainPath);

        public SettingsViewModelValidator()
        {
            RuleFor(model => model.AnkiSoundsFolder).NotEmpty();

            RuleFor(model => model.FfmpegBinFolder).NotEmpty()
                .When(x => x.CanUseFfmpeg)
                .WithMessage("'Path to Ffmpeg' must not be empty.");

            RuleFor(model => model.Mp3gainPath).NotEmpty()
                .When(x => x.CanUseMp3gain && x.UseMp3gain)
                .WithMessage("'Path to mp3gain' must not be empty.");
        }
    }
}
