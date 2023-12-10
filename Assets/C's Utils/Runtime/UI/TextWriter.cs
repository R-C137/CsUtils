/* MultiFade.cs - C's Utils
 * 
 * An animation utility with it's main purpose being to animate a text component by giving it a writing animation
 * 
 * 
 * Creation Date: 10/12/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [10/12/2023] - Initial implementation (C137)
 */
using System.Collections;
using TMPro;
using UnityEngine;

public class TextWriter : MonoBehaviour
{
    /// <summary>
    /// The text to animate
    /// </summary>
    public TMP_Text animatedText;

    /// <summary>
    /// Whether writing animation is currently being played
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles
    public bool isWriting { get; private set; }

    /// <summary>
    /// The text to be written
    /// </summary>
    [TextArea]
    public string finalText;

    /// <summary>
    /// Speed at which to write each character
    /// </summary>
    public float characterWriteSpeed = .05f;

    /// <summary>
    /// Whether to auto start the animation on play
    /// </summary>
    public bool autoStart = true;

    /// <summary>
    /// The delay before starting to write, works only on auto start
    /// </summary>
    public float startDelay;

    /// <summary>
    /// The coroutine used for the writing effect
    /// </summary>
    public Coroutine coroutine { get; private set; }
#pragma warning restore IDE1006 // Naming Styles

    /// <summary>
    /// Even raised when the writing animation has finished
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles
    public delegate void WritingFinished();
    public event WritingFinished onWritingFinished;
#pragma warning restore IDE1006 // Naming Styles

    /// <summary>
    /// The progress of writing the text
    /// </summary>
    string writingProgress;

    void Start()
    {
        if (autoStart)
            coroutine = StartCoroutine(WriterCoroutine(finalText));
    }

    /// <summary>
    /// Starts the writing animation
    /// </summary>
    /// <param name="text">The text to be written. Leave at null to use predefined text</param>
    /// <param name="forced">Whether to force the writing and override any current writing animation</param>
    public void Write(string text = null, bool forced = false)
    {
        if(forced || !isWriting)
        {
            if(coroutine != null)
                StopCoroutine(coroutine);

            coroutine = StartCoroutine(text ?? finalText);
        }
    }

    /// <summary>
    /// Stops the writing animation
    /// </summary>
    public void Stop()
    {
        StopCoroutine(coroutine);
    }

    /// <summary>
    /// The coroutine handling the actual animation
    /// </summary>
    /// <param name="text">The text to be written</param>
    /// <returns></returns>
    IEnumerator WriterCoroutine(string text)
    {
        writingProgress = string.Empty;

        yield return new WaitForSeconds(startDelay);

        isWriting = true;

        bool writingTag = false;
        foreach (char c in text)
        {
            if (c == '<')
                writingTag = true;

            if (!writingTag)
                yield return new WaitForSeconds(characterWriteSpeed);
            else if(c == '>')
                    writingTag = false;

            writingProgress += c;

            animatedText.text = writingProgress;
        }

        onWritingFinished?.Invoke();
        isWriting = false;
    }

    private void Reset()
    {
        TryGetComponent(out animatedText);

        finalText = animatedText == null ? null : animatedText.text;
    }
}
