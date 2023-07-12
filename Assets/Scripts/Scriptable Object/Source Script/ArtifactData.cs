using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewArtifactData", menuName = "Artifact/Artifact Data")]
public class ArtifactData : ScriptableObject
{
    [Header("ArtifactAbility Data")]
    [SerializeField] private string artifactName;
    [TextArea]
    [SerializeField] private string artifactDescription;

    [SerializeField]
    private GameObject abilityPrefab;

    public string Name { get { return artifactName; } }
    public string Description { get { return artifactDescription; } }

    public GameObject AbilityPrefab { get {  return abilityPrefab; } }
}
