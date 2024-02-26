# Changelog

All notable changes to this package will be documented in this file.

## [1.0.0]

- Initial release.

## [1.1.0]

- **Improvement:** Improve the counter annotation API.
- **Fix:** Added binding synchronization for multi-threaded usage.

## [1.2.0]

- **Improvement:** Added Unity profiler metrics as counters.

## [1.3.0]

- **Fix:** Added static annotation ID namespacing to avoid collisions with
  other Arm tooling, such as the Performance Advisor layer driver.

## [1.4.0]

- **Fix:** Added 64-bit counter value support in the annotation API.

## [1.5.0]

- **Improvement:** Improved CAM annotation API to support track nesting.
- **Improvement:** Improved CAM annotation API to support job dependencies.
- **Fix:** Renamed counter `set_value()` to `setValue()`.

- - -

_Copyright © 2021-2024, Arm Limited and contributors. All rights reserved._
