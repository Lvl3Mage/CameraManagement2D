using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraManagement2D
{
	/// <summary>
	/// Manages multiple camera controllers, allowing switching between them.
	/// </summary>
	public class CameraModuleManager : CameraController
	{
		/// <summary>
		/// The array of camera controllers managed by this module.
		/// </summary>
		[SerializeField] CameraController[] cameraControllers;
		int activeIndex = 0;
		/// <summary>
		/// Propagates user input changes to all managed camera controllers.
		/// </summary>
		/// <param name="value">Whether user input is enabled.</param>
		protected override void OnUserInputChange(bool value)
		{
			foreach (var controller in cameraControllers){
				controller.UseUserInput(value);
			}
		}
		/// <summary>
		/// Switches the active camera controller to the one at the specified index.
		/// </summary>
		/// <param name="index">The index of the camera controller to switch to.</param>
		public void SwitchToController(int index)
		{
			activeIndex = index;
		}
		/// <summary>
		/// Computes the current camera state based on the active camera controller.
		/// </summary>
		/// <returns>The computed CameraState.</returns>
		protected override CameraState ComputeCameraState()
		{
			return cameraControllers[activeIndex].GetCameraState();
		}
	}
}