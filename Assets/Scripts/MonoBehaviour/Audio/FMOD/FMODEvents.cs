﻿using System.Collections;
using UnityEngine;

/* Example usage: 
 * [SerializeField] private PlayerAudioData playerAudio;
 * FMODUnity.RuntimeManager.PlayOneShot(playerAudio.jump); */
[CreateAssetMenu(menuName = "Audio/FMOD/Data", fileName = "New Player Sheet")]
public class PlayerAudioData : ScriptableObject
{
  [Header("Movement")]
  [FMODUnity.EventRef][SerializeField] private string footsteps = null;
  public string Footsteps {get => footsteps; private set => footsteps = value;}
  [FMODUnity.EventRef][SerializeField] private string jump = null;
  public string Jump {get => jump; private set => jump = value;}
  [FMODUnity.EventRef][SerializeField] private string dash = null;
  public string Dash {get => dash; private set => dash = value;}
  [FMODUnity.EventRef][SerializeField] private string land = null;
  public string Land {get => land; private set => land = value;}

  [Header("Attacks")]
  [FMODUnity.EventRef][SerializeField] private string[] swordAttacks = null;
  public string[] SwordAttacks {get => swordAttacks; private set => swordAttacks = value;}
  [FMODUnity.EventRef][SerializeField] private string hammerAttack = null;
  public string HammerAttack {get => hammerAttack; private set => hammerAttack = value;}
  [FMODUnity.EventRef][SerializeField] private string shield = null;
  public string Shield {get => shield; private set => shield = value;}
}

public class FMODEvents : MonoBehaviour
{
  [FMODUnity.EventRef][SerializeField] private string eventPath = null;

  private FMOD.Studio.EventInstance loopInstance, posInstance, instance;

  private FMOD.DSP dsp = new FMOD.DSP();
  private FMOD.DSP_METERING_INFO meterInfo = new FMOD.DSP_METERING_INFO();
  private FMOD.ChannelGroup channelGroup;
  private bool loaded;

  private void Start()
  {
    instance = FMODUnity.RuntimeManager.CreateInstance(eventPath);
    instance.start();
    StartCoroutine(GetChannelGroup());
  }

  private IEnumerator GetChannelGroup()
  {
    if(instance.isValid())
    {
      while(instance.getChannelGroup(out channelGroup) != FMOD.RESULT.OK)
      {
        yield return new WaitForEndOfFrame();
        loaded = false;
      }

      channelGroup.getDSP(0, out dsp);
      dsp.setMeteringEnabled(false, true);

      loaded = true;
    }
    else
    {
      Debug.Log($"\"{instance}\" is non-existent!");

      yield return null;
    }
  }

  public void PlayOneShot(string eventPath, Vector3 location = default(Vector3))
  {
    string startWith = "event:/";
    if(!eventPath.StartsWith(startWith))
    {
      Debug.LogWarning($"\"{System.Reflection.MethodBase.GetCurrentMethod().Name}\" has to start with \"{startWith}\"!");
      return;
    }

    if(location != Vector3.zero) 
      FMODUnity.RuntimeManager.PlayOneShot(eventPath, location); 
    else
      FMODUnity.RuntimeManager.PlayOneShot(eventPath);
  }

  public void PlayOneShotAttached(string eventPath, GameObject attachedTo)
  {
    string startWith = "event:/";
    if(!eventPath.StartsWith(startWith))
    {
      Debug.LogWarning($"\"{System.Reflection.MethodBase.GetCurrentMethod().Name}\" has to start with \"{startWith}\"!");
      return;
    }

    FMODUnity.RuntimeManager.PlayOneShotAttached(eventPath, attachedTo);
  }

  public void PlayInstancedEvent(string eventPath)
  {
    string startWith = "event:/";
    if(!eventPath.StartsWith(startWith))
    {
      Debug.LogWarning($"\"{System.Reflection.MethodBase.GetCurrentMethod().Name}\" has to start with \"{startWith}\"!");
      return;
    }

    FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(eventPath);
    instance.start();
    instance.release();
  }

