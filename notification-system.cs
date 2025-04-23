#if UNITY_ANDROID || UNITY_IOS
using Unity.Notifications.Android;
using Unity.Notifications.iOS;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    private static NotificationManager _instance;
    public static NotificationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("NotificationManager instance not found!");
            }
            return _instance;
        }
    }
    
    [Header("Notification Settings")]
    [SerializeField] private string channelId = "chroma_companions_channel";
    [SerializeField] private string channelName = "Chroma Companions";
    [SerializeField] private string channelDescription = "Notifications for Chroma Companions";
    
    [Header("Notification Timing")]
    [SerializeField] private int petHungryReminderHours = 8;
    [SerializeField] private int dailyBonusReminderHours = 24;
    [SerializeField] private int newEventReminderMinutes = 30;
    [SerializeField] private int guildEventReminderHours = 2;
    
    // Message templates
    private Dictionary<NotificationType, string[]> notificationMessages = new Dictionary<NotificationType, string[]>();
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize notification system
        InitializeNotifications();
        
        // Set up notification messages
        SetupNotificationMessages();
    }
    
    private void Start()
    {
        // Subscribe to relevant events
        GameManager.Instance.OnDayChanged += OnDayChanged;
        GameManager.Instance.OnPetAdopted += OnPetAdopted;
        
        if (GuildManager.Instance != null)
        {
            GuildManager.Instance.OnGuildEventCreated += OnGuildEventCreated;
        }
        
        // Request notification permissions
        RequestPermissions();
    }
    
    private void InitializeNotifications()
    {
        #if UNITY_ANDROID
        // Create the notification channel
        var channel = new AndroidNotificationChannel()
        {
            Id = channelId,
            Name = channelName,
            Importance = Importance.Default,
            Description = channelDescription,
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        #endif
        
        #if UNITY_IOS
        // Request authorization to send notifications
        UnityEngine.iOS.NotificationServices.RegisterForNotifications(
            UnityEngine.iOS.NotificationType.Alert | 
            UnityEngine.iOS.NotificationType.Badge | 
            UnityEngine.iOS.NotificationType.Sound);
        #endif
    }
    
    private void RequestPermissions()
    {
        #if UNITY_ANDROID
        // On Android 13+ we need to request post notifications permission
        if (AndroidNotificationCenter.UserPermissionToPost != PermissionStatus.Allowed &&
            AndroidNotificationCenter.UserPermissionToPost != PermissionStatus.NotDetermined)
        {
            StartCoroutine(RequestUserPermission());
        }
        #endif
    }
    
    private IEnumerator RequestUserPermission()
    {
        #if UNITY_ANDROID
        var request = new PermissionRequest();
        request.PermissionType = Permission.PostNotifications;
        yield return request.RequestPermission();
        #else
        yield return null;
        #endif
    }
    
    private void SetupNotificationMessages()
    {
        // Pet hungry messages
        notificationMessages[NotificationType.PetHungry] = new string[]
        {
            "Your pet is getting hungry! Time for a snack!",
            "{0} is feeling hungry. Come feed them!",
            "Rumbling tummies at home! Your pet needs food!"
        };
        
        // Daily bonus messages
        notificationMessages[NotificationType.DailyBonus] = new string[]
        {
            "Your daily bonus is waiting for you!",
            "Free rewards await! Claim your daily bonus now!",
            "Don't miss out on your daily bonus!"
        };
        
        // Pet happiness messages
        notificationMessages[NotificationType.PetUnhappy] = new string[]
        {
            "{0} is feeling sad. They miss you!",
            "Your pet's happiness is dropping. Time to play!",
            "Bring some joy to your pet's day!"
        };
        
        // Guild event messages
        notificationMessages[NotificationType.GuildEvent] = new string[]
        {
            "Guild event starting soon: {0}",
            "Don't forget about the guild event: {0}",
            "Your guild is gathering for: {0}"
        };
        
        // New feature messages
        notificationMessages[NotificationType.NewFeature] = new string[]
        {
            "New feature unlocked: {0}",
            "You can now access: {0}",
            "Exciting new content available: {0}"
        };
    }
    
    // Event handlers
    private void OnDayChanged(int newDay)
    {
        // Schedule pet care reminders
        SchedulePetCareReminders();
        
        // Schedule daily bonus reminder
        ScheduleNotification(
            NotificationType.DailyBonus,
            DateTime.Now.AddHours(dailyBonusReminderHours)
        );
    }
    
    private void OnPetAdopted(PetBase pet)
    {
        // Schedule first feeding reminder
        ScheduleNotification(
            NotificationType.PetHungry,
            DateTime.Now.AddHours(petHungryReminderHours),
            pet.PetName
        );
    }
    
    private void OnGuildEventCreated(Guild guild, GuildEvent guildEvent)
    {
        // Calculate time until event
        TimeSpan timeUntilEvent = guildEvent.eventTime - DateTime.Now;
        
        // Only schedule reminder if event is in the future
        if (timeUntilEvent.TotalHours > guildEventReminderHours)
        {
            DateTime reminderTime = guildEvent.eventTime.AddHours(-guildEventReminderHours);
            
            // Schedule notification
            ScheduleNotification(
                NotificationType.GuildEvent,
                reminderTime,
                guildEvent.title
            );
        }
    }
    
    // Main scheduling methods
    public void ScheduleNotification(NotificationType type, DateTime time, string parameter = null)
    {
        // Get random message for this notification type
        string[] messages = notificationMessages[type];
        string message = messages[UnityEngine.Random.Range(0, messages.Length)];
        
        // Replace parameter if provided
        if (!string.IsNullOrEmpty(parameter))
        {
            message = string.Format(message, parameter);
        }
        
        // Schedule based on platform
        #if UNITY_ANDROID
        ScheduleAndroidNotification(type, message, time);
        #elif UNITY_IOS
        ScheduleIOSNotification(type, message, time);
        #endif
    }
    
    public void SchedulePetCareReminders()
    {
        // This would check each pet's stats and schedule appropriate reminders
        // For simplicity, just schedule generic reminders
        
        foreach (var petEntry in UserData.Instance.ownedPets)
        {
            PetSaveData petData = petEntry.Value;
            
            // If hunger or happiness is low, schedule reminder
            if (petData.stats.hunger < 30f)
            {
                ScheduleNotification(
                    NotificationType.PetHungry,
                    DateTime.Now.AddHours(1),
                    petData.petName
                );
            }
            
            if (petData.stats.happiness < 30f)
            {
                ScheduleNotification(
                    NotificationType.PetUnhappy,
                    DateTime.Now.AddHours(2),
                    petData.petName
                );
            }
        }
    }
    
    // Platform-specific scheduling
    #if UNITY_ANDROID
    private void ScheduleAndroidNotification(NotificationType type, string message, DateTime time)
    {
        var notification = new AndroidNotification();
        notification.Title = "Chroma Companions";
        notification.Text = message;
        notification.FireTime = time;
        notification.SmallIcon = "icon_small";
        notification.LargeIcon = "icon_large";
        
        // Schedule notification
        int id = (int)type * 1000 + UnityEngine.Random.Range(0, 999);
        AndroidNotificationCenter.SendNotification(notification, channelId);
    }
    #endif
    
    #if UNITY_IOS
    private void ScheduleIOSNotification(NotificationType type, string message, DateTime time)
    {
        var notification = new iOSNotification();
        notification.Title = "Chroma Companions";
        notification.Body = message;
        notification.ShowInForeground = true;
        notification.ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound;
        notification.DeliveryTime = time;
        
        // Schedule notification
        iOSNotificationCenter.ScheduleNotification(notification);
    }
    #endif
    
    // Cancel notifications
    public void CancelAllNotifications()
    {
        #if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
        #elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        iOSNotificationCenter.RemoveAllDeliveredNotifications();
        #endif
    }
    
    public void CancelNotificationType(NotificationType type)
    {
        // This would require tracking notification IDs per type
        // For simplicity, just cancel all and reschedule important ones
        CancelAllNotifications();
        
        // Reschedule important notifications
        if (type != NotificationType.DailyBonus)
        {
            ScheduleNotification(
                NotificationType.DailyBonus,
                DateTime.Now.AddHours(dailyBonusReminderHours)
            );
        }
    }
    
    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            // App going to background, schedule reminders
            SchedulePetCareReminders();
            
            // Schedule return reminder
            ScheduleNotification(
                NotificationType.DailyBonus,
                DateTime.Now.AddHours(24)
            );
        }
        else
        {
            // App returning to foreground, cancel any pending notifications
            // that are no longer relevant
            // For full implementation, would check which ones to cancel
        }
    }
}

public enum NotificationType
{
    PetHungry,
    PetUnhappy,
    DailyBonus,
    GuildEvent,
    NewFeature,
    SpecialOffer
}
