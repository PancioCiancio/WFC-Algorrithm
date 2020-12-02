using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCAlgorithm
{
    // This dictionary store all prototypes passed in,
    // this avoid repetition of instances.

    Dictionary<int, Prototype> m_Prototypes = null;
    
    int m_Width = 0;
    int m_Height = 0;
    
    // This algorithm work in 2D only for now.
    // I choose to use a 2D array to keep track
    // of still available tiles. Tiles are stored
    // in this matrix with its own ID.

    int[,] m_WaveFunctionCollapseGrid = null;
    bool m_IsFullyCollapsed = false;
    int m_LimitIteration = 10000;
    int m_IterationCount = 0;

    // Determines how we move onto the abstract grid.
    // It's defined in the constructor. It's value are fixed.

    int[] m_MovementDirections;
    
    // Used to get the socket side.

    int[] m_SocketDirections          = new int[4] { 0, 1, 2, 3 };
    int[] m_InverseSocketDirections   = new int[4] { 2, 3, 0, 1 };

    // Weight are calculated for each cell / tile.
    // Those values are related to the configuration models.

    float[] m_Weights;
    float[] m_LogWeights;
    float[] m_Entropy;

    float m_StartingWeight;
    float m_StartingLogWeight;
    float m_StartingEntropy;

    int m_LogBase = 0;
    System.Random m_Random;


    /// <summary>
    /// Constructor. Initialize all values.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="prototypes"></param>
    public WFCAlgorithm(int width, int height, params Prototype[] prototypes)
    {
        m_Width = width;
        m_Height = height;

        m_Random = new System.Random();

        m_Prototypes = new Dictionary<int, Prototype>();

        // Fill the Prototypes dictionary.
        foreach (Prototype proto in prototypes)
            m_Prototypes.Add(proto.ID, proto);

        // Define directions
        m_MovementDirections = new int[4]
        {
            + m_Width,              // Move top
            + 1,                    // Move right
            - m_Width,              // Move bottom
            - 1                     // Move left
        };

        foreach (Prototype proto in m_Prototypes.Values)
        {
            m_LogBase = m_LogBase < proto.Weight ? m_LogBase = Mathf.RoundToInt(proto.Weight + 0.4999f) : m_LogBase;
        }

        // Define Weights
        foreach (Prototype proto in m_Prototypes.Values)
        {
            m_StartingWeight    += proto.Weight;
            m_StartingLogWeight += proto.Weight * Mathf.Log(proto.Weight, m_LogBase);
        }
        m_StartingEntropy += Mathf.Log(m_StartingWeight, m_LogBase) - (m_StartingLogWeight / m_StartingWeight);    // Shannon Entropy

        m_Weights       = new float[m_Width * m_Height];
        m_LogWeights    = new float[m_Width * m_Height];
        m_Entropy       = new float[m_Width * m_Height];

        m_WaveFunctionCollapseGrid = new int[m_Width * m_Height, m_Prototypes.Count];
    }

    /// <summary>
    /// reset all weights, WFC grid, checkers.
    /// </summary>
    public void ResetValues()
    {
        m_IsFullyCollapsed = false;

        m_IterationCount = 0;

        // Reset all weights to starting weights.
        // Reset also the wave function collapse grid.
        int weightsActualIndex = 0;
        
        for (int i = 0; i < m_Width; i++)
        {
            for (int j = 0; j < m_Height; j++)
            {
                weightsActualIndex = i + j * m_Width;

                m_Weights   [weightsActualIndex] = m_StartingWeight;
                m_LogWeights[weightsActualIndex] = m_StartingLogWeight;
                m_Entropy   [weightsActualIndex] = m_StartingEntropy;

                int wfcIndex = 0;

                foreach (int id in m_Prototypes.Keys)
                {
                    m_WaveFunctionCollapseGrid[i + j * m_Width, wfcIndex] = id;
                    wfcIndex++;
                }
            }
        }
    }

    /// <summary>
    /// Start the algorithm.
    /// Note: before call this function, ResetValues() must be called.
    /// </summary>
    public bool StartAlgorithm()
    {
        while (m_IterationCount <= m_LimitIteration && !IsFullyCollapsed())
        {
            Iterate();
            m_IterationCount++;
        }

        if (m_IsFullyCollapsed) 
            Debug.Log("[WFC Algorithm] Log: Algorithm end successfully");
        else
            Debug.Log("[WFC Algorithm] Error:");

        return m_IsFullyCollapsed;
    }

    public Prototype[] GetOutputData()
    {
        Prototype[] output = new Prototype[m_Width * m_Height];

        for (int i = 0; i < m_Width; i++)
        {
            for (int j = 0; j < m_Height; j++)
            {
                for (int k = 0; k < m_Prototypes.Count; k++)
                {
                    if (m_WaveFunctionCollapseGrid[i + j * m_Width, k] != -1)
                    {
                        output[i + j * m_Width] = m_Prototypes[m_WaveFunctionCollapseGrid[i + j * m_Width, k]];
                    }
                }
            }
        }

        return output;
    }

    private bool IsFullyCollapsed()
    {
        m_IsFullyCollapsed = true;

        for (int i = 0; i < m_Entropy.Length; i++)
        {
            if (m_Entropy[i] != 0)
                m_IsFullyCollapsed = false;
        }

        return m_IsFullyCollapsed;
    }

    private void Iterate()
    {
        int lowestTileEntropy = GetLowestEntropy();
        Collapse(lowestTileEntropy);
        Propagate(lowestTileEntropy);
    }

    /// <summary>
    /// Get the cell / tile with the lowest protos available.
    /// This means it's the cell with the loweat entropy.
    /// </summary>
    /// <returns></returns>
    private int GetLowestEntropy()
    {
        int lowestEntropyIndex = -1;
        double tempEntropy          = float.MaxValue;
        double minumumEntropy       = float.MaxValue;

        for (int i = 0; i < m_Width; i++)
        {
            for (int j = 0; j < m_Height; j++)
            {
                if (m_Entropy[i + j * m_Width] == 0) continue;  // Skip this loop (go to the next iteration).

                tempEntropy = m_Entropy[i + j * m_Width];
                tempEntropy += 1E-6 * m_Random.NextDouble();    // Add some randomness to prevent == case.

                if (tempEntropy < minumumEntropy)
                {
                    minumumEntropy = tempEntropy;
                    lowestEntropyIndex = i + j * m_Width;
                }
            }
        }

        // Check if we encounter in contradiction.
        if (lowestEntropyIndex == -1)
            throw new Exception("[WFC algorithm] Error: contradiction in GetLowestEntropy()!");

        return lowestEntropyIndex;
    }

    /// <summary>
    /// In case we want to collapse in a specific coordinates a specific 
    /// Prototype, we can, Just pass its ID in the function.
    /// </summary>
    /// <param name="lowestCellEntropy"></param>
    /// <param name="choosenProtoID"></param>
    private void Collapse(int lowestCellEntropy, int choosenProtoID = -1)
    {
        // If the ID == -1 means that no "specific proto" are passed,
        // then choose one at random.
        if (choosenProtoID == -1)
        {
            float randomNumber = UnityEngine.Random.Range(0, m_Weights[lowestCellEntropy]);

            for (int i = 0; i < m_Prototypes.Count; i++)
            {
                int id = m_WaveFunctionCollapseGrid[lowestCellEntropy, i];

                if (id != -1)   // is valid?
                {
                    if (randomNumber < m_Prototypes[id].Weight)
                    {
                        choosenProtoID = id;
                        break;
                    }

                    randomNumber -= m_Prototypes[id].Weight;
                }
            }
        }

        // Remove from the lowest entropy cell / element all proto except
        // the one that has been choosen.
        for (int i = 0; i < m_Prototypes.Count; i++)
        {
            int iteratedProtoID = m_WaveFunctionCollapseGrid[lowestCellEntropy, i];

            if (iteratedProtoID != choosenProtoID &&
                iteratedProtoID != -1)
            {
                m_Weights   [lowestCellEntropy] -= m_Prototypes[iteratedProtoID].Weight;
                m_LogWeights[lowestCellEntropy] -= m_Prototypes[iteratedProtoID].Weight * Mathf.Log(m_Prototypes[iteratedProtoID].Weight, m_LogBase);
                m_Entropy   [lowestCellEntropy]  = Mathf.Log(m_Weights[lowestCellEntropy], m_LogBase) - (m_LogWeights[lowestCellEntropy] / m_Weights[lowestCellEntropy]); // Shannon Entropy

                m_WaveFunctionCollapseGrid[lowestCellEntropy, i] = -1;
            }
        }
    }

    /// <summary>
    /// The consequence of the Collapse are propagated.
    /// Search the neighbour cells and check its compatibilities.
    /// This operation is performed until there aren't anymore 
    /// consequences.
    /// </summary>
    /// <param name="lowestCellEntropy"></param>
    private void Propagate(int lowestCellEntropy)
    {
        Stack<int> stackOfIndex = new Stack<int>();
        stackOfIndex.Push(lowestCellEntropy);

        while(stackOfIndex.Count > 0)
        {
            int currentIndex = stackOfIndex.Pop();

            foreach (int dir in GetDirectionsInBounds(currentIndex))
            {
                int otherIndex = currentIndex + m_MovementDirections[dir];

                List<int> otherPossibleProtos = GetAvailableProtosAtGridPos(otherIndex);

                if (otherPossibleProtos.Count == 0) continue;

                string currentSocket = GetSocket(currentIndex, m_SocketDirections[dir]);

                foreach (int otherID in otherPossibleProtos)
                {
                    string otherSocket = m_Prototypes[otherID].Sockets[m_InverseSocketDirections[dir]];

                    if (string.Compare(currentSocket, otherSocket) != 0)
                    {
                        Constrain(otherIndex, otherID);

                        if (!stackOfIndex.Contains(otherIndex))
                            stackOfIndex.Push(otherIndex);
                    }
                }
            }
        }
    }

    private void Constrain(int otherindexing, int id)
    {
        for (int i = 0; i < m_Prototypes.Count; i++)
        {
            int iteratedProtoID = m_WaveFunctionCollapseGrid[otherindexing, i];

            if (iteratedProtoID == id)
            {
                m_Weights   [otherindexing] -= m_Prototypes[id].Weight;
                m_LogWeights[otherindexing] -= m_Prototypes[id].Weight * Mathf.Log(m_Prototypes[id].Weight, m_LogBase);
                m_Entropy   [otherindexing]  = Mathf.Log(m_Weights[otherindexing], m_LogBase) - (m_LogWeights[otherindexing] / m_Weights[otherindexing]); // Shannon Entropy

                m_WaveFunctionCollapseGrid[otherindexing, i] = -1;
                break;
            }
        }
    }

    /// <summary>
    /// Return the directions in the grid's bounds.
    /// </summary>
    /// <param name="indexing"></param>
    /// <returns></returns>
    private List<int> GetDirectionsInBounds(int indexing)
    {
        List<int> validDirections = new List<int>();

        int x = indexing % m_Width;
        int y = indexing / m_Width;

        //top side
        if (y + 1 < m_Height)
            validDirections.Add(0);
        //right side
        if (x + 1 < m_Width)
            validDirections.Add(1);
        //down side
        if (y - 1 >= 0)
            validDirections.Add(2);
        //left side
        if (x - 1 >= 0)
            validDirections.Add(3);

        return validDirections;
    }

    /// <summary>
    /// Get the remaining protos in a cell / tile.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private List<int> GetAvailableProtosAtGridPos(int index)
    {
        List<int> availableProtos = new List<int>();

        for (int i = 0; i < m_Prototypes.Count; i++)
        {
            if (m_WaveFunctionCollapseGrid[index, i] != -1)
                availableProtos.Add(m_WaveFunctionCollapseGrid[index, i]);
        }

        return availableProtos;
    }

    /// <summary>
    /// Find the unique available proto and return the socket at direction.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="actualDirection"></param>
    /// <returns></returns>
    private string GetSocket(int index, int actualDirection)
    {
        string socket = "";

        for (int i = 0; i < m_Prototypes.Count; i++)
        {
            int id = m_WaveFunctionCollapseGrid[index, i];
            if (id != -1)
            {
                socket = m_Prototypes[id].Sockets[actualDirection];
                break;
            }
        }

        return socket;
    }
}
