using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour
{
	private Body next = null;
	private Material material = null;
	private Vector3 previousPosition = Vector3.zero;
	private Quaternion previousRotation = Quaternion.identity;

	protected Body Next { get => next; set => next = value; }

	public Material Material => material;
	public Vector3 CurrentPosition => transform.position;
	public Quaternion CurrentRotation => transform.rotation;
	public Vector3 PreviousPosition => previousPosition;
	public Quaternion PreviousRotation => previousRotation;

	private void Awake()
	{
		var renderer = GetComponent<MeshRenderer>();
		material = Instantiate(renderer.material);
		renderer.material = material;
	}
	public void Insert(Body next)
	{
		next.next = this.next;
		this.next = next;
	}
	public void SetBodyPosition(Vector3 position)
	{
		previousPosition = transform.position;
		transform.position = position;
	}
	public void SetBodyRotation(Quaternion rotation) 
	{
		previousRotation = transform.rotation;
		transform.rotation = rotation;
	}
	public void DestroyAll(System.Action<Body> onDestroy)
	{
		var body = this;
		while (body != null) {
			var temp = body.next;
			onDestroy(body);
			body.next = null;
			body = temp;
		}

	}
	public void TransformAll(System.Action<Body> onTransform)
	{
		var body = this;
		while(body != null) {
			onTransform(this);
			body = body.next;
		}
	}
	public void TransformAll(System.Action<Body, Body> onTransform)
	{
		var body = this;
		while (body != null) {
			onTransform(body, body.next);
			body = body.next;
		}
	}
}
