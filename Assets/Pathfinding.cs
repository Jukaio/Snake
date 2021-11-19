using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Pathfinding
{
	public enum Mode
	{
		AStar,
		JumpPointSearch
	}
	public struct Cell
	{
		public float f;
		public float g;
		public bool v;
		public Vector3Int[] dirs;
		public Vector3Int? cameFromPos;
		public Vector3Int? cameFromDir;


		public Cell(float f, float g, Vector3Int[] dirs)
		{
			this.f = f;
			this.g = g;
			this.dirs = dirs;
			this.v = false;
			cameFromPos = null;
			cameFromDir = null;
		}

		public void ForEachDirection(System.Action<Vector3Int> action)
		{
			foreach (var dir in dirs) {
				action(dir);
			}
		}
	}

	private World world = null;
	private Queue<Vector3Int> open = new Queue<Vector3Int>();
	private TravelCostFunc travelCost = Manhattan;
	private IsBlockedFunc isBlocked = (_) => { return false; };
	private HeuristicFunc heuristic = Manhattan;
	private Mode mode = Mode.JumpPointSearch;
	
	public Pathfinding(World world)
	{
		this.world = world;
	}
	public Stack<Vector3Int> Search(Vector3Int start, Vector3Int goal) 
	{
		switch (mode) {
			case Mode.AStar:			return AStar(start, goal);
			case Mode.JumpPointSearch:	return JPS(start, goal);
		}
		throw new System.InvalidOperationException("Pathfinding mode does not exist");
	}
	
	public delegate float TravelCostFunc(Vector3Int a, Vector3Int b);
	public Pathfinding SetTravelCostFunc(TravelCostFunc travelCostAction)
	{
		travelCost = travelCostAction;
		return this;
	}
	
	public delegate bool IsBlockedFunc(Vector3Int index);
	public Pathfinding SetBlockFunc(IsBlockedFunc isBlockedAction)
	{
		isBlocked = isBlockedAction;
		return this;
	}
	
	public delegate float HeuristicFunc(Vector3Int a, Vector3Int b);
	public Pathfinding SetHeuristicFunc(HeuristicFunc heuristicAction)
	{
		heuristic = heuristicAction;
		return this;
	}
	public Pathfinding SetMode(Mode mode)
	{
		this.mode = mode;
		return this;
	}
	public static float Manhattan(Vector3Int a, Vector3Int b)
	{
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}

	private Stack<Vector3Int> AStar(Vector3Int start, Vector3Int goal)
	{
		if (!world.IsInBounds(start) || !world.IsInBounds(goal)) {
			return null;
		}
		if(start == goal) {
			var stack = new Stack<Vector3Int>();
			stack.Push(goal);
			return stack;
		}
		Initialise(start);
		return WhileSearching(start, goal, 
		(Vector3Int current, Vector3Int direction) =>
		{
			return world.WrapIndex(current + direction);
		});
	}
	private Stack<Vector3Int> JPS(Vector3Int start, Vector3Int goal)
	{
		if (!world.IsInBounds(start) || !world.IsInBounds(goal)) {
			return null;
		}
		if (start == goal) {
			var stack = new Stack<Vector3Int>();
			stack.Push(goal);
			return stack;
		}
		Initialise(start);
		return WhileSearching(start, goal, 
		(Vector3Int current, Vector3Int direction) => 
		{
			var point = Jump(current, direction, goal);
			if(point == current) {
				return world.WrapIndex(current + direction);
			}
			return point;
		});
	}
	private Stack<Vector3Int> ConstructPath(Vector3Int start, Vector3Int goal)
	{
		Stack<Vector3Int> path = new Stack<Vector3Int>();
		Vector3Int current = goal;
		while (current != start) {
			world.Get(current, out Cell currentCell);
			if (currentCell.cameFromPos == null) {
				break;
			}
			path.Push(current);
			current = currentCell.cameFromPos.Value;

		}
		return path;
	}
	private void Initialise(Vector3Int start)
	{
		open.Clear();
		world.Transform((Vector3Int index, ref Cell cell) =>
		{
			cell.f = float.PositiveInfinity;
			cell.g = float.PositiveInfinity;
			cell.v = false;
			cell.cameFromPos = null;
			cell.cameFromDir = null;
		});

		Cell cell = world.Get<Cell>(start);
		cell.g = 0.0f;
		cell.f = heuristic(start, start);
		world.Set(start, cell);
		open.Enqueue(start);
	}
	
	private delegate Vector3Int NextIndexAction(Vector3Int currentIndex, Vector3Int direction);
	private Stack<Vector3Int> WhileSearching(Vector3Int start, Vector3Int goal, NextIndexAction nextIndexFunc)
	{
		while (open.Count > 0) {
			open.OrderBy((index) => { return world.Get<Cell>(index).f; });
			var current = open.Dequeue();
			if (current == goal) {
				return ConstructPath(start, goal);
			}
			Cell currentCell = world.Get<Cell>(current);
			currentCell.v = true;

			currentCell.ForEachDirection((dir) =>
			{
				var nextIndex = nextIndexFunc(current, dir);
				world.Get(nextIndex, out Cell neighbourCell);
				if (neighbourCell.v == true || isBlocked(nextIndex)) {
					return;
				}
				float score = currentCell.g + travelCost(current, nextIndex);
				if (score < neighbourCell.g) {
					neighbourCell.cameFromPos = current;
					neighbourCell.cameFromDir = dir;
					neighbourCell.g = score;
					neighbourCell.f = score + heuristic(current, goal);
					if (!open.Contains(nextIndex)) {
						open.Enqueue(nextIndex);
					}
				}
				world.Set(nextIndex, neighbourCell);
			});
			world.Set(current, currentCell);
		}
		return null;
	}
	private Vector3Int Jump(Vector3Int current, Vector3Int direction, Vector3Int goal)
	{
		var count = 0;
		if(direction.x != 0 && direction.y == 0 && direction.z == 0) {
			count = world.Count.x;
		}
		else if (direction.x == 0 && direction.y != 0 && direction.z == 0) {
			count = world.Count.y;
		}
		else if (direction.x == 0 && direction.y == 0 && direction.z != 0) {
			count = world.Count.z;
		}
		else {
			throw new System.InvalidOperationException("Diagonal movement not available!");
		}

		Vector3Int step = current;
		float prevScore = travelCost(step, goal);
		for (int i = 1; i < count; i++)
		{
			Vector3Int prevStep = step;
			step = world.WrapIndex(current + (direction * i));
			if (step == goal) {
				return prevStep;
			}
			float curScore = travelCost(step, goal);
			if (curScore > prevScore || isBlocked(step)) {
				step = prevStep;
				break;
			}

			prevScore = curScore;
		}
		return step;
	}
}