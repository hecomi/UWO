using UnityEngine;
using System.Collections;

namespace UWO
{

public class SynchronizedObject : MonoBehaviour
{
	[HideInInspector]
	public string id = System.Guid.Empty.ToString();

	private bool isLocal_ = true;
	public bool isLocal
	{
		get { return isLocal_;  }
		set { isLocal_ = value; }
	}
	public bool isRemote
	{
		get { return !isLocal_;  }
		set { isLocal_ = !value; }
	}

	[Tooltip("サーバ側へ状態を保存するよう通知する")]
	public bool isSavedToServer = false;
	[Tooltip("ローカルとリモートで出す Prefab を切り替える")]
	public bool isOverridePrefab = false;
	[Tooltip("ログアウトした時にオブジェクトをマスターへ引き継ぐ")]
	public bool isTakenOverToMaster = false;
	[Tooltip("リモートの isKinematic を ON にする")]
	public bool isRigidBodySync = true;
	[HideInInspector]
	public string prefabPath = "Not Set";

	[Tooltip("リモートから指定した時間以上反応がない時に消去する")]
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

	void OnDestroy()
	{
		Synchronizer.UnregisterGameObject(id, isLocal);
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

		if (isRigidBodySync && rigidbody_ != null) {
			rigidbody_.isKinematic = isRemote;
		}
	}

	public void NotifyAlive()
	{
		noMessageElapsedTime_ = 0f;
	}
}

}
