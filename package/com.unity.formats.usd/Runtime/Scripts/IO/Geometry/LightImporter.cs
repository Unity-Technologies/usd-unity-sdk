

using UnityEngine;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
	public static class LightImporter<T> where T : LightSampleBase
	{
		public static void BuildLight(T usdLight,
			GameObject go,
			SceneImportOptions options)
		{
			var light = ImporterBase.GetOrAddComponent<Light>(go);
			usdLight.CopyToLight(light, setTransform: false);
		}
	}
}