using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpeedrunMod.Utils;

namespace SpeedrunMod.Notifications;

internal static class NotificationManager
{
    private static GameObject _hintScreenTemplate;

    private static GameObject _interfaceObject;

    private static float? _bottomMargin;
    private static float? _rightMargin;
    
    private static readonly List<TimedNotification> TimedNotifications = 
    [
        new RefreshRateNotification()
    ];

    private static readonly List<NotificationMessage> NotificationObjects = [];
    
    private static readonly int Hide = Animator.StringToHash("Hide");

    public static bool Show(NotificationMessage notificationMessage)
    {
        if (!EnsureObjectsSelected())
        {
            return false;
        }

        GameObject go = Object.Instantiate(_hintScreenTemplate);

        if (go == null)
        {
            Plugin.Log.LogWarning("Tried instantiating hint screen template but was unable to find it.");
            return false;
        }

        go.transform.SetParent(_interfaceObject.transform, false);

        Text text = go.GetComponentInChildren<Text>();

        if (text == null)
        {
            Plugin.Log.LogWarning("Tried getting text component but was unable to find it.");
            return false;
        }

        text.text = notificationMessage.Text;

        notificationMessage.HintObject = go;
        NotificationObjects.Add(notificationMessage);
        UpdatePositions();

        go.SetActive(true);
        return true;
    }

    private static void UpdatePositions()
    {
        NotificationObjects.RemoveAll(static n => n.HintObject == null);

        for (int i = 0; i < NotificationObjects.Count; i++)
        {
            GameObject go = NotificationObjects[i].HintObject;
            if (go == null)
            {
                continue;
            }

            if (go.TryGetComponent(out RectTransform rectTransform))
            {
                var bottomMargin = _bottomMargin ?? 0f;
                var rightMargin = _rightMargin ?? 0f;
                rectTransform.anchorMin = new Vector2(1f, 0f);
                rectTransform.anchorMax = new Vector2(1f, 0f);
                rectTransform.pivot = new Vector2(1f, 0f);
                rectTransform.anchoredPosition = new Vector2(rightMargin, i * 100f + bottomMargin);
            }
        }
    }

    internal static void Update()
    {
        if (!EnsureObjectsSelected())
        {
            return;
        }

        UpdateTimedNotifications();
        ShowNotifications();
    }

    private static void UpdateTimedNotifications()
    {
        foreach (var timedNotification in TimedNotifications)
        {
            timedNotification.Update();
        }
    }

    private static void ShowNotifications()
    {
        List<NotificationMessage> objectsToBeRemoved = [];

        foreach (NotificationMessage notificationMessage in NotificationObjects)
        {
            if (notificationMessage.HintObject == null)
            {
                objectsToBeRemoved.Add(notificationMessage);
                continue;
            }

            notificationMessage.TimeUntilHide -= Time.deltaTime;
            notificationMessage.TimeUntilDestroy -= Time.deltaTime;

            if (notificationMessage.TimeUntilHide <= 0)
            {
                notificationMessage.HintObject.GetComponent<Animator>().SetBool(Hide, true);
                // Prevent the hide animation from playing again
                notificationMessage.TimeUntilHide = 1e10f;
            }

            if (notificationMessage.TimeUntilDestroy <= 0)
            {
                objectsToBeRemoved.Add(notificationMessage);
            }
        }

        foreach (var notificationMessage in objectsToBeRemoved)
        {
            NotificationObjects.Remove(notificationMessage);
            Object.Destroy(notificationMessage.HintObject);
        }

        if (objectsToBeRemoved.Count > 0)
        {
            UpdatePositions();
        }
    }

    private static bool EnsureObjectsSelected()
    {
        if (_hintScreenTemplate != null && _interfaceObject != null)
        {
            return true;
        }

        var gc = GameUtil.GetGameController();
        if (!gc)
        {
            Plugin.Log.LogWarning(
                "Tried finding hint screen but a GameController couldn't be found.",
                context: $"{nameof(NotificationManager)}.EnsureGameControllerObject",
                throttleSeconds: 30);
            
            return false;
        }

        GameObject interfaceObject = null;
        for (int i = 0; i < gc.transform.childCount; i++)
        {
            GameObject go = gc.transform.GetChild(i).gameObject;
            if (go.name == "Interface")
            {
                interfaceObject = go;
            }
        }

        if (interfaceObject == null)
        {
            Plugin.Log.LogWarning(
                "Tried finding hint screen but an interface couldn't be found in the GameController.",
                context: $"{nameof(NotificationManager)}.EnsureInterfaceObject",
                throttleSeconds: 30);
            
            return false;
        }

        GameObject hintScreenObject = null;
        for (int i = 0; i < interfaceObject.transform.childCount; i++)
        {
            GameObject go = interfaceObject.transform.GetChild(i).gameObject;
            if (go.name == "HintScreen")
            {
                hintScreenObject = go;
            }
        }

        if (hintScreenObject == null)
        {
            Plugin.Log.LogWarning(
                "Tried finding hint screen but was unable to find it in the GameController.",
                context: $"{nameof(NotificationManager)}.EnsureHintScreenObject",
                throttleSeconds: 30);
            
            return false;
        }

        _interfaceObject = interfaceObject;
        _hintScreenTemplate = hintScreenObject;

        if (_bottomMargin != null && _rightMargin != null)
        {
            return true;
        }

        if (_hintScreenTemplate.TryGetComponent(out RectTransform templateRect))
        {
            _bottomMargin = -templateRect.anchoredPosition.y;
            _rightMargin = -templateRect.anchoredPosition.x;
        }

        return true;
    }
}