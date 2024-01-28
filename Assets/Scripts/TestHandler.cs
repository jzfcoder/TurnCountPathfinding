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

    [Range(0, 1)]
    public float noiseScale = 0.1f;

    public GameObject mapSource;

    private int prevWidth;
    private int prevHeight;
    private float prevNoiseScale;

    private CostMapGenerator mapGenerator;
    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = new CostMapGenerator(width, height, terrainHeight, mapSource, noise);
        mapGenerator.setNoiseScale(noiseScale);
    }

    // Update is called once per frame
    void Update()
    {
        if(mapGenerator != null)
        {
            if(prevHeight != height || prevWidth != width || prevNoiseScale != noiseScale)
            {
                mapGenerator.setNoiseScale(noiseScale);
                mapGenerator.generateMap(width, height, noise);
                mapGenerator.renderMap();
            }

            prevHeight = height;
            prevWidth = width;
            prevNoiseScale = noiseScale;
        }
    }

    private void OnDrawGizmos()
    {
        if(mapGenerator == null)
        {
            return;
        }
        Gizmos.color = Color.yellow;
        foreach(Node n in mapGenerator.getNodes())
        {
            Gizmos.DrawSphere(new Vector3(n.getPosition().x, n.getCost(), n.getPosition().y), 0.1f);
        }
    }
}
