using System;
using CameraManagement2D;
using UnityEngine;
using UnityEngine.Serialization;

namespace CameraManagement2D
{
public class CameraPan : CameraController
{
	[SerializeField] Vector2 sizeClamp = new(0.1f,10);
	[SerializeField] float zoomFactor = 0.5f;
	[SerializeField] float panFactor = 1;
	[SerializeField] Vector2 clampX = new Vector2(-10,10);
	[SerializeField] Vector2 clampY = new Vector2(-10,10);
	[SerializeField] bool useLocalPosition = false;

	Vector2 cursorDragStart;
	CameraState targetState;

	void Start()
	{
		targetState = CameraState.FromCamera(controllerCamera, useLocalPosition).WithoutRotation();
	}

	protected override CameraState CalculateCameraState()
	{
		if (useUserInput){
			targetState = targetState.ExponentialZoom(-Input.mouseScrollDelta.y * zoomFactor).ClampedZoom(sizeClamp);
		}
		Vector2 cursorPosition = controllerCamera.ScreenToWorldPoint(Input.mousePosition);
		if(Input.GetMouseButtonDown(1)){
			if (useUserInput){
				cursorDragStart = cursorPosition;
			}
		}
		else if(Input.GetMouseButton(1)){
			if (useUserInput){
				Vector2 cameraCursorDelta = cursorPosition - (Vector2)controllerCamera.transform.position;
				Vector2 targetPosition = cursorDragStart - cameraCursorDelta;
				
				targetState = targetState.WithPosition(targetPosition).ClampedPosition(clampX,clampY);
			}
		}
		return targetState;
	}
	void OnDrawGizmos(){
		Gizmos.color = Color.red;
		Vector3 reference = Vector3.zero;
		if (transform.parent){
			reference = transform.parent.position;
		}

		Vector3[] corners = new Vector3[]{
			reference + new Vector3(clampX.x, clampY.x, 0),
			reference + new Vector3(clampX.y, clampY.x, 0),
			reference + new Vector3(clampX.y, clampY.y, 0),
			reference + new Vector3(clampX.x, clampY.y, 0)
		};
		Gizmos.DrawLine(corners[0],corners[1]);
		Gizmos.DrawLine(corners[1],corners[2]);
		Gizmos.DrawLine(corners[2],corners[3]);
		Gizmos.DrawLine(corners[3],corners[0]);
       }
}
	
}
