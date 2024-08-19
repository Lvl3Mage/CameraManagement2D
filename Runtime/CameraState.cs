using UnityEngine;
using Lvl3Mage.InterpolationToolkit;

namespace Lvl3Mage.CameraManagement2D
{
	/// <summary>
	/// Encapsulates the state of a 2D camera, including its position, zoom level, and rotation. It provides functionality to manage, modify, and apply camera states in a flexible manner.
	/// </summary>
	public struct CameraState
	{
		private Vector2? position;
		/// <summary>
		/// Gets the position of the camera. If null the position is not represented.
		/// </summary>
		public Vector2? Position
		{
			get => position;
		}

		private float? zoom;
		/// <summary>
		/// Gets the zoom level of the camera. If null the zoom is not represented.
		/// </summary>
		public float? Zoom => zoom;

		private float? rotation;
		/// <summary>
		/// Gets the rotation of the camera. If null the rotation is not represented.
		/// </summary>
		public float? Rotation => rotation;

		/// <summary>
		/// Initializes a new instance of the CameraState class with the specified position, zoom, rotation
		/// </summary>
		/// <param name="position">The position of the camera.</param>
		/// <param name="zoom">The zoom level of the camera.</param>
		/// <param name="rotation">The rotation of the camera.</param>
		public CameraState(Vector2 position, float zoom, float rotation)
		{
			this.position = position;
			this.zoom = zoom;
			this.rotation = rotation;
		}
		/// <summary>
		/// Initializes a new instance of the CameraState class by copying another CameraState.
		/// </summary>
		/// <param name="other">The CameraState to copy.</param>
		public CameraState(CameraState other)
		{
			position = other.position;
			zoom = other.zoom;
			rotation = other.rotation;
		}
		/// <summary>
		/// Creates a new CameraState that is a copy of the current instance.
		/// </summary>
		/// <returns>A new CameraState that is a copy of the current instance.</returns>
		public CameraState Clone()
		{
			return new CameraState(this);
		}
		/// <summary>
		/// Creates a new CameraState from the given Camera.
		/// </summary>
		/// <param name="camera">The Camera to create the state from.</param>
		/// <param name="local">Indicates whether the position and rotation are local to the parent transform.</param>
		/// <returns>A new CameraState representing the state of the given Camera.</returns>
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
		/// <summary>
		/// Creates an empty CameraState with no position, zoom, or rotation values.
		/// </summary>
		/// <returns>An empty CameraState.</returns>
		public static CameraState Empty()
		{
			return new CameraState();
		}
		
		/// <summary>
		/// Creates a CameraState that covers the given bounds with the specified aspect ratio. The bounds will be centered in the camera.
		/// </summary>
		/// <param name="bounds">The bounds the camera should cover.</param>
		/// <param name="aspectRatio">The aspect ratio of the camera.</param>
		/// <returns>A new CameraState that covers the given bounds.</returns>
		public static CameraState CoveringBounds(Bounds bounds, float aspectRatio)
		{
			return CoveringBounds(bounds, aspectRatio, new Vector2(0.5f, 0.5f));
		}
		/// <summary>
		/// Creates a CameraState that covers the given bounds with the specified aspect ratio and pivot.
		/// </summary>
		/// <param name="bounds">The bounds the camera should cover.</param>
		/// <param name="aspectRatio">The aspect ratio of the camera.</param>
		/// <param name="pivot">The pivot point for the camera. A value of (0.5, 0.5) will center the camera</param>
		/// <returns>A new CameraState that covers the given bounds.</returns>
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
		
		/// <summary>
		/// Creates a CameraState that is contained the given bounds with the specified aspect ratio. The camera will be centered in the bounds.
		/// </summary>
		/// <param name="bounds">The bounds that should contain the camera.</param>
		/// <param name="aspectRatio">The aspect ratio of the camera.</param>
		/// <returns>A new CameraState that is contained the given bounds.</returns>
		public static CameraState ContainedInBounds(Bounds bounds, float aspectRatio)
		{
			return ContainedInBounds(bounds, aspectRatio, new Vector2(0.5f, 0.5f));
		}
		
