using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Synchronizer))]
public class SynchronizerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		// UpdatePrefabList();
		ShowClientInformation();
		DrawDefaultInspector();
	}

	void UpdatePrefabList()
	{
		if (Application.isPlaying) return;

		var synchronizer = target as Synchronizer;

		var resourcesAbsolutePath = System.IO.Path.Combine(Application.dataPath, "Resources");
		var prefabAbsolutePaths = System.IO.Directory.GetFiles(
			resourcesAbsolutePath, "*.prefab", System.IO.SearchOption.AllDirectories);

		synchronizer.ClearPrefabPathMap();
		foreach (var prefabAbsolutePath in prefabAbsolutePaths) {
			var localPath = prefabAbsolutePath.Substring(resourcesAbsolutePath.Length + 1).Replace(".prefab", "");
			var prefab = Resources.Load<GameObject>(localPath);
			synchronizer.AddPrefabPathMap(prefab, localPath);
		}
	}

	void ShowClientInformation()
	{
		EditorGUILayout.Toggle("Is Master", Synchronizer.IsMaster);
		EditorGUILayout.TextField("Timestamp", Synchronizer.Timestamp.ToString());
		EditorGUILayout.TextField("Connetions Num", Synchronizer.ConnectionsNum.ToString());
		EditorUtility.SetDirty(target);
	}
}
