namespace CopyWords.Core.Models
{
    public interface IBuildConfiguration
    {
        bool IsDebug { get; }
    }

    public class BuildConfiguration : IBuildConfiguration
    {
        public bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}
