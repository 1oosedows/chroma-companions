// ColorRushRacer.cs - Fast-paced endless runner with color-matching obstacles
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorRushRacer : MiniGameBase
{
    [Header("Player Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float playerSpeed = 5f;
    [SerializeField] private float laneChangeSpeed = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravity = 20f;
    
    [Header("Lane Settings")]
    [SerializeField] private int laneCount = 3;
    [SerializeField] private float laneWidth = 2f;
    
    [Header("Obstacle Settings")]
    [SerializeField] private List<GameObject> obstaclePrefabs;
    [SerializeField] private List<Color> obstacleColors;
    [SerializeField] private float initialObstacleSpawnRate = 1.5f;
    [SerializeField] private float obstacleSpawnRateDecreasePerMinute = 0.1f;
    [SerializeField] private float minimumObstacleSpawnRate = 0.5f;
    [SerializeField] private float obstacleSpeed = 8f;
    
    [Header("Collectible Settings")]
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private float collectibleSpawnChance = 0.3f;
    [SerializeField] private int collectibleBaseScore = 10;
    
    // References
    private PlayerController player;
    private Transform obstacleContainer;
    
    // Game state
    private List<Obstacle> activeObstacles = new List<Obstacle>();
    private float obstacleSpawnTimer = 0f;
    private float currentObstacleSpawnRate;
    private float distanceTraveled = 0f;
    private int colorChanges = 0;
    private bool isPlayerAlive = true;
    
    // Cached values
    private Color currentPlayerColor;
    private int currentLane = 1; // Middle lane
    
    protected override void Awake()
    {
        base.Awake();
        
        // Create obstacle container
        obstacleContainer = new GameObject("ObstacleContainer").transform;
        obstacleContainer.SetParent(transform);
    }
    
    public override void StartGame()
    {
        // Initialize game state
        InitializeGame();
        
        // Set starting values
        currentObstacleSpawnRate = initialObstacleSpawnRate;
        distanceTraveled = 0f;
        colorChanges = 0;
        isPlayerAlive = true;
        
        // Spawn player
        SpawnPlayer();
        
        // Assign random starting color
        SetRandomPlayerColor();
    }
    
    private void SpawnPlayer()
    {
        // Instantiate player at the bottom center of the screen
        Vector3 playerPosition = new Vector3(0, 1, 0);
        GameObject playerObject = Instantiate(playerPrefab, playerPosition, Quaternion.identity);
        
        // Get controller component
        player = playerObject.GetComponent<PlayerController>();
        
        if (player == null)
        {
            Debug.LogError("Player prefab does not have PlayerController component!");
            return;
        }
        
        // Set up player
        player.Initialize(this, laneCount, laneWidth);
        
        // Subscribe to events
        player.OnCollision += HandlePlayerCollision;
        player.OnColorChange += HandlePlayerColorChange;
        player.OnCollectibleCollected += HandleCollectibleCollected;
    }
    
    private void SetRandomPlayerColor()
    {
        int randomColorIndex = UnityEngine.Random.Range(0, obstacleColors.Count);
        currentPlayerColor = obstacleColors[randomColorIndex];
        player.SetColor(currentPlayerColor);
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (!isGameActive || isGamePaused || !isPlayerAlive)
            return;
            
        // Update distance traveled
        distanceTraveled += playerSpeed * Time.deltaTime;
        
        // Gradually increase game speed based on distance
        float speedMultiplier = 1f + (distanceTraveled / 1000f);
        float currentPlayerSpeed = playerSpeed * speedMultiplier;
        float currentObstacleSpeed = obstacleSpeed * speedMultiplier;
        
        // Update player
        player.UpdateSpeed(currentPlayerSpeed);
        
        // Decrease spawn rate over time (make the game harder)
        float gameTimeInMinutes = gameTimer / 60f;
        currentObstacleSpawnRate = Mathf.Max(
            initialObstacleSpawnRate - (obstacleSpawnRateDecreasePerMinute * gameTimeInMinutes),
            minimumObstacleSpawnRate
        );
        
        // Spawn obstacles
        obstacleSpawnTimer += Time.deltaTime;
        if (obstacleSpawnTimer >= currentObstacleSpawnRate)
        {
            SpawnObstacle(currentObstacleSpeed);
            obstacleSpawnTimer = 0f;
        }
        
        // Update obstacles
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            Obstacle obstacle = activeObstacles[i];
            obstacle.Move(Time.deltaTime);
            
            // Remove obstacles that have gone off-screen
            if (obstacle.transform.position.z < -10f)
            {
                Destroy(obstacle.gameObject);
                activeObstacles.RemoveAt(i);
            }
        }
        
        // Update UI (you would have a more complex UI system in a real game)
        // Display score based on distance and color changes
        int distanceScore = Mathf.FloorToInt(distanceTraveled * 0.1f);
        int colorChangeScore = colorChanges * 25;
        currentScore = distanceScore + colorChangeScore;
        
        OnScoreChanged?.Invoke(currentScore);
    }
    
    private void SpawnObstacle(float speed)
    {
        // Determine which lanes to spawn obstacles in
        bool[] laneHasObstacle = new bool[laneCount];
        int obstacleCount = UnityEngine.Random.Range(1, laneCount); // At least one lane is always free
        
        for (int i = 0; i < obstacleCount; i++)
        {
            int lane;
            do
            {
                lane = UnityEngine.Random.Range(0, laneCount);
            } while (laneHasObstacle[lane]);
            
            laneHasObstacle[lane] = true;
        }
        
        // Determine if collectibles should spawn in empty lanes
        for (int lane = 0; lane < laneCount; lane++)
        {
            if (!laneHasObstacle[lane] && UnityEngine.Random.value < collectibleSpawnChance)
            {
                SpawnCollectible(lane);
            }
        }
        
        // Spawn obstacles in marked lanes
        for (int lane = 0; lane < laneCount; lane++)
        {
            if (laneHasObstacle[lane])
            {
                // Choose random obstacle prefab
                int prefabIndex = UnityEngine.Random.Range(0, obstaclePrefabs.Count);
                GameObject obstaclePrefab = obstaclePrefabs[prefabIndex];
                
                // Choose random color, but make some match the player's color
                Color obstacleColor;
                bool isMatchingColor = UnityEngine.Random.value < 0.3f; // 30% chance to match player color
                
                if (isMatchingColor)
                {
                    obstacleColor = currentPlayerColor;
                }
                else
                {
                    // Pick a different color
                    List<Color> availableColors = new List<Color>(obstacleColors);
                    availableColors.Remove(currentPlayerColor);
                    int colorIndex = UnityEngine.Random.Range(0, availableColors.Count);
                    obstacleColor = availableColors[colorIndex];
                }
                
                // Calculate position
                float lanePosition = (lane - (laneCount - 1) / 2f) * laneWidth;
                Vector3 obstaclePosition = new Vector3(lanePosition, 1f, 50f); // Spawn ahead of the player
                
                // Create obstacle
                GameObject obstacleObject = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity, obstacleContainer);
                Obstacle obstacle = obstacleObject.GetComponent<Obstacle>();
                
                if (obstacle == null)
                {
                    Debug.LogError("Obstacle prefab does not have Obstacle component!");
                    Destroy(obstacleObject);
                    continue;
                }
                
                // Initialize obstacle
                obstacle.Initialize(speed, obstacleColor, isMatchingColor);
                
                // Add to active obstacles
                activeObstacles.Add(obstacle);
            }
        }
    }
    
    private void SpawnCollectible(int lane)
    {
        // Calculate position
        float lanePosition = (lane - (laneCount - 1) / 2f) * laneWidth;
        Vector3 collectiblePosition = new Vector3(lanePosition, 1.5f, 50f); // Spawn ahead of player, slightly higher
        
        // Create collectible
        GameObject collectibleObject = Instantiate(collectiblePrefab, collectiblePosition, Quaternion.identity, obstacleContainer);
        Collectible collectible = collectibleObject.GetComponent<Collectible>();
        
        if (collectible == null)
        {
            Debug.LogError("Collectible prefab does not have Collectible component!");
            Destroy(collectibleObject);
            return;
        }
        
        // Initialize collectible
        collectible.Initialize(obstacleSpeed, collectibleBaseScore);
        
        // Add to active obstacles (using the same list for simplicity)
        activeObstacles.Add(collectible);
    }
    
    private void HandlePlayerCollision(Obstacle obstacle)
    {
        if (!isPlayerAlive)
            return;
            
        // Check if player color matches obstacle color
        bool canPassThrough = obstacle.IsColorMatching && obstacle.GetColor() == currentPlayerColor;
        
        if (!canPassThrough)
        {
            // Player hit an obstacle they can't pass through
            PlayDeathAnimation();
            isPlayerAlive = false;
            
            // End game after short delay
            StartCoroutine(DelayedGameOver(1.5f));
        }
    }
    
    private void HandlePlayerColorChange(Color newColor)
    {
        if (newColor != currentPlayerColor)
        {
            currentPlayerColor = newColor;
            colorChanges++;
        }
    }
    
    private void HandleCollectibleCollected(int points)
    {
        AddScore(points);
    }
    
    private void PlayDeathAnimation()
    {
        // In a real game, you would have a death animation here
        player.PlayDeathAnimation();
    }
    
    private IEnumerator DelayedGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndGame();
    }
    
    public override void PauseGame()
    {
        if (!isGameActive) return;
        
        isGamePaused = true;
        
        // Pause player and obstacle movement
        if (player != null)
            player.SetPaused(true);
            
        OnGamePaused?.Invoke();
    }
    
    public override void ResumeGame()
    {
        if (!isGameActive) return;
        
        isGamePaused = false;
        
        // Resume player and obstacle movement
        if (player != null)
            player.SetPaused(false);
            
        OnGameResumed?.Invoke();
    }
    
    public override void EndGame()
    {
        if (!isGameActive) return;
        
        // Call base class finalization
        FinalizeGame();
    }
    
    protected override void OnDestroy()
    {
        // Clean up
        if (player != null)
        {
            player.OnCollision -= HandlePlayerCollision;
            player.OnColorChange -= HandlePlayerColorChange;
            player.OnCollectibleCollected -= HandleCollectibleCollected;
        }
    }
}

