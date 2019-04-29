using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class placePowerUps : MonoBehaviour {
    Transform T;
    MeshCollider MC;
    public GameObject[] powerUps;
    public int powerUpsCount = 5;
    // Use this for initialization
    void Start () {
         T = transform.Find("Collision").gameObject.transform;
        MC = T.gameObject.GetComponent<MeshCollider>();
        Mesh M = MC.sharedMesh;
        Vector3[] verts = M.vertices;//in local space
        Vector3[] v;
       for(int i=0; i<verts.Length; i++)
       {
            verts[i].x *= T.transform.localScale.x;
            verts[i].y *= T.transform.localScale.y;
            verts[i].z *= T.transform.localScale.z;



        }//now in global space




        for(int i=0; i < powerUpsCount; i++)
        {

            int randomint = Random.Range(0, verts.Length/powerUpsCount/10);
            randomint -= verts.Length / powerUpsCount / 10 / 2;

            int randompu = Random.Range(0, powerUps.Length);
            GameObject go2 = Instantiate(powerUps[randompu], verts[(i+1)* verts.Length/ (powerUpsCount+2)+randomint], Quaternion.identity);

        }

       // GameObject go = Instantiate(powerUps[0], randompoint, Quaternion.identity);


    }

	
}
