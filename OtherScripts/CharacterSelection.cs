using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour {
    public static CharacterSelection instance = null;
    public GameObject car;
    public Sprite[] carImages;
    private int index;
    public GameObject gameObj;
    int NumberOfCars = 3;
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
    }
	
	public void toggleLeft()
    {
        index--;
        if(index < 0)
        {
            index = carImages.Length - 1;
        }
        car.GetComponent<Image>().sprite = carImages[index];
    }
    public void toggleRight()
    {
        
        index++;
       
        car.GetComponent<Image>().sprite = carImages[index%NumberOfCars]; 
    }
    public void Submit()
    {
        print("sssssshimsssssss");
        gameObj.SetActive(true);
    }
    public int getIndex()
    {
        return this.index;
    }

}
