using UnityEngine;

public class GunMovement : MonoBehaviour
{
    [SerializeField] private Transform character;  // Il personaggio attorno al quale la pistola orbita
    [SerializeField] private Transform aimTarget;  // Il punto di mira (ad esempio, il punto dove il personaggio sta guardando)
    [SerializeField] private float orbitSpeed = 30f;  // Velocità di orbita della pistola
    [SerializeField] private float verticalOffset = 1f;  // Altezza verticale della pistola rispetto al personaggio

    void Update()
    {

        character.position = Vector3.Lerp(character.position, aimTarget.position, Time.deltaTime * orbitSpeed);
        character.rotation = Quaternion.Slerp(character.rotation, aimTarget.rotation, Time.deltaTime * orbitSpeed);

    }




}
