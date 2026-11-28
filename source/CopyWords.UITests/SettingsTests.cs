
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Support.UI;

namespace CopyWords.UITests;

[TestFixture]
[NonParallelizable]
public class SettingsTests
{
    private WindowsDriver _driver = null!;

    [SetUp]
    public void Setup()
    {
        var options = new AppiumOptions
        {
            PlatformName = "Windows",
            AutomationName = "Wincore",
            App = "8B3600EB-E6AD-4B34-94F9-CAB2540E8547_0be70sw268zy8!App"
        };

        options.AddAdditionalAppiumOption("ms:waitForAppLaunch", 10);

        _driver = new WindowsDriver(
            new Uri("http://127.0.0.1:4723/"),
            options);

        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
    }

    [TearDown]
    public void TearDown()
    {
        _driver?.Dispose();
    }

    [Test]
    public void SettingsDialog_CanBeOpened()
    {
        _driver.FindElement(
            MobileBy.AccessibilityId("SettingsToolbarItem"))
            .Click();

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        var settingsTitle = wait.Until(driver =>
        {
            var element = driver.FindElement(MobileBy.AccessibilityId("SettingsTitle"));

            return element.Displayed ? element : null;
        });

        Assert.That(settingsTitle!.Displayed, Is.True);
    }
}
