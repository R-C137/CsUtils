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
 *
 *
 * TODO:
 *      Add support for automatically destroying the context menu if the player clicks away
 */

using System;
using System.Collections.Generic;
using CsUtils.Extensions;
using TMPro;
using UnityEngine;
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
        
        /// <summary>
        /// Adds a new option to the context menu
        /// </summary>
        /// <param name="callback">The callback if this action is selected</param>
        /// <param name="text">The text to display for this option</param>
        /// <param name="color">The color of this option</param>
        public ContextMenuBuilder WithOption(Action callback, string text, Color? color = null)
        {
            options.Add(new OptionData(callback, text, color ?? Color.white));
            return this;
        }

        /// <summary>
        /// Displays the context menu at the mouse position
        /// <param name="offset">Offset from the mouse position</param>
        /// </summary>
        public void Build(Vector3? mousePosition = null, Vector3? offset = null)
        {
            var contextMenuData = Singleton.Get<CsSettings>().contextMenuData;
            GameObject contextMenu = Object.Instantiate(contextMenuData.contextMenuPrefab);

            Transform optionsParent = contextMenu.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0);

            foreach (var option in options)
            {
                var optionObject = Object.Instantiate(contextMenuData.optionPrefab, optionsParent, false);
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
        }
    }
}
