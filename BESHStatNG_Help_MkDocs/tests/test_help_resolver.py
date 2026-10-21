"""03.05 amendment: deployable static resolver and authored metadata parity, offline."""
from pathlib import Path
import json
import re
import subprocess
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
DOCS = ROOT / 'BESHStatNG_Help_MkDocs'
sys.path.insert(0, str(DOCS))
import generate_help_resolver_routes as generator


class HelpResolverBuildTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.temp = tempfile.TemporaryDirectory()
        cls.output = Path(cls.temp.name) / 'site'
        cls.build = subprocess.run([sys.executable, '-B', '-m', 'mkdocs', 'build',
                                   '--config-file', str(DOCS / 'mkdocs.latest.yml'), '--site-dir', str(cls.output)],
                                  cwd=ROOT, capture_output=True, text=True, timeout=120)

    @classmethod
    def tearDownClass(cls):
        cls.temp.cleanup()

    def test_real_mkdocs_build_preserves_resolver_html_and_module_paths(self):
        self.assertEqual(0, self.build.returncode, self.build.stdout + self.build.stderr)
        page = self.output / 'help-resolver/index.html'
        self.assertEqual((DOCS / 'docs/help-resolver/index.html').read_bytes(), page.read_bytes())
        html = page.read_text(encoding='utf-8')
        self.assertIn('type="module" src="../assets/javascripts/help-resolver.mjs"', html)
        self.assertIn('role="status"', html)
        self.assertIn('<noscript>', html)
        self.assertIn('href="https://www.beshstat.eu/beshstatng/help/latest/"', html)
        for asset in ['help-resolver.mjs', 'help-routes.mjs']:
            self.assertEqual((DOCS / 'docs/assets/javascripts' / asset).read_bytes(),
                             (self.output / 'assets/javascripts' / asset).read_bytes())

    def test_resolver_is_unlisted_and_not_a_method_or_utility(self):
        self.assertNotIn('help-resolver', (DOCS / 'mkdocs.base.yml').read_text(encoding='utf-8'))
        self.assertNotIn('help-resolver', generator.YAML.read_text(encoding='utf-8'))
        source = generator.render()
        routes = json.loads(source.split('Object.freeze(')[1].split(');')[0])
        self.assertEqual(79, len(routes))
        self.assertEqual('', routes['Home'])
        self.assertEqual('global-settings/', routes['GlobalSettings'])
        self.assertEqual('methods/one-way-anova/', routes['OneWayANOVA'])

    def test_generated_route_check_is_stable_and_does_not_modify_stale_output(self):
        expected = generator.render()
        self.assertEqual(expected, generator.render())
        self.assertEqual(expected, generator.OUTPUT.read_text(encoding='utf-8'))
        with tempfile.TemporaryDirectory() as directory:
            stale = Path(directory) / 'routes.mjs'
            stale.write_text('stale', encoding='utf-8')
            result = subprocess.run([sys.executable, '-B', str(DOCS / 'generate_help_resolver_routes.py'),
                                     '--check', '--output', str(stale)], capture_output=True, text=True)
            self.assertNotEqual(0, result.returncode)
            self.assertEqual('stale', stale.read_text(encoding='utf-8'))

    def test_actual_built_method_and_home_documents_pass_js_identity_checks(self):
        self.assertEqual(0, self.build.returncode, self.build.stderr)
        # Execute the same browser verifier on every actual local output document.
        code = '''
import fs from 'node:fs';
import path from 'node:path';
import assert from 'node:assert/strict';
const verifier = await import(process.argv[2]);
const metadata = await import(process.argv[3]);
for (const relative of Object.values(metadata.routes)) {
    const html = fs.readFileSync(path.join(process.argv[1], relative, 'index.html'), 'utf8');
    assert.ok(verifier.usableDocument(html, metadata.root + 'latest/' + relative), relative || 'Home');
}
const soft404 = fs.readFileSync(path.join(process.argv[1], '404.html'), 'utf8');
assert.equal(verifier.usableDocument(soft404, metadata.root + 'latest/methods/one-way-anova/'), false);
'''
        result = subprocess.run(['node', '--input-type=module', '--eval', code, str(self.output),
                                 (DOCS / 'docs/assets/javascripts/help-resolver.mjs').as_uri(),
                                 generator.OUTPUT.as_uri()], capture_output=True, text=True)
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)


if __name__ == '__main__':
    unittest.main()
