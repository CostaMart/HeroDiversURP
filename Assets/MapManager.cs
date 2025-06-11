using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;

    public static MapManager Instance
    {
        get
        {
            return _instance;
        }
        private set
        {
            if (_instance == null)
            {
                _instance = value;
            }
        }
    }



    public List<GameObject> mapElements = new List<GameObject>();
    public CinemachineCamera mainCamera;
    public CinemachineCamera mapCamera;
    public Camera main;
    public Camera GUI;

    private int culling;
    private int guiMask;
    int newMask;
    int terrain;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        _instance = this;
    }
    void Start()
    {
        newMask = LayerMask.NameToLayer("Map");
        terrain = LayerMask.NameToLayer("Terrain");
        GameManager.Instance.playerInput.actions["Map"].performed += OpenMap;


    }

    void OpenMap(CallbackContext context)
    {
        foreach (GameObject element in mapElements)
        {
            if (element != null)
            {
                element.SetActive(true);
            }
        }

        GameManager.Instance.playerInput.actions["Map"].performed -= OpenMap;
        GameManager.Instance.playerInput.SwitchCurrentActionMap("UI");
        GameManager.Instance.playerInput.actions["Map"].performed += CloseMap;

        culling = main.cullingMask;
        main.cullingMask = 1 << newMask | 1 << terrain;
        guiMask = GUI.cullingMask;
        GUI.gameObject.SetActive(!GUI.gameObject.activeSelf);
        mainCamera.gameObject.SetActive(!mainCamera.gameObject.activeSelf);
        RenderSettings.fog = false;
        mapCamera.gameObject.SetActive(!mapCamera.gameObject.activeSelf);
    }

    void CloseMap(CallbackContext context)
    {
        foreach (GameObject element in mapElements)
        {
            if (element != null)
            {
                element.SetActive(false);

            }
        }
        GameManager.Instance.playerInput.actions["Map"].performed -= CloseMap;
        GameManager.Instance.playerInput.SwitchCurrentActionMap("Player");
        GameManager.Instance.playerInput.actions["Map"].performed += OpenMap;


        main.cullingMask = culling;
        RenderSettings.fog = true;
        GUI.gameObject.SetActive(!GUI.gameObject.activeSelf);
        mainCamera.gameObject.SetActive(!mainCamera.gameObject.activeSelf);
        mapCamera.gameObject.SetActive(!mapCamera.gameObject.activeSelf);
    }
}
