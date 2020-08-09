using UnityEngine;

/* Example usage: 
 * [SerializeField] private PlayerAudioData playerAudio;
 * FMODUnity.RuntimeManager.PlayOneShot(playerAudio.jump); */

[CreateAssetMenu(menuName = "Audio/FMOD/Data", fileName = "New Player Sheet")]
public class PlayerAudioData : ScriptableObject
{
  [Header("Movement")]
  [FMODUnity.EventRef][SerializeField] private string footsteps = null;
  public string Footsteps { get => footsteps; private set => footsteps = value; }
  [FMODUnity.EventRef][SerializeField] private string jump = null;
  public string Jump { get => jump; private set => jump = value; }
  [FMODUnity.EventRef][SerializeField] private string dash = null;
  public string Dash { get => dash; private set => dash = value; }
  [FMODUnity.EventRef][SerializeField] private string land = null;
  public string Land { get => land; private set => land = value; }

  [Header("Attacks")]
  [FMODUnity.EventRef][SerializeField] private string[] swordAttacks = null;
  public string[] SwordAttacks { get => swordAttacks; private set => swordAttacks = value; }
  [FMODUnity.EventRef][SerializeField] private string hammerAttack = null;
  public string HammerAttack { get => hammerAttack; private set => hammerAttack = value; }
  [FMODUnity.EventRef][SerializeField] private string shield = null;
  public string Shield { get => shield; private set => shield = value; }
}