		//Todo implement Contained in bounds but with a rotation instead of pivot (as well as coveringBounds with rotation)
		/// <summary>
		/// Creates a CameraState that is contained the given bounds with the specified aspect ratio and pivot.
		/// </summary>
		/// <param name="bounds">The bounds that should contain the camera.</param>
		/// <param name="aspectRatio">The aspect ratio of the camera.</param>
		/// <param name="pivot">The pivot point for the camera. A value of (0.5, 0.5) will center the camera</param>
		/// <returns>A new CameraState that is contained the given bounds.</returns>
		public static CameraState ContainedInBounds(Bounds bounds, float aspectRatio, Vector2 pivot)
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
		/// <summary>
		/// Gets the bounds of the CameraState with a specified aspect ratio.
		/// Currently does not take rotation into account.
		/// </summary>
		/// <param name="aspectRatio">The aspect ratio of the bounds.</param>
		/// <returns>A Bounds object that encompasses the camera state</returns>
		public Bounds GetBounds(float aspectRatio)
		{
			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
			if(position != null){
				bounds.center = position.Value;
			}
			if(zoom != null){
				bounds.size = new Vector3(zoom.Value * aspectRatio * 2, zoom.Value * 2, 0);
			}
			//\todo implement axis aligned bounds when rotation is present
			return bounds;
		}
		
		/// <summary>
		/// Creates a new CameraState with the specified position value.
		/// </summary>
		/// <param name="position">The position value to set.</param>
		/// <returns>A new CameraState with the updated position value.</returns>
		public CameraState WithPosition(Vector2? position)
		{
			
			CameraState newState = new(this){
				position = position
			};
			return newState;
		}
		/// <summary>
		/// Creates a new CameraState with the specified position value.
		/// </summary>
		/// <param name="position">The position value to set.</param>
		/// <returns>A new CameraState with the updated position value.</returns>
		public CameraState WithPosition(Vector2 position)
		{
			CameraState newState = new(this){
				position = position
			};
			return newState;
		}
		/// <summary>
		/// Creates a new CameraState with the position value from another CameraState.
		/// </summary>
		/// <param name="other">The CameraState to copy the position value from.</param>
		/// <returns>A new CameraState with the updated position value.</returns>
		public CameraState WithPosition(CameraState other)
		{
			return WithPosition(other.position);
		}
		/// <summary>
		/// Creates a new CameraState without a position value.
		/// </summary>
		/// <returns>A new CameraState that doesn't represent a position</returns>
		public CameraState WithoutPosition()
		{
			CameraState newState = new(this){
				position = null
			};
			return newState;
		}
		
		
		/// <summary>
		/// Creates a new CameraState with the specified zoom value.
		/// </summary>
		/// <param name="zoom">The zoom value to set.</param>
		/// <returns>A new CameraState with the updated zoom value.</returns>
		public CameraState WithZoom(float zoom)
		{
			CameraState newState = new(this){
				zoom = zoom
			};
			return newState;
		}
		/// <summary>
		/// Creates a new CameraState with the specified zoom value.
		/// </summary>
		/// <param name="zoom">The zoom value to set.</param>
		/// <returns>A new CameraState with the updated zoom value.</returns>
		public CameraState WithZoom(float? zoom)
		{
			CameraState newState = new(this){
				zoom = zoom
			};
			return newState;
		}
		/// <summary>
		/// Creates a new CameraState with the zoom value from another CameraState.
		/// </summary>
		/// <param name="other">The CameraState to copy the zoom value from.</param>
		/// <returns>A new CameraState with the updated zoom value.</returns>
		public CameraState WithZoom(CameraState other)
		{
			return WithZoom(other.zoom);
		}
		/// <summary>
		/// Creates a new CameraState without a zoom value.
		/// </summary>
		/// <returns>A new CameraState that doesn't represent a zoom</returns>
		public CameraState WithoutZoom()
		{
			CameraState newState = new(this){
				zoom = null
			};
			return newState;
		}
		
		
		/// <summary>
		/// Creates a new CameraState with the specified rotation value.
		/// </summary>
		/// <param name="rotation">The rotation value to set.</param>
		/// <returns>A new CameraState with the updated rotation value.</returns>
		public CameraState WithRotation(float rotation)
		{
			CameraState newState = new(this){
				rotation = rotation
			};
			return newState;
		}
		/// <summary>
		/// Creates a new CameraState with the specified rotation value.
		/// </summary>
		/// <param name="rotation">The rotation value to set.</param>
		/// <returns>A new CameraState with the updated rotation value.</returns>
		public CameraState WithRotation(float? rotation)
		{
			CameraState newState = new(this){
				rotation = rotation
			};
			return newState;
		}
		/// <summary>
		/// Creates a new CameraState with the rotation value from another CameraState.
		/// </summary>
		/// <param name="other">The CameraState to copy the rotation value from.</param>
		/// <returns>A new CameraState with the updated rotation value.</returns>
		public CameraState WithRotation(CameraState other)
		{
			return WithRotation(other.rotation);
		}
		
