using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Reflection;
#endif

// TODO: Make it a singleton
public class World : MonoBehaviour, MultiTransformable
{
	[SerializeField] private GridRegistry gridRegistry = new GridRegistry(new Vector3Int(10, 10, 10));
	
	public Vector3Int Count => gridRegistry.Count;
	
	// NOTE: Maybe use a parameter? The objects using this event are usually aware of world
	public event System.Action OnResize
	{
		add => gridRegistry.OnResize += value;
		remove => gridRegistry.OnResize -= value;
	}

	private void Awake()
	{
		// TODO: Make multiple grids of the same kind possible -> Create returns handle
		gridRegistry.CreateGrid(typeof(Body));
		gridRegistry.CreateGrid(typeof(Food));
		gridRegistry.CreateGrid(typeof(Color));
		gridRegistry.CreateGrid(typeof(Pathfinding.Cell));

		System.Action setGridNeighbours = () =>
		{
			Vector3Int[] wrappingDirections = new Vector3Int[] {
				Vector3Int.up,
				Vector3Int.down,
				Vector3Int.left,
				Vector3Int.right,
				Vector3Int.forward,
				Vector3Int.back
			};

			gridRegistry.Transform((Vector3Int index, ref Pathfinding.Cell cell) =>
			{
				cell = new Pathfinding.Cell(float.PositiveInfinity, 
										  float.PositiveInfinity,
										  wrappingDirections);
			});
		};
		setGridNeighbours();

		gridRegistry.OnResize += setGridNeighbours;


		gridRegistry.Transform((Vector3Int index, ref Color color) =>
		{
			var x = 1.0f / index.x;
			var y = 1.0f / index.y;
			var z = 1.0f / index.z;
			var length = x + y + z;

			var r = x / length;
			var g = y / length;
			var b = z / length;
			var temp = new Color(float.IsNaN(r) ? 0.0f : r,
								 float.IsNaN(g) ? 0.0f : g,
								 float.IsNaN(b) ? 0.0f : b);
			Color.RGBToHSV(temp, out var h, out var s, out var v);
			color = Color.HSVToRGB(Mathf.Clamp(h, 0.0f, 1.0f), 1.0f, 1.0f);
		});
	}
	public bool IsInBounds(Vector3 position)
	{
		return gridRegistry.IsInBounds(WorldToIndex(position));
	}
	public bool IsInBounds(Vector3Int index) 
	{
		return gridRegistry.IsInBounds(index);
	}
	public Vector3Int WorldToIndex(Vector3 at)
	{
		var size = Vector3.one;
		var anchor = Vector3.one / 2;
		var offset = new Vector3(size.x * anchor.x,
								 size.y * anchor.y,
								 size.z * anchor.z);
		at += offset;
		return new Vector3Int(Mathf.RoundToInt(at.x / size.x), 
							  Mathf.RoundToInt(at.y / size.y), 
							  Mathf.RoundToInt(at.z / size.z));
	}
	public Vector3 IndexToWorld(Vector3Int index)
	{
		var size = Vector3.one;
		var anchor = Vector3.one / 2;
		var offset = new Vector3(size.x * anchor.x,
								 size.y * anchor.y,
								 size.z * anchor.z);
		var at = new Vector3(size.x * index.x, size.y * index.y, size.z * index.z);
		return at - offset;
	}
	public Vector3Int WrapIndex(Vector3Int index)
	{
		index.x = Wrap(index.x, 0, gridRegistry.Count.x);
		index.y = Wrap(index.y, 0, gridRegistry.Count.y);
		index.z = Wrap(index.z, 0, gridRegistry.Count.z);
		return index;
	}
	private static int Wrap(int i, int inclusiveMin, int exclusiveMax)
	{
		var distance = exclusiveMax - inclusiveMin;
		var index = ((i + distance) % distance) + inclusiveMin;
		return index;
	}
	public void Clear() => gridRegistry.Clear();
	public Vector3Int GetRandomIndex()
	{
		return new Vector3Int(Random.Range(0, gridRegistry.Count.x), 
							  Random.Range(0, gridRegistry.Count.y), 
							  Random.Range(0, gridRegistry.Count.z));
	}
	
