// Worksheet metadata only. Cell payloads use the existing bounded/tiled reader.
const valid = (s,n) => typeof s === 'string' && s.length > 0 && s.length <= n && !/[\u0000-\u001f\u007f]/.test(s);
const fail = code => { throw Object.assign(new Error(), {safeCode:code}); };
const column = n => { let s=''; for(;n;n=Math.floor((n-1)/26))s=String.fromCharCode(65+(n-1)%26)+s;return s; };
const shape = r => ({row:r.rowIndex, col:r.columnIndex, rows:r.rowCount, cols:r.columnCount});
function validate(r) {
    if (![r.row,r.col,r.rows,r.cols].every(Number.isSafeInteger) || r.row < 0 || r.col < 0 || r.rows < 1 || r.cols < 1 || r.row+r.rows>1048576 || r.col+r.cols>16384) fail('host.invalid-response');
    return r;
}
export function createWorksheetDiscovery(scope) {
    let disposed=false, session=null, document=null, owner=null, callback=null, epoch=0, queue=Promise.resolve();
    let listeners=[], watchContext=null, watchVersion=0, focusListener=null, visibilityListener=null;
    const currentDocument=()=>scope.Office?.context?.document;
    const changed=(documentChanged=false)=> {
        epoch++; if(documentChanged)session=null;
        if(!disposed && owner && callback) try { void callback.invokeMethodAsync('WorksheetCatalogInvalidated',owner,documentChanged?'user.document-changed':'spec.source-stale').catch(()=>{}); } catch {}
    };
    async function native(action) {
        if(disposed) return {code:'user.cancelled'};
        let release; const previous=queue; queue=new Promise(r=>{release=r;});
        let timer, started=false, expired=false;
        try {
            return await Promise.race([(async()=>{
                await previous;
                if(disposed || expired)fail('user.cancelled');
                if(!currentDocument() || typeof scope.Excel?.run!=='function' || !scope.crypto?.randomUUID)fail('office.unavailable');
                if(scope.Office?.context?.requirements?.isSetSupported('ExcelApi','1.17')!==true)fail('office.unsupported-api');
                const doc=currentDocument(), version=epoch;
                const check=()=>{if(disposed || expired || currentDocument()!==doc || epoch!==version)fail('user.document-changed');};
                started=true;
                try { return await scope.Excel.run(async context=>{check();const result=await action(context,check);check();return result;}); }
                finally {release();}
            })(),new Promise((_,reject)=>{timer=setTimeout(()=>{expired=true;epoch++;reject(Object.assign(new Error(),{safeCode:'input.time-budget'}));},5000);})]);
        } catch(e) {return {code:e?.safeCode || (['ItemNotFound','InvalidObjectPath'].includes(e?.code)?'user.document-changed':'host.operation-failed')};}
        finally {clearTimeout(timer);if(!started)previous.finally(release);}
    }
    async function removeListeners() {
        watchVersion++;
        const old=listeners;listeners=[];
        if(old.length)try{await scope.Excel.run(watchContext,async context=>{for(const listener of old)listener.remove();await context.sync();});}catch{}
        watchContext=null;scope.removeEventListener?.('focus',focusListener);scope.document?.removeEventListener?.('visibilitychange',visibilityListener);
        focusListener=visibilityListener=null;
    }
    async function catalog(requestOwner, dotnet) {
        if(!valid(requestOwner,128) || !dotnet?.invokeMethodAsync)return {code:'host.invalid-response'};
        if(owner!==requestOwner || document!==currentDocument() || !session) {owner=null;callback=null;await removeListeners();owner=requestOwner;callback=dotnet;}
        const result=await native(async(context,nativeCheck)=>{
            const check=()=>{nativeCheck();if(owner!==requestOwner)fail('user.cancelled');};check();
            const sheets=context.workbook.worksheets;
            if(typeof sheets?.load!=='function' || typeof sheets?.getActiveWorksheet!=='function')fail('office.unsupported-api');
            const active=sheets.getActiveWorksheet();active.load('id');
            const count=sheets.getCount?.();if(!count)fail('office.unsupported-api');
            await context.sync();check();
            if(!Number.isSafeInteger(count.value) || count.value<1)fail('host.invalid-response');
            if(count.value>256)fail('spec.sheet-limit');
            sheets.load('items/id,items/name,items/visibility');await context.sync();check();
            if(!Array.isArray(sheets.items) || sheets.items.length!==count.value || sheets.items.some(s=>!valid(s.id,1024)||!valid(s.name,256)||!['Visible','Hidden','VeryHidden'].includes(s.visibility)) || new Set(sheets.items.map(s=>s.id)).size!==sheets.items.length || !sheets.items.some(s=>s.id===active.id))fail('host.invalid-response');
            if(!listeners.length) {
                const registration=++watchVersion;
                const notify=docChanged=>{if(registration===watchVersion && owner===requestOwner)changed(docChanged);};
                const events=[sheets.onChanged,sheets.onNameChanged,sheets.onDeleted,sheets.onAdded];
                if(events.some(e=>typeof e?.add!=='function') || typeof context.workbook.onActivated?.add!=='function')fail('office.unsupported-api');
                for(const event of events)listeners.push(event.add(()=>notify(false)));
                listeners.push(context.workbook.onActivated.add(()=>notify(true)));watchContext=context;
                await context.sync();check();
                focusListener=()=>notify(false);visibilityListener=()=>{if(scope.document?.visibilityState==='visible')notify(false);};
                scope.addEventListener?.('focus',focusListener);scope.document?.addEventListener?.('visibilitychange',visibilityListener);
            }
            if(document!==currentDocument() || !session){document=currentDocument();session=scope.crypto.randomUUID();}
            return {code:'workbook.ok',documentIdentity:session,activeWorksheetId:active.id,worksheets:sheets.items.map(s=>({id:s.id,name:s.name,visibility:s.visibility}))};
        });
        if(result.code!=='workbook.ok')await removeListeners();
        return result;
    }
    async function bounds(requestOwner, identity, id) {
        if(requestOwner!==owner || identity!==session || document!==currentDocument() || !valid(id,1024))return {code:'user.document-changed'};
        return native(async(context,nativeCheck)=>{
            const check=()=>{nativeCheck();if(owner!==requestOwner || session!==identity || document!==currentDocument())fail('user.document-changed');};check();
            const sheet=context.workbook.worksheets.getItem(id);sheet.load('id,name');
            if(typeof sheet.getUsedRangeOrNullObject!=='function' || typeof sheet.getRange!=='function')fail('office.unsupported-api');
            const used=sheet.getUsedRangeOrNullObject(true);used.load('rowIndex,columnIndex,rowCount,columnCount');
            const all=sheet.getRange();if(typeof all.getSpecialCellsOrNullObject!=='function')fail('office.unsupported-api');
            const formulas=all.getSpecialCellsOrNullObject('Formulas');formulas.load('areaCount');
            await context.sync();check();
            if(sheet.id!==id || !valid(sheet.name,256) || typeof used.isNullObject!=='boolean' || typeof formulas.isNullObject!=='boolean')fail('host.invalid-response');
            const spans=[];if(!used.isNullObject)spans.push(validate(shape(used)));
            if(!formulas.isNullObject) {
                if(!Number.isSafeInteger(formulas.areaCount)||formulas.areaCount<1)fail('host.invalid-response');
                if(formulas.areaCount>64)fail('input.range-count');
                formulas.areas.load('items/rowIndex,items/columnIndex,items/rowCount,items/columnCount');await context.sync();check();
                if(!Array.isArray(formulas.areas.items)||formulas.areas.items.length!==formulas.areaCount)fail('user.document-changed');
                spans.push(...formulas.areas.items.map(r=>validate(shape(r))));
            }
            if(!spans.length)return {code:'workbook.ok',documentIdentity:session,empty:true};
            const first=Math.min(...spans.map(r=>r.col)),last=Math.max(...spans.map(r=>r.col+r.cols)),rows=Math.max(...spans.map(r=>r.row+r.rows));
            if(last-first>128)fail('spec.column-limit');
            if(rows*(last-first)>10000)fail('input.range-cells');
            return {code:'workbook.ok',documentIdentity:session,empty:false,reference:{worksheetId:id,worksheetName:sheet.name,workbookId:'',workbookName:'',addressDialect:1,startRow:1,startColumn:first+1,rowCount:rows,columnCount:last-first,address:`${column(first+1)}1:${column(last)}${rows}`}};
        });
    }
    async function stop(requestOwner){if(requestOwner!==owner)return;owner=null;callback=null;epoch++;await removeListeners();}
    async function dispose(){disposed=true;await stop(owner);}
    return {catalog,bounds,stop,dispose};
}
