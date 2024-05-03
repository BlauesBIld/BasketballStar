using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentController : MonoBehaviour
{
    public BallController ballController;
    public Transform positionAboveHead;

    public void SetName(string name)
    {
        gameObject.name = name;
    }
}