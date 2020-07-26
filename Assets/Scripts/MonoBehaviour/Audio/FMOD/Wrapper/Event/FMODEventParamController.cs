using UnityEngine;

namespace SwinguinGames.FMOD
{
	public class FMODEventParamController : MonoBehaviour
	{
		[SerializeField] private string paramName;
		public string ParamName {get => paramName; set => paramName = value;}
		
		private FMODEventController ev;

    public void SetValue(float value)
		{
			if(ev == null)
				ev = GetComponentInChildren<FMODEventController>();

			if(ev == null)
			{
        Debug.LogError("No FMODEventController was found on this object or any of its children!");
				return;
			}

			ev.Event[paramName] = value;
		}
	}
}