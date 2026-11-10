#!/usr/bin/env python3
"""Offline M1.5 contract; never accesses Komodo, Docker, BWS or production."""
from pathlib import Path
import tomllib
import unittest

ROOT = Path(__file__).resolve().parent.parent


class WhatToCookOwnership(unittest.TestCase):
    def test_source_sync_owns_only_existing_app(self):
        doc = tomllib.loads((ROOT / "deploy/komodo.toml").read_text())
        self.assertEqual(set(doc), {"stack"})
        self.assertEqual(len(doc["stack"]), 1)
        stack = doc["stack"][0]
        self.assertEqual(stack["name"], "what-to-cook")
        self.assertEqual(stack["tags"], ["wtc"])
        cfg = stack["config"]
        expected = {
            "server": "Local",
            "repo": "Kyziur/WhatToCook",
            "run_directory": "deploy",
            "project_name": "whattocook",
            "file_paths": ["docker-compose.yml"],
        }
        for name, value in expected.items():
            self.assertEqual(cfg[name], value, name)
        self.assertFalse(cfg["auto_update"])
        self.assertFalse(cfg["webhook_enabled"])
        self.assertFalse(cfg.get("destroy_before_deploy", False))
        self.assertNotIn("--force-recreate", cfg.get("extra_args", []))
        self.assertIn("flock -x", cfg["compose_cmd_wrapper"])
        self.assertIn("[[COMPOSE_COMMAND]]", cfg["compose_cmd_wrapper"])
        self.assertEqual(cfg["compose_cmd_wrapper_include"], ["config", "build", "pull", "up", "run"])
        self.assertEqual(cfg["pre_deploy"]["command"], "flock -x -w 900 /etc/komodo/.homelab-maintenance.lock /usr/local/bin/prepare-what-to-cook .env")

    def test_no_secret_value_or_token_in_stack_environment(self):
        doc = tomllib.loads((ROOT / "deploy/komodo.toml").read_text())
        env = doc["stack"][0]["config"]["environment"]
        names = [line.split("=", 1)[0].strip() for line in env.splitlines() if "=" in line]
        self.assertEqual(set(names), {
            "DATA_ROOT", "POSTGRES_DB", "POSTGRES_USER",
            "ASPNETCORE_ENVIRONMENT", "SEED_DATA_ENABLED", "DOCKER_PLATFORM",
        })
        self.assertNotIn("POSTGRES_PASSWORD", env)
        self.assertNotIn("BWS_ACCESS_TOKEN", env)
        self.assertIn("DATA_ROOT = /srv/whattocook", env)

    def test_compose_project_preserves_named_postgres_volume_and_image_root(self):
        compose = (ROOT / "deploy/docker-compose.yml").read_text()
        self.assertIn("postgres-data:/var/lib/postgresql/data", compose)
        self.assertIn("  postgres-data:", compose)
        self.assertIn("/recipe-images:/data/recipe-images", compose)
        self.assertIn("POSTGRES_PASSWORD must be provided", compose)
        self.assertNotIn("container_name:", compose)


if __name__ == "__main__":
    unittest.main()
