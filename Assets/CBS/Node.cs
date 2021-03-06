﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node {
	
	public bool walkable;
	public Vector3 worldPosition;
	public int gridX;
	public int gridY;

	public int hCost;
	public int gCost;
	public int vcost;
	public Node parent = null;
	public int time=0;
	
	public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
	}

	public int fCost {
		get {
			return gCost + hCost;
		}
	}
}

/* A particular state for A star */
public class State {
	public Node node; // node position
	public int time=0;  // time step

	public State(Node _node, int _time){
		node = _node;
		time = _time;
	}
}

public class Conflict{
	public Node node;
	public int time = 0;
	public List<int> agents; 

	public Conflict(List<int> _agents, Node _node, int _time){
		node = _node;
		agents = new List<int>(_agents);
		time = _time;
	}
}

/* Constraint tree node */
public class CTNode{
	public List<State>[] cstr; // constraints
	public int cost;
	public List<List<Node>> soln = new List<List<Node>>(); // solution

	public CTNode(List<State>[] _constraints, List<List<Node>> _solution, int _cost){
		cstr = new List<State>[_constraints.Length];
		for(int i=0;i<cstr.Length;i++){
			cstr[i] = new List<State>(_constraints[i]);
		}

		foreach(List<Node> path in _solution)
			soln.Add(new List<Node> (path));

		cost = _cost;
	}


	public int GetConstraintsCount(){
		int count = 0;
		for(int i=0;i<cstr.Length;i++){
			count+= cstr[i].Count;
		}
		return count;
	}
}