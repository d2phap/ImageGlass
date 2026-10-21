#!/usr/bin/env bash
#
# Package the prebuilt Linux build of ImageGlass for Flatpak.
#
#   1. Publishes a fresh AOT build against the pinned glibc 2.31 sysroot, then tars it
#      into a release archive, stripping debug symbols.
#   2. Writes the download URL + sha256 into the Flatpak manifest.
#   3. If flatpak-builder is available: builds a single-file .flatpak bundle
#      (for direct download / GitHub Releases) and installs it locally to test.
#
# Run standalone; it publishes itself. Distribution steps: __assets/linux/flatpak/README.md
#
# Env overrides:
#   RELEASE_TAG=10.0.2.66-beta-2   tag used to build the GitHub download URL
#                                  (defaults to <IgVersion>-<IgReleaseType>)
#   GPG_KEY=<keyid>       sign the .flatpak bundle with this GPG key and embed its
#                         public half so the signature is verifiable on install
#                         (optional; unset => unsigned bundle)
#   GLIBC_MAX=<ver>       highest glibc a shipped ELF may need (default: the sysroot's)

set -euo pipefail

WORKSPACE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PUBLISH_DIR="$WORKSPACE_DIR/__artifacts/publish/linux-x64"
FLATPAK_DIR="$WORKSPACE_DIR/__assets/linux/flatpak"
DIST_DIR="$WORKSPACE_DIR/__artifacts/bundle"
BUILD_ROOT="$WORKSPACE_DIR/__artifacts/bundle/linux-flatpak"
STATE_DIR="$BUILD_ROOT/.flatpak-builder"
TOOLS_DIR="$WORKSPACE_DIR/__artifacts/tools"
BUILD_PROPS_FILE="$WORKSPACE_DIR/Directory.Build.props"
MANIFEST="$FLATPAK_DIR/io.github.d2phap.imageglass.yaml"
APP_ID="io.github.d2phap.imageglass"
# Signing key for the .flatpak bundle, supplied via the GPG_KEY env var — the VS Code
# "pack-linux-x64-flatpak" task prompts for it, or run: GPG_KEY=<id> bash <script>.
# Empty => unsigned bundle. If the key isn't in the local keyring, the build falls
# back to unsigned (with a warning) rather than failing.
GPG_KEY="${GPG_KEY:-}"

# shellcheck source=script-linux-x64-sysroot.sh
source "$WORKSPACE_DIR/__assets/linux/script-linux-x64-sysroot.sh"

# --- Read version + release type from Directory.Build.props ---
IG_VERSION="$(sed -n 's:.*<IgVersion>\(.*\)</IgVersion>.*:\1:p' "$BUILD_PROPS_FILE" | head -n 1)"
if [[ -z "$IG_VERSION" ]]; then
	echo "Error: could not read IgVersion from $BUILD_PROPS_FILE" >&2
	exit 1
fi
IG_RELEASE_TYPE="$(sed -n 's:.*<IgReleaseType>\(.*\)</IgReleaseType>.*:\1:p' "$BUILD_PROPS_FILE" | head -n 1)"

# Release label mirrors the GitHub release tag/asset naming: <version>-<releasetype>
# (e.g. 10.0.2.66-beta-2). No "v" prefix. The tag and the asset file share this label.
REL_LABEL="$IG_VERSION"
[[ -n "$IG_RELEASE_TYPE" ]] && REL_LABEL="${IG_VERSION}-${IG_RELEASE_TYPE}"

RELEASE_TAG="${RELEASE_TAG:-$REL_LABEL}"
TARBALL_NAME="ImageGlass_${REL_LABEL}_linux-x64.tar.gz"
BUNDLE_NAME="ImageGlass_${REL_LABEL}_linux-x64.flatpak"
TARBALL_PATH="$DIST_DIR/$TARBALL_NAME"
BUNDLE_PATH="$DIST_DIR/$BUNDLE_NAME"
RELEASE_URL="https://github.com/d2phap/ImageGlass/releases/download/${RELEASE_TAG}/${TARBALL_NAME}"

