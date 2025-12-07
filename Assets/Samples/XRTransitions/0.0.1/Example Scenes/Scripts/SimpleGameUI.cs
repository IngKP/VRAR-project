using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleGameUI : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Assign the Canvas-Panel object containing your rules text.")]
    public GameObject rulesPanel;
    
    [Tooltip("Assign the Canvas-Panel object containing your victory text.")]
    public GameObject victoryPanel;

    private void Start()
    {
        // 1. Show Rules at the start
        if (rulesPanel != null) rulesPanel.SetActive(true);
        
        // 2. Ensure Victory screen is hidden
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    // Call this from your "Start Game" button OnClick() event
    public void DismissRules()
    {
        if (rulesPanel != null) rulesPanel.SetActive(false);
    }

    public void ShowVictory()
    {
        if (victoryPanel == null) return;

        // 3. Move the victory panel in front of the player so they can see it anywhere
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Place 1.5 meters in front of the camera
            victoryPanel.transform.position = mainCam.transform.position + (mainCam.transform.forward * 1.5f);
            
            // Make it face the camera
            victoryPanel.transform.LookAt(mainCam.transform);
            
            // Fix text being backwards by rotating 180 degrees
            victoryPanel.transform.Rotate(0, 180, 0);
            
            // Ensure it's at eye level height-wise (optional adjustments)
            Vector3 fixedHeight = victoryPanel.transform.position;
            fixedHeight.y = mainCam.transform.position.y;
            victoryPanel.transform.position = fixedHeight;
        }

        victoryPanel.SetActive(true);
    }

    // Call this from a "Restart" button on the Victory screen
    public void RestartGame()
    {
        // Reloads the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}