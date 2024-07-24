using System;
using CameraManagement2D;
using UnityEngine;
using UnityEngine.Serialization;

namespace CameraManagement2D
{
public class CameraPanModule : CameraController
{
	[SerializeField] float zoomFactor = 0.5f;
	[SerializeField] float panFactor = 1;
	[SerializeField] bool useLocalPosition = false;
	[SerializeField] CameraStateClamp clamp;
	[SerializeField] bool drawGizmos = true;
	Vector2 cursorDragStart;
	CameraState targetState;

	protected override void InitializeCameraController()
	{
		targetState = CameraState.FromCamera(controllerCamera, useLocalPosition).WithoutRotation();
	}
	
	protected override CameraState ComputeCameraState()
	{
		return targetState;
	}
	//Todo implement cursor relative zoom

	void Update()
	{
		Vector2 cursorPosition = controllerCamera.ScreenToWorldPoint(Input.mousePosition);
		if (useUserInput){
			
			CameraState newState = targetState.ExponentialZoom(-Input.mouseScrollDelta.y * zoomFactor,cursorPosition);
			newState = clamp.ClampState(newState, controllerCamera.aspect);
			
			targetState = targetState.ZoomTowards(newState, cursorPosition);

		}
		if(Input.GetMouseButtonDown(1)){
			if (useUserInput){
				cursorDragStart = cursorPosition;
			}
		}
		else if(Input.GetMouseButton(1)){
			if (useUserInput){
				Vector2 cameraCursorDelta = cursorPosition - (Vector2)controllerCamera.transform.position;
				Vector2 targetPosition = cursorDragStart - cameraCursorDelta;
				
				targetState = targetState.WithPosition(targetPosition); //.ClampedPosition(clampX,clampY);
			}
		}
		targetState = clamp.ClampState(targetState, controllerCamera.aspect);
	}

#if UNITY_EDITOR
	Camera gizmoCamera;
	void OnDrawGizmos(){
		if(!drawGizmos) return;
		if (!gizmoCamera){
			gizmoCamera = !controllerCamera ? GetComponent<Camera>(): controllerCamera;
			if(!gizmoCamera) return;
		}
		
		clamp.DrawGizmos(gizmoCamera);
	}
	
#endif
}
	
}
