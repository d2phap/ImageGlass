#!/usr/bin/env bash
#
# Sign, notarize, and package the macOS ImageGlass.app into a distributable DMG.
#
# Prerequisites (one-time):
#   1. Developer ID Application certificate imported into the login keychain.
#   2. A notarytool keychain profile created once with:
#
#        xcrun notarytool store-credentials "imageglass-notary" \
#            --apple-id "you@example.com" \
#            --team-id "7DV5HBKZ58" \
#            --password "app-specific-password"   # from appleid.apple.com
#
#      (The app-specific password is NOT your normal Apple ID password.)
#
# Run AFTER the app bundle exists (task: pack-mac-arm64-app).
#
# Override defaults via environment variables, e.g.:
#   NOTARY_PROFILE=my-profile SIGN_IDENTITY="Developer ID Application: ..." ./script-pack-mac-arm64-dmg.sh

set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
SIGN_IDENTITY="${SIGN_IDENTITY:-Developer ID Application: Phap Duong (7DV5HBKZ58)}"
NOTARY_PROFILE="${NOTARY_PROFILE:-imageglass-notary}"

WORKSPACE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
APP_DIR="$WORKSPACE_DIR/__artifacts/bundle/osx-arm64/ImageGlass.app"
APP_STAGE_DIR="$WORKSPACE_DIR/__artifacts/bundle/osx-arm64"
ENTITLEMENTS_FILE="$WORKSPACE_DIR/__assets/mac/ImageGlass.entitlements"
BUILD_PROPS_FILE="$WORKSPACE_DIR/Directory.Build.props"
DMG_STAGING_DIR="$WORKSPACE_DIR/__artifacts/bundle/osx-arm64/dmg-staging"
OUTPUT_DIR="$WORKSPACE_DIR/__artifacts/bundle"

# ---------------------------------------------------------------------------
# Sanity checks
# ---------------------------------------------------------------------------
if [[ ! -d "$APP_DIR" ]]; then
	echo "Error: app bundle not found at $APP_DIR" >&2
	echo "       Run the 'pack-mac-arm64-app' task first." >&2
	exit 1
fi
if [[ ! -f "$ENTITLEMENTS_FILE" ]]; then
	echo "Error: entitlements file not found at $ENTITLEMENTS_FILE" >&2
	exit 1
fi
if ! security find-identity -v -p codesigning | grep -qF "$SIGN_IDENTITY"; then
	echo "Error: signing identity not found in keychain: $SIGN_IDENTITY" >&2
	exit 1
fi
# Show notarytool's own reason: a missing profile and an unaccepted Apple agreement (HTTP 403) need different fixes.
if ! notary_out="$(xcrun notarytool history --keychain-profile "$NOTARY_PROFILE" 2>&1)"; then
	echo "Error: notarytool check failed for keychain profile '$NOTARY_PROFILE':" >&2
	echo "$notary_out" | sed 's:^:       :' >&2
	echo "       No such profile? Create it with: xcrun notarytool store-credentials \"$NOTARY_PROFILE\" ..." >&2
	echo "       HTTP 403? The Account Holder must accept the latest agreement at https://developer.apple.com/account" >&2
	exit 1
fi

IG_VERSION="$(sed -n 's:.*<IgVersion>\(.*\)</IgVersion>.*:\1:p' "$BUILD_PROPS_FILE" | head -n 1)"
if [[ -z "$IG_VERSION" ]]; then
	echo "Error: could not read IgVersion from $BUILD_PROPS_FILE" >&2
	exit 1
fi
IG_RELEASE_TYPE="$(sed -n 's:.*<IgReleaseType>\(.*\)</IgReleaseType>.*:\1:p' "$BUILD_PROPS_FILE" | head -n 1)"

# Release label mirrors the Linux/Windows asset naming: <version>-<releasetype>.
REL_LABEL="$IG_VERSION"
[[ -n "$IG_RELEASE_TYPE" ]] && REL_LABEL="${IG_VERSION}-${IG_RELEASE_TYPE}"

DMG_NAME="ImageGlass_${REL_LABEL}_mac-arm64.dmg"
DMG_PATH="$OUTPUT_DIR/$DMG_NAME"
VOLUME_NAME="ImageGlass ${IG_VERSION}"

