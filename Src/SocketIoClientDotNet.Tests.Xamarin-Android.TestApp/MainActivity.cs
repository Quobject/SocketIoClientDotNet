using System.Reflection;
using Android.App;
using Android.OS;
using Xunit.Runners.UI;
using Xunit.Sdk;

namespace EngineIoClientDotNet.Tests.Xamarin_Android
{
	[Activity(Label = "SocketIoClientDotNet.Tests.Xamarin_Android", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : RunnerActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			// tests can be inside the main assembly
			AddTestAssembly(Assembly.GetExecutingAssembly());

			AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);
			// or in any reference assemblies			

			//AddTestAssembly(typeof(PortableTests).Assembly);
			// or in any assembly that you load (since JIT is available)

			// you cannot add more assemblies once calling base
			base.OnCreate(bundle);
		}
	}
}
