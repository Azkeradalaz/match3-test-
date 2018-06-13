using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemDestroyManager : MonoBehaviour {

	[SerializeField]
	private LayerMask gemLayer;

	private List<GameObject> gemsToCheck = new List<GameObject>();

	private void Update()
	{
		CheckGemsArray();
	}

	private void CheckGemsArray() //проверка листа гемов на 3-в-ряд
	{
		if (gemsToCheck.Count > 0)
		{
			foreach (GameObject gem in gemsToCheck)
			{
				if ((gem == null) || gem.GetComponent<GemController>().CheckForDeath())
				{

				}
				else
				{
					gem.GetComponent<GemController>().SetToDestroy(true);
					List<GameObject> combinedMatches = GetCombinedMatches(gem);
					if (combinedMatches.Count >= 2)
					{
						foreach (GameObject toDestroy in combinedMatches)
						{
							toDestroy.GetComponent<GemController>().Die();
						}
						gem.GetComponent<GemController>().Die();
					}
					else gem.GetComponent<GemController>().SetToDestroy(false);

				}

			}
			gemsToCheck.Clear();
		}
	}

	private List<GameObject> GetCombinedMatches(GameObject obj)
	{
		List<GameObject> upMatches = new List<GameObject>();
		upMatches = CheckBlocks(obj.transform, Vector3.up);

		List<GameObject> downMatches = new List<GameObject>();
		downMatches = CheckBlocks(obj.transform, Vector3.down);

		List<GameObject> leftMatches = new List<GameObject>();
		leftMatches = CheckBlocks(obj.transform, Vector3.left);

		List<GameObject> rightMatches = new List<GameObject>();
		rightMatches = CheckBlocks(obj.transform, Vector3.right);

		List<GameObject> combinedMatches = new List<GameObject>();
		Debug.Log("Start finding matches");
		Debug.Log("Up " + upMatches.Count);
		Debug.Log("Down " + downMatches.Count);
		Debug.Log("Left " + leftMatches.Count);
		Debug.Log("Right " + rightMatches.Count);


		if (upMatches.Count + downMatches.Count >= 2)
		{
			foreach (GameObject match in upMatches)
			{
				if (!match.GetComponent<GemController>().CheckToBeDestroyed())
				{
					combinedMatches.Add(match);
					match.GetComponent<GemController>().SetToDestroy(true);
				}
			}
			foreach (GameObject match in downMatches)
			{
				if (!match.GetComponent<GemController>().CheckToBeDestroyed())
				{
					combinedMatches.Add(match);
					match.GetComponent<GemController>().SetToDestroy(true);
				}
			}
		}
		if (leftMatches.Count + rightMatches.Count >= 2)
		{
			foreach (GameObject match in leftMatches)
			{
				if (!match.GetComponent<GemController>().CheckToBeDestroyed())
				{
					combinedMatches.Add(match);
					match.GetComponent<GemController>().SetToDestroy(true);
				}
			}
			foreach (GameObject match in rightMatches)
			{
				if (!match.GetComponent<GemController>().CheckToBeDestroyed())
				{
					combinedMatches.Add(match);
					match.GetComponent<GemController>().SetToDestroy(true);
				}
			}
		}
		Debug.Log(combinedMatches.Count + " of combined matches");

		foreach (GameObject combObj in combinedMatches)
		{
			List<GameObject> combObjList = GetCombinedMatches(combObj);
			if (combObjList.Count >= 2)
			{
				foreach (GameObject tmpObj in combObjList)
				{
					combinedMatches.Add(tmpObj);
				}
			}

		}
		return combinedMatches;
	}

	private List<GameObject> CheckBlocks(Transform transform, Vector3 direction)
	{
		List<GameObject> list = new List<GameObject>();
		Vector3 offset = new Vector3(0, -0.3f, 0);
		RaycastHit hit;
		Physics.Raycast(transform.position + offset, direction, out hit, 0.6f, gemLayer);
		while (hit.collider != null && hit.collider.tag == transform.gameObject.tag && hit.collider.transform.GetComponent<Rigidbody>().velocity.y >= -0.05)
		{
			list.Add(hit.collider.gameObject);
			Debug.Log(hit.collider.tag + " added");
			Physics.Raycast(hit.transform.position, direction, out hit, 0.6f, gemLayer);

		}

		return list;

	}


	public void AddGemToCheck(GameObject newGem)
	{
		gemsToCheck.Add(newGem);
	}
}
