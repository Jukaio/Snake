using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Player : Body
{
	public enum Mode
	{
		Human,
		AI
	}

	private const float QUARTER_CIRCLE_DEGREES = 90.0f;

	[SerializeField] private CameraEaser cam = null;
	[SerializeField] private World world = null;
	[SerializeField] private Food food = null;
	[SerializeField] private Vector3Int spawn;
	[SerializeField] private Mode mode = Mode.AI;
	[SerializeField] private Pathfinding.Mode pathfindingMode = Pathfinding.Mode.JumpPointSearch; 

	private Vector3 rollAxis = Vector3.forward;
	private Vector3 pitchAxis = Vector3.right;
	private Vector3 direction = Vector3.up;

	private Pathfinding pathfinding = null;

	private void Start()
	{
		transform.position = world.IndexToWorld(spawn);
		
		pathfinding = new Pathfinding(world)
			.SetBlockFunc(IsIndexBlocked);
		food.Respawn(this);
	}
	/* The snake is as fast as FixedUpdate is getting called. To change the game speed, you must change
	 * the fixed update step in the Unity settings. This is meant to mimic the old game speed ideas to
	 * manipulate the hardwares frequency to speed up/slow down the game */
	private void FixedUpdate()
    {
		HandleInputAndRefreshDirection();

		OnTick(direction);

		cam.CalculateStatePointsAndStartEaseRoutine(rollAxis, direction);
	}
	private void OnTick(Vector3 direction)
	{
		var nextPosition = transform.position + direction;
		var nextIndex = world.WorldToIndex(nextPosition);
		nextIndex = world.WrapIndex(nextIndex);

		MoveToNextGridIndex(nextIndex);

		if (IsGameOver(nextIndex)) {
			Next.DestroyAll((body) =>
			{
				var index = world.WorldToIndex(body.CurrentPosition);
				world.Set<Body>(index, null);
				food.Release(body);

			});
			Next = null;
		} 
		else {
			UpdateGrid();
			CheckSnakeGrow(nextIndex);
		}

		return;
	}
	private void UpdateGrid()
	{
		TransformAll((currentBody, nextBody) =>
		{
			world.Set(world.WorldToIndex(currentBody.CurrentPosition), currentBody);
			if(nextBody == null) {
				world.Set<Body>(world.WorldToIndex(currentBody.PreviousPosition), null);
			}
		});
	}

	private Stack<Vector3Int> FindBestPath()
	{
		var start = world.WorldToIndex(CurrentPosition);
		var goal = world.WorldToIndex(food.transform.position);
		return pathfinding.SetMode(pathfindingMode)
				.Search(start, goal);
	}
	private float WrappedDistance(Vector3Int index, Vector3Int goal)
	{
		var max = Vector3Int.Max(index, goal);
		var min = Vector3Int.Min(index, goal);
		return Pathfinding.Manhattan(Vector3Int.zero, world.Count - max + Vector3Int.zero + min);
	}
	private float Heuristic(Vector3Int index, Vector3Int goal)
	{
		float score = Pathfinding.Manhattan(index, goal);
		var cell = world.Get<Pathfinding.Cell>(index);
		foreach (var direction in cell.dirs) {
			var neighbourCell = world.WrapIndex(index + direction);
			if (world.Get<Body>(neighbourCell) != null) {
				score -= 2.0f;
			}
		}
		return score;
	}
	private bool IsIndexBlocked(Vector3Int index)
	{
		return world.Get<Body>(index) != null;
	}

	private void CheckSnakeGrow(Vector3Int nextIndex)
	{
		if (world.TryGet(nextIndex, out Food food)) {
			if (food != null) {
				var nextBody = food.BodyInstance;
				nextBody.SetBodyPosition(CurrentPosition);
				Insert(nextBody);
				food.Respawn(this);
			}
		}
	}
	private void MoveToNextGridIndex(Vector3Int nextIndex)
	{
		Vector3 nextPosition = world.IndexToWorld(nextIndex);
		SetBodyPosition(nextPosition);
		TransformAll((currentBody, nextBody) =>
		{
			if (nextBody != null) {
				nextBody.SetBodyPosition(currentBody.PreviousPosition);
			}
			// 112 bytes -> typeof()
			if (world.TryGet(world.WorldToIndex(currentBody.CurrentPosition), out Color color)) {
				currentBody.Material.SetColor("ColorB", new Color(color.g, color.b, color.r, 1.0f));
				currentBody.Material.SetColor("ColorA", new Color(color.b, color.r, color.g, 1.0f));
			}
		});
	}
	private bool IsGameOver(Vector3Int nextIndex)
	{
		if (world.TryGet(nextIndex, out Body body)) {
			if (body != null) {
				return true;
			}
		}
		return false;
	}
	private void HandleInputAndRefreshDirection()
	{
		switch (mode) {
			case Mode.Human:	HandleHumanInput(); break;
			case Mode.AI:		HandleAIInput();	break;
		}
	}
	private void HandleHumanInput()
	{
		if (Input.GetKey(KeyCode.W)) {
			rollAxis = Quaternion.AngleAxis(-QUARTER_CIRCLE_DEGREES, pitchAxis) * rollAxis;
		}
		else if (Input.GetKey(KeyCode.S)) {
			rollAxis = Quaternion.AngleAxis(QUARTER_CIRCLE_DEGREES, pitchAxis) * rollAxis;
		}
		else if (Input.GetKey(KeyCode.A)) {
			pitchAxis = Quaternion.AngleAxis(-QUARTER_CIRCLE_DEGREES, rollAxis) * pitchAxis;
		}
		else if (Input.GetKey(KeyCode.D)) {
			pitchAxis = Quaternion.AngleAxis(QUARTER_CIRCLE_DEGREES, rollAxis) * pitchAxis;
		}
		direction = Vector3.Cross(rollAxis, pitchAxis);
	}
	private void HandleAIInput()
	{
		var path = FindBestPath();
		if (path != null) {
			var next = path.Pop();
			if (world.TryGet(next, out Pathfinding.Cell cell)) {
				direction = cell.cameFromDir.Value;
			}
		}
	}

#region PLAYER_DEBUG
#if UNITY_EDITOR

	[Space]
	[Header("Debug")]
	[SerializeField] private bool showDebug = true;
	[SerializeField, Range(0.1f, 50.0f)] private float gizmoLineLength = 5.0f;

	private void OnDrawGizmos()
	{
		if(!showDebug) {
			return;
		}

		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(Vector3.zero, rollAxis * gizmoLineLength);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(Vector3.zero, pitchAxis * gizmoLineLength);

		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(Vector3.zero, direction * gizmoLineLength);

	}
#endif
#endregion
}
