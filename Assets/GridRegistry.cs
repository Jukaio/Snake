using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class GridRegistry : MultiTransformable
{
	[SerializeField] private Vector3Int count;
	
	private SingleObjectTypeDictionary grids = new SingleObjectTypeDictionary();

	public Vector3Int Count => count;

	public event System.Action OnResize;

	public GridRegistry(Vector3Int count)
	{
		this.count = count;
	}

	public void CreateGrid<T>()
	{
		if (grids.Contains(typeof(T))) {
			throw new System.InvalidOperationException("Grid of type " + typeof(T).ToString() + " already created");
		}
		grids.Add(new GenericGrid<T>(count));
	}
	public void CreateGrid(System.Type type)
	{
		if (grids.Contains(type)) {
			throw new System.InvalidOperationException("Grid of type " + type.ToString() + " already created");
		}
		var gridType = typeof(GenericGrid<>).MakeGenericType(type);
		grids.Add(System.Activator.CreateInstance(gridType, count));
	}

	public bool IsInBounds(Vector3Int index)
	{
		return index.x >= 0 && index.x < Count.x &&
			   index.y >= 0 && index.y < Count.y &&
			   index.z >= 0 && index.z < Count.z;
	}
	public void Resize(Vector3Int index)
	{
		Resize(index.x, index.y, index.z);
	}
	public void Resize(int x, int y, int z)
	{
		count = new Vector3Int(x, y, z);
		grids.ForEach((object obj) =>
		{
			var grid = obj as GridBase;
			grid.Resize(x, y, z);
		});
		OnResize.Invoke();
	}
	public void DestroyGrid<T>()
	{
		grids.Remove(typeof(T));
	}
	public void DestroyGrid(System.Type type)
	{
		if (!grids.Contains(type)) {
			throw new System.InvalidOperationException("Grid of type " + type.ToString() + " does not exist");
		}
		grids.Remove(type);
	}
	public void Clear()
	{
		grids.Clear();
	}

	public void Set<T>(Vector3Int index, T that)
	{
		grids.Get<GenericGrid<T>>().Set(index, that);
	}
	public void Set<T0, T1>(Vector3Int index, T0 first, T1 second)
	{
		Set(index, first);
		Set(index, second);
	}
	public void Set<T0, T1, T2>(Vector3Int index, T0 first, T1 second, T2 third)
	{
		Set(index, first, second);
		Set(index, third);
	}
	public void Set<T0, T1, T2, T3>(Vector3Int index, T0 first, T1 second, T2 third, T3 fourth)
	{
		Set(index, first, second);
		Set(index, third, fourth);
	}

	public T Get<T>(Vector3Int index)
	{
		return grids.Get<GenericGrid<T>>().Get(index);
	}
	public void Get<T>(Vector3Int index, out T that)
	{
		var grid = grids.Get<GenericGrid<T>>();
		that = grid.Get(index);
	}
	public void Get<T0, T1>(Vector3Int index, out T0 first, out T1 second)
	{
		Get(index, out first);
		Get(index, out second);
	}
	public void Get<T0, T1, T2>(Vector3Int index, out T0 first, out T1 second, out T2 third)
	{
		Get(index, out first, out second);
		Get(index, out third);
	}
	public void Get<T0, T1, T2, T3>(Vector3Int index, out T0 first, out T1 second, out T2 third, out T3 fourth)
	{
		Get(index, out first, out second);
		Get(index, out third, out fourth);
	}

	public bool TryGet<T>(Vector3Int index, out T that)
	{
		if (grids.TryGet<GenericGrid<T>>(out var data)) {
			if (data.TryGet(index, out var resource)) {
				that = resource;
				return true;
			}
		}
		that = default(T);
		return false;
	}
	public bool TryGet<T0, T1>(Vector3Int index, out T0 first, out T1 second)
	{
		if (!TryGet(index, out first)) {
			second = default(T1);
			return false;
		}
		return TryGet(index, out second);
	}
	public bool TryGet<T0, T1, T2>(Vector3Int index, out T0 first, out T1 second, out T2 third)
	{
		if (!TryGet(index, out first, out second)) {
			third = default(T2);
			return false;
		}
		return TryGet(index, out third);
	}
	public bool TryGet<T0, T1, T2, T3>(Vector3Int index, out T0 first, out T1 second, out T2 third, out T3 fourth)
	{
		if (!TryGet(index, out first, out second, out third)) {
			fourth = default(T3);
			return false;
		}
		return TryGet(index, out fourth);
	}

	public bool TrySet<T>(Vector3Int index, T that)
	{
		if (grids.TryGet<GenericGrid<T>>(out var data)) {
			return data.TrySet(index, that);
		}
		return false;
	}
	public bool TrySet<T0, T1>(Vector3Int index, T0 first, T1 second)
	{
		if (!TrySet(index, first)) {
			return false;
		}
		return TrySet(index, second);
	}
	public bool TrySet<T0, T1, T2>(Vector3Int index, T0 first, T1 second, T2 third)
	{
		if (!TrySet(index, first, second)) {
			return false;
		}
		return TrySet(index, third);
	}
	public bool TrySet<T0, T1, T2, T3>(Vector3Int index, T0 first, T1 second, T2 third, T3 fourth)
	{
		if (!TrySet(index, first, second, third)) {
			return false;
		}
		return TrySet(index, fourth);
	}


	public delegate void MultiGridTransformAction<T0, T1, T2, T3>(Vector3Int index, T0 first, T1 second, T2 third, T3 fourth);
	public delegate void MultiGridTransformRefAction<T0, T1, T2, T3>(Vector3Int index, ref T0 first, ref T1 second, ref T2 third, ref T3 fourth);
	public void Transform<T0, T1, T2, T3>(MultiGridTransformAction<T0, T1, T2, T3> action)
	{
		MultiGridTransformAction<T0, T1, T2> innerAction = (Vector3Int index, T0 t0, T1 t1, T2 t2) =>
		{
			if (grids.TryGet<GenericGrid<T3>>(out var fourth)) {
				if (fourth.TryGet(index, out var fourthResource)) {
					action(index, t0, t1, t2, fourthResource);
				}
				else {
					throw new System.InvalidOperationException();
				}
			}
		};
		Transform(innerAction);
	}
	public void Transform<T0, T1, T2, T3>(MultiGridTransformRefAction<T0, T1, T2, T3> action)
	{
		MultiGridTransformRefAction<T0, T1, T2> innerAction = (Vector3Int index, ref T0 t0, ref T1 t1, ref T2 t2) =>
		{
			if (grids.TryGet<GenericGrid<T3>>(out var fourth)) {
				if (fourth.TryGet(index, out var fourthResource)) {
					action(index, ref t0, ref t1, ref t2, ref fourthResource);
					fourth.Set(index, fourthResource);
				}
				else {
					throw new System.InvalidOperationException();
				}
			}
		};
		Transform(innerAction);
	}
	public void Transform<T0, T1, T2, T3>(BoundsInt chunk, MultiGridTransformAction<T0, T1, T2, T3> action)
	{
		MultiGridTransformAction<T0, T1, T2> innerAction = (Vector3Int index, T0 t0, T1 t1, T2 t2) =>
		{
			if (grids.TryGet<GenericGrid<T3>>(out var fourth)) {
				if (fourth.TryGet(index, out var fourthResource)) {
					action(index, t0, t1, t2, fourthResource);
				}
				else {
					throw new System.InvalidOperationException();
				}
			}
		};
		Transform(chunk, innerAction);
	}
	public void Transform<T0, T1, T2, T3>(BoundsInt chunk, MultiGridTransformRefAction<T0, T1, T2, T3> action)
	{
		MultiGridTransformRefAction<T0, T1, T2> innerAction = (Vector3Int index, ref T0 t0, ref T1 t1, ref T2 t2) =>
		{
			if (grids.TryGet<GenericGrid<T3>>(out var fourth)) {
				if (fourth.TryGet(index, out var fourthResource)) {
					action(index, ref t0, ref t1, ref t2, ref fourthResource);
					fourth.Set(index, fourthResource);
				}
				else {
					throw new System.InvalidOperationException();
				}
			}
		};
		Transform(chunk, innerAction);
	}

	public delegate void MultiGridTransformAction<T0, T1, T2>(Vector3Int index, T0 first, T1 second, T2 third);
	public delegate void MultiGridTransformRefAction<T0, T1, T2>(Vector3Int index, ref T0 first, ref T1 second, ref T2 third);
	public void Transform<T0, T1, T2>(MultiGridTransformAction<T0, T1, T2> action)
	{
		MultiGridTransformAction<T0, T1> innerAction = (Vector3Int index, T0 t0, T1 t1) =>
		{
			if (grids.TryGet<GenericGrid<T2>>(out var third)) {
				if (third.TryGet(index, out var thirdResource)) {
					action(index, t0, t1, thirdResource);
				}
				else {
					throw new System.InvalidOperationException();
				}
			}
		};
		Transform(innerAction);
	}
	public void Transform<T0, T1, T2>(MultiGridTransformRefAction<T0, T1, T2> action)
	{
		MultiGridTransformRefAction<T0, T1> innerAction = (Vector3Int index, ref T0 t0, ref T1 t1) =>
		{
			if (grids.TryGet<GenericGrid<T2>>(out var third)) {
				if (third.TryGet(index, out var thirdResource)) {
					action(index, ref t0, ref t1, ref thirdResource);
					third.Set(index, thirdResource);
				}
				else {
					throw new System.InvalidOperationException();
				}
			}
		};
		Transform(innerAction);
	}
	public void Transform<T0, T1, T2>(BoundsInt chunk, MultiGridTransformAction<T0, T1, T2> action)
	{
		MultiGridTransformAction<T0, T1> innerAction = (Vector3Int index, T0 t0, T1 t1) =>
		{
			if (grids.TryGet<GenericGrid<T2>>(out var third)) {
				if (third.TryGet(index, out var thirdResource)) {
					action(index, t0, t1, thirdResource);
				}
				else {
					throw new System.InvalidOperationException();
				}
			}
		};
		Transform(chunk, innerAction);
	}
	public void Transform<T0, T1, T2>(BoundsInt chunk, MultiGridTransformRefAction<T0, T1, T2> action)
	{
		MultiGridTransformRefAction<T0, T1> innerAction = (Vector3Int index, ref T0 t0, ref T1 t1) =>
		{
			if (grids.TryGet<GenericGrid<T2>>(out var third)) {
				if (third.TryGet(index, out var thirdResource)) {
					action(index, ref t0, ref t1, ref thirdResource);
					third.Set(index, thirdResource);
				}
				else {
					throw new System.InvalidOperationException();
				}
			}
		};
		Transform(chunk, innerAction);
	}

	public delegate void MultiGridTransformAction<T0, T1>(Vector3Int index, T0 first, T1 second);
	public delegate void MultiGridTransformRefAction<T0, T1>(Vector3Int index, ref T0 first, ref T1 second);
	public void Transform<T0, T1>(MultiGridTransformAction<T0, T1> action)
	{
		MultiGridTransformAction<T0> innerAction = (Vector3Int index, T0 t0) =>
		{
			if (grids.TryGet<GenericGrid<T1>>(out var second)) {
				if (second.TryGet(index, out var secondResource)) {
					action(index, t0, secondResource);
				}
				else {
					throw new System.InvalidOperationException();
				}
			}
		};
		Transform(innerAction);
	}
	public void Transform<T0, T1>(MultiGridTransformRefAction<T0, T1> action)
	{
		MultiGridTransformRefAction<T0> innerAction = (Vector3Int index, ref T0 t0) =>
		{
			if (grids.TryGet<GenericGrid<T1>>(out var second)) {
				if (second.TryGet(index, out var secondResource)) {
					action(index, ref t0, ref secondResource);
					second.Set(index, secondResource);
				}
				else {
					throw new System.InvalidOperationException();
				}
			}
		};
		Transform(innerAction);
	}
	public void Transform<T0, T1>(BoundsInt chunk, MultiGridTransformAction<T0, T1> action)
	{
		MultiGridTransformAction<T0> innerAction = (Vector3Int index, T0 t0) =>
		{
			if (grids.TryGet<GenericGrid<T1>>(out var second)) {
				if (second.TryGet(index, out var secondResource)) {
					action(index, t0, secondResource);
				}
				else {
					throw new System.InvalidOperationException();
				}
			}
		};
		Transform(chunk, innerAction);
	}
	public void Transform<T0, T1>(BoundsInt chunk, MultiGridTransformRefAction<T0, T1> action)
	{
		MultiGridTransformRefAction<T0> innerAction = (Vector3Int index, ref T0 t0) =>
		{
			if (grids.TryGet<GenericGrid<T1>>(out var second)) {
				if (second.TryGet(index, out var secondResource)) {
					action(index, ref t0, ref secondResource);
					second.Set(index, secondResource);
				}
				else {
					throw new System.InvalidOperationException();
				}
			}
		};
		Transform(chunk, innerAction);
	}

	public delegate void MultiGridTransformAction<T0>(Vector3Int index, T0 resource);
	public delegate void MultiGridTransformRefAction<T0>(Vector3Int index, ref T0 resource);
	public void Transform<T>(MultiGridTransformAction<T> action)
	{
		if (grids.TryGet<GenericGrid<T>>(out var grid)) {
			grid.ForEach((index, resource) =>
			{
				action(index, resource);
			});
		}
	}
	public void Transform<T>(MultiGridTransformRefAction<T> action)
	{
		if (grids.TryGet<GenericGrid<T>>(out var grid)) {
			grid.ForEach((Vector3Int index, ref T resource) =>
			{
				action(index, ref resource);
				grid.Set(index, resource);
			});
		}
	}
	public void Transform<T>(BoundsInt chunk, MultiGridTransformAction<T> action)
	{
		if (grids.TryGet<GenericGrid<T>>(out var grid)) {
			grid.ForEach(chunk, (index, resource) =>
			{
				action(index, resource);
			});
		}
	}
	public void Transform<T>(BoundsInt chunk, MultiGridTransformRefAction<T> action)
	{
		if (grids.TryGet<GenericGrid<T>>(out var grid)) {
			grid.ForEach(chunk, (Vector3Int index, ref T resource) =>
			{
				action(index, ref resource);
				grid.Set(index, resource);
			});
		}
	}

	public delegate void MultiGridTransformAction(Vector3Int index);
	public void Transform(MultiGridTransformAction action)
	{
		for (int z = 0; z < Count.z; z++) {
			for (int y = 0; y < Count.y; y++) {
				for (int x = 0; x < Count.x; x++) {
					action(new Vector3Int(x, y, z));
				}
			}
		};
	}
	public void Transform(BoundsInt chunk, MultiGridTransformAction action)
	{
		var min = chunk.min;
		var max = chunk.max;

		for (int z = min.z; z <= max.z; z++) {
			for (int y = min.y; y <= max.y; y++) {
				for (int x = min.x; x <= max.x; x++) {
					action(new Vector3Int(x, y, z));
				}
			}
		}
	}

	public void DebugForEach(System.Action<System.Type> action)
	{
		foreach (var type in grids.GetAllTypes()) {
			action(type.GenericTypeArguments[0]);
		}
	}
	public void DebugTransform(System.Type type, GridBase.IndexObjectAction action)
	{
		var gridType = typeof(GenericGrid<>).MakeGenericType(type);
		if (grids.TryGet(gridType, out var grid)) 
		{
			var baseGrid = grid as GridBase;
			baseGrid.ForEach(action);
		}
	}
	public void DebugTransform(BoundsInt chunk, System.Type type, GridBase.IndexObjectAction action)
	{
		var gridType = typeof(GenericGrid<>).MakeGenericType(type);
		if (grids.TryGet(gridType, out var grid)) {
			var baseGrid = grid as GridBase;
			baseGrid.ForEach(chunk, action);
		}
	}
}
