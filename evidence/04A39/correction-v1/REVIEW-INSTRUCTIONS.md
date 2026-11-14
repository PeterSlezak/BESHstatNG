# 04A.39 correction review

This is a derivative for independent review. The original NOT_READY dossier, raw diagnostics and historical source references remain preserved on the owner machine. Review renderings are individually identified and hashed; they must not be substituted into the live source checkout or used to execute historical operators.

Use the archive SHA-256 in the external delivery receipt, then run:

```text
python review_readback.py <archive> --sha256 <external-delivery-sha256>
```

The reader verifies every physical member, CRC, size, SHA, path safety, source/rendering crosswalk, excluded raw hashes, 956 parent identities, 54 event identities and the restricted gates. The owner-machine scanner receipt attests its specific environment; this readback does not independently recover protected raw diagnostics or reproduce native/scientific gates.

The 25 retained scanner matches are exact synthetic negative-test literals with occurrence-specific source/hash/offset dispositions. Real historical machine references in 31 source preimages have separate sanitized review renderings. This is an explicit transport decision, not a source fix or a privacy waiver. The two repository-candidate privacy parents remain FAIL; both historical W3 UnicodeDecodeErrors remain unwaived.

The replacement workflow is `publish-review-v7.ps1`: Stage creates only an isolated explicit payload; Publish scans that physical stage before creating a ZIP. Any unknown finding, unsafe path, raw protected hash, missing/extra file or changed byte aborts. Failed stages remain retained; there is no automatic retry or cleanup.

No Git commit, merge, source cleanup, installation, publication to an external service or next-phase work is performed. If an owner later transmits binary split volumes, concatenate numbered volumes in numerical order, then extract the outer archive to obtain this original ZIP; volumes are not independent source packages.
