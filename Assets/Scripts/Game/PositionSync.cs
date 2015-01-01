using UnityEngine;

public class PositionSync : SynchronizedComponent
{
	private Vector3 to_;
	private bool isFirst_ = true;

	protected override void OnSend()
	{
		Send(transform.position);
	}

	protected override void OnReceive(Vector3 value)
	{
		if (isFirst_) {
			transform.position = to_ = value;
			isFirst_ = false;
		} else {
			to_ = value;
		}
	}

	protected override void OnRemoteUpdate()
	{
		transform.position += (to_ - transform.position) * easing;
	}
}
