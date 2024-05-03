using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopController : MonoBehaviour
{
    public static HoopController Instance { get; private set; }

    public ParticleSystem rippleEffect;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd HoopController Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        rippleEffect.Stop();
    }

    public void PlayRippleEffect()
    {
        rippleEffect.Play();
    }
}