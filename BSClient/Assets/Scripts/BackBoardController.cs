using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackBoardController : MonoBehaviour
{
    public static BackBoardController Instance { get; private set; }

    public float glowDuration = 0f;
    public float glowStartTime = 0f;

    public GameObject backBoardGlow;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd BackBoardController Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RoundManager.Instance.RoundStartedEvent += SetRandomGlowTime;
        RoundManager.Instance.RoundStartedEvent += StartGlowingAfterSetTime;
    }

    private void StartGlowingAfterSetTime()
    {
        StartCoroutine(GlowAfterStartTime());
    }

    private IEnumerator GlowAfterStartTime()
    {
        yield return new WaitForSeconds(glowStartTime);
        StartGlowing();
        yield return new WaitForSeconds(glowDuration);
        StopGlowing();
    }

    private void StartGlowing()
    {
        backBoardGlow.SetActive(true);
        Debug.Log("Started Glowing!");
    }

    private void StopGlowing()
    {
        backBoardGlow.SetActive(false);
        Debug.Log("Stopped Glowing!");
    }

    public void SetRandomGlowTime()
    {
        float maxRoundTime = RoundManager.Instance.GetRoundTime();
        float maxGlowDuration = maxRoundTime / 2;
        float minGlowDuration = maxRoundTime / 4;

        glowDuration = UnityEngine.Random.Range(minGlowDuration, maxGlowDuration);
        glowStartTime = UnityEngine.Random.Range(0, maxRoundTime - glowDuration);

        Debug.Log("Glow Duration: " + glowDuration);
        Debug.Log("Glow Start Time: " + glowStartTime);
    }

    public bool IsGlowing()
    {
        return backBoardGlow.activeSelf;
    }
}