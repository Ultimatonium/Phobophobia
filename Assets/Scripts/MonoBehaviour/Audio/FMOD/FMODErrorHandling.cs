//Actually, this should really be done by FMOD for Unity!
public class FMODErrorHandling : UnityEngine.MonoBehaviour
{
  public static void CheckRESULT(FMOD.RESULT returnValue)
  {
    if(returnValue != FMOD.RESULT.OK)
      UnityEngine.Debug.LogWarning("FMOD-Error: " + FMOD.Error.String(returnValue));
  }
}