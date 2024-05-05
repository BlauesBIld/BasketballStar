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
                RoundManager.Instance.AddPointsToPlayer(3);
                //TODO: Add effects for perfect shot
            }
            else
            {
                if (BackBoardController.Instance.IsGlowing() && playerBall.GetTouchedGameObjects()
                    .Contains(BackBoardController.Instance.gameObject))
                {
                    RoundManager.Instance.AddPointsToPlayer(6);
                    //TODO: Add effects for backboard shot while it glows
                }
                else
                {
                    RoundManager.Instance.AddPointsToPlayer(2);
                }
            }

            HoopController.Instance.PlayRippleEffect();
        }
    }
}
