using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections;
using System.Collections.Generic;

namespace Prototype.NetworkLobby
{
    public class LobbyManager : NetworkLobbyManager
    {
        static short MsgKicked = MsgType.Highest + 1;

        static public LobbyManager s_Singleton;


        [Header("Unity UI Lobby")]
        [Tooltip("Time in second between all players ready & match start")]
        public float prematchCountdown = 5.0f;

        [Space]
        [Header("UI Reference")]
        public LobbyTopPanel topPanel;

        public RectTransform mainMenuPanel;
        public RectTransform lobbyPanel;

        public LobbyInfoPanel infoPanel;
        public LobbyCountdownPanel countdownPanel;
        public GameObject addPlayerButton;

        protected RectTransform currentPanel;

        public Button backButton;

        public Text statusInfo;
        public Text hostInfo;
        public NetworkConnection NetCon;
        //Client numPlayers from NetworkManager is always 0, so we count (throught connect/destroy in LobbyPlayer) the number
        //of players, so that even client know how many player there is.
        [HideInInspector]
        public int _playerNumber = 0;

        //used to disconnect a client properly when exiting the matchmaker
        [HideInInspector]
        public bool _isMatchmaking = false;

        protected bool _disconnectServer = false;

        protected ulong _currentMatchID;

        protected LobbyHook _lobbyHooks;
       
       
        
        void Start()
        {
            s_Singleton = this;
            _lobbyHooks = GetComponent<Prototype.NetworkLobby.LobbyHook>();
            currentPanel = mainMenuPanel;

            backButton.gameObject.SetActive(false);
            GetComponent<Canvas>().enabled = true;

            DontDestroyOnLoad(gameObject);

            SetServerInfo("Offline", "None");
            


        }

        public override void OnLobbyClientSceneChanged(NetworkConnection conn)
        {
            if (SceneManager.GetSceneAt(0).name == lobbyScene)
            {
                if (topPanel.isInGame)
                {
                    ChangeTo(lobbyPanel);
                    if (_isMatchmaking)
                    {
                        if (conn.playerControllers[0].unetView.isServer)
                        {
                            backDelegate = StopHostClbk;
                        }
                        else
                        {
                            backDelegate = StopClientClbk;
                        }
                    }
                    else
                    {
                        if (conn.playerControllers[0].unetView.isClient)
                        {
                            backDelegate = StopHostClbk;
                        }
                        else
                        {
                            backDelegate = StopClientClbk;
                        }
                    }
                }
                else
                {
                    ChangeTo(mainMenuPanel);
                }

                topPanel.ToggleVisibility(true);
                topPanel.isInGame = false;
            }
            else
            {
                ChangeTo(null);

                Destroy(GameObject.Find("MainMenuUI(Clone)"));

                //backDelegate = StopGameClbk;
                topPanel.isInGame = true;
                topPanel.ToggleVisibility(false);
            }
        }

        public void ChangeTo(RectTransform newPanel)
        {
            if (currentPanel != null)
            {
                currentPanel.gameObject.SetActive(false);
            }

            if (newPanel != null)
            {
                newPanel.gameObject.SetActive(true);
            }

            currentPanel = newPanel;

            if (currentPanel != mainMenuPanel)
            {
                backButton.gameObject.SetActive(true);
            }
            else
            {
                backButton.gameObject.SetActive(false);
                SetServerInfo("Offline", "None");
                _isMatchmaking = false;
            }
        }

        public void DisplayIsConnecting()
        {
            var _this = this;
            infoPanel.Display("Connecting...", "Cancel", () => { _this.backDelegate(); });
        }

        public void SetServerInfo(string status, string host)
        {
            statusInfo.text = status;
            hostInfo.text = host;
           
        }


        public delegate void BackButtonDelegate();
        public BackButtonDelegate backDelegate;
        public void GoBackButton()
        {
            backDelegate();
            topPanel.isInGame = false;
        }

        // ----------------- Server management

        public void AddLocalPlayer()
        {
            TryToAddPlayer();
        }

        public void RemovePlayer(LobbyPlayer player)
        {
            player.RemovePlayer();
        }

        public void SimpleBackClbk()
        {
            ChangeTo(mainMenuPanel);
        }

        public void StopHostClbk()
        {
            if (_isMatchmaking)
            {
                matchMaker.DestroyMatch((NetworkID)_currentMatchID, 0, OnDestroyMatch);
                _disconnectServer = true;
            }
            else
            {
                StopHost();
            }


            ChangeTo(mainMenuPanel);
        }

        public void StopClientClbk()
        {
            StopClient();

            if (_isMatchmaking)
            {
                StopMatchMaker();
            }

            ChangeTo(mainMenuPanel);
        }

