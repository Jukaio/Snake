using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public interface GridBase
{
	void Clear();
	bool IsInBounds(Vector3Int index);
	void Resize(Vector3Int newSize);
	void Resize(int x, int y, int z);


	public delegate void IndexAction(Vector3Int index);
	public delegate void IndexObjectAction(Vector3Int index, object obj);
	public void ForEach(IndexObjectAction action);
	public void ForEach(BoundsInt bounds, IndexObjectAction action);
	public void ForEach(IndexAction action);
	public void ForEach(BoundsInt bounds, IndexAction action);
}

public interface GridForEach<Resource>
{
	public delegate void IndexResourceRecv(Vector3Int index, Resource that);
	public delegate void IndexResourceRefRecv(Vector3Int index, ref Resource that);
	public void ForEach(IndexResourceRecv receive);
	public void ForEach(IndexResourceRefRecv receive);
}

public abstract class GenericGridEnumeratorBase<Resource> 
	:	IEnumerator<GenericGridEnumeratorBase<Resource>.IndexedResource>, 
		IEnumerable<GenericGridEnumeratorBase<Resource>.IndexedResource>
{
	public struct IndexedResource
	{
		public Vector3Int index;
		public Resource resource;

		public IndexedResource(Vector3Int index, Resource resource)
		{
			this.index = index;
			this.resource = resource;
		}
	}

	private Vector3Int index = new Vector3Int(-1, 0, 0);

	public abstract Vector3Int Size { get; }
	public abstract Resource Get(Vector3Int index);
	public IndexedResource Current => new IndexedResource(index, Get(index));
	object IEnumerator.Current => new IndexedResource(index, Get(index));

	public IEnumerator<IndexedResource> GetEnumerator()
	{
		return this;
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this;
	}
	public bool MoveNext()
	{
		index.x += 1;
		if (!(index.x < Size.x)) {
			index.x = 0;
			index.y += 1;

			if (!(index.y < Size.y)) {
				index.y = 0;
				index.z += 1;

				if (!(index.z < Size.z)) {
					index.z = 0;
					return false;
				}
			}
		}
		return true;
	}
	public void Reset()
	{
		index = new Vector3Int(-1, 0, 0);
	}
	public void Dispose()
	{
		index = new Vector3Int(-1, 0, 0);
	}
}
