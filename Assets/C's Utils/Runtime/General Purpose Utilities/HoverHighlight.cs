/* HoverHighlight.cs - C's Utils
 * 
 * Highlights a game object when it is hovered
 * 
 * 
 * Creation Date: 16/12/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [16/12/2023] - Initial implementation (C137)
 *      [18/12/2023] - Added a callback to when the object is clicked (C137)
 */
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Outline))]
public class HoverHighlight : MonoBehaviour
{
    /// <summary>
    /// Reference to the outline handler
    /// </summary>
    public Outline outline;

    /// <summary>
    /// The outline width to set when the object is highlighted
    /// </summary>
    public float outlineWidth = 2f;

    /// <summary>
    /// Event raised when the player clicks on the object
    /// </summary>
    public UnityEvent onClick;

    /// <summary>
    /// The outline width before highlighting. Used internally to disable highlighting
    /// </summary>
    float normalOutlineWidth;

    /// <summary>
    /// Whether the mouse has exited the object while it was active. Used internally to prevent bugs
    /// </summary>
    bool mouseExited = true;

    private void Awake()
    {
        if (outline == null)
            TryGetComponent(out outline);
    }

    private void OnMouseEnter()
    {
        //Prevent the normal outline width from changing if the OnMouseExit wasn't called properly
        if (mouseExited)
            normalOutlineWidth = outline.OutlineWidth;

        mouseExited = false;

        outline.enabled = true;
        outline.OutlineWidth = outlineWidth;
    }

    private void OnMouseUpAsButton()
    {
        onClick?.Invoke();
    }

    private void OnMouseExit()
    {
        outline.OutlineWidth = normalOutlineWidth;

        mouseExited = true;
    }

    private void Reset()
    {
        TryGetComponent(out outline);
    }
}
