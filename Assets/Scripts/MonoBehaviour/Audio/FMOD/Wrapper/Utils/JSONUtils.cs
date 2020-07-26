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
			Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(newJSON);

			return wrapper.array;
		}
	}
}