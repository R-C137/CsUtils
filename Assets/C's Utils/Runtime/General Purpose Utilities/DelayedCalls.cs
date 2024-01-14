/* DelayedCalls.cs - C's Utils
 * 
 * Executes code at delay for specific time in the frame
 * 
 * 
 * Creation Date: 10/01/2024
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [10/01/2024] - Initial implementation (C137)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CsUtils
{
    public enum ExecutionOrder
    {
        FixedUpdate,
        Update,
        LateUpdate,
        PreCull,
        PreRender,
        Postrender,
        RenderImage,
    }

    struct ExecutionDelay
    {
        /// <summary>
        /// At what execution order should it execute
        /// </summary>
        internal ExecutionOrder executionOrder;

        /// <summary>
        /// After how many seconds should the function be called
        /// </summary>
        internal float delay;

        public ExecutionDelay(float delay, ExecutionOrder executionOrder)
        {
            this.executionOrder = executionOrder;
            this.delay = delay;
        }
    }

    public static partial class StaticUtils
    {
        /// <summary>
        /// The monobehaviour handling the delay system
        /// </summary>
        static CallDelay delayHandler;

        /// <summary>
        /// All of the delayed calls to be made
        /// </summary>
        internal static Dictionary<Action, ExecutionDelay> delayedCalls = new();

        /// <summary>
        /// The next id used to singularize the delayed calls
        /// </summary>
        static int callbackID;

        /// <summary>
        /// Calls a function after a set delay
        /// </summary>
        /// <param name="delay">How long in seconds should the delay be</param>
        /// <param name="callback">The function to call</param>
        /// <param name="executionOrder">At which execution order should the function be called</param>
        public static void DelayedCall(float delay, Action callback, ExecutionOrder executionOrder = ExecutionOrder.Update)
        {
            if (delayHandler == null)
            {
                delayHandler = new GameObject("CsUtils~").AddComponent<CallDelay>();
                UnityEngine.Object.DontDestroyOnLoad(delayHandler);
            }

            delayedCalls.Add(SingularizeAction(callback), new ExecutionDelay(delay, executionOrder));

            Action SingularizeAction(Action action)
            {
                return () =>
                {
                    int _ = callbackID += 1;
                    action();
                };
            }
        }
    }

    public class CallDelay : MonoBehaviour
    {
        /// <summary>
        /// Calls that have reached their delay and waiting to be called
        /// </summary>
        public Dictionary<Action, ExecutionOrder> pendingCalls = new();

        private void FixedUpdate()
        {
            ExecuteForOrder(ExecutionOrder.FixedUpdate);
        }

        private void Update()
        {
            DoDelayDecay();

            ExecuteForOrder(ExecutionOrder.Update);
        }

        private void LateUpdate()
        {
            ExecuteForOrder(ExecutionOrder.LateUpdate);
        }

        private void OnPreCull()
        {
            ExecuteForOrder(ExecutionOrder.PreCull);
        }

        private void OnPreRender()
        {
            ExecuteForOrder(ExecutionOrder.PreRender);
        }

        private void OnPostRender()
        {
            ExecuteForOrder(ExecutionOrder.Postrender);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            ExecuteForOrder(ExecutionOrder.RenderImage);
        }

        void ExecuteForOrder(ExecutionOrder order)
        {
            var calls = pendingCalls.Where((kv) => kv.Value == order).ToArray();

            foreach (var call in calls)
            {
                call.Key?.Invoke();
                pendingCalls.Remove(call.Key);
            }
        }
        void DoDelayDecay()
        {
            foreach (Action action in StaticUtils.delayedCalls.Keys.ToArray())
            {
                ExecutionDelay executionDelay = StaticUtils.delayedCalls[action];

                float newDelay = executionDelay.delay - Time.deltaTime;

                if(newDelay <= 0)
                {
                    pendingCalls.Add(action, executionDelay.executionOrder);
                    StaticUtils.delayedCalls.Remove(action);
                    continue;
                }

                ExecutionDelay newExecutionDelay = new()
                {
                    delay = executionDelay.delay - Time.deltaTime,
                    executionOrder = executionDelay.executionOrder
                };

                StaticUtils.delayedCalls[action] = newExecutionDelay;
            }
        }
    }
}