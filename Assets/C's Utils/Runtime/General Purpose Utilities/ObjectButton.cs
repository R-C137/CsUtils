/* ObjectButton.cs - C's Utils
 * 
 * Allows any world space component to be clickable
 * 
 * NOTE: Changing the outline width of the 'Outline' component may cause some lag in the Editor loop
 * 
 * Creation Date: 16/12/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [16/12/2023] - Initial implementation (C137)
 *      [25/12/2023] - Clicking through UI is now optional (C137)
 */
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

///Add a more comprehensible name to it that cannot be used as the class declaration name
[AddComponentMenu("Scripts/3D Button")]
public class ObjectButton : MonoBehaviour
{
    /// <summary>
    /// Whether the object should be clickable through the UI
    /// </summary>
    public bool bypassUI;

    /// <summary>
    /// Callback to when the button is clicked
    /// </summary>
    public UnityEvent onClick;

    private void OnMouseUpAsButton()
    {
        if(bypassUI || !EventSystem.current.IsPointerOverGameObject())
            onClick?.Invoke();
    }
}
