"""04A.01 amendment preflight/checks; preserves evidence, never stages files."""
from __future__ import annotations
import argparse
import csv
import hashlib
import json
import pathlib
import shutil
import subprocess

ROOT = pathlib.Path(__file__).resolve().parents[2]
LEDGER = ROOT / "docs/architecture/WindowsMigrationLedger"
SNAPSHOT = ROOT / "TestResults/04A-01-amendment"


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def git(*args):
    return subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), *args], cwd=ROOT, text=True).rstrip()


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def rows(path):
    with path.open(encoding="utf-8-sig", newline="") as stream:
        return list(csv.DictReader(stream))


def capture():
    manifest = SNAPSHOT / "before.json"
    if manifest.exists():
        raise RuntimeError("Amendment snapshot exists; refusing to replace original evidence")
    assert git("branch", "--show-current") == "codex/04A-01"
    assert not git("diff", "--name-only") and not git("diff", "--cached", "--name-only")
    SNAPSHOT.mkdir(parents=True, exist_ok=True)
    paths = list(LEDGER.iterdir()) + list((ROOT / "scripts/windows-migration-audit").iterdir())
    paths += [ROOT / "docs/architecture/WindowsMigrationLedger.md"]
    paths += list((ROOT / "TestResults/04A-01").glob("*.json")) + list((ROOT / "TestResults/04A-01").glob("*.log"))
    # Product-suite results and logs are historical evidence, retained before a rerun.
    for directory in ("Debug", "Release"):
        paths += list((ROOT / "TestResults/04A-01" / directory).rglob("*"))
    original = []
    for path in sorted(set(p for p in paths if p.is_file())):
        relative = path.relative_to(ROOT).as_posix()
        target = SNAPSHOT / "original" / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(path, target)
        original.append({"path": relative, "sha256": sha(path)})
    index = read(LEDGER / "index.json")
    index_errors = [r["path"] for r in index["artifacts"] if sha(ROOT / r["path"]) != r["sha256"]]
    source_errors = [r["path"] for r in rows(LEDGER / "files.csv") if sha(ROOT / r["path"]) != r["sha256"]]
    tracked = [{"path": p, "sha256": sha(ROOT / p)} for p in git("ls-files").splitlines()]
    gate = read(ROOT / "TestResults/04A-01/gate-c-hashes-before.json")
    gate_errors = [r["path"] for r in gate if sha(ROOT / r["path"]) != r["sha256"]]
    before = read(ROOT / "TestResults/04A-01/protected/before.json")
    protected_errors = [r["path"] for r in before if sha(ROOT / r["path"]) != r["sha256"]]
    changed_vb = git("diff-tree", "--no-commit-id", "--name-only", "-r", "HEAD", "--", "BESHstatNG/*.vb").splitlines()
    state = {"branch": git("branch", "--show-current"), "head": git("rev-parse", "HEAD"), "main": git("rev-parse", "main"),
             "status": git("status", "--short", "--untracked-files=all"), "ignored_status": git("status", "--short", "--untracked-files=all", "--ignored"),
             "index_entries": len(index["artifacts"]), "index_errors": index_errors, "compiled_files": len(rows(LEDGER / "files.csv")), "source_hash_errors": source_errors,
             "gate_files": len(gate), "gate_errors": gate_errors, "protected_files": len(before), "protected_errors": protected_errors,
             "head_cleanup_windows_vb": changed_vb, "head_cleanup_diff": git("show", "--format=fuller", "--stat", "HEAD"),
             "zip_comparison": "No ZIP exists under this checkout. Cannot independently compare the earlier ZIP; current index and all source hashes verified. Git HEAD contains 19 Windows VB cleanup paths; tracked worktree is clean.",
             "original_artifacts": original, "tracked": tracked, "gate": gate}
    manifest.write_text(json.dumps(state, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({k: v for k, v in state.items() if k in ("branch", "head", "main", "index_entries", "index_errors", "compiled_files", "source_hash_errors", "gate_files", "gate_errors", "protected_files", "protected_errors", "zip_comparison")}, indent=2))
    assert not (index_errors or source_errors or gate_errors or protected_errors), "Preexisting evidence drift; stop before modifying audit"


def check():
    before = read(SNAPSHOT / "before.json")
    errors = []
    for label, records, base in (("tracked", before["tracked"], ROOT), ("GateC", before["gate"], ROOT),
                                 ("original-audit", before["original_artifacts"], SNAPSHOT / "original")):
        bad = [r["path"] for r in records if not (base / r["path"]).exists() or sha(base / r["path"]) != r["sha256"]]
        errors += [label + ": " + p for p in bad]
        print(f"{label}: {len(records)} checked; {len(bad)} changed")
    assert git("branch", "--show-current") == before["branch"] and git("rev-parse", "HEAD") == before["head"]
    assert not git("diff", "--cached", "--name-only"), "Index changed"
    assert not errors, errors


def retain_baseline():
    """Make the original indexed audit reviewable alongside the amended report."""
    manifest = LEDGER / "baseline-manifest.json"
    if manifest.exists():
        raise RuntimeError("Retained baseline already exists; refusing to overwrite")
    original_root = SNAPSHOT / "original"
    original_index = read(original_root / "docs/architecture/WindowsMigrationLedger/index.json")
    entries = []
    names = [a["path"] for a in original_index["artifacts"]] + ["docs/architecture/WindowsMigrationLedger/index.json"]
    for name in names:
        source = original_root / name
        target = LEDGER / "baseline" / name
        target.parent.mkdir(parents=True, exist_ok=True)
        assert not target.exists()
        shutil.copyfile(source, target)
        entries.append({"original_path": name, "retained_path": target.relative_to(ROOT).as_posix(), "sha256": sha(target)})
    manifest.write_text(json.dumps({"purpose": "Immutable pre-amendment audit, 51 indexed artifacts plus original index; not current acceptance evidence.",
                                    "entries": entries}, indent=2) + "\n", encoding="utf-8")
    print("Retained original indexed audit:", len(entries), "files")


def check_tracking():
    index = read(LEDGER / "index.json")
    expected = {a["path"] for a in index["artifacts"]} | {"docs/architecture/WindowsMigrationLedger/index.json"}
    proposed = read(LEDGER / "proposed-tracked-files.json")["paths"]
    assert len(proposed) == len(set(proposed)), "Duplicate proposed path"
    assert set(proposed) == expected, {"missing": sorted(expected-set(proposed)), "extra": sorted(set(proposed)-expected)}
    for artifact in index["artifacts"]:
        path = ROOT / artifact["path"]
        assert path.is_file() and sha(path) == artifact["sha256"] and path.stat().st_size == artifact["bytes"], "Index artifact drift: " + artifact["path"]
    retained = read(LEDGER / "baseline-manifest.json")["entries"]
    assert len(retained) == 52
    assert all(sha(ROOT / a["retained_path"]) == a["sha256"] for a in retained), "Retained original audit changed"
    # NUL-delimited binary input avoids Windows text-mode CRLF translation. Git
    # otherwise treats a literal CR as part of each supplied filename.
    process = subprocess.run(["git", "-c", "safe.directory=" + ROOT.as_posix(), "check-ignore", "-z", "--stdin"],
                             input=("\0".join(proposed)+"\0").encode("utf-8"), cwd=ROOT, capture_output=True)
    assert process.returncode in (0, 1), process.stderr
    ignored = {p.decode("utf-8") for p in process.stdout.split(b"\0") if p}
    should_ignore = {p for p in proposed if p.startswith("docs/architecture/WindowsMigrationLedger/")}
    assert ignored == should_ignore, {"ignore_policy_mismatch": True, "unexpected_ignored":sorted(ignored-should_ignore), "unexpected_visible":sorted(should_ignore-ignored)}
    assert not git("diff", "--cached", "--name-only"), "Unexpected staging"
    print(f"PASS: {len(index['artifacts'])} index artifacts + index.json = {len(proposed)} proposed paths; {len(ignored)} individually force-included ledger files; {len(proposed)-len(ignored)} normally staged visible audit files. Nothing staged.")
    print("PASS: all 52 retained original audit files match their recorded hashes.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(__doc__)
    parser.add_argument("mode", choices=("capture", "check", "retain-baseline", "check-tracking"))
    args = parser.parse_args()
    {"capture":capture,"check":check,"retain-baseline":retain_baseline,"check-tracking":check_tracking}[args.mode]()
