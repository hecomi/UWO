using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UWO
{

// Synchronizer はローカルとサーバの橋渡しをするクラス
// 各 GameObject と SynchronizedComponent の管理や、
// 各 Component からのメッセージの受付、JSON の送受信、
// その後の Component へのメッセージ配信を行う
[RequireComponent(typeof(WebSocketSyncServer))]
public class Synchronizer : MonoBehaviour
{
	public static Synchronizer Instance { get; set; }

	[Tooltip("サーバから自動的に指定されるマスターかどうかのフラグ")]
	public static bool IsMaster = false;
	[Tooltip("タイムスタンプ")]
	public static ulong Timestamp = 0;
	[Tooltip("サーバに接続しているクライアント数")]
	public static int ConnectionsNum = 0;
	[Tooltip("どれくらいの頻度でサーバと通信するか")]
	public int syncFps = 60;

	public float syncCycle
	{
		get { return 1f / syncFps; }
	}
	private float elapsedTimeFromLastSync_ = 0f;

	public static readonly char CommandDelimiterChar = '\t';
	public static readonly char MessageDelimiterChar = '\n';
	private static readonly char[] CommandDelimiter = new char[] {CommandDelimiterChar};
	private static readonly char[] MessageDelimiter = new char[] {MessageDelimiterChar};

	// 指定された ID の GameObject を特定するテーブル
	private Dictionary<string, GameObject> gameObjectLookup_
		= new Dictionary<string, GameObject>();

	// 指定された ID の SynchronizedComponent を特定するテーブル
	private Dictionary<string, SynchronizedComponent> componentLookup_
		= new Dictionary<string, SynchronizedComponent>();

	// サーバに送信するデータ
	// 適宜色々な Component から SynchronizedComponent.Send() 経由で情報を詰められ、
	// 適当なタイミングで Emit() でサーバに送られる。
	// 送信後はクリアされる
	private Dictionary<string, string> updatedComponents_  = new Dictionary<string, string>();
	private Dictionary<string, string> savedComponents_  = new Dictionary<string, string>();
	private List<string> deletedGameObjects_ = new List<string>();

	private struct AddedNetworkGameObject
	{
		public string prefabPath;
		public Vector3 position;
		public Quaternion rotation;
	}
	private List<AddedNetworkGameObject> addedNetworkGameObjects_ = new List<AddedNetworkGameObject>();

	// WebSocket サーバ
	private WebSocketSyncServer server_;

	// 大量のメッセージを受信した場合はキャッシュしておいて順次実行する
	private List<string> cachedCommands_ = new List<string>();

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

	void Update()
	{
		ProcessCachedCommands(10);
		if (Time.frameCount > 600 && cachedCommands_.Count > 5000) {
			ProcessCachedCommands(cachedCommands_.Count);
		}
	}

	void OnDestroy()
	{
		server_.OnReceive -= OnReceive;
	}

	void LateUpdate()
	{
		elapsedTimeFromLastSync_ += Time.deltaTime;
		if (elapsedTimeFromLastSync_ + 0.001f > syncCycle) {
			elapsedTimeFromLastSync_ = 0;
			Emit();
		}
	}

	void Emit()
	{
		if (server_.isConnected) {
			var message = "";

			// 更新のみ行うコンポーネント
			foreach (var updatedComponent in updatedComponents_) {
				var componentId = updatedComponent.Key;
				var args = updatedComponent.Value;
				message += "u" + CommandDelimiterChar + componentId
					+ CommandDelimiterChar + args + MessageDelimiterChar;
			}
			updatedComponents_.Clear();

			// サーバに保存されるコンポーネント
			foreach (var savedComponent in savedComponents_) {
				var componentId = savedComponent.Key;
				var args = savedComponent.Value;
				message += "s" + CommandDelimiterChar + componentId
					+ CommandDelimiterChar + args + MessageDelimiterChar;
			}
			savedComponents_.Clear();

			// 削除するゲームオブジェクト
			foreach (var id in deletedGameObjects_) {
				message += "d" + CommandDelimiterChar + id + MessageDelimiterChar;
			}
			deletedGameObjects_.Clear();

			// 新たに追加されるゲームオブジェクト
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
		cachedCommands_.AddRange(message.Split(MessageDelimiter).ToList());
		ProcessCachedCommands(100);
	}

	void ProcessCachedCommands(int maxCommand)
	{
		int n = 0;
		for (int i = 0; i < maxCommand && i < cachedCommands_.Count; ++i, ++n) {
			var command = cachedCommands_[i];
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
				case "s":
				case "u":
					UpdateComponent(args);
					break;
				case "o": // 最初に接続してきたクライアントが保存されたデータを所有
					UpdateComponent(args, true);
					break;
				case "i":
					UpdateClientInformation(args);
					break;
			}
		}
		cachedCommands_.RemoveRange(0, n);
	}

	void UpdateClientInformation(string[] args)
	{
		if (args.Length < 4) {
			Debug.LogWarning("Invalid arguments for SetMaster");
			return;
		}

		IsMaster = args[1].AsBool();
		Timestamp = args[2].AsUlong();
		ConnectionsNum = args[3].AsInt();

		GlobalGUI.SetNum(ConnectionsNum.ToString());
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

	void UpdateComponent(string[] args, bool isBecomeLocalImmediately = false)
	{
		if (args.Length < 7) {
			Debug.LogWarning("Invalid arguments for UpdateComponent: " + string.Join("  ", args));
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
			var isForceUpdate = isBecomeLocalImmediately;
			if (type == "") {
				Debug.Log("No type specified: " + string.Join("  ", args));
				return;
			}
			component.Receive(value, type, isForceUpdate);
			if (isBecomeLocalImmediately) {
				component.syncObject.isLocal = true;
			}
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
		var synchronizedObj = 
			gameObj.GetComponent<SynchronizedObject>() ?? 
			gameObj.AddComponent<SynchronizedObject>();
		gameObj.transform.position = Vector3.one * 999999f;
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
			Debug.LogError(componentName + " is not attached to " + syncObjectId);
			return null;
		}

		targetComponent.id = id;
		componentLookup_.Add(id, targetComponent);

		return targetComponent;
	}

	void SendImpl(SynchronizedComponent component, string value, string type)
	{
		if (string.IsNullOrEmpty(type)) {
			Debug.LogWarningFormat(
				"type is not specified:\n  {0}: \"{1}\"", 
				component.componentName, value);
			return;
		}
		var id = component.id;
		var args = component.syncObjectId  + CommandDelimiterChar +
		           component.prefabPath    + CommandDelimiterChar +
		           component.componentName + CommandDelimiterChar +
		           value + CommandDelimiterChar + type;
		if (component.syncObject.isSavedToServer) {
			savedComponents_[id] = args;
		} else {
			updatedComponents_[id] = args;
		}
	}

	void NotifyDead(string syncObjectId)
	{
		if (deletedGameObjects_.Contains(syncObjectId)) return;
		deletedGameObjects_.Add(syncObjectId);
	}

	void RegisterComponentImpl(string id, SynchronizedComponent component)
	{
		if (componentLookup_.ContainsKey(id)) return;
		componentLookup_.Add(id, component);
	}

	void UnregisterComponentImpl(string id)
	{
		if (!componentLookup_.ContainsKey(id)) return;
		componentLookup_.Remove(id);
	}

	void RegisterGameObjectImpl(string id, GameObject obj)
	{
		if (gameObjectLookup_.ContainsKey(id)) return;
		gameObjectLookup_.Add(id, obj);
	}

	void UnregisterGameObjectImpl(string id, bool isLocal)
	{
		if (!gameObjectLookup_.ContainsKey(id)) return;
		if (isLocal) {
			NotifyDead(id);
		}
		gameObjectLookup_.Remove(id);
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
		var target = Resources.Load<GameObject>(prefabPath);
		// Synchronized GameObject
		if (target.GetComponent<SynchronizedObject>()) {
			Instantiate(target, position, rotation);
		// Not Synchronized GameObject
		} else {
			var addedNetworkGameObject = new AddedNetworkGameObject() {
				prefabPath = prefabPath,
				position   = position,
				rotation   = rotation
			};
			Instance.addedNetworkGameObjects_.Add(addedNetworkGameObject);
		}
	}

	public static void Destroy(GameObject target)
	{
		var syncObj = target.GetComponent<SynchronizedObject>();
		if (!syncObj) {
			Debug.LogWarning(target.name + " is not synchronized object.");
			return;
		}
		Instance.NotifyDead(syncObj.id);
	}
}

}
