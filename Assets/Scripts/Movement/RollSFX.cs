using UnityEngine;

public class RollSFX : MonoBehaviour
{
    [SerializeField] private float yThreshold = 0f;
    [SerializeField] private float velocityThreshold = 0.1f;
    [SerializeField] private float maxVelocity = 10f;
    [SerializeField] private float maxPitch = 0.75f;
    [SerializeField] private float maxVolume = 1f;

    private Rigidbody rb;
    private AudioSource audioSource;
    private bool isPlayingSound = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        float velocity = rb.linearVelocity.magnitude;
        bool shouldPlay = transform.position.y < yThreshold && velocity > velocityThreshold;

        if (shouldPlay)
        {
            if (!isPlayingSound)
            {
                audioSource.Play();
                isPlayingSound = true;
            }

            float velocityRatio = Mathf.Clamp01(velocity / maxVelocity);
            audioSource.pitch = velocityRatio * maxPitch;
            audioSource.volume = velocityRatio * maxVolume;
        }
        else if (isPlayingSound)
        {
            audioSource.Stop();
            isPlayingSound = false;
        }
    }
}