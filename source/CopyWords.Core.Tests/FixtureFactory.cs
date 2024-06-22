using AutoFixture;
using AutoFixture.AutoMoq;

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
    }
}
