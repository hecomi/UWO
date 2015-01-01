using UnityEngine;
using UnityEditor;

public static class SynchronizerEditorUtility
{
	public static void AddSynchronizerGameObject()
	{
		if (GameObject.FindObjectOfType<Synchronizer>() == null) {
			var obj = new GameObject("Synchronizer");
			obj.AddComponent<Synchronizer>();
		}
	}

	public static void DrawHorizontalLine(float height)
	{
		var area = GUILayoutUtility.GetRect(Screen.width - 120, height);
		Handles.color = new Color(1f, 1f, 1f, 0.1f);
		Handles.DrawLine(
			new Vector3(area.x,    area.y + height/2),
			new Vector3(area.xMax, area.y + height/2)
		);
	}

	public static void ReadOnlyTextField(string label, string text)
	{
		EditorGUILayout.BeginHorizontal(); {
			EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
			EditorGUILayout.SelectableLabel(text, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
		} EditorGUILayout.EndHorizontal();
	}
}
