using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour {

	[SerializeField]
	private GameObject gameMaster;
	private bool paused = false;

	public void PauseGame()
	{
		if (paused)
		{
			UnFreeze();
		}
		else Freeze();

	}
	private void Freeze()
	{
		paused = true;
		Time.timeScale = 0f;
		gameMaster.GetComponent<GameController>().enabled = false;
		gameMaster.GetComponent<GemDestroyManager>().enabled = false;


	}
	private void UnFreeze()
	{
		Time.timeScale = 1f;
		gameMaster.GetComponent<GemDestroyManager>().enabled = true;
		gameMaster.GetComponent<GameController>().enabled = true;
		paused = false;
	}

	public void GameOver()
	{
		Freeze();
	}

	public void Restart()
	{
		Time.timeScale = 1f;
		paused = false;
		
	}


}
