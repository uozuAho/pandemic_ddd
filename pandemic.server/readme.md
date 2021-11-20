# Pandemic game server

Hosts a game of Pandemic, allowing clients to connect & play via ZeroMQ. Aimed
at being compatible with [OpenSpiel](https://github.com/deepmind/open_spiel),
allowing its algorithms to be used to play Pandemic.

```sh
dotnet run tcp://*:5555
```
