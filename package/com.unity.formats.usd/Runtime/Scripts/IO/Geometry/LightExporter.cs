


using UnityEngine;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
	public static class LightExporter
	{
		public static void ExportLight<T>(ObjectContext objContext, ExportContext exportContext) where T : LightSampleBase, new()
		{
			UnityEngine.Profiling.Profiler.BeginSample("USD: Light Conversion");

			T sample = (T)objContext.sample;
			Light light = objContext.gameObject.GetComponent<Light>();
			var path = objContext.path;
			var scene = exportContext.scene;
			bool fastConvert = exportContext.basisTransform == BasisTransformation.FastWithNegativeScale;

			sample.CopyFromLight(light, convertTransformToUsd: !fastConvert);

			if (fastConvert)
			{
				// Partial change of basis.
                var basisChange = Matrix4x4.identity;
                // Invert the forward vector.
                basisChange[2, 2] = -1;
                // Full change of basis would be b*t*b-1, but here we're placing only a single inversion
                // at the root of the hierarchy, so all we need to do is get the camera into the same
                // space.
                sample.transform = sample.transform * basisChange;

                // Is this also a root path?
                // If so the partial basis conversion must be completed on the camera itself.
                if (path.LastIndexOf("/") == 0)
                {
                    sample.transform = basisChange * sample.transform;
                }
			}

			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("USD: Light Write");
            scene.Write(path, sample);
            UnityEngine.Profiling.Profiler.EndSample();
		}
	}
}