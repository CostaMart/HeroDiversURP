using UnityEngine;

namespace Utility
{
    public class ManualAgentPosition : MonoBehaviour
    {
        private AgentController agentController;
        
        private void Start()
        {
            agentController = GetComponent<AgentController>();
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    agentController.MoveTo(hit.point);
                }
            }
        }
    }
}