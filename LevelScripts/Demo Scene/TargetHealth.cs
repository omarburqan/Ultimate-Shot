using UnityEngine;

// This class is created for the example scene. There is no support for this script.
public class TargetHealth : HealthManager
{
	public bool boss;
	public AudioClip toggleSound;

	private Vector3 targetRotation;
	private float health, totalHealth = 80;
	private RectTransform healthBar;
	private float originalBarScale;
	private bool dead;

	void Awake ()
	{
		targetRotation = this.transform.localEulerAngles;
		targetRotation.x = -90;
		if (boss)
		{
			healthBar = this.transform.Find("Health/Bar").GetComponent<RectTransform>();
			healthBar.parent.gameObject.SetActive(false);
			originalBarScale = healthBar.sizeDelta.x;
		}
		dead = true;
		health = totalHealth;
	}

	void Update ()
	{
		this.transform.localRotation = Quaternion.Slerp(this.transform.localRotation, Quaternion.Euler(targetRotation), 10 * Time.deltaTime);
	}

	public bool IsDead { get { return dead; } }

	public override void TakeDamage(Vector3 location, Vector3 direction, float damage)
	{
		if (boss)
		{
			health -= damage;
			UpdateHealthBar();
			if (health <= 0 && (int)this.transform.localEulerAngles.x == 0)
			{
				Kill();
			}
		}
		else if ((int)this.transform.localEulerAngles.x >= -15 && !dead)
		{
			Kill();
		}
	}

	public void Kill()
	{
		if(boss)
			healthBar.parent.gameObject.SetActive(false);
		dead = true;
		targetRotation.x = -90;
		AudioSource.PlayClipAtPoint(toggleSound, transform.position);
	}

	public void Revive()
	{
		if (boss)
		{
			health = totalHealth;
			healthBar.parent.gameObject.SetActive(true);
			UpdateHealthBar();
		}
		dead = false;
		targetRotation.x = 0;
		AudioSource.PlayClipAtPoint(toggleSound, transform.position);
	}

	private void UpdateHealthBar()
	{
		float scaleFactor = health / totalHealth;

		healthBar.sizeDelta = new Vector2(scaleFactor * originalBarScale, healthBar.sizeDelta.y);
	}
}
