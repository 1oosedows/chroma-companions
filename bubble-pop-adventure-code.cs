// BubblePopAdventure.cs - Pop colored bubbles to progress through fantasy worlds
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubblePopAdventure : MiniGameBase
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 12;
    [SerializeField] private float bubbleSize = 0.8f;
    [SerializeField] private Transform gridContainer;
    
    [Header("Bubble Settings")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private List<Color> bubbleColors = new List<Color>();
    [SerializeField] private float shootSpeed = 10f;
    [SerializeField] private int startingRows = 5;
    
    [Header("Special Bubble Settings")]
    [SerializeField] private float bombBubbleChance = 0.05f;
    [SerializeField] private float rainbowBubbleChance = 0.03f;
    [SerializeField] private float starBubbleChance = 0.04f;
    
    [Header("Level Settings")]
    [SerializeField] private int requiredBubblesToClear = 50;
    [SerializeField] private float newRowInterval = 15f;
    
    // Game objects
    private Bubble[,] grid;
    private List<Bubble> activeBubbles = new List<Bubble>();
    private Bubble currentBubble;
    private Bubble nextBubble;
    private Transform shooter;
    private LineRenderer trajectoryLine;
    
    // Game state
    private Vector2 shootDirection;
    private float rowTimer;
    private int bubblesCleared;
    private int bubblesToClear;
    private bool isAiming;
    private bool isShooting;
    
    // References
    private Camera mainCamera;
    
    protected override void Awake()
    {
        base.Awake();
        
        mainCamera = Camera.main;
        
        // Create shooter
        shooter = new GameObject("Bubble Shooter").transform;
        shooter.SetParent(transform);
        shooter.localPosition = new Vector3(0, -5, 0);
        
        // Create trajectory line
        GameObject lineObj = new GameObject("Trajectory");
        lineObj.transform.SetParent(shooter);
        trajectoryLine = lineObj.AddComponent<LineRenderer>();
        trajectoryLine.startWidth = 0.1f;
        trajectoryLine.endWidth = 0.1f;
        trajectoryLine.positionCount = 2;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = new Color(1, 1, 1, 0.3f);
        trajectoryLine.endColor = new Color(1, 1, 1, 0.0f);
        trajectoryLine.enabled = false;
    }
    
    public override void StartGame()
    {
        // Init game state
        InitializeGame();
        
        // Reset state
        bubblesCleared = 0;
        bubblesToClear = requiredBubblesToClear;
        rowTimer = newRowInterval;
        isAiming = false;
        isShooting = false;
        
        // Create grid
        CreateGrid();
        
        // Fill initial rows
        FillStartingRows();
        
        // Create first bubble to shoot
        CreateNextBubble();
        PrepareNextBubble();
    }
    
    private void CreateGrid()
    {
        // Create grid array
        grid = new Bubble[gridWidth, gridHeight];
        
        // Clear any existing grid elements
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
        
        activeBubbles.Clear();
    }
    
    private void FillStartingRows()
    {
        for (int y = gridHeight - 1; y >= gridHeight - startingRows; y--)
        {
            // Offset even rows
            float xOffset = (y % 2 == 0) ? 0.5f : 0f;
            
            for (int x = 0; x < gridWidth; x++)
            {
                // Random chance to skip some bubbles for more interesting patterns
                if (UnityEngine.Random.value < 0.2f)
                    continue;
                    
                CreateBubbleInGrid(x, y);
            }
        }
    }
    
    private void CreateBubbleInGrid(int x, int y)
    {
        // Skip if position already has a bubble
        if (grid[x, y] != null)
            return;
            
        // Calculate position (with offset for hexagonal grid)
        float xOffset = (y % 2 == 0) ? 0.5f : 0f;
        Vector3 position = new Vector3((x + xOffset) * bubbleSize, y * bubbleSize * 0.85f, 0);
        
        // Create bubble
        GameObject bubbleObj = Instantiate(bubblePrefab, position, Quaternion.identity, gridContainer);
        Bubble bubble = bubbleObj.GetComponent<Bubble>();
        
        if (bubble == null)
        {
            Debug.LogError("Bubble prefab does not have Bubble component!");
            Destroy(bubbleObj);
            return;
        }
        
        // Determine bubble type
        BubbleType bubbleType = BubbleType.Normal;
        
        // Small chance for special bubbles
        float specialRoll = UnityEngine.Random.value;
        if (specialRoll < bombBubbleChance)
            bubbleType = BubbleType.Bomb;
        else if (specialRoll < bombBubbleChance + rainbowBubbleChance)
            bubbleType = BubbleType.Rainbow;
        else if (specialRoll < bombBubbleChance + rainbowBubbleChance + starBubbleChance)
            bubbleType = BubbleType.Star;
        
        // Initialize bubble
        int colorIndex = UnityEngine.Random.Range(0, bubbleColors.Count);
        bubble.Initialize(bubbleColors[colorIndex], bubbleType, new Vector2Int(x, y));
        
        // Add to grid
        grid[x, y] = bubble;
        activeBubbles.Add(bubble);
    }
    
    private void CreateNextBubble()
    {
        // Create bubble at shooter position
        GameObject bubbleObj = Instantiate(bubblePrefab, shooter.position, Quaternion.identity);
        currentBubble = bubbleObj.GetComponent<Bubble>();
        
        if (currentBubble == null)
        {
            Debug.LogError("Bubble prefab does not have Bubble component!");
            Destroy(bubbleObj);
            return;
        }
        
        // Determine bubble type (mostly normal, with some special bubbles)
        BubbleType bubbleType = BubbleType.Normal;
        
        float specialRoll = UnityEngine.Random.value;
        if (specialRoll < rainbowBubbleChance * 2) // More common in player bubbles
            bubbleType = BubbleType.Rainbow;
        
        // Initialize bubble with random color
        int colorIndex = UnityEngine.Random.Range(0, bubbleColors.Count);
        currentBubble.Initialize(bubbleColors[colorIndex], bubbleType, Vector2Int.zero);
        
        // Attach to shooter temporarily
        currentBubble.transform.SetParent(shooter);
        currentBubble.transform.localPosition = Vector3.zero;
    }
    
    private void PrepareNextBubble()
    {
        // Create bubble for preview
        GameObject bubbleObj = Instantiate(bubblePrefab, Vector3.zero, Quaternion.identity);
        nextBubble = bubbleObj.GetComponent<Bubble>();
        
        if (nextBubble == null)
        {
            Debug.LogError("Bubble prefab does not have Bubble component!");
            Destroy(bubbleObj);
            return;
        }
        
        // Initialize with random color
        int colorIndex = UnityEngine.Random.Range(0, bubbleColors.Count);
        
        // Determine bubble type
        BubbleType bubbleType = BubbleType.Normal;
        
        float specialRoll = UnityEngine.Random.value;
        if (specialRoll < rainbowBubbleChance * 2)
            bubbleType = BubbleType.Rainbow;
        
        nextBubble.Initialize(bubbleColors[colorIndex], bubbleType, Vector2Int.zero);
        
        // Position next to shooter
        nextBubble.transform.SetParent(shooter);
        nextBubble.transform.localPosition = new Vector3(-2, 0, 0);
        nextBubble.transform.localScale = Vector3.one * 0.8f; // Slightly smaller for preview
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (!isGameActive || isGamePaused)
            return;
            
        // Handle countdown for new row
        rowTimer -= Time.deltaTime;
        if (rowTimer <= 0)
        {
            AddNewRow();
            rowTimer = newRowInterval;
        }
        
        // Check win condition
        if (bubblesCleared >= bubblesToClear)
        {
            // Level complete!
            EndGame();
            return;
        }
        
        // Check lose condition
        foreach (Bubble bubble in activeBubbles)
        {
            if (bubble.GridPosition.y <= 1)
            {
                // Game over - bubbles reached the bottom
                EndGame();
                return;
            }
        }
        
        // Handle player input
        if (!isShooting && currentBubble != null)
        {
            HandleAiming();
            
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                ShootBubble();
            }
        }
    }
    
    private void HandleAiming()
    {
        // Get mouse position
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        // Calculate direction from shooter to mouse
        Vector3 shootPos = shooter.position;
        Vector3 direction = (mousePos - shootPos).normalized;
        
        // Limit to upward directions only
        if (direction.y < 0.1f)
        {
            direction = new Vector3(Mathf.Sign(direction.x), 0.1f, 0).normalized;
        }
        
        // Save shoot direction
        shootDirection = direction;
        
        // Update trajectory line
        DrawTrajectory(shootPos, direction);
        
        // Rotate shooter to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        shooter.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    private void DrawTrajectory(Vector3 startPos, Vector3 direction)
    {
        if (!isAiming)
        {
            trajectoryLine.enabled = true;
            isAiming = true;
        }
        
        // Simple straight line trajectory
        trajectoryLine.SetPosition(0, startPos);
        trajectoryLine.SetPosition(1, startPos + direction * 10f);
    }
    
    private void ShootBubble()
    {
        if (currentBubble == null || isShooting)
            return;
            
        isShooting = true;
        trajectoryLine.enabled = false;
        isAiming = false;
        
        // Detach from shooter
        currentBubble.transform.SetParent(null);
        
        // Start moving
        StartCoroutine(MoveBubble(currentBubble, shootDirection));
    }
    
    private IEnumerator MoveBubble(Bubble bubble, Vector2 direction)
    {
        Vector3 position = bubble.transform.position;
        Vector3 velocity = direction * shootSpeed;
        
        while (true)
        {
            // Move bubble
            position += velocity * Time.deltaTime;
            bubble.transform.position = position;
            
            // Check for collisions with walls
            if (position.x < -bubbleSize * 0.5f || position.x > gridWidth * bubbleSize + bubbleSize * 0.5f)
            {
                // Bounce off walls
                velocity = new Vector3(-velocity.x, velocity.y, 0);
                
                // Add slight vertical adjustment to avoid getting stuck
                velocity = velocity.normalized * shootSpeed;
            }
            
            // Check for collision with existing bubbles
            Vector2Int gridPos = GetGridPosition(position);
            
            if (gridPos.y >= 0 && gridPos.y < gridHeight)
            {
                bool collidedWithBubble = false;
                
                // Check surrounded grid positions
                for (int yOffset = -1; yOffset <= 1; yOffset++)
                {
                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        int checkX = gridPos.x + xOffset;
                        int checkY = gridPos.y + yOffset;
                        
                        // Skip if out of grid bounds
                        if (checkX < 0 || checkX >= gridWidth || checkY < 0 || checkY >= gridHeight)
                            continue;
                            
                        // Skip if this is the same position
                        if (xOffset == 0 && yOffset == 0)
                            continue;
                            
                        // Check if position has a bubble
                        if (grid[checkX, checkY] != null)
                        {
                            // Found collision
                            collidedWithBubble = true;
                            
                            // Find the closest empty grid position
                            gridPos = FindEmptyGridPosition(position, gridPos);
                            break;
                        }
                    }
                    
                    if (collidedWithBubble)
                        break;
                }
                
                // Top of the grid is also a collision
                if (gridPos.y >= gridHeight - 1)
                {
                    collidedWithBubble = true;
                }
                
                if (collidedWithBubble)
                {
                    // Snap to grid position
                    SnapBubbleToGrid(bubble, gridPos);
                    break;
                }
            }
            
            yield return null;
        }
        
        // Bubble has stopped moving
        yield return new WaitForSeconds(0.1f);
        
        // Check for matches
        CheckForMatches(bubble);
        
        // Reset shooting state
        isShooting = false;
        
        // Current bubble becomes next bubble
        currentBubble = nextBubble;
        currentBubble.transform.SetParent(shooter);
        currentBubble.transform.localPosition = Vector3.zero;
        currentBubble.transform.localScale = Vector3.one;
        
        // Create new next bubble
        PrepareNextBubble();
    }
    
    private Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        // Convert world position to grid position
        int y = Mathf.RoundToInt(worldPosition.y / (bubbleSize * 0.85f));
        
        // Adjust x based on whether row is offset
        float xOffset = (y % 2 == 0) ? 0.5f : 0f;
        int x = Mathf.RoundToInt((worldPosition.x - xOffset * bubbleSize) / bubbleSize);
        
        return new Vector2Int(x, y);
    }
    
    private Vector2Int FindEmptyGridPosition(Vector3 worldPosition, Vector2Int nearestGridPos)
    {
        // Already checked if the nearest grid position has a bubble
        if (nearestGridPos.x >= 0 && nearestGridPos.x < gridWidth && 
            nearestGridPos.y >= 0 && nearestGridPos.y < gridHeight &&
            grid[nearestGridPos.x, nearestGridPos.y] == null)
        {
            return nearestGridPos;
        }
        
        // Check adjacent grid positions
        List<Vector2Int> candidates = new List<Vector2Int>();
        
        // Get positions around the hit point
        for (int yOffset = -1; yOffset <= 1; yOffset++)
        {
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                int checkX = nearestGridPos.x + xOffset;
                int checkY = nearestGridPos.y + yOffset;
                
                // Skip if out of grid bounds
                if (checkX < 0 || checkX >= gridWidth || checkY < 0 || checkY >= gridHeight)
                    continue;
                    
                // Skip if position already has a bubble
                if (grid[checkX, checkY] != null)
                    continue;
                    
                candidates.Add(new Vector2Int(checkX, checkY));
            }
        }
        
        // Find the closest candidate
        float closestDistance = float.MaxValue;
        Vector2Int bestPosition = nearestGridPos;
        
        foreach (Vector2Int candidate in candidates)
        {
            // Calculate world position of this grid position
            float yPos = candidate.y * bubbleSize * 0.85f;
            float xOffset = (candidate.y % 2 == 0) ? 0.5f : 0f;
            float xPos = (candidate.x + xOffset) * bubbleSize;
            
            Vector3 candidateWorldPos = new Vector3(xPos, yPos, 0);
            
            float distance = Vector3.Distance(worldPosition, candidateWorldPos);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestPosition = candidate;
            }
        }
        
        return bestPosition;
    }
    
    private void SnapBubbleToGrid(Bubble bubble, Vector2Int gridPos)
    {
        // Check if position is valid
        if (gridPos.x < 0 || gridPos.x >= gridWidth || gridPos.y < 0 || gridPos.y >= gridHeight)
            return;
            
        // Check if position already has a bubble
        if (grid[gridPos.x, gridPos.y] != null)
            return;
            
        // Calculate world position
        float yPos = gridPos.y * bubbleSize * 0.85f;
        float xOffset = (gridPos.y % 2 == 0) ? 0.5f : 0f;
        float xPos = (gridPos.x + xOffset) * bubbleSize;
        
        Vector3 position = new Vector3(xPos, yPos, 0);
        
        // Snap to position
        bubble.transform.position = position;
        
        // Update bubble grid position
        bubble.GridPosition = gridPos;
        
        // Add to grid
        grid[gridPos.x, gridPos.y] = bubble;
        activeBubbles.Add(bubble);
    }
    
    private void CheckForMatches(Bubble bubble)
    {
        // Handle special bubble types
        switch (bubble.Type)
        {
            case BubbleType.Bomb:
                HandleBombBubble(bubble);
                return;
                
            case BubbleType.Rainbow:
                HandleRainbowBubble(bubble);
                return;
                
            case BubbleType.Star:
                HandleStarBubble(bubble);
                return;
        }
        
        // For normal bubbles, find connected bubbles of the same color
        List<Bubble> matchedBubbles = FindMatchingBubbles(bubble);
        
        if (matchedBubbles.Count >= 3)
        {
            // Pop the matched bubbles
            foreach (Bubble matchedBubble in matchedBubbles)
            {
                PopBubble(matchedBubble);
            }
            
            // Check for floating bubbles after popping
            CheckForFloatingBubbles();
            
            // Award points based on number of bubbles popped
            int pointsPerBubble = 10;
            int matchBonus = (matchedBubbles.Count - 3) * 5; // Bonus for matching more than 3
            AddScore(matchedBubbles.Count * pointsPerBubble + matchBonus);
            
            // Update bubbles cleared count
            bubblesCleared += matchedBubbles.Count;
        }
    }
    
    private List<Bubble> FindMatchingBubbles(Bubble startBubble)
    {
        List<Bubble> matchingBubbles = new List<Bubble>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        // Use breadth-first search to find connected bubbles of the same color
        Queue<Bubble> queue = new Queue<Bubble>();
        queue.Enqueue(startBubble);
        visited.Add(startBubble.GridPosition);
        
        while (queue.Count > 0)
        {
            Bubble current = queue.Dequeue();
            matchingBubbles.Add(current);
            
            // Check adjacent bubbles
            List<Vector2Int> adjacentPositions = GetAdjacentPositions(current.GridPosition);
            
            foreach (Vector2Int adjacentPos in adjacentPositions)
            {
                // Skip if already visited
                if (visited.Contains(adjacentPos))
                    continue;
                    
                // Skip if out of grid bounds
                if (adjacentPos.x < 0 || adjacentPos.x >= gridWidth || 
                    adjacentPos.y < 0 || adjacentPos.y >= gridHeight)
                    continue;
                    
                // Skip if no bubble at this position
                if (grid[adjacentPos.x, adjacentPos.y] == null)
                    continue;
                    
                Bubble adjacentBubble = grid[adjacentPos.x, adjacentPos.y];
                
                // Check if it's the same color
                if (adjacentBubble.GetColor() == startBubble.GetColor())
                {
                    queue.Enqueue(adjacentBubble);
                    visited.Add(adjacentPos);
                }
            }
        }
        
        return matchingBubbles;
    }
    
    private List<Vector2Int> GetAdjacentPositions(Vector2Int position)
    {
        List<Vector2Int> adjacentPositions = new List<Vector2Int>();
        
        // Different adjacency depending on whether the row is odd or even
        bool isEvenRow = position.y % 2 == 0;
        
        // Adjacent positions for even rows
        if (isEvenRow)
        {
            adjacentPositions.Add(new Vector2Int(position.x - 1, position.y));     // Left
            adjacentPositions.Add(new Vector2Int(position.x + 1, position.y));     // Right
            adjacentPositions.Add(new Vector2Int(position.x - 1, position.y + 1));  // Top-Left
            adjacentPositions.Add(new Vector2Int(position.x, position.y + 1));      // Top-Right
            adjacentPositions.Add(new Vector2Int(position.x - 1, position.y - 1));  // Bottom-Left
            adjacentPositions.Add(new Vector2Int(position.x, position.y - 1));      // Bottom-Right
        }
        // Adjacent positions for odd rows
        else
        {
            adjacentPositions.Add(new Vector2Int(position.x - 1, position.y));     // Left
            adjacentPositions.Add(new Vector2Int(position.x + 1, position.y));     // Right
            adjacentPositions.Add(new Vector2Int(position.x, position.y + 1));      // Top-Left
            adjacentPositions.Add(new Vector2Int(position.x + 1, position.y + 1));  // Top-Right
            adjacentPositions.Add(new Vector2Int(position.x, position.y - 1));      // Bottom-Left
            adjacentPositions.Add(new Vector2Int(position.x + 1, position.y - 1));  // Bottom-Right
        }
        
        return adjacentPositions;
    }
    
    private void PopBubble(Bubble bubble)
    {
        // Remove from grid
        grid[bubble.GridPosition.x, bubble.GridPosition.y] = null;
        activeBubbles.Remove(bubble);
        
        // Play pop animation
        bubble.PlayPopAnimation();
        
        // Destroy after animation
        Destroy(bubble.gameObject, 0.3f);
    }
    
    private void HandleBombBubble(Bubble bubble)
    {
        List<Bubble> bubblesInRadius = new List<Bubble>();
        
        // Get all bubbles in a certain radius
        for (int y = bubble.GridPosition.y - 2; y <= bubble.GridPosition.y + 2; y++)
        {
            for (int x = bubble.GridPosition.x - 2; x <= bubble.GridPosition.x + 2; x++)
            {
                // Skip if out of grid bounds
                if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
                    continue;
                    
                // Skip if no bubble at this position
                if (grid[x, y] == null)
                    continue;
                    
                bubblesInRadius.Add(grid[x, y]);
            }
        }
        
        // Pop all bubbles in radius
        foreach (Bubble bubbleInRadius in bubblesInRadius)
        {
            PopBubble(bubbleInRadius);
        }
        
        // Pop the bomb bubble itself
        PopBubble(bubble);
        
        // Check for floating bubbles
        CheckForFloatingBubbles();
        
        // Award points
        int pointsPerBubble = 10;
        int bombBonus = 50;
        AddScore(bubblesInRadius.Count * pointsPerBubble + bombBonus);
        
        // Update bubbles cleared count
        bubblesCleared += bubblesInRadius.Count + 1;
    }
    
    private void HandleRainbowBubble(Bubble bubble)
    {
        // Find a color that's present in the grid
        List<Color> colorsInGrid = new List<Color>();
        
        foreach (Bubble gridBubble in activeBubbles)
        {
            if (gridBubble != bubble && gridBubble.Type == BubbleType.Normal)
            {
                Color color = gridBubble.GetColor();
                if (!colorsInGrid.Contains(color))
                {
                    colorsInGrid.Add(color);
                }
            }
        }
        
        if (colorsInGrid.Count > 0)
        {
            // Pick a random color
            Color targetColor = colorsInGrid[UnityEngine.Random.Range(0, colorsInGrid.Count)];
            
            // Find all bubbles of that color
            List<Bubble> bubblesToPop = new List<Bubble>();
            
            foreach (Bubble gridBubble in activeBubbles)
            {
                if (gridBubble.GetColor() == targetColor)
                {
                    bubblesToPop.Add(gridBubble);
                }
            }
            
            // Pop all bubbles of that color
            foreach (Bubble bubbleToPop in bubblesToPop)
            {
                PopBubble(bubbleToPop);
            }
            
            // Pop the rainbow bubble itself
            PopBubble(bubble);
            
            // Check for floating bubbles
            CheckForFloatingBubbles();
            
            // Award points
            int pointsPerBubble = 10;
            int rainbowBonus = 30;
            AddScore(bubblesToPop.Count * pointsPerBubble + rainbowBonus);
            
            // Update bubbles cleared count
            bubblesCleared += bubblesToPop.Count + 1;
        }
        else
        {
            // Just pop the rainbow bubble if there are no other bubbles
            PopBubble(bubble);
            AddScore(30);
            bubblesCleared++;
        }
    }
    
    private void HandleStarBubble(Bubble bubble)
    {
        // Pop the entire row
        int rowToClear = bubble.GridPosition.y;
        List<Bubble> bubblesToPop = new List<Bubble>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, rowToClear] != null)
            {
                bubblesToPop.Add(grid[x, rowToClear]);
            }
        }
        
        // Pop all bubbles in the row
        foreach (Bubble bubbleToPop in bubblesToPop)
        {
            PopBubble(bubbleToPop);
        }
        
        // Check for floating bubbles
        CheckForFloatingBubbles();
        
        // Award points
        int pointsPerBubble = 10;
        int starBonus = 40;
        AddScore(bubblesToPop.Count * pointsPerBubble + starBonus);
        
        // Update bubbles cleared count
        bubblesCleared += bubblesToPop.Count;
    }
    
    private void CheckForFloatingBubbles()
    {
        // Find all bubbles connected to the top row
        HashSet<Vector2Int> connectedPositions = new HashSet<Vector2Int>();
        
        // Start from all bubbles in the top row
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, gridHeight - 1] != null)
            {
                // Use breadth-first search to find all connected bubbles
                Queue<Vector2Int> queue = new Queue<Vector2Int>();
                queue.Enqueue(new Vector2Int(x, gridHeight - 1));
                connectedPositions.Add(new Vector2Int(x, gridHeight - 1));
                
                while (queue.Count > 0)
                {
                    Vector2Int current = queue.Dequeue();
                    
                    // Check adjacent positions
                    List<Vector2Int> adjacentPositions = GetAdjacentPositions(current);
                    
                    foreach (Vector2Int adjacent in adjacentPositions)
                    {
                        // Skip if already checked or out of bounds
                        if (connectedPositions.Contains(adjacent) ||
                            adjacent.x < 0 || adjacent.x >= gridWidth ||
                            adjacent.y < 0 || adjacent.y >= gridHeight)
                            continue;
                            
                        // If there's a bubble, add to connected set
                        if (grid[adjacent.x, adjacent.y] != null)
                        {
                            queue.Enqueue(adjacent);
                            connectedPositions.Add(adjacent);
                        }
                    }
                }
            }
        }
        
        // Find all floating bubbles (not connected to the top)
        List<Bubble> floatingBubbles = new List<Bubble>();
        
        foreach (Bubble bubble in activeBubbles.ToArray()) // Use ToArray to avoid modification during enumeration
        {
            if (!connectedPositions.Contains(bubble.GridPosition))
            {
                floatingBubbles.Add(bubble);
            }
        }
        
        // Pop all floating bubbles
        foreach (Bubble floatingBubble in floatingBubbles)
        {
            PopBubble(floatingBubble);
        }
        
        // Award points for floating bubbles
        if (floatingBubbles.Count > 0)
        {
            int pointsPerBubble = 15; // Bonus points for floating bubbles
            AddScore(floatingBubbles.Count * pointsPerBubble);
            
            // Update bubbles cleared count
            bubblesCleared += floatingBubbles.Count;
        }
    }
    
    private void AddNewRow()
    {
        // Shift all bubbles down one row
        for (int y = 0; y < gridHeight - 1; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                grid[x, y] = grid[x, y + 1];
                
                // Update grid position if there's a bubble
                if (grid[x, y] != null)
                {
                    grid[x, y].GridPosition = new Vector2Int(x, y);
                    
                    // Update world position
                    float yPos = y * bubbleSize * 0.85f;
                    float xOffset = (y % 2 == 0) ? 0.5f : 0f;
                    float xPos = (x + xOffset) * bubbleSize;
                    
                    grid[x, y].transform.position = new Vector3(xPos, yPos, 0);
                }
            }
        }
        
        // Add new row at the top
        int topRow = gridHeight - 1;
        
        for (int x = 0; x < gridWidth; x++)
        {
            // Clear old bubble if exists
            if (grid[x, topRow] != null)
            {
                Bubble oldBubble = grid[x, topRow];
                activeBubbles.Remove(oldBubble);
                Destroy(oldBubble.gameObject);
                grid[x, topRow] = null;
            }
            
            // Randomly skip some positions
            if (UnityEngine.Random.value < 0.3f)
                continue;
                
            CreateBubbleInGrid(x, topRow);
        }
    }
    
    public override void PauseGame()
    {
        if (!isGameActive) return;
        
        isGamePaused = true;
        
        OnGamePaused?.Invoke();
    }
    
    public override void ResumeGame()
    {
        if (!isGameActive) return;
        
        isGamePaused = false;
        
        OnGameResumed?.Invoke();
    }
    
    public override void EndGame()
    {
        if (!isGameActive) return;
        
        // Call base class finalization
        FinalizeGame();
    }
}

