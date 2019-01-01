using UnityEngine;
using UnityEngine.Networking;
// This is a template script for in-game object health manager.
// Any in-game entity that reacts to a shot must have this script with the public function TakeDamage().
public class HealthManager : NetworkBehaviour
{
    private float Healtpoints;
    private NetworkAnimator anim; 
    private void Start()
    {
        if (!isLocalPlayer)
            return;
        this.Healtpoints = 100;
        anim = GetComponent<NetworkAnimator>();
    }
    // This is the mandatory function that receives damage from shots.
    // You may remove the 'virtual' keyword before coding the content.
  
    private void OnParticleCollision(GameObject other)
    {
        if (!isLocalPlayer)
            return;
        if(other.gameObject.tag == "M4A1")
        {
            print("Hit");
            this.Healtpoints -= 25;
            if (Healtpoints < 1)
            {
                anim.SetTrigger("Death");
                this.GetComponent<CapsuleCollider>().direction = 0;
                if (isServer)
                {
                    RpcDie();
                }
                else
                {
                    CmdDie();
                }
            }
        }
    }
    [Command]
    void CmdDie()
    {
        if (isServer)
        {
            Die();
        }
        RpcDie();
    }
    [ClientRpc]
    void RpcDie()
    {
        if (!isLocalPlayer && !isServer)
        {
            Die();
        }
    }
    void Die()
    {
        this.GetComponent<CapsuleCollider>().direction = 0;
    }
    
}
