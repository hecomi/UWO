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
	public int fireCoolDownFrame = 5;
	private int fireCoolDownCount_ = 0;

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

		Move();
		Rotate();
		Jump();
		Fire();
	}

	void Move()
	{
		var velocity = Vector3.zero;
		velocity += Input.GetAxisRaw("Horizontal") * transform.right;
		velocity += Input.GetAxisRaw("Vertical") * transform.forward;
		velocity = velocity.normalized * moveSpeed;
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
			velocity *= 1.5f;
		}
		transform.position += velocity * Time.deltaTime;
	}

	void Rotate()
	{
		var angularVelocity = 0f;
		if (Input.GetKey(KeyCode.Q)) {
			angularVelocity -= 1;
		}
		if (Input.GetKey(KeyCode.E)) {
			angularVelocity += 1;
		}
		angularVelocity *= rotationSpeed;
		transform.Rotate(Vector3.up, angularVelocity);
	}

	void Jump()
	{
		if (isGround_ && Input.GetKeyDown(KeyCode.Space)) {
			isGround_ = false;
			rigidbody.AddForce(Vector3.up * jumpForce);
		}
	}

	void Fire()
	{
		if (Input.GetKey(KeyCode.F)) {
			if (fireCoolDownCount_ <= 0) {
				fireCoolDownCount_ = fireCoolDownFrame;
				var forward = Camera.main.transform.forward;
				var obj = Instantiate(bullet, transform.position + forward, bullet.transform.rotation) as GameObject;
				obj.GetComponent<Rigidbody>().AddForce(forward * 3000);
				Synchronizer.Instantiate(emitEffect, obj.transform.position, obj.transform.rotation);
			}
		}
		--fireCoolDownCount_;
	}

	void OnCollisionEnter(Collision collision)
	{
		isGround_ = true;
	}
}