using UnityEngine;
using UWO;

public class TransformSync : SynchronizedComponent
{
	private bool isFirst_ = true;
	private Vector3 syncStartPos_, syncEndPos_;
	private Quaternion syncStartRot_, syncEndRot_;
	private Vector3 syncStartScale_, syncEndScale_;
	private float syncStartTime_, syncEndTime_;

	public bool isPositionSync = true;
	public bool isRotationSync = true;
	public bool isScaleSync    = false;

	protected override void OnSend()
	{
		var values = new MultiValue();

		if (isPositionSync) values.Push(transform.position);
		if (isRotationSync) values.Push(transform.rotation);
		if (isScaleSync)    values.Push(transform.localScale);

		Send(values);
	}

	protected override void OnReceive(MultiValue values)
	{
		if (isPositionSync) {
			syncStartPos_  = transform.position;
			syncEndPos_    = values.PopValue().AsVector3();
			if (isFirst_) transform.position = syncEndPos_;
		}

		if (isRotationSync) {
			syncStartRot_  = transform.rotation;
			syncEndRot_    = values.PopValue().AsQuaternion();
			if (isFirst_) transform.rotation = syncEndRot_;
		}

		if (isScaleSync) {
			syncStartScale_  = transform.localScale;
			syncEndScale_    = values.PopValue().AsVector3();
			if (isFirst_) transform.localScale = syncEndScale_;
		}

		syncStartTime_ = Time.time;
		syncEndTime_   = syncStartTime_ + sendCycle;

		if (isFirst_) isFirst_ = false;
	}

	protected override void OnRemoteUpdate()
	{
		if (isFirst_) return;

		float t = (Time.time - syncStartTime_) / (syncEndTime_ - syncStartTime_);

		if (isPositionSync) transform.position   = Vector3.Lerp(syncStartPos_, syncEndPos_, t);
		if (isRotationSync) transform.rotation   = Quaternion.Lerp(syncStartRot_, syncEndRot_, t);
		if (isScaleSync)    transform.localScale = Vector3.Lerp(syncStartScale_, syncEndScale_, t);
	}
}
