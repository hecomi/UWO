using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Notification : MonoBehaviour 
{
	public static Notification Instance;
	public Text textUi;
	private Animator animator_;
	private float duration = 3f;
	private float time_ = 0f;
	private bool isHelpShowing_ = false;

	void Awake()
	{
		Instance = this;
		animator_ = GetComponent<Animator>();
	}

	void Start()
	{
		StartCoroutine(ShowHelp());
	}

	IEnumerator ShowHelp()
	{
		yield return new WaitForEndOfFrame();
		if (!isHelpShowing_) {
			isHelpShowing_ = true;
			Notification.Show("WASD: Move   QE: Rotate   Space: Jump   MouseDrag: Roatete Camera", 8f);
			yield return new WaitForSeconds(8f);
			Notification.Show("1: Fire Bullet   2: Create Block   3: Delete Block   4: Change Block Color", 8f);
			yield return new WaitForSeconds(8f);
			Notification.Show("O: Toggle Camera Effect   L: Toggle Light   P: Screenshot", 8f);
			yield return new WaitForSeconds(8f);
			Notification.Show("You can show this message by '?' key again. Please Enjoy!!");
			isHelpShowing_ = false;
		}
	}

	void Update()
	{
		time_ += Time.deltaTime;
		if (time_ > duration) {
			Hide();
		}

		if (Input.GetKeyDown(KeyCode.Question)) {
			StartCoroutine(ShowHelp());
		}
	}

	void ShowImpl(string text, float notificationDuration)
	{
		duration = notificationDuration;
		animator_.SetBool("IsShown", true);
		textUi.text = text;
		time_ = 0f;
		SoundManager.Play("Notification", Vector3.zero);
	}

	void HideImpl()
	{
		animator_.SetBool("IsShown", false);
	}

	public static void Show(string text, float notificationDuration = 3f)
	{
		Instance.ShowImpl(text, notificationDuration);
	}

	public static void Hide()
	{
		Instance.HideImpl();
	}
}
