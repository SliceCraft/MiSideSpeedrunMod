using System.Text;
using SpeedrunMod.Overlay.Snapshots;
using UnityEngine;

namespace SpeedrunMod.Overlay.Modules.Movement;

internal readonly struct MovementOverlaySnapshot : IOverlaySnapshot
{
	private readonly Vector3 _position;
	private readonly string _targetName;
	private readonly float _sampleIntervalSeconds;
	private readonly Vector3 _dpos;
	private readonly float _transformSpeed;
	private readonly Vector3 _bodyVelocity;
	private readonly float _bodySpeed;
	private readonly float _transformAccel;
	private readonly float _bodyAccel;
	private readonly float _maxSpeed;
	private readonly float _maxBodySpeed;
	private readonly float _maxTransformAccel;
	private readonly float _maxBodyAccel;
	private readonly int _frame;

	internal MovementOverlaySnapshot(
		Vector3 position,
		string targetName,
		float sampleIntervalSeconds,
		Vector3 dpos,
		float transformSpeed,
		Vector3 bodyVelocity,
		float bodySpeed,
		float transformAccel,
		float bodyAccel,
		float maxSpeed,
		float maxBodySpeed,
		float maxTransformAccel,
		float maxBodyAccel,
		int frame)
	{
		_position = position;
		_targetName = targetName;
		_sampleIntervalSeconds = sampleIntervalSeconds;
		_dpos = dpos;
		_transformSpeed = transformSpeed;
		_bodyVelocity = bodyVelocity;
		_bodySpeed = bodySpeed;
		_transformAccel = transformAccel;
		_bodyAccel = bodyAccel;
		_maxSpeed = maxSpeed;
		_maxBodySpeed = maxBodySpeed;
		_maxTransformAccel = maxTransformAccel;
		_maxBodyAccel = maxBodyAccel;
		_frame = frame;
	}

	internal static MovementOverlaySnapshot Empty(int frame)
	{
		return new MovementOverlaySnapshot(
			Vector3.zero,
			null,
			0f,
			Vector3.zero,
			0f,
			Vector3.zero,
			0f,
			0f,
			0f,
			0f,
			0f,
			0f,
			0f,
			frame);
	}

	public string Format()
	{
		var value = string.IsNullOrEmpty(_targetName) ? "?" : _targetName;
		var text = new StringBuilder();

		text.AppendLine($"Target:\t{value}");
		text.AppendLine($"Frame:\t{_frame}");
		text.AppendLine($"Position:\t{_position.x:F3}, {_position.y:F3}, {_position.z:F3}");
		text.AppendLine($"Sample interval:\t{_sampleIntervalSeconds:F4} seconds");
		text.AppendLine($"dpos:\t{_dpos.x:F4}, {_dpos.y:F4}, {_dpos.z:F4}");
		
		text.AppendLine($"Transform speed:\t{_transformSpeed:F3} u/s");
		text.AppendLine($"Transform accel:\t{_transformAccel:F3} u/s2");
		text.AppendLine($"Max transform speed:\t{_maxSpeed:F3} u/s");
		text.AppendLine($"Max transform accel:\t{_maxTransformAccel:F3} u/s2");

		text.AppendLine($"Body velocity:\t{_bodyVelocity.x:F3}, {_bodyVelocity.y:F3}, {_bodyVelocity.z:F3} u/s");
		text.AppendLine($"Body speed:\t{_bodySpeed:F3} u/s");
		text.AppendLine($"Body accel:\t{_bodyAccel:F3} u/s2");
		text.AppendLine($"Max body speed:\t{_maxBodySpeed:F3} u/s");
		text.Append($"Max body accel:\t{_maxBodyAccel:F3} u/s2");

		return text.ToString();
	}

	public override string ToString()
	{
		return Format();
	}
}
