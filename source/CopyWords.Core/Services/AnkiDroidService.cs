// Ignore Spelling: Anki ankidroid

using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services.Wrappers;

namespace CopyWords.Core.Services
{
    public interface IAnkiDroidService
    {
        bool IsAvailable();

        bool HasPermission();

        Task RequestPermissionAsync();

        IEnumerable<string> GetDeckNames();

        IEnumerable<string> GetModelNames();

        Task<long> AddNoteAsync(AnkiNote note, CancellationToken cancellationToken);
    }

    public class AnkiDroidService : IAnkiDroidService
    {
        private readonly IAnkiContentApi _ankiContentApi;
        private readonly IDialogService _dialogService;
        private readonly ISaveImageFileService _saveImageFileService;
        private readonly ISaveSoundFileService _saveSoundFileService;

        public AnkiDroidService(
            IAnkiContentApi ankiContentApi,
            IDialogService dialogService,
            ISaveImageFileService saveImageFileService,
            ISaveSoundFileService saveSoundFileService)
        {
            _ankiContentApi = ankiContentApi;
            _dialogService = dialogService;
            _saveImageFileService = saveImageFileService;
            _saveSoundFileService = saveSoundFileService;
        }

        #region Public Methods

        public bool IsAvailable() => _ankiContentApi.IsAvailable();

        public bool HasPermission() => _ankiContentApi.HasPermission();

        public async Task RequestPermissionAsync() => await _ankiContentApi.RequestPermissionAsync();

        public IEnumerable<string> GetDeckNames() =>
            _ankiContentApi.GetDeckList()?.Values.ToList() ?? [];

        public IEnumerable<string> GetModelNames() =>
            _ankiContentApi.GetModelList()?.Values.ToList() ?? [];

        public async Task<long> AddNoteAsync(AnkiNote note, CancellationToken cancellationToken)
        {
            long noteId = 0;

            // Get deck ID from deck name
            long? deckId = _ankiContentApi.GetDeckList()?.FirstOrDefault(kv => kv.Value == note.DeckName).Key;
            if (deckId == null || deckId == 0)
            {
                throw new AnkiDroidDeckNotFoundException($"Deck '{note.DeckName}' not found in AnkiDroid.");
            }

            // Get model ID from model name
            long? modelId = _ankiContentApi.GetModelList()?.FirstOrDefault(kv => kv.Value == note.ModelName).Key;
            if (modelId is null || modelId == 0)
            {
                throw new AnkiDroidModelNotFoundException($"Model '{note.ModelName}' not found in AnkiDroid.");
            }

            // Get field names for the model
            string[]? fieldNames = _ankiContentApi.GetFieldList(modelId.Value);
            if (fieldNames is null || fieldNames.Length == 0)
            {
                throw new AnkiDroidFieldsNotFoundException($"No fields found for model '{note.ModelName}'.");
            }

            // Check for duplicate notes using the Front field value
            bool isDuplicate = _ankiContentApi.FindDuplicateNotes(modelId.Value, note.Front).Any();
            if (isDuplicate)
            {
                bool shouldAdd = await _dialogService.DisplayAlertAsync(
                    "Note already exists",
                    $"Note '{note.Front}' already exists in the deck '{note.DeckName}'. Do you want to add a duplicate note?",
                    "Yes",
                    "No");

                if (!shouldAdd)
                {
                    return noteId;
                }
            }

            Note noteModel = new Note()
            {
                Front = note.Front,
                Back = note.Back,
                PartOfSpeech = note.PartOfSpeech ?? string.Empty,
                Forms = note.Forms ?? string.Empty,
                Example = note.Example ?? string.Empty
            };

            if (note.Picture != null && note.Picture.Any())
            {
                // AnkiDroid API returns image tags when saving images, for example '<img src="voluntario.jpg_6481766173072004017.jpg" />'.
                // We need to save images first and then use these image tags in the back field.
                IEnumerable<ImageTag> imageTags = await SaveImagesAsync(note.Picture, cancellationToken);

                // Replace image html tag in the back field with the new image tags with correct file names
                foreach (var imageTag in imageTags)
                {
                    noteModel.Back = noteModel.Back.Replace($"<img src=\"{imageTag.FileName}\">", imageTag.HtmlTag);
                }
            }

            if (note.Audio != null && note.Audio.Any())
            {
                // Save sound with AnkiDroid API and get the sound tag
                var soundTag = await SaveSoundAsync(note.Audio, cancellationToken);
                noteModel.Sound = soundTag.AnkiTag;
            }

            // Build fields array matching the model's field order
            string[] fields = BuildFieldsArray(fieldNames, noteModel);

            // Add the note
            noteId = _ankiContentApi.AddNote(modelId.Value, deckId.Value, fields, null);
            return noteId;
        }

        #endregion

        #region Internal Methods

        internal async Task<IEnumerable<ImageTag>> SaveImagesAsync(IEnumerable<AnkiMedia> ankiMediaImages, CancellationToken cancellationToken)
        {
            List<ImageTag> imageTags = new();

            foreach (AnkiMedia ankiMedia in ankiMediaImages)
            {
                await using Stream stream = await _saveImageFileService.DownloadAndResizeImageAsync(ankiMedia.Url, cancellationToken);

                string htmlTag = await _ankiContentApi.AddImageToAnkiMediaAsync(ankiMedia.Filename, stream);
                imageTags.Add(new ImageTag(ankiMedia.Filename, htmlTag));
            }

            return imageTags;
        }

        internal async Task<SoundTag> SaveSoundAsync(IEnumerable<AnkiMedia> ankiMediaSounds, CancellationToken cancellationToken)
        {
            // We only allow to save one sound file per note
            AnkiMedia ankiMedia = ankiMediaSounds.First();

            // We will pass "Word" in the filename property
            string word = ankiMedia.Filename;
            await using Stream stream = await _saveSoundFileService.DownloadSoundFileAsync(ankiMedia.Url, word, cancellationToken);

            string fileName = $"{word}.mp3";
            string htmlTag = await _ankiContentApi.AddImageToAnkiMediaAsync(fileName, stream);

            return new SoundTag(fileName, htmlTag);
        }

        #endregion

        #region Private Methods

        private static string[] BuildFieldsArray(string[] fieldNames, Note note)
        {
            var fields = new string[fieldNames.Length];

            for (int i = 0; i < fieldNames.Length; i++)
            {
                fields[i] = fieldNames[i] switch
                {
                    "Front" => note.Front,
                    "Back" => note.Back,
                    "PartOfSpeech" => note.PartOfSpeech,
                    "Forms" => note.Forms,
                    "Example" => note.Example,
                    "Sound" => note.Sound,
                    _ => string.Empty
                };
            }

            return fields;
        }

        #endregion

        private record Note
        {
            public string Front { get; set; } = "";
            public string Back { get; set; } = "";
            public string PartOfSpeech { get; set; } = "";
            public string Forms { get; set; } = "";
            public string Example { get; set; } = "";
            public string Sound { get; set; } = "";
        }
    }
}
