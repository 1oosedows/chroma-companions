// GuildManager.cs - Handles guild functionality
using System;
using System.Collections.Generic;
using UnityEngine;

public class GuildManager : MonoBehaviour
{
    private static GuildManager _instance;
    public static GuildManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("GuildManager instance not found!");
            }
            return _instance;
        }
    }
    
    [SerializeField] private int minLevelForGuildAccess = 15;
    [SerializeField] private GameObject guildTutorialPrefab;
    
    public Guild currentUserGuild;
    private Dictionary<string, Guild> cachedGuilds = new Dictionary<string, Guild>();
    
    // Badges and achievements
    [SerializeField] private List<GuildBadge> availableBadges = new List<GuildBadge>();
    
    // Events
    public Action<Guild> OnGuildJoined;
    public Action<GuildBadge> OnBadgeEarned;
    public Action<Guild, GuildEvent> OnGuildEventCreated;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public bool CanAccessGuildFeature()
    {
        // Check if player meets level requirement
        return GameManager.Instance.GetPlayerLevel() >= minLevelForGuildAccess;
    }
    
    public void ShowGuildAccessRequirements()
    {
        UIManager.Instance.ShowMessage($"Reach level {minLevelForGuildAccess} to unlock guilds!");
    }
    
    public void ShowGuildTutorial()
    {
        // Show one-time tutorial about guild features and community guidelines
        Instantiate(guildTutorialPrefab);
    }
    
    public void AttemptJoinGuild(string guildId)
    {
        if (!CanAccessGuildFeature())
        {
            ShowGuildAccessRequirements();
            return;
        }
        
        // Check if player has completed tutorial
        if (!PlayerPrefs.HasKey("GuildTutorialCompleted"))
        {
            ShowGuildTutorial();
            return;
        }
        
        // In a real app, this would make a server call
        FetchGuildById(guildId, (guild) => {
            if (guild != null)
            {
                JoinGuild(guild);
            }
        });
    }
    
    private void JoinGuild(Guild guild)
    {
        // Add player to guild with probationary status
        currentUserGuild = guild;
        
        // Add player with Novice rank initially
        GuildMember newMember = new GuildMember
        {
            memberId = UserData.Instance.userId,
            displayName = UserData.Instance.displayName,
            joinDate = DateTime.Now,
            rank = GuildRank.Novice,
            contributionPoints = 0,
            isInProbation = true
        };
        
        guild.members.Add(newMember);
        
        // Reset probation after 3 days of active play (in a real app)
        // This is simulated here
        
        OnGuildJoined?.Invoke(guild);
        
        UIManager.Instance.ShowMessage("Welcome to " + guild.guildName + "!");
    }
    
    public void LeaveGuild()
    {
        if (currentUserGuild != null)
        {
            string oldGuildName = currentUserGuild.guildName;
            currentUserGuild = null;
            
            UIManager.Instance.ShowMessage("You have left " + oldGuildName);
        }
    }
    
    public List<Guild> GetRecommendedGuilds()
    {
        // In a real app, this would fetch from server based on player preferences
        // Simulated here with placeholder data
        List<Guild> recommendations = new List<Guild>();
        
        // Add some placeholder guilds
        recommendations.Add(CreateSampleGuild("Color Guardians", "A friendly guild for new players"));
        recommendations.Add(CreateSampleGuild("Pet Masters", "Focus on advanced pet training"));
        recommendations.Add(CreateSampleGuild("Rainbow Collectors", "For serious collectors of rare pets"));
        
        return recommendations;
    }
    
    private Guild CreateSampleGuild(string name, string description)
    {
        return new Guild
        {
            guildId = System.Guid.NewGuid().ToString(),
            guildName = name,
            description = description,
            creationDate = DateTime.Now.AddDays(-UnityEngine.Random.Range(10, 100)),
            members = new List<GuildMember>(),
            events = new List<GuildEvent>(),
            levelRequirement = 15
        };
    }
    
    private void FetchGuildById(string guildId, Action<Guild> callback)
    {
        // In a real app, this would be a server call
        // For now, check cache or create placeholder
        
        if (cachedGuilds.ContainsKey(guildId))
        {
            callback(cachedGuilds[guildId]);
            return;
        }
        
        // Simulate network delay
        StartCoroutine(SimulateNetworkDelay(() => {
            Guild guild = CreateSampleGuild("Color Masters", "A guild for color enthusiasts");
            guild.guildId = guildId;
            
            cachedGuilds[guildId] = guild;
            callback(guild);
        }));
    }
    
    private System.Collections.IEnumerator SimulateNetworkDelay(Action callback)
    {
        yield return new WaitForSeconds(0.5f);
        callback();
    }
    
    public void AwardContributionPoints(int points)
    {
        if (currentUserGuild == null) return;
        
        // Find member in current guild
        GuildMember currentMember = currentUserGuild.members.Find(m => m.memberId == UserData.Instance.userId);
        
        if (currentMember != null)
        {
            currentMember.contributionPoints += points;
            
            // Check for rank advancement
            UpdateMemberRank(currentMember);
            
            // Check for badges
            CheckForBadgeAwards(currentMember);
        }
    }
    
    private void UpdateMemberRank(GuildMember member)
    {
        // Simple rank progression based on points
        if (member.contributionPoints >= 1000 && member.rank < GuildRank.Elder)
        {
            member.rank = GuildRank.Elder;
            UIManager.Instance.ShowMessage("You've been promoted to Elder!");
        }
        else if (member.contributionPoints >= 500 && member.rank < GuildRank.Mentor)
        {
            member.rank = GuildRank.Mentor;
            UIManager.Instance.ShowMessage("You've been promoted to Mentor!");
        }
        else if (member.contributionPoints >= 200 && member.rank < GuildRank.Member)
        {
            member.rank = GuildRank.Member;
            member.isInProbation = false;
            UIManager.Instance.ShowMessage("You're now a full Member!");
        }
    }
    
    private void CheckForBadgeAwards(GuildMember member)
    {
        // Example of awarding badges based on contribution
        foreach (GuildBadge badge in availableBadges)
        {
            if (!member.badges.Contains(badge.badgeId) && 
                member.contributionPoints >= badge.requiredPoints)
            {
                member.badges.Add(badge.badgeId);
                OnBadgeEarned?.Invoke(badge);
                UIManager.Instance.ShowMessage($"You earned the '{badge.badgeName}' badge!");
            }
        }
    }
    
    public void CreateGuildEvent(string title, string description, DateTime eventTime)
    {
        if (currentUserGuild == null) return;
        
        // Check if member has permission (Member rank or higher)
        GuildMember currentMember = currentUserGuild.members.Find(m => m.memberId == UserData.Instance.userId);
        
        if (currentMember == null || currentMember.rank < GuildRank.Member)
        {
            UIManager.Instance.ShowMessage("You need to be a full Member to create events");
            return;
        }
        
        GuildEvent newEvent = new GuildEvent
        {
            eventId = System.Guid.NewGuid().ToString(),
            title = title,
            description = description,
            creatorId = currentMember.memberId,
            eventTime = eventTime,
            participants = new List<string>()
        };
        
        currentUserGuild.events.Add(newEvent);
        OnGuildEventCreated?.Invoke(currentUserGuild, newEvent);
        
        UIManager.Instance.ShowMessage("Guild event created!");
    }
    
    public List<GuildBadge> GetAllBadges()
    {
        return availableBadges;
    }
    
    // For moderation purposes - only visible to guild leaders and moderators
    public int GetMemberTrustScore(string memberId)
    {
        if (currentUserGuild == null) return 0;
        
        // Check if caller has permission
        GuildMember currentMember = currentUserGuild.members.Find(m => m.memberId == UserData.Instance.userId);
        
        if (currentMember == null || currentMember.rank < GuildRank.Elder)
        {
            Debug.LogWarning("Attempted to access trust score without permission");
            return 0;
        }
        
        // Calculate trust score based on:
        // - Time in guild
        // - Contribution points
        // - Reports against them
        // - Positive interactions
        
        GuildMember targetMember = currentUserGuild.members.Find(m => m.memberId == memberId);
        if (targetMember == null) return 0;
        
        int trustScore = 50; // Base score
        
        // Add points for membership time (up to 30 points for 30+ days)
        int daysInGuild = (int)(DateTime.Now - targetMember.joinDate).TotalDays;
        trustScore += Math.Min(daysInGuild, 30);
        
        // Add points for contributions (up to 20 points)
        trustScore += Math.Min(targetMember.contributionPoints / 50, 20);
        
        // Subtract for reports (not implemented in this example)
        
        return Mathf.Clamp(trustScore, 0, 100);
    }
}

// Guild-related data structures
[Serializable]
public class Guild
{
    public string guildId;
    public string guildName;
    public string description;
    public DateTime creationDate;
    public List<GuildMember> members = new List<GuildMember>();
    public List<GuildEvent> events = new List<GuildEvent>();
    public int levelRequirement = 15;
}

[Serializable]
public enum GuildRank
{
    Novice,     // New members in probation
    Member,     // Regular members
    Mentor,     // Can assist new members
    Elder,      // Can moderate and make decisions
    Leader      // Guild owner
}

[Serializable]
public class GuildMember
{
    public string memberId;
    public string displayName;
    public DateTime joinDate;
    public GuildRank rank;
    public int contributionPoints;
    public bool isInProbation;
    public List<string> badges = new List<string>();
}

[Serializable]
public class GuildEvent
{
    public string eventId;
    public string title;
    public string description;
    public string creatorId;
    public DateTime eventTime;
    public List<string> participants = new List<string>();
}

[Serializable]
public class GuildBadge
{
    public string badgeId;
    public string badgeName;
    public string description;
    public Sprite badgeIcon;
    public int requiredPoints;
}