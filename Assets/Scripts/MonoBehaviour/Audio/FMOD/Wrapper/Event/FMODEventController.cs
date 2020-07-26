using UnityEngine;

namespace SwinguinGames.FMOD
{
	public class FMODEventController : MonoBehaviour
	{
    [FMODUnity.EventRef][SerializeField] private string[] eventPaths;
		public string[] EventPaths {get => eventPaths; set => eventPaths = value;}
		[SerializeField] private bool startAutomatically = false;
		public bool StartAutomatically {get => startAutomatically; set => startAutomatically = value;}

		public FMODEvent[] Events {get; private set;}
		public FMODEvent Event => Events[0];

		public float this[string paramName]
		{
			set
			{
				for(int i = 0; i < Events.Length; i++)
				{
					if(Events[i] != null && Events[i].IsValid)
						Events[i][paramName] = value;
				}
			}
		}

		private void Awake()
		{
			Events = new FMODEvent[eventPaths.Length];
			for(int i = 0; i < Events.Length; i++)
				Events[i] = new FMODEvent(eventPaths[i]).LoadMarkers();
		}

		private void Start()
		{
			if(startAutomatically)
				StartEvents();
		}

		public void StartEvents()
		{
			for(int i = 0; i < Events.Length; i++)
				Events[i].Start();
		}

		public void StopEvents(bool release)
		{
			for(int i = 0; i < Events.Length; i++)
			{
				if(Events[i] != null && Events[i].IsValid)
				{
					Events[i].Stop();
					if(release)
						Events[i].Release();
				}
			}
		}

		public void ReleaseEvents()
		{
			for(int i = 0; i < Events.Length; i++)
			{
				if(Events[i] != null && Events[i].IsValid)
				{
					Events[i].Release();
				}
			}
		}

		private void Update()
		{
			for(int i = 0; i < Events.Length; i++)
			{
				if(Events[i] != null && Events[i].IsValid)
					Events[i].Position = transform.position;
			}
		}

		private void OnDestroy() => StopEvents(true);

		public void JumpTo(string marker)
		{
			int appliedTo = 0;
			for(int i = 0; i < Events.Length; i++)
			{
				if(Events[i] == null || !Events[i].IsValid || !Events[i].HasMarker(marker))
					continue;

				appliedTo++;
				Events[i].JumpToMarker(marker);
			}

			if(appliedTo < 1)
        Debug.LogError("Marker was not applied to any events!");
		}
	}
}