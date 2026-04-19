using System.Linq;
using System.Collections.Generic;
using SpeedrunMod.Overlay.Modules.Display;
using SpeedrunMod.Overlay.Modules.Movement;
using SpeedrunMod.Overlay.Modules.Persistent;

namespace SpeedrunMod.Overlay.Modules;

internal class OverlayModulesRepository
{
    internal static readonly OverlayModulesRepository Repository = new OverlayModulesRepository();

    private readonly IOverlayModule[] _modules;
    private readonly IOverlayModule[] _persistentModules;
    private readonly Dictionary<string, IOverlayModule> _moduleByName;

    private OverlayModulesRepository()
    {
        _modules =
        [
            MovementOverlayModule.Instance,
            DisplayOverlayModule.Instance,
            RefreshRateWarningOverlayModule.Instance
        ];

        _persistentModules = _modules
            .Where(module => module is IPersistentOverlayModule)
            .ToArray();

        _moduleByName = _modules.ToDictionary(module => module.Name);
    }

    internal bool IsAnyPersistent => _persistentModules.Length > 0;

    internal IOverlayModule GetByName(string name)
    {
        return _moduleByName.GetValueOrDefault(name);
    }

    internal IOverlayModule[] GetAll()
    {
        return _modules;
    }

    internal IOverlayModule[] GetPersistent()
    {
        return _persistentModules;
    }
}