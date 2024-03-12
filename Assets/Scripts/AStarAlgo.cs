using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarAlgo
{
    public AStarAlgo()
    {

    }

    public List<Node> solve(Node startNode, Node goalNode, float costWeight)
    {
        return solve(startNode, goalNode, costWeight, 0);
    }

    Node prevQ, q;
    float prevDirection, newG, currentDirection;
    public List<Node> solve(Node startNode, Node goalNode, float costWeight, float turnWeight)
    {
        List<Node> openSet = new List<Node>();
        List<Node> closedSet = new List<Node>();

        prevQ = startNode;
        prevDirection = 0;
        /*  1  2  3
         *   \ | /
         * 4 - n - 5
         *   / | \
         *  5  6  7
         */

        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            q = openSet[0];
            for(int i = 1; i < openSet.Count; i++)
            {
                if(openSet[i].f < q.f || openSet[i].f == q.f)
                {
                    if(openSet[i].h < q.h)
                    {
                        q = openSet[i];
                    }
                }
            }

            // prevDirection = getDirection(prevQ, q);
            openSet.Remove(q);
            closedSet.Add(q);

            if(q.samePosition(goalNode))
            {
                return traceback(startNode, goalNode);
            }

            newG = 0;
            currentDirection = 0;
            foreach (Node successor in q.getNeighbors())
            {
                if (successor == null || closedSet.Contains(successor))
                {
                    continue;
                }
                currentDirection = getDirection(q, successor);

                // successor.g = q.g + getDistance(successor, q) + (successor.getCost() * weight);
                newG = q.g + getDistance(successor, q) + (successor.getCost() * costWeight) + (calculateTurnCost(prevDirection, currentDirection) * turnWeight);
                // newG = q.g + getDistance(successor, q) + (successor.getCost() * costWeight);
                if(newG < successor.g || !openSet.Contains(successor))
                {
                    successor.g = newG;
                    successor.h = getDistance(successor, goalNode);
                    successor.parent = q;

                    if(!openSet.Contains(successor))
                    {
                        openSet.Add(successor);
                    }
                }
            }
        }

        return null;
    }

    private float xDist;
    private float yDist;
    public float getDirection(Node prev, Node next)
    {
        if(prev == null)
        {
            return -1;
        }
        xDist = next.getPosition().x - prev.getPosition().x;
        yDist = next.getPosition().y - prev.getPosition().y;
        return Mathf.Atan(yDist / xDist);
    }

    private float calculateTurnCost(float prevDir, float dir)
    {
        return Mathf.Abs(dir - prevDir);
    }

    List<Node> path;
    Node current;
    private List<Node> traceback(Node startNode, Node goalNode)
    {
        path = new List<Node>();
        current = goalNode;

        while (!current.samePosition(startNode))
        {
            path.Add(current);
            current = current.parent;
        }
        path.Add(current);
        path.Reverse();

        return path;
    }

    private float getDistance(Node a, Node b)
    {
        return getExactDistance(a, b);
    }

    private float getManhattanDistance(Node a, Node b)
    {
        xDist = Mathf.Abs(a.getPosition().x - b.getPosition().x);
        yDist = Mathf.Abs(a.getPosition().y - b.getPosition().y);
        return (xDist + yDist) - (2 * yDist) * Mathf.Min(xDist, yDist);
    }

    private float getExactDistance(Node a, Node b)
    {
        xDist = (a.getPosition().x - b.getPosition().x);
        yDist = (a.getPosition().y - b.getPosition().y);
        return Mathf.Sqrt((xDist * xDist) + (yDist * yDist));
    }

    private bool lowerFExists(List<Node> set, Node node)
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

    Queue<Node> queue;
    HashSet<Node> visited;
    Node currentNodeF;
    public void resetF(Node start)
    {
        queue = new Queue<Node>();
        visited = new HashSet<Node>();

        queue.Enqueue(start);
        visited.Add(start);

        while(queue.Count > 0)
        {
            currentNodeF = queue.Dequeue();

            currentNodeF.g = 0;
            currentNodeF.h = 0;

            foreach (Node n in currentNodeF.getNeighbors())
            {
                if (!visited.Contains(n))
                {
                    queue.Enqueue(n);
                    visited.Add(n);
                }
            }
        }
    }
}
