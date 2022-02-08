using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Menu class
/// Toggle between menu and detail view
/// Allow button interaction
/// </summary>
public class Menu : MonoBehaviour
{

    void Start()
    {
        
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMenu();
        }
    }

    /// <summary>
    /// Display menu on plane
    /// </summary>
    public void ToggleMenu()
    {

    }
}
