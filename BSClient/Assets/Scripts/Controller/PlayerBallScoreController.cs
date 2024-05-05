using UnityEngine;

public class PlayerBallScoreController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HoopScoreCollider"))
        {
            BallController playerBall = GetComponentInParent<BallController>();
            Debug.Log("Touched game objects: " + playerBall.GetTouchedGameObjects().Count);
            if (playerBall.GetTouchedGameObjects().Count == 0)
            {
                RoundManager.Instance.AddPointsToPlayerAndSpawnDisappearingText(3);
                HoopController.Instance.PlayExplosionEffect();
            }
            else
            {
                if (BackBoardController.Instance.IsGlowing() && playerBall.GetTouchedGameObjects()
                        .Contains(BackBoardController.Instance.gameObject))
                {
                    RoundManager.Instance.AddPointsToPlayerAndSpawnDisappearingText(6);
                    BackBoardController.Instance.PlaySparksEffect();
                }
                else
                {
                    RoundManager.Instance.AddPointsToPlayerAndSpawnDisappearingText(2);
                    HoopController.Instance.PlayRippleEffect();
                }
            }
        }
    }
}