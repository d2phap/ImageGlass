#!/usr/bin/env bash
#
# Package the Linux build of ImageGlass as a single-file AppImage.
#
#   1. Publishes a fresh self-contained AOT build (the version is baked into
#      AppBuildInfo.g.cs, so a stale publish dir ships old code under a new version).
#   2. Stages an AppDir: payload under usr/lib/imageglass (mirroring the Flatpak's
#      /app/imageglass), a generated AppRun, the root .desktop + icon + .DirIcon, and the
#      usr/share/{applications,icons,metainfo} copies integration tools read.
#   3. Prints the glibc floor of the shipped ELFs. Unlike Flatpak, an AppImage runs
#      against the HOST glibc, so a toolchain bump that raises it must be caught here.
#   4. Builds __artifacts/bundle/ImageGlass_<version>_linux-x64.AppImage, optionally
#      GPG-signed, fetching appimagetool if it is not already available.
#
# Run standalone; it publishes itself. Distribution: __assets/linux/appimage/README.md
#
# Env overrides:
#   GPG_KEY=<keyid>            sign the AppImage with this key (unset => unsigned)
#   APPIMAGETOOL=<path>        use this appimagetool instead of the cached/downloaded one
#   APPIMAGETOOL_URL=<url>     where to fetch appimagetool when it is missing
#   APPIMAGETOOL_SHA256=<hex>  expected sha256 of that download (empty => warn, no check)
#   COMP=zstd|xz|gzip          squashfs compression (default: zstd)
#   NO_APPSTREAM=1             skip appimagetool's AppStream validation
#   SKIP_PUBLISH=1             reuse __artifacts/publish/linux-x64 -- DEBUG ONLY, it can
#                              ship stale code under a new version number

set -euo pipefail

WORKSPACE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PUBLISH_DIR="$WORKSPACE_DIR/__artifacts/publish/linux-x64"
APPIMAGE_DIR="$WORKSPACE_DIR/__assets/linux/appimage"
FLATPAK_DIR="$WORKSPACE_DIR/__assets/linux/flatpak"
DIST_DIR="$WORKSPACE_DIR/__artifacts/bundle"
BUILD_ROOT="$WORKSPACE_DIR/__artifacts/bundle/linux-appimage"
# Not named APPDIR: that is the variable the AppImage runtime exports, and the generated
# AppRun below refers to it literally.
APPDIR_STAGE="$BUILD_ROOT/AppDir"
TOOLS_DIR="$WORKSPACE_DIR/__artifacts/tools"
BUILD_PROPS_FILE="$WORKSPACE_DIR/Directory.Build.props"
APP_ID="io.github.d2phap.imageglass"

GPG_KEY="${GPG_KEY:-}"
COMP="${COMP:-zstd}"
# appimagetool aborts with "Please set the ARCH environment variable" when it cannot
# guess the target from the AppDir's ELFs. Never leave it to guesswork.
export ARCH="${ARCH:-x86_64}"

# --- Read version + release type from Directory.Build.props ---
IG_VERSION="$(sed -n 's:.*<IgVersion>\(.*\)</IgVersion>.*:\1:p' "$BUILD_PROPS_FILE" | head -n 1)"
if [[ -z "$IG_VERSION" ]]; then
	echo "Error: could not read IgVersion from $BUILD_PROPS_FILE" >&2
	exit 1
fi
IG_RELEASE_TYPE="$(sed -n 's:.*<IgReleaseType>\(.*\)</IgReleaseType>.*:\1:p' "$BUILD_PROPS_FILE" | head -n 1)"

# Release label mirrors the GitHub release tag/asset naming: <version>-<releasetype>
# (e.g. 10.0.2.66-beta-2). No "v" prefix.
REL_LABEL="$IG_VERSION"
[[ -n "$IG_RELEASE_TYPE" ]] && REL_LABEL="${IG_VERSION}-${IG_RELEASE_TYPE}"

APPIMAGE_NAME="ImageGlass_${REL_LABEL}_linux-x64.AppImage"
APPIMAGE_PATH="$DIST_DIR/$APPIMAGE_NAME"

# --- Publish a fresh self-contained AOT build ---
if [[ "${SKIP_PUBLISH:-0}" == "1" ]]; then
	echo "==> SKIP_PUBLISH=1 -- reusing $PUBLISH_DIR (debug only)"
