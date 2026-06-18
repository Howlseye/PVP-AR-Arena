using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float hp = 100f;
    [SerializeField] private float armor = 0f;

    public float HP 
    { 
        get { return hp; }
        set { hp = value; }
    }

    public float Armor 
    { 
        get { return armor; }
        set { armor = value; }
    }

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
