using System.Collections;
using UnityEngine;

public abstract class BaseAnimation : MonoBehaviour
{
    public float duration = 1f;
    public abstract void Play();
}