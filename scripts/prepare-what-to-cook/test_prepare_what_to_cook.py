"""Host-path guard tests for scripts/prepare-what-to-cook/prepare-what-to-cook.

Ported from homelab tests/test_whattocook_host_predeploy.py; the homelab helper
is retired by this change. Never uses production /srv paths.

Stage 1 (any POSIX runner): a fake `docker` on PATH records its argv, proving
untrusted .env values cannot select a privileged mount and that the Docker
flags stay narrow. The embedded host script is executed locally with `/host-srv`
redirected to a temporary tree; a `stat` shim reports the owner so the owner and
mode gates are reached without root. The Komodo pre_deploy command from
deploy/komodo.toml runs for real from cwd=deploy with the lock path redirected
to a temporary file under the real `flock(1)`.

Stage 2 (root on a disposable POSIX runner with Docker): the exact captured
Docker argv runs against a scratch tree bound at /host-srv, covering the positive
idempotent path and symlink, untrusted-parent and leaf-ownership fail-closed
cases. Every refusal is asserted by its stderr message, not by exit status alone.

Set WTC_TEST_BASH to a bash binary (Git for Windows: C:/Program Files/Git/bin/bash.exe).
Set WTC_REQUIRE_REAL_DOCKER=1 in CI: any missing prerequisite then fails instead of skipping.
Run: sudo -E WTC_REQUIRE_REAL_DOCKER=1 python3 -m unittest discover -s scripts/prepare-what-to-cook -p 'test_*.py' -v
"""

import os
import shutil
import stat
import subprocess
import tempfile
import tomllib
import unittest
from pathlib import Path

HERE = Path(__file__).resolve().parent
HOOK = HERE / "prepare-what-to-cook"
REPO = HERE.parents[1]
KOMODO = REPO / "deploy" / "komodo.toml"
BASH = os.environ.get("WTC_TEST_BASH") or shutil.which("bash")
IS_ROOT = hasattr(os, "geteuid") and os.geteuid() == 0
REQUIRE_REAL = os.environ.get("WTC_REQUIRE_REAL_DOCKER") == "1"
LOCK_LITERAL = "/etc/komodo/.homelab-maintenance.lock"
COMMAND_PREFIX = f"flock -x -w 900 {LOCK_LITERAL} "
COMMAND_SUFFIX = "../scripts/prepare-what-to-cook/prepare-what-to-cook .env"

VALID_ENV = """DATA_ROOT=/srv/whattocook
POSTGRES_DB=whattocook
POSTGRES_USER=whattocook
ASPNETCORE_ENVIRONMENT=Production
SEED_DATA_ENABLED=false
DOCKER_PLATFORM=linux/arm64/v8
"""

FAKE_DOCKER = """#!/bin/sh
printf '%s\\0' "$@" > "$FAKE_DOCKER_LOG"
exit 0
"""

# Reports the owner from WTC_FAKE_OWNER (default root) for `%u:%g` and from
# WTC_FAKE_UID for `%u`; every other stat call goes to the real binary.
STAT_SHIM = """#!/bin/sh
fmt=""
prev=""
for a in "$@"; do
  if [ "$prev" = "-c" ]; then fmt="$a"; fi
  prev="$a"
done
case "$fmt" in
  '%u:%g') echo "${WTC_FAKE_OWNER:-0:0}" ;;
  '%u') echo "${WTC_FAKE_UID:-0}" ;;
  *) exec "$(PATH=/usr/bin:/bin command -v stat)" "$@" ;;
esac
"""

IMAGE = "busybox:1.36@sha256:73aaf090f3d85aa34ee199857f03fa3a95c8ede2ffd4cc2cdb5b94e566b11662"
MOUNT = "type=bind,source=/srv,target=/host-srv"
EXPECTED_PREFIX = [
    "run",
    "--rm",
    "--network",
    "none",
    "--read-only",
    "--security-opt",
    "no-new-privileges",
    "--cap-drop",
    "ALL",
    "--cap-add",
    "CHOWN",
    "--cap-add",
    "FOWNER",
    "--cap-add",
    "DAC_OVERRIDE",
    "--pids-limit",
    "32",
    "--mount",
    MOUNT,
    "--entrypoint",
    "sh",
    IMAGE,
    "-ec",
]
REFUSAL = "Untrusted WhatToCook host path"


