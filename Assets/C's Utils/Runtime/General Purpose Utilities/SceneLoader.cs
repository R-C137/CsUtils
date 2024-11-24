/* SceneLoader.cs - C's Utils
 * 
 * A scene loading utility with a simple loading animation
 * 
 * 
 * Creation Date: 06/12/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [06/12/2023] - Initial implementation (C137)
 *      [09/12/2023] - Removed unnecessary using statements (C137)
 *      [01/05/2024] - Fixed 'DontDestroyOnLoad()' warning when this object is parented (C137)
 */
using CsUtils;
using CsUtils.UI;
using System;
using System.Collections;
using CsUtils.Modules.LeanTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CsUtils
{
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// Whether a scene is currently being loaded
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Fader used for the background
        /// </summary>
        public MultiFade backgroundFader;

        /// <summary>
        /// Fader used for the loading bar
        /// </summary>
        public MultiFade loadingBarFader;

        /// <summary>
        /// The slider for the loading bar
        /// </summary>
        public Slider loadingBarSlider;

        /// <summary>
        /// After how long should the loading bar display if the scene is still loading
        /// </summary>
        public float loadingBarDisplayTimeout = 1.5f;

        /// <summary>
        /// How should should the scene activation be delayed by
        /// </summary>
        public float loadingDelay = 0f;

        /// <summary>
        /// How long should the fade in animation take
        /// </summary>
        public float fadeInTime = .5f;

        /// <summary>
        /// How long should the fade out animation take
        /// </summary>
        public float fadeOutTIme = .5f;

        /// <summary>
        /// The id of the tween handling the fading animation
        /// </summary>
        int fadingTween;

        private void Awake()
        {
            transform.SetParent(null);
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// Loads a scene with a simple animation
        /// </summary>
        /// <param name="sceneBuildIndex">The index of the scene to load</param>
        /// <param name="parameters">The parameters to load the scene with</param>
        public void LoadScene(int sceneBuildIndex, LoadSceneParameters parameters)
        {
            if(!isLoading)
                StartCoroutine(LoadSceneAsync(sceneBuildIndex, parameters));
            else
                CsSettings.Logger.LogDirect("[SceneLoader] Cannot load scene with ID:{0} as another one is currently being loaded", CsUtils.Systems.Logging.LogSeverity.Warning, parameters: sceneBuildIndex);
        }
        /// <summary>
        /// Loads a scene with a simple animation
        /// </summary>
        /// <param name="sceneName">The name of the scene to load</param>
        /// <param name="mode">The mode to load the scene with</param>
        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single) => LoadScene(SceneManager.GetSceneByName(sceneName).buildIndex, new LoadSceneParameters { loadSceneMode = mode });
        /// <summary>
        /// Loads a scene with a simple animation
        /// </summary>
        /// <param name="sceneBuildIndex">The index of the scene to load</param>
        /// <param name="mode">The mode to load the scene with</param>
        public void LoadScene(int sceneBuildIndex, LoadSceneMode mode = LoadSceneMode.Single) => LoadScene(sceneBuildIndex, new LoadSceneParameters { loadSceneMode = mode });

        public IEnumerator LoadSceneAsync(int sceneBuildIndex, LoadSceneParameters parameters)
        {
            isLoading = true;

            //Enables and sets the background alpha to 0
            backgroundFader.SetAlpha(0f);
            backgroundFader.gameObject.SetActive(true);

            //Do the fade in animation
            DoFading(0, 1, fadeInTime);

            //Wait for the fade in animation to finish
            yield return new WaitForSeconds(fadeInTime);

            //Do the scene loading after the fade in so as to prevent stuttering
            var operation = SceneManager.LoadSceneAsync(sceneBuildIndex, parameters);

            //Handles the delaying of the scene loading (Unity Editor Only)
#if UNITY_EDITOR
            operation.allowSceneActivation = false;
            LeanTween.delayedCall(loadingDelay, () => operation.allowSceneActivation = true);
#endif
            //Display the loading bar if the loading takes too long
            LeanTween.delayedCall(loadingBarDisplayTimeout, () =>
            {
                if(operation.isDone)
                    return;

                loadingBarFader.gameObject.SetActive(true);
                loadingBarFader.SetAlpha(1f);
            });

            //Display the loading progress in the loading bar
            while (operation.progress <= .9f)
            {
                loadingBarSlider.value = Mathf.Clamp01(operation.progress / .9f);
                yield return null;
            }

            //Do the fading out
            DoFading(1, 0, fadeOutTIme, () => Destroy(gameObject));
            isLoading = false;
        }

        public void DoFading(float from, float to, float time, Action onComplete = null)
        {
            LeanTween.cancel(fadingTween);

            fadingTween = LeanTween.value(from, to, time)
                .setOnUpdate((v) => MultiFade.SetAlpha(v, backgroundFader, loadingBarFader))
                .setOnComplete(onComplete)
                .uniqueId;
        }
    }
}