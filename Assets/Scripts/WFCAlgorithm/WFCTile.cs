using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Prototype
{
    public int ID;
    /// <summary>
    /// Use to weight the random, 
    /// usefull to collapse the wave to a single tile.
    /// </summary>
    public float Weight;
    /// <summary>
    /// This path will be past to search in the resources 
    /// folder.
    /// </summary>
    public string RenderePath;
    /// <summary>
    /// Each direction has its unic socket compatibility
    /// such us a color, or a given code.
    /// </summary>
    public string[] Sockets;

    public Prototype(int id = 0, float weight = .1f)
    {
        ID = id;
        Weight = weight;
        RenderePath = "";
        Sockets = new string[4] { "#FFFFFF", "#FFFFFF", "#FFFFFF", "#FFFFFF" };
    }
}

/// <summary>
/// Usefull to serialize it in a json file.
/// </summary>
[System.Serializable]
public struct PrototypeCollection
{
    public Prototype[] Prototypes;
}