def unavailable(test, reason):
    """Skip locally; fail when WTC_REQUIRE_REAL_DOCKER=1 so CI cannot pass silently."""
    if REQUIRE_REAL:
        test.fail(f"required by WTC_REQUIRE_REAL_DOCKER=1: {reason}")
    test.skipTest(reason)


class HookTestCase(unittest.TestCase):
    def setUp(self):
        if not BASH:
            unavailable(self, "a bash binary is required to run the hook")
        self.temp = tempfile.TemporaryDirectory(prefix="wtc-host-safety-")
        self.addCleanup(self.temp.cleanup)
        self.tmp = Path(self.temp.name)
        self.envfile = self.tmp / ".env"
        self.deploy = self.tmp / "deploy"
        self.deploy.mkdir()
        self.bin = self.tmp / "bin"
        self.bin.mkdir()
        docker = self.bin / "docker"
        docker.write_bytes(FAKE_DOCKER.encode())
        docker.chmod(0o755)
        self.log = self.tmp / "docker.log"
        self.environment = dict(
            os.environ,
            PATH=f"{self.bin}{os.pathsep}{os.environ.get('PATH', '')}",
            FAKE_DOCKER_LOG=str(self.log),
        )

    def call(self, text=VALID_ENV, arg=".env"):
        self.envfile.write_text(text, encoding="utf-8")
        return subprocess.run(
            [BASH, str(HOOK), str(self.envfile) if arg == ".env" else arg],
            cwd=self.deploy,
            env=self.environment,
            text=True,
            capture_output=True,
            timeout=20,
            check=False,
        )

    def captured_args(self):
        return self.log.read_bytes().decode().split("\0")[:-1]

    def assert_refused(self, result):
        self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertFalse(self.log.exists(), "docker must not run for refused input")


class HostPredeployTests(HookTestCase):
    def test_usage_requires_exactly_one_argument(self):
        result = subprocess.run(
            [BASH, str(HOOK)], cwd=self.deploy, env=self.environment,
            text=True, capture_output=True, timeout=20, check=False,
        )
        self.assert_refused(result)

    def test_missing_env_file_is_refused(self):
        result = subprocess.run(
            [BASH, str(HOOK), str(self.tmp / "missing.env")], cwd=self.deploy,
            env=self.environment, text=True, capture_output=True, timeout=20, check=False,
        )
        self.assert_refused(result)

    def test_trusted_env_uses_only_narrow_host_mount(self):
        result = self.call('DATA_ROOT = "/srv/whattocook"\n')
        self.assertEqual(result.returncode, 0, result.stderr)
        args = self.captured_args()
        self.assertEqual(args[: len(EXPECTED_PREFIX)], EXPECTED_PREFIX)
        self.assertEqual(len(args), len(EXPECTED_PREFIX) + 1, "exactly one host script argument")
        self.assertEqual(args.count("--mount"), 1, "only the /srv bind is allowed")
        self.assertNotIn("-v", args)
        self.assertNotIn("type=bind,source=/,target=/host", args)
        self.assertNotIn("--privileged", args)
        self.assertEqual(args[-2], "-ec")
        self.assertIn("trusted_parent", args[-1])
        self.assertIn('leaf="$parent/recipe-images"', args[-1])

    def test_host_script_keeps_safety_gates(self):
        self.assertEqual(self.call().returncode, 0)
        script = self.captured_args()[-1]
        for needle in [
            "trusted_parent /host-srv",
            'trusted_parent "$parent"',
            '[ ! -L "$leaf" ] || fail',
            'case "$leaf_uid" in 0|1654)',
            'chown 1654:1654 "$leaf"',
        ]:
            with self.subTest(needle=needle):
                self.assertIn(needle, script)

    def test_reject_dangerous_data_root_and_duplicate_keys_before_docker(self):
        for raw in (
            "DATA_ROOT=/\n",
            "DATA_ROOT=/srv\n",
            "DATA_ROOT=/srv/whattocook/../elsewhere\n",
            "DATA_ROOT=/srv/whattocook/evil\n",
            "DATA_ROOT=/srv/whattocook\nDATA_ROOT=/\n",
            "DATA_ROOT=[[MISSING]]\n",
            "DATA_ROOT=$(touch /tmp/should-not-exist)\n",
            "DATA_ROOT=/srv/whattocook; touch /tmp/should-not-exist\n",
            "DATA_ROOT= \n",
            "OTHER=1\n",
        ):
            with self.subTest(raw=raw):
                self.log.unlink(missing_ok=True)
                result = self.call(raw)
                self.assert_refused(result)
                self.assertNotIn("$(touch", result.stderr)

    def test_reject_symlinked_env(self):
        target = self.tmp / "safe.env"
        target.write_text("DATA_ROOT=/srv/whattocook\n")
        link = self.tmp / "linked.env"
        try:
            os.symlink(target, link)
        except OSError as exc:
            unavailable(self, f"symlinks unavailable: {exc}")
        result = subprocess.run(
            [BASH, str(HOOK), str(link)], cwd=self.deploy, env=self.environment,
            text=True, capture_output=True, timeout=20, check=False,
        )
        self.assert_refused(result)


