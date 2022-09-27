Unity Bindings for Mobile Studio
================================

Introduction
------------

This package contains C# bindings needed to export application-generated
counters and event annotations to the Mobile Studio Streamline profiler.

The Streamline profiler can collect and present application software profiling
information alongside sample-based hardware performance counter data from both
Arm CPUs and GPUs. Streamline has an *annotations* feature which allows the
application being profiled to emit additional information that can be displayed
alongside the other captured performance information.

Installation
------------

1) Open the package manager in Unity.
2) Click `+` in the toolbar and select `Add package from git URL`.
3) Import the Mobile Studio package from GitHub into your project.

It is recommended that you set up a define so you can easily remove the
package from release builds without leaving errors in your code from package
usage. To set up the define, follow these steps:

4) If you do not have an asmdef file for scripts that reference the Mobile
   Studio API, create one.
5) In the asmdef file, under `Assembly Definition References`,
   add `MobileStudio.Runtime`.
6) In the asmdef file, under `Version Defines`, add a rule:
      a) Set `Resource` to `com.arm.mobile-studio`.
      b) Set `Define` to `MOBILE_STUDIO`.
      c) Set `Expression` to `1.0.0`
   This rule makes Unity define `MOBILE_STUDIO` if the `com.arm.mobile-studio`
   package is present in the project and if its version is greater than `1.0.0`.
7) In your code, wrap `MOBILE_STUDIO` around the Mobile Studio API:

   #if MOBILE_STUDIO
      // Package usage
   #endif

You can now easily add and remove the package without breaking your project,
which avoids errors in release builds.

Usage
-----

### Markers

The simplest annotations are markers. To use them in a project into which you
have included this package, simply call into the Mobile Studio library as
follows:

    MobileStudio.Annotations.marker("Frame");

This will emit a timestamped marker with the label "Frame", which Streamline
will show along the top of the timeline.

You can also specify the color of the marker by passing an optional Color
object, such as:

    MobileStudio.Annotations.marker("Frame", Color.green);

### Channels

Channels are custom event timelines associated with a software thread. When a
channel has been created, you can place annotations within it. A channel
annotation has a text label and a color but, unlike markers, they span a range
of time.

To create a channel:

    channel = new MobileStudio.Annotations.Channel("AI");

Annotations can be inserted into a channel easily:

    channel.annotate("NPC AI", Color.red);
    // Do work ...
    channel.end();

### Counters

Counters are numerical data points that can be plotted as a chart in the
Streamline timeline view. Counters can be created as either absolute counters,
where every value is an absolute value, or as a delta counter, where values are
the number of instances of an event since the last value was emitted. All
values are floats and will be presented to 2 decimal places.

When charts are first defined the user can specify two strings, a title and
series name. The title names the chart, the series names the data series.
Multiple counter series can use the same title, which means that they will be
plotted on the same chart in the Streamline timeline.

To create a counter, e.g.:

    counter = new MobileStudio.Annotations.Counter(
        "Title", "Series", MobileStudio.Annotations.CounterType.Absolute);

Counter values are set easily:

    counter.setValue(42.2f);

### Custom Activity Maps

Custom Activity Map (CAM) views allow execution information to be plotted on
a hierarchical set of timelines. Like channel annotations, CAM views plot Jobs
on tracks, but unlike channel annotations, CAM views are not associated with a
specific thread. CAM Jobs can also be linked by dependency lines, allowing
control flow between them to be visualized.

To create a CAM view:

    gameCAM = new MobileStudio.Annotations.CAM("Game Activity");

To add tracks to the CAM:

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

If you'd like to know more or raise any questions, please see the Mobile Studio
developer pages at:

  * https://developer.arm.com/mobile-studio

Community support is available from Arm's Graphics and Multimedia forum at:

  * https://community.arm.com/support-forums/f/graphics-gaming-and-vr-forum
