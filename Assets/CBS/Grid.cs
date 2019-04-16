using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// code by pulak

public class Grid : MonoBehaviour {

	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	Node[,] grid;

	float nodeDiameter;
	int gridSizeX, gridSizeY;
	public List<Color> colors = new List<Color> ();

	public Node GetRandomNode(){
		for(int i=0;i<10;i++){
			int col = Random.Range(0, grid.GetLength(0));
			int row = Random.Range(0, grid.GetLength(1));
			if(grid[col,row].walkable)
				return grid[col,row];
		}
		return grid[0,0];
	}

	public Node GetNode(int x, int y){
		return grid[x,gridSizeY-y-1];
	}

	public void ResetNodes(){
		foreach(Node n in grid){
			n.time = 0;
			n.parent = null;
		}
	}

	void AddColors(){
		colors.Add(Color.yellow);
		colors.Add(Color.red);
		colors.Add(Color.green);
		colors.Add(Color.blue);
		colors.Add(Color.black);
	}

	void Awake() {
		AddColors();
		
		nodeDiameter = nodeRadius*2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
		CreateGrid();
	}

	void CreateGrid() {
		grid = new Node[gridSizeX,gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;

		for (int x = 0; x < gridSizeX; x ++) {
			for (int y = 0; y < gridSizeY; y ++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));
				grid[x,y] = new Node(walkable,worldPoint, x,y);
			}
		}
	}

	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0 || x==y || x ==-y)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(grid[checkX,checkY]);
				}
			}
		}

		return neighbours;
	}
	

	public Node NodeFromWorldPoint(Vector3 worldPosition) {
		float percentX = (worldPosition.x + gridWorldSize.x/2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y/2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY-1) * percentY);
		return grid[x,y];
	}

	public List<List<Node>> paths = new List<List<Node>>();
	void OnDrawGizmos() {
		Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y));

		if (grid != null) {
			foreach (Node n in grid) {
				bool _is_path = false;
				Gizmos.color = (n.walkable)?Color.white:Color.white;
				if(paths!=null)
					foreach(List<Node> path in paths) {
						if (path.Contains (n)) {
							Gizmos.color = colors[paths.FindIndex(tmp_path=>tmp_path==path)%5];
							_is_path = true;
						}
					}
				if(_is_path || !n.walkable)
					Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));
			}
		}
	}

}