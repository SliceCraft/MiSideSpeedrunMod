using UnityEngine;
using SpeedrunMod.Debug.Models;

namespace SpeedrunMod.Debug;

internal sealed class MovementOverlayModule : IDebugOverlayModule
{
	internal static readonly MovementOverlayModule Instance = new MovementOverlayModule();

	private const float MinDeltaTime = 1E-05f;

	private Vector3? _anchorPos;
	private float _anchorTime;
	private float _lastSpeed;
	private float _lastBodySpeed;
	private float _maxSpeed;
	private float _maxBodySpeed;
	private float _maxAbsTransformAccel;
	private float _maxAbsBodyAccel;

	public string Name => "Movement";

	public void Reset()
	{
		ClearMovementState();
	}

	private void ClearMovementState()
	{
		_anchorPos = null;
		_anchorTime = 0f;
		_lastSpeed = 0f;
		_lastBodySpeed = 0f;
		_maxSpeed = 0f;
		_maxBodySpeed = 0f;
		_maxAbsTransformAccel = 0f;
		_maxAbsBodyAccel = 0f;
	}

	public IDebugOverlaySnapshot Update(in DebugOverlayContext ctx)
	{
		int frameCount = ctx.Time.FrameCount;
		if (ctx.PlayerMove == null)
		{
			ClearMovementState();
			return MovementOverlaySnapshot.Empty(frameCount);
		}

		var playerMove = ctx.PlayerMove;
		var transform = playerMove.transform;
		var position = transform.position;
		var realtimeSinceStartup = ctx.Time.RealtimeSinceStartup;
		var name = transform.name;
		
		var body = playerMove.GetComponent<Rigidbody>();
		var bodyVelocity = body?.velocity ?? Vector3.zero;
		var dt = realtimeSinceStartup - _anchorTime;
		
		if (_anchorPos == null || dt < MinDeltaTime)
		{
			_anchorPos = position;
			_anchorTime = realtimeSinceStartup;

			return new MovementOverlaySnapshot(
				position,
				name, 
				0f, 
				Vector3.zero, 
				0f, 
				bodyVelocity, 
				0f,
				0f,
				0f,
				_maxSpeed, 
				_maxBodySpeed, 
				_maxAbsTransformAccel, 
				_maxAbsBodyAccel,
				frameCount);
		}

		var dpos = position - _anchorPos!.Value;
		var transformSpeed = dpos.magnitude / dt;
		var bodySpeed = bodyVelocity.magnitude / dt;
		var transformAccel = (transformSpeed - _lastSpeed) / dt;
		var bodyAccel = (bodySpeed - _lastBodySpeed) / dt;

		_maxSpeed = Mathf.Max(_maxSpeed, transformSpeed);
		_maxBodySpeed = Mathf.Max(_maxBodySpeed, bodySpeed);
		_maxAbsTransformAccel = Mathf.Max(_maxAbsTransformAccel, transformAccel);
		_maxAbsBodyAccel = Mathf.Max(_maxAbsBodyAccel, bodyAccel);

		var movementOverlaySnapshot = new MovementOverlaySnapshot(
			position,
			name,
			dt,
			dpos,
			transformSpeed,
			bodyVelocity,
			bodySpeed,
			transformAccel,
			bodyAccel,
			_maxSpeed,
			_maxBodySpeed,
			_maxAbsTransformAccel,
			_maxAbsBodyAccel,
			frameCount);
		
		_lastSpeed = transformSpeed;
		_lastBodySpeed = bodySpeed;
		_anchorPos = position;
		_anchorTime = realtimeSinceStartup;

		return movementOverlaySnapshot;
	}

	IDebugOverlaySnapshot IDebugOverlayModule.Update(in DebugOverlayContext ctx)
	{
		return Update(in ctx);
	}
}
