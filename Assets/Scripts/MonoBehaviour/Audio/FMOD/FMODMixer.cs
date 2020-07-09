using UnityEngine;

public class FMODMixer : MonoBehaviour
{
  [FMODUnity.EventRef][SerializeField] private string eventPath = null;
  [FMODUnity.EventRef][SerializeField] private string busPath = null;
  [FMODUnity.EventRef][SerializeField] private string vcaPath = null;

  private FMOD.Studio.EventInstance instance, snapshot;
  private FMOD.Studio.Bus bus;
  private FMOD.Studio.VCA vca;

  private void Start()
  {
    instance = FMODUnity.RuntimeManager.CreateInstance(eventPath);
    instance.start();

    bus = FMODUnity.RuntimeManager.GetBus(busPath);
    vca = FMODUnity.RuntimeManager.GetVCA(vcaPath);
  }

  public void SetBus(string busPath)
  {
    string startWith = "bus:/";
    if(!busPath.StartsWith(startWith))
    {
      Debug.LogWarning($"\"{System.Reflection.MethodBase.GetCurrentMethod().Name}\" has to start with \"{startWith}\"!");
      return;
    }

    bus = FMODUnity.RuntimeManager.GetBus(busPath);
  }

  public void SetBusVolume(float busVolume)
  {
    float volume = Mathf.Pow(10f, busVolume / 20f);
    bus.setVolume(volume);
  }

  public void SetVCA(string vcaPath)
  {
    string startWith = "vca:/";
    if(!vcaPath.StartsWith(startWith))
    {
      Debug.LogWarning($"\"{System.Reflection.MethodBase.GetCurrentMethod().Name}\" has to start with \"{startWith}\"!");
      return;
    }

    vca = FMODUnity.RuntimeManager.GetVCA(vcaPath);
  }

  public void SetVCAVolume(float vcaVolume)
  {
    float volume = Mathf.Pow(10f, vcaVolume / 20f);
    vca.setVolume(volume);
  }

  public void StartInstancedSnapshot(string snapshotPath)
  {
    string startWith = "snapshot:/";
    if(!snapshotPath.StartsWith(startWith))
    {
      Debug.LogWarning($"\"{System.Reflection.MethodBase.GetCurrentMethod().Name}\" has to start with \"{startWith}\"!");
      return;
    }

    snapshot = FMODUnity.RuntimeManager.CreateInstance(snapshotPath);
    snapshot.start();
  }

  public void StopInstancedSnapshot(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
  {
    snapshot.stop(stopMode);
    snapshot.release();
  }

  public void StopEntireBus(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT) => bus.stopAllEvents(stopMode);

  //Ensure there are no memory-leaks!
  private void OnDestroy()
  {
    instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    snapshot.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    instance.release();
    snapshot.release();
  }
}