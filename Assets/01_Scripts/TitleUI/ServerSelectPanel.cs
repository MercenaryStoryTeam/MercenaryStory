using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ServerSelectPanel : MonoBehaviour
{
	public Button server1Button;
	public Button server2Button;
	public Button connectButton;
	public GameObject appearance1;
	public GameObject appearance2;
	public GameObject appearance3;

	private int serverNum = 0;

	private void Awake()
	{
		server1Button.onClick.AddListener(() => serverNum = 1);
		server2Button.onClick.AddListener(() => serverNum = 2);
		connectButton.onClick.AddListener(OnConnectButtonClick);
	}

	private void Start()
	{
		serverNum = 0;
		switch (FirebaseManager.Instance.CurrentUserData.user_Appearance)
		{
			case 1:
				appearance1.SetActive(true);
				break;
			case 2:
				appearance2.SetActive(true);
				break;
			case 3:
				appearance3.SetActive(true);
				break;
		}
	}

	private void Update()
	{
		connectButton.interactable =
			PhotonManager.Instance.isReadyToJoinGameServer && serverNum != 0;
	}

	private void OnConnectButtonClick()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene("LJW_TownScene");
		ServerManager.JoinOrCreatePersistentRoom(serverNum.ToString());
		GameManager.Instance.ChangeStage(GameManager.Instance.CurrentScene+1);
	}
}