﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// code by pulak

public class AStar{
	public const int MODE_VH = 0; // vcost heuristic mode
	public const int MODE_N = 1; // normal mode

	public static int Mode = MODE_VH;

	static int GetDistance(Node nodeA, Node nodeB) {
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}

	/* get min cost node
	 * from list of nodes */
	static Node getMinCostNode(List<Node> nodes){
		Node node = nodes[0];
		for (int i = 1; i < nodes.Count; i ++) {
			if(nodes[i].fCost < node.fCost)
				node = nodes[i];

			if(Mode == MODE_VH){
				/* New heuristic based 
				on constraint avoidance */
				if (nodes[i].fCost == node.fCost) 
					if(nodes[i].vcost < node.vcost)
						node = nodes[i];
			}else if(Mode == MODE_N){
				/* Original heuristic */
				if (nodes[i].fCost == node.fCost) 
					if (nodes[i].hCost < node.hCost) node = nodes[i];
			}
		}

		return node;
	}

	
	static int GetVCost(Node n, List<List<Node>> soln, int curAgent){
		if(soln==null || curAgent<0) return 0;
		int vcost = 0;
		for(int i=0;i<soln.Count;i++){
			if(i!=curAgent){
				// check conflicts for all other agents
				List<Node> path = soln[i];
				if(n.time < path.Count && path[n.time] == n)
						vcost++;
			}
		}
		return vcost;
	}

	/* Find the minimum path 
	while obeying the constraints (cstr) */
	static public List<Node> FindMinPath(Vector3 startPos, Vector3 targetPos, Grid grid, List<State> cstr, List<List<Node>> prevSoln, int curAgent) {
		grid.ResetNodes();

		// get the nodes of the agent and target
		Node startNode = grid.NodeFromWorldPoint(startPos);
		Node targetNode = grid.NodeFromWorldPoint(targetPos);
		
		/* open set contains 
		the list of nodes to be visited*/
		List<Node> openSet = new List<Node>();

		/* closed set contains
		 * the set of nodes 
		 * already visited */
		HashSet<Node> closedSet = new HashSet<Node>();

		/* add the starting node */
		openSet.Add(startNode);

		while (openSet.Count > 0) {
			// get the min cost node from the open set
			Node node = getMinCostNode (openSet);

			openSet.Remove(node);
			closedSet.Add(node);

			/* if target node is found */
			if (node == targetNode) {
				return RetracePath(startNode,targetNode);
			}

			foreach (Node neighbour in grid.GetNeighbours(node)) {
				/* check constraint match */
				if(cstr.Exists(
					state => state.node.gridX == neighbour.gridX && state.node.gridY == neighbour.gridY && state.time == node.time + 1
				)) continue;

				/* check if neighbour is walkable 
				 * and not already visited*/
				if (!neighbour.walkable || closedSet.Contains(neighbour)) {
					continue;
				}

				int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);

				/* if new cost is smaller 
				 * than previous or if 
				 * neighbor is a new entry 
				 * to open set */
				if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
					/* update costs */
					neighbour.gCost = newCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);

					/* main path assign part */
					neighbour.parent = node;
					neighbour.time = neighbour.parent.time+1;

					/* updating v cost */
					neighbour.vcost = neighbour.parent.vcost + GetVCost(neighbour, prevSoln, curAgent);

					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);
				}
			}
		}

		List<Node> path = new List<Node> ();
		path.Add(startNode);
		return path;
	}

	static List<Node> RetracePath(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Add(startNode);
		path.Reverse();

		return path;
	}
}
