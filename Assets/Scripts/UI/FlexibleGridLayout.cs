using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    [SerializeField]
    int m_Rows;
    [SerializeField]
    int m_Columns;
    [SerializeField]
    Vector2 m_CellSize;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        float sqrt  = Mathf.Sqrt(transform.childCount);
        m_Rows      = Mathf.CeilToInt(sqrt);
        m_Columns   = Mathf.CeilToInt(sqrt);

        float parentWidth   = rectTransform.rect.width;
        float parentHeight  = rectTransform.rect.height;

        float cellWidth     = parentWidth / (float)m_Columns;
        float cellHeight    = parentHeight / (float)m_Rows;

        m_CellSize.x = cellWidth;
        m_CellSize.y = cellHeight;

        int columnCount = 0;
        int rowCount    = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            rowCount    = i / m_Columns;
            columnCount = i % m_Columns;

            var item = rectChildren[i];

            var xPos = (m_CellSize.x * columnCount);
            var yPos = (m_CellSize.y * rowCount);

            SetChildAlongAxis(item, 0, xPos, m_CellSize.x);
            SetChildAlongAxis(item, 1, yPos, m_CellSize.y);
        }
    }

    public override void CalculateLayoutInputVertical()
    {
        

    }

    public override void SetLayoutHorizontal()
    {

    }

    public override void SetLayoutVertical()
    {
    }
}
