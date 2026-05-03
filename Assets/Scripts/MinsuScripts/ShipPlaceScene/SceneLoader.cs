using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void GoToBattle()
    {
        SceneManager.LoadScene("Battle");
    }
}
