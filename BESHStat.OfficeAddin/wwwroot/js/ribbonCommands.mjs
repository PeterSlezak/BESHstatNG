// One shared runtime, no workbook calls, no storage queue, no untrusted URLs.
const actions = Object.freeze({ BeshAnova:'method.OneWayANOVA', BeshKruskal:'method.KruskalWallisTest',
    BeshAbout:'about', BeshHelp:'help', BeshSettings:'options', BeshLog:'diagnostics', BeshUnavailable:'export' });
const allowed = new Set(['analysis', ...Object.values(actions)]);
export function createCommandBridge(scope = globalThis, now = () => Date.now()) {
    let binding, pending, serial = 0, installed = false;
    const runtime = () => {
        const supported = (set,version) => { try { return scope.Office?.context?.requirements?.isSetSupported(set,version) === true; } catch { return false; } };
        return { excel: !!scope.Office?.context && supported('ExcelApi','1.1'), excel117:supported('ExcelApi','1.17'),
            addinCommands:supported('AddinCommands','1.1'), sharedRuntime:supported('SharedRuntime','1.1') };
    };
    const notify = code => { try { const e=scope.document?.getElementById('command-bootstrap-status'); if(e)e.textContent=code==='command.open-failed'?'Ribbon could not open the pane. Use Open BESHStatNG.':''; } catch { } };
    async function deliver(intent) {
        pending = undefined;
        if (!binding?.ready || intent.expires < now() || intent.serial !== serial) return 'command.expired';
        try { return await binding.reference.invokeMethodAsync('Dispatch',intent.id); } catch { return 'command.rejected'; }
    }
    async function invoke(id, event, show = true) {
        try {
            if (!allowed.has(id)) return 'command.invalid';
            if (id === 'export') {
                if(binding?.ready) try { return await binding.reference.invokeMethodAsync('Dispatch',id); } catch { }
                return 'command.unavailable';
            }
            const caps = runtime();
            if (show && (!caps.excel || !caps.sharedRuntime || !caps.addinCommands || typeof scope.Office?.addin?.showAsTaskpane !== 'function')) return 'command.host-unsupported';
            const version = ++serial;
            const intent = {id, serial:version, expires:now()+30000};
            // A command cannot navigate unless its own open request succeeded.
            if (show) {
                try { await scope.Office.addin.showAsTaskpane(); } catch { notify('command.open-failed'); return 'command.open-failed'; }
            }
            if (version !== serial) return 'command.superseded';
            pending = intent;
            return binding?.ready ? await deliver(intent) : 'command.pending';
        } finally { try { event?.completed(); } catch { } }
    }
    function install() {
        if(installed || typeof scope.Office?.actions?.associate !== 'function')return false;
        for(const [name,id] of Object.entries(actions)) scope.Office.actions.associate(name,event=>invoke(id,event));
        installed=true;return true;
    }
    return { invoke, install, runtime,
        bind(owner, reference) { binding={owner,reference,ready:false}; return runtime(); },
        async ready(owner) { if(binding?.owner!==owner)return;binding.ready=true;if(pending)await deliver(pending); },
        unbind(owner) { if(binding?.owner===owner){binding=undefined;pending=undefined;serial++;} }
    };
}
const bridge = createCommandBridge();
export const bind = (owner,reference) => bridge.bind(owner,reference);
export const ready = owner => bridge.ready(owner);
export const unbind = owner => bridge.unbind(owner);
export function install() { return bridge.install(); }
export function showAbout() { const dialog=globalThis.document?.getElementById('about-panel'); if(dialog && !dialog.open)dialog.showModal(); }
// Hosts without the commands API keep the accepted single readiness waiter.
try { if(typeof globalThis.Office?.actions?.associate === 'function' && globalThis.Office?.onReady)
    Promise.resolve(globalThis.Office.onReady(()=>bridge.install())).catch(()=>{}); } catch { }
