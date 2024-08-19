using System.Collections;
using System.Collections.Generic;
using Lvl3Mage.CameraManagement2D;
using MyBox;
using UnityEngine;

namespace Lvl3Mage.CameraManagement2D
{
	/// <summary>
	/// Provides functionality to clamp the camera state, including position, bounds, and zoom.
	/// </summary>
	[System.Serializable]
	public class CameraStateClamp
	{
		public CameraStateClamp(ClampMode mode, Bounds bounds, Vector2 zoomClamp)
		{
			clampMode = mode;
			xClamp = new Vector2(bounds.min.x, bounds.max.x);
			clampXAxis = true;
			yClamp = new Vector2(bounds.min.y, bounds.max.y);
			clampYAxis = true;
			zoomClamp = zoomClamp;
			clampZoom = true;
		}
		/// <summary>
		/// Modes for clamping the camera state.
		/// </summary>
		public enum ClampMode
		{
			/// <summary>
			/// Clamps the camera state based on the center position.
			/// </summary>
			ClampPosition,
			/// <summary>
			/// Clamps the camera state based on the camera bounds.
			/// </summary>
			ClampBounds
		}
		/// <summary>
		/// The mode for clamping the camera state.
		/// </summary>
		[SerializeField] ClampMode clampMode = ClampMode.ClampPosition;
		/// <summary>
		/// Indicates whether the X-axis should be clamped.
		/// </summary>
		[SerializeField] bool clampXAxis = true;
		/// <summary>
		/// The range for clamping the X-axis.
		/// </summary>
		[ConditionalField("clampXAxis")] [SerializeField]
		Vector2 xClamp = new Vector2(-10, 10);
		/// <summary>
		/// Indicates whether the Y-axis should be clamped.
		/// </summary>
		[SerializeField] bool clampYAxis = true;
		/// <summary>
		/// The range for clamping the Y-axis.
		/// </summary>
		[ConditionalField("clampYAxis")] [SerializeField]
		Vector2 yClamp = new Vector2(-10, 10);
		/// <summary>
		/// Indicates whether the zoom should be clamped.
		/// </summary>
		[SerializeField] bool clampZoom = true;
		/// <summary>
		/// The range for clamping the zoom level.
		/// </summary>
		[ConditionalField("clampZoom")] [SerializeField]
		Vector2 zoomClamp = new Vector2(0.1f, 15);
		
		/// <summary>
		/// Clamps the camera state based on the configured clamp settings.
		/// </summary>
		/// <param name="state2D">The camera state to clamp.</param>
		/// <param name="cameraAspect">The aspect ratio of the camera.</param>
		/// <returns>The clamped camera state.</returns>
		public CameraState ClampState(CameraState state, float cameraAspect)
		{
			if (clampZoom){
				state = state.ClampedZoom(zoomClamp);
			}

			if (clampMode == ClampMode.ClampPosition){
				state = ClampPosition(state);
			}
			else if (clampMode == ClampMode.ClampBounds){
				state = ClampBounds(state, cameraAspect);
			}
			//Todo clamp rotation
			return state;
		}
		/// <summary>
		/// Clamps the position of the camera state based on the configured X and Y clamps.
		/// </summary>
		/// <param name="state2D">The camera state to clamp.</param>
		/// <returns>The clamped camera state.</returns>
		CameraState ClampPosition(CameraState state)
		{
			if (clampXAxis){
				state = state.ClampedPositionX(xClamp);
			}

			if (clampYAxis){
				state = state.ClampedPositionY(yClamp);
			}

			return state;
		}
		/// <summary>
		/// Clamps the bounds of the camera state based on the configured X and Y clamps.
		/// </summary>
		/// <param name="state2D">The camera state to clamp.</param>
		/// <param name="cameraAspect">The aspect ratio of the camera.</param>
		/// <returns>The clamped camera state.</returns>
		CameraState ClampBounds(CameraState state, float cameraAspect)
		{
			Bounds bounds = state.GetBounds(cameraAspect);
			Vector3 correction = Vector3.zero;
			if (clampXAxis){
				float xCorrection = GetCorrection(bounds.min.x, bounds.max.x, xClamp.x, xClamp.y);
				correction += new Vector3(xCorrection, 0, 0);
			}

			if (clampYAxis){
				float yCorrection = GetCorrection(bounds.min.y, bounds.max.y, yClamp.x, yClamp.y);
				correction += new Vector3(0, yCorrection, 0);
			}

			bounds.center += correction;
			if(!clampXAxis || !clampYAxis){
				return CameraState.ContainedInBounds(bounds, cameraAspect);
			}
			bounds.max = Vector3.Min(bounds.max, new Vector3(xClamp.y, yClamp.y, 0));
			bounds.min = Vector3.Max(bounds.min, new Vector3(xClamp.x, yClamp.x, 0));
			Vector2 correctionSign = new Vector2(correction.x == 0 ? 0 : Mathf.Sign(correction.x),
				correction.y == 0 ? 0 : Mathf.Sign(correction.y));
			Vector2 pivot = new Vector2(0.5f, 0.5f) - correctionSign * 0.5f;

			return CameraState.ContainedInBounds(bounds, cameraAspect, pivot);
		}
		/// <summary>
		/// Calculates the correction needed to keep the bounds within the specified clamp range.
		/// </summary>
		/// <param name="min">The minimum value of the bounds.</param>
		/// <param name="max">The maximum value of the bounds.</param>
		/// <param name="clampMin">The minimum clamp value.</param>
		/// <param name="clampMax">The maximum clamp value.</param>
		/// <returns>The correction value.</returns>
		float GetCorrection(float min, float max, float clampMin, float clampMax)
		{
			//positive when correction is needed
			float minCorrection = Mathf.Max(clampMin - min, 0);

			//negative when correction is needed
			float maxCorrection = Mathf.Min(clampMax - max, 0);

			//Selecting the biggest correction
			return (minCorrection > -maxCorrection) ? minCorrection : maxCorrection;
		}

		/// <summary>
		/// Draws the clamp bounds in the editor using gizmos.
		/// </summary>
		public void DrawGizmos(Color? color = null) 
		{
			Vector2 clampX = clampXAxis ? xClamp : Vector2.zero;
			Vector2 clampY = clampYAxis ? yClamp : Vector2.zero;
			Vector3[] corners ={
				new Vector3(clampX.x, clampY.x, 0),
				new Vector3(clampX.x, clampY.y, 0),
				new Vector3(clampX.y, clampY.y, 0),
				new Vector3(clampX.y, clampY.x, 0)
			};
			Gizmos.color = color ?? Color.red;
			DrawGizmosPath(corners);
		}

		void DrawGizmosPath(Vector3[] points)
		{
			for (int i = 0; i < points.Length; i++){
				Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
			}
		}
	}
}