using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;
// a class to pass player input from the lobby into the game .
public class NetworkLobbyHook : LobbyHook {

    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
       LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        if (gamePlayer.GetComponent<HealthManager>())
        {
            HealthManager _player = gamePlayer.GetComponent<HealthManager>();

            _player.Team = lobby.TeamValue+1;
            _player.nickName = lobby.playerName;
            _player.Color = lobby.playerColor;
            _player.Numofplayers = LobbyPlayerList._instance._players.Count;
        }
        else if(gamePlayer.GetComponent<CarHealthManager>())
        {
            CarHealthManager _player = gamePlayer.GetComponent<CarHealthManager>();
            _player.Team = lobby.TeamValue + 1;
            _player.nickName = lobby.playerName;
            _player.Color = lobby.playerColor;
            _player.Numofplayers = LobbyPlayerList._instance._players.Count;
        }
    }
}
