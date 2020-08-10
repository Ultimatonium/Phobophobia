using UnityEngine;

[CreateAssetMenu(menuName = "Audio/FMOD/TowerData", fileName = "TowerAudio")]
public class TowerAudioData : ScriptableObject
{
  [Header("Movement")]
  [FMODUnity.EventRef][SerializeField] private string towerMove = null;
  public string TowerMove {get => towerMove; private set => towerMove = value;}

  [Header("Fighting")]
  [FMODUnity.EventRef][SerializeField] private string towerShooting = null;
  public string TowerShooting {get => towerShooting; private set => towerShooting = value;}

  [Header("Health")]
  [FMODUnity.EventRef][SerializeField] private string build = null;
  public string Build {get => build; private set => build = value;}
  [FMODUnity.EventRef][SerializeField] private string repair = null;
  public string Repair {get => repair; private set => repair = value;}
}