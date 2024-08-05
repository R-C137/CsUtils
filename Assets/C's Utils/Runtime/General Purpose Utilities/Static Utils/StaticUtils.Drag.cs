/* StaticUtils.Drag.cs - C's Utils
 *
 * QoL class to handle dragging UI on the screen. Can also be used to create a cursor
 *
 *
 * Creation Date: 05/08/2024
 * Authors: C137
 * Original: C137
 *
 * Edited By: C137
 *
 * Changes:
 *      [05/08/2024] - Initial implementation (C137)
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CsUtils
{
    public struct DragData
    {
        /// <summary>
        /// The canvas used to display the dragged UI element
        /// </summary>
        public Canvas parentCanvas;

        /// <summary>
        /// The offset by which to drag the UI element
        /// </summary>
        public Vector2 offset;

        /// <summary>
        /// The id of the touch which this drag is assigned
        /// </summary>
        public int? touchID;
    }
    
    public static partial class StaticUtils
    {
        private static DragHandler dragHandler;

        /// <summary>
        /// Moves a UI element to the mouse position everything, giving a drag effect
        /// </summary>
        /// <param name="transform">The transform to be moved</param>
        /// <param name="parentCanvas">The canvas in which the transform is found. Will search for one if unspecified</param>
        /// <param name="offset">The offset by which to move the transform to the mouse position</param>
        /// <param name="touchID">The touch id to drag the UI element with. Set null to use the mouse position</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static bool Drag(Transform transform, Canvas parentCanvas = null, Vector2? offset = null, int? touchID = null)
        {
            offset ??= Vector2.zero;

            parentCanvas ??= SearchForCanvas();

            if(parentCanvas == null)
                throw new ArgumentException("The given transform is not child of a canvas", nameof(transform));

            if(dragHandler == null)
            {
                dragHandler = (CsSettings.csUtilsGameobject ??= new GameObject("CsUtils~")).AddComponent<DragHandler>();
                UnityEngine.Object.DontDestroyOnLoad(dragHandler);
            }

            return dragHandler.itemsToDrag.TryAdd(transform, new DragData { parentCanvas = parentCanvas, offset = offset.Value, touchID = touchID});
            
            Canvas SearchForCanvas()
            {
                Transform parent = transform.parent;

                while (parent is not null)
                {
                    if(parent.TryGetComponent(out Canvas canvas))
                        return canvas;

                    parent = parent.parent;
                }
                return null;
            }
        }

        /// <summary>
        /// Checks whether a UI element is currently being dragged
        /// </summary>
        /// <param name="transform">The UI element to check</param>
        /// <returns></returns>
        public static bool IsDragging(Transform transform)
        {
            if(dragHandler == null)
                return false;

            return dragHandler.itemsToDrag.ContainsKey(transform);
        }

        /// <summary>
        /// Stops dragging a UI element
        /// </summary>
        /// <param name="transform">The UI element to stop dragging</param>
        public static void StopDrag(Transform transform)
        {
            if(dragHandler != null)
                dragHandler.itemsToDrag.Remove(transform);
        }
    }

    public class DragHandler : MonoBehaviour
    {
        public Dictionary<Transform, DragData> itemsToDrag = new ();

        public void Update()
        {
            DoDrag();
        }

        void DoDrag()
        {
            foreach (var kvp in itemsToDrag)
            {
                DragData drag = kvp.Value;

                Vector2 mousePos = drag.touchID == null ? Input.mousePosition : Input.GetTouch(drag.touchID.Value).position;
                StaticUtils.MoveUIToMouse(kvp.Key, drag.parentCanvas, mousePos, drag.offset);
            }
        }
    }
}
