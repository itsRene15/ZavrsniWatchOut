using UnityEngine;

public class PlayerDeathEffects : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem particlePrefab;
    public AudioSource audioSource;
    public AudioClip deathClip;
    public float deathVolume = 1f;

    // Ensure we have an AudioSource
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
        }
    }

    // Spawn particles and play sound
    public void TriggerDeathEffects(Vector3 position)
    {
        if (particlePrefab != null)
        {
            ParticleSystem ps;
            if (particlePrefab.gameObject.scene.IsValid() && particlePrefab.transform.IsChildOf(transform))
            {
                ps = Instantiate(particlePrefab, position, Quaternion.identity);
            }
            else
            {
                ps = Instantiate(particlePrefab, position, Quaternion.identity);
            }
            var main = ps.main;
            ps.gameObject.AddComponent<AutoDestroyParticle>();
            ps.Play();
        }

        if (audioSource != null && deathClip != null)
        {
            audioSource.PlayOneShot(deathClip, deathVolume);
        }
    }
}

public class AutoDestroyParticle : MonoBehaviour
{
    private ParticleSystem[] systems;

    // Cache child particle systems
    private void Awake()
    {
        systems = GetComponentsInChildren<ParticleSystem>(true);
    }

    // Destroy when all particles are dead
    private void Update()
    {
        bool anyAlive = false;
        for (int i = 0; i < systems.Length; i++)
        {
            if (systems[i] != null && systems[i].IsAlive(true)) { anyAlive = true; break; }
        }
        if (!anyAlive) Destroy(gameObject);
    }
}