	public T Get<T>(Vector3Int index) => gridRegistry.Get<T>(index);
	public void Get<T>(Vector3Int index, out T that) => gridRegistry.Get(index, out that);
	public void Get<T0, T1, T2, T3>(Vector3Int index, out T0 first, out T1 second, out T2 third, out T3 fourth) => gridRegistry.Get(index, out first, out second, out third, out fourth);
	public void Get<T0, T1, T2>(Vector3Int index, out T0 first, out T1 second, out T2 third) => gridRegistry.Get(index, out first, out second, out third);
	public void Get<T0, T1>(Vector3Int index, out T0 first, out T1 second) => gridRegistry.Get(index, out first, out second);

	public void Set<T>(Vector3Int index, T that) => gridRegistry.Set(index, that);
	public void Set<T0, T1, T2, T3>(Vector3Int index, T0 first, T1 second, T2 third, T3 fourth) => gridRegistry.Set(index, first, second, third, fourth);
	public void Set<T0, T1, T2>(Vector3Int index, T0 first, T1 second, T2 third) => gridRegistry.Set(index, first, second, third);
	public void Set<T0, T1>(Vector3Int index, T0 first, T1 second) => gridRegistry.Set(index, first, second);

	public void Transform<T>(GridRegistry.MultiGridTransformAction<T> action) => gridRegistry.Transform(action);
	public void Transform<T>(GridRegistry.MultiGridTransformRefAction<T> action) => gridRegistry.Transform(action);
	public void Transform<T0, T1, T2, T3>(GridRegistry.MultiGridTransformAction<T0, T1, T2, T3> action) => gridRegistry.Transform(action);
	public void Transform<T0, T1, T2, T3>(GridRegistry.MultiGridTransformRefAction<T0, T1, T2, T3> action) => gridRegistry.Transform(action);
	public void Transform<T0, T1, T2>(GridRegistry.MultiGridTransformAction<T0, T1, T2> action) => gridRegistry.Transform(action);
	public void Transform<T0, T1, T2>(GridRegistry.MultiGridTransformRefAction<T0, T1, T2> action) => gridRegistry.Transform(action);
	public void Transform<T0, T1>(GridRegistry.MultiGridTransformAction<T0, T1> action) => gridRegistry.Transform(action);
	public void Transform<T0, T1>(GridRegistry.MultiGridTransformRefAction<T0, T1> action) => gridRegistry.Transform(action);
	
	public void Transform<T>(BoundsInt chunk, GridRegistry.MultiGridTransformAction<T> action) => gridRegistry.Transform(chunk, action);
	public void Transform<T>(BoundsInt chunk, GridRegistry.MultiGridTransformRefAction<T> action) => gridRegistry.Transform(chunk, action);
	public void Transform<T0, T1, T2, T3>(BoundsInt chunk, GridRegistry.MultiGridTransformAction<T0, T1, T2, T3> action) => gridRegistry.Transform(chunk, action);
	public void Transform<T0, T1, T2, T3>(BoundsInt chunk, GridRegistry.MultiGridTransformRefAction<T0, T1, T2, T3> action) => gridRegistry.Transform(chunk, action);
	public void Transform<T0, T1, T2>(BoundsInt chunk, GridRegistry.MultiGridTransformAction<T0, T1, T2> action) => gridRegistry.Transform(chunk, action);
	public void Transform<T0, T1, T2>(BoundsInt chunk, GridRegistry.MultiGridTransformRefAction<T0, T1, T2> action) => gridRegistry.Transform(chunk, action);
	public void Transform<T0, T1>(BoundsInt chunk, GridRegistry.MultiGridTransformAction<T0, T1> action) => gridRegistry.Transform(chunk, action);
	public void Transform<T0, T1>(BoundsInt chunk, GridRegistry.MultiGridTransformRefAction<T0, T1> action) => gridRegistry.Transform(chunk, action);
	
