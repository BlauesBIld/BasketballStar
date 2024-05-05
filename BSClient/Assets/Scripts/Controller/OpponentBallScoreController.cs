using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentBallScoreController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HoopScoreCollider"))
        {
            int points = 0;
            BallController opponentBall = GetComponentInParent<BallController>();
            if (opponentBall.GetTouchedGameObjects().Count == 0)
            {
                points = 3;
                HoopController.Instance.PlayExplosionEffect();
            }
            else
            {
                if (BackBoardController.Instance.IsGlowing() && opponentBall.GetTouchedGameObjects()
                        .Contains(BackBoardController.Instance.gameObject))
                {
                    points = 6;
                    BackBoardController.Instance.PlaySparksEffect();
                }
                else
                {
                    points = 2;
                    HoopController.Instance.PlayRippleEffect();
                }
            }

            if (opponentBall.owner.GetComponent<AIController>() != null)
            {
                RoundManager.Instance.AddPointsToOpponent(opponentBall.owner.GetComponent<OpponentController>(),
                    points);
            }
        }
    }
}