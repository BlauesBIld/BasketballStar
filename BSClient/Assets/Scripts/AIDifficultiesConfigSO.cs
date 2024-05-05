using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AIDifficultiesConfigSO", menuName = "Configs/AI Difficulties Config")]
public class AIDifficultiesConfigSO : ScriptableObject
{
    public List<DifficultySetting> difficulties;
}

[System.Serializable]
public class DifficultySetting
{
    public string difficultyName;
    public float minDelayBeforeThrow;
    public float maxDelayBeforeThrow;
    public float maxThrowPowerError;
}