	public bool TryGet<T>(Vector3Int index, out T that) => gridRegistry.TryGet(index, out that);
	public bool TryGet<T0, T1, T2, T3>(Vector3Int index, out T0 first, out T1 second, out T2 third, out T3 fourth) => gridRegistry.TryGet(index, out first, out second, out third, out fourth);
	public bool TryGet<T0, T1, T2>(Vector3Int index, out T0 first, out T1 second, out T2 third) => gridRegistry.TryGet(index, out first, out second, out third);
	public bool TryGet<T0, T1>(Vector3Int index, out T0 first, out T1 second) => gridRegistry.TryGet(index, out first, out second);

	public bool TrySet<T>(Vector3Int index,  T that) => gridRegistry.TrySet(index,  that);
	public bool TrySet<T0, T1, T2, T3>(Vector3Int index,  T0 first,  T1 second,  T2 third,  T3 fourth) => gridRegistry.TrySet(index,  first,  second,  third,  fourth);
	public bool TrySet<T0, T1, T2>(Vector3Int index,  T0 first,  T1 second,  T2 third) => gridRegistry.TrySet(index,  first,  second,  third);
	public bool TrySet<T0, T1>(Vector3Int index,  T0 first,  T1 second) => gridRegistry.TrySet(index,  first,  second);

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying) {
			return;
		}

		Transform((Vector3Int index, Color color) =>
		{
			Vector3 from = IndexToWorld(index);
			Gizmos.color = color;
			Gizmos.DrawWireCube(from, Vector3.one);
		});
	}
}

#region WORLD_EDITOR
#if UNITY_EDITOR
public class ReflectionField<T>
{
	private BindingFlags FLAGS = BindingFlags.Public |
								 BindingFlags.Instance |
								 BindingFlags.NonPublic |
								 BindingFlags.Static;

	private object boundObject;
	private FieldInfo fieldInfo;

	public ReflectionField(object boundObject, string fieldName)
	{
		this.boundObject = boundObject;
		this.fieldInfo = boundObject.GetType().GetField(fieldName, FLAGS);
	}

	public T Value
	{
		get => (T)fieldInfo.GetValue(boundObject);
		set => fieldInfo.SetValue(boundObject, value);
	}
}

public class WorldEditorWindow : EditorWindow
{
	private const string COUNT_LABEL = "Cell Size: ";

	private static GridRegistry selectedRegistry = new GridRegistry(new Vector3Int(10, 10, 10));
	private Dictionary<GridRegistry, string> allRegistriesNameLookUp = new Dictionary<GridRegistry, string>();
	private System.Type selectedGridType = null;
	private List<GridRegistry> allRegistries = new List<GridRegistry>();

	private ReflectionField<Vector3Int> gridRegistryCount = new ReflectionField<Vector3Int>(selectedRegistry, "count");
	
	private int gridLevelZ = 0;
	private Vector2 scrollPosition = Vector2.zero;

	[MenuItem("Custon/Grid Registry")]
	public static void OpenWindw()
	{
		GetWindow<WorldEditorWindow>("Grid Registry");
	}

