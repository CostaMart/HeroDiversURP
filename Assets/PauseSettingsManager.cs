using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PauseSettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;
    private PlayerInput playerInput;
    [SerializeField] AudioMixer audioMixer;
    bool pause = false;
    void Awake()
    {
        // just to force the ItemManager to be loaded
        float valueInDb = PlayerPrefs.GetFloat("sfxVolume", 1f);
        audioMixer.SetFloat("SFX", valueInDb);


        float valueinDbShooting = PlayerPrefs.GetFloat("shootingVolume", 1f);
        audioMixer.SetFloat("Shooting", valueinDbShooting);

        float valueInDbMusic = PlayerPrefs.GetFloat("musicVolume", 1f);
        audioMixer.SetFloat("Music", valueInDbMusic);


        float explosionVolume = PlayerPrefs.GetFloat("explosionVolume", 1f);
        audioMixer.SetFloat("Explosion", explosionVolume);
    }

    void OnEnable()
    {
        playerInput = GameObject.Find("Player").GetComponent<PlayerInput>();
        playerInput.SwitchCurrentActionMap("Player");
        playerInput.actions["Pause"].performed += TogglePause;
    }

    void OnDisable()
    {
        playerInput.actions["Pause"].performed -= TogglePause;
    }

    void TogglePause(CallbackContext ctx)
    {

        if (!pause)
        {
            Debug.Log("Pausing");
            playerInput.actions["Pause"].performed -= TogglePause;

            playerInput.SwitchCurrentActionMap("UI");

            playerInput.actions["Pause"].performed += TogglePause;
            pauseCanvas.SetActive(!pauseCanvas.activeSelf);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            pause = true;
        }
        else
        {
            Debug.Log("Unpausing");
            playerInput.actions["Pause"].performed -= TogglePause;

            playerInput.SwitchCurrentActionMap("Player");

            playerInput.actions["Pause"].performed += TogglePause;

            pauseCanvas.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            pause = false;
        }

    }
}
