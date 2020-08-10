using UnityEngine;

/* Example usage: 
 * [SerializeField] private PlayerAudioData playerAudio;
 * FMODUnity.RuntimeManager.PlayOneShot(playerAudio.jump); */

[CreateAssetMenu(menuName = "Audio/FMOD/PlayerData", fileName = "PlayerAudio")]
public class PlayerAudioData : ScriptableObject
{
  [Header("Movement")]
  [FMODUnity.EventRef][SerializeField] private string footstepsMetal = null;
  public string FootstepsMetal {get => footstepsMetal; private set => footstepsMetal = value;}
  [FMODUnity.EventRef][SerializeField] private string footstepsSand = null;
  public string FootstepsSand {get => footstepsSand; private set => footstepsSand = value;}
  [FMODUnity.EventRef][SerializeField] private string footstepsWood = null;
  public string FootstepsWood {get => footstepsWood; private set => footstepsWood = value;}

  [Header("Fighting")]
  [FMODUnity.EventRef][SerializeField] private string pillowAttack = null;
  public string PillowAttack {get => pillowAttack; private set => pillowAttack = value;}
  [FMODUnity.EventRef][SerializeField] private string pillowBlock = null;
  public string PillowBlock {get => pillowBlock; private set => pillowBlock = value;}

  [Header("Health")]
  [FMODUnity.EventRef][SerializeField] private string heartbeats = null;
  public string Heartbeats {get => heartbeats; private set => heartbeats = value;}
  [FMODUnity.EventRef][SerializeField] private string hitsounds = null;
  public string Hitsounds {get => hitsounds; private set => hitsounds = value;}
  [FMODUnity.EventRef][SerializeField] private string healthRegen = null;
  public string HealthRegen {get => healthRegen; private set => healthRegen = value;}
  [FMODUnity.EventRef][SerializeField] private string respawn = null;
  public string Respawn {get => respawn; private set => respawn = value;}
}