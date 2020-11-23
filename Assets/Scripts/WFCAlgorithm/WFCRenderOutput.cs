using System.IO;
using UnityEngine;

/// <summary>
/// Create a class that derive from this one and implement how the data is rendered.
/// </summary>
public abstract class WFCRenderOutput : MonoBehaviour
{
    [SerializeField]
    string filePath = "/TextAssets/Prototypes.json";
    PrototypeCollection PrototypesCollection;

    protected WFCAlgorithm m_WFCAlgorithm;
    protected Prototype[] m_OutputPrototypes;

    [SerializeField, Tooltip("")]
    protected int m_Width = 5;
    [SerializeField, Tooltip("")]
    protected int m_Height = 5;

    protected int Row(int index)      { return index / m_Width; }
    /// <summary>
    /// Represent the X axis.
    /// </summary>
    protected int Column(int index)   { return index % m_Width; }

    virtual protected void WFCStart()
    {
        m_WFCAlgorithm = new WFCAlgorithm(m_Width, m_Height, PrototypesCollection.Prototypes);
        m_WFCAlgorithm.ResetValues();
        bool success = m_WFCAlgorithm.StartAlgorithm();

        if (success)
            m_OutputPrototypes = m_WFCAlgorithm.GetOutputData();
    }

    protected PrototypeCollection GetPrototypesFromJson(string filePath)
    {
        string output = File.ReadAllText($"{Application.dataPath}{filePath}");
        PrototypeCollection collection = JsonUtility.FromJson<PrototypeCollection>(output);
        return collection;
    }

    /// <summary>
    /// Allow to set manually what prototype use.
    /// This means the data must be defined previously.
    /// </summary>
    protected void SetPrototypesCollection(params Prototype[] prototypes)
    {
        PrototypesCollection.Prototypes = new Prototype[prototypes.Length];

        for (int i = 0; i < prototypes.Length; i++)
        {
            PrototypesCollection.Prototypes[i] = prototypes[i];
        }

    }
}
