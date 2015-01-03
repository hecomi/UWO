using UnityEngine;
using System.Collections;

namespace UWO
{

public class SynchronizedObject : MonoBehaviour 
{
	[HideInInspector]
	public string id = System.Guid.Empty.ToString();
	public bool isLocal = true;
	public bool isSavedToServer = false;
	public bool isOverridePrefab = false;
	public bool isTakenOverToMaster = false;
	public bool isRemote
	{
		get { return !isLocal;  }
		set { isLocal = !value; }
	}
	public bool isSetNetworkRigidBodyKinematicAutomatically = true;
	[HideInInspector]
	public string prefabPath = "Not Set";

	public float deadTime = 5f;
	private float noMessageElapsedTime_ = 0f;

	private Rigidbody rigidbody_;

	void Awake()
	{
		id = System.Guid.NewGuid().ToString();
		rigidbody_ = GetComponent<Rigidbody>();
	}

	void Start()
	{
		if (isLocal) {
			Synchronizer.RegisterGameObject(id, gameObject);
		}
	}

	void Update()
	{
		if (isRemote) {
			noMessageElapsedTime_ += Time.deltaTime;
			if (noMessageElapsedTime_ > deadTime) {
				if (Synchronizer.IsMaster && isTakenOverToMaster) {
					isLocal = true;
				} else {
					DestroyImmediate(gameObject);
				}
			}
		}

		if (isSetNetworkRigidBodyKinematicAutomatically && rigidbody_ != null) {
			rigidbody_.isKinematic = isRemote;
		}
	}

	void OnDestroy()
	{
		Synchronizer.UnregisterGameObject(id, isLocal);
	}

	public void NotifyAlive()
	{
		noMessageElapsedTime_ = 0f;
	}
}

}