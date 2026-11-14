# Publication closeout addendum — NOT_READY_04A38

The current partial packet uses publication-manifest-v4.json and verify_publication_v2.py. Pass the manifest path explicitly:

`<PYTHON> -B evidence/04A38/completion/w2/continuation-v1/verify_publication_v2.py evidence/04A38/completion/w2/continuation-v1/publication-manifest-v4.json`

The isolated-copy command is check_public_copy_v2.py with the same argument. The external export receipt contains observed archive bytes/SHA-256/entry count/CRC and final live Git checks; no ZIP identity is invented inside the ZIP that is hashed.

Manifest v2 was rejected before ZIP creation because the original privacy-adversary source contains a fictional EXAMPLE_PRIVATE home-path example. Its bytes and failed reader remain unchanged. New reader v2 permits only that exact reviewed source hash/fictional marker in that one file. Generated leaks in other payloads and any modification of the reviewed source still fail. No actual owner/home path is whitelisted and no statistical/source/trace bytes were redacted.

The first revised publication test used a reassigned module constant but left Python's already-bound default manifest argument at v1. Its mutation-context failures are preserved and are not accepted coverage. Corrected test_publication_v3.py passes the selected v3 manifest explicitly; its15 conditions include generated privacy leakage and mutation of the reviewed synthetic source. Later v4 adds these actual receipts without altering the v3 or older manifests.

All other ownership/implementation limitations in owner-review and NEXT_WAVE_HANDOVER remain:60 historical supported/644 OPEN,303 support declarations unresolved, W2 IN_PROGRESS and W3 NOT_STARTED. The latest P1 body adverse suite has15 actual conditions, P0 has9, and current publication controls have15. No new original reference, native/scientific run, Git publication, owner acceptance or04A.39 work occurred.
