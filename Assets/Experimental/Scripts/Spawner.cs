using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility.Positioning;

namespace Spawning
{
    /// <summary>
    /// An object spawner that generates game objects using configurable spawn strategies
    /// </summary>
    public class Spawner : InteractiveObject
    {
        [Header("Prefab Settings")]
        [Tooltip("The prefabs to spawn. If multiple are provided, one will be selected randomly")]
        [SerializeField] private GameObject[] prefabsToSpawn;
        
        [Tooltip("Optional container to parent spawned objects to")]
        [SerializeField] private Transform spawnContainer;
        
        [Header("Spawn Behavior")]
        [Tooltip("Maximum number of objects to spawn (0 = unlimited)")]
        [SerializeField] private int maxSpawnCount = 5;
        
        [Tooltip("Whether to start spawning automatically on start")]
        [SerializeField] private bool autoStart = true;
        
        [Header("Spawn Strategy")]
        [Tooltip("The strategy to use for spawning objects")]
        [SerializeField] private SpawnStrategyType spawnStrategyType = SpawnStrategyType.Area;
        
        [Header("Area Spawn Settings")]
        [SerializeField] private AreaSpawnSettings areaSettings = new();
        
        [Header("Grid Spawn Settings")]
        [SerializeField] private GridSpawnSettings gridSettings = new();
        
        [Header("Path Spawn Settings")]
        [SerializeField] private PathSpawnSettings pathSettings = new();
        
        [Header("Spawn Timing")]
        [SerializeField] private SpawnTimingSettings timingSettings = new();

        [Header("General Settings")]
        [SerializeField] private GeneralSettings generalSettings = new();
        
        [Header("Events")]
        public UnityEvent<GameObject> OnObjectSpawned;
        public UnityEvent OnSpawningCompleted;
        public UnityEvent OnSpawningStarted;
        
        // Runtime state
        private int currentSpawnCount;
        private bool isSpawning;
        private Coroutine spawnRoutine;
        private ISpawnStrategy activeStrategy;
        private RandomPointGenerator pointGenerator;
        
        #region Spawn Strategy Settings Classes
        
        [Serializable]
        public enum SpawnStrategyType
        {
            Area,   // Spawn in a defined area (circle, rectangle)
            Grid,   // Spawn in a grid pattern
            Path    // Spawn along a path
        }
        
        [Serializable]
        public class AreaSpawnSettings
        {
            [Tooltip("Shape of the spawn area")]
            public RandomPointGenerator.AreaShape shape = RandomPointGenerator.AreaShape.Circle;
            
            [Tooltip("Radius for circular spawn area")]
            public float radius = 10f;
            
            [Tooltip("Width for rectangular spawn area")]
            public float width = 20f;
            
            [Tooltip("Length for rectangular spawn area")]
            public float length = 20f;
            
            [Tooltip("Distribution pattern of spawned objects")]
            public RandomPointGenerator.DistributionType distribution = RandomPointGenerator.DistributionType.Uniform;
            
            [Tooltip("Minimum distance between spawned objects (0 = no minimum)")]
            public float minDistance = 0f;
            
            [Tooltip("Whether to spawn objects in a single batch or over time")]
            public bool spawnAsBatch = false;
            
            [Tooltip("Number of objects to spawn when spawning as batch")]
            public int batchSize = 5;
        }
        
        [Serializable]
        public class GridSpawnSettings
        {
            [Tooltip("Distance between grid points")]
            public float spacing = 5f;
            
            [Tooltip("Random variation to apply to grid points (0-1, where 0 means perfect grid)")]
            public float jitter = 0.2f;
            
            [Tooltip("Width of the grid")]
            public float width = 20f;
            
            [Tooltip("Length of the grid")]
            public float length = 20f;
            
            [Tooltip("Whether to fill the grid all at once or cell by cell over time")]
            public bool fillAllAtOnce = false;
            
            [Tooltip("Whether to traverse the grid in random order")]
            public bool randomTraversal = false;
        }
        
        [Serializable]
        public class PathSpawnSettings
        {
            [Tooltip("Path nodes to spawn along")]
            public Transform[] pathNodes;
            
