#!/usr/bin/env python3
"""Reject CRLF in governed text, both stored in the index and present in the checkout.

Uses `git ls-files --eol -z`, so Git's own binary and attribute classification applies.
`i/` describes the committed index bytes and does not depend on core.autocrlf.
`w/` describes the working tree, which shows what a checkout produced.
Binary paths (`-text`) and paths with an explicit `eol=crlf` attribute are skipped.
"""

import argparse
import re
import subprocess
import sys

RECORD = re.compile(r"^i/(\S+)\s+w/(\S+)\s+attr/(.*)$")
LF_ONLY = {"none", "lf"}


def parse_eol(output: bytes):
    for record in output.split(b"\0"):
        if not record:
            continue
        meta, _, path = record.partition(b"\t")
        match = RECORD.match(meta.decode("utf-8", "surrogateescape").strip())
        if match is None:
            raise ValueError(f"unexpected git ls-files --eol record: {record!r}")
        index_eol, worktree_eol, attrs = match.groups()
        yield path.decode("utf-8", "surrogateescape"), index_eol, worktree_eol, attrs.split()


def violations(entries):
    for path, index_eol, worktree_eol, attrs in entries:
        if index_eol == "-text" or worktree_eol == "-text":
            continue
        if "eol=crlf" in attrs:
            continue
        if index_eol not in LF_ONLY:
            yield f"{path}: index stores {index_eol} (expected lf)"
        if worktree_eol not in LF_ONLY:
            yield f"{path}: checkout has {worktree_eol} (expected lf)"


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--repo", default=".", help="repository to check (default: current directory)")
    args = parser.parse_args(argv)

    result = subprocess.run(
        ["git", "-C", args.repo, "ls-files", "--eol", "-z"],
        capture_output=True,
        check=False,
    )
    if result.returncode != 0:
        sys.stderr.write(result.stderr.decode("utf-8", "replace"))
        return 2

    entries = list(parse_eol(result.stdout))
    problems = list(violations(entries))
    for problem in problems:
        print(problem)
    if problems:
        print(f"line-ending check failed: {len(problems)} problem(s); governed text must be LF", file=sys.stderr)
        return 1
    print(f"line-ending check passed: {len(entries)} tracked path(s) inspected")
    return 0


if __name__ == "__main__":
    sys.exit(main())