// Helper class for bubbles
public enum BubbleType
{
    Normal,
    Bomb,
    Rainbow,
    Star
}

public class Bubble : Obstacle // Reusing the Obstacle class as a base
{
    [SerializeField] private SpriteRenderer bubbleRenderer;
    [SerializeField] private Animator animator;
    
    private BubbleType bubbleType;
    private Vector2Int gridPosition;
    
    public BubbleType Type => bubbleType;
    public Vector2Int GridPosition
    {
        get => gridPosition;
        set => gridPosition = value;
    }
    
    public void Initialize(Color color, BubbleType type, Vector2Int position)
    {
        // Set color
        obstacleColor = color;
        bubbleRenderer.color = color;
        
        // Set type and adjust appearance
        bubbleType = type;
        
        // Store grid position
        gridPosition = position;
        
        // Apply visual effects based on bubble type
        switch (type)
        {
            case BubbleType.Bomb:
                // Make bubble darker and add bomb visual
                bubbleRenderer.color = new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f);
                // In a real game, you'd add additional visual elements
                break;
                
            case BubbleType.Rainbow:
                // Rainbow bubbles have a special shifting color effect
                bubbleRenderer.color = Color.white; // Base color
                // In a real game, you'd add a rainbow shader or animation
                break;
                
            case BubbleType.Star:
                // Star bubbles have a glowing effect
                bubbleRenderer.color = new Color(color.r * 1.5f, color.g * 1.5f, color.b * 1.5f);
                // In a real game, you'd add a star shape or glow effect
                break;
        }
    }
    
    public void PlayPopAnimation()
    {
        // Play animation if available
        if (animator != null)
        {
            animator.SetTrigger("Pop");
        }
        else
        {
            // Simple scale animation as fallback
            StartCoroutine(ScaleAnimation());
        }
    }
    
    private IEnumerator ScaleAnimation()
    {
        float duration = 0.2f;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            
            // Scale up then down
            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.5f;
            transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        transform.localScale = Vector3.zero;
    }
}