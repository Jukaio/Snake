using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Food : MonoBehaviour
{
	[SerializeField] private World world = null;
	[SerializeField] private Body body = null;

	private ObjectPool<Body> pool = null;

	public Body BodyInstance => pool.Get();

	private void Start()
	{
		pool = new ObjectPool<Body>(OnCreatePool,
									OnTakeFromPool,
									OnReturnedToPool, 
									Destroy, 
									true, 
									10, 
									world.Count.x * world.Count.y * world.Count.z);
	}
	public void Release(Body body)
	{
		pool.Release(body);
	}
	public bool Respawn(Player player)
	{
		world.Set<Food>(world.WorldToIndex(transform.position), null);

		var count = world.Count.x * world.Count.y * world.Count.z;
		for (int i = 0; i < count; i++) {
			var at = world.GetRandomIndex();
			if (IsPositionValid(at, player)) {
				world.Set(at, this);
				transform.position = world.IndexToWorld(at);
				return true;
			}
		}
		return false;
	}
	private bool IsPositionValid(Vector3Int index, Player player)
	{
		return world.Get<Body>(index) == null;
	}

	// Pool Actions
	private Body OnCreatePool()
	{
		var temp = Instantiate(body);
		temp.gameObject.SetActive(false);
		return temp;
	}
	private void OnReturnedToPool(Body body)
	{
		body.gameObject.SetActive(false);
	}
	private void OnTakeFromPool(Body body)
	{
		body.gameObject.SetActive(true);
	}

}
