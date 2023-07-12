using Asyncoroutine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    private Vector3 targetPosition;
    private bool move;
    private int damageValue;

    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
    }
    public void SetDamageValue(float dmg)
    {
        damageValue = (int)dmg;
    }

    public void TriggerMove()
    {
        move = true;
    }

    private void Update()
    {
        float speed = 5f;

        if (move)
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            if (other.CompareTag("Player"))
                DealDamage(other.GetComponent<Player>());

            if (other.gameObject.CompareTag("Enemy"))
                DealDamage(other.GetComponent<Enemy>());

            Destroy(gameObject);
        }
    }

    private void DealDamage(Player player)
    {
        DamageHandler.ApplyDamage(player, damageValue);
    }
    private void DealDamage(Enemy enemy)
    {
        DamageHandler.ApplyDamage(enemy, damageValue);
    }
}
