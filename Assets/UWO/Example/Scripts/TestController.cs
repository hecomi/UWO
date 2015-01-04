#pragma warning disable 0108

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UWO;

[RequireComponent(typeof(SynchronizedObject))]
public class TestController : MonoBehaviour 
{
	public enum FireMode
	{
		FireBullet,
		CreateBlock,
		DeleteBlock
	}
	public FireMode fireMode;

	public bool isMainPlayer
	{
		get { return GetComponent<SynchronizedObject>().isLocal; }
	}

	public float moveSpeed = 1f;
	public float rotationSpeed = 10f;
	public float jumpForce = 3000f;
	public const int fireCoolDownFrame = 5;
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
	[ResourcePathAsPopup("prefab")]
	public string block;

	public GameObject blockTarget;
	public LayerMask blockAddableLayer;
	public const int mouseClickFrame = 15;
	private int mouseClickCount_ = 0;

	private GameObject previousSelectedObject_;
	private Material previousSelectedMaterial_;
	public Material deleteTargetMaterial;

	void Start()
	{
	}

	void Update() 
	{
		if (!isMainPlayer) return;
		if (GlobalState.isInputting) return;

		ChangeMode();
		Move();
		Rotate();
		Jump();
		Fire();
	}

	void ChangeMode()
	{
		if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1)) {
			fireMode = FireMode.FireBullet;
		}
		if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2)) {
			fireMode = FireMode.CreateBlock;
		}
		if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3)) {
			fireMode = FireMode.DeleteBlock;
		}

		switch (fireMode) {
			case FireMode.FireBullet: 
				GlobalGUI.SetTool("BULLET");
				break;
			case FireMode.CreateBlock: 
				GlobalGUI.SetTool("+BLOCK");
				break;
			case FireMode.DeleteBlock: 
				GlobalGUI.SetTool("-BLOCK");
				break;
		}
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
		var isClick = CheckMouseClick();
		FirePreProcess();
		switch (fireMode) {
			case FireMode.FireBullet:
				FireBullet(isClick);
				break;
			case FireMode.CreateBlock:
				CreateBlock(isClick); 
				break;
			case FireMode.DeleteBlock:
				DeleteBlock(isClick); 
				break;
		}
		FirePostProcess();
	}

	bool CheckMouseClick()
	{
		bool isClick = false;
		if (Input.GetMouseButtonDown(0)) {
			mouseClickCount_ = 0;
		} else if (Input.GetMouseButtonUp(0) && mouseClickCount_ <= mouseClickFrame) {
			isClick = true;
		}
		++mouseClickCount_;
		return isClick;
	}

	void FirePreProcess()
	{
		// Reset CreateBlock
		blockTarget.SetActive(false);

		// Reset DeleteBlock
		if (previousSelectedObject_) {
			previousSelectedObject_.GetComponent<Renderer>().material = previousSelectedMaterial_;
		}
	}

	void FirePostProcess()
	{
	}

	void FireBullet(bool isFire)
	{
		if (Input.GetKey(KeyCode.F) || isFire) {
			if (fireCoolDownCount_ <= 0) {
				fireCoolDownCount_ = fireCoolDownFrame;
				var forward = Camera.main.transform.forward;
				var obj = Instantiate(bullet, transform.position + forward, bullet.transform.rotation) as GameObject;
				obj.GetComponent<Rigidbody>().AddForce(forward * 3000);
				Synchronizer.Instantiate(emitEffect, obj.transform.position, obj.transform.rotation);
				Score.Add(5);
			}
		}
		if (Input.GetKeyUp(KeyCode.F)) {
			fireCoolDownCount_ = 0;
		}
		--fireCoolDownCount_;
	}

	void CreateBlock(bool isClick)
	{
		RaycastHit hit;
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, 10f, blockAddableLayer)) {
			var distance = Vector3.Distance(transform.position, hit.point);
			if (distance < 3f) {
				var point = hit.point + hit.normal * 0.5f;
				point.x = Mathf.Floor(point.x + 0.5f);
				point.y = Mathf.Floor(point.y + 0.5f);
				point.z = Mathf.Floor(point.z + 0.5f);

				if (point.y < 30f) {
					blockTarget.transform.position = point;
					blockTarget.SetActive(true);
					if (isClick) {
						Synchronizer.Instantiate(block, point, Quaternion.identity); 
						Score.Add(100);
					}
				}
			}
		}
	}

	void DeleteBlock(bool isClick)
	{
		RaycastHit hit;
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, 10f, blockAddableLayer)) {
			var distance = Vector3.Distance(transform.position, hit.point);
			if (distance < 3f && hit.transform.tag == "Deletable") {
				previousSelectedObject_ = hit.transform.gameObject;
				previousSelectedMaterial_ = hit.transform.GetComponent<Renderer>().material;
				hit.transform.GetComponent<Renderer>().material = deleteTargetMaterial;

				if (isClick) {
					Destroy(previousSelectedObject_);
					previousSelectedObject_ = null;
					Score.Add(10);
				}
			}
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		isGround_ = true;
	}
}