@unittest.skipUnless(os.name == "posix", "symlink and mode checks need POSIX")
class HostScriptShimTests(HookTestCase):
    """Runs the embedded host script locally; `stat` owner is faked, paths are temporary."""

    def run_host_script(self, host_srv, owner="0:0", uid="0"):
        # The shipped script binds /srv at /host-srv inside the container; tests
        # redirect that literal path to a temporary tree to exercise the checks.
        script = self.captured_args()[-1].replace("/host-srv", host_srv.as_posix())
        shim_dir = self.tmp / "stat-shim"
        shim_dir.mkdir(exist_ok=True)
        stat_bin = shim_dir / "stat"
        stat_bin.write_bytes(STAT_SHIM.encode())
        stat_bin.chmod(0o755)
        env = dict(
            os.environ,
            PATH=f"{shim_dir}{os.pathsep}{os.environ.get('PATH', '')}",
            WTC_FAKE_OWNER=owner,
            WTC_FAKE_UID=uid,
        )
        return subprocess.run([BASH, "-ec", script], env=env, text=True, capture_output=True, timeout=60)

    def make_srv(self):
        srv = self.tmp / "host-srv"
        srv.mkdir(mode=0o755)
        return srv

    def make_symlink(self, target, link):
        try:
            os.symlink(target, link, target_is_directory=True)
        except OSError as exc:
            unavailable(self, f"symlinks unavailable: {exc}")

    def test_host_script_refuses_symlinked_leaf_and_leaves_target_untouched(self):
        self.assertEqual(self.call().returncode, 0)
        srv = self.make_srv()
        root = srv / "whattocook"
        root.mkdir(mode=0o755)
        victim = self.tmp / "victim"
        victim.mkdir(mode=0o700)
        self.make_symlink(victim, root / "recipe-images")

        result = self.run_host_script(srv)

        self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn(REFUSAL, result.stderr)
        self.assertEqual(stat.S_IMODE(victim.stat().st_mode), 0o700)

    def test_host_script_refuses_symlinked_parent(self):
        self.assertEqual(self.call().returncode, 0)
        srv = self.make_srv()
        victim = self.tmp / "victim"
        victim.mkdir(mode=0o755)
        self.make_symlink(victim, srv / "whattocook")

        result = self.run_host_script(srv)

        self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn(REFUSAL, result.stderr)
        self.assertFalse((victim / "recipe-images").exists())

    def test_host_script_refuses_group_writable_parent(self):
        self.assertEqual(self.call().returncode, 0)
        srv = self.make_srv()
        root = srv / "whattocook"
        root.mkdir(mode=0o755)
        root.chmod(0o777)

        result = self.run_host_script(srv)

        self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn(REFUSAL, result.stderr)
        self.assertFalse((root / "recipe-images").exists())

    def test_host_script_refuses_user_owned_parent_with_0755(self):
        self.assertEqual(self.call().returncode, 0)
        srv = self.make_srv()
        root = srv / "whattocook"
        root.mkdir(mode=0o755)

        result = self.run_host_script(srv, owner="1000:1000")

        self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn(REFUSAL, result.stderr)
        self.assertFalse((root / "recipe-images").exists())

    def test_host_script_refuses_leaf_owned_by_untrusted_uid(self):
        self.assertEqual(self.call().returncode, 0)
        srv = self.make_srv()
        root = srv / "whattocook"
        root.mkdir(mode=0o755)
        (root / "recipe-images").mkdir(mode=0o750)

        result = self.run_host_script(srv, uid="1000")

        self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn(REFUSAL, result.stderr)


