using UnityEngine;

namespace Lvl3Mage.CameraManagement2D
{
	/// <summary>
	/// A class that provides easy access to the scene camera, and allows for easy activation/deactivation of the camera
	/// </summary>
	[RequireComponent(typeof(Camera))]
	public class SceneCamera : MonoBehaviour
	{
		static SceneCamera instance;
		[Tooltip("Activate the camera on start")]
		[SerializeField] bool startActive = true;
		[Tooltip("Components that should be enabled/disabled with the camera.")]
		[SerializeField] Behaviour[] attachedBehaviours; 
		Camera camera;
		void Awake()
		{
			camera = GetComponent<Camera>();


			if (!startActive) {
				camera.enabled = false;
				return;
			}
			if(instance != null){
				Debug.LogError("An instance of WorldCamera already exists! Ensure only one camera has the 'startActive' flag set.", this);
			}
			instance = this;
		}
		/// <summary>
		/// Activate this camera, deactivating any other active Scene cameras
		/// </summary>
		public void Activate(){
			if(instance != null){
				instance.ToggleActive(false);
			}
			ToggleActive(true);
		}

		void ToggleActive(bool active){
			camera.enabled = active;
			foreach (var component in attachedBehaviours){
				component.enabled = active;
			}
			instance = active ? this : null;
		}
		/// <summary>
		/// Get the world position of the mouse in the active camera's view
		/// </summary>
		/// <returns>A Vector2 representing the cursor's world position</returns>
		public static Vector2 GetWorldMousePosition(){
			return instance.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
		}
		/// <summary>
		/// Get the world position of the mouse in the active camera's view at a specified depth
		/// </summary>
		/// <param name="depth">
		/// The depth at which to get the mouse position
		/// </param>
		/// <returns>
		/// A Vector3 representing the cursor's world position at the specified depth
		/// </returns>
        public static Vector3 GetWorldMousePosition(float depth){
			return instance.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth));
		}
		/// <summary>
		/// Get the camera attached to the active SceneCamera
		/// </summary>
		/// <returns>The active Camera component</returns>
		public static Camera GetCamera(){
			return instance.camera;
		}
		/// <summary>
		/// Check if a point in worldspace is within the active camera's view
		/// </summary>
		/// <param name="point">
		/// The point to check
		/// </param>
		/// <returns>
		/// true if the point is within the camera's view, false otherwise
		/// </returns>
		public static bool PointInView(Vector2 point){
			Vector3 screenPoint = instance.camera.WorldToViewportPoint(point);
			return screenPoint.x is > 0 and < 1 && screenPoint.y is > 0 and < 1;
		}

	}
}