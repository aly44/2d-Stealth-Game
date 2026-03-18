using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    // when player enters, call GameManager.PlayerWon() to end the game
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.PlayerWon();
        }
    }
}
