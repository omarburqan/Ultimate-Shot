using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager instance = null;
    private GameObject[] Players;
    private GameObject[] Drivers;
    private static Dictionary<string, HealthManager> players = new Dictionary<string, HealthManager>();
    private static Dictionary<string, CarHealthManager> drivers = new Dictionary<string, CarHealthManager>();

    public Text WinText;
    public Text LoseText;
    public Text FirstPlaceText;
    public Text SecondPlaceText;
    public Text ThirdPlaceText;
    public GameObject[] Laps;
    public GameObject position;
    public bool stPlace;
    public bool ndPlace;
    public bool rdPlace;
    public delegate void OnPlayerKilledCallback(string player,string source);
    public OnPlayerKilledCallback onPlayerKilledCallback;
    


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
    /********************************************/
    public void RegisterDriver(string _PlayerID, CarHealthManager _player)
    {
        if (drivers.ContainsKey(_PlayerID))
        {
            drivers[_PlayerID] = _player;
        }
        else
        {
            drivers.Add(_PlayerID, _player);
        }
        
    }
    public void UnRegisterDriver(string Name)
    {
        if(drivers!=null)
            drivers.Remove(Name);
    }
    public void RegisterPlayer(string _PlayerID, HealthManager _player)
    {
        if (players.ContainsKey(_PlayerID))
        {
            players[_PlayerID] = _player;
        }
        else
        {
            players.Add(_PlayerID, _player);
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
    /******************************************************/
    public bool changeLap()
    {
        bool Finished = false;
        if (GameObject.FindGameObjectWithTag("MAP1"))
        {
            instance.Laps[0].SetActive(false);
            if (Laps[1].activeInHierarchy == false) { // lap 2 will start 
                instance.Laps[1].SetActive(true);
            } 
            else // finish of the game 
            {
                Finished = true;
            }
        }
        else if (GameObject.FindGameObjectWithTag("MAP2"))
        {
            instance.Laps[2].SetActive(false);
            if (Laps[3].activeInHierarchy == false) // lap 2 will start 
                instance.Laps[3].SetActive(true);
            else // finish of the game 
            {
                Finished = true;
            }
        }
        stPlace = false;
        ndPlace = false;
        rdPlace = false;
        return Finished;

    }
    public void Lose() {
        LoseText.gameObject.SetActive(true);
        StartCoroutine(hideText(LoseText));
    }
    public void Win()
    {
        WinText.gameObject.SetActive(true);
        StartCoroutine(hideText(LoseText));
    }
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
    IEnumerator hideText(Text text)
    {
        yield return new WaitForSeconds(5);
        text.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        print("SSS");
        drivers.Clear();
        players.Clear();
    }
}
