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
		public CameraState(CameraState other)
		{
			position = other.position;
			zoom = other.zoom;
			rotation = other.rotation;
			local = other.local;
		}
		public CameraState Clone()
		{
			return new CameraState(this);
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
		public static CameraState CoveringBounds(Bounds bounds, float aspectRatio)
		{
			return CoveringBounds(bounds, aspectRatio, new Vector2(0.5f, 0.5f));
		}
		public static CameraState CoveringBounds(Bounds bounds, float aspectRatio, Vector2 pivot)
		{
			Vector2 pos = bounds.center;
			Vector2 offset = Vector2.zero;
			float sizeFromX = bounds.size.x / aspectRatio;
			float sizeFromY = bounds.size.y;
			float size = Mathf.Max(sizeFromX, sizeFromY);
			if(sizeFromX < sizeFromY){
				offset.x = (bounds.size.x - size*aspectRatio) * ( pivot.x - 0.5f);
			}
			else{
				offset.y = (bounds.size.y - size) * (pivot.y - 0.5f);
			}
			pos += offset;
			return Empty().WithPosition(pos).WithZoom(size*0.5f);
		}
		public static CameraState ContainingBounds(Bounds bounds, float aspectRatio)
		{
			return ContainingBounds(bounds, aspectRatio, new Vector2(0.5f, 0.5f));
		}
		public static CameraState ContainingBounds(Bounds bounds, float aspectRatio, Vector2 pivot)
		{
			Vector2 pos = bounds.center;
			Vector2 offset = Vector2.zero;
			float sizeFromX = bounds.size.x / aspectRatio;
			float sizeFromY = bounds.size.y;
			float size = Mathf.Min(sizeFromX, sizeFromY);
			if(sizeFromX > sizeFromY){
				offset.x = (bounds.size.x - size*aspectRatio) * ( pivot.x - 0.5f);
			}
			else{
				offset.y = (bounds.size.y - size) * (pivot.y - 0.5f);
			}
			pos += offset;
			return Empty().WithPosition(pos).WithZoom(size*0.5f);
		}
		public Bounds GetBounds(float aspectRatio)
		{
			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
			if(position != null){
				bounds.center = position.Value;
			}
			if(zoom != null){
				bounds.size = new Vector3(zoom.Value * aspectRatio * 2, zoom.Value * 2, 0);
			}
			//Todo implement axis aligned bounds when rotation is present
			return bounds;
		}
		
		
		public CameraState WithPosition(Vector2? position)
		{
			
			CameraState newState = new(this){
				position = position
			};
			return newState;
		}
		public CameraState WithPosition(Vector2 position)
		{
			CameraState newState = new(this){
				position = position
			};
			return newState;
		}
		
		public CameraState WithPosition(CameraState other)
		{
			return WithPosition(other.position);
		}
		public CameraState WithoutPosition()
		{
			CameraState newState = new(this){
				position = null
			};
			return newState;
		}
		
		
		public CameraState WithZoom(float zoom)
		{
			CameraState newState = new(this){
				zoom = zoom
			};
			return newState;
		}
		public CameraState WithZoom(float? zoom)
		{
			CameraState newState = new(this){
				zoom = zoom
			};
			return newState;
		}
		public CameraState WithZoom(CameraState other)
		{
			return WithZoom(other.zoom);
		}
		public CameraState WithoutZoom()
		{
			CameraState newState = new(this){
				zoom = null
			};
			return newState;
		}
		
		
		public CameraState WithRotation(float rotation)
		{
			CameraState newState = new(this){
				rotation = rotation
			};
			return newState;
		}
		public CameraState WithRotation(float? rotation)
		{
			CameraState newState = new(this){
				rotation = rotation
			};
			return newState;
		}
		public CameraState WithRotation(CameraState other)
		{
			return WithRotation(other.rotation);
		}
		public CameraState WithoutRotation()
		{
			CameraState newState = new(this){
				rotation = null
			};
			return newState;
		}

		public CameraState AsLocal(bool local = true)
		{
			CameraState newState = new(this){
				local = local
			};
			return newState;
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
		public CameraState DecayPositionTo(CameraState target, float factor, float deltaTime = -1)
		{
			if(deltaTime < 0){
				deltaTime = Time.deltaTime;
			}
			if (position == null){
				return Clone();
			}
			if(target.position == null){
				return Clone();
			}
			return WithPosition(Decay.To(position.Value, target.position.Value, factor, deltaTime));
		}
		
		/// <summary>
		/// Decay the zoom of the camera to a target zoom
		/// </summary>
		/// <returns>A clone of the state with modified zoom</returns>
		public CameraState DecayZoomTo(CameraState target, float factor, float deltaTime = -1)
		{
			if(deltaTime < 0){
				deltaTime = Time.deltaTime;
			}
			if (zoom == null){
				return Clone();
			}
			if(target.zoom == null){
				return Clone();
			}
			return WithZoom(Decay.ToZoom(zoom.Value, target.zoom.Value, factor, deltaTime));
		}
		/// <summary>
		/// Decay the rotation of the camera to a target rotation
		/// </summary>
		/// <returns>A clone of the state with modified rotation</returns>
		public CameraState DecayRotationTo(CameraState target, float factor, float deltaTime = -1)
		{
			if(deltaTime < 0){
				deltaTime = Time.deltaTime;
			}
			if (rotation == null){
				return Clone();
			}
			if(target.rotation == null){
				return Clone();
			}
			return WithRotation(Decay.ToAngle(rotation.Value, target.rotation.Value, factor, deltaTime));
		}

		public CameraState ZoomTowards(float newZoom, Vector2 target)
		{
			if (zoom == null){
				return Clone();
			}
			CameraState newState = Clone();
			
			if(position != null){
				Vector2 offset = target - position.Value;
				Vector2 newOffset = offset * (newZoom / newState.zoom.Value);
				
				newState.position += offset - newOffset;
			}
			newState.zoom = newZoom;
			return newState;
		}
		
		public CameraState ZoomTowards(CameraState other, Vector2 target)
		{
			if (zoom == null){
				return Clone();
			}
			if(other.zoom == null){
				return Clone();
			}
			CameraState newState = Clone();
			
			if(position != null){
				Vector2 offset = target - position.Value;
				Vector2 newOffset = offset * (other.zoom.Value / newState.zoom.Value);
				
				newState.position += offset - newOffset;
			}
			newState.zoom = other.zoom.Value;
			return newState;
		}
		public CameraState ExponentialZoom(float value, Vector2 target)
		{
			if (zoom == null){
				return Clone();
			}
			float newZoom = Mathf.Exp(Mathf.Log(zoom.Value) + value);
			return ZoomTowards(newZoom, target);
		}
		public CameraState ExponentialZoom(float value)
		{
			if (zoom == null){
				return Clone();
			}
			return WithZoom(Mathf.Exp(Mathf.Log(zoom.Value) + value));
		}
		public CameraState ClampedZoom(float min, float max)
		{
			if (zoom == null){
				return Clone();
			}
			return WithZoom(Mathf.Clamp(zoom.Value, min, max));
		}
		public CameraState ClampedZoom(Vector2 clamp)
		{
			return ClampedZoom(clamp.x, clamp.y);
		}
		public CameraState ClampedPosition(Vector2 clampX, Vector2 clampY)
		{
			if (position == null){
				return Clone();
			}
			return WithPosition(new Vector2(
				Mathf.Clamp(position.Value.x, clampX.x, clampX.y),
				Mathf.Clamp(position.Value.y, clampY.x, clampY.y)
			));
		}
		public CameraState ClampedPositionX(Vector2 clamp)
		{
			if (position == null){
				return Clone();
			}
			return WithPosition(new Vector2(
				Mathf.Clamp(position.Value.x, clamp.x, clamp.y),
				position.Value.y
			));
		}
		public CameraState ClampedPositionY(Vector2 clamp)
		{
			if (position == null){
				return Clone();
			}
			return WithPosition(new Vector2(
				position.Value.x,
				Mathf.Clamp(position.Value.y, clamp.x, clamp.y)
			));
		}
	}
}