/* AxisFollow.cs - C's Utils
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
 *      [16/12/2023] - Fixed script name in the meta data (C137)
 *                   - Removed unnecessary using statements (C137)
 *      
 *      [18/12/2023] - Fixed animation curve being null on Reset() (C137)
 *                   - Fixed easing not following smoothing (C137)
 *                   
 *      [25/12/2023] - Fixed null reference exception (C137)
 *                   - Smoothing is now done per axis (C137)
 *                   
 *      [08/01/2024] - Improved automatic offset calculation (C137)
 *                   
 * TODO:
 *      Add a button to set the offset automatically (C137)
 */
using System;
using UnityEngine;
namespace CsUtils
{
    [Serializable]
    public struct AxisInfo
    {
        /// <summary>
        /// Which of the axes to follow
        /// </summary>
        public bool followX, followY, followZ;

        /// <summary>
        /// The axes to apply smoothing on
        /// </summary>
        public SmoothingAxes smoothAxes;
    }

    [Serializable]
    public struct SmoothingAxes
    {
        /// <summary>
        /// Which of the axes to apply smoothing on
        /// </summary>
        public bool smoothX, smoothY, smoothZ;
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
        [Range(0f, 1f)]
        public float smoothing = 1;

        /// <summary>
        /// The easing curve to use
        /// </summary>
        public AnimationCurve easing;

        /// <summary>
        /// The offset by which to follow the target
        /// </summary>
        public Vector3 offset;

        /// <summary>
        /// Information about the following of the axes
        /// </summary>
        public AxisInfo axes;

        /// <summary>
        /// Whether to automatically set the offset when a new target is added
        /// </summary>
        public bool autoOffset = true;

        /// <summary>
        /// Whether to use the easing for smoothing
        /// </summary>
        public bool useEasing;

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

            if(axes.followX)
                pos.x = target.position.x + offset.x;
            if(axes.followY)
                pos.y = target.position.y + offset.y;
            if(axes.followZ)
                pos.z = target.position.z + offset.z;

            transform.position = new Vector3(
                axes.smoothAxes.smoothX ? Mathf.Lerp(transform.position.x, pos.x, easing.Evaluate(smoothing) * Time.timeScale) : pos.x,
                axes.smoothAxes.smoothY ? Mathf.Lerp(transform.position.y, pos.y, easing.Evaluate(smoothing) * Time.timeScale) : pos.y,
                axes.smoothAxes.smoothZ ? Mathf.Lerp(transform.position.z, pos.z, easing.Evaluate(smoothing) * Time.timeScale) : pos.z);
        }

        /// <summary>
        /// Set the proper default values
        /// </summary>
        private void Reset()
        {
            easing = new AnimationCurve();
            easing.AddKey(0, 1);
            easing.AddKey(1, 1);
        }

        /// <summary>
        /// Handles auto offsetting and prevents illegal values
        /// </summary>
        private void OnValidate()
        {
            smoothing = Mathf.Clamp01(smoothing);

            if(autoOffset && target != null && _oldTarget != target)
            {
                offset = transform.position - target.position;
                autoOffset = false;
            }

            _oldTarget = target;

            if(useEasing)
            {
                Keyframe[] keys = easing.keys;

                keys[^1] = new Keyframe(1, smoothing);

                easing.keys = keys;
            }
            else
            {
                easing ??= new AnimationCurve();
                easing.ClearKeys();
                easing.AddKey(0, smoothing);
                easing.AddKey(1, smoothing);
            }
        }
    }
}