class KomodoPreDeployCommandTests(HookTestCase):
    """Runs the exact pre_deploy command from deploy/komodo.toml from cwd=deploy."""

    def setUp(self):
        super().setUp()
        if os.name != "posix" or shutil.which("flock") is None:
            unavailable(self, "POSIX flock(1) is required for the lock fixture")

    def stack_pre_deploy_command(self):
        data = tomllib.loads(KOMODO.read_text(encoding="utf-8"))
        found = []

        def walk(node):
            if isinstance(node, dict):
                for value in node.values():
                    walk(value)
            elif isinstance(node, list):
                for value in node:
                    walk(value)
            elif isinstance(node, str) and "prepare-what-to-cook" in node:
                found.append(node)

        walk(data)
        self.assertEqual(len(found), 1, found)
        return found[0]

    def test_pre_deploy_command_keeps_lock_and_runs_hook_from_deploy_dir(self):
        command = self.stack_pre_deploy_command()
        self.assertTrue(command.startswith(COMMAND_PREFIX), command)
        self.assertTrue(command.endswith(COMMAND_SUFFIX), command)

        repo = self.tmp / "repo"
        deploy = repo / "deploy"
        deploy.mkdir(parents=True)
        hook_dir = repo / "scripts" / "prepare-what-to-cook"
        hook_dir.mkdir(parents=True)
        shutil.copy2(HOOK, hook_dir / "prepare-what-to-cook")
        (deploy / ".env").write_text(VALID_ENV, encoding="utf-8")
        lock = self.tmp / "maintenance.lock"
        lock.touch()

        local = command.replace(LOCK_LITERAL, str(lock))
        result = subprocess.run(
            local, shell=True, cwd=deploy, env=self.environment,
            text=True, capture_output=True, timeout=120, check=False,
        )

        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        args = self.captured_args()
        self.assertEqual(args[: len(EXPECTED_PREFIX)], EXPECTED_PREFIX)
        self.assertEqual(args.count("--mount"), 1)


class HelperFileTests(HookTestCase):
    def test_helper_is_regular_non_symlink_file(self):
        self.assertTrue(HOOK.is_file(), HOOK)
        self.assertFalse(HOOK.is_symlink(), HOOK)

    @unittest.skipUnless(os.name == "posix", "executable bit is POSIX-only")
    def test_helper_is_executable(self):
        self.assertTrue(os.access(HOOK, os.X_OK), HOOK)

    @unittest.skipUnless(
        os.name == "posix",
        "Windows checkouts may apply core.autocrlf; the stored blob is LF via .gitattributes",
    )
    def test_helper_uses_lf_line_endings(self):
        # .gitattributes pins eol=lf; a CRLF checkout breaks bash parsing of the hook.
        self.assertNotIn(b"\r", HOOK.read_bytes())


