using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int maxHealth = 50;   // Maximum health of the enemy
    public int currentHealth;    // Current health of the enemy
    public int attackPower = 15; // Attack power of the enemy
    public int defense = 5;      // Defense of the enemy

    public float minAttackInterval = 1.5f; // Minimum time between attacks
    public float maxAttackInterval = 3f;   // Maximum time between attacks

    private PlayerStats playerStats;       // Reference to the player's stats

    void Start()
    {
        // Initialize the enemy's current health
        currentHealth = maxHealth;

        // Find the PlayerStats component in the scene (assuming one player)
        playerStats = FindObjectOfType<PlayerStats>();

        // Start the automatic attack coroutine
        StartCoroutine(AutoAttack());
    }

    // Coroutine to handle automatic attacks at random intervals
    private System.Collections.IEnumerator AutoAttack()
    {
        while (currentHealth > 0 && playerStats.currentHealth > 0)
        {
            // Wait for a random interval between min and max attack intervals
            float waitTime = Random.Range(minAttackInterval, maxAttackInterval);
            yield return new WaitForSeconds(waitTime);

            // Perform the attack if both enemy and player are still alive
            if (playerStats != null && playerStats.currentHealth > 0)
            {
                Attack(playerStats);
                Debug.Log("Enemy attacks player for " + attackPower + " damage.");
            }
        }
    }

    // Method to take damage
    public void TakeDamage(int damage)
    {
        int damageTaken = Mathf.Max(damage - defense, 0); // Calculate damage after defense
        currentHealth -= damageTaken;
        Debug.Log("Enemy took " + damageTaken + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Method to attack the player
    public void Attack(PlayerStats player)
    {
        player.TakeDamage(attackPower);
    }

    // Method called when the enemy's health reaches zero
    private void Die()
    {
        Debug.Log("Enemy has died.");
        // Handle enemy death (e.g., remove from game, drop loot)
        StopAllCoroutines(); // Stop auto-attacks when the enemy dies
        Destroy(gameObject); // Destroy the enemy GameObject
    }
}
