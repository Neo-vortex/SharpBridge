
# Sharp Bridge

A .NET implemation of WalletConnect Bridge (https://walletconnect.com/), designed to be highly responsive.
It is using ASP.NET Core websocket.



## Motivation
Motivation
My motiviation for writing this implementation is that the official server (wss://bridge.walletconnect.org) does not seem consistently reliable. Messages are often only dispatched with a considerable delay and sometimes disappear entirely. While part of that can probably be attributed to a high load, part of that is probably also attributable to some flaws in the implementation.
## Running the bridge
### Manual
You will need dotnet 7 to run the Bridge (https://dotnet.microsoft.com/en-us/download/dotnet/7.0), 
Running the bridge should be pretty easy. just use ```dotnet run``` command and its done!. You will get an output like this : 
```csharp
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5296
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /home/neo/Personal/SharpBridge/SharpBridge

```
Then you can connect to the bridge with this url ```http://[IP]:[Port]/v1/ws```
### Docker
There is also a dockerfile availble for this project, you can simple build and run the dockerfile, bridge will be availble at ```http://[IP]:5001/v1/ws```

## Documentation
WalletConnect protocol for version 1 is pretty simple. It is a pub/sub pattern with ack message after each part recives the packet.
These Notes are important while using this implemation.
### Lifetime
Messages are kept alive for two minutes if not deliverd rightaway. you can change this lifetime in the code as you want.
### Multi sub/pub support
This implementation supports multiple sub/pub per Topic. Unlike some other implementation like [This](https://github.com/aktionariat/walletconnect-bridge)

## Features

- Multithreded and thred-safe implemation.
- All in-memory operation (no disk activity or database connection)
- Responsive
- Cross platform
- Flexable (you can change messages lifetime and ...)



## Roadmap

- Support Version 2 of WalletConnect

- Add optinal Redis database option to keep the heap small

- Faster JSON serilize and deserilize with source generation in C#

- Writing Tests

## License

[MIT](https://choosealicense.com/licenses/mit/)


## Tech Stack


**Server:** dotnet 7, C#


## Feedback

If you have any feedback, please reach out to us at neo.vortex@pm.me


## Related

Here are some related projects

[Java implemation of  WalletConnect protocol](https://github.com/matiassingers/awesome-readme)


## Used By

This project is used by the following companies:

- http://soter.chainovastudio.com/


## Contributing

Contributions are always welcome!


## API Reference

#### Connect to WalletConnect Version 1

```http
  GET /v1/ws (WebSocket connection)
```
#### respecting https://docs.walletconnect.com/1.0/bridge-server#test-hello-world

```http
  GET /v1/hello/
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `-`      | `-` | returns version and bridge name |


