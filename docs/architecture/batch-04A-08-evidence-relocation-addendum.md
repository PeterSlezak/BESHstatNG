# Official 04A.08 owner acceptance and sanitized evidence publication addendum

**OWNER-ACCEPTED SOFTWARE SCOPE; SANITIZED PUBLICATION MATERIAL PREPARED;
GIT PUBLICATION STILL REQUIRES OWNER APPROVAL.** This records owner instructions,
not an agent signature. The original
[owner-review dossier](batch-04A-08-owner-review.md) and original local snapshots
remain unchanged. This note supersedes their pending software-acceptance status
without changing any scientific or native qualification.

> Official Batch **04A.08 is owner-accepted for its software scope**: portable regression-effect authoring and typed model-specification policy in `BESHStat.Excel.Common`, with `RegressionEffectsController` and typed removal integration in the Windows host. The owner accepts the **five managed failures and 49 Python nonpasses as documented nonpasses**. Core coefficient-label and categorical-reference normalization issues are unresolved; numerical references are unpromoted candidates. **All native obligations remain OPEN**, with zero control releases and zero obligation discharges. **P = NO; N1 = NO GO; N2 = NO; Gate C2 = UNMET.**

The owner first authorized copy-and-repoint publication preparation, then
explicitly requested general placeholders in `evidence/04A08/validation-v2.json`,
`relocation-manifest-v1.json`, `README.md` and this acceptance addendum, with
old-location files retained for testing and development. That subsequent
instruction authorizes the public sanitation recorded here. It does not
change the owner-pinned originals or authorize Git staging/publication.

## Repository and evidence identities

Branch remains `codex/04A-08`, HEAD/local main
`b48877406c3c185a992d5dd5c42f951611a52569`; the index remains empty.
No new branch, fetch, pull, reset, stash, clean, commit, merge or push occurred.
The already accepted product implementation is unchanged by this operation.

The seven public files remain at [evidence/04A08](../../evidence/04A08/README.md).
The README records their current byte counts/hashes. Six files remain exact
copies of the corresponding originals under `docs/architecture/04A08/`.
The public validation file is explicitly a sanitized derivative:

* Original SHA-256:
  `e8da3029d7f8493c770e651378c94d6b9b079e8cb4d4f6f3853ef3b8ffe6d1c2`.
* Sanitized publication SHA-256:
  `11f185fecde5a870f55f67fb6d99944f86212cd9480547bfb4ed92ee1e3c081d`.
* Original inventory SHA-256 remains
  `899e53098a05bf95e1418e4270201b9b5fec781d6a9c0ec4de6368a6da043ef2`.

The [mapping manifest](../../evidence/04A08/relocation-manifest-v1.json) now uses
schema `04A08-evidence-publication-v2`, separating source/publication hashes and
marking validation as `sanitized-derived`. It does not claim all seven public
files are byte-identical. The original copied inventory retains historical
paths/pins without being rewritten. The previous raw relocation manifest is
archived only under the ignored original evidence directory, with its identity
recorded in the public mapping. Its raw identifying values are not republished.

## Sanitation and development authority

General placeholders replace local executable, repository/profile and other
absolute machine paths. Public command examples use `python`. The mapping
contains aggregate finding counts only: no raw account names, machine paths or
per-finding JSON locations. The replacements affect documentation strings;
test names, outcomes, counts, source hashes, restrictions and scientific
qualifications are preserved. Screening covers the requested documents and all
public JSON files. This is a limited automated check, not a confidentiality
assurance or a claim that a secret was discovered.

**Use `docs/architecture/04A08/` originals for testing and development.**
The capture-contract reader now selects the original local crosswalk without a
publication fallback. The publication helper provides a distinct original-pin
`read_development` reader and a public-integrity reader. Clone-shaped tests can
inspect the sanitized public package without ignored originals, but statistical
or audit reproduction still requires the original local environment and files.
Capture emitters and historical summary writers retain their local output paths;
no protected capture was regenerated. No TestResults tree was copied or written.

The three excluded old JSON files (`baseline.json`, `validation.json`,
`publication-inventory.json`) and the newly archived raw relocation manifest
remain local-only. Original snapshots, accepted dossier, historic source pins
and all product/numerical sources remain untouched. No `.gitignore` edit or
force-add was used. All publication destinations remain trackable, and all
work remains unstaged/uncommitted pending separate publication approval.

## Focused checks and retained restrictions

Focused relocation/publication tests cover original source pins, six exact
copies plus one deterministic derivative, public hashes, placeholders/privacy,
clone-shaped access, absence of fallback for development, corruption/duplicate
rejection, ignore rules, exclusions, retained nonpass identities/counts and OPEN
obligations. The existing eight authoring source/wiring tests and both read-only
mapping/contract checks are also run. Full Debug/Release/Node suites are not
rerun for a documentation/evidence sanitation.

Reproduction commands use a locally selected Python interpreter:

```powershell
python -B -m unittest discover -s scripts/tests -p test_04a08_evidence_relocation.py -v
python -B -m unittest discover -s scripts/tests -p test_04a08_authoring_successor.py -v
python -B scripts/relocate-04a08-evidence.py check
python -B scripts/capture-04a08-contract.py check
git diff --check
git diff --cached --exit-code
git status --short --untracked-files=all
```

The accepted **five managed failures and 49 Python nonpasses remain nonpasses**;
no original test result or scientific reference was repaired/rebaselined here.
All native obligations remain **OPEN**, with **zero controls released and zero
obligations discharged**. **P NO; N1 NO GO; N2 NO; Gate C2 UNMET.** No native
Excel/COM/UIA execution, numerical promotion, publication or 04A.09 occurred.
Future manual native smoke remains NOT RUN and needs its own authority.
