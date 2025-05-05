using System;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private float remainingTime;
    private bool isRunning = false;

    public Action<object[]> OnComplete;


    void Update()
    {
        if (isRunning)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0f)
            {
                Debug.LogError("Timer completed!");
                remainingTime = 0f;
                isRunning = false;
                OnComplete?.Invoke(null);
            }
        }
    }

    public void StartTimer(float duration = 5f)
    {
        remainingTime = duration;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public float GetRemainingTime()
    {
        return remainingTime;
    }

    public bool IsRunning()
    {
        return isRunning;
    }
}
