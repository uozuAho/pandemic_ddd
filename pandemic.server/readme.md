# Pandemic game server

Hosts a game of Pandemic, allowing clients to connect & play via ZeroMQ. Aimed
at being compatible with [OpenSpiel](https://github.com/deepmind/open_spiel),
allowing its algorithms to be used to play Pandemic.

```sh
# run the server
dotnet run tcp://*:5555
```

See https://github.com/uozuAho/open_spiel_playground/blob/main/zmq_ttt/client.py
for an example of how to play the game with a remote client.
