using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

	public GameObject mapSource;
	public GameObject tile;
	public LineRenderer lineRenderer;

	private int prevWidth;
	private int prevHeight;
	private float prevNoiseScale;
	private List<Node> path;
	private AStarAlgo aStarAlgo;

	private CostMapGenerator mapGenerator;
	// Start is called before the first frame update
	void Start()
	{
		mapGenerator = new CostMapGenerator(width, height, terrainHeight, mapSource, noise, tile);
		mapGenerator.setNoiseScale(noiseScale / 20f);
		aStarAlgo = new AStarAlgo();
	}

	// Update is called once per frame
	void Update()
	{
		if(mapGenerator != null)
		{
			if(prevHeight != height || prevWidth != width || prevNoiseScale != noiseScale)
			{
				regenerate();
			}

			prevHeight = height;
			prevWidth = width;
			prevNoiseScale = noiseScale;
		}
	}

	[HideInInspector]
	public float weightOfCost = 2f;
	public void calculate()
	{
		path = aStarAlgo.solve(mapGenerator.getNodes()[0], mapGenerator.getNodes()[mapGenerator.getNodes().Count - 1], weightOfCost);
		/*
		for(int i = 0; i < path.Count; i++)
		{
			Debug.Log(path[i].getPosition().ToString());
		}
		*/
		drawPath();
	}

	public void regenerate()
	{
		mapGenerator.setNoiseScale(noiseScale / 20f);
		mapGenerator.generateMap(width, height, noise);
		mapGenerator.renderMap();
	}

	public AStarAlgo getAStarAlgo()
	{
		return aStarAlgo;
	}

	public CostMapGenerator getMapGenerator()
	{
		return mapGenerator;
	}

	[HideInInspector]
	public int selectedX = 0;
	[HideInInspector]
	public int selectedY = 0;
	[HideInInspector]
	public double selectedF = 0;
	public void deleteSelected()
    {
		mapGenerator.deleteSelected(selectedX, selectedY);
    }

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
			if (n == null) continue;
			x = n.getPosition().x;
			y = n.getPosition().y;

			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(new Vector3(x, n.getCost() * terrainHeight, y), 0.1f);

			// draw neighbors of selected node
			if(x == selectedX && y == selectedY)
			{
				foreach(Node neighbor in n.getNeighbors())
				{
					Gizmos.color = Color.red;
					Gizmos.DrawLine(
						new Vector3(x, n.getCost() * terrainHeight, y),
						new Vector3(
							neighbor.getPosition().x,
							neighbor.getCost() * terrainHeight,
							neighbor.getPosition().y
					));

					Gizmos.DrawCube(
						new Vector3(x - width, 0, y),
						new Vector3(0.1f, 0.1f, 0.1f)
						);
				}
				selectedF = n.f;
			}
		}
		renderPath();
	}

	private void drawPath()
    {
		if(path != null)
		{
			Vector3[] positions = new Vector3[path.Count];
			lineRenderer.positionCount = path.Count;
			for(int i = 0; i < path.Count - 1; i++)
			{
				positions[i] = new Vector3(path[i].getPosition().x - width, 0.1f, path[i].getPosition().y);
				positions[i + 1] = new Vector3(path[i + 1].getPosition().x - width - 10, 0.1f, path[i + 1].getPosition().y);
			}
			lineRenderer.SetPositions(positions);
		}
    }

	private void renderPath()
	{
		if(path != null)
		{
			for(int i = 0; i < path.Count - 1; i++)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawLine(
					new Vector3(path[i].getPosition().x, path[i].getCost() * terrainHeight, path[i].getPosition().y),
					new Vector3(path[i + 1].getPosition().x, path[i + 1].getCost() * terrainHeight, path[i + 1].getPosition().y)
					);
				
				Gizmos.DrawLine(
					new Vector3(path[i].getPosition().x - width - 10, 0.1f, path[i].getPosition().y),
					new Vector3(path[i + 1].getPosition().x - width - 10, 0.1f, path[i + 1].getPosition().y)
					);
			}
		}
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestHandler))]
class HandlerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var handler = (TestHandler)target;
		if (handler == null) return;

		GUILayout.BeginHorizontal();
		GUILayout.Label("selected:");
		if (GUILayout.Button("+"))
		{
			handler.selectedX++;
		}

		if (GUILayout.Button("-"))
		{
			handler.selectedX--;
		}

		GUILayout.Label("x: " + handler.selectedX.ToString());

		if (GUILayout.Button("+"))
		{
			handler.selectedY++;
		}

		if (GUILayout.Button("-"))
		{
			handler.selectedY--;
		}
		GUILayout.Label("y: " + handler.selectedY.ToString());

		GUILayout.Label("F: " + handler.selectedF.ToString());
		if (GUILayout.Button("delete selected"))
        {
			handler.deleteSelected();
        }

		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("weightOfCost");
		handler.weightOfCost = GUILayout.HorizontalSlider(handler.weightOfCost, 0f, 10f);
		GUILayout.Label(handler.weightOfCost.ToString("0.0000"));
		if (GUILayout.Button("resetF"))
		{
			handler.getAStarAlgo().resetF(handler.getMapGenerator().getNodes()[0]);
		}

		if (GUILayout.Button("regenerate map"))
		{
			handler.regenerate();
		}

		if (GUILayout.Button("calculate"))
		{
			handler.calculate();
		}
		GUILayout.EndHorizontal();
	}
}
#endif
