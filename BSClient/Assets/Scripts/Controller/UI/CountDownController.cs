using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountDownController : MonoBehaviour
{
    public TextMeshProUGUI countDownText;
    public static CountDownController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd CountDownController Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    public void StartCountdownAndRoundAfter()
    {
        gameObject.SetActive(true);
        StartCoroutine(CountdownAndStartRoundAfter());
    }

    private IEnumerator CountdownAndStartRoundAfter()
    {
        countDownText.text = "3";
        yield return RotateCountDownObjectFromLeftToRight();
        countDownText.text = "2";
        yield return RotateCountDownObjectFromLeftToRight();
        countDownText.text = "1";
        yield return RotateCountDownObjectFromLeftToRight();
        countDownText.text = "GO!";
        yield return RotateCountDownObjectFromLeftToRight();
        countDownText.text = "";
        RoundManager.Instance.StartRound();
        gameObject.SetActive(false);
    }

    private IEnumerator RotateCountDownObjectFromLeftToRight()
    {
        float duration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration * 0.3f)
        {
            float zRotation = Mathf.Lerp(45, 0, elapsedTime / (duration * 0.3f));
            transform.rotation = Quaternion.Euler(0, 0, zRotation);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, 0, 0);

        yield return new WaitForSeconds(duration * 0.4f);

        elapsedTime = 0f;
        while (elapsedTime < duration * 0.3f)
        {
            float zRotation = Mathf.Lerp(0, -45, elapsedTime / (duration * 0.3f));
            transform.rotation = Quaternion.Euler(0, 0, zRotation);
            elapsedTime += Time.deltaTime;

            yield return null;
        }
    }
}