// Helper class for the player
public class PlayerController : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    
    private ColorRushRacer gameController;
    private int laneCount;
    private float laneWidth;
    private int currentLane = 1; // Middle lane
    private float targetLanePosition;
    private float moveSpeed;
    private float laneChangeSpeed = 10f;
    private bool isGrounded = true;
    private bool isPaused = false;
    private Color currentColor;
    
    // Events
    public event Action<Obstacle> OnCollision;
    public event Action<Color> OnColorChange;
    public event Action<int> OnCollectibleCollected;
    
    public void Initialize(ColorRushRacer controller, int lanes, float width)
    {
        gameController = controller;
        laneCount = lanes;
        laneWidth = width;
        currentLane = lanes / 2; // Start in middle lane
        UpdateTargetLanePosition();
    }
    
    public void UpdateSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    public void SetColor(Color color)
    {
        currentColor = color;
        meshRenderer.material.color = color;
        OnColorChange?.Invoke(color);
    }
    
    public void SetPaused(bool paused)
    {
        isPaused = paused;
        
        if (paused)
        {
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }
    }
    
    private void Update()
    {
        if (isPaused)
            return;
            
        // Handle lane changes
        HandleInput();
        
        // Move toward target lane
        float currentLanePosition = transform.position.x;
        float newPosition = Mathf.Lerp(currentLanePosition, targetLanePosition, laneChangeSpeed * Time.deltaTime);
        
        Vector3 position = transform.position;
        position.x = newPosition;
        transform.position = position;
        
        // Check if grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }
    
    private void HandleInput()
    {
        // Move left
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (currentLane > 0)
            {
                currentLane--;
                UpdateTargetLanePosition();
            }
        }
        
        // Move right
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentLane < laneCount - 1)
            {
                currentLane++;
                UpdateTargetLanePosition();
            }
        }
        
        // Jump
        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            Jump();
        }
        
        // Change color (in a real game, you'd have a more complex color switching system)
        if (Input.GetKeyDown(KeyCode.C))
        {
            CycleColor();
        }
    }
    
    private void UpdateTargetLanePosition()
    {
        targetLanePosition = (currentLane - (laneCount - 1) / 2f) * laneWidth;
    }
    
    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, Mathf.Sqrt(2 * jumpHeight * Physics.gravity.magnitude), rb.velocity.z);
        isGrounded = false;
    }
    
    private void CycleColor()
    {
        // In a real game, you'd have a UI for color selection
        // This is a simplified version that just cycles through colors
        Color[] availableColors = gameController.obstacleColors.ToArray();
        int currentIndex = Array.IndexOf(availableColors, currentColor);
        int nextIndex = (currentIndex + 1) % availableColors.Length;
        
        SetColor(availableColors[nextIndex]);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Obstacle obstacle = other.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            OnCollision?.Invoke(obstacle);
        }
        
        Collectible collectible = other.GetComponent<Collectible>();
        if (collectible != null)
        {
            OnCollectibleCollected?.Invoke(collectible.GetScore());
            Destroy(collectible.gameObject);
        }
    }
    
    public void PlayDeathAnimation()
    {
        // In a real game, you would have a more complex death animation
        meshRenderer.material.color = Color.gray;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }
}

// Helper class for obstacles
public class Obstacle : MonoBehaviour
{
    [SerializeField] protected MeshRenderer meshRenderer;
    [SerializeField] protected Collider obstacleCollider;
    
    protected float speed;
    protected Color obstacleColor;
    protected bool isColorMatching;
    
    public bool IsColorMatching => isColorMatching;
    
    public virtual void Initialize(float moveSpeed, Color color, bool matchingColor = false)
    {
        speed = moveSpeed;
        obstacleColor = color;
        isColorMatching = matchingColor;
        
        // Set color
        meshRenderer.material.color = color;
    }
    
    public Color GetColor()
    {
        return obstacleColor;
    }
    
    public virtual void Move(float deltaTime)
    {
        transform.Translate(0, 0, -speed * deltaTime);
    }
}

// Helper class for collectibles
public class Collectible : Obstacle
{
    private int scoreValue;
    
    public void Initialize(float moveSpeed, int baseScore)
    {
        speed = moveSpeed;
        
        // Collectibles always have a distinct color
        meshRenderer.material.color = Color.yellow;
        
        // Vary the score value slightly
        scoreValue = baseScore + UnityEngine.Random.Range(-2, 3);
    }
    
    public int GetScore()
    {
        return scoreValue;
    }
}
