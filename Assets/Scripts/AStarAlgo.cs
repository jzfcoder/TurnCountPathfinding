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

    public List<Node> solve(Node startNode, Node goalNode, float costWeight, float turnWeight)
    {
        List<Node> openSet = new List<Node>();
        List<Node> closedSet = new List<Node>();

        Node prevQ = startNode;
        float prevDirection = 0;
        /*  1  2  3
         *   \ | /
         * 4 - n - 5
         *   / | \
         *  5  6  7
         */

        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            Node q = openSet[0];
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

            float newG;
            float currentDirection = 0;
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
    
    public float getDirection(Node prev, Node next)
    {
        if(prev == null)
        {
            return -1;
        }
        float xDist = next.getPosition().x - prev.getPosition().x;
        float yDist = next.getPosition().y - prev.getPosition().y;
        return Mathf.Atan(yDist / xDist);
    }

    private float calculateTurnCost(float prevDir, float dir)
    {
        return Mathf.Abs(dir - prevDir);
    }

    private List<Node> traceback(Node startNode, Node goalNode)
    {
        List<Node> path = new List<Node>();
        Node current = goalNode;

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
        float xDist = Mathf.Abs(a.getPosition().x - b.getPosition().x);
        float yDist = Mathf.Abs(a.getPosition().y - b.getPosition().y);
        float outDist = (xDist + yDist) - (2 * yDist) * Mathf.Min(xDist, yDist);
        return outDist;
    }

    private float getExactDistance(Node a, Node b)
    {
        float xDist = (a.getPosition().x - b.getPosition().x);
        float yDist = (a.getPosition().y - b.getPosition().y);
        float outDist = Mathf.Sqrt((xDist * xDist) + (yDist * yDist));
        return outDist;
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

    public void resetF(Node start)
    {
        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> visited = new HashSet<Node>();

        queue.Enqueue(start);
        visited.Add(start);

        while(queue.Count > 0)
        {
            Node currentNode = queue.Dequeue();

            currentNode.g = 0;
            currentNode.h = 0;

            foreach (Node n in currentNode.getNeighbors())
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
