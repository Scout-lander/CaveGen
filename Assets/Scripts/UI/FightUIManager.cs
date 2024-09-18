using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.SceneManagement; // Import SceneManagement for restarting the game
using System.Collections.Generic; // Needed for List<>

public class FightUIManager : MonoBehaviour
{
    public GameObject fightPanel;       // The fight UI panel
    public Button attackButton;         // Attack button
    public Button healButton;           // Heal button
    public TMP_Text combatLogText;      // TMP_Text for combat log
    public GameObject moveButtons;      // Parent GameObject for movement buttons
    public Button closeFightUIButton;   // Button to close the fight UI

    // UI elements for displaying player
    public Image playerImage;           // Image component for displaying the player
    public TMP_Text playerNameText;     // TMP_Text to display player's name
    public TMP_Text playerHealthText;   // TMP_Text to display player's health

    // UI elements for displaying enemy
    public Image enemyImage;            // Image component for displaying the enemy
    public TMP_Text enemyNameText;      // TMP_Text to display enemy's name
    public TMP_Text enemyHealthText;    // TMP_Text to display enemy's health

    // UI elements for death message and restart functionality
    public TMP_Text deathMessageText;   // TMP_Text to display "You Died" message
    public Button restartButton;        // Button to restart the game

    private PlayerStats playerStats;    // Reference to the player's stats
    private List<EnemyStats> enemyStatsList; // Reference to the list of enemies
    private int currentEnemyIndex = 0;  // Track which enemy is currently being fought
    private CharacterMovement characterMovement; // Reference to the CharacterMovement script

    public bool InCombat;

