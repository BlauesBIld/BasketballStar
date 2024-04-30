using UnityEngine;

public class PlayerBallScoreController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered Trigger with tag: " + other.tag);
        if (other.CompareTag("HoopScoreCollider"))
        {
            Debug.Log("Player scored a point!");
            RoundManager.Instance.AddPointsToPlayerScore(2);
        }
    }
}