using UnityEngine;

public class RotationSync : SynchronizedComponent
{
	private Quaternion to_;
	private bool isFirst_ = true;

	protected override void OnSend()
	{
		Send(transform.rotation);
	}

	protected override void OnReceive(Quaternion value)
	{
		if (isFirst_) {
			transform.rotation = to_ = value;
			isFirst_ = false;
		} else {
			to_ = value;
		}
	}

	protected override void OnRemoteUpdate()
	{
		transform.rotation = Quaternion.Lerp(transform.rotation, to_, easing);
	}
}