else
	echo "==> Publishing ImageGlass $IG_VERSION (linux-x64, AOT)"
	rm -rf "$PUBLISH_DIR"
	dotnet publish "$WORKSPACE_DIR/ImageGlass.Linux/ImageGlass.Linux.csproj" \
		-c Release -r linux-x64 -p:Platform=x64 \
		-p:PublishAot=true -p:PublishSingleFile=true -p:PublishTrimmed=true \
		-o "$PUBLISH_DIR" --self-contained true
	cp -r "$WORKSPACE_DIR/__assets/__app/." "$PUBLISH_DIR/"

	# Windows-only assets from __assets/__app/. _ext_icons holds file-type association
	# icons read by Win32DefaultAppApi; 14 MB of dead weight on Linux.
	for win_only_dir in _ext_icons; do
		rm -rf "$PUBLISH_DIR/$win_only_dir"
	done
fi

if [[ ! -x "$PUBLISH_DIR/ImageGlass" ]]; then
	echo "Error: publish did not produce $PUBLISH_DIR/ImageGlass" >&2
	exit 1
fi

# --- Stage the AppDir ---
echo "==> Staging AppDir"
rm -rf "$APPDIR_STAGE"
mkdir -p "$APPDIR_STAGE/usr/lib/imageglass" \
         "$APPDIR_STAGE/usr/lib/fallback" \
         "$APPDIR_STAGE/usr/bin" \
         "$APPDIR_STAGE/usr/share/applications" \
         "$APPDIR_STAGE/usr/share/icons/hicolor/512x512/apps" \
         "$APPDIR_STAGE/usr/share/icons/hicolor/scalable/apps" \
         "$APPDIR_STAGE/usr/share/metainfo" \
         "$DIST_DIR"

# Copy the payload WHOLE: an explicit file list silently drops assets added to
# __assets/__app later (that is how _lang and default.webp went missing on Flatpak).
( cd "$PUBLISH_DIR" && cp -a . "$APPDIR_STAGE/usr/lib/imageglass/" )
find "$APPDIR_STAGE/usr/lib/imageglass" -type f \( -name "*.dbg" -o -name "*.pdb" \) -delete

# The squashfs is read-only, so a .igportable marker makes ConfigMode fail its writability
# probe, and PortableError is a hard startup abort -- never a fallback. Same rule as MSIX.
if [[ -e "$APPDIR_STAGE/usr/lib/imageglass/.igportable" ]]; then
	echo "Error: .igportable must never ship in an AppImage (read-only mount => startup abort)" >&2
	exit 1
fi

ln -sf ../lib/imageglass/ImageGlass "$APPDIR_STAGE/usr/bin/imageglass"
install -m 755 "$APPIMAGE_DIR/ig-appimage-integrate" "$APPDIR_STAGE/usr/bin/ig-appimage-integrate"
chmod +x "$APPDIR_STAGE/usr/lib/imageglass/ImageGlass"

# --- Icons ---
# appimagetool REQUIRES a root icon matching the desktop file's Icon= key, so a missing
# source is fatal here (the Flatpak script can afford to skip silently).
for pair in "logo_c_512.png:$APP_ID.png" "logo_c.svg:$APP_ID.svg"; do
	src="$WORKSPACE_DIR/__assets/${pair%%:*}"
	if [[ ! -f "$src" ]]; then
		echo "Error: missing icon source $src" >&2
		exit 1
	fi
	cp "$src" "$APPDIR_STAGE/${pair##*:}"
done
cp "$APPDIR_STAGE/$APP_ID.png" "$APPDIR_STAGE/usr/share/icons/hicolor/512x512/apps/$APP_ID.png"
cp "$APPDIR_STAGE/$APP_ID.svg" "$APPDIR_STAGE/usr/share/icons/hicolor/scalable/apps/$APP_ID.svg"
# What file managers and integration tools read out of the mounted image.
ln -sf "$APP_ID.png" "$APPDIR_STAGE/.DirIcon"

# AppStream metadata is shared with the Flatpak on purpose: it describes the APPLICATION,
# not the packaging, and Flathub is the stricter consumer, so one file keeps both honest.
cp "$FLATPAK_DIR/$APP_ID.metainfo.xml" "$APPDIR_STAGE/usr/share/metainfo/$APP_ID.metainfo.xml"

# --- Desktop entry (+ generated version keys) ---
cp "$APPIMAGE_DIR/$APP_ID.desktop" "$APPDIR_STAGE/$APP_ID.desktop"
{
	echo "X-AppImage-Name=ImageGlass"
	echo "X-AppImage-Version=$REL_LABEL"
	echo "X-AppImage-Arch=$ARCH"
} >> "$APPDIR_STAGE/$APP_ID.desktop"
# Copy AFTER appending so both entries match.
cp "$APPDIR_STAGE/$APP_ID.desktop" "$APPDIR_STAGE/usr/share/applications/$APP_ID.desktop"

