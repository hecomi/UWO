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
		DeleteBlock,
		ChangeBlockColor,
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

	public GameObject changeBlockColorTarget;

	[ResourcePathAsPopup("prefab")]
	public string changeBlockColorEffect;

	public int aquirePointPerFrame = 1;
	public int fireConsumePoint = 10;
	public int createBlockConsumePoint = 300;
	public int deleteBlockConsumePoint = 100;
	public int  changeBlockColorConsumePoint = 200;

	[ResourcePathAsPopup("prefab")]
	public string createEffect;
	[ResourcePathAsPopup("prefab")]
	public string destroyEffect;

	void Update() 
	{
		if (!isMainPlayer) return;
		if (GlobalState.isInputting) return;

		ChangeMode();
		Move();
		Rotate();
		Jump();
		Fire();

		var altitudeBonus = Mathf.FloorToInt((transform.position.y + 4.5f) / 12f);
		Score.Add(aquirePointPerFrame + altitudeBonus);
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
		if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4)) {
			fireMode = FireMode.ChangeBlockColor;
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
			case FireMode.ChangeBlockColor:
				GlobalGUI.SetTool("COLOR");
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
			var player = GameObject.FindGameObjectWithTag("Player");
			var position = player != null ? player.transform.position : Vector3.zero;
			SoundManager.Play("Jump", position);
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
			case FireMode.ChangeBlockColor:
				ChangeBlockColor(isClick); 
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

		// Reset ChangeBlockColor
		changeBlockColorTarget.SetActive(false);
	}

	void FirePostProcess()
	{
	}

	void FireBullet(bool isFire)
	{
		if (Input.GetKey(KeyCode.F) || isFire) {
			if (fireCoolDownCount_ <= 0 && Score.CanUse(fireConsumePoint)) {
				fireCoolDownCount_ = fireCoolDownFrame;

				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				var direction = ray.direction;
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
					direction = (hit.point - transform.position).normalized;
				}
				var obj = Instantiate(bullet, transform.position + direction, bullet.transform.rotation) as GameObject;
				obj.GetComponent<Rigidbody>().AddForce(direction * 3000);
				Synchronizer.Instantiate(emitEffect, obj.transform.position, obj.transform.rotation);
				Score.Sub(fireConsumePoint);
				SoundManager.Play("FireBullet", obj.transform.position);
			}
		}
		if (Input.GetKeyUp(KeyCode.F)) {
			fireCoolDownCount_ = 0;
		}
		--fireCoolDownCount_;
	}

	bool ScreenPointToAddableLayer(out RaycastHit hit)
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		return Physics.Raycast(ray, out hit, 10f, blockAddableLayer);
	}

	void CreateBlock(bool isClick)
	{
		RaycastHit hit;
		if (ScreenPointToAddableLayer(out hit)) {
			var distance = Vector3.Distance(transform.position, hit.point);
			if (distance < 5f) {
				var point = hit.point + hit.normal * 0.5f;
				point.x = Mathf.Floor(point.x + 0.5f);
				point.y = Mathf.Floor(point.y + 0.5f);
				point.z = Mathf.Floor(point.z + 0.5f);

				if (point.y < 30f) {
					blockTarget.transform.position = point;
					blockTarget.SetActive(true);
					if (isClick) {
						if (Score.CanUse(createBlockConsumePoint)) {
							Synchronizer.Instantiate(block, point, Quaternion.identity); 
							Synchronizer.Instantiate(createEffect, point, Quaternion.identity);
							Score.Sub(createBlockConsumePoint);
							SoundManager.Play("CreateBlock", point);
						} else {
							Notification.Show("More point is needed to create a block...");
						}
					}
				}
			}
		}
	}

	void DeleteBlock(bool isClick)
	{
		RaycastHit hit;
		if (ScreenPointToAddableLayer(out hit)) {
			var distance = Vector3.Distance(transform.position, hit.point);
			if (distance < 5f && hit.transform.tag == "Block") {
				previousSelectedObject_ = hit.transform.gameObject;
				previousSelectedMaterial_ = hit.transform.GetComponent<Renderer>().material;
				hit.transform.GetComponent<Renderer>().material = deleteTargetMaterial;

				if (isClick) {
					if (Score.CanUse(deleteBlockConsumePoint)) {
						var t = previousSelectedObject_.transform;
						Synchronizer.Instantiate(destroyEffect, t.position, t.rotation);
						Synchronizer.Destroy(previousSelectedObject_);
						previousSelectedObject_ = null;
						Score.Sub(deleteBlockConsumePoint);
						SoundManager.Play("DeleteBlock", hit.transform.position);
					} else {
						Notification.Show("More point is needed to delete a block...");
					}
				}
			}
		}
	}

	void ChangeBlockColor(bool isClick) 
	{
		RaycastHit hit;
		if (ScreenPointToAddableLayer(out hit)) {
			var distance = Vector3.Distance(transform.position, hit.point);
			if (distance < 5f && hit.transform.tag == "Block") {
				var colorSetter = hit.transform.GetComponent<ChangeBlockColorAndSync>();
				changeBlockColorTarget.transform.position = hit.transform.position;
				changeBlockColorTarget.SetActive(true);
				changeBlockColorTarget.SendMessage("ChangeColor", colorSetter.GetNextColor());

				if (isClick) {
					if (Score.CanUse(changeBlockColorConsumePoint)) {
						Synchronizer.Instantiate(changeBlockColorEffect, hit.transform.position, hit.transform.rotation);
						colorSetter.SetNextColor();
						previousSelectedObject_ = null;
						Score.Sub(changeBlockColorConsumePoint);
						SoundManager.Play("ChangeBlockColor", hit.transform.position);
					} else {
						Notification.Show("More point is needed to change the block color...");
					}
				}
			}
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		isGround_ = true;
	}
}