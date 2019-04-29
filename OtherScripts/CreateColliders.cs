using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateColliders : MonoBehaviour {
    public GameObject[] collidersToAdd;
	// Use this for initialization
	void Start () {
        if (collidersToAdd.Length == 0)
        {
            collidersToAdd = new GameObject[3];
            collidersToAdd[0] = transform.Find("Cliffs").gameObject;
            collidersToAdd[1] = transform.Find("Ground").gameObject;
            collidersToAdd[2] = transform.Find("Repentances").gameObject;
           // collidersToAdd[2] = transform.Find("Road").gameObject;
        }
        addColliders(collidersToAdd);
		
	}

    private void addColliders(GameObject[] collidersToAdd)
    {
        foreach(GameObject parent in collidersToAdd)
        {//add collideers to each child of each parent
            for (int i=0;i< parent.transform.childCount; i++)
            {
                GameObject child = parent.transform.GetChild(i).gameObject;
                if (!child)
                {
                    Debug.Log("cant find childern of "+ parent.name);
                    continue;
                }
                MeshFilter MF = child.GetComponent<MeshFilter>();
                if (!MF) 
                {
                    Debug.Log("cant find mesh filter on child " + child.name);

                }

                MeshCollider myMC = child.gameObject.AddComponent<MeshCollider>();

                myMC.sharedMesh = MF.mesh;

 

            }

        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
