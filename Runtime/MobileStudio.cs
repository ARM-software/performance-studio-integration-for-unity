/**
 * SPDX-License-Identifier: BSD-3-Clause
 *
 * Copyright (c) 2021, Arm Limited
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

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MobileStudio
{
    public class Annotations
    {
        // Maintain global UID for each Custom Activity Map.
        private static UInt32 globalCamView = 1;

        // Global state as to whether annotations are available for use.
        private enum AnnotationState { Active, Inactive };

        // Counter type
        public enum CounterType { Absolute, Delta };

        private static AnnotationState state = getAnnotationState();

#if UNITY_ANDROID && !UNITY_EDITOR
        [DllImport("mobilestudio")]
        private static extern void gator_annotate_setup();

        [DllImport("mobilestudio")]
        private static extern void gator_annotate_marker(
            string str);

        [DllImport("mobilestudio")]
        private static extern void gator_annotate_marker_color(
            UInt32 color, string str);

        [DllImport("mobilestudio")]
        private static extern void gator_annotate_color(
            UInt32 channel, UInt32 color, string str);

        [DllImport("mobilestudio")]
        private static extern void gator_annotate_str(
            UInt32 channel, string str);

        [DllImport("mobilestudio")]
        private static extern void gator_annotate_name_channel(
            UInt32 channel, UInt32 group, string str);

        [DllImport("mobilestudio")]
        private static extern void gator_cam_view_name(
            UInt32 view_uid, string name);

        [DllImport("mobilestudio")]
        private static extern void gator_cam_track(
            UInt32 view_uid, UInt32 track_uid, UInt32 parent_track, string name);

        [DllImport("mobilestudio")]
        private static extern UInt64 gator_get_time();

        [DllImport("mobilestudio")]
        private static extern void gator_cam_job_start(
            UInt32 view_uid, UInt32 job_uid, string name, UInt32 track, UInt64 time, UInt32 color);

        [DllImport("mobilestudio")]
        private static extern void gator_cam_job_stop(
            UInt32 view_uid, UInt32 job_uid, UInt64 time);

        [DllImport("mobilestudio")]
        private static extern void gator_cam_job(
            UInt32 view_uid, UInt32 job_uid, string name, UInt32 track, UInt64 startTime,
            UInt64 duration, UInt32 color, UInt32 primaryDependency, IntPtr dependencyCount,
            IntPtr dependencies);

        [DllImport("mobilestudio")]
        private static extern void gator_annotate_counter(
            UInt32 counter_id, string title, string name, UInt32 per_cpu, UInt32 counter_class,
            UInt32 display_class, string units, UInt32 modifier, UInt32 display_composition,
            UInt32 display_renderer, UInt32 avg_selection, UInt32 avg_cores, UInt32 percentage,
            IntPtr activity_count, IntPtr activity_names, IntPtr activity_colors, UInt32 cores,
            UInt32 color, string description);

        [DllImport("mobilestudio")]
        private static extern void gator_annotate_counter_value(
            UInt32 cpu, UInt32 counter_id, UInt32 value);

#endif

        /*
         * Converts a Unity Color32 into a 32-bit Int used by gator to represent
         * the color. Gator's format is little-endian with a 0x1b escape code.
         */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt32 colorToGatorInt(Color32 color)
        {
            UInt32 colorInt = ((uint)(color.b) << 24) +
                             ((uint)(color.g) << 16) +
                             ((uint)(color.r) << 8) +
                             ((uint)0x1b);

            return colorInt;
        }

        /*
         * Returns the active state if annotations are supported (we are running on Android
         * and successfully initialized the library), inactive otherwise
         */
        private static AnnotationState getAnnotationState()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    gator_annotate_setup();
                    return AnnotationState.Active;
                }
                catch (System.EntryPointNotFoundException)
                {
                    return AnnotationState.Inactive;
                }
            #else
                return AnnotationState.Inactive;
            #endif
        }

        /*
         * Emit a simple marker that is displayed along the top of Streamline's
         * timeline.
         */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_ANDROID")]
        public static void marker(string str)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                if (state == AnnotationState.Active)
                {
                    gator_annotate_marker(str);
                }
            #endif
        }

        /*
         * Emit a colored marker that is displayed along the top of Streamline's
         * timeline.
         */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_ANDROID")]
        public static void marker(string str, Color32 color)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                if (state == AnnotationState.Active)
                {
                    UInt32 col = colorToGatorInt(color);
                    gator_annotate_marker_color(col, str);
                }
            #endif
        }

        /*
         * Return a timestamp in a format that can later be used to register
         * a job on a Custom Activity Map's Track.
         */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 getTime()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                if (state == AnnotationState.Active)
                {
                    return gator_get_time();
                }
            #endif
            return 0;
        }

       /*
         * Represents a single counter in the timeline.
         */
        public class Counter
        {
            // Maintain a unique ID for each counter.
            static UInt32 counterCount = 0;

            // The scaling modifier used to display series as float to 2dp.
            const UInt32 modifier = 100;

            // The counter classes used by Streamline's native interface.
            const UInt32 CC_DELTA = 1;
            const UInt32 CC_ABSOLUTE = 2;

            // The display classes used by Streamline's native interface.
            const UInt32 DC_ACCUMULATE = 2;
            const UInt32 DC_MAXIMUM = 4;

            // The renderer classes used by Streamline's native interface.
            const UInt32 RC_OVERLAY = 2;
            const UInt32 RC_LINE = 2;

            // Our counter ID.
            UInt32 counter;

            /*
             * Specify the counter chart title, series name, and value type.
             */
            public Counter(string title, string name, CounterType type)
            {
                counterCount++;
                counter = counterCount;

                #if UNITY_ANDROID && !UNITY_EDITOR
                    if (state == AnnotationState.Active)
                    {
                        UInt32 counterClass = type == CounterType.Delta ? CC_DELTA : CC_ABSOLUTE;
                        UInt32 displayClass = type == CounterType.Delta ? DC_ACCUMULATE : DC_MAXIMUM;

                        gator_annotate_counter(
                            counter, title, name, 0, counterClass, displayClass,
                            null, modifier, RC_OVERLAY, RC_LINE, 0, 0, 0,
                            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, 0, null);
                    }
                #endif
            }

            /*
             * Update the counter value.
             */
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [Conditional("UNITY_ANDROID")]
            public void set_value(float value)
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                    if (state == AnnotationState.Active)
                    {
                        UInt32 ivalue = (UInt32)(value * (float)modifier);
                        gator_annotate_counter_value(0, counter, ivalue);
                    }
                #endif
            }
        }

        /*
         * Represents a channel of activity for the thread in which the channel
         * was created. Displayed as a row in Streamline's Heat Map view,
         * inside the process.
         */
        public class Channel
        {
            // Maintain a unique ID for each channel.
            static UInt32 channelCount = 0;

            // Our channel ID.
            UInt32 channel;

            /*
             * Specify a name, which will be displayed in Streamline's Heat
             * Map view.
             */
            public Channel(string name)
            {
                channelCount++;
                channel = channelCount;

                #if UNITY_ANDROID && !UNITY_EDITOR
                    if (state == AnnotationState.Active)
                    {
                        gator_annotate_name_channel(channel, 0, name);
                    }
                #endif
            }

            /*
             * Starts an annotation in the channel, which will be labelled as
             * specified. This will appear in the Channel as an activity that
             * begins at the point in time where this method was called. It
             * will end when the next annotate() call is made, or the next end()
             * call.
             */
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [Conditional("UNITY_ANDROID")]
            public void annotate(String str)
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                    if (state == AnnotationState.Active)
                    {
                        gator_annotate_str(channel, str);
                    }
                #endif
            }

            /*
             * As above, but with a specific colour.
             */
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [Conditional("UNITY_ANDROID")]
            public void annotate(String str, Color32 color)
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                    if (state == AnnotationState.Active)
                    {
                        UInt32 intColor = colorToGatorInt(color);
                        gator_annotate_color(channel, intColor, str);
                    }
                #endif
            }

            /*
             * Marks the end of an annotation. The other way to end an
             * annotation is to start a new one in the channel.
             */
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [Conditional("UNITY_ANDROID")]
            public void end()
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                    if (state == AnnotationState.Active)
                    {
                        gator_annotate_str(channel, null);
                    }
                #endif
            }
        }

        /*
         * Custom Activity Maps (CAMs) are each special views, displayed in the
         * bottom half of the Streamline UI. Each CAM consists of several
         * Tracks, and each Track can have Jobs placed on it (like annotations
         * in channels, but more flexible).
         */
        public class CAM
        {
            /*
             * Represents a Track within a CAM
             */
            public interface CAMTrack
            {
                /*
                 * Creates a CAMJob object, which will mark itself as starting
                 * immediately. Mark completion with a call to stop() on the
                 * resulting CAMJob.
                 */
                CAMJob makeJob(string _name, Color32 color);

                /*
                 * Register a job with the specified start and stop times. This
                 * is useful if you are unable to register jobs as they happen,
                 * for example when they are created as part of a job running in
                 * the Unity Job Scheduler.
                 */
                void registerJob(string name, Color32 color, UInt64 startTime, UInt64 stopTime);
            }

            /*
             * Represents a Job within a Track in a CAM.
             */
            public interface CAMJob
            {
                /*
                 * Registers that a job has completed at this point in time.
                 */
                void stop();
            }

            private UInt32 trackCount;
            private UInt32 jobCount;
            private UInt32 viewUid;

            /*
             * When creating a Custom Activity Map, you must specify a name,
             * which is used to name the view in the Streamline UI.
             */
            public CAM(string name)
            {
                // Tracks and Jobs need unique IDs.
                this.trackCount = 0;
                this.jobCount = 0;

                // Each CAM needs a unuque ID.
                this.viewUid = globalCamView++;

                #if UNITY_ANDROID && !UNITY_EDITOR
                    if (state == AnnotationState.Active)
                    {
                        gator_cam_view_name(this.viewUid, name);
                    }
                #endif
            }

            /*
             * Creates a track with the specified name. This can then be used
             * to register Jobs. Each Track appears as a named row in the
             * parent CAM.
             */
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CAMTrack createTrack(string _name)
            {
                return new CAMTrackImp(this, _name, ++trackCount);
            }

            /*
             * Private implementation of the CAMTrack.
             */
            private class CAMTrackImp : CAMTrack
            {
                // Parent CAM - needed because Jobs must reference the CAM ID.
                private CAM parent;

                // Maintain unique UIDs for each Track.
                private UInt32 trackUid;

                /*
                 * Each Track just needs a name - the rest is maintained
                 * automatically by the parent CAM.
                 */
                public CAMTrackImp(CAM parent, String name, UInt32 trackUid)
                {
                    this.parent = parent;
                    this.trackUid = trackUid;

                    #if UNITY_ANDROID && !UNITY_EDITOR
                        if (state == AnnotationState.Active)
                        {
                            gator_cam_track(parent.viewUid, this.trackUid, 0xffffffff, name);
                        }
                    #endif
                }

                /*
                 * Creates a CAMJob object, which will mark itself as starting
                 * immediately. Mark completion with a call to stop() on the
                 * resulting CAMJob.
                 */
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public CAMJob makeJob(string _name, Color32 color)
                {
                    UInt32 intColor = colorToGatorInt(color);

                    return new CAMJobImp(parent, trackUid, parent.jobCount++, _name, intColor);
                }

                /*
                 * Register a job with the specified start and stop times. This
                 * is useful if you are unable to register jobs as they happen,
                 * for example when they are created as part of a job running in
                 * the Unity Job Scheduler.
                 */
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void registerJob(string name, Color32 color, UInt64 startTime, UInt64 stopTime)
                {
                    UInt32 jobUid = parent.jobCount++;

                    #if UNITY_ANDROID && !UNITY_EDITOR
                        if (state == AnnotationState.Active)
                        {
                            UInt32 intColor = colorToGatorInt(color);
                            gator_cam_job(parent.viewUid, jobUid, name, trackUid, startTime, stopTime - startTime, 0x00ffff1b, 0xffffffff, new System.IntPtr(0), new System.IntPtr(0));
                        }
                    #endif
                }
            }

            /*
             * Private implementation of the CAMJob.
             */
            private class CAMJobImp : CAMJob
            {
                // These are all maintained by the CAM and the Track.
                private UInt32 viewUid;
                private UInt32 jobUid;
                private UInt32 trackUid;

                /*
                 * Retrieves the existing time and uses it to register the start
                 * of a job. Finish it with a call to stop(). If you want to
                 * register a job after the fact, use Track.registerJob() to
                 * register a Job with a specific start and end time.
                 */
                public CAMJobImp(CAM parent, UInt32 trackUid, UInt32 jobUid, string name, UInt32 color)
                {
                    this.viewUid = parent.viewUid;
                    this.jobUid = jobUid;
                    this.trackUid = trackUid;

                    #if UNITY_ANDROID && !UNITY_EDITOR
                        if (state == AnnotationState.Active)
                        {
                            UInt64 startTime = gator_get_time();
                            gator_cam_job_start(this.viewUid, this.jobUid, name, this.trackUid, startTime, color);
                        }
                    #endif
                }

                /*
                 * Marks the end of a Job, using the current time as the end
                 * time. If you want to register a job after the fact, use
                 * Track.registerJob() to register a Job with a specific start
                 * and end time.
                 */
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void stop()
                {
                    #if UNITY_ANDROID && !UNITY_EDITOR
                        if (state == AnnotationState.Active)
                        {
                            UInt64 stopTime = gator_get_time();
                            gator_cam_job_stop(this.viewUid, this.jobUid, stopTime);
                        }
                    #endif
                }
            }
        }
    }
}
