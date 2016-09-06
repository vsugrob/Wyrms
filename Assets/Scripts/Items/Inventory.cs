using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory : ScriptableObject {
	public List <Item> Items = new List <Item> ();

	[System.Serializable]
	public class Item {
		public float Quantity = 1;
		public GameObject Prefab;
	}
}
