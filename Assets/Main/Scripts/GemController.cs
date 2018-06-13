using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GemController : MonoBehaviour {
	
	
	//private bool isGrounded = false;
	private bool toBeDestroyed = false;
	private bool hasBonus=false;
	private bool dying = false;
	private bool mustCheckBlocks =true;
	private bool isControlled = false;
	//private bool isInFirstRow = false;

	private Transform toRotate1, toRotate2;
	[SerializeField]
	private LayerMask layer;
	[SerializeField]
	private LayerMask gemLayer;

	private Rigidbody myRB;

	private GemDestroyManager gemDestroyManager;

	private Renderer rend1;
	private Renderer rend2;

	[SerializeField]
	private Material dissolveMaterial;

	[SerializeField]
	private float speed = 100f;
	private float disolve=-0.5f;
	private float randNoise1;
	private float randNoise2;

	[SerializeField]
	private AudioClip firstRowDestroySound;
	[SerializeField]
	private AudioClip gemDestroySound;

	
	
	// Use this for initialization
	void Start () {
		rend1 = gameObject.transform.GetChild(0).GetComponent<Renderer>();
		rend2 = gameObject.transform.GetChild(1).GetComponent<Renderer>();
		myRB = gameObject.GetComponent<Rigidbody>();
		toRotate1 = transform.GetChild(0);
		toRotate2 = transform.GetChild(1);
		randNoise1 = Random.Range(15f, 80f);
		randNoise2 = Random.Range(15f, 80f);
		gemDestroyManager = GameObject.Find("GameMaster").GetComponent<GemDestroyManager>();
	}
	
	// Update is called once per frame
	void Update () {

		CheckForBlocks();
		
		if (isControlled)
		{
			RotateGems();
			CheckForLayerBelow();
		}
	}
	private void FixedUpdate()
	{
		if (dying)//когда активно - "поглощает" модели
		{
			disolve += Time.deltaTime*5;
			rend1.material.SetFloat("Vector1_7B2B0159", disolve);
			rend1.material.SetFloat("Vector1_F7E627EB", randNoise1);

			rend2.material.SetFloat("Vector1_7B2B0159", disolve);
			rend2.material.SetFloat("Vector1_F7E627EB", randNoise2);

		}
	}
	//private void OnMouseDown()
	//{
	//	Destroy(gameObject);
	//}

	public void Controlled()
	{
		isControlled = true;


	}
	private void RotateGems()//кручение модели
	{
			toRotate1.Rotate(new Vector3(1,-1, 1) * Time.deltaTime*speed);

			toRotate2.Rotate(new Vector3(1, 1, -1) * Time.deltaTime*speed);

	}
	private void CeaseControl() //отключение контроля, выравнивание гема
	{
		isControlled = false;
		toRotate1.rotation = Quaternion.identity;
		toRotate2.rotation = Quaternion.identity;
		GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameController>().CeaseControl();
	}
	private void CheckForLayerBelow()//проверка на остановку
	{
		if (transform.position.y <= 9)
		{
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 0.6f, layer) && myRB.velocity.y >= 0)
			{
				CeaseControl();
			}
		}
	}
	public void ActivateBonus()
	{
		transform.GetChild(2).gameObject.SetActive(true);
		hasBonus = true;
	}
	private void CheckForBlocks()//проверка гемов на 3-в-ряд 
	{
		if ((myRB.velocity.y < 0)&&!mustCheckBlocks)
		{
			mustCheckBlocks = true;
		}
		if ((myRB.velocity.y >= 0) && mustCheckBlocks)
		{
			//CheckForFirstRow();
			CheckForGameOver();
			//CheckBlocks(0, gemType);
			mustCheckBlocks = false;
			//isGrounded = false;
			if (!dying)
			{
				gemDestroyManager.AddGemToCheck(gameObject);
			}
			//SetToDestroy();
			//List<GameObject> combinedMatches = GetCombinedMatches(gameObject);
			//if (combinedMatches.Count >= 2)
			//{

			//	foreach (GameObject toDestroy in combinedMatches)
			//	{
			//		toDestroy.GetComponent<GemController>().Die();
			//	}
			//	Die();
			//}
			//else toBeDestroyed = false;

		}
	}


	
	private void CheckForGameOver()//проверка на конец игры
	{
		int count = 0;
		RaycastHit hit;
		
		if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.7f, gemLayer))
		{
			while (Physics.Raycast(hit.transform.position, Vector3.down, out hit, 0.7f, gemLayer))
			{
				if(myRB.velocity.y < 0)
				{
					break;
				}
				if(hit.collider.tag == "level")
				{
					break;
				}
				count++;
			}
			if (count >= 6)
			{
				GameObject.Find("GameMaster").GetComponent<GameController>().GameOver();
			}
		}


	}


	public void Die()//функция смерти
	{
		if (!dying)
		{
			dying = true;//включение "поглощения" модели
			gameObject.GetComponent<BoxCollider>().enabled = false;//выключение коллайдера
			myRB.isKinematic = true;

			rend1.material = dissolveMaterial;
			rend2.material = dissolveMaterial;

			//if (isInFirstRow)
			//{
			//	GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameController>().DeleteFromFirstRow(gameObject);
			//}
			AudioSource.PlayClipAtPoint(gemDestroySound, gameObject.transform.position);
	/*else*/if (hasBonus)
			{
				transform.GetChild(2).GetComponent<Animator>().Play("collapse");
				List<GameObject> firstRow =	GameObject.Find("Level/FirstRowChecker").GetComponent<FirstRowChecker>().GetFirstRow();
				AudioSource.PlayClipAtPoint(firstRowDestroySound, gameObject.transform.position);
				foreach (GameObject gem in firstRow)
				{
					gem.GetComponent<GemController>().Die();
				}
				//GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameController>().FirstRowDie();
				//StartCoroutine(DestroyFirstRow());
			}

			//избавление от "застревания" гемов
			Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
			foreach (Collider hit in colliders)
			{
				Rigidbody rb = hit.GetComponent<Rigidbody>();

				if (rb != null)
					rb.AddExplosionForce(0.1f, transform.position, 2f, 0.1f);
			}

			Destroy(gameObject, 1.5f);
		}
	}
	public void SetToDestroy(bool set)
	{
		toBeDestroyed = set;
	}
	public bool CheckToBeDestroyed()
	{
		return toBeDestroyed;
	}
	public bool CheckForDeath()
	{
		return dying;
	}
	//private void OnCollisionEnter(Collision collision)
	//{
	//	if (!isGrounded)
	//	{
	//		RaycastHit hit;
	//		if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.6f, layer))
	//		{
	//			isGrounded = true;
	//			Debug.Log(gameObject.name + " landed");
	//		}
	//	}
	//}



	//public List<GameObject> GetCombinedMatches(GameObject obj)
	//{
	//	List<GameObject> upMatches = new List<GameObject>();
	//	upMatches = CheckBlocks(obj.transform, Vector3.up);

	//	List<GameObject> downMatches = new List<GameObject>();
	//	downMatches = CheckBlocks(obj.transform, Vector3.down);

	//	List<GameObject> leftMatches = new List<GameObject>();
	//	leftMatches = CheckBlocks(obj.transform, Vector3.left);

	//	List<GameObject> rightMatches = new List<GameObject>();
	//	rightMatches = CheckBlocks(obj.transform, Vector3.right);

	//	List<GameObject> combinedMatches = new List<GameObject>();
	//	Debug.Log("Start finding matches");
	//	Debug.Log("Up " + upMatches.Count);
	//	Debug.Log("Down " + downMatches.Count);
	//	Debug.Log("Left " + leftMatches.Count);
	//	Debug.Log("Right " + rightMatches.Count);


	//	if (upMatches.Count + downMatches.Count >= 2)
	//	{
	//		foreach (GameObject match in upMatches)
	//		{
	//			if (!match.GetComponent<GemController>().CheckToBeDestroyed())
	//			{
	//				combinedMatches.Add(match);
	//				match.GetComponent<GemController>().SetToDestroy();
	//			}
	//		}
	//		foreach (GameObject match in downMatches)
	//		{
	//			if (!match.GetComponent<GemController>().CheckToBeDestroyed())
	//			{
	//				combinedMatches.Add(match);
	//				match.GetComponent<GemController>().SetToDestroy();
	//			}
	//		}
	//	}
	//	if (leftMatches.Count + rightMatches.Count >= 2)
	//	{
	//		foreach (GameObject match in leftMatches)
	//		{
	//			if (!match.GetComponent<GemController>().CheckToBeDestroyed())
	//			{
	//				combinedMatches.Add(match);
	//				match.GetComponent<GemController>().SetToDestroy();
	//			}
	//		}
	//		foreach (GameObject match in rightMatches)
	//		{
	//			if (!match.GetComponent<GemController>().CheckToBeDestroyed())
	//			{
	//				combinedMatches.Add(match);
	//				match.GetComponent<GemController>().SetToDestroy();
	//			}
	//		}
	//	}
	//	Debug.Log(combinedMatches.Count + " of combined matches");

	//	foreach(GameObject combObj in combinedMatches)
	//	{
	//		List<GameObject> combObjList = GetCombinedMatches(combObj);
	//		if (combObjList.Count >= 2)
	//		{
	//			foreach(GameObject tmpObj in combObjList)
	//			{
	//				combinedMatches.Add(tmpObj);
	//			}
	//		}


	//	}
	//	return combinedMatches;
	//}

	//private List<GameObject> CheckBlocks(Transform transform, Vector3 direction)
	//{
	//	List<GameObject> list = new List<GameObject>();
	//	Vector3 offset = new Vector3(0, -0.45f, 0);
	//	RaycastHit hit;
	//	Physics.Raycast(transform.position + offset, direction, out hit, 0.6f, gemLayer);
	//	while (hit.collider != null && hit.collider.tag == gameObject.tag /*&& hit.collider.transform.GetComponent<Rigidbody>().velocity.y >= -0.05*/)
	//	{
	//		list.Add(hit.collider.gameObject);
	//		Debug.Log(hit.collider.tag + " added");
	//		Physics.Raycast(hit.transform.position, direction, out hit, 0.6f, gemLayer);

	//	}

	//	return list;

	//}



	//private void CheckForFirstRow()
	//{
	//	RaycastHit hit;
	//	Physics.Raycast(transform.position, Vector3.down, out hit, 0.6f, layer);
	//	if (hit.collider.tag == "level")
	//	{
	//		isInFirstRow = true;
	//		GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameController>().AddNewToFirstRow(gameObject);
	//	}
	//}
	//public void SetOnFirstRow()
	//{
	//	isInFirstRow = true;
	//}

	//private IEnumerator DestroyFirstRow()
	//{
	//	yield return new WaitForSeconds(0.1f);
	//	GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameController>().FirstRowDie();

	//}
}
