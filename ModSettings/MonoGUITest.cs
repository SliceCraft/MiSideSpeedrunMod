using UnityEngine;

namespace SpeedrunMod.ModSettings;

// This class is a test for using the IMGUI features for displaying a settings menu
// It appears that this is very broken though, so this is not used for now
// I'm keeping this here just in case this will be used in the future
// Maybe in the future it can get deleted
public class MonoGUITest : MonoBehaviour
{
    private bool _showingMenu;
    private MenuType _menuType = MenuType.MainMenu;

    private enum MenuType
    {
        MainMenu,
        MinigamesList,
        CursorList
    }
    
    public void ToggleMenu()
    {
        _showingMenu = !_showingMenu;
        _menuType = MenuType.MainMenu;
    }

    public void OnGUI()
    {
        // Make a background box
        GUI.Box(new Rect(10,10,100,90), "Loader Menu");
    
        // Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
        if(GUI.Button(new Rect(20,40,80,20), "Level 1"))
        {
            Plugin.Log.LogInfo("Hi 1");
        }
    
        // Make the second button.
        if(GUI.Button(new Rect(20,70,80,20), "Level 2")) 
        {
            Plugin.Log.LogInfo("Hi 2");
        }
    }
    
    public void aOnGUI()
    {
        if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
        {
            Plugin.Log.LogInfo(Event.current.type);
            Plugin.Log.LogWarning($"Current Menu Type: {_menuType}");
        }
        if (_showingMenu)
        {
            switch (_menuType)
            {
                case MenuType.MainMenu:
                default:
                    RenderMainMenu();
                    break;
                case MenuType.MinigamesList:
                    RenderMiniGamesList();
                    break;
                case MenuType.CursorList:
                    RenderCursorList();
                    break;
            }
        }

        if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
        {
            Plugin.Log.LogWarning($"Current Menu Type: {_menuType}");
        }
    }

    private void RenderMainMenu()
    {
        GUI.Box(new Rect(10,10,300,90), $"Slice's Speedrun Mod Settings : V{MyPluginInfo.PLUGIN_VERSION}");
        
        if(GUI.Button(new Rect(40,40,220,20), "Play Minigames"))
        {
            Plugin.Log.LogInfo("Changing Minigames");
            _menuType = MenuType.MinigamesList;
        }
        
        if(GUI.Button(new Rect(40,700,220,20), "Custom Cursors")) 
        {
            Plugin.Log.LogInfo("Changing Custom Cursors");
            _menuType = MenuType.CursorList;
            Plugin.Log.LogInfo(_menuType.ToString());
        }
    }
    
    private void RenderMiniGamesList()
    {
        GUI.Box(new Rect(10,10,300,90), $"Minigames List");
        
        if(GUI.Button(new Rect(40,40,220,20), "Back to Main Menu"))
        {
            _menuType = MenuType.MainMenu;
        }
        
        if(GUI.Button(new Rect(40,70,220,20), "Soon TM")) 
        {
            // TODO: Actually implement minigames
            Plugin.Log.LogInfo("TODO Minigame button pressed");
        }
    }

    private void RenderCursorList()
    {
        GUI.Box(new Rect(10,10,300,90), $"Custom cursors");
        
        if(GUI.Button(new Rect(40,40,220,20), "Back to Main Menu"))
        {
            _menuType = MenuType.MainMenu;
        }
        
        if(GUI.Button(new Rect(40,70,220,20), "Soon TM")) 
        {
            // TODO: Actually implement custom cursors
            Plugin.Log.LogInfo("TODO Cursor button pressed");
        }
    }
}