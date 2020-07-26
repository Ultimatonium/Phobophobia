using System.Runtime.InteropServices;

//This is useful for syncing game with sound events.
public class BeatSystem : UnityEngine.MonoBehaviour
{
  public static int Beat {get; private set;}
  public static string Marker {get; private set;}

  [StructLayout(LayoutKind.Sequential)]
  private class TimelineInfo
  {
    public int CurrentMusicBeat {get; set;} = 0;
    public FMOD.StringWrapper LastMarker {get; set;} = new FMOD.StringWrapper();
  }

  private TimelineInfo timelineInfo;
  private GCHandle timelineHandle;

  private FMOD.Studio.EVENT_CALLBACK beatCallback;

  public void AssignBeatEvent(FMOD.Studio.EventInstance instance)
  {
    timelineInfo = new TimelineInfo();
    timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);
    beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);
    instance.setUserData(GCHandle.ToIntPtr(timelineHandle));
    instance.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
  }

  public void StopAndClear(FMOD.Studio.EventInstance instance)
  {
    instance.setUserData(System.IntPtr.Zero);
    instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    instance.release();
    timelineHandle.Free();
  }

  [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
  private static FMOD.RESULT BeatEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, FMOD.Studio.EventInstance instance, System.IntPtr parameterPtr)
  {
    System.IntPtr timelineInfoPtr;
    FMOD.RESULT result = instance.getUserData(out timelineInfoPtr);
    if(result != FMOD.RESULT.OK)
      UnityEngine.Debug.LogError("Timeline Callback Error: " + result);
    else if(timelineInfoPtr != System.IntPtr.Zero)
    {
      GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr);
      TimelineInfo timelineInfo = (TimelineInfo)timelineHandle.Target;

      switch(type)
      {
        case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
          var paraBeat = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
          timelineInfo.CurrentMusicBeat = paraBeat.beat;
          Beat = timelineInfo.CurrentMusicBeat;
          break;
        case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
          var paraMarker = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
          timelineInfo.LastMarker = paraMarker.name;
          Marker = timelineInfo.LastMarker;
          break;
      }
    }

    return FMOD.RESULT.OK;
  }
}

/* Put this in a seperate file to test with it.
public class Test : MonoBehaviour
{
  private FMOD.Studio.EventInstance instance;
  private BeatSystem bS;

  private void Start()
  {
    bS = GetComponent<BeatSystem>();
  }

  private void Update()
  {
    if(Input.GetKeyDown(KeyCode.Space))
    {
      instance = FMODUnity.RuntimeManager.CreateInstance("event:/musik");
      instance.start();
      bS.AssignBeatEvent(instance);
    }

    if(Input.GetKeyDown(KeyCode.LeftControl))
      bS.StopAndClear(instance);
  }
}*/