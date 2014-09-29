# SocketIoClientDotNet
====================

Socket.IO Client Library for .Net

This is the Socket.IO v1.0 Client Library for C#, which is ported from the [JavaScript client](https://github.com/Automattic/socket.io-client).

See also: [EngineIoClientDotNet](https://github.com/Quobject/EngineIoClientDotNet)

## Installation
Nuget install:
```
Install-Package SocketIoClientDotNet
```

## Usage
SocketIoClientDotNet has a similar api to those of the [JavaScript client](https://github.com/Automattic/socket.io-client).

```cs
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


## Features
This library supports all of the features the JS client does, including events, options and upgrading transport.

## Framework Versions
.Net Framework 4.5, Windows 8, Windows 8.1, Windows Phone 8, Windows Phone 8.1, Mono


# Support me

If you like this project you may support me by donating something, starring this repository or reporting bugs and ideas in the issue section.

[![Donate Bitcoins](pics/donate-bitcoins.png)](https://coinbase.com/checkouts/621810df8d49a896e170dd5d5bd28a73)


## License

MIT
