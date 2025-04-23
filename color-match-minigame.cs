// ColorMatchMiniGame.cs - A color matching puzzle mini-game
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorMatchMiniGame : MiniGameManager.MiniGameBase
{
    [Header("Grid Settings")]
    [SerializeField] private int gridSizeX = 6;
    [SerializeField] private int gridSizeY = 8;
    [SerializeField] private float cellSize = 60f;
    [SerializeField] private float cellSpacing = 5f;
    
    [Header("Gameplay")]
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private int minMatchSize = 3;
    [SerializeField] private int baseScorePerTile = 10;
    [SerializeField] private int comboMultiplier = 5;
    
    [Header("Color Settings")]
    [SerializeField] private Color[] tileColors;
    
    [Header("UI References")]
    [SerializeField] private RectTransform gridContainer;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button endGameButton;
    
    // Game state
    private ColorTile[,] grid;
    private bool isGameActive = false;
    private int currentScore = 0;
    private float remainingTime;
    private int currentCombo = 0;
    private int maxCombo = 0;
    private ColorTile selectedTile;
    
    // Animation
    private List<ColorTile> tilesToClear = new List<ColorTile>();
    private bool isAnimating = false;
    
    private void Start()
    {
        if (endGameButton != null)
        {
            endGameButton.onClick.AddListener(OnEndGameButtonClicked);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    public override void Initialize(PetBase pet)
    {
        base.Initialize(pet);
        
        // Initialize UI
        if (scoreText) scoreText.text = "0";
        if (comboText) comboText.text = "x1";
        
        remainingTime = gameDuration;
        
        if (timeBonus > 0)
        {
            // Apply time bonus from pet abilities
            remainingTime += gameDuration * timeBonus;
        }
        
        if (timeText) timeText.text = Mathf.CeilToInt(remainingTime).ToString();
        if (timeSlider) timeSlider.value = 1f;
        
        currentScore = 0;
        currentCombo = 0;
        maxCombo = 0;
        
        // Create the grid
        CreateGrid();
        
        // Start the game
        isGameActive = true;
        
        // Pet-specific welcome message
        if (pet is ColorDragon)
        {
            UIManager.Instance.ShowMessage("Your dragon's color-changing ability will help you!");
        }
        else if (pet is PuzzleFox)
        {
            UIManager.Instance.ShowMessage("Your fox's puzzle skills give you a bonus!");
        }
    }
    
    private void Update()
    {
        if (!isGameActive) return;
        
        // Update timer
        remainingTime -= Time.deltaTime;
        
        if (timeText) timeText.text = Mathf.CeilToInt(remainingTime).ToString();
        if (timeSlider) timeSlider.value = Mathf.Clamp01(remainingTime / gameDuration);
        
        // Check for game over
        if (remainingTime <= 0)
        {
            EndGame();
        }
        
        // Check for match input
        if (Input.GetMouseButtonDown(0) && !isAnimating)
        {
            HandleTouchInput();
        }
        
        // Update combo text
        if (comboText && currentCombo > 1)
        {
            comboText.text = "x" + currentCombo;
        }
        else if (comboText)
        {
            comboText.text = "x1";
        }
    }
    
    private void CreateGrid()
    {
        // Create the tile grid
        grid = new ColorTile[gridSizeX, gridSizeY];
        
        // Calculate total width and height
        float totalWidth = gridSizeX * (cellSize + cellSpacing) - cellSpacing;
        float totalHeight = gridSizeY * (cellSize + cellSpacing) - cellSpacing;
        
        // Set container size
        if (gridContainer)
        {
            gridContainer.sizeDelta = new Vector2(totalWidth, totalHeight);
        }
        
        // Starting position (top-left of grid)
        Vector2 startPos = new Vector2(-totalWidth / 2 + cellSize / 2, totalHeight / 2 - cellSize / 2);
        
        // Create individual tiles
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                CreateTile(x, y, startPos);
            }
        }
        
        // Check for initial matches and shift tiles if needed
        StartCoroutine(CheckInitialMatches());
    }
    
    private void CreateTile(int x, int y, Vector2 startPos)
    {
        GameObject tileObj = new GameObject("Tile_" + x + "_" + y);
        tileObj.transform.SetParent(gridContainer.transform, false);
        
        // Position
        RectTransform rectTransform = tileObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
        float posX = startPos.x + x * (cellSize + cellSpacing);
        float posY = startPos.y - y * (cellSize + cellSpacing);
        rectTransform.anchoredPosition = new Vector2(posX, posY);
        
        // Image component
        Image tileImage = tileObj.AddComponent<Image>();
        
        // Button component for interaction
        Button tileButton = tileObj.AddComponent<Button>();
        ColorBlock colors = tileButton.colors;
        colors.disabledColor = Color.white; // No color change when disabled
        tileButton.colors = colors;
        
        // Create tile data
        ColorTile tile = new ColorTile();
        tile.gameObject = tileObj;
        tile.image = tileImage;
        tile.button = tileButton;
        tile.gridX = x;
        tile.gridY = y;
        
        // Randomly assign a color
        AssignRandomColor(tile);
        
        // Add to grid
        grid[x, y] = tile;
        
        // Add button click handler
        tileButton.onClick.AddListener(() => OnTileClicked(tile));
    }
    
    private void AssignRandomColor(ColorTile tile)
    {
        if (tileColors.Length == 0) return;
        
        int randomIndex = Random.Range(0, tileColors.Length);
        tile.colorIndex = randomIndex;
        tile.image.color = tileColors[randomIndex];
    }
    
    private IEnumerator CheckInitialMatches()
    {
        bool foundMatch;
        
        do
        {
            foundMatch = false;
            
            // Check for matches
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    List<ColorTile> horizontalMatches = CheckHorizontalMatch(x, y);
                    List<ColorTile> verticalMatches = CheckVerticalMatch(x, y);
                    
                    if (horizontalMatches.Count >= minMatchSize || verticalMatches.Count >= minMatchSize)
                    {
                        // Found a match, replace tiles
                        foreach (ColorTile tile in horizontalMatches)
                        {
                            AssignRandomColor(tile);
                        }
                        
                        foreach (ColorTile tile in verticalMatches)
                        {
                            // Avoid duplicates
                            if (!horizontalMatches.Contains(tile))
                            {
                                AssignRandomColor(tile);
                            }
                        }
                        
                        foundMatch = true;
                    }
                }
            }
            
            yield return null;
            
        } while (foundMatch);
    }
    
    private void HandleTouchInput()
    {
        if (!isGameActive) return;
        
        // Check for click/touch
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            ProcessInput(touch.position);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            ProcessInput(Input.mousePosition);
        }
    }
    
    private void ProcessInput(Vector2 inputPosition)
    {
        // Convert to canvas space if necessary
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridContainer, inputPosition, Camera.main, out Vector2 localPoint);
        
        // Find which tile was clicked
        float halfWidth = (gridSizeX * (cellSize + cellSpacing) - cellSpacing) / 2;
        float halfHeight = (gridSizeY * (cellSize + cellSpacing) - cellSpacing) / 2;
        
        int x = Mathf.FloorToInt((localPoint.x + halfWidth) / (cellSize + cellSpacing));
        int y = Mathf.FloorToInt((-localPoint.y + halfHeight) / (cellSize + cellSpacing));
        
        // Validate coordinates
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
        {
            OnTileClicked(grid[x, y]);
        }
    }
    
    private void OnTileClicked(ColorTile tile)
    {
        if (!isGameActive || isAnimating) return;
        
        if (selectedTile == null)
        {
            // First selection
            selectedTile = tile;
            selectedTile.image.color = Color.Lerp(tileColors[selectedTile.colorIndex], Color.white, 0.3f);
        }
        else if (selectedTile == tile)
        {
            // Deselect
            selectedTile.image.color = tileColors[selectedTile.colorIndex];
            selectedTile = null;
        }
        else if (AreAdjacent(selectedTile, tile))
        {
            // Try to swap
            StartCoroutine(SwapTiles(selectedTile, tile));
        }
        else
        {
            // Select new tile
            selectedTile.image.color = tileColors[selectedTile.colorIndex];
            selectedTile = tile;
            selectedTile.image.color = Color.Lerp(tileColors[selectedTile.colorIndex], Color.white, 0.3f);
        }
    }
    
    private bool AreAdjacent(ColorTile a, ColorTile b)
    {
        return (Mathf.Abs(a.gridX - b.gridX) == 1 && a.gridY == b.gridY) ||
               (Mathf.Abs(a.gridY - b.gridY) == 1 && a.gridX == b.gridX);
    }
    
    private IEnumerator SwapTiles(ColorTile a, ColorTile b)
    {
        isAnimating = true;
        
        // Reset selection visual
        a.image.color = tileColors[a.colorIndex];
        
        // Store original positions
        Vector2 posA = a.gameObject.GetComponent<RectTransform>().anchoredPosition;
        Vector2 posB = b.gameObject.GetComponent<RectTransform>().anchoredPosition;
        
        // Animate swap
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            a.gameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(posA, posB, t);
            b.gameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(posB, posA, t);
            
            yield return null;
        }
        
        // Swap in grid
        int tempX = a.gridX;
        int tempY = a.gridY;
        
        grid[a.gridX, a.gridY] = b;
        grid[b.gridX, b.gridY] = a;
        
        b.gridX = tempX;
        b.gridY = tempY;
        a.gridX = b.gridX;
        a.gridY = b.gridY;
        
        // Check for matches
        List<ColorTile> matches = CheckMatches();
        
        if (matches.Count > 0)
        {
            // Valid move - clear matches
            yield return StartCoroutine(ClearMatches(matches));
        }
        else
        {
            // Invalid move - swap back
            yield return StartCoroutine(SwapTiles(b, a));
            currentCombo = 0;
        }
        
        selectedTile = null;
        isAnimating = false;
    }
    
    private List<ColorTile> CheckMatches()
    {
        List<ColorTile> allMatches = new List<ColorTile>();
        
        // Check horizontal matches
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                List<ColorTile> horizontalMatches = CheckHorizontalMatch(x, y);
                
                if (horizontalMatches.Count >= minMatchSize)
                {
                    allMatches.AddRange(horizontalMatches);
                    // Skip ahead past this match
                    x += horizontalMatches.Count - 1;
                }
            }
        }
        
        // Check vertical matches
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                List<ColorTile> verticalMatches = CheckVerticalMatch(x, y);
                
                if (verticalMatches.Count >= minMatchSize)
                {
                    // Add only tiles that aren't already in matches
                    foreach (ColorTile tile in verticalMatches)
                    {
                        if (!allMatches.Contains(tile))
                        {
                            allMatches.Add(tile);
                        }
                    }
                    
                    // Skip ahead past this match
                    y += verticalMatches.Count - 1;
                }
            }
        }
        
        return allMatches;
    }
    
    private List<ColorTile> CheckHorizontalMatch(int startX, int y)
    {
        List<ColorTile> matches = new List<ColorTile>();
        
        if (startX >= gridSizeX) return matches;
        
        ColorTile startTile = grid[startX, y];
        matches.Add(startTile);
        
        // Check right
        for (int x = startX + 1; x < gridSizeX; x++)
        {
            if (grid[x, y].colorIndex == startTile.colorIndex)
            {
                matches.Add(grid[x, y]);
            }
            else
            {
                break;
            }
        }
        
        return matches.Count >= minMatchSize ? matches : new List<ColorTile>();
    }
    
    private List<ColorTile> CheckVerticalMatch(int x, int startY)
    {
        List<ColorTile> matches = new List<ColorTile>();
        
        if (startY >= gridSizeY) return matches;
        
        ColorTile startTile = grid[x, startY];
        matches.Add(startTile);
        
        // Check down
        for (int y = startY + 1; y < gridSizeY; y++)
        {
            if (grid[x, y].colorIndex == startTile.colorIndex)
            {
                matches.Add(grid[x, y]);
            }
            else
            {
                break;
            }
        }
        
        return matches.Count >= minMatchSize ? matches : new List<ColorTile>();
    }
    
    private IEnumerator ClearMatches(List<ColorTile> matches)
    {
        if (matches.Count == 0) yield break;
        
        isAnimating = true;
        
        // Increase combo
        currentCombo++;
        maxCombo = Mathf.Max(currentCombo, maxCombo);
        
        // Calculate score
        int matchScore = matches.Count * baseScorePerTile;
        
        // Apply combo multiplier
        if (currentCombo > 1)
        {
            matchScore += matchScore * (currentCombo - 1) * comboMultiplier / 100;
        }
        
        // Apply pet bonus if any
        matchScore = Mathf.RoundToInt(matchScore * scoreMultiplier);
        
        // Add to total score
        currentScore += matchScore;
        
        // Update score text
        if (scoreText) scoreText.text = currentScore.ToString();
        
        // Special effect for pet type
        if (associatedPet is ColorDragon && Random.Range(0, 100) < 30)
        {
            // Dragon special - change adjacent tiles to same color
            foreach (ColorTile tile in matches)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = tile.gridX + dx;
                        int ny = tile.gridY + dy;
                        
                        if (nx >= 0 && nx < gridSizeX && ny >= 0 && ny < gridSizeY &&
                            !matches.Contains(grid[nx, ny]))
                        {
                            // Add extra time for special effect
                            remainingTime += 0.5f;
                        }
                    }
                }
            }
        }
        else if (associatedPet is RainbowBunny && Random.Range(0, 100) < 20)
        {
            // Bunny special - bonus points
            int bonus = matchScore / 2;
            currentScore += bonus;
            
            if (scoreText) scoreText.text = currentScore.ToString();
            UIManager.Instance.ShowMessage("Bunny Bonus: +" + bonus + " points!");
        }
        else if (associatedPet is PuzzleFox && Random.Range(0, 100) < 25)
        {
            // Fox special - extra time
            float timeBonus = 2f;
            remainingTime += timeBonus;
            UIManager.Instance.ShowMessage("Fox Bonus: +" + timeBonus + " seconds!");
        }
        
        // Play clear animation
        foreach (ColorTile tile in matches)
        {
            // Animate tile clear effect
            float duration = 0.3f;
            float elapsed = 0f;
            
            Color originalColor = tile.image.color;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                tile.image.color = Color.Lerp(originalColor, Color.white, t);
                tile.gameObject.GetComponent<RectTransform>().localScale = 
                    Vector3.Lerp(Vector3.one, Vector3.zero, t);
                
                yield return null;
            }
        }
        
        // Replace cleared tiles with new ones
        foreach (ColorTile tile in matches)
        {
            AssignRandomColor(tile);
            tile.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        
        // Check for new matches
        yield return new WaitForSeconds(0.2f);
        
        List<ColorTile> newMatches = CheckMatches();
        
        if (newMatches.Count > 0)
        {
            // Chain reaction! 
            yield return StartCoroutine(ClearMatches(newMatches));
        }
        else
        {
            // No more matches, reset combo
            currentCombo = 0;
        }
        
        isAnimating = false;
    }
    
    private void EndGame()
    {
        isGameActive = false;
        
        // Set final score based on original score * pet bonus
        int finalScore = CalculateFinalScore(currentScore);
        
        // Update UI
        if (finalScoreText) finalScoreText.text = finalScore.ToString();
        if (gameOverPanel) gameOverPanel.SetActive(true);
        
        // Call the base class method to report the score
        EndGame(finalScore);
    }
    
    private void OnEndGameButtonClicked()
    {
        EndGame();
    }
    
    // Tile class
    private class ColorTile
    {
        public GameObject gameObject;
        public Image image;
        public Button button;
        public int gridX;
        public int gridY;
        public int colorIndex;
    }
}