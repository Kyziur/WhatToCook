#!/bin/sh
# One-shot bootstrap for the Web Data Protection certificate.
# Generates an RSA self-signed PEM only when the key ring is empty and the certificate is absent.
# An existing certificate is never replaced. Key and certificate material is never printed.
set -eu
umask 077

fail() {
    printf 'web-data-protection-init: %s\n' "$1" >&2
    exit 1
}

key_ring=/var/lib/whattocook/data-protection
cert_dir=/var/lib/whattocook/data-protection-certificate
cert_path=$cert_dir/web-data-protection.pem

for directory in "$key_ring" "$cert_dir"; do
    [ -d "$directory" ] && [ ! -L "$directory" ] \
        && [ "$(stat -c '%u:%g:%a' "$directory")" = '1654:1654:700' ] \
        || fail "Data Protection volume must be a directory owned by 1654:1654 with mode 0700"
done

# Hold the certificate directory open as the lock so concurrent bootstraps serialize.
exec 9<"$cert_dir"
flock -x -w 300 9 || fail "timed out waiting for the certificate directory lock"

# Remove staging directories left by an interrupted run; the lock guarantees no other writer.
for stale in "$cert_dir"/.staging.*; do
    if [ -d "$stale" ] && [ ! -L "$stale" ]; then
        rm -rf -- "$stale"
    fi
done

if [ -e "$cert_path" ] || [ -L "$cert_path" ]; then
    [ -f "$cert_path" ] && [ ! -L "$cert_path" ] \
        || fail "certificate is not a regular file"
    [ "$(stat -c '%u:%g:%a' "$cert_path")" = '1654:1654:400' ] \
        || fail "certificate must be owned by 1654:1654 with mode 0400"
    openssl x509 -in "$cert_path" -noout >/dev/null 2>&1 \
        && openssl pkey -in "$cert_path" -passin pass: -noout >/dev/null 2>&1 \
        || fail "existing certificate or private key is unreadable; restore the original certificate"
    printf 'web-data-protection-init: existing certificate retained\n'
    exit 0
fi

# An unreadable or failing listing must stop the run; only a successful empty listing means empty.
ring_entries=$(find "$key_ring" -mindepth 1 -maxdepth 1 -print -quit) \
    || fail "could not inspect the key ring"
if [ -n "$ring_entries" ]; then
    fail "key ring has entries but the certificate is missing; refusing to create a new certificate"
fi

staging=$(mktemp -d "$cert_dir/.staging.XXXXXX") || fail "could not create staging directory"
trap 'rm -rf -- "$staging"' EXIT

openssl req -x509 -newkey rsa:3072 -noenc -days 3650 \
    -subj "/CN=WhatToCook Web Data Protection" \
    -keyout "$staging/key.pem" -out "$staging/cert.pem" >/dev/null 2>&1 \
    || fail "certificate generation failed"

cat "$staging/cert.pem" "$staging/key.pem" > "$staging/web-data-protection.pem"
rm -f "$staging/cert.pem" "$staging/key.pem"
chmod 0400 "$staging/web-data-protection.pem"
sync -- "$staging/web-data-protection.pem"

# Hard link publishes atomically and fails if the target already exists, so nothing is replaced.
ln -T "$staging/web-data-protection.pem" "$cert_path" 2>/dev/null \
    || fail "could not publish certificate without replacing an existing file"
sync -- "$cert_dir"

printf 'web-data-protection-init: generated certificate\n'
