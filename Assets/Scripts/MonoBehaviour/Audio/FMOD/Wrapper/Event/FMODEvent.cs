using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using FMOD;
using FMOD.Studio;

namespace SwinguinGames.FMOD
{
	[Serializable]
	public class FMODException : Exception
	{
		public FMODException() {}
		public FMODException(string message) : base(message) {}
		public FMODException(string message, Exception inner) : base(message, inner) {}
		protected FMODException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) {}
	}

	[Serializable]
	public class FMODMarker
	{
		[SerializeField] private string name;
		public string Name {get => name; set => name = value;}
		[SerializeField] private double position;
		public double Position {get => position; set => position = value;}
		public static string MarkerJSONFolder {get; set;} = "FMOD-Markers";

		public static Dictionary<string, int> Load(TextAsset markerJSON)
		{
			FMODMarker[] markers = JSONUtils.DeserializeArray<FMODMarker>(markerJSON.text);
			var output = new Dictionary<string, int>();
			for(int i = 0; i < markers.Length; i++)
				output[markers[i].name] = Mathf.FloorToInt((float)(markers[i].position * 1000));

			return output;
		}

		public static Dictionary<string, int> Load(string eventPath)
		{
			string resourcePath = string.Format("{0}/{1}", MarkerJSONFolder, System.IO.Path.GetFileNameWithoutExtension(eventPath));
			var jsonAsset = Resources.Load(resourcePath);
			if(jsonAsset == null)
			{
				UnityEngine.Debug.LogError($"Could not find marker file \"Resources/{resourcePath}.json\"!");

				return null;
			}

			return Load((TextAsset)jsonAsset);
		}
	}

	public class FMODEvent : IDisposable
	{
		public EventInstance Instance {get; private set;}
    private EventDescription description;
		public EventDescription Description {get => description; set => description = value;}

		private readonly string eventPath;
		public string EventPath => eventPath;

		public int TimeMilliseconds
		{
			//get {throw new NotImplementedException();}
			set {Do(Instance.setTimelinePosition(value), "Could not set timeline!");}
		}

		public PLAYBACK_STATE PlaybackState
		{
			get
			{
				PLAYBACK_STATE state;
				Do(Instance.getPlaybackState(out state), "Could not retrieve playback state!");

				return state;
			}
		}

		public Vector3 Position
		{
			//get {throw new NotImplementedException();}
			set {Instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(value));}
		}

		public FMODEvent SetPosition(Vector3 position)
		{
			Position = position;

			return this;
		}

		public bool IsValid
		{
			get {return Instance.isValid();}
		}

		public float Length
		{
			get
			{
        Do(description.getLength(out int length), "Could not retrieve event timeline length!");

        return length / 1000f;
			}
		}

    private event Action<FMODEvent> onPlayStateChanged;
		private event Action<FMODEvent, string> onMarker;
		private event Func<FMODEvent, string, Sound> onProgrammerSoundCreated;

		private EVENT_CALLBACK callbackSink = null;
		private Dictionary<string, int> markers = null;
		private bool markersLoaded = false;

		public event Action<FMODEvent> OnPlayStateChanged
		{
			add {onPlayStateChanged += value; EnableCallbacks();}
			remove {onPlayStateChanged -= value; DisableCallbacks();}
		}

		public event Action<FMODEvent, string> OnMarker
		{
			add {onMarker += value; EnableCallbacks();}
			remove {onMarker -= value; DisableCallbacks();}
		}

		public event Func<FMODEvent, string, Sound> OnProgrammerSoundCreated
		{
			add {onProgrammerSoundCreated += value; EnableCallbacks();}
			remove {onProgrammerSoundCreated -= value; DisableCallbacks();}
		}

		public FMODEvent(string fmodEventPath)
		{
			eventPath = fmodEventPath;
			Instance = FMODUnity.RuntimeManager.CreateInstance(fmodEventPath);
			Do(Instance.getDescription(out description), "Could not retrieve event description!");
		}

		private void EnableCallbacks()
		{
			if(onPlayStateChanged == null && onMarker == null && onProgrammerSoundCreated == null)
				return;

			if(callbackSink == null)
			{
				callbackSink = new EVENT_CALLBACK(FMODEventCallback);
				Do(Instance.setCallback(callbackSink,
																EVENT_CALLBACK_TYPE.STARTED |
																EVENT_CALLBACK_TYPE.STARTING |
																EVENT_CALLBACK_TYPE.RESTARTED |
																EVENT_CALLBACK_TYPE.STOPPED |
																EVENT_CALLBACK_TYPE.START_FAILED |
																EVENT_CALLBACK_TYPE.TIMELINE_MARKER |
																EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND),
					                      "Callback could not be set!");
			}
		}

		private void DisableCallbacks()
		{
			if(onPlayStateChanged != null || onMarker != null || onProgrammerSoundCreated != null)
				return;

			if(callbackSink != null)
			{
				Do(Instance.setCallback(null),
																"Callback could not be unset!");
																callbackSink = null;
			}
		}

		[AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
		private RESULT FMODEventCallback(EVENT_CALLBACK_TYPE type, EventInstance instance, IntPtr parameterPtr)
		{
			switch(type)
			{
				case EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
					if(onMarker != null)
					{
						var parameter = (TIMELINE_MARKER_PROPERTIES)System.Runtime.InteropServices.Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES));
						onMarker.Invoke(this, parameter.name);
					}
					break;
				case EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
					if(onProgrammerSoundCreated != null)
					{
						UnityEngine.Debug.Log("CREATE_PROGRAMMER_SOUND");
						var parameter = (PROGRAMMER_SOUND_PROPERTIES)System.Runtime.InteropServices.Marshal.PtrToStructure(parameterPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));

						Sound sound = onProgrammerSoundCreated.Invoke(this, parameter.name);
						if(sound.hasHandle())
						{
							parameter.sound = sound.handle;
							parameter.subsoundIndex = -1;
						System.Runtime.InteropServices.Marshal.StructureToPtr(parameter, parameterPtr, false);
						}
					}
					break;
				default:
				  if(onPlayStateChanged != null)
            onPlayStateChanged.Invoke(this);
				  break;
			}

			return RESULT.OK;
		}

		private void Do(RESULT result, string error, FMODSeverity severity = FMODSeverity.Error) => FMODUtils.Check(result, error, EventPath, severity);

		public float this[string name]
		{
			get
			{
				float val, fVal;
				Do(Instance.getParameterByName(name, out val, out fVal), string.Format("Could not retrieve parameter \"{0}\"!", name));

				return fVal;
			}

			set {Do(Instance.setParameterByName(name, value), string.Format("Could not set parameter \"{0}\"!", name));}
		}

		public FMODEvent SetParam(string name, float value)
		{
			this[name] = value;
			return this;
		}

		public void Release()
		{
			if(!Instance.isValid())
				return;

			Do(Instance.setCallback(null, EVENT_CALLBACK_TYPE.ALL), "Could not remove instance callback!");
			callbackSink = null;

			Do(Instance.release(), "Could not release event instance!");
		}

		void IDisposable.Dispose() => Release();

    public FMODEvent Start()
		{
			Do(Instance.start(), "Could not start event!");
			return this;
		}

		public FMODEvent Pause()
		{
			Do(Instance.setPaused(true), "Could not pause event!");
			return this;
		}

		public FMODEvent Resume()
		{
			Do(Instance.setPaused(false), "Could not resume event!");
			return this;
		}

		public FMODEvent Stop(bool hard = false)
		{
			Do(Instance.stop(hard ? global::FMOD.Studio.STOP_MODE.IMMEDIATE : global::FMOD.Studio.STOP_MODE.ALLOWFADEOUT), "Could not stop event instance!");
			return this;
		}

		public FMODEvent LoadMarkers()
		{
			markers = FMODMarker.Load(eventPath);
			markersLoaded = true;

			return this;
		}

		public bool HasMarker(string marker)
		{
			if(!markersLoaded)
				LoadMarkers();

			return markers != null && markers.ContainsKey(marker);
		}

		public FMODEvent JumpToMarker(string marker)
		{
			if(!IsValid)
				return this;

			if(!markersLoaded)
				LoadMarkers();

			marker = marker.Trim();
			if(markers == null)
			{
				UnityEngine.Debug.LogError("Unfortunately, there are no markers available - try calling \"LoadMarkers()\" first.");
				return this;
			}

      if(markers.TryGetValue(marker, out int ms))
        TimeMilliseconds = ms;
      else
        UnityEngine.Debug.LogError($"Can't find marker named \"{marker}\"!");

      return this;
		}

		public static void PlayOneShot(string eventPath) => FMODUnity.RuntimeManager.PlayOneShot(eventPath);

		public static void PlayOneShot(string eventPath, Action<FMODEvent> init)
		{
			var ev = new FMODEvent(eventPath);
			init(ev);
			ev.Start().Release();
		}

		public static void KillAllInTransform(Transform transform, bool withChildren)
		{
			MonoBehaviour[] scripts = withChildren ? transform.GetComponentsInChildren<MonoBehaviour>(true) 
																						 : transform.GetComponents<MonoBehaviour>();

			foreach(MonoBehaviour script in scripts)
				KillAllInScript(script);
		}

		public static void KillAllInScript(MonoBehaviour script)
		{
			Type type = script.GetType();
			PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			IEnumerable<FMODEvent> events =	props.Where(prop => typeof(FMODEvent).IsAssignableFrom(prop.PropertyType)).Select(prop => (FMODEvent)prop.GetValue(script, null)).Union(
																			fields.Where(field => typeof(FMODEvent).IsAssignableFrom(field.FieldType)).Select(field => (FMODEvent)field.GetValue(script)));

			foreach(FMODEvent ev in events)
			{
				if(ev != null && ev.IsValid)
					ev.Stop(true).Release();
			}
		}
	}
}