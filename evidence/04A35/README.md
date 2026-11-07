# 04A.35 review packet

The current owner dossier, validation, ownership map and source transitions are
the final software-scope authority. The prior 04A.34 packet is read-only history;
its genuine byte/source-pin failures are retained and qualified separately.

Independent readers (stdlib only for the manifest reader):

```text
<PYTHON> -B evidence/04A35/verify_packet.py
<PYTHON> -B -m unittest discover -s evidence/04A35 -p test_audit.py -v
```

`run.py` reuses the accepted offline isolated runner. `baseline` builds/tests are
the actual fresh merged-main baseline. `final` builds, Core/Common/Office full
lanes, focused runs, Node/Python/strict/profile/docs are final executions.
The full `serial-Debug-Legacy` and `serial-Release-Legacy` lanes are the canonical
final Framework runs. Earlier concurrent Framework deadline failures and exact
targeted reruns remain in `managed-boundary-timing.json`; no deadline was relaxed.

Run folders are immutable and ignored. Reproduction requires fresh labels and
explicit reader path adaptation, never deletion/overwrite of old receipts.
Original wrappers are NOT_RUN because they hardcode shared historical outputs
and Legacy packaging; the isolated equivalent test scopes executed.

The valid pre-edit numerical authority is `pre-edit-capture-v2.csv` (4480 rows).
The first capture is preserved as an invalid text-comparison fixture; V2 used
the real Windows OptionCompare Text and was checked against1120 actual original
title/validation adapter calls before production edits. Original source bodies,
inputs, compiler options and SHA references remain inspectable.

Final post captures are byte-identical to V2 in both configurations and remain
ignored to avoid duplicate frozen numerical payloads. `capture-post.ps1` and
`compatibility-comparison.json` reproduce/verify them. Accepted34 helper engines
are explicitly reused, not independent scientific oracles. All data is NOT_GOLDEN.

`freeze.py`, `implement.py` and `tests.py` document staged development, not current
write authority. Do not rerun their apply/freeze actions on the completed tree.
The current checked-out production/test sources are authoritative. In particular,
later focused mutation/compiled-consumer cases remain in current tests, not in
the initial test generator. No native Excel/R/browser execution or Git publication
is performed by this packet.