# --- glibc sysroot: the tarball also runs outside Flatpak, on the host glibc ---
ig_sysroot_prepare "$TOOLS_DIR"
GLIBC_MAX="${GLIBC_MAX:-$IG_SYSROOT_GLIBC}"

# --- Publish a fresh self-contained AOT build ---
# Always re-publish so the bundle matches the current source and IgVersion. The
# version is baked into the binary (AppBuildInfo.g.cs); packaging a stale publish
# dir would ship the wrong version and old code.
echo "==> Publishing ImageGlass $IG_VERSION (linux-x64, AOT, glibc $IG_SYSROOT_GLIBC sysroot)"
rm -rf "$PUBLISH_DIR"
ig_force_native_relink "$WORKSPACE_DIR/ImageGlass.Linux" linux-x64
dotnet publish "$WORKSPACE_DIR/ImageGlass.Linux/ImageGlass.Linux.csproj" \
	-c Release -r linux-x64 -p:Platform=x64 \
	-p:PublishAot=true -p:PublishSingleFile=true -p:PublishTrimmed=true \
	-p:SysRoot="$IG_SYSROOT" \
	-o "$PUBLISH_DIR" --self-contained true
cp -r "$WORKSPACE_DIR/__assets/__app/." "$PUBLISH_DIR/"

# Windows-only assets from __assets/__app/. _ext_icons holds file-type association
# icons read by Win32DefaultAppApi; dead weight on Linux.
for win_only_dir in _ext_icons; do
	rm -rf "$PUBLISH_DIR/$win_only_dir"
done

if [[ ! -x "$PUBLISH_DIR/ImageGlass" ]]; then
	echo "Error: publish did not produce $PUBLISH_DIR/ImageGlass" >&2
	exit 1
fi

# --- Prepare app-id-named icons from the shared assets ---
if [[ -f "$WORKSPACE_DIR/__assets/logo_c.svg" ]]; then
	cp "$WORKSPACE_DIR/__assets/logo_c.svg" "$FLATPAK_DIR/$APP_ID.svg"
fi
if [[ -f "$WORKSPACE_DIR/__assets/logo_c_512.png" ]]; then
	cp "$WORKSPACE_DIR/__assets/logo_c_512.png" "$FLATPAK_DIR/$APP_ID.png"
fi

# --- Stage the payload and build the tarball ---
# Tar with a single top-level "ImageGlass/" dir so the manifest can use
# strip-components: 1. Exclude debug artifacts that bloat the package.
echo "==> Staging payload (excluding *.dbg / *.pdb)"
STAGE_DIR="$BUILD_ROOT/stage"
rm -rf "$STAGE_DIR"
mkdir -p "$STAGE_DIR/ImageGlass" "$DIST_DIR"
( cd "$PUBLISH_DIR" && cp -a . "$STAGE_DIR/ImageGlass/" )
find "$STAGE_DIR/ImageGlass" -type f \( -name "*.dbg" -o -name "*.pdb" \) -delete

# The tarball also runs as-is on the host, where the glibc floor applies as for AppImage.
ig_check_glibc_floor "$GLIBC_MAX" "$STAGE_DIR/ImageGlass"

echo "==> Creating tarball: $TARBALL_NAME"
rm -f "$TARBALL_PATH"
tar -czf "$TARBALL_PATH" -C "$STAGE_DIR" ImageGlass

SHA256="$(sha256sum "$TARBALL_PATH" | cut -d' ' -f1)"
echo "    sha256: $SHA256"

