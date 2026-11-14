import os, unittest
from unittest.mock import patch
sys.path.insert(0, str(ROOT / 'BESHstatNG/tools'))
import audit_transitional_officejs as audit
before = sha(Path(audit.__file__).read_bytes())
original = audit.audit_solution
original_glob = Path.rglob
counters = dict(ignored_directories_pruned=0, relevant_files=0)

def bounded_glob(path, pattern, *args, **kwargs):
    if path.resolve() != ROOT or pattern != '*':
        return original_glob(path, pattern, *args, **kwargs)

    def walk():
        for directory, dirs, files in os.walk(ROOT, followlinks=False):
            kept = [name for name in dirs if name not in audit.IGNORED_COMPONENTS]
            counters['ignored_directories_pruned'] += len(dirs) - len(kept)
            dirs[:] = kept
            for name in files:
                p = Path(directory) / name
                if p.suffix.lower() in audit.SOURCE_SUFFIXES:
                    counters['relevant_files'] += 1
                    yield p
    return walk()

def bounded_audit(root, **kwargs):
    if Path(root).resolve() != ROOT:
        return original(root, **kwargs)
    with patch.object(Path, 'rglob', bounded_glob):
        return original(root, **kwargs)
audit.audit_solution = bounded_audit
