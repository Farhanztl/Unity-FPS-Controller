using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerHealthController - Tracks player health and handles damage from zombie bullets.
/// Attach this to the same GameObject as PlayerController.
/// Optionally wire up a UI Slider for a health bar.
/// </summary>
public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance;

    [Header("Health")]
    public int maxHealth      = 100;
    public int currentHealth;

    [Header("UI (optional)")]
    public Slider healthBarSlider;  // Drag a UI Slider here in the Inspector

    [Header("Hit Flash (optional)")]
    public Image hitFlashImage;     // A full-screen red Image UI element
    public float hitFlashDuration = 0.15f;

    [Header("Death")]
    public GameObject deathScreenUI; // Drag a "You Died" UI panel here (optional)

    private bool isDead = false;

    // ────────────────────────────────────────────────────────────
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    // ────────────────────────────────────────────────────────────
    /// <summary>Call this from BulletController when a zombie bullet hits the player.</summary>
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth  = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        // Red screen flash
        if (hitFlashImage != null)
            StartCoroutine(HitFlash());

        Debug.Log($"[Player] Took {amount} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    // ────────────────────────────────────────────────────────────
    void UpdateHealthUI()
    {
        if (healthBarSlider != null)
            healthBarSlider.value = (float)currentHealth / maxHealth;
    }

    // ────────────────────────────────────────────────────────────
    IEnumerator HitFlash()
    {
        if (hitFlashImage == null) yield break;
        Color c        = hitFlashImage.color;
        c.a            = 0.4f;
        hitFlashImage.color = c;
        yield return new WaitForSeconds(hitFlashDuration);
        c.a            = 0f;
        hitFlashImage.color = c;
    }

    // ────────────────────────────────────────────────────────────
    void Die()
    {
        isDead = true;
        Debug.Log("[Player] YOU DIED!");

        // Show death screen if assigned
        if (deathScreenUI != null)
            deathScreenUI.SetActive(true);

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Disable player movement
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;
    }

    // ────────────────────────────────────────────────────────────
    /// <summary>Heal the player (e.g. from a health pickup).</summary>
    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHealthUI();
    }
}
