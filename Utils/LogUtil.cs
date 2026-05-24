using System.Collections.Concurrent;
using UnityEngine;

namespace SpeedrunMod.Utils;

public static class LogUtil
{
    private static ConcurrentDictionary<string, float> _lastLogTimes = new();

    public static void LogInfo(
        this BepInEx.Logging.ManualLogSource source, 
        string message, 
        string context,
        float throttleSeconds = 0f)
    => Log(source, BepInEx.Logging.LogLevel.Info, message, context, throttleSeconds);

    public static void LogWarning(
        this BepInEx.Logging.ManualLogSource source,
        string message,
        string context,
        float throttleSeconds = 0f)
    => Log(source, BepInEx.Logging.LogLevel.Warning, message, context, throttleSeconds);

    public static void LogDebug(
        this BepInEx.Logging.ManualLogSource source,
        string message,
        string context,
        float throttleSeconds = 0f)
    => Log(source, BepInEx.Logging.LogLevel.Debug, message, context, throttleSeconds);

    public static void LogError(
        this BepInEx.Logging.ManualLogSource source,
        string message,
        string context,
        float throttleSeconds = 0f)
    => Log(source, BepInEx.Logging.LogLevel.Error, message, context, throttleSeconds);

    public static void Log(
        this BepInEx.Logging.ManualLogSource source,
        BepInEx.Logging.LogLevel level,
        string message,
        string context,
        float throttleSeconds = 0f)
    {
        var now = Time.realtimeSinceStartup;
        var timePassed = now - _lastLogTimes.GetOrAdd(context, 0);

        if (throttleSeconds > 0f && timePassed < throttleSeconds) return;

        source.Log(level, $"[{context}] {message}");
        _lastLogTimes.AddOrUpdate(context, now, (_, _) => now);
    }
}