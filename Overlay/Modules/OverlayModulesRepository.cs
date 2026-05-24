using System.Linq;
using System.Collections.Generic;
using SpeedrunMod.Overlay.Modules.Display;
using SpeedrunMod.Overlay.Modules.Movement;
using SpeedrunMod.Overlay.Modules.Persistent;

namespace SpeedrunMod.Overlay.Modules;

internal class OverlayModulesRepository
{
    internal static readonly OverlayModulesRepository Repository = new();

    private readonly IOverlayModule[] _modules;
    private readonly IOverlayModule[] _persistentModules;
    private readonly Dictionary<string, IOverlayModule> _moduleByName;
    private readonly SortedDictionary<string, IOverlayModule[]> _modulesByGroup;

    private OverlayModulesRepository()
    {
        _modules =
        [
            MovementOverlayModule.Instance,
            DisplayOverlayModule.Instance,
            RefreshRateWarningOverlayModule.Instance
        ];

        _persistentModules = _modules
            .Where(m => m is IPersistentOverlayModule)
            .ToArray();

        _moduleByName = _modules.ToDictionary(m => m.Name);

        _modulesByGroup = new SortedDictionary<string, IOverlayModule[]>();
        var groupModules = _modules
            .Where(m => m is not IPersistentOverlayModule)
            .GroupBy(m => m.GroupKey);

        foreach (var group in groupModules)
        {
            _modulesByGroup[group.Key] = group.ToArray();
        }
    }

    internal bool IsAnyPersistent => _persistentModules.Length > 0;
    
    internal int GroupCount => _modulesByGroup.Count;

    internal IOverlayModule GetByName(string name)
    {
        return _moduleByName.GetValueOrDefault(name);
    }
    
    internal IOverlayModule[] GetAll() => _modules;

    internal IOverlayModule[] GetPersistent() => _persistentModules;

    internal IOverlayModule[] GetByGroupKey(string key)
    {
        return _modulesByGroup.GetValueOrDefault(key) ?? [];
    }

    internal KeyValuePair<string, IOverlayModule[]> GetByGroupIndex(int index)
    {
        return _modulesByGroup.ElementAtOrDefault(index);
    }
}