            [Tooltip("Whether to close the path as a loop")]
            public bool closedPath = false;
            
            [Tooltip("Spacing between spawned objects along the path")]
            public float spacing = 2f;
            
            [Tooltip("Random offset from the path (0 = directly on path)")]
            public float randomOffset = 0f;
        }
        
        [Serializable]
        public class SpawnTimingSettings
        {
            [Tooltip("Delay before first spawn")]
            public float initialDelay = 0f;
            
            [Tooltip("Time between spawns")]
            public float interval = 2f;
            
            [Tooltip("Whether to use random intervals")]
            public bool useRandomInterval = false;
            
            [Tooltip("Minimum random interval")]
            public float minInterval = 1f;
            
            [Tooltip("Maximum random interval")]
            public float maxInterval = 5f;
        }
        
        [Serializable]
        public class GeneralSettings
        {
            [Tooltip("Height offset for spawned objects")]
            public float heightOffset = 0.5f;
            
            [Tooltip("Whether to validate spawn positions on NavMesh")]
            public bool validateOnNavMesh = true;
            
            [Tooltip("Maximum search distance for NavMesh validation")]
            public float navMeshSearchDistance = 5f;
            
            [Tooltip("Whether to avoid overlaps with other objects")]
            public bool avoidOverlaps = true;
            
            [Tooltip("Radius to check for overlaps")]
            public float overlapCheckRadius = 0.5f;
            
            [Tooltip("Layers to check for overlaps")]
            public LayerMask overlapLayerMask = default;
        }
        
        #endregion
        
        #region Spawn Strategy Interfaces
        
        /// <summary>
        /// Interface for all spawn strategies
        /// </summary>
        private interface ISpawnStrategy
        {
            void Initialize(Spawner spawner, RandomPointGenerator pointGenerator);
            Vector3? GetNextSpawnPosition();
            void OnDrawGizmos(Spawner spawner);
            bool HasRemainingPositions();
        }
        
        /// <summary>
        /// Strategy for spawning in a defined area
        /// </summary>
        private class AreaSpawnStrategy : ISpawnStrategy
        {
            private Spawner spawner;
            private RandomPointGenerator pointGenerator;
            private List<Vector3> cachedPositions;
            private int currentIndex;
            
            public void Initialize(Spawner spawner, RandomPointGenerator pointGenerator)
            {
                this.spawner = spawner;
                this.pointGenerator = pointGenerator;
                this.cachedPositions = new List<Vector3>();
                this.currentIndex = 0;
                
                if (spawner.areaSettings.spawnAsBatch)
                {
                    GenerateBatchPositions();
                }
            }
            
            public Vector3? GetNextSpawnPosition()
            {
                if (spawner.areaSettings.spawnAsBatch)
                {
                    // Use pre-generated positions
                    if (currentIndex < cachedPositions.Count)
                    {
                        return cachedPositions[currentIndex++];
                    }
                    return null;
                }
                else
                {
                    // Generate a new position each time
                    return GenerateSinglePosition();
                }
            }
            
            public bool HasRemainingPositions()
            {
                if (spawner.areaSettings.spawnAsBatch)
                {
                    return currentIndex < cachedPositions.Count;
                }
                return true; // For non-batch, we can always try to generate more
            }
            
            private void GenerateBatchPositions()
            {
                Vector3 size = GetSizeVector();
                var results = pointGenerator.GeneratePoints(
                    spawner.transform.position,
                    size,
                    spawner.areaSettings.batchSize,
                    spawner.areaSettings.shape,
                    spawner.areaSettings.minDistance
                );
                
                cachedPositions.Clear();
                foreach (var result in results)
                {
                    if (result.IsValid)
                    {
                        cachedPositions.Add(result.Position);
                    }
                }
            }
            
            private Vector3? GenerateSinglePosition()
            {
                Vector3 size = GetSizeVector();
                var result = pointGenerator.GeneratePoint(spawner.transform.position, size, spawner.areaSettings.shape);
                
                if (result.IsValid)
                {
                    return result.Position;
                }
                return null;
            }
            
