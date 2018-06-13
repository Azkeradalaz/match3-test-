using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstRowChecker : MonoBehaviour {

	private void OnTriggerEnter(Collider other)
	{
		if(other.transform.tag != "level")
		{
			other.transform.SetParent(gameObject.transform);

		}
	}

	public List<GameObject> GetFirstRow()
	{
		List<GameObject> firstRow = new List<GameObject>();

		int children = transform.childCount;
		for (int i = 0; i < children; i++)
		{
			firstRow.Add(transform.GetChild(i).gameObject);
		}
		return firstRow;
	}

}
