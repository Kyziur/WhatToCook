"""Exercise check-line-endings.py against disposable Git repositories."""

import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CHECKER = Path(__file__).resolve().parent / "check-line-endings.py"
POLICY = b"* text=auto eol=lf\n*.png binary\n*.bat text eol=crlf\n"
LF_SCRIPT = b"#!/usr/bin/env bash\necho ok\n"


def git(repo, *args, input=None):
    command = ["git", "-C", str(repo), "-c", "user.name=ci", "-c", "user.email=ci@example.invalid", *args]
    return subprocess.run(command, check=True, capture_output=True, input=input).stdout.decode().strip()


class LineEndingCheckerTest(unittest.TestCase):
    def setUp(self):
        scratch = ROOT / ".tmp" / "line-endings-tests"
        scratch.mkdir(parents=True, exist_ok=True)
        tmp = tempfile.TemporaryDirectory(dir=scratch)
        self.addCleanup(tmp.cleanup)
        self.repo = Path(tmp.name) / "repo"
        self.repo.mkdir()
        git(self.repo, "init", "-q")
        git(self.repo, "config", "core.autocrlf", "false")

    def commit(self, files, executable=()):
        """Store bytes verbatim (no clean filter) so a CRLF blob can be committed, then check out."""
        for name, data in files.items():
            sha = git(self.repo, "hash-object", "-w", "--no-filters", "--stdin", input=data)
            mode = "100755" if name in executable else "100644"
            git(self.repo, "update-index", "--add", "--cacheinfo", mode, sha, name)
        git(self.repo, "commit", "-q", "-m", "fixture")
        git(self.repo, "checkout-index", "--all", "--force")

    def run_checker(self):
        return subprocess.run(
            [sys.executable, str(CHECKER), "--repo", str(self.repo)],
            capture_output=True,
            text=True,
            check=False,
        )

    def test_lf_repository_with_binary_and_exception_passes(self):
        self.commit(
            {
                ".gitattributes": POLICY,
                "scripts/entry": LF_SCRIPT,
                "deploy/app.yml": b"name: app\n",
                "empty.txt": b"",
                "no-newline.txt": b"no newline",
                "assets/logo.png": b"\x89PNG\r\n\x1a\n\x00\r\n",
                "legacy.bat": b"@echo off\r\n",
            },
            executable={"scripts/entry"},
        )

        result = self.run_checker()

        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn("passed", result.stdout)

    def test_committed_crlf_shell_script_without_extension_fails(self):
        self.commit({".gitattributes": POLICY, "scripts/entry": b"#!/usr/bin/env bash\r\necho ok\r\n"})

        result = self.run_checker()

        self.assertEqual(result.returncode, 1)
        self.assertIn("scripts/entry: index stores crlf (expected lf)", result.stdout)

    def test_committed_crlf_yaml_fails(self):
        self.commit({".gitattributes": POLICY, "deploy/app.yml": b"name: app\r\n"})

        result = self.run_checker()

        self.assertEqual(result.returncode, 1)
        self.assertIn("deploy/app.yml: index stores crlf (expected lf)", result.stdout)

    def test_mixed_endings_in_index_fail(self):
        self.commit({".gitattributes": POLICY, "deploy/app.yml": b"a: 1\r\nb: 2\n"})

        result = self.run_checker()

        self.assertEqual(result.returncode, 1)
        self.assertIn("deploy/app.yml: index stores mixed (expected lf)", result.stdout)

    def test_crlf_in_checkout_fails_even_when_index_is_lf(self):
        self.commit({".gitattributes": POLICY, "deploy/app.yml": b"name: app\n"})
        (self.repo / "deploy" / "app.yml").write_bytes(b"name: app\r\n")

        result = self.run_checker()

        self.assertEqual(result.returncode, 1)
        self.assertIn("deploy/app.yml: checkout has crlf (expected lf)", result.stdout)

    def test_autocrlf_checkout_keeps_lf_with_policy(self):
        self.commit({".gitattributes": POLICY, "deploy/app.yml": b"name: app\n", "scripts/entry": LF_SCRIPT})
        git(self.repo, "-c", "core.autocrlf=true", "checkout-index", "--all", "--force")

        result = self.run_checker()

        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)

    def test_autocrlf_checkout_without_policy_is_rejected(self):
        self.commit({"deploy/app.yml": b"name: app\n", "scripts/entry": LF_SCRIPT})
        git(self.repo, "-c", "core.autocrlf=true", "checkout-index", "--all", "--force")

        result = self.run_checker()

        self.assertEqual(result.returncode, 1)
        self.assertIn("checkout has crlf (expected lf)", result.stdout)


if __name__ == "__main__":
    unittest.main()
