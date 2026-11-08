"""Use the accepted isolated, offline runner; do not overwrite earlier runs."""
from pathlib import Path
import importlib.util
import sys

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('isolated37', ROOT / 'scripts/test_suite_rationalization.py')
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)

if __name__ == '__main__':
    action, config, label = sys.argv[1:4]
    private = ROOT / '.test-rationalization-04A37.tmp'
    private.mkdir(exist_ok=True)
    runner = m.Runner(private)
    if action == 'build':
        result = runner.build(config, label)
    elif action == 'managed':
        before = m.source_pins()
        result = dict(configuration=config, results=[runner.test(config, label, s, label+'-'+config+'-serial-'+s)
                      for s in ['Core', 'Common', 'Legacy', 'ControlMitigation', 'OfficeAddin', 'OfficeExecution']],
                      source_unchanged=before == m.source_pins())
        m.save(private / (label+'-'+config+'-managed.json'), result)
    elif action == 'matrix':
        result = runner.matrix(config, label)
    elif action == 'serial':
        result = runner.serial(label)
    else:
        raise ValueError(action)
    sys.exit(1 if (any(result['exit_codes']) if action == 'build' else m.failed(result)) else 0)
