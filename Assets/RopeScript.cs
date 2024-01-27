using System;    
using UnityEngine;

public class RopeScript : MonoBehaviour
{
    public MeshFilter meshFilter;

    public int subSteps = 10;
    public int iterations = 3;

    public float edgeCompliance = 0.5f;
    public float mass = 1f;


    public Vector3[] nodes;
    public int[] edgeNodeIds;

    [SerializeField]
    private float[] _restEdgeLengths;    
    [SerializeField]
    private float[] _inverseMasses;
    [SerializeField]
    private Vector3[] _velocities;
    [SerializeField]
    private Vector3[] _prevPositions;

    private Vector3 _gravity;

    private float[] _lagrange;

    private bool _isInitialized = false;

    public int NodeCount
    {
        get
        {
            return nodes.Length;
        }
    }

    public int EdgeCount
    {
        get
        {
            return edgeNodeIds.Length / 2;
        }
    }

    private void Awake()
    {
        //Create the Rope Nodes/Vertices.
        nodes = new Vector3[]
        {
            new Vector3(0f,2f,0f),
            new Vector3(0f,1.75f,0f),
            new Vector3(0f,1.5f,0f),
            new Vector3(0f,1.25f,0f),
            new Vector3(0f,1f,0f),
            new Vector3(0f,0.75f,0f),
            new Vector3(0f,0.5f,0f)
        };

        edgeNodeIds = new int[]
        {
            0,1,
            1,2,
            2,3,
            3,4,
            4,5,
            5,6
        };

        var mesh = new Mesh();
        mesh.vertices = nodes;
        mesh.SetIndices(edgeNodeIds, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;

        //Get Local Vector of Gravity.
        _gravity = transform.InverseTransformDirection(Physics.gravity);

        InitializeSimulation();
    }


    private void FixedUpdate()
    {
        if (!_isInitialized)
            return;

        //Get the Substep.
        var sdt = Time.fixedDeltaTime / subSteps;

        for (int i = 0; i < subSteps; i++)
        {

            PreSolve(sdt, _gravity);

            //Clear the Values of the Lagrange Multipliers.
            Array.Clear(_lagrange, 0, _lagrange.Length);
            for (int n = 0; n < iterations; n++)
            {
                Solve(sdt);
            }
        
            PostSolve(sdt);
        }
    }


    private void InitializeSimulation()
    {
        //Initialize the Arrays.
        _restEdgeLengths = new float[EdgeCount];        
        _inverseMasses = new float[NodeCount];
        _velocities = new Vector3[NodeCount];
        _prevPositions = new Vector3[NodeCount];
        _lagrange = new float[EdgeCount];
    

        //Get all of the Rest Lengths for each Edge.
        for (int i = 0; i < EdgeCount; i++)
        {
            //_restEdgeLengths[i] = tetMesh.GetSqrEdgeLength(i);
            _restEdgeLengths[i] = GetMagEdgeLength(i);
        }

        var w = 1f / (mass / NodeCount);

        //Get all of the Inverse Masses.
        for (int i = 0; i < NodeCount; i++)
        {
            _inverseMasses[i] = w;            
        }

        _inverseMasses[0] = 0f;
        //_inverseMasses[6] = 0f;

        _isInitialized = true;
    }


    private void PreSolve(float dTime, Vector3 gravity)
    {
        //For each node.
        for (int i = 0; i < NodeCount; i++)
        {
            var w = _inverseMasses[i];

            //Skip if the Inverse Mass is Zero/Infinite.
            if (w == 0f)
                continue;

            //Get selected Velocity and add the Gravity vector.
            _velocities[i] += (gravity * dTime) / w;

            //Cache Previous Position.
            _prevPositions[i] = nodes[i];

            //Add Velocity vector to the Nodes Position vector.
            nodes[i] += _velocities[i] * dTime;
        }

    }

    private void Solve(float dTime)
    {
        SolveEdges(dTime, edgeCompliance);                
    }

    private void PostSolve(float dTime)
    {
        for (int i = 0; i < NodeCount; i++)
        {
            //Skip if the Inverse Mass is Zero/Infinite.
            if (_inverseMasses[i] == 0f)
                continue;

            //Update the selected Velocity.
            _velocities[i] = (nodes[i] - _prevPositions[i]) / dTime;
        }

        //Update the Mesh.
        UpdateMesh();
    }

    private void SolveEdges(float dTime, float compliance)
    {
        //Calculate the alpha for stiffness.
        var alpha = compliance / (dTime * dTime);

        //For each Edge.
        for (int i = 0; i < EdgeCount; i++)
        {
            //Get the Node Id's for the selected Edge.
            var nodeIds = GetEdgeNodeIds(i);            

            //Get the Inverse Masses for each Node.
            var w0 = _inverseMasses[nodeIds[0]];
            var w1 = _inverseMasses[nodeIds[1]];

            //Sum the Inverse Masses.
            var w = w0 + w1;

            //If they are Infinite, Skip.
            if (w == 0f)
                continue;

            //Get the selected Edge Nodes/Vertices.
            var eNodes = GetEdgeNodes(i);

            //Get the current length of the edge.
            var len = GetMagEdgeLength(i);

            //Check if current length is Zero.
            if (len == 0f)
                continue;

            //Get the Constraint Error between the Current Length and the Rest length.
            var c = len - _restEdgeLengths[i];

            //Get the Direction of the Edge.
            var n = Vector3.Normalize((eNodes[0] - eNodes[1]));                        

            //Calculate the Delta Lagrange Multiplier.
            var dLag = -(c + alpha * _lagrange[i]) / (w + alpha);

            //This version also worked and I liked its result better.
            //But it doesnt match the source documentation.
            //var dLagrange = -(c - alpha * _lagrange[i]) / (w + alpha);

            //Add to the delta Lagrange Multiplier to Lagrange Multiplier.
            _lagrange[i] += dLag;

            //Cacluate the Delta Positions for each point.
            var p0 = (dLag * n);
            var p1 = (dLag * -n);

            //Add Delta Positions to Update the Positions.
            nodes[nodeIds[0]] += p0 * w0;
            nodes[nodeIds[1]] += p1 * w1;            
        }
    }

    /// <summary>
    /// Applies the changes performed by the simulation to the mesh.
    /// </summary>
    private void UpdateMesh()
    {
        //Update the Mesh Object Vertices.
        meshFilter.mesh.vertices = nodes;
    }

    /// <summary>
    /// Calculates the length of a selected edge.
    /// </summary>
    /// <param name="index">Id of the selected edge.</param>
    /// <returns>The length of the selected edge. Also known as its magnitude.</returns>
    public float GetMagEdgeLength(int index)
    {
        //Get the Nodes of the selected Edge.
        var node1 = nodes[edgeNodeIds[2 * index]];        
        var node2 = nodes[edgeNodeIds[2 * index + 1]];

        //Return the Manitude of the 2 Points.
        return Vector3.Magnitude(node2 - node1);
    }

    /// <summary>
    /// Gets the nodes/vertices of the selected edge.
    /// </summary>
    /// <param name="index">Id of the selected edge.</param>
    /// <returns>The nodes/vertices of the selected edge.</returns>
    public Vector3[] GetEdgeNodes(int index)
    {
        return new Vector3[2]
        {
            nodes[edgeNodeIds[2 * index]],
            nodes[edgeNodeIds[2 * index + 1]],
        };
    }

    /// <summary>
    /// Gets the Id's of the Nodes/Vertices of the selected edge.
    /// </summary>
    /// <param name="index">Id of the selected edge.</param>
    /// <returns>The Id's of the Nodes/Vertices of the selected edge</returns>
    public int[] GetEdgeNodeIds(int index)
    {
        return new int[2]
        {
            edgeNodeIds[2 * index],
            edgeNodeIds[2 * index + 1]
        };
    }
}