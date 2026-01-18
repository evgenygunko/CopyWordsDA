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
    }
}
