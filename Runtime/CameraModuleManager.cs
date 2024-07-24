using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraManagement2D
{
	public class CameraModuleManager : CameraController
	{
		[SerializeField] CameraController[] cameraControllers;
		int activeIndex = 0;
		protected override void OnUserInputChange(bool value)
		{
			foreach (var controller in cameraControllers){
				controller.UseUserInput(value);
			}
		}
		public void SwitchToController(int index)
		{
			activeIndex = index;
		}
		protected override CameraState ComputeCameraState()
		{
			return cameraControllers[activeIndex].GetCameraState();
		}
	}
}