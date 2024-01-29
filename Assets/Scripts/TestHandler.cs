using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHandler : MonoBehaviour
{
    [Range(1, 100)]
    public int width = 100;

    [Range(1, 100)]
    public int height = 100;

    [Range(1, 50)]
    public int terrainHeight = 10;

    public Boolean noise = false;

    [Range(0, 10)]
    public int noiseScale = 2;

    public Boolean regenerate = false;
    public Boolean calculate = false;

    public GameObject mapSource;

    private int prevWidth;
    private int prevHeight;
    private float prevNoiseScale;
    private List<Node> path;
    private AStarAlgo aStarAlgo;

    private CostMapGenerator mapGenerator;
    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = new CostMapGenerator(width, height, terrainHeight, mapSource, noise);
        mapGenerator.setNoiseScale(noiseScale / 20f);
        aStarAlgo = new AStarAlgo();
    }

    // Update is called once per frame
    void Update()
    {
        if(mapGenerator != null)
        {
            if(prevHeight != height || prevWidth != width || prevNoiseScale != noiseScale || regenerate)
            {
                mapGenerator.setNoiseScale(noiseScale / 20f);
                mapGenerator.generateMap(width, height, noise);
                mapGenerator.renderMap();
                regenerate = false;
            }

            if(calculate)
            {
                path = aStarAlgo.solve(mapGenerator.getNodes()[0], mapGenerator.getNodes()[mapGenerator.getNodes().Count - 1]);
                Debug.Log(path);
                calculate = false;
            }

            prevHeight = height;
            prevWidth = width;
            prevNoiseScale = noiseScale;
        }
    }

    public int selectedX = 0;
    public int selectedY = 0;
    private void OnDrawGizmos()
    {
        if(mapGenerator == null)
        {
            return;
        }
        float x;
        float y;
        foreach(Node n in mapGenerator.getNodes())
        {
            x = n.getPosition().x;
            y = n.getPosition().y;

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(new Vector3(x, n.getCost(), y), 0.1f);

            // draw neighbors of selected node
            if(x == selectedX && y == selectedY)
            {
                foreach(Node neighbor in n.getNeighbors())
                {

                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(
                        new Vector3(x, n.getCost(), y),
                        new Vector3(
                            neighbor.getPosition().x,
                            neighbor.getCost(),
                            neighbor.getPosition().y
                    ));
                }
            }
        }
        renderPath();
    }

    private void renderPath()
    {
        if(path != null)
        {
            Debug.Log("rendering..." + path.Count);
            for(int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(
                    new Vector3(path[i].getPosition().x, path[i].getCost(), path[i].getPosition().y),
                    new Vector3(path[i + 1].getPosition().x, path[i + 1].getCost(), path[i + 1].getPosition().y)
                    );
            }
        }
    }
}
