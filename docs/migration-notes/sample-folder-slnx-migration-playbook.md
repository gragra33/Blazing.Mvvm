# Sample-folder move + `.slnx` migration playbook

## 1. Purpose / scope

This runbook captures the structural migration pattern for this repository so it can be repeated cleanly in the future.

**In scope only:**
- moving sample applications into the repo-root `samples/` layout
- splitting sample-specific shared MSBuild configuration under `samples/`
- converting the affected solutions from `.sln` to `.slnx`
- applying that structural change from a fresh `develop`-based branch

**Explicitly out of scope:** later PR-review fixes, runtime fixes, analyzer fixes, package-feed fixes, and other post-migration cleanup except where called out as follow-up work.

## 2. Exact migration goals

When repeating this work, the target state is:

1. Sample applications live directly under the repo-root `samples/` folder.
2. Sample-specific shared build/package configuration lives under `samples/`, not mixed into the main `src/` solution area.
3. The main source solution is represented by `src/Blazing.Mvvm.slnx`.
4. Each sample has a colocated `.slnx` file, or in the hybrid case a sample-root `.slnx` that organizes its related projects.
5. Structural changes are isolated from runtime, analyzer, or review-driven fixes.

## 3. Preconditions and branch strategy

Before repeating the migration:

1. Sync your local branch state with the current `develop` branch.
2. Create a fresh migration branch from `develop`.
3. Keep the migration branch scoped to folder structure + `.slnx` changes only.

Standard branch pattern:

- base the work on `develop`
- use a fresh feature branch for the migration
- avoid mixing unrelated cleanup into the structural migration branch

## 4. Step-by-step procedure

### Step 1: Start from `develop`

Create a clean migration branch from the current `develop` branch.

### Step 2: Use the structural commits as the reference diff

Use this playbook and the current target layout as the reference, and copy only the **folder move + `.slnx` conversion** behavior.

Do not pull in later commits that fix:
- runtime behavior
- analyzers
- review feedback unrelated to structure
- package restore/feed issues not required for the structural move itself

### Step 3: Normalize the sample layout under `samples/`

Move sample apps so they live directly under the repo-root `samples/` folder.

The migrated layout includes sample folders such as:
- `samples/Blazing.Mvvm.AnalyzerTest`
- `samples/Blazing.Mvvm.Sample.Server`
- `samples/Blazing.Mvvm.Sample.Wasm`
- `samples/Blazing.SubpathHosting.Server`
- `samples/ParameterResolution.Sample.Wasm`
- `samples/ParentChildSample`
- `samples/Blazing.Mvvm.Sample.WebApp`
- `samples/Blazing.Mvvm.Sample.HybridMaui`
- `samples/Blazing.Mvvm.Security.Wasm`
- `samples/HybridSamples`

Preserve any multi-project internal structure that belongs to a sample, especially `HybridSamples` with its `core/` and `libs/` subtrees.

### Step 4: Split sample-level shared configuration

Ensure the shared sample MSBuild files exist under `samples/`:

- `samples/Directory.Build.props`
- `samples/Directory.Packages.props`
- `samples/Directory.Build.local.props.example`

These files define the sample-side shared build and package behavior independently of the main `src/` solution.

Important structural detail from the migrated layout:

- `samples/Directory.Build.props` computes repo-relative paths back to the repo root and `src/`
- this relative-path logic must remain valid after the sample folder move

### Step 5: Convert solution files to `.slnx`

Replace the migrated solution files with `.slnx` files.

Current reference outputs include:

- `src/Blazing.Mvvm.slnx`
- `samples/Blazing.Mvvm.AnalyzerTest/Blazing.Mvvm.AnalyzerTest.slnx`
- `samples/Blazing.Mvvm.Sample.Server/Blazing.Mvvm.Sample.Server.slnx`
- `samples/Blazing.Mvvm.Sample.Wasm/Blazing.Mvvm.Sample.Wasm.slnx`
- `samples/Blazing.SubpathHosting.Server/Blazing.SubpathHosting.Server.slnx`
- `samples/ParameterResolution.Sample.Wasm/ParameterResolution.Sample.Wasm.slnx`
- `samples/ParentChildSample/Blazing.Mvvm.ParentChildSample.slnx`
- `samples/Blazing.Mvvm.Sample.WebApp/Blazing.Mvvm.Sample.WebApp.slnx`
- `samples/Blazing.Mvvm.Sample.HybridMaui/Blazing.Mvvm.Sample.HybridMaui.slnx`
- `samples/Blazing.Mvvm.Security.Wasm/Blazing.Mvvm.Security.Wasm.slnx`
- `samples/HybridSamples/HybridSamples.slnx`

