using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarAlgo
{
    public AStarAlgo()
    {

    }

    public List<Node> solve(Node startNode, Node goalNode)
    {
        List<Node> openSet = new List<Node>();
        List<Node> closedSet = new List<Node>();

        startNode.g = 0;
        startNode.f = getDistance(startNode, goalNode);

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // find node with lowest f in the open set
            Node q = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].f < q.f || (openSet[i].f == q.f && Random.value < 0.5))
                {
                    q = openSet[i];
                }
            }
            Debug.Log("selected node " + q.getPosition().ToString());

            openSet.Remove(q);
            closedSet.Add(q);

            foreach (Node successor in q.getNeighbors())
            {
                if (successor.samePosition(goalNode))
                {
                    closedSet.Add(successor);
                    return closedSet;
                }

                successor.g = q.g + getDistance(startNode, successor);
                successor.h = getDistance(successor, goalNode);

                successor.f = successor.g + successor.h;

                if (!ifLowerFExists(openSet, successor) && !ifLowerFExists(closedSet, successor))
                {
                    openSet.Add(successor);
                }
                // Debug.Log("g: " + successor.g + " h: " + successor.h + " f: " + successor.f);
            }
            closedSet.Add(q);
            // Debug.Log("open set count " + openSet.Count);
        }

        // No path found
        return null;
    }

    private float getDistance(Node a, Node b)
    {
        float xDist = (a.getPosition().x - b.getPosition().x);
        float yDist = (a.getPosition().y - b.getPosition().y);
        float outDist = Mathf.Sqrt((xDist * xDist) + (yDist * yDist));
        return outDist;
    }

    private bool ifLowerFExists(List<Node> set, Node node)
    {
        for(int i = 0; i < set.Count; i++)
        {
            if(set[i].samePosition(node) && set[i].f < node.f)
            {
                return true;
            }
        }
        return false;
    }
}
