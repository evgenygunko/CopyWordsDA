namespace CopyWords.Core.Services
{
    /// <summary>
    /// Abstraction over the AnkiDroid AddContentApi for testability.
    /// Uses .NET types instead of Java types.
    /// </summary>
    public interface IAnkiContentApi
    {
        bool IsAvailable();

        bool HasPermission();

        Task RequestPermissionAsync();

        /// <summary>
        /// Gets the list of decks as a dictionary of deck ID to deck name.
        /// </summary>
        IDictionary<long, string>? GetDeckList();

        /// <summary>
        /// Gets the list of models (note types) as a dictionary of model ID to model name.
        /// </summary>
        IDictionary<long, string>? GetModelList();

        /// <summary>
        /// Gets the field names for a given model.
        /// </summary>
        string[]? GetFieldList(long modelId);

        /// <summary>
        /// Adds a note to AnkiDroid.
        /// </summary>
        /// <returns>True if the note was added successfully; false otherwise.</returns>
        long AddNote(long modelId, long deckId, string[] fields, string[]? tags);

        /// <summary>
        /// Finds all notes in the collection with the given model, where a first field matches the given key.
        /// </summary>
        /// <param name="modelId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        List<long> FindDuplicateNotes(long modelId, string key);

        /// <summary>
        /// Saves an image to the Anki media folder.
        /// </summary>
        /// <param name="fileName">The target file name.</param>
        /// <param name="imageStream">The image content as a stream.</param>
        /// <returns>Image tag with file name saves in the media folder</returns>
        /// <example><img src="voluntario.jpg_6481766173072004017.jpg" /></example>
        Task<string> AddImageToAnkiMediaAsync(string fileName, Stream imageStream);
    }
}
