using System.Runtime.InteropServices;

/* Programmer sounds are ideal for an efficient implementation of dialogue systems.
 * They need an audio table - a group of audio files that are compressed in a bank and not associated with any event; they can be accessed solely by a string key.
 * 
 * Example usage: PlayDialogue(nameOfDialogue); */
class ProgrammerSounds : UnityEngine.MonoBehaviour
{
  [FMODUnity.EventRef][UnityEngine.SerializeField] private  string eventPath = null;

  private FMOD.Studio.EVENT_CALLBACK dialogueCallback;

  private void Start()
  {
    dialogueCallback = new FMOD.Studio.EVENT_CALLBACK(DialogueEventCallback);
  }

  private void PlayDialogue(string key)
  {
    var dialogueInstance = FMODUnity.RuntimeManager.CreateInstance(eventPath);

    GCHandle stringHandle = GCHandle.Alloc(key, GCHandleType.Pinned);
    dialogueInstance.setUserData(GCHandle.ToIntPtr(stringHandle));

    dialogueInstance.setCallback(dialogueCallback);
    dialogueInstance.start();
    dialogueInstance.release();
  }

  private static FMOD.RESULT DialogueEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, FMOD.Studio.EventInstance instance, System.IntPtr parameterPtr)
  {
    instance.getUserData(out System.IntPtr stringPtr);

    GCHandle stringHandle = GCHandle.FromIntPtr(stringPtr);
    string key = stringHandle.Target as string;

    switch(type)
    {
      case FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
        {
          FMOD.MODE soundMode = FMOD.MODE.LOOP_NORMAL | FMOD.MODE.CREATECOMPRESSEDSAMPLE | FMOD.MODE.NONBLOCKING;
          var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));

          if(key.Contains("."))
          {
            FMOD.Sound dialogueSound;
            var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(UnityEngine.Application.streamingAssetsPath + "/" + key, soundMode, out dialogueSound);
            if(soundResult == FMOD.RESULT.OK)
            {
              parameter.sound = dialogueSound.handle;
              parameter.subsoundIndex = -1;
              Marshal.StructureToPtr(parameter, parameterPtr, false);
            }
          }
          else
          {
            FMOD.Studio.SOUND_INFO dialogueSoundInfo;
            var keyResult = FMODUnity.RuntimeManager.StudioSystem.getSoundInfo(key, out dialogueSoundInfo);
            if(keyResult != FMOD.RESULT.OK)
              break;

            FMOD.Sound dialogueSound;
            var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(dialogueSoundInfo.name_or_data, soundMode | dialogueSoundInfo.mode, ref dialogueSoundInfo.exinfo, out dialogueSound);
            if(soundResult == FMOD.RESULT.OK)
            {
              parameter.sound = dialogueSound.handle;
              parameter.subsoundIndex = dialogueSoundInfo.subsoundindex;
              Marshal.StructureToPtr(parameter, parameterPtr, false);
            }
          }
        }
        break;
      case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
        {
          var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));
        var sound = new FMOD.Sound
        {
          handle = parameter.sound
        };
        sound.release();
        }
        break;
      case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROYED:
        stringHandle.Free();
        break;
    }

    return FMOD.RESULT.OK;
  }
}