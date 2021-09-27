using UnityEngine;

namespace USD.NET.Unity
{
	[System.Serializable]
	public class LightSampleBase : XformSample
	{
		public LightSampleBase()
		{
		}

		public virtual void CopyFromLight(UnityEngine.Light light, bool convertTransformToUsd = true)
		{
			var tr = light.transform;
			transform = UnityEngine.Matrix4x4.TRS(tr.localPosition,
                tr.localRotation,
                tr.localScale);
            if (convertTransformToUsd)
            {
                ConvertTransform();
            }
		}

		public virtual void CopyToLight(UnityEngine.Light light, bool setTransform)
		{
			if (setTransform)
            {
                var tr = light.transform;
                var xf = transform;
                UnityTypeConverter.SetTransform(xf, tr);
            }
		}
	}

	[System.Serializable]
	[UsdSchema("DistantLight")]
	public class DistantLightSample : LightSampleBase
	{
		// Core Light parameters
		public float angle;
		public float intensity;

		public DistantLightSample()
		{
		}

		public DistantLightSample(UnityEngine.Light fromLight)
		{
			CopyFromLight(fromLight);
		}

		override public void CopyFromLight(UnityEngine.Light light, bool convertTransformToUsd = true)
		{
			intensity = light.intensity;
			base.CopyFromLight(light, convertTransformToUsd);
		}

		override public void CopyToLight(UnityEngine.Light light, bool setTransform)
		{
			light.type = LightType.Directional;
			light.intensity = intensity;
			base.CopyToLight(light, setTransform);
		}
	}

	[System.Serializable]
	[UsdSchema("SphereLight")]
	public class SphereLightSample : LightSampleBase
	{
		// Core Light parameters
		public bool treatAsPoint;
		public float radius;

		[UsdNamespace("shaping:cone")]
		public float angle;

		public SphereLightSample()
		{
		}

		public SphereLightSample(UnityEngine.Light fromLight)
		{
			CopyFromLight(fromLight);
		}

		override public void CopyFromLight(UnityEngine.Light light, bool convertTransformToUsd = true)
		{
			treatAsPoint = true;
			radius = light.range;
			if (light.spotAngle > 0)
			{
				angle = light.spotAngle;
			}
			base.CopyFromLight(light, convertTransformToUsd);
		}

		override public void CopyToLight(UnityEngine.Light light, bool setTransform)
		{
			if (angle > 0)
			{
				light.type = LightType.Spot;
				light.spotAngle = angle;
			}
			else
			{
				light.type = LightType.Point;
			}

			light.range = radius;
			base.CopyToLight(light, setTransform);
		}
	}

	[System.Serializable]
	[UsdSchema("RectLight")]
	public class RectLightSample : LightSampleBase
	{
		public RectLightSample()
		{
		}

		public RectLightSample(UnityEngine.Light fromLight)
		{
			base.CopyFromLight(fromLight);
		}

		override public void CopyToLight(UnityEngine.Light light, bool setTransform)
		{
			light.type = LightType.Rectangle;
			base.CopyToLight(light, setTransform);
		}
	}

	[System.Serializable]
	[UsdSchema("DiskLight")]
	public class DiskLightSample : LightSampleBase
	{
		public DiskLightSample()
		{
		}

		public DiskLightSample(UnityEngine.Light fromLight)
		{
			base.CopyFromLight(fromLight);
		}

		override public void CopyToLight(UnityEngine.Light light, bool setTransform)
		{
			light.type = LightType.Disc;
			base.CopyToLight(light, setTransform);
		}
	}
}