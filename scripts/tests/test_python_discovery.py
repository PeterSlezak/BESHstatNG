"""Exercise real PowerShell Python discovery without installing packages."""
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
POWERSHELL = shutil.which('powershell.exe')


@unittest.skipUnless(POWERSHELL, 'Windows PowerShell is required for the Windows build resolver')
class PythonDiscoveryTests(unittest.TestCase):
    def resolve(self, *arguments, env=None):
        return subprocess.run([
            POWERSHELL, '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File',
            str(ROOT / 'scripts/resolve-beshstatng-python.ps1'), *arguments,
        ], capture_output=True, text=True, env=env, timeout=30)

    def test_explicit_interpreter(self):
        result = self.resolve('-PythonExe', sys.executable)
        self.assertEqual(0, result.returncode, result.stderr)
        self.assertEqual(Path(sys.executable).resolve(), Path(result.stdout.strip()).resolve())

    def test_path_discovery(self):
        env = {key: value for key, value in os.environ.items() if key.upper() != 'PYTHONEXE'}
        env['PATH'] = str(Path(sys.executable).parent)
        result = self.resolve(env=env)
        self.assertEqual(0, result.returncode, result.stderr)
        self.assertEqual(Path(sys.executable).resolve(), Path(result.stdout.strip()).resolve())

    def test_environment_override(self):
        env = {key: value for key, value in os.environ.items() if key.upper() != 'PYTHONEXE'}
        env['PythonExe'] = sys.executable
        result = self.resolve(env=env)
        self.assertEqual(0, result.returncode, result.stderr)
        self.assertEqual(Path(sys.executable).resolve(), Path(result.stdout.strip()).resolve())

    def test_invalid_explicit_interpreter_does_not_fall_back(self):
        with tempfile.TemporaryDirectory(prefix='besh python test ') as directory:
            result = self.resolve('-PythonExe', str(Path(directory) / 'missing python.exe'))
        self.assertNotEqual(0, result.returncode)
        self.assertIn('Python discovery failed', result.stderr)
        self.assertIn('PyYAML', result.stderr)
        self.assertEqual('', result.stdout)

    def test_empty_path_has_actionable_failure(self):
        with tempfile.TemporaryDirectory() as directory:
            env = {key: value for key, value in os.environ.items() if key.upper() != 'PYTHONEXE'}
            env['PATH'] = directory
            result = self.resolve(env=env)
        self.assertNotEqual(0, result.returncode)
        self.assertIn('PythonExe', result.stderr)
        self.assertIn('No packages were installed', result.stderr)


if __name__ == '__main__':
    unittest.main()