  public void StartInstancedLoopEvent(string eventPath)
  {
    string startWith = "event:/";
    if(!eventPath.StartsWith(startWith))
    {
      Debug.LogWarning($"\"{System.Reflection.MethodBase.GetCurrentMethod().Name}\" has to start with \"{startWith}\"!");
      return;
    }

    loopInstance = FMODUnity.RuntimeManager.CreateInstance(eventPath);
    loopInstance.start();
  }

  public void StopInstancedLoopEvent(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
  {
    loopInstance.stop(stopMode);
    loopInstance.release();
  }

  public void StartInstanced3DEvent(string eventPath)
  {
    string startWith = "event:/";
    if(!eventPath.StartsWith(startWith))
    {
      Debug.LogWarning($"\"{System.Reflection.MethodBase.GetCurrentMethod().Name}\" has to start with \"{startWith}\"!");
      return;
    }

    posInstance = FMODUnity.RuntimeManager.CreateInstance(eventPath);
    FMODUnity.RuntimeManager.AttachInstanceToGameObject(posInstance, GetComponent<Transform>(), GetComponent<Rigidbody>()); 
    posInstance.start();
  }

  public void StopInstanced3DEvent(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
  {
    posInstance.stop(stopMode);
    posInstance.release();
  }

  public void StartInstancedEvent(string eventPath)
  {
    string startWith = "event:/";
    if(!eventPath.StartsWith(startWith))
    {
      Debug.LogWarning($"\"{System.Reflection.MethodBase.GetCurrentMethod().Name}\" has to start with \"{startWith}\"!");
      return;
    }

    instance = FMODUnity.RuntimeManager.CreateInstance(eventPath);
    instance.start();
  }

  public void StopInstancedEvent(ref FMOD.Studio.EventInstance instance, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
  {
    instance.stop(stopMode);
    instance.release();
  }

  private FMOD.Studio.PLAYBACK_STATE PlaybackState(FMOD.Studio.EventInstance instance)
  {
    FMOD.Studio.PLAYBACK_STATE pS;
    instance.getPlaybackState(out pS);

    return pS;
  }

  public bool IsEventInstanceStartable(ref FMOD.Studio.EventInstance instance)
  {
    bool okay = PlaybackState(instance) != FMOD.Studio.PLAYBACK_STATE.PLAYING && PlaybackState(instance) != FMOD.Studio.PLAYBACK_STATE.SUSTAINING;
    if(okay)
      return true;
    else if(PlaybackState(instance) == FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
    {
      instance.triggerCue(); //Move past one sustain point.
      if(okay) //Was that already enough?
        return true;
      else
        return false;
    }
    else
    {
      Debug.Log($"\"{instance}\" is playing!");

      return false;
    }
  }

  private float GetRMS()
  {
    if(!loaded)
    {
      Debug.LogWarning("Channel group(s) could not be retrieved!");

      return 0f;
    }

    float rms = 0f;

    dsp.getMeteringInfo(System.IntPtr.Zero, out meterInfo);
    for(int i = 0; i < meterInfo.numchannels; i++)
      rms += meterInfo.rmslevel[i] * meterInfo.rmslevel[i];

    rms = Mathf.Sqrt(rms / (float)meterInfo.numchannels);

    float dB = rms > 0f ? 20f * Mathf.Log10(rms * Mathf.Sqrt(2f)) : -80f;
    if(dB > 10f)
      dB = 10f;

    return dB;
  }

  //Ensure there are no memory-leaks!
  private void OnDestroy()
  {
    loopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    posInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    loopInstance.release();
    posInstance.release();
    instance.release();
  }

  private void Update()
  {
    //Following is the alternative to: FMODUnity.RuntimeManager.AttachInstanceToGameObject(posInstance, GetComponent<Transform>(), GetComponent<Rigidbody>()); 
    //posInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
  }
}