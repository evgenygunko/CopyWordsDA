// Ignore Spelling: Validator Anki

using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.Validators;
using FluentAssertions;
using FluentValidation.Results;

namespace CopyWords.Core.Tests.Validators
{
    [TestClass]
    public class AnkiNoteValidatorTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        [TestMethod]
        public void Validate_WhenAllRequiredFieldsHaveValues_ReturnsTrue()
        {
            var ankiNote = _fixture.Create<AnkiNote>();

            var sut = new AnkiNoteValidator();
            ValidationResult result = sut.Validate(ankiNote);

            result.IsValid.Should().BeTrue(result.Errors.FirstOrDefault()?.ErrorMessage);
        }

        [TestMethod]
        public void Validate_WhenDeckNameIsEmpty_ReturnsFalse()
        {
            var ankiNote = _fixture.Build<AnkiNote>()
                .With(x => x.DeckName, string.Empty)
                .Create();

            var sut = new AnkiNoteValidator();
            ValidationResult result = sut.Validate(ankiNote);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().ErrorMessage.Should().Be("'Deck Name' must not be empty.");
        }

        [TestMethod]
        public void Validate_WhenModelNameIsEmpty_ReturnsFalse()
        {
            var ankiNote = _fixture.Build<AnkiNote>()
                .With(x => x.ModelName, string.Empty)
                .Create();

            var sut = new AnkiNoteValidator();
            ValidationResult result = sut.Validate(ankiNote);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().ErrorMessage.Should().Be("'Model Name' must not be empty.");
        }

        [TestMethod]
        public void Validate_WhenBothDeckNameAndModelNameAreEmpty_ReturnsFalse()
        {
            var ankiNote = _fixture.Build<AnkiNote>()
                .With(x => x.DeckName, string.Empty)
                .With(x => x.ModelName, string.Empty)
                .Create();

            var sut = new AnkiNoteValidator();
            ValidationResult result = sut.Validate(ankiNote);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
        }

        [TestMethod]
        public void Validate_WhenAnkiNoteIsNull_ReturnsFalse()
        {
            AnkiNote? ankiNote = null;

            var sut = new AnkiNoteValidator();
            ValidationResult result = sut.Validate(ankiNote!);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().ErrorMessage.Should().Be("Please ensure a model was supplied.");
        }
    }
}
