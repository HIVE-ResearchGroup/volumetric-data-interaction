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
        Debug.Log($"Started waiting for {seconds} Second(s).");
        yield return new WaitForSeconds(seconds);
        Debug.Log("Waiting over.");
        IsTimerElapsed = true;
        TimerElapsed?.Invoke();
    }
}
