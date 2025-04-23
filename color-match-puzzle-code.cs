// ColorMatchPuzzle.cs - Match colored blocks in unique patterns with power-ups and daily challenges
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorMatchPuzzle : MiniGameBase
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 8;
    [SerializeField] private float cellSize = 1.0f;
    [SerializeField] private Transform gridContainer;
    
    [Header("Block Settings")]
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private List<Color> blockColors = new List<Color>();
    [SerializeField] private float matchAnimationDuration = 0.3f;
    [SerializeField] private float fallAnimationDuration = 0.5f;
    
    [Header("Power-up Settings")]
    [SerializeField] private float powerUpChance = 0.05f; // 5% chance
    [SerializeField] private GameObject colorBombPrefab;
    [SerializeField] private GameObject rowClearerPrefab;
    [SerializeField] private GameObject columnClearerPrefab;
    
    // Grid data
    private ColorBlock[,] grid;
    private bool isInputAllowed = false;
    private ColorBlock selectedBlock;
    private Vector2Int selectedPosition;
    
    // Scoring
    [SerializeField] private int baseMatchScore = 10;
    [SerializeField] private int bonusPerExtraBlock = 5;
    
    // State tracking
    private bool isAnimating = false;
    private int comboCount = 0;
    
    // Prefab pool
    private List<ColorBlock> blockPool = new List<ColorBlock>();
    
    public override void StartGame()
    {
        // Init game state
        InitializeGame();
        
        // Create grid
        CreateGrid();
        
        // Check for any initial matches and clear them
        StartCoroutine(InitialGridSetup());
    }
    
    private IEnumerator InitialGridSetup()
    {
        yield return new WaitForSeconds(0.5f);
        
        // Check for initial matches and resolve them
        while (FindAllMatches().Count > 0)
        {
            yield return StartCoroutine(ClearMatchesAndRefill());
            yield return new WaitForSeconds(0.3f);
        }
        
        // Allow input
        isInputAllowed = true;
    }
    
    private void CreateGrid()
    {
        // Create grid array
        grid = new ColorBlock[gridWidth, gridHeight];
        
        // Clear any existing grid elements
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create new grid
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                CreateBlockAt(x, y);
            }
        }
    }
    
    private void CreateBlockAt(int x, int y)
    {
        // Calculate position
        Vector3 position = new Vector3(x * cellSize, y * cellSize, 0);
        
        // Create block instance
        ColorBlock block;
        
        // Try to reuse a block from the pool
        if (blockPool.Count > 0)
        {
            block = blockPool[0];
            blockPool.RemoveAt(0);
            block.transform.position = position;
            block.gameObject.SetActive(true);
        }
        else
        {
            GameObject blockObj = Instantiate(blockPrefab, position, Quaternion.identity, gridContainer);
            block = blockObj.GetComponent<ColorBlock>();
            
            if (block == null)
            {
                Debug.LogError("Block prefab does not have ColorBlock component!");
                return;
            }
        }
        
        // Assign random color
        int colorIndex = UnityEngine.Random.Range(0, blockColors.Count);
        block.SetColor(blockColors[colorIndex]);
        
        // Assign to grid
        grid[x, y] = block;
        
        // Add click handler
        block.OnBlockClicked += () => OnBlockClicked(x, y);
    }
    
    private void OnBlockClicked(int x, int y)
    {
        if (!isGameActive || isGamePaused || !isInputAllowed || isAnimating)
            return;
            
        // If no block is selected, select this one
        if (selectedBlock == null)
        {
            selectedBlock = grid[x, y];
            selectedPosition = new Vector2Int(x, y);
            selectedBlock.SetSelected(true);
        }
        else
        {
            // Check if adjacent
            if (IsAdjacent(selectedPosition, new Vector2Int(x, y)))
            {
                // Swap blocks
                StartCoroutine(SwapBlocks(selectedPosition, new Vector2Int(x, y)));
            }
            else
            {
                // Deselect and select new
                selectedBlock.SetSelected(false);
                selectedBlock = grid[x, y];
                selectedPosition = new Vector2Int(x, y);
                selectedBlock.SetSelected(true);
            }
        }
    }
    
    private bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y) == 1;
    }
    
    private IEnumerator SwapBlocks(Vector2Int pos1, Vector2Int pos2)
    {
        isAnimating = true;
        isInputAllowed = false;
        
        // Deselect
        selectedBlock.SetSelected(false);
        selectedBlock = null;
        
        // Get blocks
        ColorBlock block1 = grid[pos1.x, pos1.y];
        ColorBlock block2 = grid[pos2.x, pos2.y];
        
        // Animate swap
        float timer = 0f;
        Vector3 startPos1 = block1.transform.position;
        Vector3 startPos2 = block2.transform.position;
        Vector3 targetPos1 = block2.transform.position;
        Vector3 targetPos2 = block1.transform.position;
        
        while (timer < matchAnimationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / matchAnimationDuration;
            
            block1.transform.position = Vector3.Lerp(startPos1, targetPos1, t);
            block2.transform.position = Vector3.Lerp(startPos2, targetPos2, t);
            
            yield return null;
        }
        
        // Update grid
        grid[pos1.x, pos1.y] = block2;
        grid[pos2.x, pos2.y] = block1;
        
        // Check for matches
        List<List<Vector2Int>> matches = FindAllMatches();
        
        if (matches.Count > 0)
        {
            // Found matches, clear and refill
            yield return StartCoroutine(ClearMatchesAndRefill());
        }
        else
        {
            // No matches, swap back
            timer = 0f;
            while (timer < matchAnimationDuration)
            {
                timer += Time.deltaTime;
                float t = timer / matchAnimationDuration;
                
                block1.transform.position = Vector3.Lerp(targetPos1, startPos1, t);
                block2.transform.position = Vector3.Lerp(targetPos2, startPos2, t);
                
                yield return null;
            }
            
            // Restore grid
            grid[pos1.x, pos1.y] = block1;
            grid[pos2.x, pos2.y] = block2;
        }
        
        isAnimating = false;
        isInputAllowed = true;
    }
    
    private List<List<Vector2Int>> FindAllMatches()
    {
        List<List<Vector2Int>> allMatches = new List<List<Vector2Int>>();
        
        // Check horizontal matches
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth - 2; x++)
            {
                Color color = grid[x, y].GetColor();
                if (grid[x + 1, y].GetColor() == color && grid[x + 2, y].GetColor() == color)
                {
                    List<Vector2Int> match = new List<Vector2Int> { 
                        new Vector2Int(x, y), 
                        new Vector2Int(x + 1, y), 
                        new Vector2Int(x + 2, y) 
                    };
                    
                    // Check for longer matches
                    int nextX = x + 3;
                    while (nextX < gridWidth && grid[nextX, y].GetColor() == color)
                    {
                        match.Add(new Vector2Int(nextX, y));
                        nextX++;
                    }
                    
                    allMatches.Add(match);
                    
                    // Skip ahead to avoid duplicate matches
                    x = nextX - 3;
                }
            }
        }
        
        // Check vertical matches
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight - 2; y++)
            {
                Color color = grid[x, y].GetColor();
                if (grid[x, y + 1].GetColor() == color && grid[x, y + 2].GetColor() == color)
                {
                    List<Vector2Int> match = new List<Vector2Int> { 
                        new Vector2Int(x, y), 
                        new Vector2Int(x, y + 1), 
                        new Vector2Int(x, y + 2) 
                    };
                    
                    // Check for longer matches
                    int nextY = y + 3;
                    while (nextY < gridHeight && grid[x, nextY].GetColor() == color)
                    {
                        match.Add(new Vector2Int(x, nextY));
                        nextY++;
                    }
                    
                    allMatches.Add(match);
                    
                    // Skip ahead to avoid duplicate matches
                    y = nextY - 3;
                }
            }
        }
        
        return allMatches;
    }
    
    private IEnumerator ClearMatchesAndRefill()
    {
        isAnimating = true;
        
        // Find all matches
        List<List<Vector2Int>> matches = FindAllMatches();
        
        if (matches.Count > 0)
        {
            comboCount++;
            
            // Calculate score for this combo
            int totalBlocks = 0;
            foreach (var match in matches)
            {
                totalBlocks += match.Count;
            }
            
            int matchScore = baseMatchScore * matches.Count;
            int bonusScore = bonusPerExtraBlock * (totalBlocks - (matches.Count * 3));
            int comboBonus = comboCount > 1 ? comboCount * 10 : 0;
            int totalScore = matchScore + bonusScore + comboBonus;
            
            AddScore(totalScore);
            
            // Show score popup at a random match position
            // (Implementation would depend on UI setup)
            
            // Clear matched blocks
            foreach (var match in matches)
            {
                foreach (var pos in match)
                {
                    // Check for potential power-up generation
                    bool createPowerUp = UnityEngine.Random.value <= powerUpChance && match.Count >= 4;
                    
                    if (createPowerUp)
                    {
                        // Create appropriate power-up based on match length and orientation
                        // (Implementation would depend on power-up system)
                    }
                    else
                    {
                        // Return block to pool
                        grid[pos.x, pos.y].gameObject.SetActive(false);
                        blockPool.Add(grid[pos.x, pos.y]);
                        grid[pos.x, pos.y] = null;
                    }
                }
            }
            
            yield return new WaitForSeconds(matchAnimationDuration);
            
            // Refill the grid
            yield return StartCoroutine(RefillGrid());
            
            // Check for new matches that may have formed
            matches = FindAllMatches();
            if (matches.Count > 0)
            {
                yield return StartCoroutine(ClearMatchesAndRefill());
            }
            else
            {
                comboCount = 0;
            }
        }
        
        isAnimating = false;
    }
    
    private IEnumerator RefillGrid()
    {
        // Process columns from bottom to top
        for (int x = 0; x < gridWidth; x++)
        {
            // Count empty spaces in this column
            int emptySpaces = 0;
            
            // Move existing blocks down
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == null)
                {
                    emptySpaces++;
                }
                else if (emptySpaces > 0)
                {
                    // Move this block down by emptySpaces
                    ColorBlock block = grid[x, y];
                    Vector3 startPos = block.transform.position;
                    Vector3 targetPos = new Vector3(x * cellSize, (y - emptySpaces) * cellSize, 0);
                    
                    grid[x, y - emptySpaces] = block;
                    grid[x, y] = null;
                    
                    // Animate the fall
                    float timer = 0f;
                    while (timer < fallAnimationDuration)
                    {
                        timer += Time.deltaTime;
                        float t = timer / fallAnimationDuration;
                        
                        block.transform.position = Vector3.Lerp(startPos, targetPos, t);
                        
                        yield return null;
                    }
                    
                    block.transform.position = targetPos;
                }
            }
            
            // Create new blocks at the top
            for (int i = 0; i < emptySpaces; i++)
            {
                int y = gridHeight - 1 - i;
                CreateBlockAt(x, y);
                
                // Animate from above the grid
                ColorBlock block = grid[x, y];
                Vector3 startPos = new Vector3(x * cellSize, gridHeight * cellSize, 0);
                Vector3 targetPos = new Vector3(x * cellSize, y * cellSize, 0);
                
                block.transform.position = startPos;
                
                float timer = 0f;
                while (timer < fallAnimationDuration)
                {
                    timer += Time.deltaTime;
                    float t = timer / fallAnimationDuration;
                    
                    block.transform.position = Vector3.Lerp(startPos, targetPos, t);
                    
                    yield return null;
                }
                
                block.transform.position = targetPos;
            }
        }
    }
    
    public override void PauseGame()
    {
        if (!isGameActive) return;
        
        isGamePaused = true;
        isInputAllowed = false;
        
        OnGamePaused?.Invoke();
    }
    
    public override void ResumeGame()
    {
        if (!isGameActive) return;
        
        isGamePaused = false;
        isInputAllowed = true;
        
        OnGameResumed?.Invoke();
    }
    
    public override void EndGame()
    {
        if (!isGameActive) return;
        
        isInputAllowed = false;
        
        // Call base class finalization
        FinalizeGame();
    }
}

// Helper class for the colored blocks
public class ColorBlock : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject selectionIndicator;
    
    private Color blockColor;
    
    public event Action OnBlockClicked;
    
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);
    }
    
    public void SetColor(Color color)
    {
        blockColor = color;
        spriteRenderer.color = color;
    }
    
    public Color GetColor()
    {
        return blockColor;
    }
    
    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null)
            selectionIndicator.SetActive(selected);
    }
    
    public void OnMouseDown()
    {
        OnBlockClicked?.Invoke();
    }
}
