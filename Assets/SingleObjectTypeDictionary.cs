using System.Collections.Generic;
using System.Linq;

public class SingleObjectTypeDictionary 
{
	private Dictionary<System.Type, object> data = new Dictionary<System.Type, object>();

	public int Count => data.Count;

	public delegate void AccessAction(object obj);
	public void ForEach(AccessAction action)
	{
		foreach(var item in data){
			action(item.Value);
		}
	}
	public System.Type[] GetAllTypes()
	{
		return data.Keys.ToArray();
	}
	public void Add(object obj, params object[] objects)
	{
		Add(obj.GetType(), obj);
		foreach (object o in objects) {
			Add(o.GetType(), o);
		}
	}
	public void Add<T>(T obj, params T[] objects)
	{
		Add(typeof(T), obj);
		foreach (object o in objects) {
			Add(typeof(T), o);
		}
	}
	public void Remove(System.Type type)
	{
		if (data.ContainsKey(type)) {
			data.Remove(type);
			return;
		}
	}
	public void Remove<T>()
	{
		if (data.ContainsKey(typeof(T))) {
			data.Remove(typeof(T));
			return;
		}
	}
	public bool Contains<T>()
	{
		return Contains(typeof(T));
	}
	public bool Contains(System.Type type)
	{
		return data.ContainsKey(type);
	}
	public T Get<T>()
	{
		return (T)data[typeof(T)];
	}
	public object Get(System.Type type)
	{
		return data[type];
	}
	public bool TryGet<T>(out T obj)
	{
		if (data.TryGetValue(typeof(T), out var item)) {
			obj = (T)item;
			return true;
		}
		obj = default(T);
		return false;
	}
	public bool TryGet(System.Type type, out object obj)
	{
		if (data.TryGetValue(type, out var item)) {
			obj = item;
			return true;
		}
		obj = null;
		return false;
	}
	public void Clear()
	{
		data.Clear();
	}

	private void Add(System.Type key, object value)
	{
		if (!data.ContainsKey(key)) {
			data.Add(key, value);
			return;
		}
		throw new System.AccessViolationException();
	}
}
