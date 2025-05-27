using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Utility.Positioning
{
    /// <summary>
    /// Sistema per la generazione di punti casuali nel mondo di gioco,
    /// con supporto per diverse forme, vincoli NavMesh e distribuzioni spaziali.
    /// 
    /// ================= USE CASES ==================
    /// - Utilizzo base
    ///   // Creazione di un generatore con opzioni predefinite
    ///   var generator = new RandomPointGenerator();
    /// 
    ///   // Generare un singolo punto in un'area circolare
    ///   var point = generator.GeneratePoint(
    ///     center: transform.position,
    ///     size: new Vector3(10f, 0, 0),  // Raggio di 10 unità
    ///     shape: RandomPointGenerator.AreaShape.Circle
    ///   );
    /// 
    ///   // Verifica se il punto è valido e utilizzalo
    ///   if (point.IsValid)
    ///   {
    ///       // Ad esempio, posiziona un nemico
    ///       Instantiate(enemyPrefab, point.Position, Quaternion.identity);
    ///   }
    /// 
    /// - Configurazione personalizzata
    ///   // Configurazione del generatore con opzioni personalizzate
    ///   var options = new RandomPointGenerator.PointGeneratorOptions
    ///   {
    ///       PointHeight = 1.0f,              // Altezza dal suolo
    ///       MaxAttempts = 50,                // Aumenta tentativi per trovare punti validi
    ///       Distribution = RandomPointGenerator.DistributionType.Perimeter,  // Punti ai bordi
    ///       ValidateOnNavMesh = true,        // Verifica che i punti siano su NavMesh
    ///       NavMeshSearchDistance = 3.0f,    // Distanza di ricerca NavMesh
    ///       NavMeshAreaMask = NavMesh.GetAreaFromName("Walkable"),  // Solo aree "Walkable"
    ///       AvoidOverlaps = true,            // Evita sovrapposizioni con altri oggetti
    ///       OverlapCheckRadius = 1.0f,       // Raggio per il controllo collisioni
    ///       OverlapLayerMask = LayerMask.GetMask("Obstacles", "Players")  // Layer da evitare
    ///    };
    ///    var generator = new RandomPointGenerator(options);
    /// 
    /// - Generazione di più punti
    ///   // Generare più punti in un rettangolo
    ///   var points = generator.GeneratePoints(
    ///       center: transform.position,
    ///       size: new Vector3(20f, 0, 15f),  // Larghezza e lunghezza
    ///       count: 10,                       // Numero di punti
    ///       shape: RandomPointGenerator.AreaShape.Rectangle
    ///    );
    ///    // Utilizzo dei punti generati
    ///    foreach (var point in points)
    ///    {
    ///        if (point.IsValid)
    ///        {
    ///            // Crea un oggetto in quel punto
    ///            Instantiate(itemPrefab, point.Position, Quaternion.identity);
    ///        }
    ///    }
    /// 
    /// - Punti con distanza minima
    ///   // Generare punti distanziati (utile per posizionare nemici o oggetti)
    ///   var spawnPoints = generator.GeneratePoints(
    ///       center: transform.position,
    ///       size: new Vector3(30f, 0, 30f),
    ///       count: 5,
    ///       shape: RandomPointGenerator.AreaShape.Circle,
    ///       minDistance: 8f    // Almeno 8 unità di distanza tra i punti
    ///   );
    /// 
    /// - Generazione di una griglia
    ///   // Generare punti in una griglia con leggera casualità
    ///   var gridPoints = generator.GenerateGridPoints(
    ///       center: transform.position,
    ///       size: new Vector2(20f, 20f),
    ///       spacing: 5f,       // Distanza tra i punti
    ///       jitter: 0.3f       // Casualità (0-1)
    ///   );
    /// 
    /// - Visualizzazione per debug
    ///   // Nel metodo OnDrawGizmos di un MonoBehaviour
    ///   private void OnDrawGizmos()
    ///   {
    ///       if (debugPoints != null && debugPoints.Count > 0)
    ///       {
    ///           // Mostra i punti come sfere nella scena
    ///           debugPoints.DrawGizmos(
    ///               radius: 0.5f,
    ///               validColor: Color.green, 
    ///               invalidColor: Color.red
    ///           );
    ///       }
    ///   }
    ///   
    ///   // Oppure crea GameObject per rappresentare i punti
    ///   void CreateDebugObjects()
    ///   {
    ///       var points = generator.GeneratePoints(transform.position, new Vector3(10f, 0, 10f), 5);
    ///       var gameObjects = points.CreateGameObjects("DebugPoint");
    ///   }
    /// 
    /// - Utilizzo delle distribuzioni
    ///   // Configurazione per una distribuzione specifica
    ///   var centeredOptions = new RandomPointGenerator.PointGeneratorOptions
    ///   {
    ///       Distribution = RandomPointGenerator.DistributionType.Centered  // Più punti al centro
    ///   };
    ///   var centeredGenerator = new RandomPointGenerator(centeredOptions);
    ///   
    ///   // Generare punti in una sfera (utile per effetti 3D)
    ///   var spherePoints = centeredGenerator.GeneratePoints(
    ///       center: transform.position,
    ///       size: new Vector3(5f, 5f, 5f),  // Raggio della sfera
    ///       count: 20,
    ///       shape: RandomPointGenerator.AreaShape.Sphere
    ///   );
    /// </summary>
    public class RandomPointGenerator
    {
        #region Enums e Structs
        
        /// <summary>
        /// Determina la forma dell'area in cui generare i punti casuali
        /// </summary>
        public enum AreaShape
        {
            Circle,
            Rectangle,
            Sphere // Utile per gli effetti, ma forse la tolgo
        }
        
        /// <summary>
        /// Determina il tipo di distribuzione dei punti casuali
        /// </summary>
        public enum DistributionType
        {
            /// <summary>Distribuzione completamente casuale</summary>
            Uniform,
            
            /// <summary>Maggiore concentrazione al centro</summary>
            Centered, 
            
            /// <summary>Maggiore concentrazione ai bordi</summary>
            Perimeter
        }
        
        /// <summary>
        /// Risultato della generazione di un punto
        /// </summary>
        public readonly struct PointResult
        {
            /// <summary>Posizione del punto</summary>
            public readonly Vector3 Position { get; }
            
            /// <summary>Il punto è valido e utilizzabile</summary>
            public readonly bool IsValid { get; }
            
            /// <summary>Il punto si trova su una NavMesh</summary>
            public readonly bool IsOnNavMesh { get; }
            
            /// <summary>Superficie NavMesh su cui si trova il punto</summary>
            public readonly int NavMeshAreaType { get; }
            
            public PointResult(Vector3 position, bool isValid, bool isOnNavMesh, int navMeshAreaType = 0)
            {
                Position = position;
                IsValid = isValid;
                IsOnNavMesh = isOnNavMesh;
                NavMeshAreaType = navMeshAreaType;
            }
            
            public override string ToString() => 
                $"Position: {Position}, Valid: {IsValid}, OnNavMesh: {IsOnNavMesh}, Area: {NavMeshAreaType}";
        }
        
        #endregion
        
        #region Configurazione
        
        /// <summary>
        /// Opzioni di configurazione per la generazione dei punti
        /// </summary>
        public class PointGeneratorOptions
        {
            /// <summary>Tentativi massimi per trovare un punto valido</summary>
            public int MaxAttempts { get; set; } = 30;
            
            /// <summary>Altezza verticale da aggiungere ai punti generati</summary>
            public float PointHeight { get; set; } = 0.5f;
            
            /// <summary>Tipo di distribuzione da utilizzare</summary>
            public DistributionType Distribution { get; set; } = DistributionType.Uniform;
            
            /// <summary>Evita sovrapposizioni con altri oggetti</summary>
            public bool AvoidOverlaps { get; set; } = true;
            
            /// <summary>Raggio per il controllo di sovrapposizione</summary>
            public float OverlapCheckRadius { get; set; } = 0.5f;
            
            /// <summary>Layer mask per il controllo di sovrapposizione</summary>
            public LayerMask OverlapLayerMask { get; set; } = default;
            
            /// <summary>Validazione dei punti sulla NavMesh</summary>
            public bool ValidateOnNavMesh { get; set; } = true;
            
            /// <summary>Distanza massima per la ricerca di punti NavMesh</summary>
            public float NavMeshSearchDistance { get; set; } = 5f;
            
            /// <summary>Tipi di area NavMesh da considerare validi</summary>
            public int NavMeshAreaMask { get; set; } = NavMesh.AllAreas;
        }
        
        #endregion
        
        #region Campi privati
        
        private readonly PointGeneratorOptions options;
        private readonly HashSet<Vector3> existingPoints = new HashSet<Vector3>();
        
        #endregion
        
        #region Costruttori
        
        /// <summary>
        /// Inizializza un nuovo generatore di punti casuali con le opzioni predefinite
        /// </summary>
        public RandomPointGenerator() : this(new PointGeneratorOptions()) { }
        
        /// <summary>
        /// Inizializza un nuovo generatore di punti casuali con le opzioni specificate
        /// </summary>
        public RandomPointGenerator(PointGeneratorOptions options)
        {
            this.options = options ?? new PointGeneratorOptions();
        }
        
        #endregion
        
        #region Metodi pubblici principali
        
        /// <summary>
        /// Genera un singolo punto casuale in un'area specificata
        /// </summary>
        /// <param name="center">Centro dell'area</param>
        /// <param name="size">Dimensione dell'area (raggio o dimensioni)</param>
        /// <param name="shape">Forma dell'area</param>
        /// <returns>Risultato della generazione del punto</returns>
        public PointResult GeneratePoint(Vector3 center, Vector3 size, AreaShape shape = AreaShape.Circle)
        {
            Vector3 randomPoint;
            
            switch (shape)
            {
                case AreaShape.Circle:
                    randomPoint = GenerateCirclePoint(center, size.x);
                    break;
                case AreaShape.Rectangle:
                    randomPoint = GenerateRectanglePoint(center, size.x, size.z);
                    break;
                case AreaShape.Sphere:
                    randomPoint = GenerateSpherePoint(center, size.x);
                    break;
                default:
                    throw new System.ArgumentException($"Forma non supportata: {shape}");
            }
            
            return ValidateAndProcessPoint(randomPoint);
        }
        
        /// <summary>
        /// Genera più punti casuali in un'area specificata
        /// </summary>
        /// <param name="center">Centro dell'area</param>
        /// <param name="size">Dimensione dell'area (raggio o dimensioni)</param>
        /// <param name="count">Numero di punti da generare</param>
        /// <param name="shape">Forma dell'area</param>
        /// <param name="minDistance">Distanza minima tra i punti (0 per nessuna)</param>
        /// <returns>Lista di risultati dei punti generati</returns>
        public List<PointResult> GeneratePoints(
            Vector3 center, 
            Vector3 size, 
            int count, 
            AreaShape shape = AreaShape.Circle,
            float minDistance = 0f)
        {
            if (count <= 0)
            {
                Debug.LogError("Il numero di punti deve essere maggiore di zero");
                return new List<PointResult>();
            }
            
            // Reset della lista di punti esistenti per questo batch
            existingPoints.Clear();
            
            var results = new List<PointResult>(count);
            int totalAttempts = 0;
            int maxTotalAttempts = count * options.MaxAttempts * 3;
            
            while (results.Count < count && totalAttempts < maxTotalAttempts)
            {
                // Genera un punto casuale
                var result = GeneratePoint(center, size, shape);
                
                if (result.IsValid)
                {
                    // Se abbiamo un vincolo di distanza, controlliamo che sia rispettato
                    if (minDistance > 0)
                    {
                        bool tooClose = false;
                        
                        foreach (var existingPoint in existingPoints)
                        {
                            if (Vector3.Distance(result.Position, existingPoint) < minDistance)
                            {
                                tooClose = true;
                                break;
                            }
                        }
                        
                        if (tooClose)
                        {
                            totalAttempts++;
                            continue;
                        }
                    }
                    
                    // Il punto è valido, lo aggiungiamo al risultato
                    results.Add(result);
                    existingPoints.Add(result.Position);
                }
                
                totalAttempts++;
            }
            
            if (results.Count < count)
            {
                Debug.LogWarning($"Sono stati generati {results.Count} punti di {count} previsti" +
                                 $"(min. distanza: {minDistance}, forma: {shape})");
            }
            
            return results;
        }
        
        /// <summary>
        /// Genera punti distribuiti in una griglia regolare, con leggera casualità
        /// </summary>
        /// <param name="center">Centro dell'area</param>
        /// <param name="size">Dimensioni dell'area (x, z)</param>
        /// <param name="spacing">Distanza tra i punti della griglia</param>
        /// <param name="jitter">Quantità di casualità da aggiungere (0-1)</param>
        /// <returns>Lista di punti generati</returns>
        public List<PointResult> GenerateGridPoints(
            Vector3 center,
            Vector2 size,
            float spacing,
            float jitter = 0.2f)
        {
            if (spacing <= 0)
            {
                Debug.LogError("La spaziatura deve essere maggiore di zero");
                return new List<PointResult>();
            }
            
            var results = new List<PointResult>();
            jitter = Mathf.Clamp01(jitter);
            
            int cols = Mathf.FloorToInt(size.x / spacing);
            int rows = Mathf.FloorToInt(size.y / spacing);
            
            if (cols <= 0 || rows <= 0)
            {
                Debug.LogError($"Dimensioni griglia troppo piccole per lo spacing {spacing}");
                return results;
            }
            
            // Calcola il punto di partenza (angolo in basso a sinistra)
            Vector3 startPoint = center - new Vector3(cols * spacing / 2, 0, rows * spacing / 2);
            
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    // Posizione base nella griglia
                    Vector3 basePos = startPoint + new Vector3(col * spacing, 0, row * spacing);
                    
                    // Aggiungi una leggera casualità se richiesto
                    if (jitter > 0)
                    {
                        float jitterX = GetRandomFloat(-spacing * jitter / 2, spacing * jitter / 2);
                        float jitterZ = GetRandomFloat(-spacing * jitter / 2, spacing * jitter / 2);
                        basePos += new Vector3(jitterX, 0, jitterZ);
                    }
                    
                    var result = ValidateAndProcessPoint(basePos);
                    if (result.IsValid)
                    {
                        results.Add(result);
                    }
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// Verifica se un punto è valido (su NavMesh e senza sovrapposizioni)
        /// </summary>
        public bool IsPointValid(Vector3 point)
        {
            // Controlla sovrapposizioni
            if (options.AvoidOverlaps && 
                Physics.CheckSphere(point, options.OverlapCheckRadius, options.OverlapLayerMask))
            {
                return false;
            }
            
            // Se non richiediamo validazione NavMesh, il punto è valido
            if (!options.ValidateOnNavMesh)
                return true;
            
            // Controlla se il punto è su NavMesh
            return NavMesh.SamplePosition(
                point, 
                out _, 
                options.NavMeshSearchDistance, 
                options.NavMeshAreaMask);
        }
        
        #endregion
        
        #region Metodi privati di generazione
        
        private Vector3 GenerateCirclePoint(Vector3 center, float radius)
        {
            switch (options.Distribution)
            {
                case DistributionType.Uniform:
                    return GenerateUniformCirclePoint(center, radius);
                    
                case DistributionType.Centered:
                    return GenerateCenteredCirclePoint(center, radius);
                    
                case DistributionType.Perimeter:
                    return GeneratePerimeterCirclePoint(center, radius);
                    
                default:
                    return GenerateUniformCirclePoint(center, radius);
            }
        }
        
        private Vector3 GenerateUniformCirclePoint(Vector3 center, float radius)
        {
            // Genera un angolo casuale e una distanza casuale
            float angle = GetRandomFloat(0, Mathf.PI * 2);
            float distance = GetRandomFloat(0, 1);
            
            // La radice quadrata garantisce una distribuzione uniforme nell'area
            distance = Mathf.Sqrt(distance) * radius;
            
            float x = Mathf.Cos(angle) * distance;
            float z = Mathf.Sin(angle) * distance;
            
            return center + new Vector3(x, 0, z);
        }
        
        private Vector3 GenerateCenteredCirclePoint(Vector3 center, float radius)
        {
            // Genera un angolo casuale e una distanza casuale
            float angle = GetRandomFloat(0, Mathf.PI * 2);
            
            // Usiamo la distribuzione quadratica per concentrare i punti verso il centro
            float distance = GetRandomFloat(0, 1);
            distance = distance * distance * radius;
            
            float x = Mathf.Cos(angle) * distance;
            float z = Mathf.Sin(angle) * distance;
            
            return center + new Vector3(x, 0, z);
        }
        
        private Vector3 GeneratePerimeterCirclePoint(Vector3 center, float radius)
        {
            // Genera un angolo casuale
            float angle = GetRandomFloat(0, Mathf.PI * 2);
            
            // Per concentrarsi sul perimetro, usiamo la radice cubica
            float distance = GetRandomFloat(0.5f, 1);
            distance = Mathf.Pow(distance, 1/3f) * radius;
            
            float x = Mathf.Cos(angle) * distance;
            float z = Mathf.Sin(angle) * distance;
            
            return center + new Vector3(x, 0, z);
        }
        
        private Vector3 GenerateRectanglePoint(Vector3 center, float width, float length)
        {
            float x, z;
            
            switch (options.Distribution)
            {
                case DistributionType.Centered:
                    // Distribuzione triangolare per favorire il centro
                    x = (GetRandomFloat(0, 1) + GetRandomFloat(0, 1) - 1) * width/2;
                    z = (GetRandomFloat(0, 1) + GetRandomFloat(0, 1) - 1) * length/2;
                    break;
                    
                case DistributionType.Perimeter:
                    // Scegli quale lato del rettangolo
                    float perimeter = 2 * (width + length);
                    float pos = GetRandomFloat(0, perimeter);
                    
                    if (pos < width) // Lato inferiore
                    {
                        x = pos - width/2;
                        z = -length/2;
                    }
                    else if (pos < width + length) // Lato destro
                    {
                        x = width/2;
                        z = pos - width - length/2;
                    }
                    else if (pos < 2 * width + length) // Lato superiore
                    {
                        x = width/2 - (pos - width - length);
                        z = length/2;
                    }
                    else // Lato sinistro
                    {
                        x = -width/2;
                        z = length/2 - (pos - 2 * width - length);
                    }
                    
                    // Aggiungi un po' di spessore al perimetro (5% verso l'interno)
                    if (GetRandomFloat(0, 1) > 0.5f)
                    {
                        x *= 0.95f;
                        z *= 0.95f;
                    }
                    break;
                    
                default: // Distribuzione uniforme
                    x = GetRandomFloat(-width/2, width/2);
                    z = GetRandomFloat(-length/2, length/2);
                    break;
            }
            
            return center + new Vector3(x, 0, z);
        }
        
        private Vector3 GenerateSpherePoint(Vector3 center, float radius)
        {
            float x, y, z;
            
            switch (options.Distribution)
            {
                case DistributionType.Uniform:
                    // Generiamo punti uniformi in una sfera
                    do
                    {
                        x = GetRandomFloat(-1, 1);
                        y = GetRandomFloat(-1, 1);
                        z = GetRandomFloat(-1, 1);
                    } while (x*x + y*y + z*z > 1); // Scarta punti fuori dalla sfera unitaria
                    
                    // Scala per il raggio desiderato
                    float distance = Mathf.Sqrt(x*x + y*y + z*z);
                    if (distance > 0)
                    {
                        float scale = radius / distance;
                        x *= scale;
                        y *= scale;
                        z *= scale;
                    }
                    break;
                    
                case DistributionType.Centered:
                    // Distribuzione che favorisce il centro
                    x = GetRandomFloat(-1, 1) * GetRandomFloat(0, 1);
                    y = GetRandomFloat(-1, 1) * GetRandomFloat(0, 1);
                    z = GetRandomFloat(-1, 1) * GetRandomFloat(0, 1);
                    
                    // Scala per il raggio desiderato
                    x *= radius;
                    y *= radius;
                    z *= radius;
                    break;
                    
                case DistributionType.Perimeter:
                    // Distribuzione sulla superficie della sfera
                    float theta = GetRandomFloat(0, Mathf.PI * 2);
                    float phi = GetRandomFloat(0, Mathf.PI);
                    
                    // Formula sferica -> cartesiana
                    float sinPhi = Mathf.Sin(phi);
                    x = Mathf.Cos(theta) * sinPhi;
                    y = Mathf.Cos(phi);
                    z = Mathf.Sin(theta) * sinPhi;
                    
                    // Aggiungi un po' di variazione del raggio (90-100%)
                    float r = radius * GetRandomFloat(0.9f, 1f);
                    x *= r;
                    y *= r;
                    z *= r;
                    break;
                    
                default:
                    x = GetRandomFloat(-radius, radius);
                    y = GetRandomFloat(-radius, radius);
                    z = GetRandomFloat(-radius, radius);
                    
                    // Vincola all'interno della sfera
                    float mag = Mathf.Sqrt(x*x + y*y + z*z);
                    if (mag > radius)
                    {
                        float scale = radius / mag;
                        x *= scale;
                        y *= scale;
                        z *= scale;
                    }
                    break;
            }
            
            return center + new Vector3(x, y, z);
        }
        
        private PointResult ValidateAndProcessPoint(Vector3 point)
        {
            // Controlla sovrapposizioni
            if (options.AvoidOverlaps && 
                Physics.CheckSphere(point, options.OverlapCheckRadius, options.OverlapLayerMask))
            {
                return new PointResult(point, false, false);
            }
            
            // Se non richiediamo validazione NavMesh, il punto è già valido
            if (!options.ValidateOnNavMesh)
            {
                Vector3 finalPoint = new Vector3(point.x, point.y + options.PointHeight, point.z);
                return new PointResult(finalPoint, true, false);
            }
            
            // Controlla se il punto è su NavMesh
            if (NavMesh.SamplePosition(point, out NavMeshHit navHit, 
                options.NavMeshSearchDistance, options.NavMeshAreaMask))
            {
                // Aggiusta l'altezza del punto
                Vector3 finalPoint = new Vector3(
                    navHit.position.x, 
                    navHit.position.y + options.PointHeight, 
                    navHit.position.z);
                
                // Controlla di nuovo sovrapposizioni al punto finale
                if (options.AvoidOverlaps && 
                    Physics.CheckSphere(finalPoint, options.OverlapCheckRadius, options.OverlapLayerMask))
                {
                    return new PointResult(finalPoint, false, true, navHit.mask);
                }
                
                return new PointResult(finalPoint, true, true, navHit.mask);
            }
            
            // Nessun punto NavMesh trovato
            return new PointResult(point, false, false);
        }
        
        #endregion
        
        #region Utility
        
        private float GetRandomFloat(float min, float max)
        {
            return Random.Range(min, max);
        }
        
        #endregion
    }
    
    #region Extensions

    /// <summary>
    /// Estensioni per RandomPointGenerator
    /// </summary>
    public static class RandomPointGeneratorExtensions
    {
        /// <summary>
        /// Crea oggetti di gioco per ciascun punto generato (utile per debugging)
        /// </summary>
        public static List<GameObject> CreateGameObjects(
            this List<RandomPointGenerator.PointResult> points,
            string namePrefix = "RandomPoint",
            GameObject prefab = null)
        {
            var result = new List<GameObject>();
            
            foreach (var point in points)
            {
                if (!point.IsValid)
                    continue;
                
                GameObject obj;
                
                if (prefab != null)
                {
                    obj = GameObject.Instantiate(prefab, point.Position, Quaternion.identity);
                }
                else
                {
                    obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    obj.transform.position = point.Position;
                    obj.transform.localScale = Vector3.one * 0.5f;
                    
                    // Colore diverso per punti su NavMesh
                    if (point.IsOnNavMesh)
                    {
                        var renderer = obj.GetComponent<Renderer>();
                        if (renderer != null)
                            renderer.material.color = Color.green;
                    }
                }
                
                obj.name = $"{namePrefix}_{result.Count}";
                result.Add(obj);
            }
            
            return result;
        }
        
        /// <summary>
        /// Disegna i punti come Gizmos nella scena (chiamare da OnDrawGizmos)
        /// </summary>
        public static void DrawGizmos(
            this List<RandomPointGenerator.PointResult> points,
            float radius = 0.5f,
            Color validColor = default,
            Color invalidColor = default)
        {
            if (validColor == default)
                validColor = Color.green;
                
            if (invalidColor == default)
                invalidColor = Color.red;
                
            foreach (var point in points)
            {
                Gizmos.color = point.IsValid ? validColor : invalidColor;
                Gizmos.DrawSphere(point.Position, radius);
            }
        }
    }
    
    #endregion
}