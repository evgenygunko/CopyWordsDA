namespace CopyWords.Core.Models
{
    public record NormalizeSoundRequest(
         string SoundUrl,
         string Word,
         string Version);
}
