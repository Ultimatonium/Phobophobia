using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using FMOD;
using FMOD.Studio;
using FMODUnity;

namespace SwinguinGames.FMOD
{
	public class FMODRecordingDevice
	{
		public int DeviceIndex {get; private set;}
		public string Name {get; private set;}
		public int SampleRate {get; private set;}
		public int Channels {get; private set;}
		public DRIVER_STATE State {get; private set;}

		private const int LATENCY_MS = 0;
		private const int DRIFT_MS = 1;

		public int DesiredLatency
		{
			//User specified latency:
			get {return (SampleRate * LATENCY_MS) / 1000;}
		}

		public int DriftThreshold
		{
			//The point to start compensating for drift.
			get {return (SampleRate * DRIFT_MS) / 1000;}
		}

		public global::FMOD.Sound StartRecording()
		{
      //Create user sound to record into, then start recording.
			global::FMOD.CREATESOUNDEXINFO soundInfo = new global::FMOD.CREATESOUNDEXINFO()
			{
				cbsize = Marshal.SizeOf(typeof(global::FMOD.CREATESOUNDEXINFO)),
				format = global::FMOD.SOUND_FORMAT.PCM16,
				defaultfrequency = SampleRate,
				length = (uint)(SampleRate * Channels * sizeof(short)),  //One second buffer, which doesn't change latency.
				numchannels = Channels
			};

			global::FMOD.Sound sound;
			FMODUtils.Check(RuntimeManager.CoreSystem.createSound(string.Empty, global::FMOD.MODE.OPENUSER | global::FMOD.MODE.LOOP_NORMAL, ref soundInfo, out sound));
			FMODUtils.Check(RuntimeManager.CoreSystem.recordStart(DeviceIndex, sound, true));

			return sound;
		}

		public static IEnumerable<FMODRecordingDevice> GetAllDevices()
		{
			int totalMicCount, connectedMicCount;
			FMODUtils.Check(RuntimeManager.CoreSystem.getRecordNumDrivers(out totalMicCount, out connectedMicCount), "Could not list recording devices!");

			for(int i = 0; i < connectedMicCount; i++)
				yield return GetDevice(i);
		}

		public static FMODRecordingDevice GetDevice(int deviceIndex)
		{
			string name;
			System.Guid guid;
			int systemRate;
			SPEAKERMODE speakerMode;
			int speakerModeChannels;
			DRIVER_STATE state;

			FMODUtils.Check(RuntimeManager.CoreSystem.getRecordDriverInfo(deviceIndex,
																																		out name,
																																		100,
																																		out guid,
																																		out systemRate,
																																		out speakerMode,
																																		out speakerModeChannels,
																																		out state),
											"Could not retrievw recording device!");

			return new FMODRecordingDevice()
			{
				DeviceIndex = deviceIndex,
				Name = name,
				SampleRate = systemRate,
				Channels = speakerModeChannels,
				State = state
			};
		}
	}
}