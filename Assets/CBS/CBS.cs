using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CBS : MonoBehaviour{

	public Transform[] agents;
    public Transform[] targets;

	// Vector3[] agentPositions;

	private bool cbsComplete=true;

	public int maxAgents;
	int nAgents;

	public bool testing;
	public bool logTestResults;
    string logdir = "Assets/Resources/";

	Grid grid;


	void Awake() {
		nAgents = agents.Length<targets.Length?agents.Length:targets.Length;
		nAgents = nAgents<maxAgents?nAgents:maxAgents; // less than max agents

		for(int i = nAgents; i < agents.Length; i++)
			agents[i].gameObject.SetActive(false);

		for(int i = nAgents; i < targets.Length; i++)
			targets[i].gameObject.SetActive(false);

		// get the whole grid layout
		grid = GetComponent<Grid> ();

	}

	void Update() {
		// main algorithm
		if(cbsComplete){
			cbsComplete = false;
			if(testing)
				AStar.Mode = AStar.MODE_VH;
			else 
				AStar.Mode = AStar.MODE_N;
			StartCoroutine(StepCBS());
		}
	}

	void Log(string str){
		StreamWriter writer;
		if(AStar.Mode == AStar.MODE_VH)
        	writer = new StreamWriter(logdir+"VH.txt", true);
		else
        	writer = new StreamWriter(logdir+"N.txt", true);
		writer.WriteLine(str);
        writer.Close();
	}

	/* 	Takes a list of new 
		constraints, previous 
		solution and the new 
		constrained agent */
	List<List<Node>> GetSolution(List<State>[] cstr, List<List<Node>> prevSoln=null, int cstrAgent=-1){
		List<List<Node>> soln = new List<List<Node>> ();
		List<Node> cstrPath = new List<Node>();

		// solve constraint agent first
		if(cstrAgent!=-1){
			cstrPath = AStar.FindMinPath(agents[cstrAgent].position, targets[cstrAgent].position, grid, cstr[cstrAgent], prevSoln, cstrAgent);
			prevSoln[cstrAgent] = cstrPath;
		}

		// solve the rest of the agents
		for(int i = 0; i <cstr.Length; i++){
			if(cstrAgent==-1 || i!=cstrAgent){
				List<Node> path = AStar.FindMinPath (agents[i].position, targets[i].position, grid, cstr[i], prevSoln, i);
				if(prevSoln!=null)prevSoln[i] = path;
				soln.Add(path);
			}
			if(cstrAgent!=-1 && i==cstrAgent)
				soln.Add(cstrPath);
		}
				
		return soln;
	}

	int GetSolutionCost(List<List<Node>> paths){
		int cost=0;
		foreach(List<Node> path in paths)
			cost += path.Count;
		return cost;
	}

	/* get min cost node
	 * from list of nodes */
	static CTNode GetMinCostNode(List<CTNode> nodes){
		CTNode node = nodes[0];
		for (int i = 1; i < nodes.Count; i ++) 
			if (nodes[i].cost < node.cost) 
				node = nodes[i];
			else if(nodes[i].cost == node.cost 
			&& nodes[i].GetConstraintsCount() < node.GetConstraintsCount())
				node = nodes[i];

		return node;
	}





	IEnumerator StepCBS(){
		grid.paths = null;
		List<CTNode> OPEN = new List<CTNode> ();
		List<Conflict> curConflicts =  new List<Conflict>();
		
		/* empty constraints */
		List<State>[] emptyCstr = new List<State> [nAgents];
		for(int i=0;i<emptyCstr.Length;i++)
			emptyCstr[i] = new List<State>();

		List<List<Node>> soln = GetSolution(emptyCstr);
		int cost = GetSolutionCost(soln);
		
		/* add root node */
		CTNode curNode = new CTNode(emptyCstr, soln, cost);
		OPEN.Add(curNode);

		int nodesTraversed = 0;
		while(OPEN.Count > 0){
			nodesTraversed++;

			curNode = GetMinCostNode(OPEN);
			OPEN.Remove(curNode);

			grid.paths = curNode.soln;
			curConflicts = GetConflicts(curNode.soln);

			// Debug.Log(curNode.cost);
			yield return 0;
			if(curConflicts.Count == 0){
				Debug.Log("final->");
				Debug.Log(curNode.cost);
				break;
			}


			else if(curConflicts.Count > 0){
				foreach(Conflict conflict in curConflicts){
					foreach(int agentID in conflict.agents){
						// copy constraints
						List<State>[] newConstraints = new List<State>[nAgents];
						for(int i=0;i<newConstraints.Length;i++)
							newConstraints[i] = new List<State>(curNode.cstr[i]);

						// add new constraint
						newConstraints[agentID].Add(new State(conflict.node, conflict.time));

						// solve with new constraints
						List<List<Node>> newSoln = GetSolution(newConstraints, curNode.soln, agentID);
						int newCost = GetSolutionCost(newSoln);

						// add new node
						CTNode newNode = new CTNode(newConstraints, newSoln, newCost);
						OPEN.Add(newNode);
					}
				}
			}
		}

		if(testing && logTestResults) Log(nodesTraversed.ToString()+", "+curNode.cost);
		if(AStar.Mode == AStar.MODE_VH){
			AStar.Mode = AStar.MODE_N;
			StartCoroutine(StepCBS());
		}else if(AStar.Mode == AStar.MODE_N){
			StartCoroutine(FlyDrones());
		}
	}

	IEnumerator FlyDrones(){
		int minTimeStep = int.MaxValue;
		int MAXSTEPS = 10;
		float speed = grid.nodeRadius*100/MAXSTEPS;

		foreach(List<Node> path in grid.paths)
			if(path.Count != 1 && path.Count < minTimeStep) 
				minTimeStep = path.Count;

		if(minTimeStep == int.MaxValue){
			Debug.Log("Deadlock!");
			yield break;
		}

		// for each time step
		for(int t=0;t<minTimeStep;t++){ 
			// for each agent
			if(!testing && t>0)
				for(int step=0;step<MAXSTEPS;step++){
					for(int i=0;i<nAgents;i++)
						if(t < grid.paths[i].Count)
							agents[i].position = Vector3.MoveTowards(agents[i].position, grid.paths[i][t].worldPosition, speed*Time.deltaTime);
					yield return 0;
				}

			for(int i=0;i<nAgents;i++){
				if(t < grid.paths[i].Count)
					agents[i].position = grid.paths[i][t].worldPosition;
				
				// change target location to new random node
				if(grid.paths[i].Count!=0 && t == grid.paths[i].Count-1){
					Vector3 newNodePos = grid.GetRandomNode().worldPosition;
					targets[i].position = new Vector3(newNodePos.x, newNodePos.y, newNodePos.z);
				}
			}
			yield return 0;
		}
		
		cbsComplete = true;
	}

	List<Conflict> GetConflicts(List<List<Node>> paths){
		List<Conflict> conflicts = new List<Conflict>();

		bool conflictFound = false;
		bool agentLeft=false;

		for(int t=0;!conflictFound;t++,agentLeft=false){
			for(int i=0;i<paths.Count && !conflictFound;i++){
				for(int j=i+1;j<paths.Count && !conflictFound;j++){
					if(i!=j){

						if(t < paths[i].Count && t < paths[j].Count){
							agentLeft = true;

							/* CASE 1 
								0: A B C F
								1: D E C G
									|
									------ conflict (same node at same time)
							
							*/
							if(paths[i][t] == paths[j][t]){
								List<int> curAgents = new List<int> ();
								curAgents.Add(i);
								curAgents.Add(j);
								conflicts.Add(new Conflict(curAgents, paths[i][t], t));
								conflictFound = true;
								continue;
							}

							/* CASE 2
								0: A B C E
								1: D C B F
									| |
									------- conflict (crossing each other)
							*/
							if(t+1 < paths[i].Count && t+1 < paths[j].Count){
								if(paths[i][t] == paths[j][t+1] && paths[i][t+1] == paths[j][t]){
									List<int> curAgents = new List<int> ();
									curAgents.Add(i);
									conflicts.Add(new Conflict(curAgents, paths[i][t+1], t+1));

									curAgents = new List<int> ();
									curAgents.Add(j);
									conflicts.Add(new Conflict(curAgents, paths[j][t+1], t+1));
									conflictFound = true;
									continue;
								}
							}
						}

						// check stationary object collision
						if(t>0 && t<paths[j].Count && paths[i].Count == 1 && paths[j][t] == paths[i][0]){
							List<int> curAgents = new List<int> ();
							curAgents.Add(j);
							conflicts.Add(new Conflict(curAgents, paths[i][0], t));
							conflictFound = true;
							continue;
						}
						if(t>0 && t<paths[i].Count && paths[j].Count ==1 && paths[i][t] == paths[j][0]){
							List<int> curAgents = new List<int> ();
							curAgents.Add(i);
							conflicts.Add(new Conflict(curAgents, paths[j][0], t));
							conflictFound = true;
							continue;
						}

					}
				}
			}
			
			if(!agentLeft) break;
		}

		return conflicts;
	}
}
