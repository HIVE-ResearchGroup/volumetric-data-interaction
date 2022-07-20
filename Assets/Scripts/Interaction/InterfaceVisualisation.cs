using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu class
/// Toggle between menu and detail view
/// </summary>
public class InterfaceVisualisation : MonoBehaviour
{
    private UnityEngine.UI.Text hud;
    private Text leftText;
    private Text rightText;
    private Text centerText;

    private void Start()
    {
        hud = GameObject.Find(StringConstants.HudText).GetComponent<Text>(); ;
        leftText = GameObject.Find(StringConstants.LeftText).GetComponent<Text>(); ;
        rightText = GameObject.Find(StringConstants.RightText).GetComponent<Text>(); ;
        centerText = GameObject.Find(StringConstants.CenterText).GetComponent<Text>(); ;
    }

    public void SetLeftRightText(string leftText, string rightText)
    {
        this.leftText.text = leftText;
        this.rightText.text = rightText;
        centerText.text = "";
    }

    public void SetCenterText(string text)
    {
        centerText.text = text;
        this.leftText.text = "";
        this.rightText.text = "";
    }

    public void SetHUD(string text = "")
    {
        hud.text = text;
    }
}
