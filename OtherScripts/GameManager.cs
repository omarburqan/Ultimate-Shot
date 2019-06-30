using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
// a class which manage the game which include a static attribute in order to access it from every where
public class GameManager : MonoBehaviour {
    public static GameManager instance = null;
    private GameObject[] Players;
    private GameObject[] Drivers;
    // saving the players componenets in a dictionaries in order to get when needed
    private static Dictionary<string, HealthManager> players = new Dictionary<string, HealthManager>(); 
    private static Dictionary<string, CarHealthManager> drivers = new Dictionary<string, CarHealthManager>();
    private static Dictionary<string, StatusManager> Allplayers = new Dictionary<string, StatusManager>();
    public Text WinText;
    public Text LoseText;
    public Text FirstPlaceText;
    public Text SecondPlaceText;
    public Text ThirdPlaceText;
    public bool stPlace;
    public bool ndPlace;
    public bool rdPlace;
    
    


    // Use this for initialization
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        stPlace = false;
        ndPlace = false;
        rdPlace = false;
    }
    /************ these methods(register,unregister,get,set,getall,...) handle the players registertion to the dictionaries  ************/
    public void RegisterDriver(string _PlayerID, CarHealthManager _player , StatusManager playerStatus)
    {
        if (drivers.ContainsKey(_PlayerID))
        {
            drivers[_PlayerID] = _player;
            Allplayers[_PlayerID] = playerStatus;
        }
        else
        {
            drivers.Add(_PlayerID, _player);
            Allplayers.Add(_PlayerID, playerStatus);
        }
        
    }
    public void UnRegisterDriver(string Name)
    {
        if(drivers!=null)
            drivers.Remove(Name);
    }
    public void RegisterPlayer(string _PlayerID, HealthManager _player, StatusManager playerStatus)
    {
        if (players.ContainsKey(_PlayerID))
        {
            players[_PlayerID] = _player;
            Allplayers[_PlayerID] = playerStatus;
        }
        else
        {
            players.Add(_PlayerID, _player);
            Allplayers.Add(_PlayerID, playerStatus);
        }
    }
    public void UnRegisterPlayer(string Name)
    {
        if(players!=null)
            players.Remove(Name);
    }
    public HealthManager getPlayer(string Name)
    {
        if (players[Name])
        {
            return players[Name];
        }
        return null;
    }
    public CarHealthManager getDriver(string Name)
    {
        return drivers[Name];
    }
    public HealthManager[] getallPlayers()
    {
        return players.Values.ToArray();
    }
    public CarHealthManager[] getallDrivers()
    {
        return drivers.Values.ToArray();
    }
    public StatusManager[] getAll()
    {
        return Allplayers.Values.ToArray();
    }
    public StatusManager getStatus(string Name)
    {
        if (Name == null)
            return null;
            if (Allplayers[Name])
            {
                return Allplayers[Name];
            }
            return null;
    }
    public  Dictionary<string,StatusManager>.KeyCollection getkeys()
    {
        return Allplayers.Keys;
    }
    /**********************************************************************************************/
    // re init the places for each level
    public void changeLap()
    {
        stPlace = false;
        ndPlace = false;
        rdPlace = false;

    }
    // display lose or win text
    public void Lose() {
        LoseText.gameObject.SetActive(true);
        StartCoroutine(hideText(LoseText));
    }
    public void Win()
    {
        WinText.gameObject.SetActive(true);
        StartCoroutine(hideText(LoseText));
    }
    // display 1st,2nd,3rd canvas when finish the race
    public string  DriverWinner()
    {
        string Special=" ";
        if (!stPlace)
        {
            FirstPlaceText.gameObject.SetActive(true);
            Special= "1stplace";
            StartCoroutine(hideText(FirstPlaceText));
        }
        else if (!ndPlace)
        {
            SecondPlaceText.gameObject.SetActive(true);
            Special = "2ndplace";
            StartCoroutine(hideText(SecondPlaceText));
        }
        else if (!rdPlace)
        {
            ThirdPlaceText.gameObject.SetActive(true);
            Special = "3rdplace";
            StartCoroutine(hideText(ThirdPlaceText));
        }
        return Special ;
    }
    // delay to hide the texts after special time
    IEnumerator hideText(Text text)
    {
        yield return new WaitForSeconds(5);
        text.gameObject.SetActive(false);
    }
    // when disconnection
    private void OnDestroy()
    {
        drivers.Clear();
        players.Clear();
        Allplayers.Clear();
    }
}
