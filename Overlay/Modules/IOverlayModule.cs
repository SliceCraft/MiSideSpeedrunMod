using System;
using SpeedrunMod.Overlay.Snapshots;

namespace SpeedrunMod.Overlay.Modules;

internal interface IOverlayModule
{
	string Name { get; }

	string GroupKey { get; }

	TimeSpan UpdateInterval { get; }

	void Reset();

	IOverlaySnapshot Update();
}
