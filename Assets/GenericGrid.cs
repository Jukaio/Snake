using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

public static class GridUtility
{
	public static bool IsInBounds(Vector3Int index, Vector3Int count)
	{
		return index.x >= 0 && index.x < count.x &&
			   index.y >= 0 && index.y < count.y &&
			   index.z >= 0 && index.z < count.z;
	}

	public static BoundsInt CreateBounds(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
	{
		return CreateBounds(new Vector3Int(minX, minY, minZ), new Vector3Int(maxX, maxY, maxZ));
	}

	public static BoundsInt CreateBounds(Vector3Int min, Vector3Int max)
	{
		var bounds = new BoundsInt();
		bounds.min = min;
		bounds.max = max;
		return bounds;
	}

	public static BoundsInt CreateBounds(Vector3Int center, Vector3Int dimensions, Vector3Int count)
	{
		var min = new Vector3Int(center.x - dimensions.x > 0 ? center.x - dimensions.x : 0,
								 center.y - dimensions.y > 0 ? center.y - dimensions.y : 0,
								 center.z - dimensions.z > 0 ? center.z - dimensions.z : 0);

		var max = new Vector3Int(center.x + dimensions.x < count.x - 1 ? center.x + dimensions.x : count.x - 1,
								 center.y + dimensions.y < count.y - 1 ? center.y + dimensions.y : count.y - 1,
								 center.z + dimensions.z < count.z - 1 ? center.z + dimensions.z : count.z - 1);
		var bounds = new BoundsInt();
		bounds.min = min;
		bounds.max = max;
		return bounds;
	}

	public static bool Create(Vector3Int center, Vector3Int dimensions, Vector3Int count, out BoundsInt chunk)
	{
		if (!IsInBounds(center, count)) {
			chunk = new BoundsInt(Vector3Int.zero, Vector3Int.zero);
			return false;
		}

		chunk = CreateBounds(center, dimensions, count);
		return true;
	}
}

public class GenericGrid<Resource> : GenericGridEnumeratorBase<Resource>, GridBase, GridForEach<Resource>
{
	private struct Cell
	{
		private Resource resource;
		public Resource Resource => resource;

		public Cell(Resource resource)
		{
			this.resource = resource;
		}

		public void Set(Resource resource)
		{
			this.resource = resource;
		}
	}

	private Cell[,,] cells = null;
	
	public override Vector3Int Size => new Vector3Int(cells.GetLength(0), cells.GetLength(1), cells.GetLength(2));

	public GenericGrid(Vector3Int size)
	{
		cells = new Cell[size.x, size.y, size.z];
		for(int z = 0; z < size.z; z++) {
			for (int y = 0; y < size.y; y++) {
				for (int x = 0; x < size.x; x++) {
					cells[x, y, z] = new Cell();
				}
			}
		}
	}
	public Resource this[int x, int y, int z]
	{
		get => Get(new Vector3Int(x, y, z));
		set => Set(new Vector3Int(x, y, z), value);
	}
	public Resource this[Vector3Int index]
	{
		get => Get(index);
		set => Set(index, value);
	}

