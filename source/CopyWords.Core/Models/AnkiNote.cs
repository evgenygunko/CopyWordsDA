using Newtonsoft.Json;

namespace CopyWords.Core.Models
{
    public record AnkiNote(
        string DeckName,
        string ModelName,
        string Front,
        string Back,
        string? PartOfSpeech = null,
        string? Forms = null,
        string? Example = null,
        string? Sound = null,
        IEnumerable<string>? Tags = null,
        AnkiNoteOptions? Options = null,
        IEnumerable<AnkiMedia>? Audio = null,
        IEnumerable<AnkiMedia>? Video = null,
        IEnumerable<AnkiMedia>? Picture = null);

    public record AnkiNoteOptions(
        bool AllowDuplicate = false,
        string? DuplicateScope = null,
        AnkiDuplicateScopeOptions? DuplicateScopeOptions = null);

    public record AnkiDuplicateScopeOptions(
        string? DeckName = null,
        bool CheckChildren = false,
        bool CheckAllModels = false);

    public record AnkiMedia(
        string Url,
        string Filename,
        string? SkipHash = null,
        IEnumerable<string>? Fields = null);

    internal record AddNoteRequest(
        [property: JsonProperty("action")] string Action,
        [property: JsonProperty("version")] int Version,
        [property: JsonProperty("params")] AddNoteParams Params);

    internal record AddNoteParams(
        [property: JsonProperty("note")] AddNoteNote Note);

    internal record AddNoteNote(
        [property: JsonProperty("deckName")] string DeckName,
        [property: JsonProperty("modelName")] string ModelName,
        [property: JsonProperty("fields")] IReadOnlyDictionary<string, string> Fields,
        [property: JsonProperty("tags")] IEnumerable<string>? Tags,
        [property: JsonProperty("options")] AddNoteNoteOptions? Options = null,
        [property: JsonProperty("audio")] IEnumerable<AddNoteMedia>? Audio = null,
        [property: JsonProperty("video")] IEnumerable<AddNoteMedia>? Video = null,
        [property: JsonProperty("picture")] IEnumerable<AddNoteMedia>? Picture = null);

    internal record AddNoteNoteOptions(
        [property: JsonProperty("allowDuplicate")] bool AllowDuplicate,
        [property: JsonProperty("duplicateScope")] string? DuplicateScope = null,
        [property: JsonProperty("duplicateScopeOptions")] AddNoteDuplicateScopeOptions? DuplicateScopeOptions = null);

    internal record AddNoteDuplicateScopeOptions(
        [property: JsonProperty("deckName")] string? DeckName = null,
        [property: JsonProperty("checkChildren")] bool CheckChildren = false,
        [property: JsonProperty("checkAllModels")] bool CheckAllModels = false);

    internal record AddNoteMedia(
        [property: JsonProperty("url")] string Url,
        [property: JsonProperty("filename")] string Filename,
        [property: JsonProperty("skipHash")] string? SkipHash = null,
        [property: JsonProperty("fields")] IEnumerable<string>? Fields = null);

    internal record AddNoteResponse(
        [property: JsonProperty("result")] long? Result,
        [property: JsonProperty("error")] string? Error);

    internal record FindNotesRequest(
        [property: JsonProperty("action")] string Action,
        [property: JsonProperty("version")] int Version,
        [property: JsonProperty("params")] FindNotesParams Params);

    internal record FindNotesParams(
        [property: JsonProperty("query")] string Query);

    internal record FindNotesResponse(
        [property: JsonProperty("result")] IEnumerable<long>? Result,
        [property: JsonProperty("error")] string? Error);

    internal record DeckNamesResponse(
        [property: JsonProperty("result")] IEnumerable<string>? Result,
        [property: JsonProperty("error")] string? Error);

    internal record ModelNamesResponse(
        [property: JsonProperty("result")] IEnumerable<string>? Result,
        [property: JsonProperty("error")] string? Error);
}
