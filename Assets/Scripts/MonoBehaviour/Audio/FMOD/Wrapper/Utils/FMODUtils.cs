using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using UnityEngine;
using FMOD;
using FMOD.Studio;
using FMODUnity;

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

		public static global::FMOD.Sound CreateSound(int sampleSize, int channels = 1, int sampleRate = 44100)
		{
			/* Explicitly create the delegate object and assign it to a member, so it doesn't get freed by the garbage collected while it's being used.
			   pcmReadCallback = new global::FMOD.SOUND_PCMREADCALLBACK(PcmReadCallback);
			   pcmSetPosCallback = new global::FMOD.SOUND_PCMSETPOSCALLBACK(PcmSetPosCallback); */

			global::FMOD.CREATESOUNDEXINFO soundInfo = new global::FMOD.CREATESOUNDEXINFO()
			{
				cbsize = Marshal.SizeOf(typeof(global::FMOD.CREATESOUNDEXINFO)),
				format = global::FMOD.SOUND_FORMAT.PCMFLOAT,
				defaultfrequency = sampleRate,
				length = (uint)(sampleSize * channels * sizeof(float)),
				numchannels = channels,
				//pcmreadcallback = pcmReadCallback,
				//pcmsetposcallback = pcmSetPosCallback
			};

			global::FMOD.Sound sound;
			FMODUtils.Check(FMODUnity.RuntimeManager.CoreSystem.createSound(string.Empty, global::FMOD.MODE.OPENUSER | global::FMOD.MODE.LOOP_NORMAL, ref soundInfo, out sound));

			return sound;
		}
	}
}