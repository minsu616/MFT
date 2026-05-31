using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class ResultManager : MonoBehaviour
{
    public TextMeshProUGUI resultText;

    void Start()
    {
        if (GameData.Instance == null) return;

        if (GameData.Instance.isVictory)
        {
            resultText.text = "½Â¸®!";
            resultText.color = Color.yellow;
        }
        else
        {
            resultText.text = "ÆÐ¹è...";
            resultText.color = Color.red;
        }
    }

    public void OnRestartButton()
    {
        GameData.Instance.ClearFleet();
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LoadLevel("ShipPlacement");
        else
            SceneManager.LoadScene("ShipPlacement");
    }

    public void OnMainMenuButton()
    {
        GameData.Instance.ClearFleet();
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("MainMenu");
    }
}