		/// <summary>
		/// Creates a new CameraState without a rotation value.
		/// </summary>
		/// <returns>A new CameraState that doesn't represent a rotation</returns>
		public CameraState WithoutRotation()
		{
			CameraState newState = new(this){
				rotation = null
			};
			return newState;
		}
		
		public CameraState WithTransform(CameraStateTransform transform)
		{
			CameraState newState = this;
			newState.position = (newState.position ?? Vector2.zero) + transform.Translation;
			
			newState.rotation = (newState.rotation ?? 0) + transform.RotationDelta;
			
			if(zoom == null && transform.ZoomDelta != 0){
				Debug.LogWarning("Could not apply zoom delta to camera state without zoom value");
			}
			newState = newState.ExponentialZoom(transform.ZoomDelta);
			
			

			return newState;
		}
		public static CameraState operator +(CameraState a, CameraStateTransform b)
		{
			return a.WithTransform(b);
		}
		public static CameraState operator +(CameraStateTransform a, CameraState b)
		{
			return b.WithTransform(a);
		}
		public static CameraState operator -(CameraState a, CameraStateTransform b)
		{
			return a.WithTransform(-b);
		}
		public static CameraState operator -(CameraStateTransform a, CameraState b)
		{
			return b.WithTransform(-a);
		}
		public static CameraStateTransform operator -(CameraState a, CameraState b)
		{
			CameraStateTransform transform = new CameraStateTransform(
				translation: a.position - b.position,
				rotationDelta: a.rotation - b.rotation,
				zoomDelta: a.zoom - b.zoom
			);
			return transform;
		}
		
		public void ApplyTo(Camera camera)
		{
			ApplyTo(camera, false);
		}
		/// <summary>
		/// Applies the CameraState to a given Camera.
		/// </summary>
		/// <param name="camera">The Camera to which the state will be applied.</param>
		/// <param name="local">Whether the position and rotation should be applied as local values. Defaults to false.</param>
		public void ApplyTo(Camera camera, bool local = false)
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
		/// <summary>
		/// Checks if the CameraState represents a zoom value.
		/// </summary>
		/// <returns>True if the CameraState represents a zoom value, otherwise false.</returns>
		public bool RepresentsZoom()
		{
			return zoom != null;
		}
		/// <summary>
		/// Checks if the CameraState represents a rotation value.
		/// </summary>
		/// <returns>True if the CameraState represents a rotation value, otherwise false.</returns>
		public bool RepresentsRotation()
		{
			return rotation != null;
		}
		/// <summary>
		/// Checks if the CameraState represents a position value.
		/// </summary>
		/// <returns>True if the CameraState represents a position value, otherwise false.</returns>
		public bool RepresentsPosition()
		{
			return position != null;
		}
		/// <summary>
		/// Checks if the CameraState is empty (i.e., no position, zoom, or rotation values).
		/// </summary>
		/// <returns>True if the CameraState is empty, otherwise false.</returns>
		public bool IsEmpty()
		{
			return position == null && zoom == null && rotation == null;
		}
		/// <summary>
		/// Decays the position of the CameraState to a target position over multiple calls.
		/// </summary>
		/// <param name="target">The target CameraState to decay towards.</param>
		/// <param name="factor">The decay factor.</param>
		/// <param name="deltaTime">The deltaTime between the calls. Defaults to Time.deltaTime if not specified.</param>
		/// <returns>A new CameraState with the decayed position.</returns>
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
		/// Decays the zoom of the CameraState to a target position over multiple calls.
		/// </summary>
		/// <param name="target">The target CameraState to decay towards.</param>
		/// <param name="factor">The decay factor.</param>
		/// <param name="deltaTime">The deltaTime between the calls. Defaults to Time.deltaTime if not specified.</param>
		/// <returns>A new CameraState with the decayed zoom.</returns>
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
		/// Decays the rotation of the CameraState to a target position over multiple calls.
		/// </summary>
		/// <param name="target">The target CameraState to decay towards.</param>
		/// <param name="factor">The decay factor.</param>
		/// <param name="deltaTime">The deltaTime between the calls. Defaults to Time.deltaTime if not specified.</param>
		/// <returns>A new CameraState with the decayed rotation.</returns>
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

