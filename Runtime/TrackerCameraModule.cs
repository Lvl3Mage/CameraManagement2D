#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace CameraManagement2D
{
	/// <summary>
	/// A camera module that tracks multiple objects and adjusts the camera's state to ensure all tracked objects
	/// are visible within the camera's view. Provides options for predicting object movement and adjusting the camera's
	/// bounds to include padding around tracked objects.
	/// </summary>
	public class TrackerCameraModule : CameraController
	{
		
		/// <summary>
		/// The list of objects currently being tracked by the camera.
		/// </summary>
		[SerializeField] List<TrackedObject> trackedObjects = new();
		
		/// <summary>
		/// The amount of padding around the tracked objects' bounds.
		/// </summary>
		[SerializeField] float cameraPadding = 1f;
		
		/// <summary>
		/// Indicates whether to predict the movement of tracked objects.
		/// </summary>
		[SerializeField] bool predictMovement;
		

		/// <summary>
		/// Indicates whether to enforce the current bounds when predicting the movement of tracked objects.
		/// Only used if <see cref="predictMovement"/> is true.
		/// </summary>
		[ConditionalField("predictMovement")][SerializeField] bool enforceCurrentBounds = true;
		
		/// <summary>
		/// The time into the future to predict the position of tracked objects.
		/// Only used if <see cref="predictMovement"/> is true.
		/// </summary>
		[ConditionalField("predictMovement")][SerializeField] float predictionTime = 0.1f;
		/// <summary>
		/// The component used to clamp the camera's state to certain bounds.
		/// </summary>
		[SerializeField] CameraStateClamp clamp;
		protected override void InitializeCameraController()
		{
			trackedObjects.ForEach((el)=>el.Initialize());
		}
		/// <summary>
        /// Sets the objects to be tracked by the camera.
        /// </summary>
        /// <param name="objects">The gameobjects to track.</param>
        /// <param name="boundsType">The type of bounds to use for tracking.</param>
		public void SetTrackedObjects(GameObject[] objects, TrackedObject.BoundsType boundsType = TrackedObject.BoundsType.None)
		{
			trackedObjects = new List<TrackedObject>();
			trackedObjects.Capacity = objects.Length;
			foreach (var obj in objects){
				trackedObjects.Add(new TrackedObject(obj, boundsType));
			}
		}
		/// <summary>
		/// Adds a new object to be tracked by the camera.
		/// </summary>
		/// <param name="obj">The gameobject to track.</param>
		/// <param name="boundsType">The type of bounds to use for tracking.</param>
		public void AddTrackedObject(GameObject obj, TrackedObject.BoundsType boundsType = TrackedObject.BoundsType.None)
		{
			if(trackedObjects.Any((el)=>el.Tracks(obj)))
			{
				Debug.LogWarning($"Object {obj.gameObject.name} already tracked", obj);
				return; 
			}
			trackedObjects.Add(new TrackedObject(obj, boundsType));
		}
		/// <summary>
		/// Removes an object from being tracked by the camera.
		/// </summary>
		/// <param name="obj">The gameobject to stop tracking.</param>
		public void RemoveTrackedObject(GameObject obj)
		{
			int objectIndex = trackedObjects.FindIndex((el)=>el.Tracks(obj));
			if (objectIndex < 0){
				Debug.LogWarning($"Object {obj.gameObject.name} not tracked", obj);
				return;
			}
			trackedObjects.RemoveAt(objectIndex);
		}
		protected override CameraState ComputeCameraState()
		{
			if(trackedObjects.Count == 0){
				Debug.LogWarning("No objects to track", this);
				return CameraState.Empty();
			}

			Bounds bounds = GetBoundsFor(trackedObjects[0]);
			for (int i = 1; i < trackedObjects.Count; i++){
				bounds.Encapsulate(GetBoundsFor(trackedObjects[i]));
			}
			
			bounds.Expand(cameraPadding);
			
			
			Vector2 center = bounds.center;
			float zoomX = bounds.size.x / controllerCamera.aspect;
			float zoomY = bounds.size.y;
			float zoom = Mathf.Max(zoomX, zoomY)*0.5f;
			CameraState state = CameraState.Empty().WithPosition(center).WithZoom(zoom);
			state = clamp.ClampState(state, controllerCamera.aspect);
			return state;
		}
		Bounds GetBoundsFor(TrackedObject obj)
		{
			if(predictMovement){
				Bounds bounds = obj.GetPredictedBounds(predictionTime);
				if (enforceCurrentBounds){
					bounds.Encapsulate(obj.GetBounds());
				}

				return bounds;
			}
			
			return obj.GetBounds();
		}

		void FixedUpdate()
		{
			trackedObjects.ForEach((el)=>el.Update(Time.fixedDeltaTime));
		}
	}
	/// <summary>
	/// Enumeration for the types of bounds that can be used for tracking.
	/// </summary>
	public enum BoundsSource
	{
		/// <summary>
		/// No bounds are used for tracking. Only the object's transform is considered.
		/// </summary>
		None,
		/// <summary>
		/// The object's renderer bounds are used for tracking.
		/// </summary>
		Renderer,
		/// <summary>
		/// The object's collider bounds are used for tracking.
		/// </summary>
		Collider,
		/// <summary>
		/// Both the renderer and collider bounds are used for tracking.
		/// </summary>
		All,
	}
	/// <summary>
	/// Represents an object being tracked by the camera, including its bounds and prediction capabilities.
	/// </summary>
	[System.Serializable]
	public class TrackedObject
	{
		
		/// <summary>
		/// The game object being tracked.
		/// </summary>
		[SerializeField] GameObject gameObject;
		/// <summary>
		/// The type of bounds to use for tracking the object.
		/// </summary>
		[SerializeField] BoundsSource boundsSource;
		/// <summary>
		/// Padding to add around the object's bounds.
		/// </summary>
		[SerializeField]float boundsPadding = 0f;
		Transform transform;
		Vector2 pastPosition;
		float updateDeltaTime;
		Renderer renderer;
		Collider2D collider;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="TrackedObject"/> class.
		/// </summary>
		/// <param name="gameObject">The game object to track.</param>
		/// <param name="boundsSource">The type of bounds to use for tracking.</param>
		/// <param name="boundsPadding">The padding to add around the object's bounds.</param>
		public TrackedObject(GameObject gameObject, BoundsSource boundsSource = BoundsSource.None, float boundsPadding = 0f)
		{
			this.gameObject = gameObject;
			this.boundsSource = boundsSource;
			this.boundsPadding = boundsPadding;
			Initialize();
		}
		/// <summary>
		/// Initializes the tracked object, including setting up the transform, renderer, and collider.
		/// </summary>
		public void Initialize()
		{
			
			if (!gameObject){
				Debug.LogError("GameObject is null on deserialization!", gameObject);
			}
			transform = gameObject.transform;
			pastPosition = transform.position;
			
			renderer = gameObject.GetComponent<Renderer>();
			if (!renderer && boundsSource is BoundsSource.Renderer){
				Debug.LogWarning("Renderer not found on object", gameObject);
				
			}
			collider = gameObject.GetComponent<Collider2D>();
			
			if (!collider && boundsSource is BoundsSource.Collider){
				Debug.LogWarning("Collider2D not found on object", gameObject);
				
			}
		}
		/// <summary>
		/// Checks if this tracked object is the same as the specified object.
		/// </summary>
		/// <param name="obj">The object to check.</param>
		/// <returns>True if this object is tracking the specified object; otherwise, false.</returns>
		public bool Tracks(GameObject obj)
		{
			return gameObject == obj;
		}
		/// <summary>
		/// Updates the tracked object's past position and delta time.
		/// </summary>
		/// <param name="deltaTime">The time elapsed since the last update.</param>
		public void Update(float deltaTime)
		{
			pastPosition = transform.position;
			updateDeltaTime = deltaTime;
		}
		/// <summary>
		/// Gets the bounds of the tracked object, including any specified padding.
		/// </summary>
		/// <returns>The bounds of the tracked object.</returns>
		public Bounds GetBounds()
		{
			Bounds bounds = new Bounds(transform.position, Vector3.zero);
			if(renderer && boundsSource is BoundsSource.Renderer or BoundsSource.All)
			{
				bounds.Encapsulate(renderer.bounds);
			}
			if(collider && boundsSource is BoundsSource.Collider or BoundsSource.All)
			{
				bounds.Encapsulate(collider.bounds);
			}
			bounds.Expand(boundsPadding);
			return bounds;
		}
		/// <summary>
		/// Gets the predicted bounds of the tracked object based on its velocity and the specified prediction time.
		/// </summary>
		/// <param name="predictionTime">The time into the future to predict the object's position.</param>
		/// <returns>The predicted bounds of the tracked object.</returns>
		public Bounds GetPredictedBounds(float predictionTime)
		{
			Bounds bounds = GetBounds();
			Vector2 velocity = ((Vector2)transform.position - pastPosition) / updateDeltaTime;
			bounds.center += (Vector3)velocity * predictionTime;
			return bounds;
		}
	}
}