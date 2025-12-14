using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace CopyWords.Core.Services.Wrappers
{
    public interface IMediaElementWrapper
    {
        MediaSource? Source { get; set; }

        MediaElementState CurrentState { get; }

        void Play();

        Task SeekTo(TimeSpan position, CancellationToken cancellationToken);
    }

    public class MediaElementWrapper : IMediaElementWrapper
    {
        private readonly IMediaElement _mediaElement;

        public MediaElementWrapper(IMediaElement mediaElement)
        {
            _mediaElement = mediaElement;
        }

        public MediaSource? Source
        {
            get => _mediaElement.Source;
            set => _mediaElement.Source = value;
        }

        public MediaElementState CurrentState => _mediaElement.CurrentState;

        public void Play()
        {
            _mediaElement.Play();
        }

        public Task SeekTo(TimeSpan position, CancellationToken cancellationToken)
        {
            return _mediaElement.SeekTo(position, cancellationToken);
        }
    }
}