using System.Collections.Generic;
using UnityEngine;

public class ScavengerHuntManager : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private List<MisplacedObject> _huntSequence;
    
    // --- NEW: Reference to the UI script ---
    [SerializeField] private SimpleGameUI _gameUI; 

    private int _currentIndex = 0;

    private void Start()
    {
        InitializeSequence();
    }

    private void OnEnable()
    {
        MisplacedObject.OnObjectFound += HandleObjectFound;
    }

    private void OnDisable()
    {
        MisplacedObject.OnObjectFound -= HandleObjectFound;
    }

    private void InitializeSequence()
    {
        for (int i = 0; i < _huntSequence.Count; i++)
        {
            if (_huntSequence[i] != null)
            {
                _huntSequence[i].gameObject.SetActive(i == 0);
            }
        }
    }

    private void HandleObjectFound(MisplacedObject foundObj)
    {
        if (_currentIndex < _huntSequence.Count && foundObj == _huntSequence[_currentIndex])
        {
            _currentIndex++;

            if (_currentIndex < _huntSequence.Count)
            {
                MisplacedObject nextItem = _huntSequence[_currentIndex];
                if (nextItem != null)
                {
                    nextItem.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.Log("You Win!");
                // --- NEW: Trigger Victory Screen ---
                if (_gameUI != null) _gameUI.ShowVictory();
            }
        }
    }
}