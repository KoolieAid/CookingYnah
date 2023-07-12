using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactUIHandler : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] private GameObject artifactMenu;
    [SerializeField] private GameObject closeMenuTrigger;

    [Header("ArtifactAbility")]
    [SerializeField] private ArtifactProgress progress;
    [SerializeField] private SerializedDictionary<Artifacts, GameObject> artifactIcons = new SerializedDictionary<Artifacts, GameObject>();

    [Header("Tool Tip")]
    [SerializeField] private GameObject artifactToolTip;
    [SerializeField] private TextMeshProUGUI artifactName;
    [SerializeField] private TextMeshProUGUI artifactDescription;

    public static event Action<int, Artifacts, GameObject> OnArtifactSelected;
    private int selectedSlot;

    private void Start()
    {
        artifactMenu.SetActive(false);
        artifactToolTip.SetActive(false);
        closeMenuTrigger.SetActive(false);
    }
    private void Update()
    {
        if (artifactToolTip.activeInHierarchy)
        {
            artifactToolTip.transform.position = Input.mousePosition;
        }
    }

    public void ShowArtifactMenu(GameObject slot)
    {
        artifactMenu.SetActive(true);
        artifactMenu.transform.position = slot.transform.position;
        closeMenuTrigger.SetActive(true);
        closeMenuTrigger.transform.position = slot.transform.position;

        SerializedDictionary<Artifacts, bool> pair = progress.UnlockedArtifacts;

        // Gray out artifacts that you don't have yet
        // to be replaced with switching icon etc.
        foreach (var kvp in artifactIcons)
        {
            Color currentColor = kvp.Value.GetComponent<Image>().color;
            if (pair[kvp.Key])
            {
                Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, 1);
                kvp.Value.GetComponent<Image>().color = newColor;
            }
            else
            {
                Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0.5f);
                kvp.Value.GetComponent<Image>().color = newColor;
            }
        }
    }
    public void HideArtifactMenu()
    {
        artifactMenu.SetActive(false);
    }
    public void ShowArtifactToolTip(string artifactName)
    {
        artifactToolTip.SetActive(true);
        SetToolTipTexts(artifactName);
    }
    public void HideArtifactToolTip()
    {
        artifactToolTip.SetActive(false);
    }

    public void OnArtifactClick(string artifactSelected)
    {
        Artifacts a = FindArtifactByName(artifactSelected);
        
        if (progress.UnlockedArtifacts[a])
        {
            OnArtifactSelected?.Invoke(selectedSlot, a, progress.ArtifactDescriptions[a].AbilityPrefab);
            return;
        }
    }

    public void SetSelectedSlot(int slotNum)
    {
        selectedSlot = slotNum;
    }

    private Artifacts FindArtifactByName(string name)
    {
        KeyValuePair<Artifacts, ArtifactData> result = progress.ArtifactDescriptions.FirstOrDefault(kvp => kvp.Value.name == name);
        return result.Key;
    }

    private void SetToolTipTexts(string artifactHighlighted)
    {
        SerializedDictionary<Artifacts, ArtifactData> pair = progress.ArtifactDescriptions;

        foreach (var kvp in pair)
        {
            if (kvp.Value.Name == artifactHighlighted)
            {
                SerializedDictionary<Artifacts, bool> keyValuePair = progress.UnlockedArtifacts;
                if (!keyValuePair[kvp.Key])
                {
                    artifactName.text = "????";
                    artifactDescription.text = "??????????";
                    return;
                }
                artifactName.text = kvp.Value.Name;
                artifactDescription.text = kvp.Value.Description;
            }
        }
    }
}
