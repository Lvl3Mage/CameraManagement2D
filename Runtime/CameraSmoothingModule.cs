using System;
using MyBox;
using UnityEngine;

namespace CameraManagement2D
{
	public class CameraSmoothingModule : CameraController
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
		CameraState interpolatedState;
		protected override void InitializeCameraController()
		{
			interpolatedState = targetController.GetCameraState();
		}
		protected override CameraState ComputeCameraState()
		{
			CameraState state = targetController.GetCameraState();
			if(interpolatePosition){
				state = state.WithPosition(interpolatedState);
			}
			if(interpolateZoom){
				state = state.WithZoom(interpolatedState);
			}
			if (interpolateRotation){
				state = state.WithRotation(interpolatedState);
			}
			return state;
		}

		void FixedUpdate()
		{
			if(!active){return;}
			interpolatedState = Decay(interpolatedState, targetController.GetCameraState(), Time.fixedDeltaTime);
		}

		CameraState Decay(CameraState from, CameraState to, float deltaTime)
		{
			from = from.DecayPositionTo(to, panSpeed, deltaTime);
			from = from.DecayZoomTo(to, zoomSpeed, deltaTime);
			from = from.DecayRotationTo(to, rotationSpeed, deltaTime);
			return from;
		}
	}
}