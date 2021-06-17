using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance;

    public GameObject xrRig;
    public Image fadeImage;
    [Tooltip("How long the fade animation is in seconds")]
    public float fadeTime;

    public CustomDirectInteractor[] interactors;
    

    // The currently loaded scene
    Scene currentScene;
    // The async operation loading the new scene
    AsyncOperation loadOp;

    bool isLoading = false;



    void Awake()
    {
        instance = this;

        // If there are less than 2 scenes loaded, load the main menu
        if (SceneManager.sceneCount < 2)
        {
            SceneManager.LoadScene(1, LoadSceneMode.Additive);
        }
    }
    void Start()
    {
        // Scene 0 contains this script, but we care about the scene with content
        currentScene = SceneManager.GetSceneAt(1);
        // Set it as the active scene to use its lighting settings
        SceneManager.SetActiveScene(currentScene);
    }


    public void ChangeScene(string sceneName)
    {
        // Prevent loading multiple times
        if (isLoading)
		{
            return;
        }
        isLoading = true;

        // Start fading out
        StartCoroutine(FadeOut(fadeImage));

        // Make the persistant scene active for now
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));

        // Start loading the new scene, preventing it from activating
        loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = false;


        StartCoroutine(LoadNewScene());
    }
	public void ChangeScene(int sceneIndex)
	{
        // Prevent loading multiple times
        if (isLoading)
        {
            return;
        }
        isLoading = true;

        // Start fading out
        StartCoroutine(FadeOut(fadeImage));

        // Make the persistant scene active for now
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));

        // Start loading the new scene, preventing it from activating
        loadOp = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = false;

        
        StartCoroutine(LoadNewScene());
    }

    private IEnumerator LoadNewScene()
    {
        // Wait for the fade out to end
        yield return new WaitForSeconds(fadeTime + 0.5f);


        // Start unloading the current scene
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);
        // Allow the new scene to be activated
        loadOp.allowSceneActivation = true;

        // Wait until both operations are complete
        yield return new WaitUntil(() => { return loadOp.isDone && unloadOp.isDone; });

        // Reset interactable targets to fix 
        foreach (var interactor in interactors)
        {
            interactor.ResetValidTargets();
        }

        // Get the new scene and set it as the active scene
        currentScene = SceneManager.GetSceneAt(1);
        SceneManager.SetActiveScene(currentScene);
        // Remove ref to async operation
        loadOp = null;

        // Reset the rigs transform
        xrRig.transform.position = Vector3.zero;
        xrRig.transform.rotation = Quaternion.identity;

        // Start fading in
        StartCoroutine(FadeIn(fadeImage));

        // Reset flag
        isLoading = false;
    }

    private Image FindFadeImage()
    {
        // Go through the objects in the current scene
        foreach (var obj in currentScene.GetRootGameObjects())
        {
            // Only the XR rig is tagged as the player
            if (obj.CompareTag("Player"))
            {
                // Return the image component in its children
                return obj.GetComponentInChildren<Image>(true);
            }
        }

        // Image wasnt found
        return null;
    }

    private IEnumerator FadeOut(Image fadeImage)
	{
        // Activate fade image
        //fadeImage.enabled = true;

        float t = 0;
        while (t < fadeTime)
		{
            //fadeImage.color = Color.Lerp(Color.clear, Color.black, t / fadeTime);
            Unity.XR.Oculus.Utils.SetColorScaleAndOffset(Color.Lerp(Color.clear, Color.black, t / fadeTime), Vector4.zero);

            t += Time.deltaTime;
            yield return null;
		}
	}
    private IEnumerator FadeIn(Image fadeImage)
	{
        // Activate fade image
        //fadeImage.enabled = true;

        float t = 0;
        while (t < fadeTime)
        {
            //fadeImage.color = Color.Lerp(Color.black, Color.clear, t / fadeTime);
            Unity.XR.Oculus.Utils.SetColorScaleAndOffset(Color.Lerp(Color.black, Color.clear, t / fadeTime), Vector4.zero);

            t += Time.deltaTime;
            yield return null;
        }

        // Deactivate fade image
        //fadeImage.enabled = false;
    }
}