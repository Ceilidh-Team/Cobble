# Getting Started with Cobble

## Create an Application from Scratch

1. Reference Cobble from your application
    * This can be done through [NuGet](https://www.nuget.org/packages/ProjectCeilidh.Cobble/), or by downloading the binaries direct from the [GitHub releases](https://github.com/Ceilidh-Team/Cobble/releases) page.
2. Create a `CobbleContext` object in your main method
3. Register your implementations using `CobbleContext.AddManaged`.
    * Implementations are classes with a single public constructor. Cobble will attempt to fill in the parameters with every matching implementation already registered in that `CobbleContext`.
    * Constructor parameters can either be scalar values or `IEnumerable<>`s. Scalar values will be populated with a single instance that can be cast to that type, and `IEnumerable<>`s will be populated with every instance that can be cast to the generic argument.
4. Call `CobbleContext.Execute`
    * This will construct the dependency graph and construct all your instances, satisfying dependencies where requested.
5. Optional: Use `CobbleContext.TryGetSingleton` and `CobbleContext.TryGetImplementations` to access the constructed instances after execution.

## Advanced Usage

* If any class you register with the system implements `ILateInject<>` at least once, the constructed instance will be notified whenever any new classes are registered with the owner `CobbleContext` after `CobbleContext.Execute` is called.
  * This allows for, in certain conditions, plugins to be loaded without requiring an application restart.
* `CobbleContext.AddManaged` can accept any implementation of `IInstanceGenerator`, not just existing types. This can allow for object construction that does not use a type constructor.
  * See the [reference implementations](ProjectCeilidh.Cobble/Generator) of `IInstanceGenerator` for examples on how to implement this interface.
* Creating a `CobbleContext` automatically registers itself with the dependency injection system, allowing you to depend on it and access it later on.
