using System;
using System.Collections;
using System.Collections.Generic;
using ARPG.Core;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Damageable _playerController;

    [Header("UI Elements")]
    public GameObject gameOverUI;
    public Slider healthBar;
    public TMPro.TextMeshProUGUI scoreText;

    private int _score;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Initialize game state, load resources, etc.
        Debug.Log("Game Manager Initialized");
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            _playerController = playerObject.GetComponent<Damageable>();
        }
    }

    void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthBar.value = Mathf.Lerp(healthBar.value, (float)currentHealth / maxHealth, Time.deltaTime * 5f);
    }

    void Update()
    {
        if (_playerController != null)
        {
            UpdateHealthBar(_playerController.health, _playerController.maxHealth);
            if (_playerController.health <= 0 && !gameOverUI.activeSelf)
            {
                gameOverUI.SetActive(true);
                Time.timeScale = 0; // Pause the game
            }
        }
    }

    public void RestartGame()
    {
        // Reload the scene or reset game state
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1; // Resume the game
    }
    
    public void AddScore(int amount)
    {
        _score += amount;
        scoreText.text = "Score: " + _score;
    }
}
