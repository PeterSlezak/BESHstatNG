"""03.05: trusted destinations, deterministic Windows generation and preserved contexts."""
import copy
from pathlib import Path
import re
import sys
import tempfile
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / 'BESHstatNG/tools'))
import generate_help_links as windows
import generate_analysis_metadata as common
from validate_methods_catalog import _load_yaml, validate_catalog

YAML = ROOT / common.YAML_REL


class HelpMetadataTests(unittest.TestCase):
    def test_all_urls_and_topic_order_match_authored_metadata(self):
        meta = _load_yaml(YAML)
        source = common.render_catalog(ROOT)
        enum_source = (ROOT / 'BESHstatNG/src/Help/HelpLinks.vb').read_text(encoding='utf-8')
        enum = enum_source.split('Public Enum HelpTopic\n')[1].split('End Enum')[0]
        self.assertEqual(['Home'] + [m['key'] for m in meta['methods'] + meta['pages']], enum.split())
        for section, prefix in [('methods', 'methods/'), ('pages', '')]:
            for record in meta[section]:
                self.assertIn(f'"{meta["site"]["baseUrl"]}/latest/{prefix}{record["slug"]}/"', source)
        self.assertIn('HelpDestination.ForMethodKey(key)', enum_source)
        self.assertIn('HelpDestination.ForPageKey(key)', enum_source)
        self.assertNotIn('WebRequest', enum_source)

    def test_windows_generation_is_byte_stable_and_check_is_read_only(self):
        with tempfile.TemporaryDirectory() as directory:
            project = Path(directory)
            windows.generate(YAML, project)
            first = {p.name: p.read_bytes() for p in (project / 'src/Help').glob('*.vb')}
            windows.generate(YAML, project)
            second = {p.name: p.read_bytes() for p in (project / 'src/Help').glob('*.vb')}
            self.assertEqual(first, second)
            windows.generate(YAML, project, check=True)
            for name, content in first.items():
                self.assertEqual(content, (ROOT / 'BESHstatNG/src/Help' / name).read_bytes())
                self.assertNotIn(b'Generated: ', content)
            stale = project / 'src/Help/HelpContext.vb'
            stale.write_text('stale', encoding='utf-8')
            with self.assertRaisesRegex(ValueError, 'absent/stale'):
                windows.generate(YAML, project, check=True)
            self.assertEqual('stale', stale.read_text(encoding='utf-8'))
            missing = project / 'missing'
            with self.assertRaises(ValueError):
                windows.generate(YAML, missing, check=True)
            self.assertFalse(missing.exists())

    def test_context_discriminators_match_every_authored_form(self):
        meta = _load_yaml(YAML)
        source = (ROOT / 'BESHstatNG/src/Help/HelpContext.vb').read_text(encoding='utf-8')
        actual = dict(re.findall(r'\{"([^"\n]+)", HelpTopic\.(\w+)\}', source))
        expected = {}
        for record in meta['methods'] + meta['pages']:
            for form in record.get('forms', []):
                key = form['form'] + '|' + windows.normalize_for_key(form.get('analysisTitle') or record['title'])
                if key in expected:
                    self.assertEqual(expected[key], record['key'])
                expected[key] = record['key']
        self.assertEqual(expected, actual)
        self.assertIn('If TypeOf frm.Tag Is HelpTopic', source)
        self.assertIn('If [Enum].TryParse(Of HelpTopic)', source)

    def test_duplicate_title_across_forms_is_disambiguated_and_conflicts_are_rejected(self):
        meta = _load_yaml(YAML)
        # Existing shared title has distinct class/title context identities.
        meta['methods'][0]['forms'][0]['analysisTitle'] = 'Paired T-test'
        meta['methods'][1]['forms'][0]['analysisTitle'] = 'Paired T-test'
        with tempfile.TemporaryDirectory() as directory:
            candidate = Path(directory) / 'methods.yml'
            import yaml
            candidate.write_text(yaml.safe_dump(meta, allow_unicode=True), encoding='utf-8')
            problems, _ = validate_catalog(ROOT, candidate)
            self.assertFalse(any('conflict' in p.lower() or 'ambiguous' in p.lower() for p in problems), problems)
            meta['methods'][1]['forms'][0]['form'] = meta['methods'][0]['forms'][0]['form']
            candidate.write_text(yaml.safe_dump(meta, allow_unicode=True), encoding='utf-8')
            problems, _ = validate_catalog(ROOT, candidate)
            self.assertTrue(any('conflict' in p.lower() or 'ambiguous' in p.lower() for p in problems), problems)
            with self.assertRaisesRegex(ValueError, 'Invalid methods.yml'):
                windows.generate(candidate, Path(directory) / 'out')
            # Defensive renderer still omits ambiguity if validation is bypassed in isolation.
            with patch('validate_methods_catalog.validate_catalog', return_value=([], {})):
                windows.generate(candidate, Path(directory) / 'out')
            source = (Path(directory) / 'out/src/Help/HelpContext.vb').read_text(encoding='utf-8')
            key = meta['methods'][0]['forms'][0]['form'] + '|paired t test'
            self.assertNotIn(f'{{"{key}",', source)

    def test_untrusted_origin_or_path_is_rejected_before_generation(self):
        import yaml
        for root in ['http://www.beshstat.eu/beshstatng/help', 'https://evil.test',
                     'https://www.beshstat.eu@evil.test/help', 'https://www.beshstat.eu/beshstatng/help?next=x',
                     'https://www.beshstat.eu/beshstatng/help/../other']:
            with self.subTest(root=root), tempfile.TemporaryDirectory() as directory:
                meta = copy.deepcopy(_load_yaml(YAML))
                meta['site']['baseUrl'] = root
                candidate = Path(directory) / 'methods.yml'
                candidate.write_text(yaml.safe_dump(meta, allow_unicode=True), encoding='utf-8')
                self.assertTrue(any('trusted HTTPS' in e for e in validate_catalog(ROOT, candidate)[0]))
                with self.assertRaises(ValueError):
                    windows.generate(candidate, Path(directory) / 'out')
                self.assertFalse((Path(directory) / 'out').exists())


if __name__ == '__main__':
    unittest.main()
