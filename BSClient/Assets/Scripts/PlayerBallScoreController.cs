using UnityEngine;

public class PlayerBallScoreController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HoopScoreCollider"))
        {
            RoundManager.Instance.AddPointsToPlayerScore(2);
        }
    }
}
