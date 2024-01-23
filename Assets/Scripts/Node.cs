using UnityEngine;

public class Node
{
    private Vector2 position;
    private float cost;

    public Node(Vector2 position, float cost)
    {
        this.position = position;
        this.cost = cost;
    }

    public Vector2 getPosition()
    {
        return position;
    }

    public float getCost()
    {
        return cost;
    }
}
