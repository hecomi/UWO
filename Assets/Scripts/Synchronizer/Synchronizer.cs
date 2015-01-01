using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UWO;

// Synchronizer はローカルとサーバの橋渡しをするクラス
// 各 GameObject と SynchronizedComponent の管理や、
// 各 Component からのメッセージの受付、JSON の送受信、
// その後の Component へのメッセージ配信を行う
[RequireComponent(typeof(WebSocketSyncServer))]
public class Synchronizer : MonoBehaviour
{
	public static Synchronizer Instance { get; set; }
	public static bool IsMaster = false;
	public static ulong Timestamp = 0;
	public static int ConnectionsNum = 0;

	private const char CommandDelimiterChar = '\t';
	private const char MessageDelimiterChar = '\n';
	static private readonly char[] CommandDelimiter = new char[] {CommandDelimiterChar};
	static private readonly char[] MessageDelimiter = new char[] {MessageDelimiterChar};

	// Resources 内の Prefab を Instantiate した時に
	// そのプレファブのパス（e.g. Bullet/Big）を特定するためのマップ
	private Dictionary<GameObject, string> prefabPathMap_
		= new Dictionary<GameObject, string>();

	// 指定された ID の GameObject を特定するテーブル
	private Dictionary<string, GameObject> gameObjectLookup_
		= new Dictionary<string, GameObject>();

	// 指定された ID の SynchronizedComponent を特定するテーブル
	private Dictionary<string, SynchronizedComponent> componentLookup_
		= new Dictionary<string, SynchronizedComponent>();

	// サーバに送信するデータ
	// 適宜色々な Component から Send() 経由で情報を詰められ、
	// 適当なタイミングでサーバに送られる。
	// 送信後はクリアされる
	private Dictionary<string, string> updatedComponents_  = new Dictionary<string, string>();
	private List<string> deletedGameObjects_ = new List<string>();

	private struct AddedNetworkGameObject
	{
		public string prefabPath;
		public Vector3 position;
		public Quaternion rotation;
	}
	private List<AddedNetworkGameObject> addedNetworkGameObjects_ = new List<AddedNetworkGameObject>();

	// どれくらいの頻度でサーバと通信するか
	public int emitSkipCount = 0;
	private int emitSkipCounter_ = 0;

	// WebSocket サーバ
	private WebSocketSyncServer server_;

	void Awake()
	{
		if (Instance != null) {
			Debug.LogWarning("Synchronizer already exists in this scene.");
			return;
		}
		Instance = this;
	}

	// WebSocket サーバへのイベントリスナ登録
	void Start()
	{
		server_ = GetComponent<WebSocketSyncServer>();
		server_.OnReceive += OnReceive;
	}

	void LateUpdate()
	{
		++emitSkipCounter_;
		if (emitSkipCounter_ > emitSkipCount) {
			emitSkipCounter_ = 0;
			Emit();
		}
	}

	void Emit()
	{
		if (server_.isConnected) {
			var message = "";
			foreach (var updateComponent in updatedComponents_) {
				var componentId = updateComponent.Key;
				var args = updateComponent.Value;
				message += "u" + CommandDelimiterChar + componentId
					+ CommandDelimiterChar + args + MessageDelimiterChar;
			}
			updatedComponents_.Clear();

			foreach (var id in deletedGameObjects_) {
				message += "d" + CommandDelimiterChar + id + MessageDelimiterChar;
			}
			deletedGameObjects_.Clear();

			foreach (var obj in addedNetworkGameObjects_) {
				message += "a" + CommandDelimiterChar +
					obj.prefabPath + CommandDelimiterChar + 
					obj.position.AsString() + CommandDelimiterChar + 
					obj.rotation.AsString() + MessageDelimiterChar;
			}
			addedNetworkGameObjects_.Clear();

			if (message != "") {
				server_.Send(message);
			}
		}
	}

	void OnReceive(string message)
	{
		var commands = message.Split(MessageDelimiter);

		foreach (var command in commands) {
			if (string.IsNullOrEmpty(command)) {
				continue;
			}

			var args = command.Split(CommandDelimiter);
			var kind = args[0];

			switch (kind) {
				case "a":
					AddNetworkGameObject(args);
					break;
				case "d":
					DeleteGameObject(args);
					break;
				case "u":
					UpdateComponent(args);
					break;
				case "i":
					SetClientInformation(args);
					break;
			}
		}
	}

	void SetClientInformation(string[] args)
	{
		if (args.Length < 4) {
			Debug.LogWarning("Invalid arguments for SetMaster");
			return;
		}

		IsMaster = args[1].AsBool();
		Timestamp = args[2].AsUlong();
		ConnectionsNum = args[3].AsInt();
	}

	void AddNetworkGameObject(string[] args)
	{
		if (args.Length < 4) {
			Debug.LogWarning("Invalid arguments for AddNetworkGameObject");
			return;
		}

		var prefabPath = args[1];
		var prefab = Resources.Load<GameObject>(prefabPath);
		if (prefab == null) {
			Debug.LogWarning(prefabPath + " is invalid prefab path");
			return;
		}

		var position = args[2].AsVector3();
		var rotation = args[3].AsQuaternion();
		Instantiate(prefab, position, rotation);
	}

	void DeleteGameObject(string[] args)
	{
		if (args.Length < 2) {
			Debug.LogWarning("Invalid arguments for DeleteGameObject");
			return;
		}

		var id = args[1];
		if (gameObjectLookup_.ContainsKey(id)) {
			DestroyImmediate(gameObjectLookup_[id]);
		}
	}

