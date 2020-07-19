using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    [SerializeField]
    private Object scene;
    [SerializeField]
    private float time;

    private void Start()
    {
        if (scene != null)
            DelayedLoadScene(scene, time);
    }

    public void LoadScene(Object scene)
    {
        SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Single);
    }

    public void DelayedLoadScene(Object scene, float time)
    {
        StartCoroutine(WaitForSceneLoad(scene, time));
    }

    private IEnumerator WaitForSceneLoad(Object scene, float time)
    {
        yield return new WaitForSeconds(time);
        LoadScene(scene);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
