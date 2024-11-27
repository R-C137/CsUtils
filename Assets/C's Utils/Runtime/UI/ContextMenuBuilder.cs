/* ContextMenu.cs - C's Utils
 *
 * Handles the behaviour for a context menu
 *
 *
 * Creation Date: 31/07/2024
 * Authors: C137
 * Original: C137
 *
 * Edited By: C137
 *
 * Changes:
 *      [31/07/2024] - Initial implementation (C137)
 *      [22/11/2024] - Added support for default values from 'CsSettings' (C137)
 *      [27/11/2024] - A reference to the spawned context menu is now returned when created a context menu (C137)
 *                   - Improved parameter ordering (C137)
 *                   - Support for destroying context menu when a specific key is pressed (C137)
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CsUtils.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CsUtils
{
    public class ContextMenuBuilder
    {
        public struct OptionData
        {
            /// <summary>
            /// The callback if this option is selected
            /// </summary>
            public Action callback;
            
            /// <summary>
            /// The text to display for this option
            /// </summary>
            public string text;

            /// <summary>
            /// The background color of this option
            /// </summary>
            public Color? color;

            public OptionData(Action callback, string text, Color? color)
            {
                this.callback = callback;
                this.text = text;
                this.color = color;
            }
        }
        
        public List<OptionData> options = new();

        public List<KeyCode> cancelKeys = new();

        public int cancelTouchId;
        
        /// <summary>
        /// Adds a new option to the context menu
        /// </summary>
        /// <param name="text">The text to display for this option</param>
        /// <param name="callback">The callback if this action is selected</param>
        /// <param name="color">The color of this option</param>
        public ContextMenuBuilder WithOption(string text, Action callback, Color? color = null)
        {
            options.Add(new OptionData(callback, text, color ?? Color.white));
            return this;
        }

        /// <summary>
        /// Sets the keys that will cancel the context menu if pressed<br></br>
        /// Useful to make it so that clicking outside of the context menu will destroy it
        /// </summary>
        /// <param name="keys">The keys that will cancel the context menu</param>
        public ContextMenuBuilder WithCancelKeys(params KeyCode[] keys)
        {
            cancelKeys.AddRange(keys);
            return this;
        }

        /// <summary>
        /// Sets the touch id that will cancel the context menu if pressed<br></br>
        /// Useful to make it so that pressing outside of the context menu will destroy it
        /// </summary>
        /// <param name="touchId">ID of the cancelling touch. Set to -1 to allow for any touch</param>
        public ContextMenuBuilder WithCancelTouchId(int touchId)
        {
            cancelTouchId = touchId;
            return this;
        }
        
        /// <summary>
        /// Displays the context menu at the mouse position
        /// <param name="mousePosition">The mouse position in screen space to spawn the context menu</param>
        /// <param name="offset">Offset from the mouse position</param>
        /// <param name="allowCancel">Whether to alllow the context menu to be cancelled</param>
        /// </summary>
        /// <returns>Gameobject reference to the created context menu</returns>
        public GameObject Build(Vector3? mousePosition = null, Vector3? offset = null, bool allowCancel = true)
        {
            Singleton.TryGet(out CsSettings csSettings);
            GameObject contextMenu = Object.Instantiate(csSettings.contextMenuPrefab);

            Transform optionsParent = contextMenu.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0);

            foreach (var option in options)
            {
                var optionObject = Object.Instantiate(csSettings.optionPrefab, optionsParent, false);
                optionObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    option.callback?.Invoke();
                    Object.Destroy(contextMenu);
                });

                optionObject.transform.GetChild(0).GetComponent<Image>().color = option.color ?? Color.white;
                optionObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = option.text;
            }

            Transform contextMenuParent = contextMenu.transform.GetChild(0);

            Vector3 topLeftOffset = new Vector3((contextMenuParent as RectTransform).sizeDelta.x / 2,
                (contextMenuParent as RectTransform).sizeDelta.y / -2) + new Vector3(15, -5);

            StaticUtils.MoveUIToMouse(contextMenuParent, contextMenu.GetComponent<Canvas>(),
                mousePosition ?? Input.mousePosition, topLeftOffset + (offset ?? Vector3.zero));

            if(allowCancel)
            {
                ContextMenuCanceller destroyer = contextMenu.AddComponent<ContextMenuCanceller>();

                if(cancelKeys.Any())
                    destroyer.cancelKeys = cancelKeys.ToArray();
                
                destroyer.cancelTouchId = cancelTouchId;
            }
            
            return contextMenu;
        }
    }

    public class ContextMenuCanceller : MonoBehaviour
    {
        /// <summary>
        /// The keys that will cancel the context menu
        /// </summary>
        public KeyCode[] cancelKeys = { KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Escape };

        /// <summary>
        /// The id of the touch that will cancel the context menu
        /// </summary>
        public int cancelTouchId;
        
        private void Update()
        {
            if(InputQuery.GetAnyKeyUp(cancelKeys) || (cancelTouchId == -1 ? Input.touches.Length > 0 : Input.touches.Any(t => t.fingerId == cancelTouchId) ))
                Destroy(gameObject);
            
        }
    }
}
