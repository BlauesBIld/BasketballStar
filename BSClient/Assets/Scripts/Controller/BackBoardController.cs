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
    public GameObject backBoardOuterGlow;

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
    }

    private void StopGlowing()
    {
        backBoardGlow.SetActive(false);
    }

    public void SetRandomGlowTime()
    {
        float maxRoundTime = RoundManager.Instance.GetRoundTime();
        float maxGlowDuration = maxRoundTime / 2;
        float minGlowDuration = maxRoundTime / 4;

        glowDuration = UnityEngine.Random.Range(minGlowDuration, maxGlowDuration);
        glowStartTime = UnityEngine.Random.Range(0, maxRoundTime - glowDuration);
    }

    public bool IsGlowing()
    {
        return backBoardGlow.activeSelf;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            if (IsGlowing() && CheckIfBallDidNotTouchHoop(collision) && collision.transform.position.y > HoopController.Instance.hoopCenter.position.y)
            {
                backBoardOuterGlow.SetActive(true);
                StartCoroutine(StopOuterGlowAfterHalfASecond());
            }
        }
    }
    static bool CheckIfBallDidNotTouchHoop(Collision collision)
    {
        return !collision.gameObject.GetComponent<BallController>().GetTouchedGameObjects().Contains(HoopController.Instance.gameObject);
    }
    IEnumerator StopOuterGlowAfterHalfASecond()
    {
        yield return new WaitForSeconds(0.5f);
        if (backBoardOuterGlow.activeSelf)
        {
            backBoardOuterGlow.SetActive(false);
        }
    }
}
