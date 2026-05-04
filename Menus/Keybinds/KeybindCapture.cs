using System;
using System.Linq;
using UnityEngine;

namespace SpeedrunMod.Menus.Keybinds;

internal static class KeybindCapture
{
    private static string _capturingContext;
    private static Action<bool, KeyCode> _onComplete;
    private static readonly KeyCode[] BindableKeys = BuildBindableKeys();

    internal static bool IsCapturing()
    {
        return _capturingContext != null;
    }

    internal static bool IsCapturing(string context)
    {
        return _capturingContext?.Equals(context) ?? false;
    }

    internal static void BeginCapture(string context, Action<bool, KeyCode> onComplete)
    {
        _onComplete = onComplete;
        _capturingContext = context;
    }

    internal static void CancelCapture()
    {
        _onComplete?.Invoke(false, KeyCode.None);
        EndCapture();
    }

    internal static bool CancelCapture(string context)
    {
        if (_capturingContext != context) return false;
        CancelCapture();
        return true;
    }

    internal static void Update()
    {
        if (_capturingContext == null) return;

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
        _capturingContext = null;
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
