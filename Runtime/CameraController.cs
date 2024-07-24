using System;
using UnityEngine;

namespace CameraManagement2D
{
	/// <summary>
	/// An abstract base class for camera controllers.
	/// </summary>
	[RequireComponent(typeof(Camera))]
	public abstract class CameraController : MonoBehaviour
	{
		/// <summary>
		/// Indicates whether user input is used to control the camera.
		/// </summary>
		protected bool useUserInput = true;
		/// <summary>
		/// The camera that this controller is managing.
		/// </summary>
		protected Camera controllerCamera;
		/// <summary>
		/// Indicates whether the camera controller is active.
		/// </summary>
		[SerializeField] protected bool active = true;
		bool initialized;

		/// <remarks>When overriding call the base method</remarks>
		protected virtual void Awake()
		{
			controllerCamera = gameObject.GetComponent<Camera>();
		}
		/// <remarks>Do not redefine. Use <see cref="InitializeCameraController"/> to initialize the component instead</remarks>
		void Start()
		{
			if(!initialized){
				InitializeCameraController();
				initialized = true;
			}
		}
		/// <summary>
		/// Enables or disables user input for the camera controller.
		/// </summary>
		/// <param name="value">True to enable user input, false to disable.</param>
		public void UseUserInput(bool value)
		{
			useUserInput = value;
			OnUserInputChange(value);
		}
		/// <summary>
		/// Called when the user input state changes. Override to propagate user input changes to derived classes.
		/// </summary>
		/// <param name="value">The new user input state.</param>
		protected virtual void OnUserInputChange(bool value){ }
		/// <summary>
		/// Sets the active state of the camera controller.
		/// </summary>
		/// <param name="value">True to activate the controller, false to deactivate.</param>
		public void SetActive(bool value)
		{
			active = !value;
		}
		/// <summary>
		/// Gets the current state of the camera.
		/// </summary>
		/// <returns>The current CameraState.</returns>
		public CameraState GetCameraState()
		{
			if (active){
				Debug.LogWarning("Camera Controller is still active when getting state. This may cause unexpected behaviour.");
			}
			if(!initialized){
				InitializeCameraController();
				initialized = true;
			}
			return ComputeCameraState();
		}
		void LateUpdate()
		{
			if(!active){return;}
			if(!initialized){
				InitializeCameraController();
				initialized = true;
			}
			ComputeCameraState().ApplyTo(controllerCamera);
		}
		/// <summary>
		/// Called before the first ComputeCameraState or Update call. Override to initialize the camera controller.
		/// </summary>
		protected virtual void InitializeCameraController(){}
		
		/// <summary>
		/// Computes the camera state based on the available data. Must not have any side effects.
		/// </summary>
		/// <returns>A CameraState calculated from the available data.</returns>
		protected abstract CameraState ComputeCameraState();
	}

	
}

