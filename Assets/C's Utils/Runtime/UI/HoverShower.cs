using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CsUtils
{
    public class HoverShower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// The object to show on hover
        /// </summary>
        public Transform show;

        /// <summary>
        /// The previous show object, used for automatically finding the canvas
        /// </summary>
        private Transform oldShow;

        /// <summary>
        /// The parent canvas of the object to show
        /// </summary>
        public Canvas showParentCanvas;

        /// <summary>
        /// How long should the hover be before the object is shown
        /// </summary>
        public float delay;

        /// <summary>
        /// Whether the shown transform should be dragged
        /// </summary>
        public bool doDragging;
        
        /// <summary>
        /// The offset to drag/show the transform by
        /// </summary>
        public Vector2 offset;
        
        /// <summary>
        /// The delayed called use to show the object
        /// </summary>
        public Action delayedCall;

        /// <summary>
        /// Shows the hover object and starts dragging it
        /// </summary>
        public void Show()
        {
            show.gameObject.SetActive(true);
            
            if(doDragging)
                StaticUtils.Drag(show);
        }

        public void Hide()
        {
            if(doDragging)
                StaticUtils.StopDrag(transform);
            
            show.gameObject.SetActive(false);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(delay == 0)
                Show();
            else
                delayedCall = StaticUtils.DelayedCall(delay, Show);
        }
        
        
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if(delayedCall != null)
                StaticUtils.CancelDelayedCall(delayedCall);

            Hide();
        }

        private void OnValidate()
        {
            if(show == null || oldShow == show)
            {
                oldShow = show;
                return;
            }

            showParentCanvas = showParentCanvas == null ? SearchForCanvas() : showParentCanvas;

            oldShow = show;
            
            Canvas SearchForCanvas()
            {
                Transform parent = show.parent;

                while (parent is not null)
                {
                    if(parent.TryGetComponent(out Canvas canvas))
                        return canvas;

                    parent = parent.parent;
                }
                return null;
            }
        }
    }
}
