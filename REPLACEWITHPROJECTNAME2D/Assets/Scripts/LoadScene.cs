using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [UnityEngine.SerializeField]
    private string levelName;

    void Start ()
    {
        SceneManager.LoadSceneAsync(this.levelName);
	}
}
