using UnityEngine;

[CreateAssetMenu(menuName = "Audio/FMOD/EnemyData", fileName = "EnemyAudio")]
public class EnemyAudioData : ScriptableObject
{
  [Header("Movement")]
  [FMODUnity.EventRef][SerializeField] private string footstepsMetal = null;
  public string FootstepsMetal {get => footstepsMetal; private set => footstepsMetal = value;}
  [FMODUnity.EventRef][SerializeField] private string footstepsSand = null;
  public string FootstepsSand {get => footstepsSand; private set => footstepsSand = value;}
  [FMODUnity.EventRef][SerializeField] private string footstepsWood = null;
  public string FootstepsWood {get => footstepsWood; private set => footstepsWood = value;}

  [Header("Fighting")]
  [FMODUnity.EventRef][SerializeField] private string lasershotLight = null;
  public string LasershotLight {get => lasershotLight; private set => lasershotLight = value;}
  [FMODUnity.EventRef][SerializeField] private string lasershotDeep = null;
  public string LasershotDeep {get => lasershotDeep; private set => lasershotDeep = value;}

  [Header("Health")]
  [FMODUnity.EventRef][SerializeField] private string spawn = null;
  public string Spawn {get => spawn; private set => spawn = value;}
  [FMODUnity.EventRef][SerializeField] private string hitsounds = null;
  public string Hitsounds {get => hitsounds; private set => hitsounds = value;}
  [FMODUnity.EventRef][SerializeField] private string dead = null;
  public string Dead {get => dead; private set => dead = value;}
}