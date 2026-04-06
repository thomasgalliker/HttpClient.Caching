# Feasibility: replace copied `Microsoft.Extensions.Caching.InMemory` code with NuGet package

## Short answer
A **full replacement is not possible without changing target frameworks**. The project targets `netstandard1.2`, while `Microsoft.Extensions.Caching.Memory` 10.0.5 targets `.NET Standard 2.0` and `.NET Framework 4.6.2` (or newer), not `netstandard1.2`.

## What about `Count` and `Clear` specifically?
Yes, a NuGet-based replacement would still help:

- `Count` and `Clear` **exist on the concrete** `Microsoft.Extensions.Caching.Memory.MemoryCache` type.
- `Count` and `Clear` **do not exist on** `IMemoryCache` (interface).

So these members can be substituted, but you need one of these approaches:
1. Use `MemoryCache` (concrete type) at call sites that need `Count`/`Clear`.
2. Keep code typed as `IMemoryCache` and add a small adapter/extension that performs a safe cast to `MemoryCache` when available.

## Evidence in this repository
- The library currently multi-targets: `net48;netstandard1.2;netstandard2.0;netstandard2.1;net6.0;net7.0;net8.0`.
- The project sets `RootNamespace` to `Microsoft.Extensions.Caching` and contains local copies of `Abstractions` and `InMemory` APIs.
- Internal handlers (`InMemoryCacheHandler`, `InMemoryCacheFallbackHandler`) directly depend on the copied `IMemoryCache` abstraction and extension helpers in this repository.

## External package compatibility check
- NuGet package `Microsoft.Extensions.Caching.Memory` 10.0.5 supports `netstandard2.0` / `net462+`, not `netstandard1.2`.
- Microsoft Learn API docs show `MemoryCache.Count` and `MemoryCache.Clear()` on the concrete type.
- Microsoft Learn API docs for `IMemoryCache` show methods like `TryGetValue`, `CreateEntry`, `Remove` and do not list `Count`/`Clear`.

## Migration options

### Option A (recommended): Keep `netstandard1.2` support (hybrid)
1. Keep copied implementation only for `netstandard1.2`.
2. For other TFMs, reference:
   - `Microsoft.Extensions.Caching.Memory`
   - `Microsoft.Extensions.Caching.Abstractions`
   - `Microsoft.Extensions.Primitives`
3. Add a tiny helper for `Count`/`Clear` (cast to concrete `MemoryCache` when possible).

Pros:
- No breaking TFM change.
- You still gain value from official package on modern frameworks.

Cons:
- Dual-path maintenance complexity.

### Option B: Full replacement with official packages
1. Drop `netstandard1.2` from targets.
2. Remove copied abstractions/in-memory files.
3. Refactor type usage to official APIs (e.g., `IChangeToken` from `Microsoft.Extensions.Primitives`).
4. Decide how to expose `Count`/`Clear` (concrete type or wrapper abstraction).

Pros:
- Removes forked cache maintenance.
- Aligns with current Microsoft behavior.

Cons:
- Breaking change for consumers requiring `netstandard1.2`.

## Recommendation
If backward compatibility matters, do Option A now. If not, do Option B in a major release.

Either way, **`Count` and `Clear` are not blockers** by themselves; the main blocker is `netstandard1.2`.
