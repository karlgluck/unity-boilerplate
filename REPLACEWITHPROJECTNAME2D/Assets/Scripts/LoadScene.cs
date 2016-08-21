using UnityEngine;
using System.Collections;

public class LoadScene : MonoBehaviour
{
    [UnityEngine.SerializeField]
    private string levelName;

    void Start ()
    {
        Application.LoadLevelAsync(this.levelName);
	}
}
