namespace SpeedrunMod.Toggles
{
    internal class EnableRunToggle
    {
        public static void Update()
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
            }
            else
            {
                Plugin.Log.LogInfo("Enabling running");
            }
            playerMove.RunNeed(!playerMove.needRun);
        }
    }
}
