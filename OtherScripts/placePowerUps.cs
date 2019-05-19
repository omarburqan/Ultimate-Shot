using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class placePowerUps : NetworkBehaviour
{
    Transform T;
    MeshCollider MC;
    public GameObject[] powerUps;
    public int powerUpsCount = 5;
    Vector3[] verts;
    // Use this for initialization

    public void placePowerUp()
    {
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
    }
}
