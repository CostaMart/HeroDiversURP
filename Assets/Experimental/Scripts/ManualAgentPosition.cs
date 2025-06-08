// using UnityEngine;

// namespace Utility
// {
//     public class ManualAgentPosition : InteractiveObject
//     {
//         private AgentController agentController;

//         protected override void Awake()
//         {
//             base.Awake();
//             name = "Player";
//         }
        
//         void Start()
//         {
//             agentController = GetComponent<AgentController>();
//             EntityManager.Instance.RegisterEntity(name, gameObject);
//             RegisterAction("MoveTo", MoveTo);
//             RegisterEvent("OnMoveToComplete");
//         }
        
//         private void Update()
//         {
//             if (Input.GetMouseButtonDown(0))
//             {
//                 Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//                 if (Physics.Raycast(ray, out RaycastHit hit))
//                 {
//                     agentController.MoveTo(hit.point);
//                 }
//             }
//         }

//         private void MoveTo(object[] parameters)
//         {
//             if (parameters.Length > 0 && parameters[0] is Vector3 targetPosition)
//             {
//                 agentController.MoveTo(targetPosition);
//             }
//             else
//             {
//                 // Debug.LogWarning("Invalid parameters for MoveTo action.");
//                 // Generate random position within a certain range
//                 var randomPosition = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
//                 agentController.MoveTo(randomPosition); 
//             }
//         }
//     }
// }