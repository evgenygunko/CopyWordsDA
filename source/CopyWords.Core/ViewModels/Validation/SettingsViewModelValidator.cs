﻿// Ignore Spelling: Validator

using FluentValidation;

namespace CopyWords.Core.ViewModels.Validation
{
    public class SettingsViewModelValidator : AbstractValidator<SettingsViewModel>
    {
        public SettingsViewModelValidator()
        {
            RuleFor(model => model.AnkiSoundsFolder).NotEmpty();

            RuleFor(model => model.FfmpegBinFolder).NotEmpty()
                .When(x => x.CanUseFfmpeg)
                .WithMessage("'Path to Ffmpeg' must not be empty.");

            RuleFor(model => model.Mp3gainPath).NotEmpty()
                .When(x => x.CanUseMp3gain && x.UseMp3gain)
                .WithMessage("'Path to mp3gain' must not be empty.");

            RuleFor(x => x.TranslatorApiUrl)
                .Must(LinkMustBeAUri)
                .When(x => x.UseTranslator)
                .WithMessage("'TranslatorAPI URL' must be a valid URL.");
        }

        private static bool LinkMustBeAUri(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                return false;
            }

            Uri outUri;
            return Uri.TryCreate(link, UriKind.Absolute, out outUri)
                   && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
