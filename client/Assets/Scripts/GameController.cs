using UnityEngine;
using System.Text;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class GameController : MonoBehaviour
{

	[SerializeField] private List<InvaderTypeProperties> m_invaderTypeProperties;

	public static GameController Instance;

	public GameObject player;
	public GameObject invaderPrefab;
	public Transform invaderSpawnPoint;
	public int numberOfInvaders = 20;
	[SerializeField] private int numberOfInvaderRows = 2;
	[SerializeField] private float m_invaderShootingCooldown = 0.5f;
	[SerializeField] private List<WeaponProperties> m_weaponUpgrades;

	public GameObject gameOverUI;

	[Header("UI References")]
	[SerializeField] private GameObject m_playerNameInputUI;
	[SerializeField] private InputField m_playerNameInputField;
	[SerializeField] private Button m_loginButton;
	[SerializeField] private Text m_playerScoreText;
	[SerializeField] private Text m_playerHealthText;
	[SerializeField] private GameObject m_leaderboardUI;
	[SerializeField] private Button m_leaderboardButton;
	[SerializeField] private GameObject m_winUI;

	private bool moveRight = true;
	private bool loggedIn = false;
	private List<Invader> m_spawnedInvaders;
	private static float lowestPosition = Mathf.Infinity;
	private int aggression;
	private List<Invader> m_invadersWithClearPaths = new List<Invader>();
	private float m_timeElapsedSinceLastShoot = 0f;
	private Player m_playerComponent;
	private LeaderboardUI m_leaderboardUIComponent;
	private bool m_isGameOver = false;

	public bool LoggedIn => loggedIn;

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	private byte[] GetByteEncoding(string str)
	{
		return Encoding.UTF8.GetBytes(str);
	}

	private IEnumerator Login(System.Action<string> callback)
	{
		const string url = "http://localhost:4444/join";
		var request = new UnityWebRequest(url, "POST");
		var playerDetailsJson = JsonUtility.ToJson(m_playerComponent.PlayerData);
		var content = GetByteEncoding(playerDetailsJson);
		request.uploadHandler = new UploadHandlerRaw(content); // Removes .NET HTTP dependency and redundant Content-Type definition
		request.uploadHandler.contentType = "application/json";
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		request.timeout = 5;

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success)
		{
			callback?.Invoke(request.downloadHandler.text);
			request.Dispose();
		}
	}


	// 3.1: Round time retrieval method
	public IEnumerator RetrieveRoundTime(System.Action<string> callback)
	{
		if (!loggedIn)
		{
			callback(string.Empty);
			yield break;
		}

		const string url = "http://localhost:4444/timeLeft";
		var request = new UnityWebRequest(url, "GET");
		var playerDetailsJson = JsonUtility.ToJson(m_playerComponent.PlayerData);
		var content = GetByteEncoding(playerDetailsJson);
		request.uploadHandler = new UploadHandlerRaw(content);
		request.uploadHandler.contentType = "application/json";
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		request.timeout = 5;

		yield return request.SendWebRequest();

		callback?.Invoke(request.downloadHandler.text);
		request.Dispose();
	}

	private void Start()
	{
		m_playerComponent = player.GetComponent<Player>();
		m_spawnedInvaders = new List<Invader>(numberOfInvaders);
		loggedIn = false;

		m_loginButton.onClick.AddListener(() =>
		{
			if (string.IsNullOrEmpty(m_playerNameInputField.text))
			{
				Debug.LogWarning("Username cannot be empty.");
				return;
			}
			m_playerComponent.PlayerData.username = m_playerNameInputField.text;
		});

		m_leaderboardButton.onClick.AddListener(() =>
		{
			m_leaderboardUI.SetActive(true);
			if (m_leaderboardUI.activeSelf)
			{
				StartCoroutine(RetrieveLeaderboardEntries());
			}
		});

		m_leaderboardUIComponent = m_leaderboardUI.GetComponent<LeaderboardUI>();

		StartCoroutine(ReceivePlayerName());
	}

	public void SaveScore()
	{
		StartCoroutine(SaveScoreToServer());
	}

	private IEnumerator SaveScoreToServer()
	{
		const string url = "http://localhost:4444/save";
		var request = new UnityWebRequest(url, "POST");
		var playerDetailsJson = JsonUtility.ToJson(m_playerComponent.PlayerData);
		var content = GetByteEncoding(playerDetailsJson);
		request.uploadHandler = new UploadHandlerRaw(content);
		request.uploadHandler.contentType = "application/json";
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		request.timeout = 5;

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success)
		{
			Debug.Log("Score saved successfully.");
			request.Dispose();
		}
		else
		{
			Debug.LogError($"Failed to save score: {request.error}");
			request.Dispose();
		}
	}

	private IEnumerator RetrieveLeaderboardEntries()
	{
		const string url = "http://localhost:4444/leaderboard";
		var request = new UnityWebRequest(url, "GET");
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		request.timeout = 5;

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success)
		{
			var leaderboardEntries = JsonUtility.FromJson<LeaderboardEntries>(request.downloadHandler.text);
			foreach (var entry in leaderboardEntries.entries)
			{
				m_leaderboardUIComponent.AddEntry(entry.username, entry.score);
			}
			request.Dispose();
		}
		else
		{
			Debug.LogError($"Failed to retrieve leaderboard entries: {request.error}");
			request.Dispose();
		}
	}

	private void StartGame(string loginResponse)
	{
		loggedIn = true;
		TryUpgradeWeapon();
		SpawnInvaders();
		StartCoroutine(TickInvaders());
	}

	private void SpawnInvaders()
	{
		for (int i = 0; i < numberOfInvaders; i++)
		{
			var invader = Instantiate(invaderPrefab,
			 invaderSpawnPoint.position + new Vector3(i % (numberOfInvaders / numberOfInvaderRows), -i / (numberOfInvaders / numberOfInvaderRows), 0),
			 Quaternion.identity, invaderSpawnPoint);

			var invaderComponent = invader.GetComponent<Invader>();
			invaderComponent.Init(m_invaderTypeProperties[i % numberOfInvaderRows]);
			m_spawnedInvaders.Add(invaderComponent);
		}
	}

	public void Update()
	{
		if (m_isGameOver)
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				EndRound();
			}
		}

		if (!loggedIn)
			return;

		m_playerScoreText.text = m_playerComponent.PlayerData.score.ToString();
		m_playerHealthText.text = m_playerComponent.CurrentHealth.ToString();

		if (m_spawnedInvaders.Count == 0)
		{
			Debug.Log("All invaders defeated!");
			Win();
		}
	}

	private void FixedUpdate()
	{
		if (loggedIn)
		{
			m_timeElapsedSinceLastShoot += Time.fixedDeltaTime;
			if (m_timeElapsedSinceLastShoot >= m_invaderShootingCooldown)
			{
				m_timeElapsedSinceLastShoot = 0f;
				CheckInvadersWithClearPaths();
			}
		}

	}

	private void CheckInvadersWithClearPaths()
	{
		m_invadersWithClearPaths.Clear();
		foreach (var invader in m_spawnedInvaders)
		{
			if (invader.IsClearBulletPath())
			{
				m_invadersWithClearPaths.Add(invader);
			}
		}

		int numberOfInvadersToShoot = Mathf.Min(m_invadersWithClearPaths.Count, aggression);

		for (int i = 0; i < numberOfInvadersToShoot; i++)
		{
			var randomAttemptToShoot = UnityEngine.Random.Range(0f, 1f);
			if (randomAttemptToShoot < 0.5f)
			{
				continue;
			}

			var randomInvader = UnityEngine.Random.Range(0, m_invadersWithClearPaths.Count);

			if (m_invadersWithClearPaths[randomInvader] != null)
			{
				m_invadersWithClearPaths[randomInvader].Shoot();
			}
		}
	}

	public void InvaderHitEdge()
	{
		moveRight = !moveRight;
		foreach (var invader in m_spawnedInvaders)
		{
			Invader invaderScript = invader.GetComponent<Invader>();
			if (invaderScript != null)
			{
				invaderScript.ChangeDirection(moveRight);
			}
		}
	}

	public void EndRound()
	{
		m_isGameOver = false;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void GameOver()
	{
		loggedIn = false;
		StopAllCoroutines();
		StartCoroutine(EndServerMatch());
		gameOverUI.SetActive(true);
		m_isGameOver = true;
	}

	public void Win()
	{
		loggedIn = false;
		StopAllCoroutines();
		StartCoroutine(EndServerMatch());
		m_winUI.SetActive(true);
	}

	private IEnumerator EndServerMatch()
	{
		const string url = "http://localhost:4444/endmatch";
		var request = new UnityWebRequest(url, "POST");
		var playerDetailsJson = JsonUtility.ToJson(m_playerComponent.PlayerData);
		var content = GetByteEncoding(playerDetailsJson);
		request.uploadHandler = new UploadHandlerRaw(content);
		request.uploadHandler.contentType = "application/json";
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		request.timeout = 5;

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success)
		{
			Debug.Log("Server match ended successfully.");
			request.Dispose();
		}
		else
		{
			Debug.LogError($"Failed to end server match: {request.error}");
			request.Dispose();
		}
	}


	// 3.2 Block gameplay until player name is received.
	private IEnumerator ReceivePlayerName()
	{
		m_playerNameInputUI.SetActive(true);
		yield return new WaitUntil(() => m_playerComponent.PlayerData.username != null);
		m_playerNameInputUI.SetActive(false);
		StartCoroutine(Login(StartGame));
	}

	private IEnumerator TickInvaders()
	{
		while (loggedIn)
		{
			foreach (var invader in m_spawnedInvaders)
			{
				Vector3 movement = moveRight ? Vector3.right : Vector3.left;
				invader.transform.Translate(movement * invader.speed);

				if (invader.transform.position.y < lowestPosition)
				{
					lowestPosition = transform.position.y;
				}
			}

			foreach (var invader in m_spawnedInvaders)
			{
				if (CheckScreenEdgeCollision(invader))
				{
					InvaderHitEdge();
					break;
				}
			}

			StartCoroutine(RetrieveAggressionLevel(UpdateAggression));

			yield return new WaitForSeconds(1f);
		}
	}


	// 2.1: Avoid using Physics2D.Raycast outside of FixedUpdate. I prefer avoiding Physics at all and just comparing with Camera bounds instead.
	private bool CheckScreenEdgeCollision(Invader invader)
	{
		if (invader.transform.position.x >= CameraExtensions.OrthographicBounds(Camera.main).max.x - 0.5f ||
			invader.transform.position.x <= CameraExtensions.OrthographicBounds(Camera.main).min.x + 0.5f)
		{
			return true;
		}
		return false;
	}


	// 3.3: Invader firing is implemented and so is aggression level retrieval.
	private IEnumerator RetrieveAggressionLevel(System.Action<string> callback)
	{
		if (!loggedIn)
		{
			callback(string.Empty);
			yield break;
		}

		const string url = "http://localhost:4444/aggression";
		var request = new UnityWebRequest(url, "GET");
		var playerDetailsJson = JsonUtility.ToJson(m_playerComponent.PlayerData);
		// 3.4: Byte data is retrieved without relying on .NET HTTP dependencies.
		var content = GetByteEncoding(playerDetailsJson);
		request.uploadHandler = new UploadHandlerRaw(content);
		request.uploadHandler.contentType = "application/json";
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		request.timeout = 5;

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success)
		{
			callback?.Invoke(request.downloadHandler.text);
			request.Dispose();
		}
	}

	private void UpdateAggression(string res)
	{
		aggression = (int)JsonUtility.FromJson<AggressionLevel>(res).aggression;
		Debug.Log($"Aggression Level: {aggression}");
	}

	public void OnInvaderDeath(Invader invader)
	{
		if (m_spawnedInvaders.Contains(invader))
		{
			m_spawnedInvaders.Remove(invader);
			AddPlayerScore(invader.InvaderType.Points);
			Destroy(invader.gameObject);
			Debug.Log("An invader has been defeated!");
		}
		else
		{
			Debug.LogWarning("Attempted to remove an invader that does not exist in the list.");
		}
	}

	private void AddPlayerScore(int score)
	{
		m_playerComponent.PlayerData.score += score;
		Debug.Log($"Player {m_playerComponent.PlayerData.username} scored {score} points. Total score: {m_playerComponent.PlayerData.score}");

		TryUpgradeWeapon();
	}

	private void TryUpgradeWeapon()
	{
		// Weapon upgrading logic

		if (m_playerComponent.CurrentWeapon == null)
		{
			m_playerComponent.CurrentWeapon = m_weaponUpgrades[0];
			return;
		}

		var currWepGrade = m_playerComponent.CurrentWeapon.Grade;
		// No more upgrades available -> return
		if (currWepGrade >= m_weaponUpgrades.Count)
		{
			return;
		}

		for (int i = 0; i < m_weaponUpgrades.Count; i++)
		{
			if (m_weaponUpgrades[i].Grade > currWepGrade && m_weaponUpgrades[i].PointsToUnlock <= m_playerComponent.PlayerData.score)
			{
				m_playerComponent.CurrentWeapon = m_weaponUpgrades[i];
				Debug.Log($"Player {m_playerComponent.PlayerData.username} upgraded weapon to {m_weaponUpgrades[i].Type}");
			}
		}
	}
	
	private void OnApplicationQuit()
	{
		if (loggedIn)
		{
			StartCoroutine(EndServerMatch());
		}
		loggedIn = false;
		m_isGameOver = false;
		m_spawnedInvaders.Clear();
	}
}

public struct AggressionLevel
{
	public float aggression;
}
