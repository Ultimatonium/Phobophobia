using UnityEngine;

public class VolumeChanger : MonoBehaviour
{
  [SerializeField] private UnityEngine.UI.Slider volumeSlider = null;
  [Header("FMOD")]
  [FMODUnity.EventRef][SerializeField] private string masterBusPath = "bus:/Master";
  [FMODUnity.EventRef][SerializeField] private string pausePatterBusPath = "bus:/PausePatter";
  [Space(10)]
  [SerializeField] private float initialVolume = 0.5f;

  private FMOD.Studio.Bus masterBus;

  private void Start()
  {
    masterBus = FMODUnity.RuntimeManager.GetBus(masterBusPath);
    volumeSlider.value = initialVolume;
  }

  public void ChangeVolume(float newValue)
  {
    if(!masterBus.isValid())
      return;

    CheckRESULT(masterBus.setVolume(newValue));
    CheckRESULT(FMODUnity.RuntimeManager.GetBus(pausePatterBusPath).setVolume(newValue));

    //Debug.Log("New volume-value: " + newValue);
  }

  private void CheckRESULT(FMOD.RESULT returnValue)
  {
    if(returnValue != FMOD.RESULT.OK)
      Debug.LogWarning("FMOD-Error: " + FMOD.Error.String(returnValue));
  }
}