if command -v desktop-file-validate >/dev/null 2>&1; then
	desktop-file-validate "$APPDIR_STAGE/$APP_ID.desktop" \
		|| { echo "Error: the staged .desktop is invalid (appimagetool would reject it)" >&2; exit 1; }
fi

# --- Bundle libgomp.so.1 as a FALLBACK only ---
# Magick.Native needs libgomp.so.1 (DT_NEEDED, no RUNPATH) and minimal systems lack it.
# AppRun only uses this copy when the host has none, so it never shadows the host's.
GOMP_SRC=""
for d in /lib/x86_64-linux-gnu /usr/lib/x86_64-linux-gnu /usr/lib64 /lib64 /usr/lib; do
	if [[ -e "$d/libgomp.so.1" ]]; then GOMP_SRC="$d/libgomp.so.1"; break; fi
done
if [[ -n "$GOMP_SRC" ]]; then
	# -L: land a real file; a dangling symlink in a squashfs is useless.
	cp -L "$GOMP_SRC" "$APPDIR_STAGE/usr/lib/fallback/libgomp.so.1"
	GOMP_FLOOR="$(objdump -T "$APPDIR_STAGE/usr/lib/fallback/libgomp.so.1" 2>/dev/null \
		| grep -o 'GLIBC_[0-9][0-9.]*' | sort -uV | tail -n1 || true)"
	echo "    libgomp fallback: $GOMP_SRC (itself needs ${GOMP_FLOOR:-unknown})"
else
	echo "    WARNING: libgomp.so.1 not found on this host -- no fallback bundled."
	echo "             Users without libgomp installed cannot load the Magick codecs."
fi

# --- AppRun ---
echo "==> Writing AppRun"
cat > "$APPDIR_STAGE/AppRun" <<'APPRUN_EOF'
#!/bin/sh
# ImageGlass AppImage entry point. Keep it minimal: everything exported here is inherited
# by every HOST binary the app spawns (xdg-open, gdbus, paplay, lpr, external tools).

# Exported by the runtime, but NOT when running an extracted squashfs-root/AppRun.
if [ -z "${APPDIR:-}" ]; then
    APPDIR="$(dirname "$(readlink -f "$0")")"
    export APPDIR
fi

# Relative file args and the Save As default dir both follow the cwd; current runtimes
# already preserve it, so this only guards against one that does not.
if [ -n "${OWD:-}" ] && [ -d "$OWD" ]; then
    cd "$OWD" || :
fi

# Only when the host has none: LD_LIBRARY_PATH beats ld.so.cache, so an unconditional
# entry would shadow the host's own glibc-matched copy.
if [ -e "$APPDIR/usr/lib/fallback/libgomp.so.1" ]; then
    _ig_gomp=""
    for _ig_d in /lib/x86_64-linux-gnu /usr/lib/x86_64-linux-gnu /usr/lib64 /lib64 /usr/lib; do
        if [ -e "$_ig_d/libgomp.so.1" ]; then _ig_gomp=1; break; fi
    done
    if [ -z "$_ig_gomp" ] && [ -x /sbin/ldconfig ]; then
        /sbin/ldconfig -p 2>/dev/null | grep -q 'libgomp\.so\.1' && _ig_gomp=1
    fi
    if [ -z "$_ig_gomp" ]; then
        LD_LIBRARY_PATH="$APPDIR/usr/lib/fallback${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
        export LD_LIBRARY_PATH
    fi
    unset _ig_gomp _ig_d
fi

# Re-points an existing entry after the image is moved. Never prompts or installs, so it
# cannot block and hold the read-only mount open.
if [ -n "${APPIMAGE:-}" ] && [ -x "$APPDIR/usr/bin/ig-appimage-integrate" ]; then
    "$APPDIR/usr/bin/ig-appimage-integrate" --maybe >/dev/null 2>&1 &
fi

# Never export TMPDIR, XDG_DATA_HOME, XDG_DATA_DIRS, FONTCONFIG_*, or an LD_LIBRARY_PATH
# covering the payload; and never clear APPIMAGE. See the AppImage notes in CLAUDE.md.

# exec (no subshell) so ProcessName stays "ImageGlass" -- HasOtherInstances() and
# CloseOtherInstances() match on it.
exec "$APPDIR/usr/lib/imageglass/ImageGlass" "$@"
APPRUN_EOF
chmod 755 "$APPDIR_STAGE/AppRun"
sh -n "$APPDIR_STAGE/AppRun" || { echo "Error: generated AppRun is not valid POSIX sh" >&2; exit 1; }

