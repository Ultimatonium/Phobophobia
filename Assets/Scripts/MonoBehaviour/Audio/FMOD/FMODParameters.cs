public class FMODParameters : UnityEngine.MonoBehaviour
{
  [FMODUnity.EventRef][UnityEngine.SerializeField] private string eventPath = null;

  private FMOD.Studio.EventInstance paraInstance;

  private void Start()
  {
    paraInstance = FMODUnity.RuntimeManager.CreateInstance(eventPath);
    paraInstance.start();
  }

  public void SetParametersByName(System.Collections.Generic.Dictionary<string, float> parameters, bool global = false)
  {
    foreach(System.Collections.Generic.KeyValuePair<string, float> parameter in parameters)
    {
      if(!global)
        paraInstance.setParameterByName(parameter.Key, parameter.Value);
      else
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(parameter.Key, parameter.Value);
    }
  }

  //Ensure there are no memory-leaks!
  private void OnDestroy()
  {
    paraInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    paraInstance.release();
  }
}