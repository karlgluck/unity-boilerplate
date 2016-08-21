using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [UnityEngine.SerializeField]
    private string sceneName;

    void Start ()
    {
        SceneManager.LoadSceneAsync(this.sceneName);
	}
}
