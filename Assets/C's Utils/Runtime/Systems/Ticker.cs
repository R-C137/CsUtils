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
            public int tickRate;

            /// <summary>
            /// The last tick at which the callback was called
            /// </summary>
            public int lastTick;

            public Tickable(Action callback, int tickRate)
            {
                this.callback = callback;
                this.tickRate = tickRate;

                lastTick = 0;
            }
        }

        /// <summary>
        /// How many ticks should be done in one second
        /// </summary>
        public int globalTickRate = 60;

        /// <summary>
        /// The amount of ticks that have elapsed so far
        /// </summary>
        public float elapsedTicks;

#pragma warning disable IDE0044 // Add readonly modifier
        /// <summary>
        /// Reference to all the tickables subscribed
        /// </summary>
        List<Tickable> tickables = new();
#pragma warning restore IDE0044 // Add readonly modifier

        private void Awake()
        {
            Singleton.Create(this);
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
            //If a the ticker has just started or has been reset, no tickable should be called
            if (Mathf.RoundToInt(elapsedTicks) == 0)
                return;

            foreach (var tickable in tickables)
            {

                if (tickable.lastTick != Mathf.RoundToInt(elapsedTicks) && Mathf.RoundToInt(elapsedTicks % tickable.tickRate) == 0)
                {
                    tickable.lastTick = Mathf.RoundToInt(elapsedTicks);
                    tickable.callback();
                }
            }
        }

        /// <summary>
        /// Updates the elapsed ticks
        /// </summary>
        void UpdateTick()
        {
            if (elapsedTicks >= globalTickRate * 10)
                elapsedTicks = 0;

            elapsedTicks += 60 * Time.deltaTime;
        }

        /// <summary>
        /// Subscribe a callback to the tick system
        /// </summary>
        /// <param name="tickRate">The tick rate at which the callback should be called<br>Cannot be more than 10x the global tick rate</br></param>
        /// <param name="callback">The callback to execute at the given tick rate</param>
        public static void SubscribeTick(int tickRate, Action callback)
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