using System;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private readonly string playerTag = "Player";
    public static event Action OnStartRace;
    public static event Action OnFinishRace;
    private bool _startRace = false;

    private void OnTriggerExit2D(Collider2D other)
    {
        var direction = (other.transform.position - transform.position).normalized;

        if (other.CompareTag(playerTag) && !_startRace)
        {
            if (direction.x > 0.0f)
            {
                _startRace = true;
                OnStartRace?.Invoke();
            }
        }

        if (_startRace && direction.x < 0.0f) _startRace = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var direction = (other.transform.position - transform.position).normalized;

        if (other.CompareTag(playerTag) && _startRace)
        {
            if (direction.x < 0.0f)
            {
                _startRace = false;
                OnFinishRace?.Invoke();
            }
        }
    }
}