using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
	enum GenerationType { RandomColumns, OneColumn};

	[SerializeField]
	private GenerationType generationType;
	[SerializeField]
	private Transform parentForGems; //родительский объект для гемов (для чистоты инспектора)

	[SerializeField]
	private GameObject[] prefabs; //хранилище гемов
	private GameObject currentlyControlledGem;

	private bool canCoutinue = true;
	private bool isPaused = false;

	[SerializeField]
	private LayerMask layer;
	//private List<GameObject> firstRow= new List<GameObject>();
	private List<int> possiblePositionsForCurrentGem = new List<int>();


	[SerializeField]
	private Text gameOverText;

	[SerializeField]
	private Button pauseButton;
	[SerializeField]
	private Button restartButton;

	[SerializeField]
	private int minimalFieldX = -2;
	[SerializeField]
	private int maximalFieldX = 3;
	[SerializeField]
	private int generationCount = 3;


	// Use this for initialization
	void Start()
	{
		FillStart(); //заполнение первого ряда
		StartCoroutine(StartTheGame()); //начало игры (первые гемы появятся через 2 сек)
	}

	// Update is called once per frame
	void Update()
	{
		GetControls();
	}

	private void FillStart() //генерация гемов первого ряда
	{
		int lastColor1=-1;
		int lastColor2=-2;
		for (int columnNumber = minimalFieldX; columnNumber < maximalFieldX; columnNumber++)
		{
			bool done = false;
			while (!done)
			{
				int randomColorObject = Random.Range(0, prefabs.Length);
				if (!((lastColor1 == lastColor2)&&(lastColor2==randomColorObject))) //исколючение 3 одинаковых в первом ряду
				{
					GameObject newGem = Instantiate(prefabs[randomColorObject], new Vector3(columnNumber, 0, 0), gameObject.transform.rotation, parentForGems);
					lastColor2 = lastColor1;
					lastColor1 = randomColorObject;
					//firstRow.Add(newGem);
					//newGem.GetComponent<GemController>().SetOnFirstRow();
					done = true;
				}
			}
		}

	}


	private void GenerateNewGems()//генерация новых гемов, отправка списка новых гемов функции установления контроля + позиций гемов по оси X
	{
		if (generationType == GenerationType.RandomColumns)
		{
			List<int> gemLocations = new List<int>();
			List<GameObject> gems = new List<GameObject>();
			List<int> gemPositions = new List<int>();

			gemLocations = GetRandomNumbers(generationCount, minimalFieldX, maximalFieldX);

			foreach (int gemLocation in gemLocations)
			{
				int randomColorObject = Random.Range(0, prefabs.Length);
				GameObject newGem = Instantiate(prefabs[randomColorObject], new Vector3(gemLocation, 15, 0), gameObject.transform.rotation, parentForGems);
				bool bonus = GambleBonus();
				if (bonus)
				{
					newGem.GetComponent<GemController>().ActivateBonus();
				}
				gems.Add(newGem);
				gemPositions.Add(gemLocation);
			}
			GetGemControlRandomColumns(gems, gemPositions);
		}
		if (generationType == GenerationType.OneColumn)
		{
			List<GameObject> gems = new List<GameObject>();
			List<int> gemPositions = new List<int>();
			int locationX = Random.Range(minimalFieldX, maximalFieldX);
			int locationY = 15;
			int generateCount = generationCount;
			while (generateCount > 0)
			{
				bool bonus = GambleBonus();
				int randomColorObject = Random.Range(0, prefabs.Length);
				GameObject newGem = Instantiate(prefabs[randomColorObject], new Vector3(locationX, locationY, 0), gameObject.transform.rotation, parentForGems);
				generateCount--;
				locationY--;
				if (bonus)
				{
					newGem.GetComponent<GemController>().ActivateBonus();
				}
				gems.Add(newGem);

			}
			GetGemControlOneColumn(gems);
			foreach(GameObject gem in gems)
			{
				Debug.Log(gem.name + " in gems");
			}

		}
	}


	private void GetGemControlOneColumn(List<GameObject> gems)
	{

		int randomControllableGem = Random.Range(0, gems.Count);// выбор нового контролируемого гема
		currentlyControlledGem = gems[randomControllableGem];//установление контроля над новых гемом
		Debug.Log(currentlyControlledGem.name + " is currently controlled");
		currentlyControlledGem.GetComponent<GemController>().Controlled();//гем "узнает", что находится под контролем
		List<int> possiblePositions = new List<int>();//возможные позиции для перемещения гема

		for (int i = minimalFieldX; i < maximalFieldX; i++)//заполнение возможных позиций
		{
			possiblePositions.Add(i);
			Debug.Log("New possible position to move added: " + i);
		}
		possiblePositionsForCurrentGem = possiblePositions;
	}

	private void GetGemControlRandomColumns(List<GameObject> gems, List<int> gemPositions)
	{
		
		int randomControllableGem = Random.Range(0, gems.Count);// выбор нового контролируемого гема
		currentlyControlledGem = gems[randomControllableGem];//установление контроля над новых гемом
		int currentlyControlledGemPos = gemPositions[randomControllableGem];//позиция контролируемого гема
		currentlyControlledGem.GetComponent<GemController>().Controlled();//гем "узнает", что находится под контролем
		Debug.Log(gems[randomControllableGem] + " in position " + currentlyControlledGemPos + " is under direct control");
		List<int> possiblePositions = new List<int>();//возможные позиции для перемещения гема

		for (int i = minimalFieldX; i < maximalFieldX; i++)//заполнение возможных позиций
		{
			bool isViable = true;
			foreach (int gemPos in gemPositions)
			{
				if ((i == gemPos) && (i != currentlyControlledGemPos))
				{
					isViable = false;
				}
			}
			if (isViable)
			{
				possiblePositions.Add(i);
				Debug.Log("New possible position to move added: " + i);
			}
		}
		possiblePositionsForCurrentGem = possiblePositions;
	

	}

	private IEnumerator StartTheGame()//начальная генерация новых гемов и запуск бесконечной зацикленной генерации
	{
		while (isPaused)
		{
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		while (isPaused)
		{
			yield return null;
		}
		GenerateNewGems();
		StartCoroutine(ContinueTheGame());
	}
	private IEnumerator ContinueTheGame()//бесконечная генерация, до тех пор, пока можно продолжать
	{
		while (isPaused)
		{
			yield return null;
		}
		yield return new WaitForSeconds(5f);
		while (isPaused)
		{
			yield return null;
		}
		GenerateNewGems();
		if (canCoutinue)
		{
			StartCoroutine(ContinueTheGame());
		}
	}


	private void GetControls()
	{
		if (generationType == GenerationType.RandomColumns)
		{
			if (currentlyControlledGem != null)
			{
				if (Input.GetKeyDown(KeyCode.Q))
				{
					MoveLeft();
				}
				if (Input.GetKeyDown(KeyCode.E))
				{
					MoveRight();
				}
			}
		}
		if(generationType == GenerationType.OneColumn)
		{
			if (currentlyControlledGem != null)
			{
				if (Input.GetKeyDown(KeyCode.Q))
				{
					MoveLeftOneColumn();
				}
				if (Input.GetKeyDown(KeyCode.E))
				{
					MoveRightOneColumn();
				}
			}


		}
	}

	private void MoveLeftOneColumn()
	{
		if (currentlyControlledGem != null)
		{
			int newPosX = (int)currentlyControlledGem.transform.position.x;

			RaycastHit hit;
			Physics.Raycast(currentlyControlledGem.transform.position + new Vector3(0, -0.4f, 0), Vector3.left, out hit, 1f, layer);
			if (hit.transform == null)
			{
				newPosX--;
				Vector3 newPosition = new Vector3(newPosX, currentlyControlledGem.transform.position.y, currentlyControlledGem.transform.position.z);
				currentlyControlledGem.transform.position = newPosition;
			}
		}
		

	}//перемещение влево

	private void MoveRightOneColumn()
	{
		if (currentlyControlledGem != null)
		{
			int newPosX = (int)currentlyControlledGem.transform.position.x;

			RaycastHit hit;
			Physics.Raycast(currentlyControlledGem.transform.position + new Vector3(0, -0.2f, 0), Vector3.right, out hit, 1f, layer);
			if (hit.transform == null)
			{
				newPosX++;
				Vector3 newPosition = new Vector3(newPosX, currentlyControlledGem.transform.position.y, currentlyControlledGem.transform.position.z);
				currentlyControlledGem.transform.position = newPosition;
			}
		}
		

	}//перемещение влево

	private void MoveLeft()
	{
		int newPosX= (int)currentlyControlledGem.transform.position.x;

		foreach(int pos in possiblePositionsForCurrentGem)
		{
			if ((int)currentlyControlledGem.transform.position.x > pos)
			{
				newPosX = pos;
			}
		}
		Vector3 newPosition = new Vector3(newPosX, currentlyControlledGem.transform.position.y, currentlyControlledGem.transform.position.z);
		currentlyControlledGem.transform.position = newPosition;
	}//перемещение влево
	private void MoveRight()
	{
		int newPosX = (int)currentlyControlledGem.transform.position.x;

		foreach (int pos in possiblePositionsForCurrentGem)
		{
			if ((int)currentlyControlledGem.transform.position.x < pos)
			{
				newPosX = pos;
				break;
			}
		}
		Vector3 newPosition = new Vector3(newPosX, currentlyControlledGem.transform.position.y, currentlyControlledGem.transform.position.z);
		currentlyControlledGem.transform.position = newPosition;	
	}//перемещение вправо




	private List<int> GetRandomNumbers(int howMany, int min, int max)//получение рандомных чисел для позиции
	{
		if ((howMany <= 0) || (min > max) || ((max - min) <= howMany))
		{
			Debug.Log("GetRandomNumbers got incorrect input");
			return null;
		}
		List<int> result = new List<int>();
		while (howMany > 0)
		{
			bool isViable = true;
			int newResult = Random.Range(min, max);
			Debug.Log(newResult);
			if (result.Count > 0)
			{
				foreach (int possibleResult in result)
				{
					Debug.Log("Possible result " + possibleResult);
					if (newResult == possibleResult)
					{
						isViable = false;
						break;
					}
				}
			}
			if (isViable)
			{
				result.Add(newResult);
				howMany--;
			}
		}
		return result;
	}
	
	private bool GambleBonus()
	{
		int i = Random.Range(0, 100);
		if (i < 3)
		{
			return true;
		}
		else return false;
	}

	public void CeaseControl()
	{
		currentlyControlledGem = null;
		possiblePositionsForCurrentGem = null;
	}
	public void MoveLeftBtn()
	{
		if (generationType == GenerationType.RandomColumns)
		{
			MoveLeft();
		}
		if (generationType == GenerationType.OneColumn)
		{
			MoveLeftOneColumn();
		}
	}
	public void MoveRightBtn()
	{
		if (generationType == GenerationType.RandomColumns)
		{
			MoveRight();
		}
		if (generationType == GenerationType.OneColumn)
		{
			MoveRightOneColumn();
		}
	}
	public void IsPaused()
	{
		isPaused = !isPaused;
	}

	public void GameOver()
	{
		GameObject.Find("PauseManager").GetComponent<PauseManager>().GameOver();
		GetComponent<GameController>().enabled = false;
		GetComponent<GemDestroyManager>().enabled = false;
		gameOverText.gameObject.SetActive(true);
		restartButton.gameObject.SetActive(true);
		pauseButton.gameObject.SetActive(false);

	}
	//public void FirstRowDie()//уничтожение первой линии
	//{
	//	if (firstRow.Count != 0)
	//	{
	//		foreach (GameObject gem in firstRow)
	//		{
	//			if (gem != null)
	//			{
	//				Destroy(gem);
	//				//gem.GetComponent<GemController>().Die();
	//				DeleteFromFirstRow(gem);
	//			}
	//		}
	//	}

	//}

	//public void AddNewToFirstRow(GameObject newGem)
	//{
	//	firstRow.Add(newGem);
	//}
	//public void DeleteFromFirstRow(GameObject lostGem)
	//{
	//	firstRow.Remove(lostGem);
	//}

	// систему 3 в ряд
	// счет
	// меню
}
