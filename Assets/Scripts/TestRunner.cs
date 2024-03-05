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

	[Range(1, 50)]
	public int terrainHeight = 3;

	public Boolean noise = false;

	[Range(0, 10)]
	public int noiseScale = 2;

	public GameObject mapSource;
	public GameObject tile;
	public LineRenderer lineRenderer;
	public LineRenderer mapLineRenderer;

	[SerializeField]
	String filePath = Application.dataPath + "/data/" + "output.csv";

	private List<Node> path;
	private AStarAlgo aStarAlgo;

	private CostMapGenerator mapGenerator;
	private Stopwatch stopwatch;
	private StreamWriter writer;
	private int currentWidth;
	private int currentHeight;
	// Start is called before the first frame update
	void Start()
	{
		mapGenerator = new CostMapGenerator(startWidth, startHeight, terrainHeight, mapSource, noise, tile);
		mapGenerator.setNoiseScale(noiseScale / 20f);
		aStarAlgo = new AStarAlgo();
		stopwatch = new Stopwatch();
		currentHeight = startHeight;
		currentWidth = startWidth;

		filePath = Application.dataPath + "/data/" + startWidth + "to" + endWidth + "noise" + (noise ? noiseScale : -1) + ".csv";

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
		if(mapGenerator != null && path == null && currentWidth < endWidth)
		{
			
			currentWidth++;
			currentHeight++;
			regenerate(currentWidth, currentHeight);
			aStarAlgo.resetF(mapGenerator.getNodes()[0]);
			stopwatch.Start();
			calculate();
			stopwatch.Stop();
			recordData(currentWidth, stopwatch.ElapsedMilliseconds);
			path = null;
			if(currentWidth == endWidth)
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

	[HideInInspector]
	public float weightOfCost = 2f;
	public void calculate()
	{
		path = aStarAlgo.solve(mapGenerator.getNodes()[0], mapGenerator.getNodes()[mapGenerator.getNodes().Count - 1], weightOfCost);
		drawPath();
	}

	private void drawPath()
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

	public void recordData(double width, double time)
    {
        UnityEngine.Debug.Log(width + ", " + time);
		writer.WriteLine(width + "," + time);
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
	}
}
#endif
