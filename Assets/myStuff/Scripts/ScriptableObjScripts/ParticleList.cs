using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParticleList", menuName = "ParticleList/particles")]
public class ParticleList : ScriptableObject
{
    [SerializeField] public List<ParticleSystem> particles = new List<ParticleSystem>();
}
