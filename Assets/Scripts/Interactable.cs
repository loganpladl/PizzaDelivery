using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    float radius = 3.0f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
