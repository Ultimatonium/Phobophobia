using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SwinguinGames.FMOD
{
	public static class JSONUtils
	{
		[System.Serializable]
		private class Wrapper<T>
		{
			public T[] array = null;
		}

		public static T[] DeserializeArray<T>(string json)
		{
			string newJSON = "{\"array\": " + json + "}";
			Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJSON);

			return wrapper.array;
		}
	}
}