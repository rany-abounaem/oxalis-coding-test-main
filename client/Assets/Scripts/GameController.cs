using UnityEngine;
using System.Text;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Net.Http;

public class GameController : MonoBehaviour
{
	public static GameController Instance;

	public GameObject player;
	public GameObject invaderPrefab;
	public Transform invaderSpawnPoint;
	public int numberOfInvaders = 20;

	public GameObject gameOverUI;

	private bool moveRight = true;
	private bool loggedIn = false;

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	private async void Login(System.Action<string> callback)
	{
		const string url = "http://localhost:4444/join";
		var request = new UnityWebRequest(url, "POST");
		request.uploadHandler = new UploadHandlerRaw(await (new StringContent("{\"username\":\"Player\"}", Encoding.UTF8, "application/json")).ReadAsByteArrayAsync());
		request.uploadHandler.contentType = "application/json";
		request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		request.timeout = 5;

		request.SendWebRequest().completed += x => callback(request.downloadHandler.text);
	}

	private async void RetrieveRoundTime(System.Action<string> callback)
	{
		const string url = "http://localhost:4444/timeleft";
		var request = new UnityWebRequest(url, "GET");
		request.uploadHandler = new UploadHandlerRaw(await (new StringContent(string.Empty, Encoding.UTF8, "application/json")).ReadAsByteArrayAsync());
		request.uploadHandler.contentType = "application/json";
		request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		request.timeout = 5;

		request.SendWebRequest().completed += x => callback(request.downloadHandler.text);
	}

	private void Start()
	{
		loggedIn = false;
		Login(StartGame);
	}

	private void StartGame(string loginResponse)
	{
		loggedIn = true;
		SpawnInvaders();
		gameOverUI.SetActive(false);
	}

	private void SpawnInvaders()
	{
		for (int i = 0; i < numberOfInvaders; i++)
		{
			Instantiate(invaderPrefab, invaderSpawnPoint.position + new Vector3(i % 10, -i / 10, 0), Quaternion.identity, invaderSpawnPoint);
		}
	}

	public void Update()
	{
		if (!loggedIn)
			return;

		if (GameObject.FindGameObjectsWithTag("Invader").Length == 0)
		{
			EndRound();
		}
	}

	public void InvaderHitEdge()
	{
		moveRight = !moveRight;
		GameObject[] invaders = GameObject.FindGameObjectsWithTag("Invader");
		foreach (GameObject invader in invaders)
		{
			Invader invaderScript = invader.GetComponent<Invader>();
			if (invaderScript != null)
			{
				invaderScript.ChangeDirection(moveRight);
			}
		}
	}

	private void EndRound()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void GameOver()
	{
		gameOverUI.SetActive(true);
	}
}