		/// <summary>
		/// Zooms the camera state to a target zoom towards a target position 
		/// </summary>
		/// <param name="targetZoom">The target zoom value</param>
		/// <param name="target">The position in the space as the camera position to zoom towards</param>
		/// <returns>A clone of the state with modified position and zoom</returns>
		public CameraState ZoomTowards(float targetZoom, Vector2 target)
		{
			if (zoom == null){
				return Clone();
			}
			CameraState newState = Clone();
			
			if(position != null){
				Vector2 offset = target - position.Value;
				Vector2 newOffset = offset * (targetZoom / newState.zoom.Value);
				
				newState.position += offset - newOffset;
			}
			newState.zoom = targetZoom;
			return newState;
		}
		/// <summary>
		/// Zooms the camera state to a target zoom towards a target position 
		/// </summary>
		/// <param name="other">The state that contains the target zoom value</param>
		/// <param name="target">The position in the space as the camera position to zoom towards</param>
		/// <returns>A clone of the state with modified position and zoom</returns>
		public CameraState ZoomTowards(CameraState other, Vector2 target)
		{
			if (zoom == null){
				return Clone();
			}
			if(other.zoom == null){
				return Clone();
			}
			return ZoomTowards(other.zoom.Value, target);
		}
		/// <summary>
		/// Zooms the camera exponentially with the value provided towards a target position. This allows for linear zoom 'feel' when the zoom change is constant.
		/// </summary>
		/// <param name="linearZoomChange">A value that dictates the direction and magnitude of zoom</param>
		/// <param name="target">The position in the space as the camera position to zoom towards</param>
		/// <returns>A clone of the state with modified position and zoom</returns>
		public CameraState ExponentialZoom(float linearZoomChange, Vector2 target)
		{
			if (zoom == null){
				return Clone();
			}
			float newZoom = Mathf.Exp(Mathf.Log(zoom.Value) + linearZoomChange);
			return ZoomTowards(newZoom, target);
		}
		/// <summary>
		/// Zooms the camera exponentially with the value provided towards the center of the screen. This allows for linear zoom 'feel' when the zoom change is constant.
		/// </summary>
		/// <param name="linearZoomChange">A value that dictates the direction and magnitude of zoom</param>
		/// <returns>A clone of the state with modified zoom</returns>
		public CameraState ExponentialZoom(float linearZoomChange)
		{
			if (zoom == null){
				return Clone();
			}
			return WithZoom(Mathf.Exp(Mathf.Log(zoom.Value) + linearZoomChange));
		}
		/// <summary>
		/// Clamps the zoom value of the camera state between a minimum and maximum value
		/// </summary>
		/// <param name="min">The lower bound for the clamp</param>
		/// <param name="max">The upped bound for the clamp</param>
		/// <returns>A clone of the state with modified zoom</returns>
		public CameraState ClampedZoom(float min, float max)
		{
			if (zoom == null){
				return Clone();
			}
			return WithZoom(Mathf.Clamp(zoom.Value, min, max));
		}
		
		/// <summary>
		/// Clamps the zoom value of the camera state between a minimum and maximum value
		/// </summary>
		/// <param name="clamp">A vector2 with the minimum and maximum values for the clamp</param>
		/// <returns>A clone of the state with modified zoom</returns>
		public CameraState ClampedZoom(Vector2 clamp)
		{
			return ClampedZoom(clamp.x, clamp.y);
		}
		/// <summary>
		/// Clamps the position of the camera state between a minimum and maximum value on both axes
		/// </summary>
		/// <param name="clampX">A vector2 with the minimum and maximum values for the clamp on the X axis</param>
		/// <param name="clampY">A vector2 with the minimum and maximum values for the clamp on the y axis</param>
		/// <returns>A clone of the state with modified position</returns>
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
		/// <summary>
		/// Clamps the position of the camera state between a minimum and maximum value on the X axis
		/// </summary>
		/// <param name="clamp">A vector2 with the minimum and maximum values for the clamp on the X axis</param>
		/// <returns>A clone of the state with modified position</returns>
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
		/// <summary>
		/// Clamps the position of the camera state between a minimum and maximum value on the X axis
		/// </summary>
		/// <param name="clamp">A vector2 with the minimum and maximum values for the clamp on the Y axis</param>
		/// <returns>A clone of the state with modified position</returns>
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


		public void DrawGizmos(float aspect, Color? color = null)
		{
			Bounds bounds = GetBounds(aspect);
			Vector3[] cameraFrame ={
				bounds.min,
				new Vector3(bounds.min.x, bounds.max.y, 0),
				bounds.max,
				new Vector3(bounds.max.x, bounds.min.y, 0)
			};
			Gizmos.color = color ?? Color.yellow;
			DrawGizmosPath(cameraFrame);

		}
		void DrawGizmosPath(Vector3[] points)
		{
			for (int i = 0; i < points.Length; i++){
				Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
			}
		}
	}
}