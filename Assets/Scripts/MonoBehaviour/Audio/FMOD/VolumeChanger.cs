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
    volumeSlider.value = PlayerPrefs.GetFloat("SoundVolume", initialVolume); //"initialVolume" is used if PlayerPrefs hold no "SoundVolume"-float.
  }

  public void ChangeVolume(float newValue)
  {
    if(!masterBus.isValid())
      return;

    FMODErrorHandling.CheckRESULT(masterBus.setVolume(newValue));
    FMODErrorHandling.CheckRESULT(FMODUnity.RuntimeManager.GetBus(pausePatterBusPath).setVolume(newValue));

    PlayerPrefs.SetFloat("SoundVolume", newValue);

    //Debug.Log("New volume-value: " + newValue);
  }
}