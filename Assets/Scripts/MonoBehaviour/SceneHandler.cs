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
    [SerializeField]
    private bool additive;

    private void Start()
    {
        if (scene != null)
            DelayedLoadScene(scene, time, additive);
    }

    public void TryUnloadScene(Object scene)
    {
        TryUnloadScene(scene.name);
    }

    public void TryUnloadScene(string scene)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == scene)
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }
    }

    public void LoadScene(Object scene)
    {
        LoadScene(scene.name);
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
    }

    public void LoadSceneAdditive(Object scene)
    {
        LoadSceneAdditive(scene.name);
    }

    public void LoadSceneAdditive(string scene)
    {
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
    }

    public void DelayedLoadScene(Object scene, float time, bool additive)
    {
        StartCoroutine(WaitForSceneLoad(scene, time, additive));
    }

    private IEnumerator WaitForSceneLoad(Object scene, float time, bool additive)
    {
        yield return new WaitForSeconds(time);
        if (additive)
        {
            LoadSceneAdditive(scene);
        }
        else
        {
            LoadScene(scene);
        }
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
