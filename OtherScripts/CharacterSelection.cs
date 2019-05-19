using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelection : MonoBehaviour {
    public static CharacterSelection instance = null;
    private GameObject[] characterList;
    private int index;
    public GameObject gameObj;
	// Use this for initialization
	void Start () {        
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        index = 0;
        // fill the array with models
        characterList = new GameObject[transform.childCount];
        for (int i=0; i < transform.childCount; i++)
        {
            characterList[i] = transform.GetChild(i).gameObject;
        }
        // we toggle off their rendere
        foreach (GameObject go in characterList)
        {
            go.SetActive(false);
        }
        if (characterList[index])
            characterList[index].SetActive(true);
       
    }
	
	public void toggleLeft()
    {
        characterList[index].SetActive(false);
        index--;
        if(index < 0)
        {
            index = characterList.Length - 1;
        }
        characterList[index].SetActive(true);
    }
    public void toggleRight()
    {
        characterList[index].SetActive(false);
        index++;
        if (index > characterList.Length-1)
        {
            index = 0;
        }
        characterList[index].SetActive(true);
    }
    public void Submit()
    {
        gameObj.SetActive(true);
    }
    public int getIndex()
    {
        return this.index;
    }

}
