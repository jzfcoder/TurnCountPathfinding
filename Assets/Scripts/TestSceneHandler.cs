using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TestSceneHandler : MonoBehaviour
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

	public int currentWidth;
	public int currentHeight;
	public int currentTrial;

	public double cellEvalWeight;
	public double costEvalWeight;
	public double turnEvalWeight;

	public double weightOfCost;
	public double weightOfTurn;

	public Boolean noise;
	public int noiseScale;

	public GameObject mapSource;
	public GameObject tile;
	public LineRenderer lineRenderer;
	public LineRenderer mapLineRenderer;

	[SerializeField]
	private String configPath = Application.dataPath + "/config.json";

	[SerializeField]
	private String dataPath;

	private AStarAlgo aStarAlgo;
	// Start is called before the first frame update
	void Start()
	{
		StartCoroutine(run());
	}

	IEnumerator run()
    {
		readConfig();
		if(currentTrial < 1)
        {
			currentTrial = 1;
        }
		dataPath = Application.dataPath + "/data/" + (startWidth + 1) + "to" + endWidth + "noise" + (noise ? noiseScale : -1) + "turnWeight" + weightOfTurn + "trialNumber" + currentTrial + ".csv";
		
		if((currentTrial == 1 && currentWidth == startWidth) || currentWidth == 0)
        {
			createDataTable();
			currentWidth = startHeight;
			currentHeight = startWidth;
        }

		currentWidth++;
		currentHeight++;

		UnityEngine.Debug.Log(currentWidth + ", " + currentHeight);

		CostMapGenerator mapGenerator = new CostMapGenerator(currentWidth, currentHeight, 3, mapSource, noise, tile);
		mapGenerator.setNoiseScale(noiseScale / 20f);
		mapGenerator.generateMap(currentWidth, currentHeight, noise);
		mapGenerator.renderMap(false);
		aStarAlgo = new AStarAlgo();
		Stopwatch stopwatch = new Stopwatch();
		List<Node> path = null;

		stopwatch.Start();
		path = aStarAlgo.solve(mapGenerator.getStart(), mapGenerator.getEnd(), (float) weightOfCost, (float) weightOfTurn);
		stopwatch.Stop();

		drawPath(path);

		recordData(currentWidth, stopwatch.ElapsedMilliseconds, calculateEfficiency(path));

		if(currentWidth >= endWidth)
        {
			currentWidth = startWidth;
			currentHeight = startHeight;
			currentTrial++;
        }

		if(currentTrial > numTrials)
        {
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

		writeConfig();
		yield return new WaitForSeconds(0);
		loadNextScene();
    }

	public void loadNextScene()
    {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
	// Update is called once per frame
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.P))
        {
        }
	}

	public void recordData(int width, double time, double pathEfficiency)
    {
		StreamWriter writer = new StreamWriter(dataPath, true);
		writer.WriteLine(width + "," + time + "," + pathEfficiency);
		writer.Flush();
		writer.Close();
    }

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
			sum += (n.getCost() * costEvalWeight) + cellEvalWeight + (i <= 1 ? (Math.Abs(currentDir - prevDir) * turnEvalWeight) : 0);
        }
		return sum;
    }

	public void drawPath(List<Node> path)
    {
		if(path != null)
		{
			Vector3[] positions = new Vector3[path.Count];
			mapLineRenderer.positionCount = path.Count;
			lineRenderer.positionCount = path.Count;
			for(int i = 0; i < path.Count - 1; i++)
			{
				positions[i] = new Vector3(path[i].getPosition().x, 0.1f, path[i].getPosition().y);
				positions[i + 1] = new Vector3(path[i + 1].getPosition().x, 0.1f, path[i + 1].getPosition().y);
			}
			lineRenderer.SetPositions(positions);
		}
    }

	public void createDataTable()
    {
		if(File.Exists(dataPath))
		{
			File.Delete(dataPath);
			UnityEngine.Debug.Log("deleted file successfully");
		}

		StreamWriter writer = new StreamWriter(dataPath);
		writer.WriteLine("size,time,eval");
		writer.Flush();
		writer.Close();
    }

	public int checkTrial()
    {
		if(!File.Exists(configPath))
		{
			UnityEngine.Debug.Log("file DNE");
			return 0;
		}
		string configContent = File.ReadAllText(configPath);
		ConfigData config = JsonUtility.FromJson<ConfigData>(configContent);
		return config.currentTrial;
    }

	public ConfigData readConfig()
    {
		if(!File.Exists(configPath))
		{
			UnityEngine.Debug.Log("file DNE");
			return null;
		}
		string configContent = File.ReadAllText(configPath);
		ConfigData config = JsonUtility.FromJson<ConfigData>(configContent);
		startWidth = config.startWidth;
		startHeight = config.startHeight;
		endWidth = config.endWidth;
		endHeight = config.endHeight;
		numTrials = config.numTrials;

		currentWidth = config.currentWidth;
		currentHeight = config.currentHeight;
		currentTrial = config.currentTrial;

		cellEvalWeight = config.cellEvalWeight;
		costEvalWeight = config.costEvalWeight;
		turnEvalWeight = config.turnEvalWeight;

		weightOfCost = config.weightOfCost;
		weightOfTurn = config.weightOfTurn;

		noise = config.noise;
		noiseScale = config.noiseScale;

		UnityEngine.Debug.Log("read from config successfully:" + configContent);
		return config;
    }

	public void writeConfig()
    {
		if(File.Exists(configPath))
		{
			File.Delete(configPath);
		}
		string json = JsonUtility.ToJson(new ConfigData
		{
			startWidth = this.startWidth,
			startHeight = this.startHeight,
			endWidth = this.endWidth,
			endHeight = this.endHeight,
			numTrials = this.numTrials,

			currentWidth = this.currentWidth,
			currentHeight = this.currentHeight,
			currentTrial = this.currentTrial,

			cellEvalWeight = this.cellEvalWeight,
			costEvalWeight = this.costEvalWeight,
			turnEvalWeight = this.turnEvalWeight,

			weightOfCost = this.weightOfCost,
			weightOfTurn = this.weightOfTurn,

			noise = this.noise,
			noiseScale = this.noiseScale,
		}, true);
		File.WriteAllText(configPath, json);
		UnityEngine.Debug.Log("wrote to config successfully" + json);
    }
}

[System.Serializable]
public class ConfigData
{
	public int startWidth;
	public int startHeight;
	public int endWidth;
	public int endHeight;
	public int numTrials;

	public int currentWidth;
	public int currentHeight;
	public int currentTrial;

	public double cellEvalWeight;
	public double costEvalWeight;
	public double turnEvalWeight;

	public double weightOfCost;
	public double weightOfTurn;

	public Boolean noise;
	public int noiseScale;
}


#if UNITY_EDITOR
[CustomEditor(typeof(TestSceneHandler))]
class SceneRunnerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var handler = (TestSceneHandler)target;
		if (handler == null) return;

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("read config"))
        {
			handler.readConfig();
        }
		if (GUILayout.Button("write config"))
        {
			handler.writeConfig();
        }
		if (GUILayout.Button("load next scene"))
        {
			handler.loadNextScene();
        }
		GUILayout.EndHorizontal();
	}
}
#endif
