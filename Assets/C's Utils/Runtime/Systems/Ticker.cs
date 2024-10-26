/* Ticker.cs - C's Utils
 * 
 * A tick system useful for optimization
 * 
 * 
 * Creation Date: 10/05/2024
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [10/05/2024] - Initial implementation (C137)
 *      [22/07/2024] - Proper singleton implementation (C137)
 *      [28/07/2024] - Added singleton check when unsubscribing ticks (C137)
 *      [26/10/2024] - Added support for non-integer tick rates (C137)
 *      
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CsUtils.Systems.Tick
{
    public class Ticker : MonoBehaviour
    {
        public class Tickable
        {
            /// <summary>
            /// The callback for this tickable
            /// </summary>
            public Action callback;

            /// <summary>
            /// The tick rate at which to execute the callback
            /// </summary>
            public float tickRate;

            /// <summary>
            /// The last tick at which the callback was called
            /// </summary>
            public float lastTick;

            public Tickable(Action callback, float tickRate)
            {
                this.callback = callback;
                this.tickRate = tickRate;

                lastTick = 0;
            }
        }

        /// <summary>
        /// How many ticks should be done in one second
        /// </summary>
        public float globalTickRate = 60;

        /// <summary>
        /// The maximum value that the elapsed tick will reach before resetting
        /// </summary>
        private float maxTickRate;
        
        /// <summary>
        /// The amount of ticks that have elapsed so far
        /// </summary>
        public float elapsedTicks;

        /// <summary>
        /// Reference to all the tickables subscribed
        /// </summary>
        List<Tickable> tickables = new();

        private void Awake()
        {
            Singleton.Create(this);
        }

        private void Start()
        {
            maxTickRate = globalTickRate * 10;
        }

        public void Update()
        {
            UpdateTick();

            CallTicks();
        }

        /// <summary>
        /// Executes the callbacks for each appropriate tickable
        /// </summary>
        void CallTicks()
        {
            // Ensure that elapsedTicks wraps to 0 if it exceeds the maximum value 
            float wrappedElapsedTicks = elapsedTicks % maxTickRate;

            if (Mathf.Approximately(wrappedElapsedTicks, 0f))
                return;

            foreach (var tickable in tickables)
            {
                float tickDifference = wrappedElapsedTicks >= tickable.lastTick
                    ? wrappedElapsedTicks - tickable.lastTick
                    : (wrappedElapsedTicks + maxTickRate) - tickable.lastTick;

                // Skip if the difference is less than tickRate
                if (tickDifference < tickable.tickRate)
                    continue;

                // Update lastTick and call the callback function
                tickable.lastTick = wrappedElapsedTicks;
                tickable.callback();
            }
        }

        /// <summary>
        /// Updates the elapsed ticks
        /// </summary>
        void UpdateTick()
        {
            elapsedTicks += 60 * Time.deltaTime;
            
            if (elapsedTicks >= maxTickRate)
                elapsedTicks = 0;
        }
        

        /// <summary>
        /// Subscribe a callback to the tick system
        /// </summary>
        /// <param name="tickRate">The tick rate at which the callback should be called<br>Cannot be more than 10x the global tick rate</br></param>
        /// <param name="callback">The callback to execute at the given tick rate</param>
        public static void SubscribeTick(float tickRate, Action callback)
        {
            Singleton.Get<Ticker>().tickables.Add(new(callback, tickRate));
        }

        public static void UnSubscribeTick(Action callback)
        {
            if (Singleton.HasInstance<Ticker>())
                Singleton.Get<Ticker>().tickables.RemoveAll((t) => t.callback == callback);
        }

        private void OnDestroy()
        {
            Singleton.Remove(this);
        }
    }
}