using System.Reflection;
using Foundation;
using UIKit;

using Xunit.Runner;
using Xunit.Sdk;


namespace SocketIoClientDotNet.Tests.XamariniOS.TestApp
{
 // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : RunnerAppDelegate
    {
        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // We need this to ensure the execution assembly is part of the app bundle
            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);

            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());

            return base.FinishedLaunching(application, launchOptions);
        }
    }

}

