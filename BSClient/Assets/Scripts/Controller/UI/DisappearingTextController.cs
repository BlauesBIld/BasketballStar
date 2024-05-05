using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisappearingTextController : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subTitleText;

    private void Start()
    {
        StartCoroutine(MoveUpReduceAlphaAndDestroyAfterDelay());
    }

    private IEnumerator MoveUpReduceAlphaAndDestroyAfterDelay()
    {
        float elapsedTime = -0.5f;
        float duration = 1f;
        float delay = 0.5f;
        float speed = 200f;
        float alpha = 1;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.position += Vector3.up * speed * Time.deltaTime;
            if (elapsedTime > 0)
            {
                alpha = Mathf.Lerp(1, 0, elapsedTime / duration);
                titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, alpha);
                subTitleText.color = new Color(subTitleText.color.r, subTitleText.color.g, subTitleText.color.b, alpha);
            }

            yield return null;
        }

        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void SetText(string title, string subTitle = "")
    {
        titleText.text = title;
        subTitleText.text = subTitle;
    }

    public void SetColor(Color color)
    {
        titleText.color = color;
        subTitleText.color = color;
    }
}