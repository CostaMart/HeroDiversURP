using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.InputAction;

public class LostManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        Debug.Log("LostManager enabled");
        GameManager.Instance.playerInput.actions["Confirm"].performed += PlayerIsAHero;
        GameManager.Instance.playerInput.actions["Esc"].performed += PlayerIsACowardAndDoesntCareAboutHumanity;
    }

    void OnDisable()
    {
        Debug.Log("LostManager disabled");
        GameManager.Instance.playerInput.actions["Confirm"].performed -= PlayerIsAHero;
        GameManager.Instance.playerInput.actions["Esc"].performed -= PlayerIsACowardAndDoesntCareAboutHumanity;
    }

    public void PlayerIsAHero(CallbackContext ctx)
    {
        Debug.Log("Player is a hero, loading for the lost");
        SceneManager.LoadSceneAsync("DesertLevelHeroDivers", LoadSceneMode.Single);

    }
    public void PlayerIsACowardAndDoesntCareAboutHumanity(CallbackContext ctx)
    {
        Debug.Log("Player is a coward, loading StartScreen scene");
        SceneManager.LoadSceneAsync("StartScreen", LoadSceneMode.Single);
    }
}
