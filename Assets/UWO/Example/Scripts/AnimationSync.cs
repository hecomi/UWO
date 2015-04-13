using UnityEngine;
using System.Collections;
using UWO;

public class AnimationSync : SynchronizedComponent
{
	private Animator animator_;
	public Animator animator
	{
		get { return animator_ ?? (animator_ = GetComponent<Animator>()); }
	}

	protected override void OnSend()
	{
		var values = new MultiValue();

		foreach (var param in animator.parameters) {
			values.Push(param.name);
			switch (param.type) {
				case AnimatorControllerParameterType.Bool:
					values.Push(animator.GetBool(param.name));
					break;
				case AnimatorControllerParameterType.Int:
					values.Push(animator.GetInteger(param.name));
					break;
				case AnimatorControllerParameterType.Float:
					values.Push(animator.GetFloat(param.name));
					break;
				default:
					Debug.LogWarning("No supported format: " + param.type);
					break;
			}
		}

		Send(values);
	}

	protected override void OnReceive(MultiValue data)
	{
		while (data.values.Count > 0) {
			var name  = data.PopValue();
			var param = data.Pop();
			switch (param.type) {
				case "bool":
					animator.SetBool(name, param.value.AsBool());
					break;
				case "int":
					animator.SetInteger(name, param.value.AsInt());
					break;
				case "float":
					animator.SetFloat(name, param.value.AsFloat());
					break;
			}
		}
	}
}
