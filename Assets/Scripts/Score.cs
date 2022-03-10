using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    private TextMeshProUGUI _textMeshPro = null;
    private float _initialTime = 0.0f;
    private bool _runTimer = false;
    [SerializeField] private List<float> _timers = new List<float>(3);
    [SerializeField] private TextMeshProUGUI[] _texts;

    private void OnEnable()
    {
        FinishLine.OnStartRace += OnStartRace;
        FinishLine.OnFinishRace += OnFinishRace;
    }

    private void OnDisable()
    {
        FinishLine.OnStartRace -= OnStartRace;
        FinishLine.OnFinishRace -= OnFinishRace;
    }

    private void Awake() => _textMeshPro = GetComponent<TextMeshProUGUI>();

    private void Update() => RunTimer();

    private void OnStartRace()
    {
        _runTimer = true;
        _initialTime = Time.time;
    }

    private void OnFinishRace()
    {
        _runTimer = false;
        StopTimer();
        AddTime(Time.time - _initialTime);
        SetTexts();
    }

    private void RunTimer()
    {
        if (!_runTimer) return;
        _textMeshPro.SetText(FormatTime(Time.time - _initialTime));
    }

    private void StopTimer()
    {
        _textMeshPro.SetText(FormatTime(0.0f));
    }

    private void AddTime(float time)
    {
        var capacity = _timers.Capacity - 1;
        if (_timers.Count > capacity) _timers.RemoveAt(capacity);

        _timers.Add(time);
        _timers.Sort();
    }

    private void SetTexts()
    {
        var index = 0;
        foreach (var text in _texts)
        {
            if (index > _timers.Count - 1) break;
            text.SetText(FormatTime(_timers[index]));
            index++;
        }
    }

    private string FormatTime(float t)
    {
        var time = TimeSpan.FromSeconds(t);
        return time.ToString("mm':'ss'.'fff");
    }
}