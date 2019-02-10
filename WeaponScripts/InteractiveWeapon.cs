using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
// This class corresponds to any in-game weapon.
[System.Serializable]
public class InteractiveWeapon : NetworkBehaviour
{
    public string label;                                      // The weapon name. Same name will treat weapons as same regardless game object's name.
    public AudioClip shotSound, reloadSound,                  // Audio clips for shoot and reload.
        pickSound, dropSound;                                 // Audio clips for pick and drop weapon.
    public Sprite sprite;                                     // Weapon srpite to show on screen HUD.
    public Vector3 rightHandPosition;                         // Position offsets relative to the player's right hand.
    public Vector3 relativeRotation;                          // Rotation Offsets relative to the player's right hand.
    public float bulletDamage = 10f;                          // Damage of one shot.
    public float recoilAngle;                                 // Angle of weapon recoil.
    public enum WeaponType                                    // Weapon types, related to player's shooting animations.
    {
        NONE,
        SHORT,
        LONG
    }
    public enum WeaponMode                                    // Weapon shooting modes.
    {
        SEMI,
        BURST,
        AUTO
    }
    public WeaponType type = WeaponType.NONE;                 // Default weapon type, change in Inspector.
    public WeaponMode mode = WeaponMode.SEMI;                 // Default weapon mode, change in Inspector.
    public int burstSize = 0;                                 // How many shot are fired on burst mode.
    [SerializeField]
    private int mag, totalBullets;                            // Current mag capacity and total amount of bullets being carried.
    private int fullMag, maxBullets;                          // Default mag capacity and total bullets for reset purposes.
    private GameObject gameController;                // References to the player and the game controller.
    //private ShootBehaviour playerInventory;                   // Player's inventory to store weapons.
    //private SphereCollider interactiveRadius;                 // In-game radius of interaction with player.
    public BoxCollider col;                                  // Weapon collider.
    public Rigidbody rbody;                                  // Weapon rigidbody.
    private WeaponUIManager weaponHud;                        // Reference to on-screen weapon HUD.
    public bool pickable;                                    // Boolean to store whether or not the weapon is pickable (player within radius).
    private Transform pickupHUD;                              // Reference to the weapon pickup in-game label.

    void Start()
    {
        // Set up the references.
        this.gameObject.name = this.label;
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (Transform t in this.transform)
        {
            t.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
        //player = GameObject.FindGameObjectWithTag("Player"); // null because player isn't exist yet;
        //playerInventory = player.GetComponent<ShootBehaviour>();
        gameController = GameObject.FindGameObjectWithTag("GameController");
        // Assert that exists a on-screen HUD.
        if (GameObject.Find("ScreenHUD") == null)
        {
            Debug.LogError("No ScreenHUD canvas found. Create ScreenHUD inside the GameController");
        }
        weaponHud = GameObject.Find("ScreenHUD").GetComponent<WeaponUIManager>();
        pickupHUD = gameController.transform.Find("PickupHUD");
        // Create physics components and radius of interaction.
        col = this.transform.GetChild(0).gameObject.AddComponent<BoxCollider>();
        this.rbody = this.gameObject.AddComponent<Rigidbody>();
        // Assert that an weapon slot is set up.
        if (this.type == WeaponType.NONE)
        {
            Debug.LogWarning("Set correct weapon slot ( 1 - small/ 2- big)");
            type = WeaponType.SHORT;
        }
        // Assert that the gun muzzle is exists.
        if (!this.transform.Find("muzzle"))
        {
            Debug.LogError(this.name + " muzzle is not present. Create a game object named 'muzzle' as a child of this game object");
        }
        // Set default values.
        fullMag = mag;
        maxBullets = totalBullets;
        pickupHUD.gameObject.SetActive(false);

    }


    // Create the sphere of interaction with player.
    /*  private void CreateInteractiveRadius(Vector3 center)
      {
          interactiveRadius = this.gameObject.AddComponent<SphereCollider>();
          interactiveRadius.center = center;
          interactiveRadius.radius = 1f;
          interactiveRadius.isTrigger = true;
      }*/

    void Update()
    {
        // Handle player pick weapon action.
        /* if (this.pickable && Input.GetButtonDown(playerInventory.pickButton))
         {
             // Disable weapon physics.
             rbody.isKinematic = true;
             this.col.enabled = false;

             // Setup weapon and add in player inventory.
             playerInventory.AddWeapon(this);
             Destroy(interactiveRadius);
             this.Toggle(true);
             this.pickable = false;

             // Change active weapon HUD.
             TooglePickupHUD(false);
         }*/
    }

    // Handle weapon collision with environment.
   /* private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag != "Player" && Vector3.Distance(transform.position, player.transform.position) <= 5f)
        {
            AudioSource.PlayClipAtPoint(dropSound, transform.position, 0.5f);
        }
    }*/
    public void OnShooting()
    {
        AudioSource.PlayClipAtPoint(shotSound, transform.position, 5f);
    }

    /*// Handle player exiting radius of interaction.
     private void OnTriggerExit(Collider other)
     {
         if (other.gameObject == player)
         {
             pickable = false;
             TooglePickupHUD(false);
         }
     }*/

    // Handle player within radius of interaction.
    /*void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player && playerInventory && playerInventory.isActiveAndEnabled)
        {
            pickable = true;
            TooglePickupHUD(true);
        }
    }*/

    // Draw in-game weapon pickup label.
    /*public void TooglePickupHUD(bool toogle)
    {
        pickupHUD.gameObject.SetActive(toogle);
        if (toogle)
        {
            pickupHUD.position = this.transform.position + Vector3.up * 0.5f;
            Vector3 direction = player.GetComponent<BasicBehaviour>().playerCamera.forward;
            direction.y = 0f;
            pickupHUD.rotation = Quaternion.LookRotation(direction);
            pickupHUD.Find("Label").GetComponent<Text>().text = "Pick " + this.gameObject.name;
        }

    }*/

    // Manage weapon active status.
    public void Toggle(bool active)
    {
        if (active)
            AudioSource.PlayClipAtPoint(pickSound, transform.position, 0.5f);
        weaponHud.Toggle(active);
        UpdateHud();
    }

    // Manage the drop action.
    public void Drop()
    {
        this.gameObject.SetActive(true);
        this.transform.position += (Vector3.up);
        rbody.isKinematic = false;
        this.transform.parent = null;
        this.GetComponent<SphereCollider>().enabled = true;
        this.GetComponent<NetworkTransform>().enabled = true;
        this.col.enabled = true;
        weaponHud.Toggle(false);
    }

    // Start the reload action (called by shoot behaviour).
    public bool StartReload()
    {
        if (mag == fullMag || totalBullets == 0)
            return false;
        else if (totalBullets < fullMag - mag)
        {
            mag += totalBullets;
            totalBullets = 0;
        }
        else
        {
            totalBullets -= fullMag - mag;
            mag = fullMag;
        }

        return true;
    }

    // End the reload action (called by shoot behaviour).
    public void EndReload()
    {
        UpdateHud();
    }

    // Manage shoot action.
    public bool Shoot()
    {
        if (mag > 0)
        {
            mag--;
            UpdateHud();
            return true;
        }
        return false;
    }

    // Reset the bullet parameters.
    public void ResetBullets()
    {
        mag = fullMag;
        totalBullets = maxBullets;
    }

    // Update weapon screen HUD.
    private void UpdateHud()
    {
        weaponHud.UpdateWeaponHUD(sprite, mag, fullMag, totalBullets);
    }
}