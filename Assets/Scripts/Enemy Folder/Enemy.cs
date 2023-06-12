using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Asyncoroutine;
using UnityEngine.AI;



public class Enemy : MonoBehaviour
{
    [Header("Unit DropData")]
    [SerializeField] private EnemyUnitData enemyUnitData;

    private EnemyUnitData enemyDataInstance;
    [SerializeField] private SphereCollider aggroTrigger;
    [SerializeField] private GameObject targetUnit;
    [SerializeField] private Vector3 home;

    [Header("SFX")]
    public Material redMaterial;
    public Material greenMaterial;

    [Header("Health UI")]
    public Image hpBar;

    private NavMeshAgent agent;
    private bool isAlive = true;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        DamageHandler.OnEnemyUnitDeath += Death;
    }
    private void OnDisable()
    {
        DamageHandler.OnEnemyUnitDeath -= Death;
    }

    // Start is called before the first frame update
    void Start()
    {
        // enemyDataInstance = new EnemyUnitData();

        SetData(10001); // To be called and set by spawner 

        MonsterStateManager.Instance.AddMonster(this, new IdleState(MonsterStateManager.Instance, this));
        agent = GetComponent<NavMeshAgent>();

    }

    public void SetData(int enemyID)
    {
        enemyDataInstance = ScriptableObject.CreateInstance<EnemyUnitData>();
        enemyDataInstance.UnitID = enemyID;
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
    }

    public bool IsAlive()
    {
        return isAlive;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            targetUnit = other.gameObject;
            aggroTrigger.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float a = enemyDataInstance.CurrentHealth;
        float b = enemyDataInstance.MaxHealth;
        float normalized = a / b;

        hpBar.fillAmount = normalized;

        if (agent.hasPath)
        {
            Vector3 direction = agent.velocity.normalized;

            //For the animation direction(to be added)
        }
    }

    public void ResetAggro()
    {
        targetUnit = null;
        aggroTrigger.enabled = true;
    }

    public Vector3 GetHome()
    {
        return home;
    }
    public GameObject GetTargetUnit()
    {
        return targetUnit;
    }
    public EnemyUnitData GetEnemyUnitData()
    {
        return enemyDataInstance;
    }

    public async void Hit()// To be replaced by animations 
    {
        GetComponentInChildren<Renderer>().material = redMaterial;
        await new WaitForSeconds(0.5f);
        GetComponentInChildren<Renderer>().material = greenMaterial;
    }

    void Death(int id)
    {
        if (id != enemyDataInstance.UnitID) { return; }

        isAlive = false;

        GameObject clone = Instantiate(enemyDataInstance.DropObject, transform.position, Quaternion.identity);
        clone.GetComponent<Item>().SetData(enemyDataInstance.DropData);

        transform.GetComponent<NavMeshAgent>().enabled = false;

        Vector3 position = transform.position;
        position.y -= 40f;
        transform.position = position;

        Destroy(gameObject, 3.0f);
    }

    public void DealDamage()
    {
        if (targetUnit)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetUnit.transform.position);

            if (distanceToTarget <= enemyDataInstance.AttackRange)
            {
                DamageHandler.ApplyDamage(targetUnit.GetComponent<Player>(), enemyDataInstance.BasicAttackDamage);
            }
        }
    }

    public void ControlAnimations(MonsterStates state, bool isPlaying)
    {
        ResetAnimatorBool();

        var s = state;
        switch (state)
        {
            case MonsterStates.Attack:
                animator.SetBool("isAttacking", isPlaying);
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

    private void ResetAnimatorBool()
    {
        animator.SetBool("isAttacking", false);
        animator.SetBool("isChasing", false);
        animator.SetBool("isIdling", false);
        animator.SetBool("isPatrolling", false);
    }
}
