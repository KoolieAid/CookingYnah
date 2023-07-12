using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Asyncoroutine;

public class Jab : MonoBehaviour
{
    [SerializeField] [Min(0)] private float timeBetweenAttacks;

    private Coroutine timer = null;

    [SerializeField] private uint threshold = 3;
    
    [Range(0, 500)] [SerializeField] private float range;

    [SerializeField] [Min(0)] private float damage;
    [Range(1, 100)] [SerializeField] private float damageMultiplier;

    [Header("Cooldowns")]
    [SerializeField] private float shortAtkCd;
    [SerializeField] private float comboAtkCd;

    [Header("Colliders")]
    [SerializeField] private OmniAttack cone;
    [SerializeField] private OmniAttack sphere;

    private Coroutine cdRoutine;
    
    internal uint internalCounter = 0;

    public (bool, bool) Attack()
    {
        if (Cooldown(shortAtkCd))
        {
            // On ability cooldown
            return (false, false);
        }
        
        StartTimer(); // Combo timer

        if (internalCounter >= threshold)
        {
            Slow();
            internalCounter = 0;
            return (true, true);
        }

        Fast();
        return (true, false);
    }

    private async void Fast()
    {
        Debug.Log("boom");
        
        cone.DealDamage(Convert.ToInt32(damage));
    }

    private void Slow()
    {
        Debug.Log("pow");
        
        // Override cooldown
        StopCoroutine(cdRoutine);
        cdRoutine = null;
        Cooldown(comboAtkCd);

        var dmg = damage * damageMultiplier;

        sphere.DealDamage(Convert.ToInt32(dmg));
    }

    // TODO: Maybe have this function be common on all abilities. i.e. Put on base class; Have a utility class; etc 
    private Vector3 GetDirection()
    {
        // Getting direction towards mouse
        Vector3 startingPos = transform.position;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, startingPos);

        float distance;
        Vector3 endingPos = Vector3.zero;

        if (plane.Raycast(ray, out distance))
        {
            endingPos = ray.GetPoint(distance);
        }

        Vector3 direction = endingPos - startingPos;
        direction.y = 0f;

        return direction.normalized;
    }

    [Obsolete("Colliders are now being used instead of RayCasting")]
    private bool RayCast(out GameObject obj)
    {
        obj = null;

        Debug.DrawLine(transform.position, transform.position + GetDirection() * range, Color.blue, 1f, false);

        var ray = new Ray(transform.position, GetDirection());
        if (!Physics.Raycast(ray, out var info, range)) return false;

        Debug.DrawLine(transform.position, info.point, Color.red, 1f, false);

        Debug.Log($"hit {info.collider.name}");

        obj = info.collider.gameObject;

        return true;
    }

    public void Update()
    {
        // // TODO: Remove testing code
        // if (!Input.GetMouseButtonDown(1)) return;
        //
        // Attack();
    }

    private IEnumerator _timerReset()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        internalCounter = 0;
        timer = null;
    }

    private void StartTimer()
    {
        internalCounter++;
        if (timer == null)
            timer = StartCoroutine(_timerReset());
    }

    private bool Cooldown(float time)
    {
        if (cdRoutine == null)
        {
            cdRoutine = StartCoroutine(_Cd(time));
            return false; // No cd
        }

        return true; // On Cd
    }

    private IEnumerator _Cd(float time)
    {
        yield return new WaitForSeconds(time);
        cdRoutine = null;
    }

}
