using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {
    public GameObject UIElementPrefab;

    public Slider slider;
    private GameObject Canvas;
    private GameObject UIElement;

    #region Singleton, DontDestroy, Canvas

    public static LevelLoader instace;

    private void Awake()
    {
        instace = this;
        DontDestroyOnLoad(transform.gameObject);
        Canvas = GameObject.Find("Canvas");
        Transform t = Canvas.transform.Find("LevelLoaderUI");
        if (t)
        {
            UIElement = t.gameObject;
        }
        else
        {
            UIElement = GameObject.Find("LevelLoaderUI");
        }
    }

    #endregion

    private void Start()
    {
        if (!UIElement)
        {
            UIElement = Instantiate(UIElementPrefab, Canvas.transform);
        }
    }

    public void LoadLevel(int sceneIndex)
    {
        UIElement.SetActive(true);
        StartCoroutine(LoadAsync(sceneIndex));
    }

    IEnumerator LoadAsync(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        while (!operation.isDone)
        {
            OnProgressUpdate(Mathf.Clamp01(operation.progress / 0.9f));
            yield return null;
        }
    }

    private void OnProgressUpdate(float progress)
    {
        if (!slider)
        {
            slider = UIElement.GetComponentInChildren<Slider>();
        }
        slider.value = progress;
    }

}