# --- Report the glibc floor of the shipped ELFs ---
# The highest symbol version any shipped binary needs IS the minimum distro supported.
# The || true guards matter: under pipefail a no-match grep would abort the script.
echo "==> glibc floor"
GLIBC_FLOOR="$(
	{ find "$APPDIR_STAGE/usr/lib/imageglass" -type f \
		\( -name "ImageGlass" -o -name "*.so" -o -name "*.so.*" \) -print0 \
	  | xargs -0 -r -n1 objdump -T 2>/dev/null || true; } \
	| grep -o 'GLIBC_[0-9][0-9.]*' | sort -uV | tail -n1 || true
)"
echo "    requires: ${GLIBC_FLOOR:-unknown}"
case "$GLIBC_FLOOR" in
	GLIBC_2.38)
		echo "    -> Ubuntu 24.04+, Fedora 39+, Debian 13+. Symbols forcing it:"
		objdump -T "$APPDIR_STAGE/usr/lib/imageglass/ImageGlass" 2>/dev/null \
			| awk '/GLIBC_2\.38/ { print "        " $NF }' | sort -u || true
		;;
	GLIBC_2.34) echo "    -> Ubuntu 22.04+, Debian 12+, RHEL/Alma/Rocky 9+, Fedora 35+." ;;
esac

# --- Locate appimagetool ---
# No distro packages it, so cache the download under __artifacts/ (gitignored). Use
# AppImage/appimagetool, not AppImageKit v13, whose runtime cannot read a zstd squashfs.
APPIMAGETOOL_URL="${APPIMAGETOOL_URL:-https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage}"
APPIMAGETOOL_SHA256="${APPIMAGETOOL_SHA256:-}"
TOOL=""

if [[ -n "${APPIMAGETOOL:-}" ]]; then
	[[ -x "$APPIMAGETOOL" ]] || { echo "Error: APPIMAGETOOL='$APPIMAGETOOL' is not executable" >&2; exit 1; }
	TOOL="$APPIMAGETOOL"
elif command -v appimagetool >/dev/null 2>&1; then
	TOOL="$(command -v appimagetool)"
elif [[ -x "$TOOLS_DIR/appimagetool-x86_64.AppImage" ]]; then
	TOOL="$TOOLS_DIR/appimagetool-x86_64.AppImage"
else
	echo "==> appimagetool not found -- downloading to $TOOLS_DIR"
	mkdir -p "$TOOLS_DIR"
	if curl -fsSL --retry 2 --connect-timeout 15 -o "$TOOLS_DIR/appimagetool.part" "$APPIMAGETOOL_URL"; then
		GOT="$(sha256sum "$TOOLS_DIR/appimagetool.part" | cut -d' ' -f1)"
		if [[ -n "$APPIMAGETOOL_SHA256" && "$GOT" != "$APPIMAGETOOL_SHA256" ]]; then
			rm -f "$TOOLS_DIR/appimagetool.part"
			echo "Error: appimagetool sha256 mismatch." >&2
			echo "       got  $GOT" >&2
			echo "       want $APPIMAGETOOL_SHA256" >&2
			echo "       'continuous' is a MOVING tag; re-pin deliberately after reviewing." >&2
			exit 1
		fi
		[[ -z "$APPIMAGETOOL_SHA256" ]] && echo "    WARNING: no APPIMAGETOOL_SHA256 pinned -- download unverified."
		echo "    sha256: $GOT"
		echo "    pin it: APPIMAGETOOL_SHA256=$GOT"
		mv "$TOOLS_DIR/appimagetool.part" "$TOOLS_DIR/appimagetool-x86_64.AppImage"
		chmod +x "$TOOLS_DIR/appimagetool-x86_64.AppImage"
		TOOL="$TOOLS_DIR/appimagetool-x86_64.AppImage"
	else
		rm -f "$TOOLS_DIR/appimagetool.part"
		echo "    download failed (offline?)"
	fi
fi

BUILT=0
SIGNED=0

if [[ -z "$TOOL" ]]; then
	echo "==> appimagetool NOT available -- skipping the AppImage build."
	echo "    The staged AppDir is complete and runnable:"
	echo "        $APPDIR_STAGE/AppRun ~/Pictures/some-image.jpg"
	echo "    Fetch the tool (needs network), then re-run this script:"
	echo "        mkdir -p '$TOOLS_DIR'"
	echo "        curl -fsSL -o '$TOOLS_DIR/appimagetool-x86_64.AppImage' '$APPIMAGETOOL_URL'"
	echo "        chmod +x '$TOOLS_DIR/appimagetool-x86_64.AppImage'"
	echo "    Or point at your own copy: APPIMAGETOOL=/path/to/appimagetool bash <script>"