	public bool IsInBounds(Vector3Int index)
	{
		return index.x >= 0 && index.x < cells.GetLength(0) &&
			   index.y >= 0 && index.y < cells.GetLength(1) &&
			   index.z >= 0 && index.z < cells.GetLength(2);
	}
	public override Resource Get(Vector3Int index) 
	{
		return cells[index.x, index.y, index.z].Resource;
	}
	public bool TryGet(Vector3Int index, out Resource resource)
	{
		if (IsInBounds(index)) {
			resource = Get(index);
			return true;
		}
		resource = default(Resource);
		return false;
	}
	public virtual void Set(Vector3Int index, Resource data)
	{
		cells[index.x, index.y, index.z] = new Cell(data);
	}
	public bool TrySet(Vector3Int index, Resource resource)
	{
		if (IsInBounds(index)) {
			Set(index, resource);
			return true;
		}
		return false;
	}
	public void ForEach(GridForEach<Resource>.IndexResourceRefRecv receive)
	{
		for (int z = 0; z < Size.z; z++) {
			for (int y = 0; y < Size.y; y++) {
				for (int x = 0; x < Size.x; x++) {
					var index = new Vector3Int(x, y, z);
					var resource = Get(index);
					receive(index, ref resource);
					Set(index, resource);
				}
			}
		}
	}
	public void ForEach(GridForEach<Resource>.IndexResourceRecv receive)
	{
		for (int z = 0; z < Size.z; z++) {
			for (int y = 0; y < Size.y; y++) {
				for (int x = 0; x < Size.x; x++) {
					receive(new Vector3Int(x, y, z), Get(new Vector3Int(x, y, z)));
				}
			}
		}
	}
	public void ForEach(GridBase.IndexObjectAction receive) 
	{
		for (int z = 0; z < Size.z; z++) {
			for (int y = 0; y < Size.y; y++) {
				for (int x = 0; x < Size.x; x++) {
					receive(new Vector3Int(x, y, z), Get(new Vector3Int(x, y, z)));
				}
			}
		}
	}
	public void ForEach(BoundsInt bounds, GridBase.IndexObjectAction receive)
	{
		var min = bounds.min;
		var max = bounds.max;

		for (int z = min.z; z <= max.z; z++) {
			for (int y = min.y; y <= max.y; y++) {
				for (int x = min.x; x <= max.x; x++) {
					receive(new Vector3Int(x, y, z), Get(new Vector3Int(x, y, z)));
				}
			}
		}
	}
	public void ForEach(BoundsInt bounds, GridForEach<Resource>.IndexResourceRecv receive)
	{
		var min = bounds.min;
		var max = bounds.max;

		for (int z = min.z; z <= max.z; z++) {
			for (int y = min.y; y <= max.y; y++) {
				for (int x = min.x; x <= max.x; x++) {
					var neighbour = new Vector3Int(x, y, z);
					receive(neighbour, Get(neighbour));
				}
			}
		}
	}
	public void ForEach(BoundsInt bounds, GridForEach<Resource>.IndexResourceRefRecv receive)
	{
		var min = bounds.min;
		var max = bounds.max;

		for (int z = min.z; z <= max.z; z++) {
			for (int y = min.y; y <= max.y; y++) {
				for (int x = min.x; x <= max.x; x++) {
					var neighbour = new Vector3Int(x, y, z);
					var resource = Get(neighbour);
					receive(neighbour, ref resource);
					Set(neighbour, resource);
				}
			}
		}

	}
	public void ForEach(GridBase.IndexAction action)
	{
		for (int z = 0; z < Size.z; z++) {
			for (int y = 0; y < Size.y; y++) {
				for (int x = 0; x < Size.x; x++) {
					action(new Vector3Int(x, y, z));
				}
			}
		}
	}
	public void ForEach(BoundsInt bounds, GridBase.IndexAction action)
	{
		var min = bounds.min;
		var max = bounds.max;

		for (int z = min.z; z <= max.z; z++) {
			for (int y = min.y; y <= max.y; y++) {
				for (int x = min.x; x <= max.x; x++) {
					action(new Vector3Int(x, y, z));
				}
			}
		}
	}
	public void Resize(int newSizeX, int newSizeY, int newSizeZ)
	{
		var temp = new Cell[newSizeX, newSizeY, newSizeZ];
		bool IsInBound(Cell[,,] matrix, Vector3Int index)
		{
			return index.x >= 0 && index.x < matrix.GetLength(0) &&
				   index.y >= 0 && index.y < matrix.GetLength(1) &&
				   index.z >= 0 && index.z < matrix.GetLength(2);
		}

		for (int z = 0; z < newSizeZ; z++) {
			for (int y = 0; y < newSizeY; y++) {
				for (int x = 0; x < newSizeX; x++) {
					if (IsInBound(temp, new Vector3Int(x, y, z)) &&
						IsInBound(cells, new Vector3Int(x, y, z))) {
						temp[x, y, z] = cells[x, y, z];
					}
					else {
						temp[x, y, z] = new Cell();
					}
				}
			}
		}
		cells = temp;
	}
	public void Resize(Vector3Int newSize)
	{
		Resize(newSize.x, newSize.y, newSize.z);
	}
	public void Clear()
	{
		for (int z = 0; z < Size.z; z++) {
			for (int y = 0; y < Size.y; y++) {
				for (int x = 0; x < Size.x; x++) {
					cells[x, y, z] = new Cell();
				}
			}
		}
	}

}
