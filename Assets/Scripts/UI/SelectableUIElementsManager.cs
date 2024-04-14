using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SelectableUIElementsManager : MonoBehaviour
{
    private void Start()
    {
        SetAllButtons();
        SetAllSliders();
        SetAllDroppdowns();
    }

    public static void ChangeCurrentSelectedGameObject(GameObject gameObject)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    void SetAllButtons()
    {
        foreach (Button b in FindObjectsOfType<Button>(true))
        {
            if (b.gameObject.GetComponent<SelectableUIElement>() != null)
            {
                continue;
            }

            b.gameObject.AddComponent<SelectableUIElement>();
        }
    }

    void SetAllSliders()
    {
        foreach (Slider s in FindObjectsOfType<Slider>(true))
        {
            if (s.gameObject.GetComponent<SelectableUIElement>() != null)
            {
                continue;
            }

            s.gameObject.AddComponent<SelectableUIElement>();
        }
    }

    void SetAllDroppdowns()
    {
        foreach (TMP_Dropdown d in FindObjectsOfType<TMP_Dropdown>(true))
        {
            if (d.gameObject.GetComponent<SelectableUIElement>() != null)
            {
                continue;
            }

            d.gameObject.AddComponent<SelectableUIElement>();
        }
    }
}