        public void StopServerClbk()
        {
            StopServer();
            ChangeTo(mainMenuPanel);
        }

        class KickMsg : MessageBase { }
        public void KickPlayer(NetworkConnection conn)
        {
            conn.Send(MsgKicked, new KickMsg());
        }




        public void KickedMessageHandler(NetworkMessage netMsg)
        {
            infoPanel.Display("Kicked by Server", "Close", null);
            netMsg.conn.Disconnect();
        }

        //===================

        public override void OnStartHost()
        {
            base.OnStartHost();

            ChangeTo(lobbyPanel);
            backDelegate = StopHostClbk;
            SetServerInfo("Hosting", networkAddress);
        }

        public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            base.OnMatchCreate(success, extendedInfo, matchInfo);
            _currentMatchID = (System.UInt64)matchInfo.networkId;
        
        }

        public override void OnDestroyMatch(bool success, string extendedInfo)
        {
            base.OnDestroyMatch(success, extendedInfo);
            if (_disconnectServer)
            {
                StopMatchMaker();
                StopHost();
            }
        }

        //allow to handle the (+) button to add/remove player
        public void OnPlayersNumberModified(int count)
        {
            _playerNumber += count;

            int localPlayerCount = 0;
            foreach (PlayerController p in ClientScene.localPlayers)
                localPlayerCount += (p == null || p.playerControllerId == -1) ? 0 : 1;

            addPlayerButton.SetActive(localPlayerCount < maxPlayersPerConnection && _playerNumber < maxPlayers);
        }

        // ----------------- Server callbacks ------------------

        //we want to disable the button JOIN if we don't have enough player
        //But OnLobbyClientConnect isn't called on hosting player. So we override the lobbyPlayer creation

        public Dictionary<int, int> currentPlayers = new Dictionary<int, int>();
        public Dictionary<int, int> playerTeam = new Dictionary<int, int>();
        public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
        {
            print("ssss");
            if (!currentPlayers.ContainsKey(conn.connectionId))
                currentPlayers.Add(conn.connectionId, 0);
            if (!playerTeam.ContainsKey(conn.connectionId))
                playerTeam.Add(conn.connectionId, 0);

            return base.OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
        }
        public void SetPlayerTeam(NetworkConnection conn, int _type)
        {
            if (conn == null)
            {
                print("conn null");
                return;
            }

            if (playerTeam.ContainsKey(conn.connectionId))
            {
                playerTeam[conn.connectionId] = _type;
            }
            else
            {
                playerTeam.Add(conn.connectionId, _type);
            }
        }
        public void SetPlayerTypeLobby(NetworkConnection conn, int _type)
        {
            if (conn == null)
            {
                print("conn null");
                return;
            }
               
            if (currentPlayers.ContainsKey(conn.connectionId))
            { 
                currentPlayers[conn.connectionId] = _type;
            }
            else
            {
                currentPlayers.Add(conn.connectionId,_type);
            }
        }
        private int NumOfplayers = 0;
        private int NumOfCars = 0;
        string mapName = " ";
        public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
        {
            int index = currentPlayers[conn.connectionId];
            int TeamIndex = playerTeam[conn.connectionId];
            GameObject[] spawnpoints = GameObject.FindGameObjectsWithTag("SpawnPoints1");
            GameObject[] spawnpoints2 = GameObject.FindGameObjectsWithTag("SpawnPoints2");
            GameObject _temp = spawnPrefabs[0];
            if (NumOfplayers == 0 && NumOfCars==0) // if first player to spawn the guns and map
            {
                mapName = getRandomMap();
                if (mapName.Equals("map1"))
                {
                    GameObject temp1 = Instantiate(spawnPrefabs[3], spawnpoints[2].transform.position, spawnpoints[2].transform.rotation);
                    NetworkServer.Spawn(temp1);
                    GameObject temp2 = Instantiate(spawnPrefabs[2], spawnpoints[1].transform.position, spawnpoints[1].transform.rotation);
                    NetworkServer.Spawn(temp2);
                }
                else if (mapName.Equals("map2"))
                {
                    GameObject temp1 = Instantiate(spawnPrefabs[3], spawnpoints2[2].transform.position, spawnpoints2[2].transform.rotation);
                    NetworkServer.Spawn(temp1);
                    GameObject temp2 = Instantiate(spawnPrefabs[2], spawnpoints2[1].transform.position, spawnpoints2[1].transform.rotation);
                    NetworkServer.Spawn(temp2);
                }
            }
            if (index == 1)// spawning the player(driver)
            {
                if (mapName.Equals("map1")) {
                    _temp = Instantiate(spawnPrefabs[index], spawnpoints[0].transform.position, spawnpoints[0].transform.rotation);
                    _temp.GetComponent<CarHealthManager>().Team = TeamIndex + 1;
                }
                else if (mapName.Equals("map2"))
                {
                    _temp = Instantiate(spawnPrefabs[index], spawnpoints2[0].transform.position, spawnpoints2[0].transform.rotation);
                    _temp.GetComponent<CarHealthManager>().Team = TeamIndex + 1;
                }
                NumOfCars++;
            }
            else if (index == 0) //spawning the player(shooter)
            {
                if (mapName.Equals("map1"))
                {
                    _temp = Instantiate(spawnPrefabs[index], spawnpoints[1].transform.position, spawnpoints[1].transform.rotation);
                    _temp.GetComponent<HealthManager>().Team = TeamIndex + 1;
                }
                else if (mapName.Equals("map2"))
                {
                    _temp = Instantiate(spawnPrefabs[index], spawnpoints2[1].transform.position, spawnpoints2[1].transform.rotation);
                    _temp.GetComponent<HealthManager>().Team = TeamIndex + 1;
                }
                NumOfplayers++;
            }
            return _temp;
        }

