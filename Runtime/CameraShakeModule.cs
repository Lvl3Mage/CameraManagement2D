using System;
using System.Collections;
using System.Collections.Generic;
using Lvl3Mage.EditorEnhancements.Runtime;
using Lvl3Mage.InterpolationToolkit;
using Lvl3Mage.InterpolationToolkit.Splines;
using UnityEditor;
using UnityEngine;

namespace Lvl3Mage.CameraManagement2D
{
	
	public class CameraShakeModule : CameraController
	{

		public CameraStateTransform.TransformSpline CreateContinuousShake(I4PointSplineFactory splineFactory, CameraStateTransform tangent1, CameraStateTransform tangent2 = default)
		{
			return CameraStateTransform.CreateTransformSpline(shakeTransform, tangent1, tangent2, CameraStateTransform.Empty, splineFactory);
		}
		
		CameraStateTransform shakeTransform;
		Coroutine shakeCoroutine;
		public void StartShake(CameraStateTransform.TransformSpline shake, float duration)
		{
			if (shakeCoroutine != null)
			{
				StopCoroutine(shakeCoroutine);
			}
			shakeCoroutine = StartCoroutine(ShakeRoutine(shake, duration));
		}

		IEnumerator ShakeRoutine(CameraStateTransform.TransformSpline shake, float duration)
		{
			float elapsed = 0;
			while (elapsed < duration)
			{
				float t = elapsed / duration;
				shakeTransform = shake(t);
				elapsed += Time.deltaTime;
				yield return null;
			}
			shakeTransform = CameraStateTransform.Empty;
		}
		[Space(32)]
		[MethodSourceLabeledField(nameof(GetControllerFunctionality))]
		[SerializeField] CameraController targetController;
		public override string GetControllerFunctionality()
		{
			if(targetController == null){
				return base.GetControllerFunctionality();
			}
			if(targetController == this){
				return "ERROR: Recursive Reference";
			}

			return $"{base.GetControllerFunctionality()} => {targetController.GetControllerFunctionality()}";
		}
		protected override CameraState ComputeCameraState()
		{
			CameraState state = targetController.GetCameraState();
			state += shakeTransform;
			return state;
		}
	}
}