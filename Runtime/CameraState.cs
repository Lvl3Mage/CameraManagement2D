using UnityEngine;
using Interpolation;

namespace CameraManagement2D
{
	public struct CameraState
	{
		Vector2? position;
		public Vector2 Position
		{
			get
			{
				if (position == null){
					Debug.LogError("Position is null! Defaulting to Vector2.zero");
					return Vector2.zero;
				}
				return position.Value;
			}
		}
		float? zoom;
		public float Zoom
		{
			get
			{
				if (zoom == null){
					Debug.LogError("Zoom is null! Defaulting to 1.0f");
					return 1;
				}
				return zoom.Value;
			}
		}
		float? rotation;
		public float Rotation
		{
			get
			{
				if (rotation == null){
					Debug.LogError("Rotation is null! Defaulting to 0.0f");
					return 0;
				}
				return rotation.Value;
			}
		}
		bool local;
		public CameraState(Vector2 position, float zoom, float rotation, bool local = false)
		{
			this.position = position;
			this.zoom = zoom;
			this.rotation = rotation;
			this.local = local;
		}
		
		public static CameraState FromCamera(Camera camera, bool local = false)
		{
			Vector2 pos = camera.transform.position;
			if (local){
				pos = camera.transform.localPosition;
			}
			float rot = camera.transform.rotation.eulerAngles.z;
			if (local){
				rot = camera.transform.localRotation.eulerAngles.z;
			}
			return new CameraState(pos, camera.orthographicSize, rot);
		}
		public static CameraState Empty()
		{
			return new CameraState();
		}
		
		public CameraState WithPosition(Vector2 position)
		{
			this.position = position;
			return this;
		}
		public CameraState WithPosition(Vector2? position)
		{
			this.position = position;
			return this;
		}
		
		public CameraState WithPosition(CameraState other)
		{
			position = other.position ?? position;
			return this;
		}
		public CameraState WithoutPosition()
		{
			position = null;
			return this;
		}
		
		
		public CameraState WithZoom(float zoom)
		{
			this.zoom = zoom;
			return this;
		}
		public CameraState WithZoom(float? zoom)
		{
			this.zoom = zoom;
			return this;
		}
		public CameraState WithZoom(CameraState other)
		{
			zoom = other.zoom ?? zoom;
			return this;
		}
		public CameraState WithoutZoom()
		{
			zoom = null;
			return this;
		}
		
		
		public CameraState WithRotation(float rotation)
		{
			this.rotation = rotation;
			return this;
		}
		public CameraState WithRotation(float? rotation)
		{
			this.rotation = rotation;
			return this;
		}
		public CameraState WithRotation(CameraState other)
		{
			rotation = other.rotation ?? rotation;
			return this;
		}
		public CameraState WithoutRotation()
		{
			rotation = null;
			return this;
		}

		public CameraState AsLocal(bool local = true)
		{
			this.local = local;
			return this;
		}
		
		public void ApplyTo(Camera camera)
		{
			if (position != null){
				Vector3 pos = new(position.Value.x, position.Value.y, camera.transform.position.z);
				if (local){
					pos.z = camera.transform.localPosition.z;
				}

				if (local){
					camera.transform.localPosition = pos;
				}
				else{
					camera.transform.position = pos;
				}
			}
			if (rotation != null){
				Quaternion rot = Quaternion.Euler(0, 0, rotation.Value);
				if (local){
					camera.transform.localRotation = rot;
				}
				else{
					camera.transform.rotation = rot;
				}
			}
			if (zoom != null){
				camera.orthographicSize = zoom.Value;
			}
		}
		public bool RepresentsZoom()
		{
			return zoom != null;
		}
		public bool RepresentsRotation()
		{
			return rotation != null;
		}
		public bool RepresentsPosition()
		{
			return position != null;
		}
		public bool IsEmpty()
		{
			return position == null && zoom == null && rotation == null;
		}
		/// <summary>
		/// Decay the position of the camera to a target position
		/// </summary>
		/// <returns>A clone of the state with modified position</returns>
		public CameraState DecayPositionTo(CameraState target, float factor)
		{
			if (position == null){
				return this;
			}
			if(target.position == null){
				return this;
			}
			position = Decay.To(position.Value, target.position.Value, Time.deltaTime, factor);
			return this;
		}
		
		/// <summary>
		/// Decay the zoom of the camera to a target zoom
		/// </summary>
		/// <returns>A clone of the state with modified zoom</returns>
		public CameraState DecayZoomTo(CameraState target, float factor)
		{
			if (zoom == null){
				return this;
			}
			if(target.zoom == null){
				return this;
			}
			zoom = Decay.ToZoom(zoom.Value, target.zoom.Value, Time.deltaTime, factor);
			return this;
		}
		/// <summary>
		/// Decay the rotation of the camera to a target rotation
		/// </summary>
		/// <returns>A clone of the state with modified rotation</returns>
		public CameraState DecayRotationTo(CameraState target, float factor)
		{
			if (rotation == null){
				return this;
			}
			if(target.rotation == null){
				return this;
			}
			rotation = Decay.ToAngle(rotation.Value, target.rotation.Value, Time.deltaTime, factor);
			return this;
		}
		
		public CameraState ExponentialZoom(float value)
		{
			if (zoom == null){
				return this;
			}
			zoom = Mathf.Exp(Mathf.Log(zoom.Value) + value);
			return this;
		}
		public CameraState ClampedZoom(float min, float max)
		{
			if (zoom == null){
				return this;
			}
			zoom = Mathf.Clamp(zoom.Value, min, max);
			return this;
		}
		public CameraState ClampedZoom(Vector2 clamp)
		{
			return ClampedZoom(clamp.x, clamp.y);
		}
		public CameraState ClampedPosition(Vector2 clampX, Vector2 clampY)
		{
			if (position == null){
				return this;
			}
			position = new Vector2(
				Mathf.Clamp(position.Value.x, clampX.x, clampX.y),
				Mathf.Clamp(position.Value.y, clampY.x, clampY.y)
			);
			return this;
		}
		public CameraState ClampedPositionX(Vector2 clamp)
		{
			if (position == null){
				return this;
			}
			position = new Vector2(
				Mathf.Clamp(position.Value.x, clamp.x, clamp.y),
				position.Value.y
			);
			return this;
		}
		public CameraState ClampedPositionY(Vector2 clamp)
		{
			if (position == null){
				return this;
			}
			position = new Vector2(
				position.Value.x,
				Mathf.Clamp(position.Value.y, clamp.x, clamp.y)
			);
			return this;
		}
	}
}