            private Vector3 GetSizeVector()
            {
                return spawner.areaSettings.shape == RandomPointGenerator.AreaShape.Rectangle
                    ? new Vector3(spawner.areaSettings.width, 0, spawner.areaSettings.length)
                    : new Vector3(spawner.areaSettings.radius, 0, 0);
            }
            
            public void OnDrawGizmos(Spawner spawner)
            {
                Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.4f);
                
                if (spawner.areaSettings.shape == RandomPointGenerator.AreaShape.Rectangle)
                {
                    Gizmos.DrawCube(
                        spawner.transform.position, 
                        new Vector3(spawner.areaSettings.width, 0.1f, spawner.areaSettings.length)
                    );
                }
                else
                {
                    Gizmos.DrawSphere(spawner.transform.position, spawner.areaSettings.radius);
                }
                
                // Draw positions if in batch mode
                if (spawner.areaSettings.spawnAsBatch && cachedPositions != null)
                {
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
                    foreach (var pos in cachedPositions)
                    {
                        Gizmos.DrawSphere(pos, 0.3f);
                    }
                }
            }
        }
        
        /// <summary>
        /// Strategy for spawning in a grid pattern
        /// </summary>
        private class GridSpawnStrategy : ISpawnStrategy
        {
            private Spawner spawner;
            private RandomPointGenerator pointGenerator;
            private List<Vector3> gridPositions;
            private int currentIndex;
            
            public void Initialize(Spawner spawner, RandomPointGenerator pointGenerator)
            {
                this.spawner = spawner;
                this.pointGenerator = pointGenerator;
                GenerateGridPositions();
                
                if (spawner.gridSettings.randomTraversal)
                {
                    ShufflePositions();
                }
            }
            
            public Vector3? GetNextSpawnPosition()
            {
                if (currentIndex < gridPositions.Count)
                {
                    return gridPositions[currentIndex++];
                }
                return null;
            }
            
            public bool HasRemainingPositions()
            {
                return currentIndex < gridPositions.Count;
            }
            
            private void GenerateGridPositions()
            {
                var results = pointGenerator.GenerateGridPoints(
                    spawner.transform.position,
                    new Vector2(spawner.gridSettings.width, spawner.gridSettings.length),
                    spawner.gridSettings.spacing,
                    spawner.gridSettings.jitter
                );
                
                gridPositions = new List<Vector3>();
                foreach (var result in results)
                {
                    if (result.IsValid)
                    {
                        gridPositions.Add(result.Position);
                    }
                }
                
                currentIndex = 0;
            }
            
            private void ShufflePositions()
            {
                int n = gridPositions.Count;
                while (n > 1)
                {
                    n--;
                    int k = UnityEngine.Random.Range(0, n + 1);
                    (gridPositions[n], gridPositions[k]) = (gridPositions[k], gridPositions[n]);
                }
            }
            
            public void OnDrawGizmos(Spawner spawner)
            {
                // Draw grid outline
                Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.4f);
                Gizmos.DrawWireCube(
                    spawner.transform.position,
                    new Vector3(spawner.gridSettings.width, 0.1f, spawner.gridSettings.length)
                );
                
                // Draw grid lines
                Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                Vector3 center = spawner.transform.position;
                float halfWidth = spawner.gridSettings.width / 2;
                float halfLength = spawner.gridSettings.length / 2;
                float spacing = spawner.gridSettings.spacing;
                
                // Horizontal lines
                for (float z = -halfLength; z <= halfLength; z += spacing)
                {
                    Vector3 start = center + new Vector3(-halfWidth, 0, z);
                    Vector3 end = center + new Vector3(halfWidth, 0, z);
                    Gizmos.DrawLine(start, end);
                }
                
                // Vertical lines
                for (float x = -halfWidth; x <= halfWidth; x += spacing)
                {
                    Vector3 start = center + new Vector3(x, 0, -halfLength);
                    Vector3 end = center + new Vector3(x, 0, halfLength);
                    Gizmos.DrawLine(start, end);
                }
                
