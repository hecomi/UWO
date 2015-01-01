using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SynchronizedObject))]
public abstract class SynchronizedComponent: MonoBehaviour
{
	static private readonly char[] Delimiter = new char[] {','};
	public string id = System.Guid.Empty.ToString();

	private SynchronizedObject syncObject_;
	public SynchronizedObject syncObject
	{
		get { return syncObject_ ?? (syncObject_ = GetComponent<SynchronizedObject>()); }
	}

	public string syncObjectId 
	{ 
		get { return syncObject.id; } 
	}

	public bool isLocal 
	{ 
		get { return syncObject.isLocal; } 
	}

	public bool isRemote 
	{ 
		get { return syncObject.isRemote; } 
	}

	public string prefabPath
	{ 
		get { return syncObject.prefabPath; } 
	}

	public string componentName
	{
		get { return this.GetType().Name; }
	}

	public float heartBeatDuration = 1f;
	public float sendFrameRate = 30f;
	public float sendFrequency
	{
		get { return 1f / sendFrameRate; }
	}	
	public float easing
	{
		get { return sendFrameRate / 60; } // TODO: use actual framerate
	}

	private string preValue_;
	private string preType_;

	void Awake()
	{
		id = System.Guid.NewGuid().ToString();
	}

	void Start()
	{
		if (isLocal) {
			Synchronizer.RegisterComponent(id, this);
			StartCoroutine(Sync());
			StartCoroutine(HeartBeat());
			OnSend();
		}
	}

	protected virtual void OnLocalUpdate()  {}
	protected virtual void OnRemoteUpdate() {}

	void Update()
	{
		if (isLocal) {
			OnLocalUpdate();
		} else {
			OnRemoteUpdate();
		}
	}

	IEnumerator Sync()
	{
		for (;;) {
			if (isLocal) { 
				OnSend(); 
			}
			yield return new WaitForSeconds(sendFrequency);
		}
	}

	IEnumerator HeartBeat()
	{
		yield return new WaitForEndOfFrame(); 
		Synchronizer.Send(this, preValue_, preType_);
		for (;;) {
			if (isLocal && !string.IsNullOrEmpty(preValue_) && !string.IsNullOrEmpty(preType_)) {
				Synchronizer.Send(this, preValue_, preType_);
			}
			yield return new WaitForSeconds(heartBeatDuration);
		}
	}

	void OnDestroy()
	{
		Synchronizer.UnregisterComponent(id);
	}

	protected abstract void OnSend();

	protected void Send(string value, string type)
	{
		if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(type) && preValue_ != value) {
			preValue_ = value;
			preType_ = type;
			Synchronizer.Send(this, value, type);
		}
	}

	protected void Send(string value)
	{
		Send(value, "string");
	}

	protected void Send(int value)
	{
		Send(Encode(value), "int");
	}

	protected void Send(float value)
	{
		Send(Encode(value), "float");
	}

	protected void Send(Vector2 value)
	{
		Send(Encode(value), "vector2");
	}

	protected void Send(Vector3 value)
	{
		Send(Encode(value), "vector3");
	}

	protected void Send(Quaternion value)
	{
		Send(Encode(value), "quaternion");
	}

	protected virtual void OnReceive(string value) {}
	protected virtual void OnReceive(int value) {}
	protected virtual void OnReceive(float value) {}
	protected virtual void OnReceive(Vector2 value) {}
	protected virtual void OnReceive(Vector3 value) {}
	protected virtual void OnReceive(Quaternion value) {}

	public void Receive(string value, string type)
	{
		if (isLocal) return;
		syncObject.NotifyAlive();

		switch (type) {
			case "string":
				OnReceive(value);
				break;
			case "int":
				OnReceive(DecodeToInt(value));
				break;
			case "float":
				OnReceive(DecodeToFloat(value));
				break;
			case "vector2":
				OnReceive(DecodeToVector2(value));
				break;
			case "vector3":
				OnReceive(DecodeToVector3(value));
				break;
			case "quaternion":
				OnReceive(DecodeToQuaternion(value));
				break;
			default:
				Debug.LogWarning("typeof(" + type + ") is invalid type for the value of: " + value);
				break;
		}
	}

	int DecodeToInt(string value) {
		return int.Parse(value);
	}

	float DecodeToFloat(string value)
	{
		return float.Parse(value);
	}

	Vector2 DecodeToVector2(string value)
	{
		var args = value.Split(Delimiter);
		return new Vector2(float.Parse(args[0]), float.Parse(args[1]));
	}

	Vector3 DecodeToVector3(string value)
	{
		var args = value.Split(Delimiter);
		return new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
	}

	Quaternion DecodeToQuaternion(string value)
	{
		var args = value.Split(Delimiter);
		return new Quaternion(float.Parse(args[0]), float.Parse(args[1]), 
		                      float.Parse(args[2]), float.Parse(args[3]));
	}

	string Encode(int value) 
	{
		return value.ToString();
	}

	string Encode(float value) 
	{
		return value.ToString();
	}

	string Encode(Vector2 value) 
	{
		return value.x.ToString() + "," + value.y.ToString();
	}

	string Encode(Vector3 value) 
	{
		return value.x.ToString() + "," + value.y.ToString() + "," + value.z.ToString();
	}

	string Encode(Quaternion value) 
	{
		return value.x.ToString() + "," + value.y.ToString() + "," + value.z.ToString() + "," + value.w.ToString();
	}
}