else
	# appimagetool aborts on AppStream errors; surface them here so the failure is readable.
	if [[ "${NO_APPSTREAM:-0}" != "1" ]] && command -v appstreamcli >/dev/null 2>&1; then
		appstreamcli validate --no-net "$APPDIR_STAGE/usr/share/metainfo/$APP_ID.metainfo.xml" || {
			echo "    WARNING: AppStream validation reported problems (above)."
			echo "             NO_APPSTREAM=1 skips appimagetool's own check."
		}
	fi

	# --- GPG signing (optional, when GPG_KEY is set) ---
	# A set-but-unusable key is a warning, not an abort: the payload is already built.
	SIGN_ARGS=()
	if [[ -z "$GPG_KEY" ]]; then
		echo "==> GPG_KEY empty -- building an UNSIGNED AppImage."
	elif ! gpg --list-secret-keys "$GPG_KEY" >/dev/null 2>&1; then
		echo "WARNING: GPG_KEY='$GPG_KEY' is set but no matching SECRET key is in your keyring." >&2
		echo "         Building an UNSIGNED AppImage. To sign, generate the key once:" >&2
		echo "             gpg --quick-generate-key \"$GPG_KEY\" default default never" >&2
		echo "         (an EV/code-signing cert is X.509 and cannot be used here)" >&2
	else
		echo "==> GPG signing enabled (key: $GPG_KEY)"
		SIGN_ARGS=(--sign --sign-key "$GPG_KEY")
		SIGNED=1
	fi

	APPSTREAM_ARGS=(); [[ "${NO_APPSTREAM:-0}" == "1" ]] && APPSTREAM_ARGS=(-n)

	echo "==> Building AppImage: $APPIMAGE_NAME (comp=$COMP)"
	rm -f "$APPIMAGE_PATH"

	# appimagetool is itself an AppImage. APPIMAGE_EXTRACT_AND_RUN makes it self-extract
	# instead of FUSE-mounting, so this also works in containers and without libfuse2.
	APPIMAGE_EXTRACT_AND_RUN=1 "$TOOL" \
		--comp "$COMP" \
		"${APPSTREAM_ARGS[@]}" "${SIGN_ARGS[@]}" \
		"$APPDIR_STAGE" "$APPIMAGE_PATH"

	chmod +x "$APPIMAGE_PATH"
	BUILT=1

	# Prove the EMBEDDED runtime can read what we wrote: --appimage-offset would pass even
	# on an image it cannot mount, because it never touches the filesystem.
	if ( cd "$BUILD_ROOT" && "$APPIMAGE_PATH" --appimage-extract "AppRun" >/dev/null 2>&1 ); then
		echo "    runtime reads the image OK"
		rm -rf "$BUILD_ROOT/squashfs-root"
	else
		echo "    WARNING: the embedded runtime could not read this image."
		echo "             Most likely it lacks '$COMP' support -- retry with COMP=gzip."
	fi
fi

# --- Clean up staging produced during packing ---
# Keep the AppDir when the build was skipped: it is then the only usable output. A full
# `if` (not `[[ ]] && rm`), or set -e kills the run before the report.
if [[ "$BUILT" == "1" ]]; then
	rm -rf "$BUILD_ROOT"
fi

echo ""
echo "Done."
if [[ "$BUILT" == "1" ]]; then
	SHA256="$(sha256sum "$APPIMAGE_PATH" | cut -d' ' -f1)"
	SIZE="$(du -h "$APPIMAGE_PATH" | cut -f1)"
	echo "  AppImage    : $APPIMAGE_PATH"
	echo "  Size        : $SIZE (squashfs, $COMP)"
	echo "  sha256      : $SHA256"
	echo "  glibc floor : ${GLIBC_FLOOR:-unknown}"
	if [[ "$SIGNED" == "1" ]]; then
		echo "  Signed with GPG key : $GPG_KEY"
		echo "  Publish the fingerprint so users can trust the key:"
		echo "      gpg --fingerprint $GPG_KEY"
	else
		echo "  (unsigned image -- no usable GPG key)"
	fi
	echo ""
	echo "Test it:"
	echo "    '$APPIMAGE_PATH' ~/Pictures/some-image.jpg"
	echo "    (no FUSE on the host? append --appimage-extract-and-run)"
else
	echo "  AppDir (unpacked, runnable): $APPDIR_STAGE"
fi
echo ""
echo "Next: upload the .AppImage to the '$REL_LABEL' GitHub release."
echo "      Desktop integration is offered on first run; see __assets/linux/appimage/README.md."
