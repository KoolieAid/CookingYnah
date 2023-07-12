using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ArtifactAbility : MonoBehaviour
{

    [Header("Spawning")]
    [SerializeField] protected GameObject attackPrefab;

    [SerializeField] private int slotNum;

    // Update is called once per frame
    protected virtual void Update()
    {
        if (Input.GetButtonDown("Artifact " + slotNum.ToString()) && gameObject.GetComponent<Player>())
        {
            UseSpecialAttack();
        }
    }

    protected abstract void UseSpecialAttack();

    public virtual void SetAbilityData(GameObject obj, int slotIndex)
    {
        attackPrefab = obj;
        slotNum = slotIndex;
    }
}
