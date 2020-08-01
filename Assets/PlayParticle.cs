using UnityEngine;

public class PlayParticle : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem particleSystem;

    public void OneShot()
    {
        particleSystem.Stop();
        particleSystem.Play();
    }
}
