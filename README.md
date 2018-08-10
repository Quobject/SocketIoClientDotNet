# SocketIoClientDotNet
====================

Socket.IO Client Library for .Net

* NuGet Package: [![SocketIoClientDotNet](https://img.shields.io/nuget/v/SocketIoClientDotNet.svg?maxAge=2592000)](https://www.nuget.org/packages/SocketIoClientDotNet/)

This is the Socket.IO Client Library for C#, which is ported from the [JavaScript client](https://github.com/Automattic/socket.io-client) version [1.1.0](https://github.com/socketio/socket.io-client/releases/tag/1.1.0).

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
	
});

socket.On("hi", (data) =>
	{
		Console.WriteLine(data);
		socket.Disconnect();
	});
Console.ReadLine();
```

More examples can be found in [unit tests](https://github.com/Quobject/SocketIoClientDotNet/blob/master/Src/SocketIoClientDotNet.Tests.net45/ClientTests/ServerConnectionTest.cs) acting against the [test server](https://github.com/Quobject/SocketIoClientDotNet/blob/master/TestServer/server.js).

## Features
This library supports all of the features the JS client does, including events, options and upgrading transport.

## Framework Versions
.NETFramework v3.5, .NETFramework v4.0, .NETFramework v4.5

## N.B.

This library works. There are reports of incidents where it crashes (though I was never able to reproduce them). Use this code in a responsible manner and plan for failures, because [Everything fails all the time](https://thenextweb.com/2008/04/04/werner-vogels-everything-fails-all-the-time/).

## License

[MIT](http://opensource.org/licenses/MIT)
