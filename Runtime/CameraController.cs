using UnityEngine;

namespace CameraManagement2D
{
	[RequireComponent(typeof(Camera))]
	public abstract class CameraController : MonoBehaviour
	{
		protected bool useUserInput = true;
		protected Camera controllerCamera;
		[SerializeField] bool active = true;

		protected void Awake()
		{
			controllerCamera = gameObject.GetComponent<Camera>();
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

			return CalculateCameraState();
		}
		void LateUpdate()
		{
			if(!active){return;}
			CalculateCameraState().ApplyTo(controllerCamera);
		}

		protected abstract CameraState CalculateCameraState();
	}

	
}

