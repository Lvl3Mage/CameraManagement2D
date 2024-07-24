using System;
using UnityEngine;

namespace CameraManagement2D
{
	[RequireComponent(typeof(Camera))]
	public abstract class CameraController : MonoBehaviour
	{
		protected bool useUserInput = true;
		protected Camera controllerCamera;
		[SerializeField] protected bool active = true;
		bool initialized = false;

		protected void Awake()
		{
			controllerCamera = gameObject.GetComponent<Camera>();
		}

		protected void Start()
		{
			if(!initialized){
				InitializeCameraController();
				initialized = true;
			}
		}

		public void UseUserInput(bool value)
		{
			useUserInput = value;
			OnUserInputChange(value);
		}

		protected virtual void OnUserInputChange(bool value){ } //Use to propagate user input changes to children controllers
		public void SetActive(bool value)
		{
			active = !value;
		}
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
		/// Called before the first ComputeCameraState call
		/// </summary>
		protected virtual void InitializeCameraController(){}
		
		/// <summary>
		/// Must not have any side effects. Should calculate the camera state based on the available data
		/// </summary>
		/// <returns>A camera state calculated from the available data</returns>
		protected abstract CameraState ComputeCameraState();
	}

	
}

