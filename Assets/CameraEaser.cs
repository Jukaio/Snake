using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEaser : MonoBehaviour
{
	[SerializeField] private Transform focusedTransform = null;
	[SerializeField, Range(0.0f, 50.0f)] private float cameraDistance = 5.0f;

	private delegate Vector3 Vector3Ease(Vector3 a, Vector3 b, float t);
	private delegate Quaternion QuaternionEase(Quaternion a, Quaternion b, float t);

	private Coroutine cameraRoutine = null;
	private Vector3Ease positionEasingFunction = Vector3.Lerp;
	private QuaternionEase rotationEasingFunction = Quaternion.Lerp;

	public void CalculateStatePointsAndStartEaseRoutine(Vector3 forward, Vector3 up)
	{
		/* Transform.LookAt provides all the needed functionality to create 
		 * a target point for the camera easing */
		var from = new Pose(transform.position, transform.rotation);

		transform.position = focusedTransform.transform.position + (cameraDistance * forward);
		transform.LookAt(focusedTransform, up);

		var to = new Pose(transform.position, transform.rotation);
		if (cameraRoutine != null) {
			StopCoroutine(cameraRoutine);
		}
		cameraRoutine = StartCoroutine(Ease(from, to, positionEasingFunction, rotationEasingFunction));
	}
	private IEnumerator Ease(Pose from, Pose to, Vector3Ease vEaseFunc, QuaternionEase qEaseFunc)
	{
		/* Magic values in here. Nothing we can do, this is how easing works
		 * From 0.0f to 1.0f or [0.0, 1.0] */
		var t = 0.0f;
		transform.position = from.position;
		transform.rotation = from.rotation;
		yield return null;

		while (t < 1.0f) {
			t += Time.deltaTime / Time.fixedDeltaTime;

			transform.position = vEaseFunc(from.position, to.position, t);
			transform.rotation = qEaseFunc(from.rotation, to.rotation, t);
			yield return null;
		}

		transform.position = to.position;
		transform.rotation = to.rotation;
		cameraRoutine = null;
	}
}