                // Draw spawn points
                if (gridPositions != null)
                {
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
                    foreach (var pos in gridPositions)
                    {
                        Gizmos.DrawSphere(pos, 0.3f);
                    }
                    
                    // Highlight current position
                    if (currentIndex > 0 && currentIndex <= gridPositions.Count)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(gridPositions[currentIndex - 1], 0.4f);
                    }
                }
            }
        }
        
        /// <summary>
        /// Strategy for spawning along a path
        /// </summary>
        private class PathSpawnStrategy : ISpawnStrategy
        {
            private Spawner spawner;
            private RandomPointGenerator pointGenerator;
            private List<Vector3> pathPoints;
            private int currentIndex;
            
            public void Initialize(Spawner spawner, RandomPointGenerator pointGenerator)
            {
                this.spawner = spawner;
                this.pointGenerator = pointGenerator;
                GeneratePathPoints();
            }
            
            public Vector3? GetNextSpawnPosition()
            {
                if (currentIndex < pathPoints.Count)
                {
                    return pathPoints[currentIndex++];
                }
                return null;
            }
            
            public bool HasRemainingPositions()
            {
                return currentIndex < pathPoints.Count;
            }
            
            private void GeneratePathPoints()
            {
                pathPoints = new List<Vector3>();
                Transform[] nodes = spawner.pathSettings.pathNodes;
                
                if (nodes == null || nodes.Length < 2)
                {
                    Debug.LogWarning("Path spawn strategy requires at least 2 path nodes");
                    return;
                }
                
                // Calculate total path length
                float totalLength = 0;
                for (int i = 0; i < nodes.Length - 1; i++)
                {
                    totalLength += Vector3.Distance(nodes[i].position, nodes[i + 1].position);
                }
                
                if (spawner.pathSettings.closedPath)
                {
                    totalLength += Vector3.Distance(nodes[nodes.Length - 1].position, nodes[0].position);
                }
                
                // Calculate number of points based on spacing
                int pointCount = Mathf.Max(2, Mathf.FloorToInt(totalLength / spawner.pathSettings.spacing));
                float step = 1.0f / pointCount;
                
                // Generate points
                for (float t = 0; t <= 1.0f; t += step)
                {
                    Vector3 pathPoint = GetPointAlongPath(t, nodes, spawner.pathSettings.closedPath);
                    
                    // Add random offset if specified
                    if (spawner.pathSettings.randomOffset > 0)
                    {
                        Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * spawner.pathSettings.randomOffset;
                        randomOffset.y = 0; // Keep on same height plane
                        pathPoint += randomOffset;
                    }
                    
                    // Validate point
                    if (pointGenerator != null)
                    {
                        if (pointGenerator.IsPointValid(pathPoint))
                        {
                            pathPoints.Add(pathPoint);
                        }
                    }
                    else
                    {
                        pathPoints.Add(pathPoint);
                    }
                }
                
                currentIndex = 0;
            }
            
            private Vector3 GetPointAlongPath(float t, Transform[] nodes, bool closedPath)
            {
                if (nodes.Length == 0) return Vector3.zero;
                if (nodes.Length == 1) return nodes[0].position;
                
                int nodeCount = closedPath ? nodes.Length : nodes.Length - 1;
                float scaledT = t * nodeCount;
                int i = Mathf.FloorToInt(scaledT);
                float fraction = scaledT - i;
                
                i = i % nodes.Length;
                int nextI = (i + 1) % nodes.Length;
                
                return Vector3.Lerp(nodes[i].position, nodes[nextI].position, fraction);
            }
            
            public void OnDrawGizmos(Spawner spawner)
            {
                Transform[] nodes = spawner.pathSettings.pathNodes;
                if (nodes == null || nodes.Length < 2) return;
                
                // Draw path lines
                Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
                for (int i = 0; i < nodes.Length - 1; i++)
                {
                    if (nodes[i] != null && nodes[i+1] != null)
                        Gizmos.DrawLine(nodes[i].position, nodes[i+1].position);
                }
                
                if (spawner.pathSettings.closedPath && nodes[0] != null && nodes[^1] != null)
                {
                    Gizmos.DrawLine(nodes[^1].position, nodes[0].position);
                }
                
                // Draw nodes
                Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
                foreach (var node in nodes)
                {
                    if (node != null)
                        Gizmos.DrawSphere(node.position, 0.5f);
                }
                
                // Draw spawn points
                if (pathPoints != null)
                {
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
                    foreach (var point in pathPoints)
                    {
                        Gizmos.DrawSphere(point, 0.3f);
                    }
                    
                    // Highlight current position
                    if (currentIndex > 0 && currentIndex <= pathPoints.Count)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(pathPoints[currentIndex - 1], 0.4f);
                    }
                }
            }
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeSystem();
        }
        
        protected override void Start()
        {
            base.Start();
            // Register with entity manager
            if (EntityManager.Instance != null)
            {
                EntityManager.Instance.RegisterEntity(objectId, gameObject);
            }
            
            if (autoStart)
            {
                StartSpawning();
            }
        }
        
        private void OnValidate()
        {
            // Update strategy on inspector changes
            if (Application.isPlaying && isSpawning)
            {
                StopSpawning();
                InitializeSystem();
                StartSpawning();
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw different gizmos based on strategy
            if (activeStrategy != null)
            {
                activeStrategy.OnDrawGizmos(this);
            }
            else
            {
                // If not playing, create temporary strategy for gizmos
                ISpawnStrategy tempStrategy = spawnStrategyType switch
                {
                    SpawnStrategyType.Area => new AreaSpawnStrategy(),
                    SpawnStrategyType.Grid => new GridSpawnStrategy(),
                    SpawnStrategyType.Path => new PathSpawnStrategy(),
                    _ => null
                };

                if (tempStrategy != null)
                {
                    tempStrategy.Initialize(this, null);
                    tempStrategy.OnDrawGizmos(this);
                }
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Start spawning objects according to the configured strategy
        /// </summary>
        public void StartSpawning()
        {
            if (isSpawning) return;
            
            isSpawning = true;
            currentSpawnCount = 0;
            
            // Handle one-shot spawning for specific strategies
            switch (spawnStrategyType)
            {
                case SpawnStrategyType.Grid when gridSettings.fillAllAtOnce:
                case SpawnStrategyType.Area when areaSettings.spawnAsBatch:
                    SpawnBatch();
                    break;
                    
                default:
                    spawnRoutine = StartCoroutine(SpawnRoutine());
                    break;
            }
            
            OnSpawningStarted?.Invoke();
        }
        
        /// <summary>
        /// Stop the spawner from creating more objects
        /// </summary>
        public void StopSpawning()
        {
            if (!isSpawning) return;
            
            isSpawning = false;
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
        }
        
        /// <summary>
        /// Reset the spawn count to zero, allowing the spawner to start over
        /// </summary>
        public void ResetSpawner()
        {
            StopSpawning();
            currentSpawnCount = 0;
            InitializeSystem();
        }
        
        /// <summary>
        /// Spawn a single object at the next position determined by the strategy
        /// </summary>
        /// <returns>The spawned GameObject, or null if spawn failed</returns>
        public GameObject SpawnSingleObject()
        {
            if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
            {
                Debug.LogWarning("No prefabs configured for spawning");
                return null;
            }
            
            Vector3? nextPosition = activeStrategy?.GetNextSpawnPosition();
            if (!nextPosition.HasValue)
            {
                return null;
            }
            
            // Choose a random prefab
            int randomIndex = UnityEngine.Random.Range(0, prefabsToSpawn.Length);
            GameObject prefabToSpawn = prefabsToSpawn[randomIndex];

            // Spawn using EntityManager
            string entityId = prefabToSpawn.name;
            int count = 0;
            while (EntityManager.Instance.HasEntity(entityId))
            {
                entityId = prefabToSpawn.name + "_" + ++count;
            }

            GameObject spawnedObject = EntityManager.Instance.InstantiateEntity(
                entityId, 
                prefabToSpawn, 
                nextPosition.Value, 
                Quaternion.identity,
                spawnContainer
            );
            
            OnObjectSpawned?.Invoke(spawnedObject);
            currentSpawnCount++;
            
            return spawnedObject;
        }
        
        /// <summary>
        /// Spawn an object at a specific position
        /// </summary>
        /// <param name="entityId">ID to register with the EntityManager</param>
        /// <param name="position">Position to spawn at</param>
        /// <returns>The spawned GameObject</returns>
        // public GameObject SpawnAtPosition(string entityId, Vector3 position)
        // {
        //     if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        //     {
        //         Debug.LogWarning("No prefabs configured for spawning");
        //         return null;
        //     }
            
        //     int randomIndex = UnityEngine.Random.Range(0, prefabsToSpawn.Length);
        //     GameObject prefabToSpawn = prefabsToSpawn[randomIndex];
            
        //     GameObject spawnedObject = EntityManager.Instance.InstantiateEntity(
        //         entityId, 
        //         prefabToSpawn, 
        //         position, 
        //         Quaternion.identity, 
        //         spawnContainer
        //     );
            
        //     OnObjectSpawned?.Invoke(spawnedObject);
        //     currentSpawnCount++;
            
        //     return spawnedObject;
        // }
        
        /// <summary>
        /// Get the current number of spawned objects
        /// </summary>
        // public int GetCurrentSpawnCount()
        // {
        //     return currentSpawnCount;
        // }
        
        /// <summary>
        /// Change the spawn strategy at runtime
        /// </summary>
        /// <param name="newStrategy">The new strategy type to use</param>
        public void ChangeSpawnStrategy(SpawnStrategyType newStrategy)
        {
            spawnStrategyType = newStrategy;
            
            if (isSpawning)
            {
                StopSpawning();
                InitializeSystem();
                StartSpawning();
            }
            else
            {
                InitializeSystem();
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void InitializeSystem()
        {
            // Create point generator with appropriate settings
            var options = new RandomPointGenerator.PointGeneratorOptions
            {
                PointHeight = generalSettings.heightOffset,
                Distribution = areaSettings.distribution,
                ValidateOnNavMesh = generalSettings.validateOnNavMesh,
                NavMeshSearchDistance = generalSettings.navMeshSearchDistance,
                AvoidOverlaps = generalSettings.avoidOverlaps,
                OverlapCheckRadius = generalSettings.overlapCheckRadius,
                OverlapLayerMask = generalSettings.overlapLayerMask
            };
            
            pointGenerator = new RandomPointGenerator(options);
            
            // Create the appropriate strategy
            activeStrategy = spawnStrategyType switch
            {
                SpawnStrategyType.Area => new AreaSpawnStrategy(),
                SpawnStrategyType.Grid => new GridSpawnStrategy(),
                SpawnStrategyType.Path => new PathSpawnStrategy(),
                _ => null
            };
            
            // Initialize the strategy
            activeStrategy?.Initialize(this, pointGenerator);
        }
        
        /// <summary>
        /// Spawns all objects in a batch at once. Used for both grid and area batch spawning.
        /// </summary>
        private void SpawnBatch()
        {
            if (!isSpawning) return;
            
            while (activeStrategy.HasRemainingPositions() && 
                  (maxSpawnCount <= 0 || currentSpawnCount < maxSpawnCount))
            {
                SpawnSingleObject();
            }
            
            isSpawning = false;
            OnSpawningCompleted?.Invoke();
        }
        
        private IEnumerator SpawnRoutine()
        {
            yield return new WaitForSeconds(timingSettings.initialDelay);
            
            while (isSpawning && activeStrategy.HasRemainingPositions() && 
                   (maxSpawnCount <= 0 || currentSpawnCount < maxSpawnCount))
            {
                SpawnSingleObject();
                
                float nextInterval = timingSettings.useRandomInterval
                    ? UnityEngine.Random.Range(timingSettings.minInterval, timingSettings.maxInterval)
                    : timingSettings.interval;
                
                yield return new WaitForSeconds(nextInterval);
            }
            
            isSpawning = false;
            OnSpawningCompleted?.Invoke();
        }
        
        #endregion
    }
}