// Only active in Androd Players
#if UNITY_ANDROID && !UNITY_EDITOR

using System.Collections.Generic;
using System.Diagnostics;
using Unity.Profiling;
using UnityEngine;

namespace MobileStudio
{
    class UnityStatsProxyScript : MonoBehaviour
    {
        struct CounterObjectPair
        {
            public CounterObjectPair(Annotations.Counter mobileStudioCounter, ProfilerRecorder unityCounter, bool convertToMiB)
            {
                this.mobileStudioCounter = mobileStudioCounter;
                this.unityCounter = unityCounter;
                this.convertToMiB = convertToMiB;
            }

            public Annotations.Counter mobileStudioCounter;
            public ProfilerRecorder unityCounter;
            public bool convertToMiB;
        }
        List<CounterObjectPair> counters;

        struct CounterMapping
        {
            public CounterMapping(string mobileStudioTitle, string mobileStudioCounterName, string mobileStudioCounterUnit, string unityCounterName)
            {
                this.mobileStudioTitle = mobileStudioTitle;
                this.mobileStudioCounterName = mobileStudioCounterName;
                this.mobileStudioCounterUnit = mobileStudioCounterUnit;
                this.unityCounterName = unityCounterName;
            }

            public string mobileStudioTitle;
            public string mobileStudioCounterName;
            public string mobileStudioCounterUnit;
            public string unityCounterName;
        }

        // Memory counters are captured based on https://docs.unity3d.com/Manual/ProfilerMemory.html information.
        static readonly string memoryStatsTitle = "Unity Memory Usage";
        static readonly string objectStatsTitle = "Unity Objects";
        static readonly string unitBytes = "MiB";
        static readonly string unitObjects = "objects";
        const double toMiB = 1.0 / (1024.0 * 1024.0);
        static readonly CounterMapping[] mobileStudioDefaultCounters =
        {
            new CounterMapping(memoryStatsTitle, "Total Memory Usage", unitBytes, "Total Reserved Memory"),
            new CounterMapping(memoryStatsTitle, "Scripting Memory Usage", unitBytes, "GC Reserved Memory"),
            // Development player memory stats
            new CounterMapping(memoryStatsTitle, "Graphics Memory Usage", unitBytes, "Gfx Reserved Memory"),
            // Development player object stats
            new CounterMapping(objectStatsTitle, "Object Count", unitObjects, "Object Count"),
            new CounterMapping(objectStatsTitle, "Game Object Count", unitObjects, "Game Object Count"),
            new CounterMapping(objectStatsTitle, "Texture Count", unitObjects, "Texture Count"),
            new CounterMapping(objectStatsTitle, "Mesh Count", unitObjects, "Mesh Count"),
            new CounterMapping(objectStatsTitle, "Material Count", unitObjects, "Material Count"),
            new CounterMapping(objectStatsTitle, "AnimationClip Count", unitObjects, "AnimationClip Count"),
        };

        [RuntimeInitializeOnLoadMethod]
        [Conditional("UNITY_ANDROID")]
        static void OnRuntimeMethodLoad()
        {
            // Add a support object for counters synchronisation
            var go = new GameObject("MobileStudioUnityStatsProxy", typeof(UnityStatsProxyScript));
            DontDestroyOnLoad(go);
        }

        void Awake()
        {
            // Map Unity counters to Mobile Studio
            counters = new List<CounterObjectPair>();
            for (var i = 0; i < mobileStudioDefaultCounters.Length; ++i)
            {
                var recorder = new ProfilerRecorder(mobileStudioDefaultCounters[i].unityCounterName, 0);
                if (!recorder.Valid)
                {
                    recorder.Dispose();
                    continue;
                }

                var mobileStudioCounter = new Annotations.Counter(mobileStudioDefaultCounters[i].mobileStudioTitle, mobileStudioDefaultCounters[i].mobileStudioCounterName, Annotations.CounterType.Absolute, mobileStudioDefaultCounters[i].mobileStudioCounterUnit);
                counters.Add(new CounterObjectPair(mobileStudioCounter, recorder, mobileStudioDefaultCounters[i].mobileStudioCounterUnit == unitBytes));
            }
        }

        void OnDestroy()
        {
            // Cleanup Unity counters
            for (var i = 0; i < counters.Count; ++i)
            {
                counters[i].unityCounter.Dispose();
            }
        }

        void Update()
        {
            // Report Unity counters to MobileStudio on a frame basis
            for (var i = 0; i < counters.Count; ++i)
            {
                var value = counters[i].unityCounter.CurrentValueAsDouble;
                float valueToReport = counters[i].convertToMiB ? (float)(value * toMiB) : (float)value;
                counters[i].mobileStudioCounter.set_value(valueToReport);
            }
        }
    }
}

#endif
