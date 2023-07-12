using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Asyncoroutine;

public class Player : MonoBehaviour
{   
    [Header("Unit DropData")]
    [SerializeField] private PlayerUnitData playerUnitData;
    private PlayerUnitData playerDataInstance;
    private Rigidbody rb;
    public float force;

    [Header("OmniAttack")]
    [SerializeField] private OmniAttack attack;
    [SerializeField] private SphereCollider attackCollider;
    private bool canAttack = true;

    [SerializeField] private Jab jab;

    [Header("Inventory")]
    [SerializeField] private PlayerInventory inventory;

    private bool isAtCookingStation = false;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem attackParticle;

    private void Awake()
    {
        
        playerDataInstance = ScriptableObject.CreateInstance<PlayerUnitData>();
        SetInitialValues();
        
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        inventory = GetComponent<PlayerInventory>();

        UIManager.instance.UpdateHpUI();
        BuffManager.instance.SetPlayer(playerDataInstance);

        if (SceneChangeManager.instance.GetObjectToLoad() != gameObject) { Destroy(gameObject); }
    }

    void SetInitialValues()
    {

        playerDataInstance.UnitName = playerUnitData.UnitName;

        playerDataInstance.Vitality = playerUnitData.Vitality;
        playerDataInstance.Agility = playerUnitData.Agility;
        playerDataInstance.Strength = playerUnitData.Strength;
        playerDataInstance.Vigor = playerUnitData.Vigor;
        playerDataInstance.Intelligence = playerUnitData.Intelligence;
        playerDataInstance.Endurance = playerUnitData.Endurance;
        playerDataInstance.Dexterity = playerUnitData.Dexterity;
        playerDataInstance.MoveSpeed = playerUnitData.MoveSpeed;
        playerDataInstance.MaxHealth = playerUnitData.MaxHealth;
        playerDataInstance.CurrentHealth = playerDataInstance.MaxHealth;
        playerDataInstance.AttackRange = playerUnitData.AttackRange;
    }

    // Update is called once per frame
    void Update()
    {

        // TODO: Synchronize animations, since canAttack got replaced
        if (Input.GetButtonDown("Fire1")) // I removed canAttack for Omni since it got replaced by jab
        {
            //attackCollider.enabled = true;
            Attack();
            //animator.SetTrigger("attackTrigger");
        }


        if (Input.GetButton("Horizontal"))
        {
            var val = Input.GetAxis("Horizontal");
            rb.AddForce(new Vector3(val, 0, 0) * force * Time.deltaTime, ForceMode.Force);
        }

        if (Input.GetButton("Vertical"))
        {
            var val = Input.GetAxis("Vertical");
            rb.AddForce(new Vector3(0, 0, val) * force * Time.deltaTime, ForceMode.Force);
        }

        AnimateMovement();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Cook Station"))
        {
            isAtCookingStation = true;
            UIManager.instance.SetCraftingPopUp();
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Cook Station"))
        {
            isAtCookingStation = false;
            UIManager.instance.SetCraftingPopUp();
        }
    }

    public bool GetNearStation()
    {
        return isAtCookingStation;
    }

    private void AnimateMovement()
    {
        animator.SetFloat("MoveX", rb.velocity.x);
        animator.SetFloat("MoveY", rb.velocity.z);

        transform.rotation = Quaternion.Euler(new Vector3(0, rb.velocity.x > 0 ? 180f : 0f, 0));
    }

    private void Attack()
    {
        canAttack = false;

        attackCollider.radius = playerDataInstance.AttackRange;
        // attack.DealDamage((int)playerDataInstance.RawDamage);

        var (attacked, isSlow) = jab.Attack();
        
        if (!attacked) return;

        if (!isSlow)
        {
            attackParticle.Play();
            animator.ResetTrigger("Attack Finish");
            animator.SetTrigger("Attack Start");
        }
        else
        {
            throw new NotImplementedException("No slow attack animation yet");
        }

        // Debug.Log($"{playerUnitData.AttackInterval}");
        // await new WaitForSeconds(playerUnitData.AttackInterval);

        // Debug.Log("done attacking");
        

        // canAttack = true;
    }


    public PlayerInventory GetInventory()
    {
        return inventory;
    }
    public PlayerUnitData GetPlayerData()
    {
        return playerDataInstance;
    }
}
