#!/usr/bin/env bash
#
# Sourced by the Linux pack scripts. NativeAOT links against the BUILD host's glibc, so
# without a pinned old sysroot a build from a new distro will not start on older ones.

IG_SYSROOT_GLIBC="2.31"
IG_SYSROOT_MIRRORS=(
	"https://archive.ubuntu.com/ubuntu"
	"https://old-releases.ubuntu.com/ubuntu"
)
# Ubuntu 20.04 RELEASE-pocket builds: -updates versions get superseded and leave the pool.
# Hashes are the SHA256 fields of dists/focal/main/binary-amd64/Packages.
IG_SYSROOT_DEBS=(
	"pool/main/g/glibc/libc6_2.31-0ubuntu9_amd64.deb 847a68e5f1ec2cd4e6fc191c5eaf0547445dd5c473c7617c068ba4441f63b292"
	"pool/main/g/glibc/libc6-dev_2.31-0ubuntu9_amd64.deb adb78f38fb00c76af4384be7a4c5f41da242e05bea6b0483e03b7e0c86738477"
	"pool/main/g/gcc-9/libgcc-9-dev_9.3.0-10ubuntu2_amd64.deb d1db4de59b4184e502407a2abfde23ed1a966e590f17b4d206bdb4fbb7df0040"
	"pool/main/g/gcc-10/libgcc-s1_10-20200411-0ubuntu1_amd64.deb fb69f47490d22988ad8a1b3c19d5292e1ccdbd30661eedcd680f9a78b9b55d60"
	"pool/main/g/gcc-10/libgomp1_10-20200411-0ubuntu1_amd64.deb 16486e704cf644945f3900f265f6e1e5c643f5957ba79a3af5827dcbb8f35367"
)

# Sets IG_SYSROOT to a cached glibc 2.31 sysroot under $1; publish with -p:SysRoot="$IG_SYSROOT".
ig_sysroot_prepare() {
	local root="$1/sysroot-ubuntu20.04-x64"
	local stamp
	stamp="$(printf '%s\n' "${IG_SYSROOT_DEBS[@]}" | sha256sum | cut -d' ' -f1)"

	if [[ "$(cat "$root/.ig-sysroot" 2>/dev/null)" == "$stamp" ]]; then
		IG_SYSROOT="$root"
		return 0
	fi

	echo "==> Preparing glibc $IG_SYSROOT_GLIBC sysroot (Ubuntu 20.04): $root"
	local work="$root.part"
	rm -rf "$work"
	mkdir -p "$work/debs" "$work/root"

	local entry path sum deb mirror got
	for entry in "${IG_SYSROOT_DEBS[@]}"; do
		read -r path sum <<<"$entry"
		deb="$work/debs/${path##*/}"
		got=""
		for mirror in "${IG_SYSROOT_MIRRORS[@]}"; do
			if curl -fsSL --retry 2 --connect-timeout 15 -o "$deb" "$mirror/$path"; then
				got="$(sha256sum "$deb" | cut -d' ' -f1)"
				[[ "$got" == "$sum" ]] && break
				echo "    WARNING: sha256 mismatch for ${path##*/} from $mirror" >&2
			fi
			got=""
		done
		if [[ -z "$got" ]]; then
			echo "Error: could not download a verified ${path##*/} (offline?)" >&2
			return 1
		fi

		if command -v dpkg-deb >/dev/null 2>&1; then
			dpkg-deb -x "$deb" "$work/root" || return 1
		else
			ar p "$deb" data.tar.xz | tar -xJf - -C "$work/root" || return 1
		fi
	done

	# Debian links libdl.so -> /lib/..., which would resolve to the HOST's libs. Relative
	# targets also keep the sysroot valid after the rename below.
	local link target
	while IFS= read -r -d '' link; do
		target="$(readlink "$link")"
		ln -sfn "$(realpath -ms --relative-to="$(dirname "$link")" "$work/root$target")" "$link"
	done < <(find "$work/root" -type l -lname '/*' -print0)

	echo "$stamp" > "$work/root/.ig-sysroot"
	rm -rf "$root"
	mv "$work/root" "$root"
	rm -rf "$work"
	IG_SYSROOT="$root"
}

# LinkNative's only inputs are the object files, so without this a host-linked binary
# from an earlier publish is reused as-is despite the new -p:SysRoot.
ig_force_native_relink() {
	find "$1/bin" -type d -path "*/Release/*/$2/native" -prune -exec rm -rf {} + 2>/dev/null || true
}

# The highest GLIBC_ version any shipped ELF needs IS the oldest distro that runs it.
# Usage: ig_check_glibc_floor <max, e.g. 2.31> <dir>...  Sets IG_GLIBC_FLOOR.
ig_check_glibc_floor() {
	local max="$1"
	shift
	local f need bad=0
	IG_GLIBC_FLOOR=""

	echo "==> glibc floor (max allowed: GLIBC_$max)"
	while IFS= read -r -d '' f; do
		need="$(objdump -T "$f" 2>/dev/null | grep -o 'GLIBC_[0-9][0-9.]*' | sort -uV | tail -n1 || true)"
		[[ -z "$need" ]] && continue
		printf '    %-44s %s\n' "${f##*/}" "$need"

		if [[ "$(printf '%s\n' "$max" "${need#GLIBC_}" | sort -V | tail -n1)" != "$max" ]]; then
			bad=1
			objdump -T "$f" 2>/dev/null | grep -F "($need)" | awk '{ print "        needs " $NF }' | sort -u || true
		fi
		IG_GLIBC_FLOOR="$(printf '%s\n' "$IG_GLIBC_FLOOR" "$need" | sort -V | tail -n1)"
	done < <(find "$@" -type f \( -name "ImageGlass" -o -name "*.so" -o -name "*.so.*" \) -print0)

	echo "    requires: ${IG_GLIBC_FLOOR:-unknown}"
	if [[ "$bad" == "1" ]]; then
		echo "Error: the floor exceeds GLIBC_$max, so the build would not start on Ubuntu 20.04." >&2
		echo "       Was it linked without -p:SysRoot? GLIBC_MAX=<ver> overrides (debug only)." >&2
		return 1
	fi
}
