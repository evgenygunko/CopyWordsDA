// Ignore Spelling: Validator Anki

using CopyWords.Core.Models;
using FluentValidation;
using FluentValidation.Results;

namespace CopyWords.Core.Validators
{
    public class AnkiNoteValidator : AbstractValidator<AnkiNote>
    {
        public static string DeckNameProperty => nameof(AnkiNote.DeckName);
        public static string ModelNameProperty => nameof(AnkiNote.ModelName);

        public AnkiNoteValidator()
        {
            RuleFor(note => note.DeckName).NotEmpty();
            RuleFor(note => note.ModelName).NotEmpty();
        }

        protected override bool PreValidate(ValidationContext<AnkiNote> context, ValidationResult result)
        {
            if (context.InstanceToValidate == null)
            {
                result.Errors.Add(new ValidationFailure("", "Please ensure a model was supplied."));
                return false;
            }
            return true;
        }
    }
}
