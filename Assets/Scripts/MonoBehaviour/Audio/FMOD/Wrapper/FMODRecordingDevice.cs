using FMOD;

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
			get {return SampleRate * LATENCY_MS / 1000;}
		}

		public int DriftThreshold
		{
			//The point to start compensating for drift.
			get {return SampleRate * DRIFT_MS / 1000;}
		}

		public Sound StartRecording()
		{
      //Create user sound to record into, then start recording.
      CREATESOUNDEXINFO soundInfo = new CREATESOUNDEXINFO()
			{
				cbsize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CREATESOUNDEXINFO)),
				format = SOUND_FORMAT.PCM16,
				defaultfrequency = SampleRate,
				length = (uint)(SampleRate * Channels * sizeof(short)),  //One second buffer, which doesn't change latency.
				numchannels = Channels
			};

      FMODUtils.Check(FMODUnity.RuntimeManager.CoreSystem.createSound(string.Empty, MODE.OPENUSER | MODE.LOOP_NORMAL, ref soundInfo, out Sound sound));
      FMODUtils.Check(FMODUnity.RuntimeManager.CoreSystem.recordStart(DeviceIndex, sound, true));

			return sound;
		}

		public static System.Collections.Generic.IEnumerable<FMODRecordingDevice> GetAllDevices()
		{
      FMODUtils.Check(FMODUnity.RuntimeManager.CoreSystem.getRecordNumDrivers(out int totalMicCount, out int connectedMicCount), "Could not list recording devices!");

      for(int i = 0; i < connectedMicCount; i++)
				yield return GetDevice(i);
		}

		public static FMODRecordingDevice GetDevice(int deviceIndex)
		{
      FMODUtils.Check(FMODUnity.RuntimeManager.CoreSystem.getRecordDriverInfo(deviceIndex,
                                                                              out string name,
                                                                              100,
																																	          	out System.Guid guid,
																																	            out int systemRate,
                                                                              out SPEAKERMODE speakerMode,
                                                                              out int speakerModeChannels,
                                                                              out DRIVER_STATE state),
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