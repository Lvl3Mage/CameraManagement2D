using System;
using Lvl3Mage.CameraManagement2D;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lvl3Mage.CameraManagement2D
{
	/// <summary>
	/// A camera controller module that allows for panning and zooming of the camera based on user input.
	/// </summary>
	public class CameraPanModule : CameraController
	{
		/// <summary>
		/// The factor by which the zoom changes in response to user input.
		/// </summary>
		[SerializeField] float zoomFactor = 0.5f;
		/// <summary>
		/// The factor by which the camera pans in response to user input.
		/// </summary>
		[SerializeField] float panFactor = 1;
		/// <summary>
		/// Indicates whether the local position is used for panning and zooming.
		/// </summary>
		[SerializeField] bool useLocalPosition = false;
		/// <summary>
		/// The clamp settings to apply to the camera state.
		/// </summary>
		[SerializeField] CameraStateClamp clamp;
		/// <summary>
		/// Indicates whether to draw gizmos in the editor for visualizing clamping bounds.
		/// </summary>
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
				CameraState newState =
					targetState.ExponentialZoom(-Input.mouseScrollDelta.y * zoomFactor, cursorPosition);
				newState = clamp.ClampState(newState, controllerCamera.aspect);

				targetState = targetState.ZoomTowards(newState, cursorPosition);
			}

			if (Input.GetMouseButtonDown(1)){
				if (useUserInput){
					cursorDragStart = cursorPosition;
				}
			}
			else if (Input.GetMouseButton(1)){
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
		void OnDrawGizmos()
		{
			if (!drawGizmos) return;
			if (!gizmoCamera){
				gizmoCamera = !controllerCamera ? GetComponent<Camera>() : controllerCamera;
				if (!gizmoCamera) return;
			}

			clamp.DrawGizmos();
			clamp.ClampState(CameraState.FromCamera(controllerCamera, useLocalPosition).WithoutRotation(), gizmoCamera.aspect).DrawGizmos(gizmoCamera.aspect);
		}

#endif
	}
}