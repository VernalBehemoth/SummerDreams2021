using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGCharacterAnims
{
	public class CameraController:MonoBehaviour
	{
		public GameObject cameraTarget;
		public float cameraTargetOffsetY;
		private Vector3 cameraTargetOffset;
		public float rotateSpeed;
		private float rotate;
		public float height = 6.0f;
		public float distance = 5.0f;
		public float zoomAmount = 0.1f;
		public float smoothing = 2.0f;
		private Vector3 offset;
		private bool following = true;
		private Vector3 lastPosition;

		private void Start()
		{
			offset = new Vector3(cameraTarget.transform.position.x, cameraTarget.transform.position.y + height, cameraTarget.transform.position.z - distance);
			lastPosition = new Vector3(cameraTarget.transform.position.x, cameraTarget.transform.position.y + height, cameraTarget.transform.position.z - distance);
			distance = 1;
			height = 1;
		}

		private void Update()
		{
			// Follow cam.
			if (Keyboard.current.fKey.isPressed) {
				if (following) { following = false; } else { following = true; }
			}
			if (following) { CameraFollow(); } else { transform.position = lastPosition; }

			// Rotate cam.
			if (Keyboard.current.qKey.isPressed) { rotate = -1; } else if (Keyboard.current.eKey.isPressed) { rotate = 1; } else { rotate = 0; }

			// Mouse zoom.
			if (Mouse.current.scroll.ReadValue().y > 0f) { distance += zoomAmount; height += zoomAmount; }
			else if (Mouse.current.scroll.ReadValue().y < 0f) { distance -= zoomAmount; height -= zoomAmount; }

			// Set cameraTargetOffset as cameraTarget + cameraTargetOffsetY.
			cameraTargetOffset = cameraTarget.transform.position + new Vector3(0, cameraTargetOffsetY, 0);

			// Smoothly look at cameraTargetOffset.
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraTargetOffset - transform.position), Time.deltaTime * smoothing);
		}

		private void CameraFollow()
		{
			offset = Quaternion.AngleAxis(rotate * rotateSpeed, Vector3.up) * offset;
			transform.position = new Vector3(Mathf.Lerp(lastPosition.x, cameraTarget.transform.position.x + offset.x, smoothing * Time.deltaTime),
				Mathf.Lerp(lastPosition.y, cameraTarget.transform.position.y + offset.y * height, smoothing * Time.deltaTime),
				Mathf.Lerp(lastPosition.z, cameraTarget.transform.position.z + offset.z * distance, smoothing * Time.deltaTime));
		}

		private void LateUpdate()
		{
			lastPosition = transform.position;
		}
	}
}