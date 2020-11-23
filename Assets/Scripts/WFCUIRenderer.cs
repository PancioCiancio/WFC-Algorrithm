using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WFCUIRenderer : WFCRenderOutput
{
    [SerializeField]
    Image[] RenderedGrid;

    static List<Prototype> s_Prototype = new List<Prototype>();

    public void StartSorting()
    {
        SetPrototypesCollection(s_Prototype.ToArray());

        base.WFCStart();

        for (int i = 0; i < m_OutputPrototypes.Length; i++)
        {
            RenderedGrid[i].sprite = Resources.Load<Sprite>(m_OutputPrototypes[i].RenderePath);
            RenderedGrid[i].useSpriteMesh = true;
        }
    }

    public static void AddPrototype(Prototype proto)
    {
        s_Prototype.Add(proto);
    }

    public static void RemovePrototype(Prototype proto)
    {
        s_Prototype.Remove(proto);
    }
}
