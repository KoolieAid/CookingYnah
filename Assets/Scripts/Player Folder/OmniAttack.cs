using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmniAttack : MonoBehaviour
{
    private List<Enemy> enemiesInRange = new List<Enemy>();
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" && !other.isTrigger)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            enemiesInRange.Add(enemy);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy" && !other.isTrigger)
        {
            enemiesInRange.Remove(other.GetComponent<Enemy>());
        }
    }

    public void DealDamage(int damage)
    {
        foreach (Enemy enemy in enemiesInRange)
        {
            DamageHandler.ApplyDamage(enemy, damage);
        }
    } 
}
