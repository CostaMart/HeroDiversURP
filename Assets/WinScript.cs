using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.InputAction;

public class WinScript : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    void OnEnable()
    {
        text.text = $"Astro Credits earned: {ItemManager.playerDispatcher.GetFeatureByType<int>(FeatureType.astroCredits).Sum()}";
        GameManager.Instance.playerInput.actions["Confirm"].performed += PlayerIsAHero;
        var astro = PlayerPrefs.GetInt("astroCredits", 0);
        PlayerPrefs.SetInt("astroCredits", astro + ItemManager.playerDispatcher.GetFeatureByType<int>(FeatureType.astroCredits).Sum());
    }

    void OnDisable()
    {
        Debug.Log("LostManager disabled");
        GameManager.Instance.playerInput.actions["Confirm"].performed -= PlayerIsAHero;
    }

    public void PlayerIsAHero(CallbackContext ctx)
    {
        Debug.Log("Player is a hero, loading DesertLevelHeroDivers scene");
        SceneManager.LoadSceneAsync("StartScreen", LoadSceneMode.Single);

    }

}
