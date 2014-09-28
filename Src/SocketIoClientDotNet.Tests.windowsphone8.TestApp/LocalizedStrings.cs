using SocketIoClientDotNet.Tests.windowsphone8.TestApp.Resources;

namespace SocketIoClientDotNet.Tests.windowsphone8.TestApp
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources _localizedResources = new AppResources();

        public AppResources LocalizedResources { get { return _localizedResources; } }
    }
}