    void Start()
    {
        // Assign button listeners
        attackButton.onClick.AddListener(OnAttackButtonClicked);
        healButton.onClick.AddListener(OnHealButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        closeFightUIButton.onClick.AddListener(CloseFightUI);

        // Initially hide the fight panel, death message, and restart button
        fightPanel.SetActive(false);
        deathMessageText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        closeFightUIButton.gameObject.SetActive(false); 

        // Find the CharacterMovement component on the player
        characterMovement = FindObjectOfType<CharacterMovement>();
    }

    // Method to initialize and show the fight UI for multiple enemies
    public void StartFight(PlayerStats player, List<EnemyStats> enemies)
    {
        //ClearCombatLog(); // Clear combat log when the fight Starts
        playerStats = player;
        enemyStatsList = enemies;
        currentEnemyIndex = 0; // Start with the first enemy
        InCombat = true;

        // Update the UI with player and enemy details
        UpdatePlayerUI();
        UpdateEnemyUI();

        // Show the fight UI and hide movement buttons
        fightPanel.SetActive(true);
        moveButtons.SetActive(false);

        // Log initial combat message
        UpdateCombatLog("A wild " + enemyStatsList[currentEnemyIndex].name + " appears!");
    }

    // Update the player UI elements with the player's data
    private void UpdatePlayerUI()
    {
        if (playerStats != null)
        {
            playerImage.sprite = playerStats.GetComponent<SpriteRenderer>().sprite; // Display player sprite
            playerNameText.text = playerStats.name; // Display player name
            playerHealthText.text = "Health: " + playerStats.currentHealth; // Display player health
        }
    }

    // Update the enemy UI elements with the current enemy's data
    private void UpdateEnemyUI()
    {
        if (currentEnemyIndex < enemyStatsList.Count)
        {
            var currentEnemy = enemyStatsList[currentEnemyIndex];
            if (currentEnemy != null && currentEnemy.gameObject != null) // Check if the enemy still exists
            {
                enemyImage.sprite = currentEnemy.GetComponent<SpriteRenderer>().sprite;
                enemyNameText.text = currentEnemy.name;
                enemyHealthText.text = "Health: " + currentEnemy.currentHealth;
            }
        }
    }

    // Handle the attack button click
    private void OnAttackButtonClicked()
    {
        if (playerStats != null && enemyStatsList.Count > currentEnemyIndex)
        {
            EnemyStats currentEnemy = enemyStatsList[currentEnemyIndex];

            // Player attacks the current enemy
            playerStats.Attack(currentEnemy);
            UpdateCombatLog("Player attacks " + currentEnemy.name + " for " + playerStats.attackPower + " damage.");
            UpdateEnemyHealthUI(); // Update enemy health display

            // Check if the current enemy is still alive
            if (currentEnemy.currentHealth > 0)
            {
                // Enemy attacks back
                currentEnemy.Attack(playerStats);
                UpdateCombatLog(currentEnemy.name + " attacks player for " + currentEnemy.attackPower + " damage.");
                UpdatePlayerHealthUI(); // Update player health display
            }

            // Check the combat result
            CheckCombatResult();
        }
    }

    // Handle the heal button click
    private void OnHealButtonClicked()
    {
        if (playerStats != null)
        {
            // Heal the player (for example, heal 20 health points)
            playerStats.currentHealth = Mathf.Min(playerStats.currentHealth + 20, playerStats.maxHealth);
            UpdateCombatLog("Player heals for 20 health. Current health: " + playerStats.currentHealth);
            UpdatePlayerHealthUI(); // Update player health display

            // Enemy's turn after healing
            if (enemyStatsList[currentEnemyIndex].currentHealth > 0)
            {
                enemyStatsList[currentEnemyIndex].Attack(playerStats);
                UpdateCombatLog(enemyStatsList[currentEnemyIndex].name + " attacks player for " + enemyStatsList[currentEnemyIndex].attackPower + " damage.");
                UpdatePlayerHealthUI(); // Update player health display
            }

            // Check the combat result
            CheckCombatResult();
        }
    }

    // Update the combat log text using TMP_Text
    private void UpdateCombatLog(string message)
    {
        combatLogText.text += message + "\n"; // Update TMP text
    }

    // Clear the combat log text
    private void ClearCombatLog()
    {
        combatLogText.text = ""; // Clear the TMP text
    }

    // Update the player's health UI
    private void UpdatePlayerHealthUI()
    {
        if (playerHealthText != null && playerStats != null)
        {
            playerHealthText.text = "Health: " + playerStats.currentHealth; // Update health display
        }
    }

    // Update the enemy's health UI
    private void UpdateEnemyHealthUI()
    {
        if (enemyHealthText != null && enemyStatsList.Count > currentEnemyIndex)
        {
            enemyHealthText.text = "Health: " + enemyStatsList[currentEnemyIndex].currentHealth; // Update health display
        }
    }

    // Check the result of the combat and handle end of fight
    private void CheckCombatResult()
    {
        if (playerStats.currentHealth <= 0)
        {
            UpdateCombatLog("Player has been defeated!");
            ShowDeathMessage(); // Show the "You Died" message and restart button
        }
        else if (enemyStatsList[currentEnemyIndex].currentHealth <= 0)
        {
            UpdateCombatLog(enemyStatsList[currentEnemyIndex].name + " has been defeated!");

            currentEnemyIndex++; // Move to the next enemy

            // Check if there are more enemies to fight
            if (currentEnemyIndex < enemyStatsList.Count)
            {
                UpdateEnemyUI(); // Update the UI with the new enemy
                UpdateCombatLog("A wild " + enemyStatsList[currentEnemyIndex].name + " appears!");
            }
            else
            {
                // All enemies are defeated
                EndFight(); // End the fight after all enemies are defeated
            }
        }
    }

    // Show the "You Died" message and restart button
    private void ShowDeathMessage()
    {
        deathMessageText.gameObject.SetActive(true);
        deathMessageText.text = "You Died"; // Display death message
        restartButton.gameObject.SetActive(true); // Show restart button

        // Hide other UI elements related to combat
        attackButton.gameObject.SetActive(false);
        healButton.gameObject.SetActive(false);
    }

    // Handle the restart button click
    private void OnRestartButtonClicked()
    {
        // Clear combat log before restarting
        //ClearCombatLog();

        // Reload the current scene to restart the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Method to handle when an enemy dies
    public void OnEnemyDeath(EnemyStats enemy)
    {
        EndFight();
        
    }

    // End the fight and hide the fight UI
    private void EndFight()
    {
        //ClearCombatLog(); // Clear combat log when the fight ends
        fightPanel.SetActive(false);  // Hide the fight UI
        moveButtons.SetActive(true);  // Show movement buttons again
        InCombat = false;

        // Safely destroy any remaining enemy game objects (after UI is cleared)
        foreach (var enemy in enemyStatsList)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                Destroy(enemy.gameObject); // Destroy any remaining enemy game objects
            }
        }

        // If the player is not at the center of the tile, continue moving them to the center
        if (characterMovement != null)
        {
            characterMovement.MoveToTileCenterIfNeeded();
        }

        // Optionally, you can enable the "Close Fight UI" button if you want manual control over closing the fight panel
        closeFightUIButton.gameObject.SetActive(true); // Show the close button
    }

    private void CloseFightUI()
    {
        fightPanel.SetActive(false);
        moveButtons.SetActive(true);
        closeFightUIButton.gameObject.SetActive(false); // Hide the close button
        ClearCombatLog(); // Clear combat log 
    }
}
