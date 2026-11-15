from common44 import *
lock();before=runner().__class__.build.__globals__['source_pins']()
results=runner().serial('a44-final-serial-v1')
save(PRIVATE/'serial-checks-raw-v1.json',results)
assert before==runner().__class__.build.__globals__['source_pins']()
save('matrix/serial-checks-v1.json',dict(exits=[r['exit_code'] for r in results],meaning=['strict audit: nonzero remains nonpass','profile: nonzero remains nonpass','MkDocs strict: actual exit'],raw=pin(PRIVATE/'serial-checks-raw-v1.json'),source_unchanged=True,Node='INHERITED_916_PASS;NOT_RERUN_NO_AFFECTED_JS_OR_CONTRACT_DEPENDENCY',direct_verify_script='NOT_RUN: fixed TestResults/Codex/Core.trx and Common.trx plus Legacy.trx can overwrite protected historical evidence. Existing unchanged isolated runner executes full Debug/complete compilation/affected Release and preserves every original identity;no assertion/filter weakening.',full_Release='NOT_RUN',native_Excel_COM_UIA_R_WASM_manual='NOT_RUN',gates=GATES))
print('Strict/profile/MkDocs actual exits',[r['exit_code'] for r in results],flush=True)
