#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace CameraManagement2D
{
	public class TrackerCameraModule : CameraController
	{
		[SerializeField] List<TrackedObject> trackedObjects = new();
		[SerializeField] float cameraPadding = 1f;
		[SerializeField] bool predictMovement;
		[ConditionalField("predictMovement")][SerializeField] bool enforceCurrentBounds = true;
		[ConditionalField("predictMovement")][SerializeField] float predictionTime = 0.1f;
		[SerializeField] CameraStateClamp clamp;
		protected override void InitializeCameraController()
		{
			trackedObjects.ForEach((el)=>el.Initialize());
		}
		public void SetTrackedObjects(GameObject[] objects, TrackedObject.BoundsType boundsType = TrackedObject.BoundsType.None)
		{
			trackedObjects = new List<TrackedObject>();
			trackedObjects.Capacity = objects.Length;
			foreach (var obj in objects){
				trackedObjects.Add(new TrackedObject(obj, boundsType));
			}
		}
		public void AddTrackedObject(GameObject obj, TrackedObject.BoundsType boundsType = TrackedObject.BoundsType.None)
		{
			if(trackedObjects.Any((el)=>el.Tracks(obj)))
			{
				Debug.LogWarning($"Object {obj.gameObject.name} already tracked", obj);
				return; 
			}
			trackedObjects.Add(new TrackedObject(obj, boundsType));
		}
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

	[System.Serializable]
	public class TrackedObject
	{
		public enum BoundsType
		{
			None,
			Renderer,
			Collider,
			All,
		}
		[SerializeField] GameObject gameObject;
		[SerializeField] BoundsType boundsType;
		[SerializeField]float boundsPadding = 0f;
		Transform transform;
		Vector2 pastPosition;
		float updateDeltaTime;
		Renderer renderer;
		Collider2D collider;

		public TrackedObject(GameObject gameObject, BoundsType boundsType = BoundsType.None, float boundsPadding = 0f)
		{
			this.gameObject = gameObject;
			this.boundsType = boundsType;
			this.boundsPadding = boundsPadding;
			Initialize();
		}

		public void Initialize()
		{
			
			if (!gameObject){
				Debug.LogError("GameObject is null on deserialization!", gameObject);
			}
			transform = gameObject.transform;
			pastPosition = transform.position;
			
			renderer = gameObject.GetComponent<Renderer>();
			if (!renderer && boundsType is BoundsType.Renderer){
				Debug.LogWarning("Renderer not found on object", gameObject);
				
			}
			collider = gameObject.GetComponent<Collider2D>();
			
			if (!collider && boundsType is BoundsType.Collider){
				Debug.LogWarning("Collider2D not found on object", gameObject);
				
			}
		}
		public bool Tracks(GameObject obj)
		{
			return gameObject == obj;
		}
		public void Update(float deltaTime)
		{
			pastPosition = transform.position;
			updateDeltaTime = deltaTime;
		}
		public Bounds GetBounds()
		{
			Bounds bounds = new Bounds(transform.position, Vector3.zero);
			if(renderer && boundsType is BoundsType.Renderer or BoundsType.All)
			{
				bounds.Encapsulate(renderer.bounds);
			}
			if(collider && boundsType is BoundsType.Collider or BoundsType.All)
			{
				bounds.Encapsulate(collider.bounds);
			}
			bounds.Expand(boundsPadding);
			return bounds;
		}

		public Bounds GetPredictedBounds(float predictionTime)
		{
			Bounds bounds = GetBounds();
			Vector2 velocity = ((Vector2)transform.position - pastPosition) / updateDeltaTime;
			bounds.center += (Vector3)velocity * predictionTime;
			return bounds;
		}
	}
}