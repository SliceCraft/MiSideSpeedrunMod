using SpeedrunMod.Overlay.Snapshots;

namespace SpeedrunMod.Overlay.Modules;

internal sealed class ModuleState
{
	internal string ModuleName;

	internal IOverlaySnapshot CachedSnapshot;

	internal float LastUpdatedRealtime = float.NegativeInfinity;

	internal void Clear()
	{
		CachedSnapshot = null;
		LastUpdatedRealtime = float.NegativeInfinity;
	}
}
