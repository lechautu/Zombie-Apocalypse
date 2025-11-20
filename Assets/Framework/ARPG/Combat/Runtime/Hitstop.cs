// ARPG/Combat/Hitstop.cs
using UnityEngine;
using System.Collections;

namespace ARPG.Combat
{
    public static class Hitstop
    {
        static bool _busy;
        public static void Do(float seconds)
        {
            if (_busy || seconds <= 0f) return;
            var runner = new GameObject("~Hitstop").AddComponent<HitstopRunner>();
            Object.DontDestroyOnLoad(runner.gameObject);
            runner.Begin(seconds);
        }

        class HitstopRunner : MonoBehaviour
        {
            public void Begin(float s) => StartCoroutine(Run(s));
            IEnumerator Run(float s)
            {
                _busy = true;
                float prev = Time.timeScale;
                Time.timeScale = 0f;
                yield return new WaitForSecondsRealtime(s);
                Time.timeScale = prev;
                _busy = false;
                Destroy(gameObject);
            }
        }
    }
}
