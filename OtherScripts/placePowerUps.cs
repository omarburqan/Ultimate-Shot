using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
// this class manage spawning the powers up when changing from one level to other
public class placePowerUps : NetworkBehaviour
{
    Transform T;
    MeshCollider MC;
    public GameObject[] powerUps;
    private int powerUpsCount;
    Vector3[] verts;

    public void placePowerUp()
    {
        powerUpsCount = 400;
        T = GetComponent<CheckpointCheck>().getCol();
        MC = T.gameObject.GetComponent<MeshCollider>();
        Mesh M = MC.sharedMesh;
        verts = M.vertices;

        //in local space
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i].x *= T.transform.localScale.x;
            verts[i].y *= T.transform.localScale.y;
            verts[i].z *= T.transform.localScale.z;
        }
        //now in global space
        for (int i = 0; i < powerUpsCount; i++)
        {
            int randomint = Random.Range(0, verts.Length / powerUpsCount / 10);
            randomint -= verts.Length / powerUpsCount / 10 / 2;
            int randompu = Random.Range(0, powerUps.Length);
            if (isServer)
                RpcSpawn( randompu , verts[(i + 1) * verts.Length / (powerUpsCount + 2) + randomint]);
            else
                CmdSpawn(randompu, verts[(i + 1) * verts.Length / (powerUpsCount + 2) + randomint]);
        }
    }
    [Command]
    void CmdSpawn(int randompu,Vector3 verts)
    {
        RpcSpawn(randompu,verts);
    }
    [ClientRpc]
    void RpcSpawn(int randompu,Vector3 verts)
    {
        Spawn(randompu,verts);
    }
    void Spawn(int randompu, Vector3 verts)
    {
        GameObject go2 = Instantiate(powerUps[randompu], verts, Quaternion.identity);
        go2.transform.SetParent(GameObject.FindGameObjectWithTag("Map#").transform);
    }
    // spawing guns when changing levels(maps)
    public void placeWeapons()
    {
        List<GameObject> spawnPoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("SpawnPints3S"));
        spawnPoints.Sort(CompareListByName);
        
    }









    private static int CompareListByName(GameObject i1, GameObject i2)
    {
        return i1.name.CompareTo(i2.name);
    }
}
