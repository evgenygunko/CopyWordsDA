using System;
using System.Collections.Generic;
using System.Text;

namespace CopyWords.UITests.Shared;

public static class AppiumServerHelper
{
    public const string DefaultHostAddress = "127.0.0.1";
    public const int DefaultHostPort = 4723;

    public static AppiumLocalService StartAppiumLocalServer(string host = DefaultHostAddress,
        int port = DefaultHostPort)
    {
        var appiumOptions = new AppiumOptions();

        if (Debugger.IsAttached)
        {
            // How long (in seconds) the driver should wait for a new command
            // from the client before assuming the client has stopped sending requests. After the timeout
            // the session is going to be deleted. 60 seconds by default.
            // Setting it to zero disables the timer.
            //
            appiumOptions.AddAdditionalAppiumOption("newCommandTimeout",
                TimeSpan.FromMinutes(4).TotalSeconds.ToString());
        }

        var builder = new AppiumServiceBuilder()
            .WithLogFile(new FileInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".appium",
                "log.log")))
            .WithArguments(new OptionCollector()
                .AddCapabilities(appiumOptions))
            .WithIPAddress(host)
            .UsingPort(port);

        // Start the server with the builder
        var service = builder.Build();
        service.Start();
        return service;
    }

    public static void DisposeAppiumLocalServer(AppiumLocalService? service)
    {
        service?.Dispose();
    }
}