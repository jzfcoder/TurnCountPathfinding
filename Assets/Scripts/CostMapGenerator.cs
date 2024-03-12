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
    private List<Node> nodeList;
    private GameObject mapSource;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private GameObject plane;

    public CostMapGenerator(GameObject mapSource)
        : this(10, 10, 1, mapSource, false, null)
    {
    }

    public CostMapGenerator(int width, int height, GameObject mapSource)
        : this(width, height, 1, mapSource, false, null)
    {
}

    public CostMapGenerator(int width, int height, float maxRenderHeight, GameObject mapSource, bool noise, GameObject plane)
    {
        this.maxRenderHeight = maxRenderHeight;
        this.mapSource = mapSource;
        this.plane = plane;
        meshRenderer = mapSource.GetComponent<MeshRenderer>();
        meshFilter = mapSource.GetComponent<MeshFilter>();
        nodeMap = new Node[100, 100];
        nodeList = new List<Node>();

        for(int x = 0; x < 100; x++)
        {
            for(int y = 0; y < 100; y++)
            {
                Vector2 position = new Vector2(x, y);
                nodeMap[x, y] = new Node(position, 0);

                GameObject cell = GameObject.Instantiate(plane, new Vector3(position.x, 0, position.y),
                    new Quaternion(0, 0, 0, 0), mapSource.transform);
                cell.SetActive(false);
            }
        }

        renderMap(false);
    }

    public Node getStart()
    {
        return nodeMap[0, 0];
    }

    public Node getEnd()
    {
        return nodeMap[width - 1, height - 1];
    }

    public List<Node> getNodes()
    {
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

        nodeList = null;
        nodeList = new List<Node>();
        Random.Range(0, 1);
        float offset = (Random.value * 20000) - 10000;

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                float cost;
                if(!noise)
                {
                    cost = Random.Range(0f, 1f);
                } else {
                    cost = Mathf.PerlinNoise(x * noiseScale + offset, y * noiseScale + offset);
                }
                nodeMap[x, y].setCost(cost);
                nodeList.Add(nodeMap[x, y]);
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

    public void deleteSelected(int x, int y)
    {
        Node n = nodeMap[x, y];
        List<Node> neighbors = n.getNeighbors();
        for (int i = 0; i < neighbors.Count; i++)
        {
            List<Node> neighborNeighbors = neighbors[i].getNeighbors();
            for (int j = 0; j < neighborNeighbors.Count; j++)
            {
                Node node = neighborNeighbors[j];
                if(node.getPosition().x == x && node.getPosition().y == y)
                {
                    neighbors[i].getNeighbors().Remove(node);
                }
            }
        }
        n = null;
        nodeMap[x, y] = null;
        renderMap();
    }

    public void renderMap()
    {
        renderMap(true);
    }
    public void renderMap(bool rTerrain)
    {
        if(rTerrain)
        {
            renderTerrain();
        }
        renderGrid();
    }

    private void renderGrid()
    {
        GameObject cell;
        MeshRenderer renderer;
        float cost;
        foreach (Transform child in mapSource.transform)
        {
            cell = child.gameObject;
            if(child.transform.position.x < width && child.transform.position.z < height)
            {
                renderer = cell.GetComponent<MeshRenderer>();
                cost = nodeMap[(int) child.transform.position.x, (int) child.transform.position.z].getCost();
                renderer.material.SetColor("_Color", new Color(1 - cost, 1 - cost, 1 - cost));
                cell.SetActive(true);
            } else {
                cell.SetActive(false);
            }
        }
    }

    //public IEnumerator renderMap()
    private void renderTerrain()
    {
        Mesh mesh = new Mesh();
        List<Node> nodes = getNodes();
        if (nodes.Count == 0)
            return;
        Vector3[] vertices = new Vector3[nodes.Count];

        for(int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] == null)
            {
                vertices[i] = new Vector3(0, 0, 0);
                continue;
            }
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
