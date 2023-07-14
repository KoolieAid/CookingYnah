using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Asyncoroutine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Unit DropData")]
    protected EnemyUnitData enemyUnitData;
    protected EnemyUnitData enemyDataInstance;

    [SerializeField] protected SphereCollider aggroTrigger;
    [SerializeField] protected GameObject targetUnit;
    protected bool canAttack = true;
    protected bool isAttackDone = false;
    protected Vector3 home;

    [Header("Health UI")]
    public GameObject hpBarGameObject;
    public Image hpBar;
    protected Coroutine hpBarCoroutine;

    protected NavMeshAgent agent;
    protected bool isAlive = true;

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected Transform spriteTransform;

    protected virtual void OnEnable()
    {
        DamageHandler.OnEnemyUnitDeath += Death;
    }
    protected virtual void OnDisable()
    {
        DamageHandler.OnEnemyUnitDeath -= Death;
    }

    public virtual void SetEnemyData(int enemyID, EnemyUnitData unitData, Vector3 homeBase)
    {
        enemyDataInstance = ScriptableObject.CreateInstance<EnemyUnitData>();

        enemyDataInstance.UnitID = enemyID;
        enemyUnitData = unitData;
        home = homeBase;

        enemyDataInstance.MaxHealth = enemyUnitData.MaxHealth;
        enemyDataInstance.CurrentHealth = enemyDataInstance.MaxHealth;
        enemyDataInstance.UnitName = enemyUnitData.UnitName;
        enemyDataInstance.MoveSpeed = enemyUnitData.MoveSpeed;
        enemyDataInstance.DropObject = enemyUnitData.DropObject;
        enemyDataInstance.DropData = enemyUnitData.DropData;
        enemyDataInstance.AggroRange = enemyUnitData.AggroRange;
        enemyDataInstance.BasicAttackDamage = enemyUnitData.BasicAttackDamage;
        enemyDataInstance.ChaseRange = enemyUnitData.ChaseRange;
        enemyDataInstance.AttackRange = enemyUnitData.AttackRange;
        enemyDataInstance.PatrolSpeed = enemyUnitData.PatrolSpeed;
        enemyDataInstance.ChaseSpeed = enemyUnitData.ChaseSpeed;
        enemyDataInstance.AttackSpeed = enemyUnitData.AttackSpeed;

        aggroTrigger.radius = enemyDataInstance.AggroRange;

        animator.runtimeAnimatorController = enemyUnitData.Controller;

        MonsterStateManager.Instance.AddMonster(this, new IdleState(MonsterStateManager.Instance, this));
        agent = GetComponent<NavMeshAgent>();

    }

    public virtual bool IsAlive()
    {
        return isAlive;
    }
    public virtual bool GetCanAttack()
    {
        return canAttack;
    }
    public virtual void SetCanAttack(bool isAttackPossible)
    {
        canAttack = isAttackPossible;
    }
    public virtual bool GetIsAttackDone()
    {
        return isAttackDone;
    }
    public virtual void SetIsAttackDone(bool hasAttackFinished)
    {
        isAttackDone = hasAttackFinished;
    }

    public virtual async void AttackTimer()
    {
        await new WaitForSeconds(enemyDataInstance.AttackSpeed);
        SetCanAttack(true);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            targetUnit = other.gameObject;
            aggroTrigger.enabled = false;
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        float a = enemyDataInstance.CurrentHealth;
        float b = enemyDataInstance.MaxHealth;
        float normalized = a / b;

        hpBar.fillAmount = normalized;

        if (agent.hasPath)
        {
            Vector3 direction = agent.velocity.normalized;

            spriteTransform.rotation = Quaternion.Euler(new Vector3(0f, direction.x >= 0.08 ? -180f : 0f, 0f));
        }
    }


    public virtual void ResetAggro()
    {
        targetUnit = null;
        aggroTrigger.enabled = true;
    }

    public virtual Vector3 GetRandomPatrolPoint()
    {
        Vector3 randomPoint = home + UnityEngine.Random.insideUnitSphere * 7.0f;
        NavMesh.SamplePosition(randomPoint, out NavMeshHit point, 7.0f, NavMesh.AllAreas);
        return point.position;
    }
    public virtual GameObject GetTargetUnit()
    {
        return targetUnit;
    }
    public virtual EnemyUnitData GetEnemyUnitData()
    {
        return enemyDataInstance;
    }

    public virtual async void Hit() 
    {
        var r = GetComponentsInChildren<SpriteRenderer>();

        foreach (var m in r)
        {
            m.color = Color.red;
        }
        await new WaitForSeconds(0.5f);

        foreach (var m in r)
        {
            m.color = new Color(255, 255, 255, 255);
        }
    }

    protected virtual void Death(int id)
    {
        if (id != enemyDataInstance.UnitID) { return; }

        isAlive = false;

        ItemData data = GetRandomItemData();

        GameObject clone = Instantiate(enemyDataInstance.DropObject, transform.position, Quaternion.identity);
        clone.GetComponent<Item>().SetData(data);

        transform.GetComponent<NavMeshAgent>().enabled = false;

        Vector3 position = transform.position;
        position.y -= 40f;
        transform.position = position;

        Destroy(gameObject, 3.0f);
    }

    protected virtual ItemData GetRandomItemData()
    {
        float totalWeight = 0f;
        foreach (float dropChance in enemyDataInstance.DropData.Values)
        {
            totalWeight += dropChance;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);

        ItemData data = null;
        foreach (var entry in enemyDataInstance.DropData)
        {
            randomValue -= entry.Value;
            if (randomValue <= 0f)
            {
                data = entry.Key;
                break;
            }
        }

        return data;
    }

    public virtual void ExecuteAttack()
    {
        if (targetUnit)
        {
            Vector3 direction = targetUnit.transform.position - transform.position;
            direction.Normalize();
            spriteTransform.rotation = Quaternion.Euler(new Vector3(0f, direction.x >= 0.08 ? -180f : 0f, 0f));

            AttackTimer();
            DamageHandler.ApplyDamage(targetUnit.GetComponent<Player>(), enemyDataInstance.BasicAttackDamage);
        }
    }

    public virtual void ControlAnimations(MonsterStates state, bool isPlaying)
    {
        ResetAnimatorBool();

        var s = state;
        switch (s)
        {
            case MonsterStates.Attack:
                animator.SetBool("isAttacking", isPlaying);
                break;
            case MonsterStates.Combat:
                animator.SetBool("isInCombat", isPlaying);
                break;
            case MonsterStates.Chase:
                animator.SetBool("isChasing", isPlaying);
                break;
            case MonsterStates.Idle:
                animator.SetBool("isIdling", isPlaying);
                break;
            case MonsterStates.Patrol:
                animator.SetBool("isPatrolling", isPlaying);
                break;

        }
    }

    protected virtual void ResetAnimatorBool()
    {
        animator.SetBool("isAttacking", false);
        animator.SetBool("isInCombat", false);
        animator.SetBool("isChasing", false);
        animator.SetBool("isIdling", false);
        animator.SetBool("isPatrolling", false);
    }

    public void ShowHPBar()
    {
        if (hpBarCoroutine != null)
        {
            StopCoroutine(hpBarCoroutine);
        }

        hpBarGameObject.SetActive(true);
        hpBarCoroutine = StartCoroutine(HideHPBarAfterDelay());
    }

    protected IEnumerator HideHPBarAfterDelay()
    {
        yield return new WaitForSeconds(3.0f);
        hpBarGameObject.SetActive(false);
    }
}
