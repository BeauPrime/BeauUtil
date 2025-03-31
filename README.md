# BeauUtil

**Current Version: 0.10.15**
Updated 31 March 2025 | [Changelog](https://github.com/BeauPrime/BeauUtil/blob/master/CHANGELOG.md)

## About
BeauUtil is a general utility library for Unity3d. It contains a variety of code helpers, collection types, debugging utilities, and unsafe code utilities, among many other modules.

By folder:

* Animation
  * `AnimatorParamChange`: Serializable struct describing a parameter change to an Animator
  * `AnimatorStateSnapshot`: Snapshot of parameter and layer states for a specific AnimationController
* Callbacks
  * `CastableAction`: Delegate wrapper that represents a function with no return type and several parameter signatures
  * `CastableFunc`: Delegate wrapper that represents a function with a return value and several parameter signatures
  * `CastableEvent`: Invokable event class holding instances of `CastableAction`
  * `ActionEvent`: Invokable event class holding instances of `Action`
  * `CastableArgument`: Utility methods for defining how casted arguments for `CastableAction` and `CastableFunc` are casted
* Camera
  * `CameraFOVPlane`: Defines the height of a perspective camera frustum at a specified plane
  * `CameraRenderScale`: Defines camera rendering scale and upscaling filter mode
  * `CameraHelper`: Utility methods for calculating various camera parameters and attaching rendering callbacks to arbitrary cameras
* Collections
  * Cache
    * `ICache`: Interface for a value cache with limited size
    * `LruCache`: Least-recently-used cache implementation
  * Identifiers
    * `UniqueId`: 16, 32, and 64-bit index/version identifiers
    * `UniqueIdAllocator`: 16 and 32-bit id allocators
    * `UniqueIdMap`: Map of 16 and 32-bit ids to values
  * RingBuffer
    * `IRingBuffer`: Interface for a ring buffer
    * `RingBuffer`: Circular buffer/ring buffer/deque.
  * `ListSlice`: Struct representing a subregion of a list or array
  * `RandomDeck`: Set of items that can be shuffled, drawn from, and re-shuffled
More documentation coming soon...