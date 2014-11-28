# SocketIoClientDotNet
====================

Socket.IO Client Library for .Net

This is the Socket.IO Client Library for C#, which is ported from the [JavaScript client](https://github.com/Automattic/socket.io-client).

See also: [EngineIoClientDotNet](https://github.com/Quobject/EngineIoClientDotNet)

## Installation
[Nuget install](https://www.nuget.org/packages/SocketIoClientDotNet/):
```
Install-Package SocketIoClientDotNet
```

## Usage
SocketIoClientDotNet has a similar api to those of the [JavaScript client](https://github.com/Automattic/socket.io-client).

```cs
using Quobject.SocketIoClientDotNet.Client;

var socket = IO.Socket("http://localhost");
socket.On(Socket.EVENT_CONNECT, () =>
{
	socket.Emit("hi");
	socket.On("hi", (data) =>
	{
		Console.WriteLine(data);
		socket.Disconnect();
	});
});
Console.ReadLine();
```

More examples can be found in [unit tests](https://github.com/Quobject/SocketIoClientDotNet/blob/master/Src/SocketIoClientDotNet.Tests.net45/ClientTests/ServerConnectionTest.cs) acting against the [test server](https://github.com/Quobject/SocketIoClientDotNet/blob/master/TestServer/server.js).

## Features
This library supports all of the features the JS client does, including events, options and upgrading transport.

## Framework Versions
.Net Framework 3.5, .Net Framework 4.5, Windows 8, Windows 8.1, Windows Phone 8, Windows Phone 8.1, Mono, Unity


## Support me

If you like this project you may support me by donating something, starring this repository or reporting bugs and ideas in the issue section.

[![Donate PayPal](pics/donate-paypal.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=UDX488Y3Y4K36)
[![Donate Bitcoins](pics/donate-bitcoins.png)](https://coinbase.com/checkouts/621810df8d49a896e170dd5d5bd28a73)
[![Report issue](pics/issues.png)](https://github.com/Quobject/SocketIoClientDotNet/issues/new)


## License

[MIT](http://opensource.org/licenses/MIT)
