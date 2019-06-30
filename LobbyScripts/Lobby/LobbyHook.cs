using UnityEngine;
using UnityEngine.Networking;
using System.Collections;


// a hook which send a special data after spawning the players which these data will be used in game (Nickname,color,team,....)
namespace Prototype.NetworkLobby
{
    // Subclass this and redefine the function you want
    // then add it to the lobby prefab
    public abstract class LobbyHook : MonoBehaviour
    {
        public virtual void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer) { }
    }

}
