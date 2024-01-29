using UnityEngine;
using System.Collections.Generic;
using System;

public class Node
{
    public float g;
    public float h;
    public float f;

    private Vector2 position;
    private float cost;
    private List<Node> neighbors;

    public Node(Vector2 position, float cost)
    {
        this.position = position;
        this.cost = cost;
        this.neighbors = new List<Node>();
    }

    public Vector2 getPosition()
    {
        return position;
    }

    public float getCost()
    {
        return cost;
    }

    public void setCost(float cost)
    {
        this.cost = cost;
    }

    public void setNeighbors(List<Node> neighbors)
    {
        this.neighbors = neighbors;
    }

    public void addNeighbor(Node neighbor)
    {
        this.neighbors.Add(neighbor);
    }

    public List<Node> getNeighbors()
    {
        return neighbors;
    }

    public Boolean samePosition(Node n)
    {
        return (n.getPosition().x == this.position.x) && (n.getPosition().y == this.position.y);
    }
}
