using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;   // The enemy prefab
    [Range(0, 100)] public float spawnChance; // The chance that this enemy will spawn
}

public class Tile : MonoBehaviour
{
    public bool canMoveUp;
    public bool canMoveDown;
    public bool canMoveLeft;
    public bool canMoveRight;

    private Vector2 position; // The grid position of the tile

    // Path prefabs for different directions
    public GameObject pathUpPrefab;
    public GameObject pathDownPrefab;
    public GameObject pathLeftPrefab;
    public GameObject pathRightPrefab;

    // List of possible enemies to spawn on this tile
    public List<EnemySpawnInfo> enemySpawnInfos = new List<EnemySpawnInfo>();
    private List<GameObject> spawnedEnemies = new List<GameObject>(); // List of spawned enemies

    // Initialize the tile's paths; ensure backtracking path is always open
    public void InitializeTile(bool startingTile, Vector2 entryDirection)
    {
        if (startingTile)
        {
            // For the starting tile, only allow upward movement
            canMoveUp = true;
            canMoveDown = false;
            canMoveLeft = false;
            canMoveRight = false;
        }
        else
        {
            // Randomize paths only once when the tile is first generated
            RandomizePaths();

            // Ensure the path back to the previous tile is open based on entry direction
            if (entryDirection == Vector2.up) canMoveDown = true;
            else if (entryDirection == Vector2.down) canMoveUp = true;
            else if (entryDirection == Vector2.left) canMoveRight = true;
            else if (entryDirection == Vector2.right) canMoveLeft = true;

            // Spawn enemies on non-starting tiles
            SpawnEnemies();
        }

        // Spawn paths based on the available directions
        SpawnPaths();
    }

    private void RandomizePaths()
    {
        // Randomize the tile's paths (only called once upon initial tile generation)
        canMoveUp = Random.value > 0.5f;
        canMoveDown = Random.value > 0.5f;
        canMoveLeft = Random.value > 0.5f;
        canMoveRight = Random.value > 0.5f;
    }

    // Spawn the paths based on the allowed directions
    private void SpawnPaths()
    {
        float halfTileSize = 5f; // Half the size of the tile (10 units), used to position paths correctly

        if (canMoveUp && pathUpPrefab != null)
        {
            Vector3 pathPosition = transform.position + new Vector3(0, halfTileSize, 0); // Position path halfway up the tile
            GameObject path = Instantiate(pathUpPrefab, pathPosition, Quaternion.identity, transform);
            path.transform.localScale = new Vector3(0.1f, 1, 1); // Set scale to (0.1, 1, 1)
        }
        if (canMoveDown && pathDownPrefab != null)
        {
            Vector3 pathPosition = transform.position + new Vector3(0, -halfTileSize, 0); // Position path halfway down the tile
            GameObject path = Instantiate(pathDownPrefab, pathPosition, Quaternion.identity, transform);
            path.transform.localScale = new Vector3(0.1f, 1, 1); // Set scale to (0.1, 1, 1)
        }
        if (canMoveLeft && pathLeftPrefab != null)
        {
            Vector3 pathPosition = transform.position + new Vector3(-halfTileSize, 0, 0); // Position path halfway left on the tile
            GameObject path = Instantiate(pathLeftPrefab, pathPosition, Quaternion.Euler(0, 0, 90), transform);
            path.transform.localScale = new Vector3(0.1f, 1, 1); // Set scale to (0.1, 1, 1)
        }
        if (canMoveRight && pathRightPrefab != null)
        {
            Vector3 pathPosition = transform.position + new Vector3(halfTileSize, 0, 0); // Position path halfway right on the tile
            GameObject path = Instantiate(pathRightPrefab, pathPosition, Quaternion.Euler(0, 0, -90), transform);
            path.transform.localScale = new Vector3(0.1f, 1, 1); // Set scale to (0.1, 1, 1)
        }
    }

    // Spawn enemies based on their spawn chances
    private void SpawnEnemies()
    {
        foreach (var enemyInfo in enemySpawnInfos)
        {
            if (Random.Range(0f, 100f) <= enemyInfo.spawnChance)
            {
                Vector3 enemyPosition = transform.position; // Spawn enemies at the center of the tile
                GameObject spawnedEnemy = Instantiate(enemyInfo.enemyPrefab, enemyPosition, Quaternion.identity, transform);
                spawnedEnemies.Add(spawnedEnemy);
            }
        }
    }

    // Check if the tile has any enemies
    public bool HasEnemy()
    {
        return spawnedEnemies.Count > 0;
    }

    // Get the list of spawned enemies
    public List<GameObject> GetEnemies()
    {
        return spawnedEnemies;
    }

    // Save the current state of the tile's paths for future restoration
    public TileState SaveState()
    {
        return new TileState
        {
            Position = position,
            CanMoveUp = canMoveUp,
            CanMoveDown = canMoveDown,
            CanMoveLeft = canMoveLeft,
            CanMoveRight = canMoveRight
        };
    }

    // Restore the tile's paths from a saved state
    public void RestoreState(TileState state)
    {
        canMoveUp = state.CanMoveUp;
        canMoveDown = state.CanMoveDown;
        canMoveLeft = state.CanMoveLeft;
        canMoveRight = state.CanMoveRight;
        position = state.Position;

        // Ensure paths are spawned based on restored states
        SpawnPaths();
    }

    // Set the grid position of the tile
    public void SetPosition(Vector2 newPosition)
    {
        position = newPosition;
    }

    public Vector2 GetPosition()
    {
        return position;
    }
}
