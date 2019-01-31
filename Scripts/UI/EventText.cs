using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventText : MonoBehaviour {

    #region - Variables.
    public GameObject textObj;

    public float fadeInTime = 0.1f;
    public float fadeOutTime = 0.1f;
    public float displayTime = 0.2f;

    public bool currentlyRunning = false;
    public Queue<Color> messageColours = new Queue<Color>();
    public Queue<string> messages = new Queue<string>();
    #endregion

    /// <summary>
    /// Use this to set the text message and colour.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="colour"></param>
    /// <param name="message"></param>
    public void SetText(Color colour, string message)
    {
        if (currentlyRunning)
        {
            messageColours.Enqueue(colour);
            messages.Enqueue(message);
        }
        else
        {
            StartDisplaying(colour, message);
        }
    }

    /// <summary>
    /// Use this to begin the Display Text coroutine.
    /// </summary>
    /// <param name="colour"></param>
    /// <param name="message"></param>
    private void StartDisplaying(Color colour, string message)
    {
        GameObject newTextObj = GameObject.Instantiate(textObj, textObj.transform.parent);
        newTextObj.SetActive(true);
        Text newText = newTextObj.GetComponent<Text>();
        newText.text = message;
        newText.color = new Color(colour.r, colour.g, colour.b, 0);
        StartCoroutine(DisplayText(newTextObj, newText));
    }

    /// <summary>
    /// Use this coroutine to display a text on screen to the user (with fade in/fade out).
    /// </summary>
    /// <param name="newTextObj"></param>
    /// <param name="newText"></param>
    /// <returns></returns>
    private IEnumerator DisplayText(GameObject newTextObj, Text newText)
    {
        currentlyRunning = true;
        bool fadingIn = true;
        bool displaying = true;
        bool fadingOut = true;
        // This is the current time passed.
        float timePassed = 0;
        // These are the colour alpha rates of change per 0.01 of a second.
        float fadeInAlphaChange = 1 / (fadeInTime / 0.01f);
        float fadeOutAlphaChange = 1 / (fadeOutTime / 0.01f);

        newTextObj.SetActive(true);


        // Fade In.
        while (fadingIn)
        {
            // Add 0.01 to the current timer and test it.
            timePassed += 0.01f;
            if (timePassed >= fadeInTime)
            {
                // Stop the fade in.
                fadingIn = false;
                timePassed = 0;
                break;
            }
            // Wait for 0.01 of a second.
            yield return new WaitForSeconds(0.01f);
            // Change the alpha value of the text.
            float newAlpha = newText.color.a + fadeInAlphaChange;
            if (newAlpha > 1)
            {
                newAlpha = 1;
            }
            newText.color = new Color(newText.color.r, newText.color.g, newText.color.b, newAlpha);
        }

        // Display.
        while (displaying)
        {
            // Add 0.01 to the current timer and test it.
            timePassed += 0.01f;
            if (timePassed >= displayTime)
            {
                // Stop the display.
                displaying = false;
                timePassed = 0;
                break;
            }
            // Wait for 0.01 of a second.
            yield return new WaitForSeconds(0.01f);
        }

        // Fade Out.
        while (fadingOut)
        {
            // Add 0.01 to the current timer and test it.
            timePassed += 0.01f;
            if (timePassed >= fadeOutTime)
            {
                // Stop the fade out.
                fadingOut = false;
                break;
            }
            // Wait for 0.01 of a second.
            yield return new WaitForSeconds(0.01f);
            // Change the alpha value of the text.
            float newAlpha = newText.color.a - fadeOutAlphaChange;
            if (newAlpha < 0)
            {
                newAlpha = 0;
            }
            newText.color = new Color(newText.color.r, newText.color.g, newText.color.b, newAlpha);
        }
        // Destroy the object now that it has served its purpose.
        Destroy(newTextObj);
        // Check if there are any more messages to display.
        if (messages.Count > 0)
        {
            // Display the next message in the queue.
            StartDisplaying(messageColours.Dequeue(), messages.Dequeue());
        }
        else
        {
            // Turn off this IEnumerator.
            currentlyRunning = false;
        }
        // End Enumerator.
        yield return null;
    }
}
