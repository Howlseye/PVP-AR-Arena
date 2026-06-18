using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.name.ToLower().Contains("dummy") || (other.transform.root != null && other.transform.root.name.ToLower().Contains("dummy")))
        {
            var controller = GetComponentInParent<ARCharacterController>();
            if (controller != null && controller.IsAttacking())
            {
                if (ARCharacterSelectionManager.Instance != null)
                {
                    ARCharacterSelectionManager.Instance.PlayDummyPushedAnimation();
                }
            }
        }
    }
}
