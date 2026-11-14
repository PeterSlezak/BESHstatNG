# Review transport — continuation-v17

This is a Git-free review derivative. It is not the live checkout and cannot independently prove the owner's current Git/index state, private underlying files, native Windows execution, or fresh test execution. Those local observations have separately pinned command/metadata/preservation receipts. The terminal state remains NOT_READY_04A38.

Use the final external `continuation-v17-review-parts-index-v1.json` and all eight matching ZIP files. Each ZIP is independently valid: two ACTIVE and six REFERENCE complementary parts. They are **not** binary `.zip.001/.002` volumes; do not concatenate them. Each includes an identical `.review-manifest.json` metadata member. The logical payload file union has no duplicate paths; the repeated metadata member is deliberately outside that union.

Verify the actual ZIP bytes, every payload SHA-256/size/CRC and the identical manifest:

```text
python -I -B verify_c17_transport_v2.py <external-index.json> <directory-containing-the-eight-ZIPs>
```

An optional third positional argument names a fresh extraction directory. Extraction rejects absolute/parent paths, non-normalized names, duplicate/casefold collisions, encrypted/symlink members, unexpected members and modified bytes. It never overwrites an existing extraction root. The transport reader requires only the Python standard library and does not require Git, .NET, Excel or private originals.

The final owner review is `evidence/04A38/completion/w2/continuation-v17/final/owner-review.md`. The final source-proposal reader is `verify_c17_checkpoint_v2.py`, seal v2, ownership ledgers v4, and byte source freeze8. That live reader intentionally requires the owner's real checkout and local private dependencies; running it in a copied review tree is not a substitute for live authentication. Earlier failed/provisional versions remain historical evidence.

The final 29,424 comparisons executed at functional freeze7. Freeze8 removes exactly two accidental CR bytes; fresh Debug inspection proves every measured method/type/instruction/signature/exception and export equal, with rebuilt assembly hashes/MVIDs separately recorded. No fourth compatibility campaign was executed.

The 4,213,129,746-byte malformed v9 aggregation and private raw payloads are excluded. Their authoritative originals remain locally protected; no content was deleted or replaced. Separately named review derivatives may omit machine/synthetic adverse values; each records the exact raw file pin and never replaces raw authority. The inherited v16 eight physical publication ZIPs are NOT_UPLOADED in this derivative. New v17 physical ZIP hashes attest actual local bytes; independent external physical verification requires the actual uploaded files.

No upload, installation, production package, signing, deployment, Release validation, native/manual scientific validation, W3 or 04A.39 run is represented by this transport.

The first packing attempt rejected its one-ACTIVE partition before producing any ZIP or manifest because the ACTIVE compressed estimate exceeded the bound. The corrected transport uses two ACTIVE and six REFERENCE independent parts. The failed command and original README remain historical attempts.
