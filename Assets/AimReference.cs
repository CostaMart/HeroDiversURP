using System;
using System.Linq;
using UnityEngine;

public class AimFollowReference : MonoBehaviour
{
    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private Transform aimTransform;

    void FixedUpdate()
    {

        transform.position = Vector3.Lerp(transform.position, aimTransform.position,
        dispatcher.GetAllFeatureByType<float>(FeatureType.aimRotationSpeed).Sum() * Time.fixedDeltaTime);
    }
}
