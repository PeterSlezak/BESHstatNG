#!/usr/bin/env python3
"""Batch 00.01 source-to-manifest verification (stdlib only).

Run from any working directory:
    python tools/verify_stage304_inventory.py

The inventories intentionally freeze the Stage 3.04 public declarations. When
later batches deliberately change Ribbon or ExcelFunction exposure, review and
version the manifests instead of silently updating this baseline in place.
"""
from __future__ import annotations

import collections
import csv
import pathlib
import re
import sys
import xml.etree.ElementTree as ET

PROJECT = pathlib.Path(__file__).resolve().parents[1]
MANIFESTS = PROJECT / 'docs' / 'architecture' / 'manifests'
UDF_REGEX = re.compile(
    r'<\s*ExcelFunction\s*\((.*?)\)\s*>\s*'
    r'(?:\s*<[^>]+>\s*)*\s*'
    r'(?:(?:Public|Friend)\s+)(?:Shared\s+)?Function\s+(\w+)',
    re.IGNORECASE | re.DOTALL,
)
ATTR_NAME = re.compile(r'\bName\s*:=\s*"([^"]+)"', re.IGNORECASE)
ATTR_CATEGORY = re.compile(r'\bCategory\s*:=\s*"([^"]+)"', re.IGNORECASE)


def csv_rows(filename: str) -> list[dict[str, str]]:
    with (MANIFESTS / filename).open(newline='', encoding='utf-8-sig') as file:
        return list(csv.DictReader(file))


def check(label: str, condition: bool, details: str = '') -> None:
    if not condition:
        raise AssertionError(f'{label}: {details}')


def ribbon_source() -> list[dict[str, str]]:
    xml = ET.parse(PROJECT / 'BESHstatNG/Resources/RibbonXML.xml').getroot()
    return [dict(MenuLabel=element.attrib.get('label', ''),
                 Id=element.attrib.get('id', ''),
                 OnAction=element.attrib.get('onAction', ''))
            for element in xml.iter() if element.tag.split('}')[-1] == 'button']


def udfs_source() -> list[dict[str, str]]:
    declarations = []
    for path in sorted((PROJECT / 'BESHstatNG/src/ExcelUDFs').rglob('*.vb')):
        source = path.read_text(encoding='utf-8-sig')
        matches = list(UDF_REGEX.finditer(source))
        total_annotations = len(re.findall(r'<\s*ExcelFunction\s*\(', source, re.IGNORECASE))
        check(f'ExcelFunction parsing {path.name}', len(matches) == total_annotations,
              f'parsed {len(matches)} out of {total_annotations} annotations')
        for match in matches:
            name_match = ATTR_NAME.search(match.group(1))
            category_match = ATTR_CATEGORY.search(match.group(1))
            check('UDF Name attribute', name_match is not None, str(path))
            check('UDF Category attribute', category_match is not None, str(path))
            declarations.append(dict(File=path.relative_to(PROJECT).as_posix(),
                                     Method=match.group(2),
                                     ExcelFunctionName=name_match.group(1),
                                     Category=category_match.group(1)))
    return declarations


def keyed(rows: list[dict[str, str]], fields: tuple[str, ...]) -> collections.Counter:
    return collections.Counter(tuple(row[field] for field in fields) for row in rows)


def compare_manifest(label: str, actual: list[dict[str, str]], recorded: list[dict[str, str]],
                     keys: tuple[str, ...]) -> None:
    observed = keyed(actual, keys)
    frozen = keyed(recorded, keys)
    missing = sorted((observed - frozen).elements())
    obsolete = sorted((frozen - observed).elements())
    check(label, not missing and not obsolete,
          f'missing in manifest: {missing[:8]}; not found in source: {obsolete[:8]}')


def verify() -> None:
    ribbon = ribbon_source()
    ribbon_manifest = csv_rows('ribbon-inventory-stage304.csv')
    check('Ribbon declaration count', len(ribbon) == 84, f'{len(ribbon)} != 84')
    check('Ribbon ID uniqueness', len({row['Id'] for row in ribbon}) == 84)
    compare_manifest('Ribbon inventory', ribbon, ribbon_manifest, ('MenuLabel', 'Id', 'OnAction'))
    counts = collections.Counter(row['OnAction'] for row in ribbon)
    repeats = {name: count for name, count in counts.items() if count > 1}
    expected_repeats = {'OnbtmKMPressed': 2, 'OnbtmCAPressed': 2, 'OnbtmCoxPressed': 2}
    check('Intentional repeated callbacks', repeats == expected_repeats, str(repeats))
    check('Distinct Ribbon callbacks', len(counts) == 81, str(len(counts)))
    addin = (PROJECT / 'BESHstatNG/BESHStatAddIn.vb').read_text(encoding='utf-8-sig')
    missing_actions = [action for action in counts
                       if not re.search(r'\bSub\s+' + re.escape(action) + r'\s*\(',
                                        addin, re.IGNORECASE)]
    check('Ribbon callback implementation', not missing_actions, str(missing_actions))

    map_rows = csv_rows('ribbon-workflow-map-stage304.csv')
    check('Ribbon workflow map count', len(map_rows) == 84)
    compare_manifest('Ribbon workflow mapping', ribbon,
                     [dict(Id=row['RibbonId'], MenuLabel=row['MenuLabel'], OnAction=row['OnAction'])
                      for row in map_rows], ('Id', 'MenuLabel', 'OnAction'))
    approved = {'Onbtm1WayANOVAPressed': 'anova.oneway',
                'OnbtmKruskalWallisPressed': 'nonparametric.kruskal-wallis'}
    for row in map_rows:
        expected = approved.get(row['OnAction'], '')
        check('Proposed analysis IDs', row['ProposedAnalysisId'] == expected,
              row['RibbonId'])
        check('Shared callback workflow key', row['WorkflowKey'] == row['OnAction'], row['RibbonId'])

    ud = udfs_source()
    ud_manifest = csv_rows('udf-inventory-stage304.csv')
    check('UDF declaration count', len(ud) == 268, f'{len(ud)} != 268')
    check('UDF file count', len({row['File'] for row in ud}) == 23)
    check('UDF name uniqueness', len({row['ExcelFunctionName'] for row in ud}) == 268)
    check('UDF method uniqueness', len({row['Method'] for row in ud}) == 268)
    compare_manifest('UDF inventory', ud, ud_manifest,
                     ('File', 'Method', 'ExcelFunctionName', 'Category'))
    check('UDF classifications supplied', all(row['MigrationDifficulty'] and row['Notes']
                                               for row in ud_manifest))

    print('PASS Stage 3.04 inventory/source comparison')
    print('  Ribbon: 84 declarations; 84 unique IDs; 81 callbacks; 3 intentional callback repeats')
    print('  Public Excel-DNA functions: 268 names; 268 methods; 23 source files')
    print('  Common analysis IDs proposed for next batch: anova.oneway, '
          'nonparametric.kruskal-wallis')
    print('  No other Ribbon feature or UDF is marked Office-verified by Batch 00.01.')


if __name__ == '__main__':
    try:
        verify()
    except (AssertionError, OSError, ET.ParseError, KeyError, ValueError) as error:
        print(f'FAIL Stage 3.04 inventory: {error}', file=sys.stderr)
        sys.exit(1)
