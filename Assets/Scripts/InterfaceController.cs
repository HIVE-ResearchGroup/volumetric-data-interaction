using System.Collections.Generic;
using Constants;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu class
/// Toggle between menu and detail view
/// </summary>
public class InterfaceController : MonoBehaviour
{
    public const int AdditionCount = 5;
    
    [SerializeField]
    private TextMeshProUGUI hud;

    [SerializeField]
    private Transform main;
    
    [SerializeField]
    private Text centerText;

    [SerializeField]
    private Material uiMain;
    
    [SerializeField]
    private Material uiExploration;
    
    [SerializeField]
    private Material uiSelection;
    
    [SerializeField]
    private Material uiSelected;

    private MeshRenderer _mainMeshRenderer;
    
    public Transform Main => main;

    public List<Transform> Additions { get; } = new(AdditionCount);

    private void Awake()
    {
        _mainMeshRenderer = Main.GetComponent<MeshRenderer>();
        var parent = Main.parent;
        
        // the first one is main
        // get all additions and add them to the list
        for (var i = 0; i < AdditionCount; i++)
        {
            Additions.Add(parent.transform.GetChild(i + 1));
        }
    }

    private void OnEnable()
    {
        SetMode(MenuMode.None);
    }

    public void SetMode(MenuMode mode, bool isSnapshotSelected = false)
    {
        switch (mode)
        {
            case MenuMode.None:
                SetMaterial(uiMain);
                SetHUD(StringConstants.MainModeInfo);
                SetCenterText(StringConstants.MainModeInfo);
                break;
            case MenuMode.Analysis:
                SetMaterial(uiExploration);
                SetHUD(StringConstants.ExplorationModeInfo);
                SetCenterText(StringConstants.ExplorationModeInfo);
                break;
            case MenuMode.Selection:
                SetMaterial(uiSelection);
                SetHUD(StringConstants.SelectionModeInfo);
                SetCenterText(StringConstants.SelectionModeInfo);
                break;
            case MenuMode.Selected:
                if (!isSnapshotSelected)
                {
                    SetMaterial(uiSelected);
                }
                break;
            case MenuMode.Mapping:
                SetMaterial(uiSelected);
                break;
            default:
                SetMaterial(uiMain);
                break;
        }
    }

    private void SetCenterText(string text) => centerText.text = text;

    private void SetHUD(string text = "") => hud.text = text;

    public void SetMaterial([NotNull] Material mat) => _mainMeshRenderer.material = mat;
}
