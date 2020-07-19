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

    public void LoadScene(Object scene)
    {
        SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Single);
    }

    public void LoadSceneAdditive(Object scene)
    {
        SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Additive);
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
