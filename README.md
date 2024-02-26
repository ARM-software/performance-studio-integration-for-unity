Performance Studio Package
==========================

This project is a Unity package for integrating the Performance Studio tool
suite into game development workflows. This version of the package has the
following features for integrating with the Streamline profiler.

* C# language bindings for emitting event annotations.
* C# language bindings for emitting software counters.

> **Note:** Performance Studio was formerly known as Mobile Studio. For API
> backwards compatibility the package continues to use the `MobileStudio` file
> name prefix and C# namespace.

License
=======

Most files in this library are licensed under the BSD-3 Clause License (see
[LICENSE.md](LICENSE.md)).

The Unity native plugin interface header, `IUnityInterface.h`, is licensed
under the Unity Companion License (see [LICENSE_UNITY.md](LICENSE_UNITY.md)).

Technical details
=================

Requirements
------------

This version of the package is compatible with the Unity Editor version 2020.3
LTS and later.

Building
--------

This package is built using the Unity bee compiler.

1) Set the environment variable `ANDROID_NDK_ROOT` to a local Android NDK
   install. Android NDK can usually be found in:
   `\Editor\Data\PlaybackEngines\AndroidPlayer\NDK`.
2) Locate the bee compiler in your Unity install. It is usually found in:
   `\Editor\Data\il2cpp\build\BeeSettings\offline\bee.exe`.
3) To trigger a build, run `bee.exe` in the `Native~` directory.

Compatibility
-------------

This package uses application-generated Streamline annotations to add more
context to captured data. It allocates annotation handles in the ID range
16384-32767; other users of Streamline in the same process should avoid using
this range to avoid collisions.

Installing and using
--------------------

For instructions on how to install and use this package, see the
[full documentation page](Documentation/Performance-Studio.md).

- - -

_Copyright © 2021-2024, Arm Limited and contributors. All rights reserved._
