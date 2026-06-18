using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Stats (0 - 100)")]
    [SerializeField] private float hp = 100f;
    [SerializeField] private float armor = 0f;
    [SerializeField] private float attack = 50f;
    [SerializeField] private float atkSpeed = 50f;
    [SerializeField] private float moveSpeed = 50f;

    public float HP { get { return hp; } set { hp = value; } }
    public float Armor { get { return armor; } set { armor = value; } }
    public float Attack { get { return attack; } set { attack = value; } }
    public float AtkSpeed { get { return atkSpeed; } set { atkSpeed = value; } }
    public float MoveSpeed { get { return moveSpeed; } set { moveSpeed = value; } }

    public void TakeDamage(float damage)
    {
        // Simple damage formula for now
        float finalDamage = Mathf.Max(damage - armor, 1f);
        hp -= finalDamage;
        if (hp <= 0)
        {
            hp = 0;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        // Add death logic here
    }
}
