using UnityEngine;
using UnityEngine.UI;

// This class corresponds to the weapon screen HUD features.
public class WeaponUIManager : MonoBehaviour
{
	public Color bulletColor = Color.white;             // Color of the available bullets inside weapon HUD.
	public Color emptyBulletColor = Color.black;        // Color of the empty  bullets inside weapon HUD.

	private Color nobulletColor;                        // Transparent color to hide extra capacity bullet slots.
	private Image weaponHud;                            // The weapon draw inside HUD.
	private GameObject bulletMag;                       // The bullets draw inside HUD.
	private Text totalBulletsHud;                       // The bullets amount label inside HUD.

	void Start ()
	{
		// Set up references and default values.
		weaponHud = this.transform.Find("WeaponHUD/Weapon").GetComponent<Image>();
		bulletMag = this.transform.Find("WeaponHUD/Data/Mag").gameObject;
		nobulletColor = new Color(0, 0, 0, 0);
		totalBulletsHud = this.transform.Find("WeaponHUD/Data/Label").GetComponent<Text>();

		// Player begins unarmed, hide weapon HUD.
		Toggle(false);
	}

	// Manage on-screen HUD visibility.
	public void Toggle(bool active)
	{
		weaponHud.transform.parent.gameObject.SetActive(active);
	}

	// Update the weapon HUD features.
	public void UpdateWeaponHUD(Sprite weaponSprite, int bulletsLeft, int fullMag, int extraBullets)
	{
		// Update the weapon draw.
		if(weaponSprite != null && weaponHud.sprite != weaponSprite)
		{
			weaponHud.sprite = weaponSprite;
		}
		// Update bullet draws.
		int b = 0;
		foreach(Transform bullet in bulletMag.transform)
		{
			if(b < bulletsLeft)
			{
				bullet.GetComponent<Image>().color = bulletColor;
			}
			else if(b >= fullMag)
			{
				bullet.GetComponent<Image>().color = nobulletColor;
			}
			else
			{
				bullet.GetComponent<Image>().color = emptyBulletColor;
			}
			b++;
		}

		// Update bullet count label.
		totalBulletsHud.text = bulletsLeft + "/" + extraBullets;
	}
}
