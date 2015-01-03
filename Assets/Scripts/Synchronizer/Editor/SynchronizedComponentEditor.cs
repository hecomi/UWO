using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UWO
{

[CustomEditor(typeof(SynchronizedComponent), true)]
public class SynchronizedComponentEditor : Editor
{
	bool originalInspectorFoldout = true;
	bool defaultInspectorFoldout = true;

	public override void OnInspectorGUI()
	{
		SynchronizerEditorUtility.AddSynchronizerGameObject();

		DrawComponentId();

		SynchronizerEditorUtility.DrawHorizontalLine(10f);

		originalInspectorFoldout = EditorGUILayout.Foldout(originalInspectorFoldout, "Snychronized Component");
		if (originalInspectorFoldout) {
			EditorGUI.indentLevel++;
			DrawOriginalInspector();
			EditorGUI.indentLevel--;
		}

		SynchronizerEditorUtility.DrawHorizontalLine(10f);

		defaultInspectorFoldout = EditorGUILayout.Foldout(defaultInspectorFoldout, "Default Inepector");
		if (defaultInspectorFoldout) {
			EditorGUI.indentLevel++;
			DrawDefaultInspector();
			EditorGUI.indentLevel--;
		}
	}

	void DrawComponentId()
	{
		var component = target as SynchronizedComponent;
		SynchronizerEditorUtility.ReadOnlyTextField("Component ID", component.id);
	}

	void DrawOriginalInspector()
	{
		var component = target as SynchronizedComponent;

		var heartBeatDuration = EditorGUILayout.FloatField("HeartBeat (sec)", component.heartBeatDuration);
		if (heartBeatDuration != component.heartBeatDuration) {
			component.heartBeatDuration = heartBeatDuration;
		}

		var sendFrameRate = EditorGUILayout.FloatField("Send FrameRate", component.sendFrameRate);
		if (sendFrameRate != component.sendFrameRate) {
			component.sendFrameRate = sendFrameRate;
		}
	}

	void AddSynchronizerGameObject()
	{
		if (GameObject.FindObjectOfType<Synchronizer>() == null) {
			var obj = new GameObject("Synchronizer");
			obj.AddComponent<Synchronizer>();
		}
	}
}

}
