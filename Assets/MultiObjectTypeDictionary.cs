using System.Collections.Generic;

// TODO: Improve to make it work with GridRegistry better
public class MultiObjectTypeDictionary
{
	private Dictionary<System.Type, List<object>> data = new Dictionary<System.Type, List<object>>();

	public void Add(object obj, params object[] objects) {
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

	public T Get<T>() 
	{
		if(data.TryGetValue(typeof(T), out var list)) {
			return (T)list[0];
		}
		throw new System.Exception();
	}
	public object Get(System.Type type)
	{
		if (data.TryGetValue(type, out var list)) {
			return list[0];
		}
		return null;
	}

	public T[] GetAll<T>()
	{
		if (data.TryGetValue(typeof(T), out var list)) {
			return DataToArray<T>(list);
		}
		return null;
	}
	public object[] GetAll(System.Type type)
	{
		if (data.TryGetValue(type, out var list)) {
			return list.ToArray();
		}
		return null;
	}

	public bool TryGet<T>(out T obj)
	{
		if (data.TryGetValue(typeof(T), out var list)) {
			obj = (T)list[0];
			return true;
		}
		throw new System.Exception();
	}
	public bool TryGet(System.Type type, out object obj) {
		if(data.TryGetValue(type, out var list)) {
			obj = list[0];
			return true;
		}
		obj = null;
		return false;
	}

	public bool TryGetAll<T>(out T[] obj)
	{
		if (data.TryGetValue(typeof(T), out var list)) {
			obj = DataToArray<T>(list);
			return true;
		}
		obj = null;
		return false;
	}
	public bool TryGetAll(System.Type type, out object[] obj) 
	{
		if (data.TryGetValue(type, out var list)) {
			obj = list.ToArray();
			return true;
		}
		obj = null;
		return false;
	}

	private T[] DataToArray<T>(List<object> that)
	{
		if(that == null) {
			return new T[0];
		}

		T[] arr = new T[that.Count];
		for (int i = 0; i < that.Count; i++) {
			arr[i] = (T)that[i];
		}
		return arr;
	}

	private void Add(System.Type key, object value) {
		if (this.data.TryGetValue(key, out var outList)) {
			outList.Add(value);
		}
		else {
			var list = new List<object>();
			data.Add(key, list);
			list.Add(value);
		}
	}

	public void Remove(System.Type type)
	{
		if (data.TryGetValue(type, out var list)) {
			list.RemoveAt(list.Count - 1);
			if (list.Count == 0) {
				data.Remove(type);
			}
		}
		return;
	}

	public void Remove<T>()
	{
		if (data.TryGetValue(typeof(T), out var list)) {
			list.RemoveAt(list.Count - 1);
			if(list.Count == 0) {
				data.Remove(typeof(T));
			}
		}
		return;
	}
	public void RemoveAll<T>()
	{
		if (data.TryGetValue(typeof(T), out var list)) {
			list.Clear();
			data.Remove(typeof(T));
		}
		return;
	}
	public void RemoveAll(System.Type type)
	{
		if (data.TryGetValue(type, out var list)) {
			list.Clear();
			data.Remove(type);
		}
		return;
	}

	public void Clear()
	{
		foreach(var kv in data) {
			kv.Value.Clear();
		}
		data.Clear();
	}
}
