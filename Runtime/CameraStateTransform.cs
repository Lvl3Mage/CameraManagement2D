using UnityEngine;
using Lvl3Mage.InterpolationToolkit.Splines;
using Lvl3Mage.InterpolationToolkit;
namespace Lvl3Mage.CameraManagement2D
{
	public struct CameraStateTransform
	{
		Vector2? translation;
		public Vector2 Translation => translation ?? Vector2.zero;
		
		float? rotationDelta;
		public float RotationDelta => rotationDelta ?? 0;
		
		float? zoomDelta;
		public float ZoomDelta => zoomDelta ?? 0;

		public CameraStateTransform(Vector2? translation = null, float? rotationDelta = null, float? zoomDelta = null)
		{
			this.translation = translation;
			this.rotationDelta = rotationDelta;
			this.zoomDelta = zoomDelta;
		}
		public static CameraStateTransform Empty => new CameraStateTransform();
		public CameraStateTransform WithTranslation(Vector2 translation)
		{
			CameraStateTransform copy = this;
			copy.translation = translation;
			return copy;
		}
		public CameraStateTransform WithRotationDelta(float rotation)
		{
			CameraStateTransform copy = this;
			copy.rotationDelta = rotation;
			return copy;
		}
		public CameraStateTransform WithZoomDelta(float zoom)
		{
			CameraStateTransform copy = this;
			copy.zoomDelta = zoom;
			return copy;
		}
		public CameraStateTransform WithoutTranslation()
		{
			CameraStateTransform copy = this;
			copy.translation = null;
			return copy;
		}
		public CameraStateTransform WithoutRotationDelta()
		{
			CameraStateTransform copy = this;
			copy.rotationDelta = null;
			return copy;
		}
         
		public CameraStateTransform WithoutZoomDelta()
		{
			CameraStateTransform copy = this;
			copy.zoomDelta = null;
			return copy;
		}
		public static CameraStateTransform operator +(CameraStateTransform a, CameraStateTransform b)
		{
			CameraStateTransform result = new(){
				translation = a.Translation+ b.Translation,
				rotationDelta = a.RotationDelta + b.RotationDelta,
				zoomDelta = a.ZoomDelta + b.ZoomDelta
			};
			return result;
		}
		public static CameraStateTransform operator -(CameraStateTransform a, CameraStateTransform b)
		{
			CameraStateTransform result = new(){
				translation = a.translation ?? Vector2.zero - b.translation ?? Vector2.zero,
				rotationDelta = a.RotationDelta - b.RotationDelta,
				zoomDelta = a.ZoomDelta - b.ZoomDelta
			};
			return result;
		}
		public static CameraStateTransform operator *(CameraStateTransform a, float b)
		{
			CameraStateTransform result = new(){
				translation = a.translation ?? Vector2.zero * b,
				rotationDelta = a.RotationDelta * b,
				zoomDelta = a.ZoomDelta * b
			};
			return result;
		}
		public static CameraStateTransform operator *(float b, CameraStateTransform a)
		{
			return a * b;
		}
		public static CameraStateTransform operator /(CameraStateTransform a, float b)
		{
			CameraStateTransform result = new(){
				translation = a.translation ?? Vector2.zero / b,
				rotationDelta = a.RotationDelta / b,
				zoomDelta = a.ZoomDelta / b
			};
			return result;
		}
		public static CameraStateTransform operator -(CameraStateTransform a)
		{
			return a * -1;
		}
		
		public float[] ToArray()
		{
			return new[]{translation?.x ?? 0, translation?.y ?? 0, rotationDelta ?? 0, zoomDelta ?? 0};
		}
		public static CameraStateTransform FromArray(float[] array)
		{
			if (array.Length != 4){
				throw new System.ArgumentException("Array must have a length of 4", nameof(array));
			}
			return new CameraStateTransform(
				new Vector2(array[0], array[1]),
				array[2],
				array[3]
			);
		}
		
		public static TransformSpline CreateTransformSpline(CameraStateTransform control1, CameraStateTransform control2, CameraStateTransform control3, CameraStateTransform control4, I4PointSplineFactory splineFactory)
		{
			ISpline[] splines = splineFactory.CreateSplines(control1.ToArray(), control2.ToArray(), control3.ToArray(), control4.ToArray());
			
			return (t) => {
				float[] values = SplineTools.EvaluateSplines(splines, t);
				return CameraStateTransform.FromArray(values);
			};
		}
		public delegate CameraStateTransform TransformSpline(float t);
		
		
	}
}