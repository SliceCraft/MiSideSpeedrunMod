namespace SpeedrunMod.Notifications;

internal static class SoftlockFixNotifications
{
    internal const string GhostlyPuzzle = "Softlock Fix: Ghostly puzzle";
    internal const string GhostlyChapterLoad = "Softlock Fix: Ghostly chapter load";
    internal const string SleepyDialogue = "Softlock Fix: Sleepy dialogue";
    internal const string CoreThrow = "Softlock Fix: Core throw";
    internal const string CreepyDialogue = "Softlock Fix: Creepy dialogue";
    internal const string BaseballBat = "Softlock Fix: Baseball bat";
    internal const string KindBedroomPaper = "Softlock Fix: Kind bedroom paper";
    internal const string KappiRing = "Softlock Fix: Kappi ring";

    internal static void Show(string text) =>
        NotificationManager.Show(new NotificationMessage(text));
}
