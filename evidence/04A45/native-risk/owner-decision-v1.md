# 04A.45 separate Windows/COM decision packet, version 1

NOT_APPROVED. LIVE_NATIVE: NOT_RUN. ACTUAL_HWND_PID_XLL_ATTRIBUTION: NOT_VERIFIED.
This packet cannot authorize execution. No native adapter or enumerator is implemented.
The test harness has no live mode and always returns MayRunNative=False, including for
internally consistent simulated evidence or a locally edited approval claim.

The owner must name the exact machine, interactive desktop/session, Excel version and
32/64-bit architecture, process creation/reuse policy, single test workbook and isolation
copy, reviewed XLL build and managed assemblies, original controls, actions, observation
start/end, expiry, independent approver and emergency-stop operator in an external dated
decision. These targets are PENDING_OWNER_SELECTION, not fabricated from this sandbox.
This version is a concrete bounded proposal for that selection, not a request for a
general COM waiver. Refusal, uncertainty or missing fields leaves every gate closed.

Proposed first observation scope, each action requiring explicit approval:

1. Owner opens an isolated nonsensitive disposable workbook copy and selects exactly
   one Excel process/session. Record executable identity, PID, process start time,
   architecture, session/desktop, top-level HWND and its thread/process ownership.
   Ambiguous instances stop the campaign; a PID alone is insufficient.
2. Qualify the exact owner-built XLL and packed dependencies against the reviewed commit,
   source tree, configuration, architecture, file SHA-256s, build time/product version and
   managed module MVID. `build-attribution-v1.json` will identify compile-only managed
   artifacts. No XLL was packaged by this batch; its exact hash is NOT_AVAILABLE until a
   separately approved build/target is supplied. A similarly named file is not proof.
3. Independently corroborate the loaded XLL absolute file identity and physical hash
   after load, managed module MVID/SHA, version and callback registration in that same
   process. Packed assemblies may lack a physical location; memory module metadata alone
   cannot prove the original packed file. If an approved inspector cannot establish this
   chain, require independent load instrumentation/build attestation or stop NOT_VERIFIED.
   Registry entries, folder contents and host-supplied path claims are insufficient.
4. Only after the external decision and identities are authenticated: observe the
   explicitly listed Ribbon actions, image callback, approved form open/cancel/close,
   nested RefEdit selection/focus/escape, two named option sets per approved family and
   one controlled validation error. Workbook switch/recalc and selected log/cancel
   observations require their own explicitly listed actions. No synchronous numerical
   fit is claimed interruptible by fake cancellation tests.
5. Bind each capture to the original obligation/control IDs in
   `scope/original-obligations-v1.json`, callback/event, request/side effect, same PID,
   process start, HWND, session, workbook identity and loaded module set. Any restart,
   workbook switch, identity/hash change or timestamp expiry invalidates prior binding.
   N1 prerequisites are exact attribution and independent COM risk approval; N2 additionally
   needs the approved executed original-control captures and their review. Neither is met.

Proposed exclusions: updater/network activity, real settings persistence, browser/Help
launch, add-in installation/uninstallation, registry writes, permanent workbook writes,
arbitrary macros, sensitive inputs, chart export files and unattended automation. Even
loading the current add-in can schedule its existing updater in AutoOpen: the owner must
resolve that conflict explicitly before any live launch. No silent suppression/change of
AutoOpen or updater behavior is authorized here. A macro-queue callback is simulated
offline only; real queueing is an Office side effect requiring exact approval.

Risk appraisal: modal forms/RefEdit and DoEvents can cause reentrancy, frozen UI, changed
focus or duplicate commands; COM callbacks/RCWs can outlive workbook context; Excel-DNA
registration can fail; shared caches/mutex/add-in state may collide across workbooks or
instances; calculations, metadata, output/overwrite and chart operations can mutate the
workbook. Existing run leases do not establish workbook cache isolation. No production
state/cache/event-hook correction is part of this packet.

Recovery proposal: owner present throughout a short explicitly bounded window; back up
the test workbook and record baseline add-in/calculation state; use only the disposable
copy; stop on unexpected dialogs, writes, updater activity, hangs, identity drift or privacy
findings. The owner retains a manual stop/kill path for the selected process, understands
unsaved data loss and confirms no unrelated workbook is in it. Do not kill/enumerate or
attach to any process from this harness. Preserve adverse evidence; closing/restarting
requires fresh provenance and a new approval if outside the original window.

Independent authorization must be captured outside this harness with precise decision
text, approver identity, date, target/build/version, expiry, actions, original IDs and risk
acceptance. A JSON boolean, this document, 04A.44 acceptance and the earlier case-specific
04A.05/27 timing exceptions cannot establish that authority.

Blocking controls: zero/mismatched/recycled PID; stale HWND or wrong thread PID; unknown
bitness; multiple Excel instances/loaded versions; stale start/observation time; mismatched
Debug/Release/XLL/DLL/MVID/version; missing module/registration proof; forged workbook/path;
wrong session/desktop; missing external approval; privacy finding; absent manual emergency
stop; unresolved AutoOpen updater conflict. All must stop before native activity.

P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A46_NOT_AUTHORIZED
