using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton manager responsible for spawning, registering, and updating all NPCs in the scene.
/// </summary>
public class NPCManager : MonoBehaviour {
    public static NPCManager Instance { get; private set; }

    [Tooltip("Prefab used to spawn new NPCs.")]
    public GameObject npcPrefab;

    // Internal list of all active NPCs
    private List<NPC> npcList = new();

    public float spawnInterval = 5f; // Time interval for spawning NPCs
    private float spawnTimer = 0f; // Timer to track spawn intervals

    [Tooltip("Maximum number of NPCs that can exist at once")]
    public int maxNPCs = 5; // Maximum number of NPCs to spawn

    public int NPCCount => npcList.Count; // Property to get current NPC count

    void Awake() {
        // Ensure only one instance exists
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Spawns an NPC at the given position, registers it, and returns the NPC component.
    /// </summary>
    public NPC SpawnNPC(Vector3 position) {
        GameObject go = Instantiate(npcPrefab, position, Quaternion.identity);
        go.name = "NPC_" + npcList.Count;  // Name the NPC for easier identification
        go.transform.SetParent(transform); // Set the parent to the NPCManager for organization

        if (!go.TryGetComponent<NPC>(out var npcComponent))
            npcComponent = go.AddComponent<NPC>();

        RegisterNPC(npcComponent);
        return npcComponent;
    }

    /// <summary>
    /// Adds an NPC to the manager's tracking list.
    /// </summary>
    public void RegisterNPC(NPC npc) {
        if (npc != null && !npcList.Contains(npc))
            npcList.Add(npc);
    }

    /// <summary>
    /// Removes an NPC from tracking when destroyed.
    /// </summary>
    public void UnregisterNPC(NPC npc) {
        if (npcList.Contains(npc))
            npcList.Remove(npc);
    }

    void Update() {
        // Handle NPC spawning at regular intervals
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && npcList.Count < maxNPCs) {
            SpawnNPC(new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)));
            spawnTimer = 0f;
        }
    }
}