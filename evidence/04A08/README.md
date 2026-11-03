# 04A.08 accepted software evidence ? sanitized publication material

The owner accepted official 04A.08's software scope on **2026-11-03** and
subsequently requested general placeholders for machine-identifying details in
these publication files. The
[acceptance addendum](../../docs/architecture/batch-04A-08-evidence-relocation-addendum.md)
records that distinction. Git publication still requires separate owner approval.

**Testing and development use the unchanged originals in
`docs/architecture/04A08/`.** Publication renditions are documentation, not
executable validation inputs. The original accepted dossier and historical
TestResults remain unchanged and local reproduction still needs those artifacts.

Six JSON files remain byte-identical to their source snapshots.
`validation-v2.json` is now an explicitly labelled **sanitized derivative**;
it must not be described as SHA-identical to its source.

| Publication file | Bytes | Publication SHA-256 | Disposition |
|---|---:|---|---|
| [authoring-fixtures.json](authoring-fixtures.json) | 42490 | `36ec16f860627749dc9d1d3ed57485471d52fa8abb2bac0b8de80345349016e4` | Exact copy |
| [formula-compatibility.json](formula-compatibility.json) | 3199 | `e2df4a1395f37d490805ccc71408d64c7d0c35bb20e4ef157b7bbe4e2409ea59` | Exact copy |
| [ownership-matrix.json](ownership-matrix.json) | 26964 | `8e1b610db3928caf27a7d0c6230a202b484f9511208fbb2b307810e2e26ba2c7` | Exact copy |
| [removal-characterization.json](removal-characterization.json) | 7411 | `f2852d17dbe035813b27b4e42e0693b49ee9ca96cadf3b047219d210702bedd1` | Exact copy |
| [restriction-crosswalk.json](restriction-crosswalk.json) | 1118998 | `1d97e62efc5d172706814011cecb2db6fd2c2682c11b0ee6bebf4a6f67cb15c9` | Exact copy |
| [validation-v2.json](validation-v2.json) | 148276 | `11f185fecde5a870f55f67fb6d99944f86212cd9480547bfb4ed92ee1e3c081d` | Sanitized derivative |
| [publication-inventory-v2.json](publication-inventory-v2.json) | 34150 | `899e53098a05bf95e1418e4270201b9b5fec781d6a9c0ec4de6368a6da043ef2` | Exact copy |

The original validation SHA-256 remains
`e8da3029d7f8493c770e651378c94d6b9b079e8cb4d4f6f3853ef3b8ffe6d1c2`.
The [mapping manifest](relocation-manifest-v1.json) separates each `source_sha256`
from its publication `sha256` and records byte counts and derivation. Its filename
is retained, while its schema is now `04A08-evidence-publication-v2`. No recursive
manifest self-hash is embedded. The previous unsanitized relocation manifest is
retained only at the ignored local location named in the mapping.

## Placeholders and preservation

Machine-specific executable paths use `<PYTHON_EXE>`, `<MSBUILD_EXE>` or
`<VSTEST_EXE>`. Repository roots use `<REPOSITORY_ROOT>`, profile roots use
`<USER_PROFILE>`, and other absolute local paths use `<LOCAL_PATH>`.
Relative test/evidence paths and test identities, outcomes, counts, source pins,
scientific qualifications and native statuses are retained.

The public mapping contains category counts, not raw machine paths, account
names or per-finding JSON locations. Public command examples use `python`;
select the appropriate local interpreter in the development environment.
Automated checks screen all seven JSON files, this README, the mapping and the
addendum for remaining paths/accounts and configured SID/network/token patterns.
This limited screening is not a general confidentiality assurance. No claim of
GitHub publication or whole-project clone self-containment is made.

Original `baseline.json`, `validation.json`, `publication-inventory.json`, raw
relocation manifest, historical TestResults, owner workbooks and local receipts
remain excluded. Historical paths/hashes inside the unchanged copied inventory
are evidence identities, not instructions to publish the referenced local files.

## Development versus publication checks

`capture-04a08-contract.py check` reads the original local crosswalk only. It does
not fall back to a publication rendition. Capture and historical summary writers
retain their original destinations and are not rerun to overwrite old captures.
`read_development` similarly checks the seven original owner-pinned local files.

`relocate-04a08-evidence.py check` verifies public hashes and deterministic
sanitization against originals when available locally. Clone-shaped checks can
read and validate the public renditions without original files, but cannot use
those renditions as substitutes for testing/development inputs.

```powershell
python -B scripts/relocate-04a08-evidence.py check
python -B -m unittest discover -s scripts/tests -p test_04a08_evidence_relocation.py -v
python -B scripts/capture-04a08-contract.py check
```

## Retained acceptance qualifications

The five managed failures and 49 Python nonpasses remain documented nonpasses.
Core coefficient-label and categorical-reference normalization issues remain
unresolved. Numerical references remain unpromoted candidates.

**All native obligations OPEN; zero releases/discharges; P NO; N1 NO GO;
N2 NO; Gate C2 UNMET.** No product/statistical implementation, Excel/COM/UIA
execution, numerical promotion, Git staging/commit/merge/push or 04A.09 is
part of this administrative sanitation.
