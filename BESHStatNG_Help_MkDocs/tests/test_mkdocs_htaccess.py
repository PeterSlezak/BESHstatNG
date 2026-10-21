"""Latest Help packaging through real direct and owner-wrapper MkDocs builds."""
from pathlib import Path
import os
import shutil
import subprocess
import sys
import sysconfig
import tempfile
import unittest
from unittest.mock import patch

from mkdocs.exceptions import PluginError

DOCS = Path(__file__).resolve().parents[1]
ROOT = DOCS.parent
sys.path.insert(0, str(DOCS))
import mkdocs_hooks as hook

RELATIVE_ARTIFACT = Path('assets/javascripts/.htaccess')
EXPECTED = b'<IfModule mod_mime.c>\n    AddType text/javascript .mjs\n</IfModule>\n'


def run_python(arguments, cwd=ROOT):
    env = os.environ.copy()
    # The wrapper first probes the CLI; use the same installed interpreter's CLI.
    env['PATH'] = sysconfig.get_path('scripts') + os.pathsep + env.get('PATH', '')
    env['PYTHONDONTWRITEBYTECODE'] = '1'
    return subprocess.run([sys.executable, '-B', *map(str, arguments)], cwd=cwd,
                          env=env, capture_output=True, text=True, timeout=120)


def isolated_project(directory):
    project = Path(directory) / 'help'
    project.mkdir()
    for name in ('mkdocs_beshstatng.py', 'mkdocs.base.yml', 'mkdocs.latest.yml',
                 'mkdocs.version.yml', 'mkdocs_hooks.py'):
        shutil.copy2(DOCS / name, project / name)
    shutil.copytree(DOCS / 'docs', project / 'docs')
    shutil.copy2(ROOT / 'Directory.Build.props', project.parent / 'Directory.Build.props')
    return project


def add_write_failure_hook(project):
    # A directory at the file path causes a portable real post-build write failure.
    (project / 'fail_mime_write.py').write_text(
        'from pathlib import Path\n'
        'from mkdocs.plugins import event_priority\n'
        '@event_priority(100)\n'
        'def on_post_build(config, **kwargs):\n'
        '    if config["site_url"].endswith("/latest/"):\n'
        '        (Path(config["site_dir"]) / "assets/javascripts/.htaccess").mkdir()\n',
        encoding='utf-8')
    base = project / 'mkdocs.base.yml'
    base.write_text(base.read_text(encoding='utf-8').replace(
        '  - mkdocs_hooks.py\n', '  - mkdocs_hooks.py\n  - fail_mime_write.py\n'),
        encoding='utf-8')


class LatestMimeBuildTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.temp = tempfile.TemporaryDirectory()
        cls.addClassCleanup(cls.temp.cleanup)
        cls.output = Path(cls.temp.name) / 'custom latest output'
        cls.default = DOCS / '.site/latest' / RELATIVE_ARTIFACT
        cls.default_before = cls.default.read_bytes() if cls.default.exists() else None
        cls.command = ['-m', 'mkdocs', 'build', '--config-file', DOCS / 'mkdocs.latest.yml',
                       '--site-dir', cls.output]
        cls.build = run_python(cls.command)

    def assert_success(self, result):
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)

    def test_direct_custom_output_has_exact_bytes_and_existing_resolver_assets(self):
        self.assert_success(self.build)
        artifact = self.output / RELATIVE_ARTIFACT
        self.assertEqual(EXPECTED, artifact.read_bytes())
        self.assertEqual(EXPECTED.decode('utf-8'), artifact.read_text(encoding='utf-8'))
        for name in ('help-resolver.mjs', 'help-routes.mjs'):
            relative = Path('assets/javascripts') / name
            self.assertEqual((DOCS / 'docs' / relative).read_bytes(),
                             (self.output / relative).read_bytes())
        page = self.output / 'help-resolver/index.html'
        self.assertEqual((DOCS / 'docs/help-resolver/index.html').read_bytes(), page.read_bytes())
        self.assertIn('type="module" src="../assets/javascripts/help-resolver.mjs"',
                      page.read_text(encoding='utf-8'))
        self.assertIn('https://www.beshstat.eu/beshstatng/help/latest/',
                      (self.output / 'index.html').read_text(encoding='utf-8'))
        after = self.default.read_bytes() if self.default.exists() else None
        self.assertEqual(self.default_before, after, 'custom output must not write to default output')

    def test_repeated_dirty_builds_repair_deleted_and_stale_output(self):
        self.assert_success(self.build)
        artifact = self.output / RELATIVE_ARTIFACT
        for state in ('identical', 'deleted', 'stale'):
            with self.subTest(state=state):
                if state == 'deleted':
                    artifact.unlink()
                elif state == 'stale':
                    artifact.write_bytes(b'AddType application/octet-stream .mjs\r\n')
                # --dirty proves repair is by the hook, independent of MkDocs cleaning.
                self.assert_success(run_python([*self.command, '--dirty']))
                self.assertEqual(EXPECTED, artifact.read_bytes())

    def test_owner_wrapper_regenerates_configs_and_builds_both_targets_from_either_cwd(self):
        for invocation in ('owner-folder', 'repository-root'):
            with self.subTest(invocation=invocation), tempfile.TemporaryDirectory() as directory:
                project = isolated_project(directory)
                if invocation == 'owner-folder':
                    cwd = project
                    arguments = ['mkdocs_beshstatng.py', '--mode', 'build', '--project', '.',
                                 '--props', '../Directory.Build.props']
                else:
                    cwd = project.parent
                    arguments = ['help/mkdocs_beshstatng.py', '--mode', 'build', '--project', 'help',
                                 '--props', 'Directory.Build.props']
                result = run_python(arguments, cwd)
                self.assert_success(result)
                self.assertIn('[INFO] Build complete.', result.stdout)
                self.assertEqual(EXPECTED, (project / '.site/latest' / RELATIVE_ARTIFACT).read_bytes())
                versions = list((project / '.site').glob('v*'))
                self.assertEqual(1, len(versions))
                self.assertTrue((versions[0] / 'index.html').is_file())
                self.assertTrue((versions[0] / 'assets/javascripts/help-resolver.mjs').is_file())
                self.assertFalse((versions[0] / RELATIVE_ARTIFACT).exists())

    def test_post_build_write_failure_is_nonzero_in_direct_and_wrapper_builds(self):
        with tempfile.TemporaryDirectory() as directory:
            project = isolated_project(directory)
            add_write_failure_hook(project)
            commands = [
                ['-m', 'mkdocs', 'build', '--config-file', 'mkdocs.latest.yml'],
                ['mkdocs_beshstatng.py', '--mode', 'build', '--project', '.',
                 '--props', '../Directory.Build.props'],
            ]
            for command in commands:
                with self.subTest(command=command):
                    result = run_python(command, project)
                    self.assertNotEqual(0, result.returncode, result.stdout + result.stderr)
                    self.assertIn('Cannot generate latest Help MIME configuration',
                                  result.stdout + result.stderr)
                    self.assertNotIn('[INFO] Build complete.', result.stdout + result.stderr)


class LatestMimeHookTests(unittest.TestCase):
    def test_latest_url_controls_target_independent_of_output_folder_name(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / 'v-test'
            hook.on_post_build({'site_url': 'https://example.test/custom/help/latest/?x=1',
                                'site_dir': str(output)})
            self.assertEqual(EXPECTED, (output / RELATIVE_ARTIFACT).read_bytes())

    def test_versioned_or_other_urls_do_not_create_output_directories(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / 'latest'
            for url in ('https://example.test/help/v1.2.6.0/',
                        'https://example.test/help/latestish/', '', None):
                with self.subTest(url=url):
                    hook.on_post_build({'site_url': url, 'site_dir': str(output)})
                    self.assertFalse(output.exists())

    def test_output_readback_failure_is_reported(self):
        with tempfile.TemporaryDirectory() as directory:
            with patch.object(Path, 'read_bytes', return_value=b'stale'):
                with self.assertRaisesRegex(PluginError, 'generated content differs'):
                    hook.on_post_build({'site_url': 'https://example.test/help/latest/',
                                        'site_dir': directory})


if __name__ == '__main__':
    unittest.main()
