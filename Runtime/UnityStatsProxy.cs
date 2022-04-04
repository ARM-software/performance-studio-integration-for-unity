/**
 * SPDX-License-Identifier: BSD-3-Clause
 *
 * Copyright (c) 2022, Arm Limited
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * 3. Neither the name of the copyright holder nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

// Only active in Android Players
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
