using System;
using SpeedrunMod.Configs;
using SpeedrunMod.Overlay.Snapshots;
using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Overlay.Modules.Movement;

internal sealed class MovementOverlayModule : IOverlayModule
{
	internal static readonly MovementOverlayModule Instance = new();

	private const float MinDeltaTime = 1E-05f;

	private Vector3? _anchorPos;
	private float _anchorTime;
	private float _lastSpeed;
	private float _lastBodySpeed;
	private float _maxSpeed;
	private float _maxBodySpeed;
	private float _maxTransformAccel;
	private float _maxBodyAccel;

	public string Name => "Movement";

	public string GroupKey => "Core";

	public TimeSpan UpdateInterval { get; } = TimeSpan.FromSeconds(Math.Max(0f, OverlayConfig.OverlayLogInterval.Value));

	public void Reset()
	{
		_anchorPos = null;
		_anchorTime = 0f;
		_lastSpeed = 0f;
		_lastBodySpeed = 0f;
		_maxSpeed = 0f;
		_maxBodySpeed = 0f;
		_maxTransformAccel = 0f;
		_maxBodyAccel = 0f;
	}

	public IOverlaySnapshot Update()
	{
		var playerMove = ResolvePlayerMove();

		if (playerMove == null)
		{
			Reset();
			return MovementOverlaySnapshot.Empty();
		}

		var transform = playerMove.transform;
		var position = transform.position;
		var realtimeSinceStartup = Time.realtimeSinceStartup;
		var name = transform.name;
		
		var body = playerMove.GetComponent<Rigidbody>();
		var bodyVelocity = body?.velocity ?? Vector3.zero;
		var sampleIntervalSeconds = realtimeSinceStartup - _anchorTime;
		
		if (_anchorPos == null || sampleIntervalSeconds < MinDeltaTime)
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
				_maxTransformAccel, 
				_maxBodyAccel);
		}

		var dpos = position - _anchorPos!.Value;
		var transformSpeed = dpos.magnitude / sampleIntervalSeconds;
		var bodySpeed = bodyVelocity.magnitude;
		var transformAccel = (transformSpeed - _lastSpeed) / sampleIntervalSeconds;
		var bodyAccel = (bodySpeed - _lastBodySpeed) / sampleIntervalSeconds;

		_maxSpeed = Mathf.Max(_maxSpeed, transformSpeed);
		_maxBodySpeed = Mathf.Max(_maxBodySpeed, bodySpeed);
		_maxTransformAccel = Mathf.Max(_maxTransformAccel, transformAccel);
		_maxBodyAccel = Mathf.Max(_maxBodyAccel, bodyAccel);

		var movementOverlaySnapshot = new MovementOverlaySnapshot(
			position,
			name,
			sampleIntervalSeconds,
			dpos,
			transformSpeed,
			bodyVelocity,
			bodySpeed,
			transformAccel,
			bodyAccel,
			_maxSpeed,
			_maxBodySpeed,
			_maxTransformAccel,
			_maxBodyAccel);
		
		_lastSpeed = transformSpeed;
		_lastBodySpeed = bodySpeed;
		_anchorPos = position;
		_anchorTime = realtimeSinceStartup;

		return movementOverlaySnapshot;
	}

	private static PlayerMove ResolvePlayerMove()
	{
		var playerMove = UnityEngine.Object.FindObjectOfType<PlayerMove>();
		if (playerMove != null)
		{
			return playerMove;
		}

		var controller = GameUtil.GetGameController();
		if (controller == null)
		{
			return null;
		}

		var player = controller.transform.Find("Player");
		return player?.GetComponent<PlayerMove>();
	}
}
