using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UWO
{

[CustomEditor(typeof(Synchronizer))]
public class SynchronizerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		ShowClientInformation();
		DrawDefaultInspector();
	}

	void ShowClientInformation()
	{
		EditorGUILayout.Toggle("Is Master", Synchronizer.IsMaster);
		EditorGUILayout.TextField("Timestamp", Synchronizer.Timestamp.ToString());
		EditorGUILayout.TextField("Connetions Num", Synchronizer.ConnectionsNum.ToString());
		EditorUtility.SetDirty(target);
	}
}

}