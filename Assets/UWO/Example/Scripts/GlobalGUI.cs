using UnityEngine;
using UnityEngine.UI;

public class GlobalGUI : MonoBehaviour 
{
	private static GlobalGUI Instance_;
	public static GlobalGUI Instance
	{
		get { return Instance_; }
		private set { Instance_ = value; }
	}

	public Text num;
	public Text tool;

	void Awake()
	{
		Instance = this;
	}

	public static void SetNum(string numText)
	{
		if (!Instance) return;
		Instance.num.text = numText;
	}

	public static void SetTool(string toolText)
	{
		if (!Instance) return;
		Instance.tool.text = toolText;
	}
}