echo "==> Packaging ImageGlass $IG_VERSION (arm64)"
echo "    Identity : $SIGN_IDENTITY"
echo "    Profile  : $NOTARY_PROFILE"

# ---------------------------------------------------------------------------
# 1. Strip debug artifacts that should not ship (and break signing if signed).
# ---------------------------------------------------------------------------
echo "==> Removing debug artifacts from bundle"
find "$APP_DIR" -type f -name "*.pdb" -delete
find "$APP_DIR" -type d -name "*.dSYM" -exec rm -rf {} +

# Finder can drop .DS_Store / ._* into the bundle at any point after it was built.
find "$APP_DIR" -type f \( -name ".DS_Store" -o -name "._*" \) -delete

# Any real file or folder here that is not Mach-O fails the bundle signing below with an
# unhelpful "code object is not signed at all", so name the culprit up front instead.
strays="$(find "$APP_DIR/Contents/MacOS" -mindepth 1 -maxdepth 1 \
	! -type l ! -name "*.dylib" ! -name "*.so" ! -name "$(basename "$APP_DIR" .app)")"
if [[ -n "$strays" ]]; then
	echo "Error: non-code item(s) left in Contents/MacOS:" >&2
	echo "$strays" | sed 's:^:       :' >&2
	echo "       Stage them under Contents/Resources/ in script-pack-mac-arm64-app.sh." >&2
	exit 1
fi

# ---------------------------------------------------------------------------
# 2. Codesign nested Mach-O binaries first (inside-out), then the bundle.
# ---------------------------------------------------------------------------
echo "==> Signing nested native libraries"
while IFS= read -r -d '' lib; do
	echo "    sign: ${lib#"$APP_DIR/"}"
	codesign --force --timestamp --options runtime \
		--sign "$SIGN_IDENTITY" "$lib"
done < <(find "$APP_DIR/Contents/MacOS" -type f \( -name "*.dylib" -o -name "*.so" \) -print0)

echo "==> Signing app bundle (hardened runtime + entitlements)"
codesign --force --timestamp --options runtime \
	--entitlements "$ENTITLEMENTS_FILE" \
	--sign "$SIGN_IDENTITY" "$APP_DIR"

echo "==> Verifying code signature"
codesign --verify --deep --strict --verbose=2 "$APP_DIR"

# ---------------------------------------------------------------------------
# 3. Build the DMG (with an /Applications shortcut for drag-install).
# ---------------------------------------------------------------------------
echo "==> Building DMG"
rm -rf "$DMG_STAGING_DIR"
mkdir -p "$DMG_STAGING_DIR" "$OUTPUT_DIR"
# The in-app updater (MacUpdateProvider) installs the .app at the volume root, so keep it the only one.
cp -R "$APP_DIR" "$DMG_STAGING_DIR/"
ln -s /Applications "$DMG_STAGING_DIR/Applications"

rm -f "$DMG_PATH"
hdiutil create \
	-volname "$VOLUME_NAME" \
	-srcfolder "$DMG_STAGING_DIR" \
	-fs HFS+ \
	-format UDZO \
	-ov \
	"$DMG_PATH"
rm -rf "$DMG_STAGING_DIR"

echo "==> Signing DMG"
codesign --force --timestamp --sign "$SIGN_IDENTITY" "$DMG_PATH"

# ---------------------------------------------------------------------------
# 4. Notarize the DMG and staple the ticket.
# ---------------------------------------------------------------------------
echo "==> Submitting DMG for notarization (this can take a few minutes)"
xcrun notarytool submit "$DMG_PATH" \
	--keychain-profile "$NOTARY_PROFILE" \
	--wait

echo "==> Stapling notarization ticket"
xcrun stapler staple "$DMG_PATH"

echo "==> Validating Gatekeeper acceptance"
spctl --assess --type open --context context:primary-signature --verbose=2 "$DMG_PATH" || true
xcrun stapler validate "$DMG_PATH"

# Remove the staged .app bundle — the signed/notarized .dmg is the deliverable.
rm -rf "$APP_STAGE_DIR"

echo ""
echo "Done: $DMG_PATH"