	public void OnEnable()
	{
		if (Application.isPlaying) {
			foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects()) {
				var worlds = go.GetComponentsInChildren<World>(true);
				foreach(var world in worlds) {
					ReflectionField<GridRegistry> gridGetter = new ReflectionField<GridRegistry>(world, "gridRegistry");
					var registry = gridGetter.Value;
					allRegistries.Add(registry);
					allRegistriesNameLookUp.Add(registry, world.name);
				}
			}
			SetGridRegistry(allRegistries[0]);
			return;
		}
		try{
			selectedRegistry.CreateGrid<Color>();
			selectedRegistry.CreateGrid<Vector3>();
			selectedRegistry.CreateGrid<float>();
		}
		catch {

		}
	}

	public void OnDisable()
	{
		if (Application.isPlaying) {
			return;
		}

		selectedRegistry.DestroyGrid<Color>();
		selectedRegistry.DestroyGrid<Vector3>();
		selectedRegistry.DestroyGrid<float>();
	}

	private void OnGUI()
	{
		gridRegistryCount.Value = CheckAndResizeGridRegistry(gridRegistryCount.Value);
		SetGridRegistry(CheckCurrentSelectedRegistry(allRegistries, allRegistriesNameLookUp, selectedRegistry));
		selectedGridType = CheckCurrentSelectedGrid(selectedGridType);
		gridLevelZ = RefreshGridLevelZ(gridLevelZ);
		scrollPosition = DrawAllGridContent(selectedGridType, position.size, scrollPosition, gridLevelZ);
	}

	private static Vector3Int CheckAndResizeGridRegistry(Vector3Int count)
	{
		Vector3Int previousCount = count;
		Vector3Int newCount = EditorGUILayout.Vector3IntField(COUNT_LABEL, previousCount);
		if (newCount != previousCount) {
			selectedRegistry.Resize(newCount);
		}
		return newCount;
	}

	private void SetGridRegistry(GridRegistry registry)
	{
		selectedRegistry = registry;
		gridRegistryCount = new ReflectionField<Vector3Int>(selectedRegistry, "count");
	}

	private static GridRegistry CheckCurrentSelectedRegistry(List<GridRegistry> registries, Dictionary<GridRegistry, string> nameLookup, GridRegistry currentRegistry)
	{
		Color color = GUI.color;

		EditorGUILayout.BeginHorizontal();
		var newSlectedType = currentRegistry;
		foreach (var registry in registries) {
			GUI.color = registry.Equals(currentRegistry) ? Color.green : Color.red;
			if (GUILayout.Button(nameLookup[registry])) {
				newSlectedType = registry;
			}
		}
		EditorGUILayout.EndHorizontal();

		GUI.color = color;
		return newSlectedType;
	}

	private static System.Type CheckCurrentSelectedGrid(System.Type selectedType)
	{
		Color color = GUI.color;

		EditorGUILayout.BeginHorizontal();
		System.Type newSlectedType = selectedType;
		selectedRegistry.DebugForEach((type) => {
			GUI.color = type.Equals(selectedType) ? Color.green : Color.red;
			if(GUILayout.Button(type.Name)) {
				newSlectedType = type;
			}
		});
		EditorGUILayout.EndHorizontal();
		
		GUI.color = color;
		return newSlectedType;
	}

	private static Vector2 DrawAllGridContent(System.Type selectedType, Vector2 windowSize, Vector2 scrollPosition, int gridLevelZ)
	{
		if (selectedType == null) {
			return scrollPosition;
		}

		var cellWidth = windowSize.x / (selectedRegistry.Count.x + 1);
		var parameters = new GUILayoutOption[] { GUILayout.Width(cellWidth), GUILayout.Height(cellWidth) };
		var style = GUI.skin.button;
		style.wordWrap = true;
		style.alignment = TextAnchor.MiddleCenter;

		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		for (int y = 0; y < selectedRegistry.Count.y; y++) {

			BoundsInt bounds = GridUtility.CreateBounds(new Vector3Int(0, y, gridLevelZ), 
														new Vector3Int(selectedRegistry.Count.x - 1, y, gridLevelZ));
			EditorGUILayout.BeginHorizontal();
			selectedRegistry.DebugTransform(bounds, selectedType, (index, obj) =>
			{
				EditorGUILayout.LabelField(GetTitleFromGridCell(index, obj), style, parameters);
			});
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
		return scrollPosition;
	}

	private static string GetTitleFromGridCell(Vector3Int index, object obj)
	{
		string title = index.ToString() + "\n";
		title += obj != null ? obj.ToString() : "null";
		return title;
	}

	private static int RefreshGridLevelZ(int gridLevelZ)
	{
		EditorGUILayout.BeginHorizontal();
		gridLevelZ = EditorGUILayout.IntSlider(gridLevelZ, 0, selectedRegistry.Count.y - 1);
		gridLevelZ = GUILayout.Button("-") ? Mathf.Max(gridLevelZ - 1, 0) : gridLevelZ;
		gridLevelZ = GUILayout.Button("+") ? Mathf.Min(gridLevelZ + 1, selectedRegistry.Count.y - 1) : gridLevelZ;
		EditorGUILayout.EndHorizontal();
		return gridLevelZ;
	}
}
#endif
#endregion