#!/usr/bin/env python3
"""Batch 00.05: verify retired Windows Office.js placeholders stay removed.

This is a static source guard, not a proof about external consumers or a live Excel
host. It does not modify the solution and uses only the Python standard library.

Run from the solution root:
    python BESHstatNG/tools/audit_transitional_officejs.py
"""
from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
RETIRED_PATHS = (
    Path('BESHstatNG/src/DM/OfficeJsDataImporter.vb'),
    Path('BESHstatNG/src/DM/ResultWriter/OfficeJsResultWriter.vb'),
)
RETIRED_SYMBOLS = (
    'OfficeJsDataImporter',
    'OfficeJsDataImportOptions',
    'OfficeJsResultWriter',
    'GoogleSheetsResultWriter',
)
# Active language/project source, deliberately excluding historical Markdown docs,
# inventory snapshots, Python audit code, build intermediates and binary assets.
SOURCE_SUFFIXES = {'.vb', '.vbproj', '.cs', '.csproj', '.razor', '.js', '.ts',
                   '.fs', '.fsproj', '.props', '.targets', '.xml'}
IGNORED_COMPONENTS = {'bin', 'obj', '.git', 'node_modules', 'TestResults', '.vs'}
# Ensure the supported host paths remain present. Their behavioral regressions are
# verified by the existing Stage304ContractSnapshotTests and SharedExcelLayerTests.
REQUIRED_PATHS = {
    Path('BESHstatNG/BESHStatNG.vbproj'): ('ExcelDnaDataImporter.vb', 'ExcelDnaResultWriter.vb'),
    Path('BESHstatNG/src/DM/ExcelDnaSharedApplicationAdapter.vb'): ('ToGroupedNumericData',),
    Path('BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb'): ('WriteSharedRequest',),
    Path('BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb'): ('SpreadsheetRangeData',),
    Path('BESHStat.Excel.Common/src/Input/GroupedNumericRangeParser.vb'): ('GroupedNumericRangeParser',),
    Path('BESHStat.Excel.Common/src/Output/ResultTableWriteRequest.vb'): ('ResultTableWriteRequestFactory',),
    Path('BESHStat.WasmSmoke/Pages/Home.razor'): (
        'SharedAnalysisApplicationService', 'OutputDocument.FromResult',
        'OutputSemanticPlanner.Create', 'OutputLayoutPlanner.Create',
        'OfficeOutputPlanAdapter.Prepare', 'beshOffice.writeOutputPlan'),
    Path('BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb'): ('OfficeOutputPlanAdapter',),
    Path('BESHStat.WasmSmoke/wwwroot/js/officeBridge.js'): ('getSelectedRangeData', 'Excel.run', 'writeOutputPlan'),
    Path('BESHStat.Excel.Common.Tests/src/Stage304ContractSnapshotTests.vb'): ('Stage304GoldenCases',),
}
TOKEN_PATTERN = re.compile(r'\b(?:' + '|'.join(map(re.escape, RETIRED_SYMBOLS)) + r')\b', re.IGNORECASE)


def audit_solution(root: Path, *, require_host_paths: bool = True) -> list[str]:
    """Return human-readable violations; no writes or assumptions about OS APIs."""
    root = Path(root).resolve()
    errors: list[str] = []
    if not root.is_dir():
        return [f'Solution root is not a directory: {root}']

    for relative in RETIRED_PATHS:
        if (root / relative).exists():
            errors.append(f'Retired file still exists: {relative.as_posix()}')

    if require_host_paths:
        for relative, required_tokens in REQUIRED_PATHS.items():
            target = root / relative
            if not target.is_file():
                errors.append(f'Current host contract file missing: {relative.as_posix()}')
                continue
            contents = target.read_text(encoding='utf-8-sig', errors='replace')
            for token in required_tokens:
                if token not in contents:
                    errors.append(f'Expected current host contract {token!r} absent from {relative.as_posix()}')

    for path in sorted(root.rglob('*')):
        if not path.is_file() or path.suffix.lower() not in SOURCE_SUFFIXES:
            continue
        if any(part in IGNORED_COMPONENTS for part in path.relative_to(root).parts):
            continue
        source = path.read_text(encoding='utf-8-sig', errors='replace')
        for line_number, line in enumerate(source.splitlines(), start=1):
            match = TOKEN_PATTERN.search(line)
            if match:
                errors.append(f'Retired type {match.group(0)} referenced in '
                              f'{path.relative_to(root).as_posix()}:{line_number}')
        # Old project Compile paths are covered by the type-token search.
    return errors


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=ROOT, help='Solution root to inspect')
    args = parser.parse_args()
    errors = audit_solution(args.root)
    if errors:
        print(f'FAIL: {len(errors)} transitional Office.js helper violation(s):')
        for issue in errors:
            print(' - ' + issue)
        return 1
    print('PASS: Windows placeholder import/writer classes are absent, and current')
    print('      Excel-DNA, Common and WasmSmoke host boundaries remain present.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
