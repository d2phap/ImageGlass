# ImageGlass Flatpak

Files used to build and distribute ImageGlass as a Flatpak.

| File | Purpose |
|---|---|
| `io.github.d2phap.imageglass.yaml` | Flatpak manifest (submitted to Flathub). |
| `io.github.d2phap.imageglass.metainfo.xml` | AppStream metadata (required by Flathub). |
| `io.github.d2phap.imageglass.desktop` | Desktop launcher entry. |
| `io.github.d2phap.imageglass.svg` / `.png` | Icons, generated from `__assets/logo_c.*` by the pack script. |

Build script: [`../script-pack-linux-x64-flatpak.sh`](../script-pack-linux-x64-flatpak.sh) (VS Code task: `pack-linux-x64-flatpak`).

The manifest installs the prebuilt `publish-linux-x64` binary instead of compiling
from source. Building .NET 10 AOT (trimming + SkiaSharp/HarfBuzz/Magick.NET native
interop) offline on Flathub's builders isn't practical, so the manifest downloads a
release tarball with a pinned sha256.

## Build

```bash
# one-time
sudo apt install flatpak-builder
flatpak install -y flathub org.freedesktop.Platform//25.08 org.freedesktop.Sdk//25.08

# publish first (VS Code task: publish-linux-x64), then:
bash __assets/linux/script-pack-linux-x64-flatpak.sh
```

Outputs to `__artifacts/bundle/`:

- `ImageGlass_<version>_linux-x64.tar.gz` — payload the Flathub manifest points at.
- `ImageGlass_<version>_linux-x64.flatpak` — single-file bundle for direct install.

The script also installs the build for your user, so you can test it:

```bash
flatpak run io.github.d2phap.imageglass ~/Pictures/some-image.jpg
```

## Distribute on GitHub Releases

Self-hosted, no review, available immediately. Upload both output files to the
release matching the tag (default `v<IgVersion>`; override with `RELEASE_TAG=<tag>`).
Users install the bundle directly:

```bash
flatpak install --user ImageGlass_<version>_linux-x64.flatpak
```

To sign the bundle, generate a key once and pass it via `GPG_KEY`:

```bash
gpg --quick-generate-key "ImageGlass Release Signing" default default never
GPG_KEY="<your-key-id-or-email>" bash __assets/linux/script-pack-linux-x64-flatpak.sh
```

Signing is optional for a single-file bundle; users can install it either way.

### In-app updates

A bundle install's origin remote (`imageglass-origin`) has no URL, so `flatpak update` answers
"Nothing to do." ImageGlass Pro updates it in place instead: it downloads the release's
`linux-x64-flatpak` artifact, checks its sha256, and runs `flatpak install --bundle` on the host
through `flatpak-spawn --host`. That call is why `--talk-name=org.freedesktop.Flatpak` is needed.

An install from a remote with a URL (Flathub, or the dev install `flatpak-builder --install`
creates) stays check-only. A bundle installed over it keeps that origin but deploys a commit the
remote does not have, so a later `flatpak update` can downgrade it.

## Submit to Flathub

1. The tarball must be reachable at the manifest's `url`, so cut the GitHub release
   first. The script already wrote the matching `url` + `sha256`.
2. Edit `io.github.d2phap.imageglass.metainfo.xml` so each `<screenshot>` URL points
   at a real HTTPS image. Flathub rejects submissions whose screenshots don't load.
3. The license is declared as `GPL-3.0-or-later` (GPLv3).
4. Fork [`flathub/flathub`](https://github.com/flathub/flathub), branch
   `io.github.d2phap.imageglass`, add the manifest + metadata files from this folder,
   and open a PR against `new-pr`. Process:
   <https://docs.flathub.org/docs/for-app-authors/submission>.
5. The `io.github.d2phap.*` id is verified through your GitHub account (`d2phap`),
   so no domain ownership is needed. After acceptance you get the
   `flathub/io.github.d2phap.imageglass` repo; future releases bump the version,
   `url`, `sha256`, and the `<release>` entry.

## Sandbox permissions

`finish-args` grants Wayland/X11, GPU (`dri`), full filesystem (`--filesystem=host`,
for opening/saving anywhere), printing (`cups`), and "reveal in file manager".
Flathub reviewers may ask to narrow `--filesystem=host` to `--filesystem=home`.
