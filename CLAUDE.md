# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Banshee is a GNOME multimedia player/library manager written in C# on Mono (GTK 3 via Gtk#, GStreamer 1.x, SQLite, Mono.Addins, D-Bus). This fork tracks the `gtk3` branch of Hyena.

## Build

Autotools-based. `src/Hyena` is a git submodule (`git submodule update --init` on a fresh clone; it is also needed for `intltool-update`).

```sh
./autogen.sh [configure flags]      # first time; regenerates and runs configure
make                                 # assemblies land in bin/
```

Useful configure flags: `--enable-tests` / `--disable-tests` (NUnit), `--disable-gnome`, `--enable-release` (skips the dll-map verifier), plus many `--disable-<feature>` flags for optional extensions (see `build/m4/banshee/*.m4`). The configure summary lists what is enabled.

GStreamer backend: the default is the managed backend (gstreamer-sharp), which needs a `gstreamer-sharp-1.0` pkg-config file that gstreamer-sharp 1.14's meson build does not install. Use `--enable-gst-native --disable-gst-sharp` for the C backend (`libbanshee`). `--enable-gst=managed` is not a real option and is silently ignored.

Modern distros lack the required Mono/Gtk# 3 stack; `docker/` contains a pinned Ubuntu 18.04 image that builds this branch and runs it on the host display (`docker build -t rucker/banshee:2018 docker/`, then `docker/run.sh`).

**Adding/removing a source file requires editing that assembly's `Makefile.am` `SOURCES` list** (the build uses these, not the `.csproj`). Keep the `.csproj` in sync too, since `Banshee.sln` is maintained alongside. Assembly references are expressed as `LINK = $(REF_...)` variables defined in `build/build.environment.mk`; new assemblies need entries there and in `configure.ac`. In non-release builds, any `<dllmap>` in an assembly's `.dll.config` is verified against its sources and `Makefile.am`.

## Run

```sh
make run            # runs bin/Nereid.exe --debug --uninstalled (uses your real user DB/config)
src/run-banshee     # same, but with a local ./banshee.db2 database
make run-logged     # tee output to bin/last-run-log
make run-muinshee   # alternate client
make gdb | make csharp   # debugger / REPL with bin/*.dll referenced
```

`BANSHEE_DEV_OPTIONS` and `BANSHEE_DEV_MONO_OPTIONS` are appended to the command line. `--uninstalled` makes Mono.Addins scan `bin/` instead of installed locations.

## Tests

NUnit tests are compiled *into the production assemblies* (in `Tests/` subdirectories, guarded by `#if ENABLE_TESTS`) and listed in the assembly's `Makefile.am` `SOURCES`. Requires configuring with tests enabled and `nunit-console`/`nunit-console2` on PATH.

```sh
make test                         # all assemblies in tests/Makefile.am TEST_ASSEMBLIES
make -C tests Banshee.Services    # a single assembly (name matched case-insensitively against bin/*.dll)
cd src/Core/Banshee.Services && make test   # same, from inside an assembly dir
```

To run a single fixture/test, invoke the runner directly from `bin/`, e.g. `cd bin && TZ=America/Chicago LC_ALL=it_IT LANG=it_IT MONO_ENV_OPTIONS=--debug nunit-console -nologo -noshadow Banshee.Services.dll -run=Banshee.Query.Tests.GetSortTestsWithAlbumYearOn`. The tests deliberately run under an Italian locale and Chicago timezone.

## Architecture

Source layout is `src/<group>/<Assembly>/<Namespace>/<Class>.cs`:

