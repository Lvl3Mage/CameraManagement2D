using System;
using MyBox;
using UnityEngine;

namespace CameraManagement2D
{
    /// <summary>
    /// A module for smoothing camera movement, including position, zoom, and rotation.
    /// This class extends the <see cref="CameraController"/> and provides options to interpolate
    /// the camera's state based on the specified speeds.
    /// </summary>
	public class CameraSmoothingModule : CameraController
	{
		
		/// <summary>
		/// Determines whether to interpolate the camera's position.
		/// </summary>
		[SerializeField] bool interpolatePosition = false;
		
		/// <summary>
		/// The speed at which the camera's position interpolates. Only used if <see cref="interpolatePosition"/> is true.
		/// </summary>
		[ConditionalField("interpolatePosition")][SerializeField] float panSpeed = 20f;
		
		/// <summary>
		/// Determines whether to interpolate the camera's zoom.
		/// </summary>
		[SerializeField] bool interpolateZoom = false;

		/// <summary>
		/// The speed at which the camera's zoom interpolates. Only used if <see cref="interpolateZoom"/> is true.
		/// </summary>
		[ConditionalField("interpolateZoom")][SerializeField] float zoomSpeed = 20f;

        /// <summary>
        /// Determines whether to interpolate the camera's rotation.
        /// </summary>
		[SerializeField] bool interpolateRotation = false;

        /// <summary>
        /// The speed at which the camera's rotation interpolates. Only used if <see cref="interpolateRotation"/> is true.
        /// </summary>
		[ConditionalField("interpolateRotation")][SerializeField] float rotationSpeed = 20f;
        
		/// <summary>
		/// The target camera controller towards which the state will be interpolated.
		/// </summary>
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