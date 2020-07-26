using FMOD;

namespace SwinguinGames.FMOD
{
	public enum FMODSeverity
	{
		Warning,
		Error,
		Exception
	}

	public class FMODUtils
	{
		public static void Check(RESULT result, string error = "Action returned error.", string path = null, FMODSeverity severity = FMODSeverity.Exception)
		{
			if(result == RESULT.OK)
				return;

			string msg = string.Format("FMOD: {0} ({1})", error, result);

			if(path != null)
				msg += string.Format("\nEvent path: {0}", path);

      switch(severity)
      {
        case FMODSeverity.Warning:
				  UnityEngine.Debug.LogWarning(msg);
				  break;
        case FMODSeverity.Error:
			  	UnityEngine.Debug.LogError(msg);
			  	break;
        case FMODSeverity.Exception:
				  throw new FMODException(msg);
      }
    }

		public static Sound CreateSound(int sampleSize, int channels = 1, int sampleRate = 44100)
		{
      /* Explicitly create the delegate object and assign it to a member, so it doesn't get freed by the garbage collected while it's being used.
			   pcmReadCallback = new global::FMOD.SOUND_PCMREADCALLBACK(PcmReadCallback);
			   pcmSetPosCallback = new global::FMOD.SOUND_PCMSETPOSCALLBACK(PcmSetPosCallback); */

      CREATESOUNDEXINFO soundInfo = new CREATESOUNDEXINFO()
			{
				cbsize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CREATESOUNDEXINFO)),
				format = SOUND_FORMAT.PCMFLOAT,
				defaultfrequency = sampleRate,
				length = (uint)(sampleSize * channels * sizeof(float)),
				numchannels = channels,
				//pcmreadcallback = pcmReadCallback,
				//pcmsetposcallback = pcmSetPosCallback
			};

      Check(FMODUnity.RuntimeManager.CoreSystem.createSound(string.Empty, MODE.OPENUSER | MODE.LOOP_NORMAL, ref soundInfo, out Sound sound));

      return sound;
		}
	}
}