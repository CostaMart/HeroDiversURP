[System.Serializable]
public class NPCSettings
{
    // Patrol Settings
    public float patrolRadius = 10f; // Radius of the patrol area
    public int patrolCount = 5; // Number of patrol points to generate
    public float minDistance = 2f; // Minimum distance between patrol points

    // Chase Settings
    public float pathUpdateRate = 0.5f; // Frequenza di aggiornamento del percorso
}