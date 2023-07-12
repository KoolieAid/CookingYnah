using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental;
using UnityEngine;
public enum Artifacts
{
    None,
    Fire_Meatball,
    Ground_Wave,
    Spectral_Sword,
}
public class ArtifactHandler : MonoBehaviour
{
    private SerializedDictionary<int, Artifacts> artifacts = new SerializedDictionary<int, Artifacts>()
    {
        {1, Artifacts.None},
        {2, Artifacts.None}
    };

    [SerializeField] private GameObject playerGameObject;

    private void OnEnable()
    {
        ArtifactUIHandler.OnArtifactSelected += SetArtifact;
    }

    private void OnDisable()
    {
        ArtifactUIHandler.OnArtifactSelected -= SetArtifact;
    }

    public void SetArtifact(int slotNum, Artifacts artifactToSlot, GameObject obj)
    {
        if (CanSlotArtifact(artifactToSlot))
        {
            // Remove ArtifactAbility script to be replaced
            ArtifactAbility artifactScript;
            switch (artifacts[slotNum])
            {
                case Artifacts.Fire_Meatball:
                    artifactScript = playerGameObject.GetComponent<FireMeatballAbility>();
                    artifactScript.enabled = false;
                    Destroy(artifactScript);
                    break;
                case Artifacts.Ground_Wave:
                    artifactScript = playerGameObject.GetComponent<GroundWaveAbility>();
                    artifactScript.enabled = false;
                    Destroy(artifactScript);
                    break;
                case Artifacts.Spectral_Sword:
                    artifactScript = playerGameObject.GetComponent<FireMeatballAbility>();
                    artifactScript.enabled = false;
                    Destroy(artifactScript);
                    break;
                case Artifacts.None:
                    break;
            }

            artifacts[slotNum] = artifactToSlot;

            // Add ArtifactAbility script
            var a = artifactToSlot;
            switch (a)
            {
                case Artifacts.Fire_Meatball:
                    playerGameObject.AddComponent<FireMeatballAbility>();
                    playerGameObject.GetComponent<FireMeatballAbility>().SetAbilityData(obj, slotNum);
                    break;
                case Artifacts.Ground_Wave:
                    playerGameObject.AddComponent<GroundWaveAbility>();
                    playerGameObject.GetComponent<GroundWaveAbility>().SetAbilityData(obj, slotNum);
                    break;
                case Artifacts.Spectral_Sword:
                    playerGameObject.AddComponent<FireMeatballAbility>();
                    playerGameObject.GetComponent<FireMeatballAbility>().SetAbilityData(obj, slotNum);
                    break;
            }
        }
    }

    private bool CanSlotArtifact(Artifacts artifactToSlot)
    {
        // Check if the artifact is already slotted
        if (artifacts.ContainsValue(artifactToSlot))
            return false;

        return true;
    }
}
