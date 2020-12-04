using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WFCUIRenderer : WFCRenderOutput
{
    [SerializeField]
    Image[] RenderedGrid;

    static List<Prototype> s_Prototype = new List<Prototype>();

    public void StartSorting()
    {
        SetPrototypesCollection(s_Prototype.ToArray());

        base.WFCStart();

        if (m_OutputPrototypes == null) return;

        DOTween.Clear();

        for (int i = 0; i < m_OutputPrototypes.Length; i++)
        {
            RenderedGrid[i].sprite = Resources.Load<Sprite>(m_OutputPrototypes[i].RenderePath);
            RenderedGrid[i].useSpriteMesh = true;
            RenderedGrid[i].color = new Color(1, 1, 1, 0);
        }

        int j = 0;
        RenderedGrid[m_WFCAlgorithm.CollapseSequenceIndex[j++]].DOFade(1.0f, 0.12f).OnComplete(() => CheckTeweening(j));
    }

    void CheckTeweening(int index)
    {
        if (index < m_WFCAlgorithm.CollapseSequenceIndex.Count)
        {
            RenderedGrid[m_WFCAlgorithm.CollapseSequenceIndex[index++]].DOFade(1.0f, 0.12f).OnComplete(() => CheckTeweening(index));
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
