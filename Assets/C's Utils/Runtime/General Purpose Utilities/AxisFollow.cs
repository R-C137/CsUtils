/* FollowAxes.cs - C's Utils
 * 
 * Follow an object along select axes, using an offset with smoothing
 * 
 * 
 * Creation Date: 13/12/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [13/12/2023] - Initial implementation (C137)
 */
using CsUtils.Extensions;
using System;
using UnityEngine;

[Serializable]
public struct FollowAxes
{
    /// <summary>
    /// Which of the axes to follow
    /// </summary>
    public bool followX, followY, followZ;
}

public class AxisFollow : MonoBehaviour
{
    /// <summary>
    /// The target to follow
    /// </summary>
    public Transform target;

    /// <summary>
    /// How smooth should the follow be. Set to 1 to disable smoothing
    /// </summary>
    public float smoothing = 1;

    /// <summary>
    /// The easing to use
    /// </summary>
    public AnimationCurve easing;

    /// <summary>
    /// The offset by which to follow the target
    /// </summary>
    public Vector3 offset;

    /// <summary>
    /// The axes to follow
    /// </summary>
    public FollowAxes axes;

    /// <summary>
    /// Whether to automatically set the offset when a new target is added
    /// </summary>
    public bool autoOffset = true;

    /// <summary>
    /// Internal reference to determine when the follow target has changed
    /// </summary>
    Transform _oldTarget;

    public void Update()
    {
        FollowTarget();
    }

    /// <summary>
    /// Handles the following of the target
    /// </summary>
    public void FollowTarget()
    {
        Vector3 pos = transform.position;

        if (axes.followX)
            pos.x = target.position.x + offset.x;
        if (axes.followY)
            pos.y = target.position.y + offset.y;
        if (axes.followZ)
            pos.z = target.position.z + offset.z;

        transform.position = Vector3.Lerp(transform.position, pos, easing.Evaluate(smoothing));
    }

    /// <summary>
    /// Set the proper default values
    /// </summary>
    private void Reset()
    {
        easing.AddKey(0, 1);
        easing.AddKey(1, 1);
    }

    /// <summary>
    /// Handles auto offsetting and prevents illegal values
    /// </summary>
    private void OnValidate()
    {
        smoothing = Mathf.Clamp01(smoothing);

        if (autoOffset && target != null && _oldTarget != target)
        {
            offset = (transform.position - target.position).normalized * Vector3.Distance(target.position, transform.position);
            autoOffset = false;
        }

        _oldTarget = target;
    }
}
