using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class TestRunner : MonoBehaviour
{
	[Range(1, 100)]
	public int startWidth = 25;

	[Range(1, 100)]
	public int startHeight = 25;

	[Range(1, 100)]
	public int endWidth = 25;

	[Range(1, 100)]
	public int endHeight = 25;

	[Range(1, 100)]
	public int numTrials = 25;

	[Range(1, 50)]
	public int terrainHeight = 3;

	public Boolean noise = false;

	[Range(0, 10)]
	public int noiseScale = 2;

	public GameObject mapSource;
	public GameObject tile;
	public LineRenderer lineRenderer;
	public LineRenderer mapLineRenderer;
	public bool isDebugging = false;

	[SerializeField]
	String filePath = Application.dataPath + "/data/" + "output.csv";

	private List<Node> path;
	private AStarAlgo aStarAlgo;

	private CostMapGenerator mapGenerator;
	private Stopwatch stopwatch;
	private StreamWriter writer;
	private int currentWidth;
	private int currentHeight;
	private int currentTrial;
	// Start is called before the first frame update
	void Start()
	{
		mapGenerator = new CostMapGenerator(startWidth, startHeight, terrainHeight, mapSource, noise, tile);
		mapGenerator.setNoiseScale(noiseScale / 20f);
		aStarAlgo = new AStarAlgo();
		stopwatch = new Stopwatch();
		currentHeight = startHeight;
		currentWidth = startWidth;
		currentTrial = 1;

		filePath = Application.dataPath + "/data/" + startWidth + "to" + endWidth + "noise" + (noise ? noiseScale : -1) + "turnWeight" + weightOfTurn + ".csv";

		if(File.Exists(filePath))
		{
			File.Delete(filePath);
			UnityEngine.Debug.Log("deleted file successfully");
		}

		writer = new StreamWriter(filePath);
		writer.WriteLine("size,time,eval");
		path = null;
		isFirst = true;
		saveData = false;
	}

	private bool isFirst;
	private bool saveData;
	// Update is called once per frame
	void Update()
	{
		if(currentWidth == endWidth && currentTrial < numTrials)
		{
			currentTrial++;
			currentWidth = startWidth;
			currentHeight = startHeight;
		}
		if(mapGenerator != null && path == null && currentWidth < endWidth && !isDebugging)
		{
			currentWidth++;
			currentHeight++;
			regenerate(currentWidth, currentHeight);
			resetF();
			stopwatch.Start();
			calculate();
			stopwatch.Stop();
			drawPath();
			recordData(currentWidth, stopwatch.ElapsedMilliseconds, calculateEfficiency(path));
			path = null;
			if(currentWidth == endWidth && currentTrial == numTrials)
            {
				saveData = true;
            }
		}
		if(saveData && isFirst)
        {
			isFirst = false;
			writer.Flush();
			writer.Close();
			UnityEngine.Debug.Log("ended writing successfully");
        }
	}

	public void resetF()
    {
		aStarAlgo.resetF(mapGenerator.getNodes()[0]);
    }

	[HideInInspector]
	public float weightOfCost = 2f;

	[HideInInspector]
	public float weightOfTurn = 2f;
	public void calculate()
	{
		path = aStarAlgo.solve(mapGenerator.getNodes()[0], mapGenerator.getNodes()[mapGenerator.getNodes().Count - 1], weightOfCost, weightOfTurn);
	}

	public void drawPath()
    {
		if(path != null)
		{
			Vector3[] positions = new Vector3[path.Count];
			mapLineRenderer.positionCount = path.Count;
			lineRenderer.positionCount = path.Count;
			for(int i = 0; i < path.Count - 1; i++)
			{
				positions[i] = new Vector3(path[i].getPosition().x - currentWidth - 10, 0.1f, path[i].getPosition().y);
				positions[i + 1] = new Vector3(path[i + 1].getPosition().x - currentWidth - 10, 0.1f, path[i + 1].getPosition().y);
			}
			lineRenderer.SetPositions(positions);
		}
    }

	public void regenerate(int width, int height)
	{
		mapGenerator.setNoiseScale(noiseScale / 20f);
		mapGenerator.generateMap(width, height, noise);
		mapGenerator.renderMap(false);
	}

	public void recordData(double width, double time, double pathEfficiency)
    {
        UnityEngine.Debug.Log(width + ", " + time + ", " + pathEfficiency);
		writer.WriteLine(width + "," + time + "," + pathEfficiency);
    }

	public double pe_cellWeight = 1;
	public double pe_costWeight = 1;
	public double pe_turnWeight = 1;
	private float prevDir = 0;
	private float currentDir = 0;
	private double calculateEfficiency(List<Node> path)
    {
		double sum = 0;
		Node prevN;
		Node n = null;
		for(int i = 0; i < path.Count; i++)
        {
			prevN = n;
			n = path[i];
			prevDir = currentDir;
			currentDir = aStarAlgo.getDirection(prevN, n);
			sum += (n.getCost() * pe_costWeight) + pe_cellWeight + (i <= 1 ? (Math.Abs(currentDir - prevDir) * pe_turnWeight) : 0);
        }
		return sum;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(TestRunner))]
class RunnerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var handler = (TestRunner)target;
		if (handler == null) return;

		GUILayout.BeginHorizontal();
		GUILayout.Label("weightOfCost");
		handler.weightOfCost = GUILayout.HorizontalSlider(handler.weightOfCost, 0f, 10f);
		GUILayout.Label(handler.weightOfCost.ToString("0.0000"));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("weightOfTurn");
		handler.weightOfTurn = GUILayout.HorizontalSlider(handler.weightOfTurn, 0f, 10f);
		GUILayout.Label(handler.weightOfTurn.ToString("0.0000"));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("generate map"))
        {
			handler.regenerate(handler.startWidth, handler.startHeight);
			handler.resetF();
        }

		if (GUILayout.Button("calculate"))
        {
			handler.calculate();
			handler.drawPath();
        }
		GUILayout.EndHorizontal();
	}
}
#endif
