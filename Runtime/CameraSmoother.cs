using MyBox;
using UnityEngine;

namespace CameraManagement2D
{
	public class CameraSmoother : CameraController
	{
		[SerializeField] bool interpolatePosition = false;
		[ConditionalField("interpolatePosition")][SerializeField] float panSpeed = 20f;
		[SerializeField] bool interpolateZoom = false;
		[ConditionalField("interpolateZoom")][SerializeField] float zoomSpeed = 20f;
		[SerializeField] bool interpolateRotation = false;
		[ConditionalField("interpolateRotation")][SerializeField] float rotationSpeed = 20f;
		[SerializeField] CameraController targetController;
		protected override void OnUserInputChange(bool value)
		{
			targetController.UseUserInput(value);
		}

		protected override CameraState CalculateCameraState()
		{
			CameraState targetState = targetController.GetCameraState();
			CameraState currentState = CameraState.FromCamera(controllerCamera);

			if (interpolatePosition){
				currentState = currentState.DecayPositionTo(targetState, panSpeed);
			}
			else{
				currentState = currentState.WithPosition(targetState);
			}

			if (interpolateRotation){
				currentState = currentState.DecayRotationTo(targetState, rotationSpeed);
			}
			else{
				currentState = currentState.WithRotation(targetState);
			}

			if (interpolateZoom){
				currentState = currentState.DecayZoomTo(targetState, zoomSpeed);
			}
			else{
				currentState = currentState.WithZoom(targetState);
			}
			return currentState;
		}
	}
}