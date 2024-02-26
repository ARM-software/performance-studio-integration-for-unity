Unity Bindings for Performance Studio
=====================================

Introduction
------------

This package contains C# bindings needed to export application-generated
counters and event annotations to the Performance Studio Streamline profiler.

The Streamline profiler can collect and present application software profiling
information alongside sample-based hardware performance counter data from both
Arm CPUs and GPUs. Streamline has an *annotations* feature which allows the
application being profiled to emit additional information that can be displayed
alongside the other captured performance information.

> **Note:** Performance Studio was formerly known as Mobile Studio. For API
> backwards compatibility the package continues to use the `MobileStudio` file
> name prefix and C# namespace.

Installation
------------

1) Open the package manager in Unity.
2) Click `+` in the toolbar and select `Add package from git URL`.
3) Import the Performance Studio package from GitHub into your project.

It is recommended that you set up a define so you can easily remove the
package from release builds without leaving errors in your code from package
usage. To set up the define, follow these steps:

4) If you do not have an asmdef file for scripts that reference the package
   API, create one.
5) In the asmdef file, under `Assembly Definition References`,
   add `MobileStudio.Runtime`.
6) In the asmdef file, under `Version Defines`, add a rule:
      a) Set `Resource` to `com.arm.mobile-studio`.
      b) Set `Define` to `MOBILE_STUDIO`.
      c) Set `Expression` to `1.0.0`
   This rule makes Unity define `MOBILE_STUDIO` if the `com.arm.mobile-studio`
   package is present in the project and if its version is greater than `1.0.0`.
7) In your code, wrap `MOBILE_STUDIO` around the package API use:

   #if MOBILE_STUDIO
      // Package usage
   #endif

You can now easily add and remove the package without breaking your project,
which avoids errors in release builds.

Usage
-----

### Markers

Markers are the simplest annotations, emitting a timestamped point indicator
that will be shown along the top of the Streamline timeline view.

To emit a marker, e.g:

    MobileStudio.Annotations.marker("Frame");

You can also specify the color of the marker by passing an optional Unity
`Color` object, e.g:

    MobileStudio.Annotations.marker("Frame", Color.green);

### Channels

Channels are custom event swimlanes that are associated with a software thread
in the Streamline timeline. Once a channel has been created, you can place
job annotations within it. A channel job annotation has a text label and a
color but, unlike markers, they span a range of time.

To create a channel, e.g.:

    channel = new MobileStudio.Annotations.Channel("AI");

To insert an annotation time-box into the channel, e.g.:

    // Trigger the start of the annotation
    channel.annotate("NPC AI", Color.red);

    // Do the work you want to time ...

    // Trigger the end of the annotation
    channel.end();

### Counters

Counters are numerical data points that can be plotted as a chart series in the
Streamline timeline view. Counters can be created as either absolute counters,
where every value is an absolute value, or as a delta counter, where values are
the number of instances of an event since the last value was emitted. All
values are floats and will be presented to 2 decimal places.

When charts are first defined you can specify a title and a series name. The
title names the chart, the series names the data series. Multiple counter
series can use the same title, which means that they will be plotted on the
same chart in the Streamline timeline.

To create a counter, e.g.:

    counter = new MobileStudio.Annotations.Counter(
        "Title", "Series", MobileStudio.Annotations.CounterType.Absolute);

To set a value for a counter, e.g.:

    counter.setValue(42.2f);

### Custom Activity Maps

Custom Activity Map (CAM) views allow job execution information to be plotted
on a hierarchical set of swimlane tracks. Each CAM view defines a standalone
visualization and, unlike channel annotations, tracks are not associated with
with a specific calling thread. Dependency information between jobs within a
single CAM view can also be defined, allowing the visualization to show
control flow information if it is provided.

To create a CAM view, e.g:

    gameCAM = new MobileStudio.Annotations.CAM("Game Activity");

To add tracks to the CAM, e.g:

    // Create root tracks in the view
    aiTrack = gameCAM.createTrack("AI activity");
    terrainTrack = gameCAM.createTrack("Terrain generation");

    // Create a nested track inside another track
    windTrack = terrainTrack.createTrack("Wind activity");

To create a job within a track, there are two methods. The first is an
immediate-mode API which starts a job when it is created, and stops it when
the job's `stop()` method is called.


    job = aiTrack.makeJob("NPC AI", Color.blue);
    // Do work ...
    job.stop();

The other method is to store the start and end times of your work, and then
add them to the track later.

    // Run the work
    startTime = MobileStudio.Annotations.getTime();
    // Do work ...
    endTime = MobileStudio.Annotations.getTime();

    // Register the work done earlier
    aiTrack.registerJob("NPC AI", Color.blue, startTime, endTime);

The advantage of this second approach is that the `getTime()` method is very
cheap in terms of CPU cycles, and can also be safely invoked from jobs running
within the Unity Job Scheduler.

To allow dependencies between Jobs to be expressed, both `makeJob()` and
`registerJob()` accept an optional list of `CAMJob` objects, which indicate the
producers that the new Job consumes from. Dependency links will be shown in the
Streamline visualization.

Further Reading
---------------

If you'd like to know more or raise any questions, please see the Performance
Studio developer pages at:

  * https://developer.arm.com/performance-studio

Community support is available from Arm's graphics forum at:

  * https://community.arm.com/graphics

  - - -

_Copyright © 2021-2024, Arm Limited and contributors. All rights reserve
