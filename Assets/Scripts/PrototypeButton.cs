using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrototypeButton : MonoBehaviour
{
    [SerializeField]
    Prototype m_Data;

    bool Toggle = false;

    private void Start()
    {
        GetComponent<Button>().image.color = Toggle ? Color.white : Color.grey;
    }

    public void Click()
    {
        Toggle = !Toggle;

        GetComponent<Button>().image.color = Toggle ? Color.white : Color.grey;

        if (Toggle)
            WFCUIRenderer.AddPrototype(m_Data);
        else if (!Toggle)
            WFCUIRenderer.RemovePrototype(m_Data);
    }
}
