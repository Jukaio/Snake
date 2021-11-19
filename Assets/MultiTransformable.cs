using UnityEngine;

public interface MultiTransformable
{
	void Clear();
	void Set<T>(Vector3Int index, T that);
	void Set<T0, T1, T2, T3>(Vector3Int index, T0 first, T1 second, T2 third, T3 fourth);
	void Set<T0, T1, T2>(Vector3Int index, T0 first, T1 second, T2 third);
	void Set<T0, T1>(Vector3Int index, T0 first, T1 second);
	void Transform<T>(BoundsInt chunk, GridRegistry.MultiGridTransformAction<T> action);
	void Transform<T>(BoundsInt chunk, GridRegistry.MultiGridTransformRefAction<T> action);
	void Transform<T>(GridRegistry.MultiGridTransformAction<T> action);
	void Transform<T>(GridRegistry.MultiGridTransformRefAction<T> action);
	void Transform<T0, T1, T2, T3>(BoundsInt chunk, GridRegistry.MultiGridTransformAction<T0, T1, T2, T3> action);
	void Transform<T0, T1, T2, T3>(BoundsInt chunk, GridRegistry.MultiGridTransformRefAction<T0, T1, T2, T3> action);
	void Transform<T0, T1, T2, T3>(GridRegistry.MultiGridTransformAction<T0, T1, T2, T3> action);
	void Transform<T0, T1, T2, T3>(GridRegistry.MultiGridTransformRefAction<T0, T1, T2, T3> action);
	void Transform<T0, T1, T2>(BoundsInt chunk, GridRegistry.MultiGridTransformAction<T0, T1, T2> action);
	void Transform<T0, T1, T2>(BoundsInt chunk, GridRegistry.MultiGridTransformRefAction<T0, T1, T2> action);
	void Transform<T0, T1, T2>(GridRegistry.MultiGridTransformAction<T0, T1, T2> action);
	void Transform<T0, T1, T2>(GridRegistry.MultiGridTransformRefAction<T0, T1, T2> action);
	void Transform<T0, T1>(BoundsInt chunk, GridRegistry.MultiGridTransformAction<T0, T1> action);
	void Transform<T0, T1>(BoundsInt chunk, GridRegistry.MultiGridTransformRefAction<T0, T1> action);
	void Transform<T0, T1>(GridRegistry.MultiGridTransformAction<T0, T1> action);
	void Transform<T0, T1>(GridRegistry.MultiGridTransformRefAction<T0, T1> action);
	bool TryGet<T>(Vector3Int index, out T that);
	bool TryGet<T0, T1, T2, T3>(Vector3Int index, out T0 first, out T1 second, out T2 third, out T3 fourth);
	bool TryGet<T0, T1, T2>(Vector3Int index, out T0 first, out T1 second, out T2 third);
	bool TryGet<T0, T1>(Vector3Int index, out T0 first, out T1 second);
}