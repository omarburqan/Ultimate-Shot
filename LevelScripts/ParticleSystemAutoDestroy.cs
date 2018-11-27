using UnityEngine;

// This class ensures that a particle's game object will auto-destroy after its lifetime.
public class ParticleSystemAutoDestroy : MonoBehaviour
{
	private ParticleSystem ps;

	public void Start()
	{
		// Set up the references.
		ps = GetComponent<ParticleSystem>();
	}

	public void Update()
	{
		// Check if lifetime has ended to destroy it.
		if (ps)
		{
			if (!ps.IsAlive())
			{
				Destroy(gameObject);
			}
		}
	}
}
