using AutoFixture;
using AutoFixture.AutoMoq;
using CopyWords.Core.Services;
using Moq;

namespace CopyWords.Core.Tests
{
    public static class FixtureFactory
    {
        public static Fixture CreateFixture()
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization());

            return fixture;
        }

        /*public static void EnableDownloadSoundFromTranslationApp(this IFixture fixture, bool enable)
        {
            var launchDarklyServiceMock = fixture.Freeze<Mock<ILaunchDarklyService>>();
            launchDarklyServiceMock
                .Setup(x => x.GetBooleanFlag("download-sound-from-translation-app", It.IsAny<bool>()))
                .Returns(enable);
        }*/
    }
}
