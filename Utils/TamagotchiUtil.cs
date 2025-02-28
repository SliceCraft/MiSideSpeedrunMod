using UnityEngine;

namespace SpeedrunMod.Utils;

public static class TamagotchiUtil
{
    private static Tamagotchi_Main StartTamagotchi()
    {
        Tamagotchi_Main tamagotchiMain = Object.FindObjectOfType<Tamagotchi_Main>(true);
        if (tamagotchiMain == null)
        {
            Plugin.Log.LogError("Tamagotchi_Main couldn't be found during TamagotchiUtil#StartTamagotchi()");
            return null;
        }
        tamagotchiMain.gameObject.active = true;
        
        tamagotchiMain.GameStart();
        tamagotchiMain.HideBlackScreen(true);
        tamagotchiMain.showInterface = true;
        
        Tamagotchi_Music tamagotchiMusic = Object.FindObjectOfType<Tamagotchi_Music>();
        if (tamagotchiMusic == null)
        {
            Plugin.Log.LogError("Tamagotchi_Music couldn't be found during TamagotchiUtil#StartTamagotchi(), continuing without music");
        }
        else
        {
            tamagotchiMusic.audioMusicActive = true;
            tamagotchiMusic.effectAudioMobile = false;
        }
        
        return tamagotchiMain;
    }

    public static Tamagotchi_Main StartTamagotchi(int changeRoom)
    {
        Tamagotchi_Main tamagotchiMain = StartTamagotchi(); 
        tamagotchiMain.CameraChangeRoomFast(changeRoom);
        return tamagotchiMain;
    }
}