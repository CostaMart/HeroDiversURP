using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DebuManager : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    void Start()
    {
        playerInput.actions["ReloadScene"].performed += ctx => ReloadScene();
    }

    public void ReloadScene()
    {

        var loading = SceneManager.LoadSceneAsync("DesertLevelHeroDivers");
        loading.allowSceneActivation = true;
    }

}
