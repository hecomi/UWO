#pragma warning disable 0108

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UWO;

[RequireComponent(typeof(SynchronizedObject))]
public class TestController : MonoBehaviour 
{
	public bool isMainPlayer
	{
		get { return GetComponent<SynchronizedObject>().isLocal; }
	}

	public float moveSpeed = 1f;
	public float rotationSpeed = 10f;
	public float jumpForce = 3000f;

	private bool isGround_ = false;

	private Rigidbody rigidbody_;
	public Rigidbody rigidbody
	{
		get { return (rigidbody_ = rigidbody_ ?? GetComponent<Rigidbody>()); }
	}

	public GameObject bullet;
	[ResourcePathAsPopup("prefab")]
	public string emitEffect;

	void Update() 
	{
		if (!isMainPlayer) return;

		var velocity = Vector3.zero;
		var angularVelocity = 0f;

		if (Input.GetKey(KeyCode.W)) {
			velocity += transform.forward;
		}
		if (Input.GetKey(KeyCode.S)) {
			velocity -= transform.forward;
		}
		if (Input.GetKey(KeyCode.A)) {
			velocity -= transform.right;
		}
		if (Input.GetKey(KeyCode.D)) {
			velocity += transform.right;
		}

		if (Input.GetKey(KeyCode.Q)) {
			angularVelocity -= 1;
		}
		if (Input.GetKey(KeyCode.E)) {
			angularVelocity += 1;
		}

		if (isGround_ && Input.GetKeyDown(KeyCode.Space)) {
			isGround_ = false;
			rigidbody.AddForce(Vector3.up * jumpForce);
		}
	
		velocity = velocity.normalized * moveSpeed;
		angularVelocity *= rotationSpeed;

		transform.position += velocity * Time.deltaTime;
		transform.Rotate(Vector3.up, angularVelocity);

		if (Input.GetKeyDown(KeyCode.Return)) {
			Destroy(gameObject);
		}

		if (Input.GetKey(KeyCode.F)) {
			var forward = Camera.main.transform.forward;
			var obj = Instantiate(bullet, transform.position + forward, bullet.transform.rotation) as GameObject;
			obj.GetComponent<Rigidbody>().AddForce(forward * 3000);
			Synchronizer.Instantiate(emitEffect, obj.transform.position, obj.transform.rotation);
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		isGround_ = true;
	}
}