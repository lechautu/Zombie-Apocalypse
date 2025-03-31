using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Characters.CharacterController _playerController;
    public GameObject gameOverUI;

    public Slider healthBar;

    void Awake()
    {
        Instance = this;     
    }

    void Start()
    {
        // Initialize game state, load resources, etc.
        Debug.Log("Game Manager Initialized");
    }

    void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        healthBar.value = Mathf.Lerp(healthBar.value, (float)currentHealth / maxHealth, Time.deltaTime * 5f);
    }

    void Update()
    {
        if (_playerController != null)
        {
            UpdateHealthBar(_playerController.CurrentHealth, _playerController.characterDefinition.maxHealth);
            if (_playerController.IsDead && !gameOverUI.activeSelf)
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
}