# --- Wire url + sha256 into the manifest ---
echo "==> Updating manifest source (url + sha256)"
sed -i -E "s#^( *)url: https://github.com/d2phap/ImageGlass/releases/download/.*#\1url: ${RELEASE_URL}#" "$MANIFEST"
sed -i -E "s#^( *)sha256: [0-9a-f]{64}#\1sha256: ${SHA256}#" "$MANIFEST"

# --- Optional: build the .flatpak bundle + install locally via flatpak-builder ---
# Read the runtime version from the manifest so this stays in sync with it.
RUNTIME_VER="$(sed -n "s/^runtime-version: *['\"]\?\([0-9.]*\).*/\1/p" "$MANIFEST" | head -n1)"
RUNTIME_VER="${RUNTIME_VER:-25.08}"
BUNDLE_BUILT=0

if ! command -v flatpak-builder >/dev/null 2>&1; then
	echo "==> flatpak-builder NOT found — skipping bundle build."
	echo "    Install it to build the .flatpak bundle:"
	echo "        sudo apt install flatpak-builder"
elif ! { flatpak info "org.freedesktop.Platform//$RUNTIME_VER" >/dev/null 2>&1 \
		&& flatpak info "org.freedesktop.Sdk//$RUNTIME_VER" >/dev/null 2>&1; }; then
	echo "==> Runtime/SDK $RUNTIME_VER not installed — skipping bundle build."
	echo "    Install them, then re-run this script:"
	echo "        flatpak install -y flathub org.freedesktop.Platform//$RUNTIME_VER org.freedesktop.Sdk//$RUNTIME_VER"
else
	echo "==> Building Flatpak (bundle + local install)"

	# Self-contained staging dir so all manifest sources resolve locally
	# (the committed metadata files + the freshly built tarball).
	LOCAL_DIR="$BUILD_ROOT/local"
	REPO_DIR="$BUILD_ROOT/repo"
	rm -rf "$LOCAL_DIR"
	mkdir -p "$LOCAL_DIR"
	cp "$FLATPAK_DIR/$APP_ID.desktop" \
	   "$FLATPAK_DIR/$APP_ID.metainfo.xml" \
	   "$FLATPAK_DIR/$APP_ID.svg" \
	   "$FLATPAK_DIR/$APP_ID.png" \
	   "$LOCAL_DIR/"
	cp "$TARBALL_PATH" "$LOCAL_DIR/app.tar.gz"

	# Local manifest: swap the remote archive source for the local tarball
	# (replace the url line with a path, drop the now-irrelevant sha256 line).
	sed -E -e "s#^( *)url: .*#\1path: app.tar.gz#" \
	       -e "/^ *sha256: [0-9a-f]{64}/d" \
	       "$MANIFEST" > "$LOCAL_DIR/$APP_ID.yaml"

	# --- GPG signing (optional, when GPG_KEY is set) ---
	# Sign the OSTree commit AND embed the matching public key in the bundle, so
	# the origin remote created on `flatpak install <bundle>.flatpak` can actually
	# verify the signature. Without --gpg-keys the embedded signature has nothing
	# to check against and is effectively inert. Unset GPG_KEY => unsigned output.
	GPG_SIGN_ARGS=()    # repo commit signing (flatpak-builder + build-bundle)
	GPG_BUNDLE_ARGS=()  # build-bundle only: signing + embedded public key
	if [[ -z "$GPG_KEY" ]]; then
		echo "==> GPG_KEY empty — building an UNSIGNED bundle."
	elif ! gpg --list-secret-keys "$GPG_KEY" >/dev/null 2>&1; then
		# Don't abort the whole pack (the tarball is already built); just skip signing.
		echo "WARNING: GPG_KEY='$GPG_KEY' is set but no matching SECRET key is in your keyring." >&2
		echo "         Building an UNSIGNED bundle. To sign, generate the key once:" >&2
		echo "             gpg --quick-generate-key \"$GPG_KEY\" default default never" >&2
		echo "         (an EV/code-signing cert is X.509 and cannot be used here — gpg needs its own key)" >&2
	else
		PUBKEY_FILE="$BUILD_ROOT/$APP_ID.pubkey.gpg"
		echo "==> GPG signing enabled (key: $GPG_KEY) — embedding public key in bundle"
		# Export the public half (binary, what flatpak --gpg-keys expects). Redirect
		# instead of --output to avoid gpg's interactive overwrite prompt on re-runs.
		gpg --export "$GPG_KEY" > "$PUBKEY_FILE"
		GPG_SIGN_ARGS=(--gpg-sign="$GPG_KEY")
		GPG_BUNDLE_ARGS=(--gpg-sign="$GPG_KEY" --gpg-keys="$PUBKEY_FILE")
	fi

	# Build into a repo (for the bundle) and install for the current user (to test).
	# --state-dir keeps the build cache under __artifacts/ instead of the repo root.
	# --disable-cache is REQUIRED: the single module is a local tarball whose name is
	# constant (app.tar.gz) but whose contents change every release. Without it,
	# flatpak-builder matches the cached module build and ships the OLD binary even
	# though the tarball is fresh (--force-clean only wipes the build dir, not the
	# cache) — the .flatpak ends up version-stamped new but containing old code.
	flatpak-builder --state-dir="$STATE_DIR" --user --install --force-clean --disable-cache \
		--repo="$REPO_DIR" "${GPG_SIGN_ARGS[@]}" \
		"$BUILD_ROOT/build" "$LOCAL_DIR/$APP_ID.yaml"

	# Single-file bundle for direct download / GitHub Releases. --runtime-repo
	# lets installers auto-fetch the freedesktop runtime from Flathub.
	echo "==> Building .flatpak bundle: $BUNDLE_NAME"
	rm -f "$BUNDLE_PATH"
	flatpak build-bundle "${GPG_BUNDLE_ARGS[@]}" \
		--runtime-repo=https://dl.flathub.org/repo/flathub.flatpakrepo \
		"$REPO_DIR" "$BUNDLE_PATH" "$APP_ID"
	BUNDLE_BUILT=1
fi

# --- Clean up staging / build temp produced during packing ---
# The tarball + .flatpak bundle are the deliverables (in __artifacts/bundle/);
# everything under BUILD_ROOT (stage, flatpak-builder cache/state, local repo,
# exported pubkey) is intermediate and safe to drop.
rm -rf "$BUILD_ROOT"

echo ""
echo "Done."
echo "  Tarball (Flathub source): $TARBALL_PATH"
echo "  sha256                  : $SHA256"
echo "  Manifest url            : $RELEASE_URL"
if [[ "$BUNDLE_BUILT" == "1" ]]; then
	echo "  Bundle (direct install) : $BUNDLE_PATH"
	# PUBKEY_FILE is only set when signing actually happened (key present in keyring).
	if [[ -n "${PUBKEY_FILE:-}" ]]; then
		echo "  Signed with GPG key     : $GPG_KEY"
		echo "  Publish the fingerprint so users can trust the key:"
		echo "      gpg --fingerprint $GPG_KEY"
	else
		echo "  (unsigned bundle — no usable GPG key)"
	fi
	echo ""
	echo "Installed to your USER flatpak. Test with:"
	echo "    flatpak run $APP_ID [image-path]"
	# A previously double-clicked bundle installs SYSTEM-wide and will shadow this
	# fresh user install (you'd keep testing the old code). Warn if one exists.
	if flatpak --system info "$APP_ID" >/dev/null 2>&1; then
		echo ""
		echo "WARNING: an older SYSTEM-wide install exists and will be launched instead."
		echo "         Remove it so you test this build:"
		echo "             flatpak uninstall --system $APP_ID"
	fi
fi
echo ""
echo "Next: upload both files to the '$RELEASE_TAG' GitHub release, then submit"
echo "      the manifest to Flathub (see __assets/linux/flatpak/README.md)."
