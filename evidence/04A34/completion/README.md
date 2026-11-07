# 04A.34 continuation evidence

The current owner dossier and root `validation.json`, `family-coverage.json`,
member maps, contracts, preservation and proposed publication manifest describe
the completed software scope. The `checkpoint/` directory preserves partial
metadata, its original manifest and NOT_READY decisions. It is historical.
The frozen `operation-map-entry.csv` is an audit lead, not the final disposition.

The root unsuffixed managed identities, nonpasses, Source/Node and command
receipts preserve the partial run. Their `-final` counterparts describe final
executions. No old tests, expected hashes or outcomes were rewritten.

Independent readers:

```text
<PYTHON> -B evidence/04A34/verify_manifest.py
<PYTHON> -B -m unittest discover -s evidence/04A34 -p test_audit.py -v
```

Current receipt readers/producers are `reconcile.py`, `renderer_guard.py`,
`finalize_sources.py`, `summarize_final.py` and `final_packet.py` in this folder.
`run.py` uses the accepted isolated runner. Final managed builds/tests/focused
runs use label `review3`; Node and the full Python matrix use `completed`, with
the final Python scripts rerun under `review` after privacy correction.
Existing run folders are immutable: any reproduction needs a fresh label and
explicit reader path adaptation, never deletion or overwrite of old receipts.

The capture readers consume the `review3` Debug/Release builds. Original and
supplemental pre-edit CSVs are immutable source-slice authorities. Identical
post payloads remain ignored; `capture-comparison.json` records their hashes,
sizes and exact comparisons without publishing duplicate numerical payloads.
Accepted Core factories supply typed inputs and source identity, not a new
independent scientific oracle. All software compatibility is NOT_GOLDEN.

`geometry.py`, `preparation.py`, `extras.py`, `series.py`, `policies.py`,
`spc_basis.py` and original root integration/freeze scripts document the staged
extraction history. Do not rerun their freeze/apply actions on the completed
tree or treat them as current write authority. Their frozen source specifications
and compiled fixtures remain the independent compatibility side.

No native Excel/R/browser/manual approval, Git publication, packaging or future
batch is performed by this packet. The manifest is a local proposed inventory.