	void UpdateComponent(string[] args)
	{
		if (args.Length < 7) {
			Debug.LogWarning("Invalid arguments for UpdateComponent");
			return;
		}

		var componentId   = args[1];
		var gameObjectId  = args[2];
		var prefabPath    = args[3];
		var componentName = args[4];
		var value         = args[5];
		var type          = args[6];

		var component = GetSynchronizedComponent(componentId, componentName, gameObjectId, prefabPath);
		if (component != null) {
			component.Receive(value, type);
		}
	}

	GameObject GetGameObject(string id, string prefabPath = "")
	{
		if (gameObjectLookup_.ContainsKey(id)) {
			return gameObjectLookup_[id];
		} else if (prefabPath != "") {
			return AddGameObject(id, prefabPath);
		}
		return null;
	}

	GameObject AddGameObject(string id, string prefabPath)
	{
		var prefab = Resources.Load<GameObject>(prefabPath);
		if (!prefab) {
			Debug.LogError(prefabPath + " is invalid prefab path.");
			return null;
		}
		var gameObj = Instantiate(prefab) as GameObject;
		var synchronizedObj = gameObj.GetComponent<SynchronizedObject>() ?? gameObj.AddComponent<SynchronizedObject>();
		synchronizedObj.id = id;
		synchronizedObj.isRemote = true;
		synchronizedObj.prefabPath = prefabPath;
		gameObjectLookup_.Add(id, gameObj);
		return gameObj;
	}

	SynchronizedObject GetSynchronizedObject(string id)
	{
		return gameObjectLookup_.ContainsKey(id) ?
			gameObjectLookup_[id].GetComponent<SynchronizedObject>() : null;
	}

	SynchronizedComponent GetSynchronizedComponent(
		string id, string componentName = "", string gameObjectId = "", string prefabPath = "")
	{
		if (componentLookup_.ContainsKey(id)) {
			return componentLookup_[id];
		} else if (componentName != "") {
			return AddSynchronizedComponent(id, componentName, gameObjectId, prefabPath);
		}
		return null;
	}

	SynchronizedComponent AddSynchronizedComponent(
		string id, string componentName, string syncObjectId, string prefabPath)
	{
		var gameObj = GetGameObject(syncObjectId, prefabPath);
		if (gameObj == null) { return null; }

		SynchronizedComponent targetComponent = null;
		foreach (var component in gameObj.GetComponentsInChildren<SynchronizedComponent>()) {
			if (component.componentName == componentName) {
				targetComponent = component;
				break;
			}
		}
		if (targetComponent == null) {
			Debug.LogError(componentName + " does not attached to " + syncObjectId);
			return null;
		}

		targetComponent.id = id;
		componentLookup_.Add(id, targetComponent);

		return targetComponent;
	}

	void SendImpl(SynchronizedComponent component, string value, string type)
	{
		var id = component.id;
		var args = component.syncObjectId  + CommandDelimiterChar +
		           component.prefabPath    + CommandDelimiterChar +
		           component.componentName + CommandDelimiterChar +
		           value + CommandDelimiterChar + type;
		updatedComponents_[id] = args;
	}

	void NotifyDead(string syncObjectId)
	{
		deletedGameObjects_.Add(syncObjectId);
	}

	void RegisterComponentImpl(string id, SynchronizedComponent component)
	{
		componentLookup_.Add(id, component);
	}

	void UnregisterComponentImpl(string id)
	{
		componentLookup_.Remove(id);
	}

	void RegisterGameObjectImpl(string id, GameObject obj)
	{
		gameObjectLookup_.Add(id, obj);
	}

	void UnregisterGameObjectImpl(string id, bool isLocal)
	{
		if (isLocal) {
			NotifyDead(id);
		}
		gameObjectLookup_.Remove(id);
	}

	public void AddPrefabPathMap(GameObject prefab, string prefabPath)
	{
		prefabPathMap_.Add(prefab, prefabPath);
	}

	public void ClearPrefabPathMap()
	{
		prefabPathMap_.Clear();
	}

	public static void RegisterComponent(string id, SynchronizedComponent component)
	{
		Instance.RegisterComponentImpl(id, component);
	}

	public static void UnregisterComponent(string id)
	{
		if (Instance) {
			Instance.UnregisterComponentImpl(id);
		}
	}

	public static void RegisterGameObject(string id, GameObject obj)
	{
		Instance.RegisterGameObjectImpl(id, obj);
	}

	public static void UnregisterGameObject(string id, bool isLocal)
	{
		if (Instance) {
			Instance.UnregisterGameObjectImpl(id, isLocal);
		}
	}

	public static void Send(SynchronizedComponent component, string val, string type)
	{
		Instance.SendImpl(component, val, type);
	}

	public static void Instantiate(string prefabPath, Vector3 position, Quaternion rotation)
	{
		var addedNetworkGameObject = new AddedNetworkGameObject() {
			prefabPath = prefabPath,
			position   = position,
			rotation   = rotation
		};
		Instance.addedNetworkGameObjects_.Add(addedNetworkGameObject);
	}

	/*
	public static void Instantiate(GameObject gameObject, Vector3 position, Quaternion rotation)
	{
		// NOTE: not work ... always go into this if.
		if (!Instance.prefabPathMap_.ContainsKey(gameObject)) {
			Debug.LogError(gameObject.name + " is not registered to Synchronizer prefab path map.");
		}
		var prefabPath = Instance.prefabPathMap_[gameObject];
		Instantiate(prefabPath, position, rotation);
	}
	*/
}

