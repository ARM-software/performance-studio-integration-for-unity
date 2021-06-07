Unity Bindings for Mobile Studio
================================

Introduction
------------

This package contains the C# bindings for Mobile Studio's Streamline component.

Mobile Studio includes a component called Streamline, which can collect and
present hardware performance counters from Android devices. Streamline has an
*annotations* feature, which allows the application being profiled to emit
additional information that Streamline displays with the captured performance
counter information.

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

Channels are custom event timelines associated with a thread. When a channel
has been created, you can place annotations within it. Like a marker, an
annotation has a text label and a color, but unlike markers they span a range
of time.

To create a channel:

    channel = new MobileStudio.Annotations.Channel("AI");

Annotations can be inserted into a channel easily:

    channel.annotate("NPC AI", Color.red);

    // ...do work...

    channel.end();

### Counters

Counters are numerical data points that can be plotted as a chart in the
Streamline timeline view. Counters can be created as either absolute counters,
where every value is an absolute value, or as a delta counter, where values
are the difference since the last value was emitted. All values are floats
and will be presented to 2 decimal places.

When charts are first defined the user can specify two strings, a title and
series name. The title names the chart, the series names the series on the
chart. Multiple series can use the same title name, which will mean that they
will be plotted on the same chart in the Streamline timeline.

To create a counter, e.g.:

    counter = new MobileStudio.Annotations.Counter(
        "Title", "Series", MobileStudio.Annotations.CounterType.Absolute);

Counter values are set easily:

    counter.set_value(42.2f);

### Custom Activity Maps

Custom Activity Maps (CAMs) are a global (not per-thread) set of timelines.
Each CAM appears as its own view in the lower half of Streamline's UI, so each
CAM has a name, and consists of several tracks, each of which appears as a
named row in the CAM. Activity is logged into a track by registering jobs into
it.

To create a CAM:

    gameCAM = new MobileStudio.Annotations.CAM("Game Activity");

To add tracks to the CAM:

    aiTrack = gameCAM.createTrack("AI Activity");
    terrainTrack = gameCAM.createTrack("Terrain Generation Activity");
    windTrack = gameCAM.createTrack("Wind Activity");

After you have created a CAM and added tracks to it, register a job within a
track using one of the following methods. The first is to create the job just
as you start to undertake the activity you want to associate with it, and end
the job when you are done, like you did with Annotations:

    job = aiTrack.makeJob("NPC AI", Color.blue);

    // ...do work...

    job.stop();

The other method is to store the start and end times of your work, and then
later add them to the track:

    UInt64 startTime = MobileStudio.Annotations.getTime();

    // ...do work...

    UInt64 endTime = MobileStudio.Annotations.getTime();

    aiTrack.registerJob("NPC AI", Color.blue, startTime, endTime);

The advantage of this second approach is that the getTime() method is very
cheap in terms of CPU cycles, and can also be safely invoked from jobs running
within the Unity Job Scheduler.

Further Reading
---------------

If you'd like to know more or raise any questions, please see the Mobile Studio
developer pages at:

https://developer.arm.com/mobile-studio

Community support is available from Arm's Graphics and Multimedia forum at:

https://community.arm.com/developer/tools-software/graphics/f/discussions