        public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
        {
            for (int i = 0; i < lobbySlots.Length; ++i)
            {
                LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

                if (p != null)
                {
                    p.RpcUpdateRemoveButton();
                    p.ToggleJoinButton(numPlayers + 1 >= minPlayers);
                }
            }
        }

        public override void OnLobbyServerDisconnect(NetworkConnection conn)
        {
            for (int i = 0; i < lobbySlots.Length; ++i)
            {
                LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

                if (p != null)
                {
                    p.RpcUpdateRemoveButton();
                    p.ToggleJoinButton(numPlayers >= minPlayers);
                }
            }

        }

        public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
        {
            //This hook allows you to apply state data from the lobby-player to the game-player
            //just subclass "LobbyHook" and add it to the lobby object.

            if (_lobbyHooks)
                _lobbyHooks.OnLobbyServerSceneLoadedForPlayer(this, lobbyPlayer, gamePlayer);
            return true;
        }

        // --- Countdown management

        public override void OnLobbyServerPlayersReady()
        {
            bool allready = true;
            for (int i = 0; i < lobbySlots.Length; ++i)
            {
                if (lobbySlots[i] != null)
                    allready &= lobbySlots[i].readyToBegin;
            }

            if (allready)
                StartCoroutine(ServerCountdownCoroutine());
        }

        public IEnumerator ServerCountdownCoroutine()
        {
            float remainingTime = prematchCountdown;
            int floorTime = Mathf.FloorToInt(remainingTime);

            while (remainingTime > 0)
            {
                yield return null;

                remainingTime -= Time.deltaTime;
                int newFloorTime = Mathf.FloorToInt(remainingTime);

                if (newFloorTime != floorTime)
                {//to avoid flooding the network of message, we only send a notice to client when the number of plain seconds change.
                    floorTime = newFloorTime;

                    for (int i = 0; i < lobbySlots.Length; ++i)
                    {
                        if (lobbySlots[i] != null)
                        {//there is maxPlayer slots, so some could be == null, need to test it before accessing!
                            (lobbySlots[i] as LobbyPlayer).RpcUpdateCountdown(floorTime);
                        }
                    }
                }
            }

            for (int i = 0; i < lobbySlots.Length; ++i)
            {
                if (lobbySlots[i] != null)
                {
                    (lobbySlots[i] as LobbyPlayer).RpcUpdateCountdown(0);
                }
            }

            ServerChangeScene(playScene);
        }

        // ----------------- Client callbacks ------------------

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            infoPanel.gameObject.SetActive(false);

            conn.RegisterHandler(MsgKicked, KickedMessageHandler);

            if (!NetworkServer.active)
            {//only to do on pure client (not self hosting client)
                ChangeTo(lobbyPanel);
                backDelegate = StopClientClbk;
                SetServerInfo("Client", networkAddress);
            }
        }


        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            ChangeTo(mainMenuPanel);
        }

        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            ChangeTo(mainMenuPanel);
            infoPanel.Display("Client error : " + (errorCode == 6 ? "timeout" : errorCode.ToString()), "Close", null);
        }

        public string  getRandomMap()
        {
            GameObject[] myArray = new GameObject[2];
            myArray[0] = Resources.Load("map1") as GameObject;
            myArray[1] = Resources.Load("map2") as GameObject;
            GameObject element = myArray[Random.Range(0, myArray.Length)] as GameObject;
            GameObject temp3 = Instantiate(element);
            NetworkServer.Spawn(temp3);
            return element.name;


        }
    }

}