- **Hyena** (`src/Hyena`, submodule): general-purpose foundation — data models, SQLite ORM (`Hyena.Data.Sqlite`), query language, list/grid widgets (`Hyena.Gui`), logging (`Hyena.Log`), jobs.
- **Core**: `Banshee.Core` (platform-agnostic basics: `TrackInfo`, configuration, IO abstraction `Banshee.IO`), `Banshee.Services` (all non-GUI logic: service stack, sources, database collection, playback controller, media engine abstraction, playlists, metadata, hardware), `Banshee.ThickClient` (shared GTK UI: actions, source views, track list, preferences), `Banshee.Widgets`, `Banshee.WebBrowser`, `Banshee.CollectionIndexer`.
- **Backends**: platform/media implementations — `Banshee.GStreamer` (with native C `libbanshee/`), `Banshee.GStreamerSharp` (managed), `Banshee.Gnome`, `Banshee.Gio`, `Banshee.Unix`, `Banshee.Osx`, `Banshee.Windows`.
- **Dap**: device sync (`Banshee.Dap` base + MassStorage/MTP/AppleDevice/Karma).
- **Extensions**: optional features as Mono.Addins plugins (Podcasting, Lastfm, PlayQueue, Mpris, …). `Banshee.Sample` is the minimal template; `src/create-extension` scaffolds a new one.
- **Libraries**: standalone libraries (Lastfm, Migo podcast feeds, Mono.Media playlists, MusicBrainz, Mtp).
- **Clients**: entry executables. `Nereid` is the main GTK client; `Booter` is the launcher; `Muinshee`, `MediaPanel` are alternate UIs; `Halie`/`Beroe` are D-Bus/indexer helpers.

Key cross-cutting mechanisms:

- **ServiceStack** (`Banshee.Services/Banshee.ServiceStack/ServiceManager.cs`): a static service registry. Core services are registered in code; additional services come from add-ins at extension path `/Banshee/ServiceManager/Service`. Services implement `IService` and optionally `IRequiredService`, `IInitializeService`, `IDelayedInitializeService`, `IDBusExportable` (exported over D-Bus). Access services via `ServiceManager.Get<T>()` or convenience properties like `ServiceManager.PlayerEngine`, `ServiceManager.SourceManager`.
- **Add-ins**: every assembly has a `*.addin.xml` embedded resource declaring extension points and extensions (sources, services, actions, preferences, media engines). Extensions are wired up through these files, not by direct references from core.
- **Sources** (`Banshee.Sources`): everything in the left pane (library, playlists, devices, podcasts, stores) is a `Source`, typically a `DatabaseSource`/`PrimarySource` backed by `DatabaseTrackListModel` over SQLite (`Banshee.Database`, `Banshee.Collection.Database`). UI for a source is provided via `ISourceContents` in ThickClient/extensions.
- **Media engines** (`Banshee.MediaEngine`): `PlayerEngineService` selects a `PlayerEngine` implementation provided by a backend add-in.
- **Platform abstraction**: design a generic API first, implement per platform (see `Banshee.Configuration`, `Banshee.IO`). Don't use `System.IO`/`Mono.Unix` directly in top-level APIs; keep GUI code out of non-GUI namespaces.

## Conventions

Follow `HACKING` / `.github/CONTRIBUTING.md` (Mono style):

- 4-space indent; space before parentheses in calls and declarations: `Foo (args)`, `if (x) {`.
- Braces on their own line for namespaces, types, and methods; same line for control flow and properties (`get { return x; }`).
- Private fields `lower_case_with_underscores`; parameters `camelCase`; members `PascalCase`. Always write `private` explicitly.
- `Dictionary<K, V>` (space after comma in generics); spaces around all operators; ~120 col wrap.
- Acronyms: 2 letters all caps (`Banshee.IO`), 3+ letters capitalized (`Banshee.Cdrom`).
- Member order: fields, constructors, properties, methods; statics first.
- Tests use `Assert.That (actual, Is.EqualTo (expected))`.
- New files need the project's license header.
- Commit messages follow GNOME conventions with a component prefix (e.g. `gui: …`, `build: …`, `B.ThickClient: …`). `Signed-off-by` means "reviewed by a maintainer"; history should fast-forward (no merge commits).
