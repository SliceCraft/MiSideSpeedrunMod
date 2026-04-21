using SpeedrunMod.Notifications;

namespace SpeedrunMod.Toggles;

internal static class EnableRunToggle
{
    internal static void Update()
    {
        if (!UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt) || !UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.L)) return;
        PlayerMove playerMove = UnityEngine.Object.FindObjectOfType<PlayerMove>();
        if (!playerMove)
        {
            Plugin.Log.LogError("Unable to get a PlayerMove");
        }

        if(playerMove.needRun)
        {
            Plugin.Log.LogInfo("Disabling running");
            NotificationManager.Show(new NotificationMessage("Running disabled"));
        }
        else
        {
            Plugin.Log.LogInfo("Enabling running");
            NotificationManager.Show(new NotificationMessage("Running enabled"));
        }
        playerMove.RunNeed(!playerMove.needRun);
    }
}