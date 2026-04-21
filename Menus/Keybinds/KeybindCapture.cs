using System;
using System.Linq;
using UnityEngine;

namespace SpeedrunMod.Menus.Keybinds;

internal static class KeybindCapture
{
    private static bool _isCapturing;
    private static Action<bool, KeyCode> _onComplete;
    private static readonly KeyCode[] BindableKeys = BuildBindableKeys();

    internal static bool IsCapturing()
    {
        return _isCapturing;
    }

    internal static void BeginCapture(Action<bool, KeyCode> onComplete)
    {
        _onComplete = onComplete;
        _isCapturing = true;
    }

    internal static void CancelCapture()
    {
        if (!_isCapturing) return;
        _onComplete?.Invoke(false, KeyCode.None);
        EndCapture();
    }

    internal static void Update()
    {
        if (!_isCapturing) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCapture();
            return;
        }

        foreach (KeyCode keyCode in BindableKeys)
        {
            if (!Input.GetKeyDown(keyCode)) continue;
            _onComplete?.Invoke(true, keyCode);
            EndCapture();
            return;
        }
    }

    private static void EndCapture()
    {
        _isCapturing = false;
        _onComplete = null;
    }

    private static KeyCode[] BuildBindableKeys()
    {
        return ((KeyCode[])Enum.GetValues(typeof(KeyCode)))
            .Where(IsBindableKey)
            .ToArray();
    }

    private static bool IsBindableKey(KeyCode keyCode)
    {
        if (keyCode == KeyCode.None) return false;
        string key = keyCode.ToString();
        if (key.StartsWith("Mouse")) return false;
        if (key.StartsWith("Joystick")) return false;
        return true;
    }
}
