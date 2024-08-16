using System;
using System.Collections;
using System.Collections.Generic;
using Lvl3Mage.EditorEnhancements.Runtime;
using UnityEngine;

namespace Lvl3Mage.CameraManagement2D
{
	/// <summary>
	/// Manages multiple camera controllers, allowing switching between them.
	/// </summary>
	public class CameraModuleManager : CameraController
	{
		/// <summary>
		/// The array of camera controllers managed by this module.
		/// </summary>
		
		[Space(32)]
		[MethodSourceLabeledField(nameof(GetControllerFunctionality), sourceType: SourceType.Parent, hideProperty:true)]
		[SerializeField] string displayName;
		[MethodSourceLabeledField(nameof(GetControllerFunctionality), sourceType: SourceType.Field)]
		[SerializeField] CameraController[] cameraControllers = Array.Empty<CameraController>();
		public override string GetControllerFunctionality()
		{
			if (cameraControllers.Length == 0){
				return base.GetControllerFunctionality();
			}
			if(activeIndex >= cameraControllers.Length || activeIndex < 0){
				return base.GetControllerFunctionality();
			}
			CameraController targetController = cameraControllers[activeIndex];
			if(targetController == null){
				return base.GetControllerFunctionality();
			}
			if(targetController == this){
				return "ERROR: Recursive Reference";
			}
			return $"{base.GetControllerFunctionality()} => {targetController.GetControllerFunctionality()}";
		}
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