@unittest.skipUnless(os.name == "posix", "real Docker tests need a POSIX runner")
class RealDockerTests(HookTestCase):
    def setUp(self):
        super().setUp()
        missing = []
        if not IS_ROOT:
            missing.append("root")
        if not shutil.which("docker"):
            missing.append("docker on PATH")
        if missing:
            unavailable(self, "real Docker tests need " + ", ".join(missing))

    def docker_run(self, source, argv):
        args = list(argv)
        args[args.index("--mount") + 1] = f"type=bind,source={source},target=/host-srv"
        return subprocess.run(
            [shutil.which("docker"), *args], capture_output=True, text=True, timeout=120
        )

    def scratch_srv(self, name="scratch-srv"):
        srv = self.tmp / name
        srv.mkdir(mode=0o755)
        os.chown(srv, 0, 0)
        os.chmod(srv, 0o755)
        return srv

    def test_real_docker_positive_and_idempotent_without_content_mutation(self):
        self.assertEqual(self.call().returncode, 0)
        argv = self.captured_args()
        srv = self.scratch_srv()
        for attempt in (1, 2):
            result = self.docker_run(srv, argv)
            self.assertEqual(result.returncode, 0, result.stderr)
            leaf = srv / "whattocook" / "recipe-images"
            self.assertEqual(leaf.stat().st_uid, 1654)
            self.assertEqual(leaf.stat().st_gid, 1654)
            self.assertEqual(leaf.stat().st_mode & 0o777, 0o750)
            if attempt == 1:
                (leaf / "sentinel.txt").write_text("keep image data")
            else:
                self.assertEqual((leaf / "sentinel.txt").read_text(), "keep image data")

    def test_real_docker_symlinked_leaf_and_parent_fail_closed(self):
        self.assertEqual(self.call().returncode, 0)
        argv = self.captured_args()
        srv = self.scratch_srv()
        target = srv / "victim"
        target.mkdir(mode=0o700)
        parent = srv / "whattocook"
        parent.mkdir()
        leaf = parent / "recipe-images"
        leaf.symlink_to(target, target_is_directory=True)

        result = self.docker_run(srv, argv)
        self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn(REFUSAL, result.stderr)
        self.assertEqual(target.stat().st_uid, 0)
        self.assertEqual(target.stat().st_mode & 0o777, 0o700)

        leaf.unlink()
        parent.rmdir()
        parent.symlink_to(target, target_is_directory=True)
        result = self.docker_run(srv, argv)
        self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn(REFUSAL, result.stderr)
        self.assertFalse((target / "recipe-images").exists())

    def test_real_docker_untrusted_parent_fail_closed(self):
        self.assertEqual(self.call().returncode, 0)
        argv = self.captured_args()

        group_writable = self.scratch_srv("srv-group-writable")
        (group_writable / "whattocook").mkdir()
        os.chmod(group_writable / "whattocook", 0o777)
        result = self.docker_run(group_writable, argv)
        self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn(REFUSAL, result.stderr)
        self.assertFalse((group_writable / "whattocook" / "recipe-images").exists())

        user_owned = self.scratch_srv("srv-user-owned")
        parent = user_owned / "whattocook"
        parent.mkdir()
        os.chown(parent, 1000, 1000)
        os.chmod(parent, 0o755)
        result = self.docker_run(user_owned, argv)
        self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn(REFUSAL, result.stderr)
        self.assertEqual(parent.stat().st_uid, 1000)
        self.assertFalse((parent / "recipe-images").exists())

    def test_real_docker_leaf_ownership_fail_closed(self):
        self.assertEqual(self.call().returncode, 0)
        argv = self.captured_args()
        srv = self.scratch_srv()
        parent = srv / "whattocook"
        parent.mkdir()
        leaf = parent / "recipe-images"
        leaf.mkdir()
        os.chown(leaf, 1000, 1000)

        result = self.docker_run(srv, argv)
        self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn(REFUSAL, result.stderr)
        self.assertEqual(leaf.stat().st_uid, 1000)


if __name__ == "__main__":
    unittest.main()