Keep the `.slnx` files close to the projects they represent.

### Step 6: Preserve solution structure inside `.slnx`

When rebuilding the `.slnx` files:

- keep the main source solution focused on `src/`, `tests/`, and solution items
- keep single-project sample solutions minimal
- keep `HybridSamples.slnx` grouped by its internal folders (`Apps`, `core`, `libs`) with correct relative project paths

### Step 7: Remove obsolete structural artifacts

After the new layout is in place:

- remove old sample locations that were replaced by the root-level `samples/` layout
- remove superseded `.sln` files for the migrated solutions
- make sure no moved sample still points at stale pre-move paths

### Step 8: Review the change set for structural purity

Before committing, confirm the diff is still only about:

- sample folder moves
- sample shared configuration split
- `.slnx` creation/conversion
- path updates required by those structural moves

If the diff contains runtime, analyzer, or review-driven fixes, split those into later work.

## 5. Structural file checklist

Use this checklist when the migration is complete:

- `src/Blazing.Mvvm.slnx` exists
- `samples/Directory.Build.props` exists
- `samples/Directory.Packages.props` exists
- `samples/Directory.Build.local.props.example` exists
- each root-level sample folder exists under `samples/`
- each migrated sample has its expected `.slnx`
- `samples/HybridSamples/HybridSamples.slnx` exists and references its app/core/lib projects by relative path
- no migrated solution is still relying on an old `.sln`

## 6. Validation checklist (folder structure + `.slnx` only)

Validate only the structural outcome:

1. Confirm the sample directories exist directly under repo-root `samples/`.
2. Confirm each expected `.slnx` file exists in the new layout.
3. Open `src/Blazing.Mvvm.slnx` and verify:
   - source and test projects are still listed correctly
   - solution items still use correct relative paths
4. Open each sample `.slnx` and verify its project paths are valid relative to its new location.
5. Pay special attention to `samples/HybridSamples/HybridSamples.slnx` because it references projects in nested `core/` and `libs/` folders.
6. Treat `samples/Blazing.Mvvm.AnalyzerTest` as a structural special case: pathing and `.slnx` references should validate cleanly, but a build can still fail intentionally if the sample is demonstrating analyzer diagnostics.
7. For that sample, distinguish intentional analyzer-demo failures from actual migration regressions such as broken relative paths, missing solution entries, or stale pre-move references.
8. Confirm that old pre-move sample paths are no longer referenced by the migrated `.slnx` files.
9. Confirm the migration can be reviewed as a structural diff without mixing in later fix-up work.

## 7. Common pitfalls / gotchas encountered

- **Start from `develop`.** Do not begin the migration from an outdated branch snapshot or a branch that already contains unrelated fixes.
- **It is easy to accidentally mix follow-up fixes into the structural commit.** Keep the first commit structural only.
- **`Blazing.Mvvm.AnalyzerTest` can fail for the right reasons.** Structural validation should check its moved paths and solution wiring, not treat intentional analyzer-demo diagnostics as proof the migration is broken.
- **`HybridSamples` is not a flat single-project sample.** Its `.slnx` must preserve grouped project organization and correct relative paths.
- **Sample shared config is part of the migration.** The move is not complete if `samples/Directory.Build.props` and `samples/Directory.Packages.props` are missing or still assume the old layout.
- **The main source solution and the sample solutions should stay separate.** Do not collapse sample projects into `src/Blazing.Mvvm.slnx` as part of this migration.

## 8. Separate follow-up work (out of scope)

Treat these as later, separate changes if needed:

- PR-review cleanup after the structural migration lands
- runtime/build fixes caused or exposed by the move
- analyzer warnings or analyzer packaging decisions
- restore/feed/package availability issues beyond the structural split itself
- any non-structural refactors inside sample application code

If one of these is needed, create a separate commit after the structural migration commit.

## 9. Recommended commit strategy

Use a commit sequence like this:

1. **One primary structural commit** containing only the sample move + shared sample config split + `.slnx` conversion.
2. **Any fixes discovered afterward go into separate follow-up commits.**

The key rule is: keep the structural migration easy to review and reuse without dragging later fixes along with it.
