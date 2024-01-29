using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CostMapGenerator
{
    private int width;
    private int height;
    private float maxRenderHeight;
    private float noiseScale = 0.1f;

    private Node[,] nodeMap;
    private GameObject mapSource;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public CostMapGenerator(GameObject mapSource)
        : this(10, 10, 1, mapSource, false)
    {
    }

    public CostMapGenerator(int width, int height, GameObject mapSource)
        : this(width, height, 1, mapSource, false)
    {
}

    public CostMapGenerator(int width, int height, float maxRenderHeight, GameObject mapSource, bool noise)
    {
        this.maxRenderHeight = maxRenderHeight;
        this.mapSource = mapSource;
        meshRenderer = mapSource.GetComponent<MeshRenderer>();
        meshFilter = mapSource.GetComponent<MeshFilter>();
        nodeMap = new Node[width, height];
        generateMap(width, height, noise);
        renderMap();
    }

    public List<Node> getNodes()
    {
        List<Node> nodeList = new List<Node>();
        foreach(Node node in nodeMap)
        {
            nodeList.Add(node);
        }
        return nodeList;
    }

    public void setNoiseScale(float scale)
    {
        this.noiseScale = scale;
    }

    public void generateMap(int width, int height, bool noise)
    {
        this.width = width;
        this.height = height;
        mapSource.name = width + "x" + height + "map";

        nodeMap = new Node[width, height];
        Random.Range(0, 1);
        float offset = (Random.value * 20000) - 10000;

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x, y);
                float cost;
                if(!noise)
                {
                    cost = Random.Range(0f, 1f);
                } else {
                    cost = Mathf.PerlinNoise(x * noiseScale + offset, y * noiseScale + offset);
                }
                nodeMap[x, y] = new Node(position, cost);
            }
        }

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Node currentNode = nodeMap[x, y];

                // adjacent neighbors
                if (x > 0)
                    currentNode.addNeighbor(nodeMap[x - 1, y]);
                if (x < width - 1)
                    currentNode.addNeighbor(nodeMap[x + 1, y]);
                if (y > 0)
                    currentNode.addNeighbor(nodeMap[x, y - 1]);
                if (y < height - 1)
                    currentNode.addNeighbor(nodeMap[x, y + 1]);

                // diagonal neighbors
                if (x > 0 && y > 0)
                    currentNode.addNeighbor(nodeMap[x - 1, y - 1]); 
                if (x < width - 1 && y > 0)
                    currentNode.addNeighbor(nodeMap[x + 1, y - 1]);
                if (x > 0 && y < height - 1)
                    currentNode.addNeighbor(nodeMap[x - 1, y + 1]);
                if (x < width - 1 && y < height - 1)
                    currentNode.addNeighbor(nodeMap[x + 1, y + 1]);
            }
        }
    }

    //public IEnumerator renderMap()
    public void renderMap()
    {
        Mesh mesh = new Mesh();
        List<Node> nodes = getNodes();
        if (nodes.Count == 0)
            return;
        Vector3[] vertices = new Vector3[nodes.Count];

        for(int i = 0; i < nodes.Count; i++)
        {
            float height = nodes[i].getCost() * maxRenderHeight;
            vertices[i] = new Vector3(nodes[i].getPosition().x, height, nodes[i].getPosition().y);
        }

        int[] triangles = new int[(width - 1) * (height - 1) * 6];
        int ti = 0;
        int index = 0;
        for(int y = 0; y < height - 1; y++)
        {
            for(int x = 0; x < width - 1; x++)
            {
                index = x * height + y;
                try
                {
                    triangles[ti++] = index + 1;
                } catch {
                    Debug.LogError("index of " + index + " out of bounds, with x: " + x + ", y: " + y);
                    break;
                }
                triangles[ti++] = index + 1 + height;
                triangles[ti++] = index;

                triangles[ti++] = index + 1 + height;
                triangles[ti++] = index + height;
                triangles[ti++] = index;
                // yield return new WaitForSeconds(0);
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        // meshRenderer.material = new Material(Shader.Find("TerrainShader"));

        meshFilter.mesh = mesh;
    }
}
