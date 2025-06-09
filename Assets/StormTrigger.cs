using UnityEngine;

public class StormTrigger : MonoBehaviour
{
    [SerializeField] Adversity storm;
    [SerializeField] int prob = 50; // Probability of storm triggering, 0-100
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            int choosen = Random.Range(0, 100);

            if (choosen < prob)
            {
                ClimateEffectController.Instance.InjectAdversity(storm);

                Debug.Log("Storm triggered");
            }
        }
    }



}
