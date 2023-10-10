using System;
using System.Collections;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public event Action TimerElapsed;

    public bool IsTimerElapsed { get; private set; } = true;

    public void StartTimerSeconds(float seconds)
    {
        IsTimerElapsed = false;
        StartCoroutine(Waiting(seconds));
    }

    private IEnumerator Waiting(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        IsTimerElapsed = true;
        TimerElapsed?.Invoke();
    }
}
