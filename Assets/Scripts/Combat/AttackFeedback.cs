using UnityEngine;
using Cinemachine;

public class AttackFeedback : MonoBehaviour
{
    public ParticleSystem hitEffect;
    public AudioSource audioSource;
    public AudioClip hitSound;
    public CinemachineImpulseSource impulse;

    
    public void PlayHitEffect(Vector3 position)
    {
        if (hitEffect != null)
        {
            ParticleSystem ps = Instantiate(hitEffect, position, Quaternion.identity);
            ps.Play();
        }

        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        if (impulse != null)
            impulse.GenerateImpulse();
    }

    
    public void PlayHitFeedbackAt(Vector3 hitPos)
    {
        PlayHitEffect(hitPos);
    }
}
