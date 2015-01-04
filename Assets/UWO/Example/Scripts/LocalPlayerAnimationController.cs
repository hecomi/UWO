using UnityEngine;
using System.Collections;

public class LocalPlayerAnimationController : MonoBehaviour 
{
	public KeyCode toggleKey;
	public bool isAnimation = true;
	private Animator animator_;
	public Animator animator
	{
		get { return animator_ ?? (animator_ = GetComponent<Animator>()); }
	}
	private Canvas canvas_;
	public Canvas canvas
	{
		get { return canvas_ ?? (canvas_ = GetComponent<Canvas>()); }
	}

	void Update()
	{
		if (Input.GetKeyDown(toggleKey) && !GlobalState.isInputting) {
			if (isAnimation) {
				animator.SetBool("IsAppear", !animator.GetBool("IsAppear"));
			} else {
				canvas.enabled = !canvas.enabled;
			}
		}
	}
}
