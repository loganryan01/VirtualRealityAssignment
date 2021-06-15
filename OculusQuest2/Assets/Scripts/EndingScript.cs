using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingScript : MonoBehaviour
{
    public Text endingText;

    public float delayForText;
    public float fadeTime;
    public float delayForMenu;



    void Start()
    {
        StartCoroutine(FadeText());
    }

    private IEnumerator FadeText()
	{
        yield return new WaitForSeconds(delayForText);

        // Fade text in
        float t = 0;
        while (t < fadeTime)
		{
            endingText.color = Color.Lerp(Color.clear, Color.white, t / fadeTime);

            t += Time.deltaTime;
            yield return null;
		}


        yield return new WaitForSeconds(delayForMenu);
        // Return to menu
        TransitionManager.instance.ChangeScene(1);
	}
}
