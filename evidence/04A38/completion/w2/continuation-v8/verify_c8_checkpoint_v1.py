"""Independent C8 source/CIL/branch/ownership reader. Always partial NOT_READY."""
import hashlib,json,re,sys
from collections import Counter,defaultdict
from pathlib import Path
from source_views import body
from verify_c8_composition_v2 import inspect as source_composition
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
C6='evidence/04A38/completion/w2/continuation-v7/'
W1='evidence/04A38/completion/w1/'
AUTH={'../continuation-v7/review-seal-v1.json': 'c16ae8d73cacf602b1a2e2da04b2605e5787f059eeb03d26d598f1a901338b0c', '../continuation-v7/verify_c7_checkpoint_v1.py': '3ed299fec3b9a1c707cdc65bc8f05c222cae8ef797b4207c2cf8796829e6fdf6', '../continuation-v7/verify_composition_v1.py': 'c7f9c14d9178b74cc4d1b2ea2386810d51cb003a9beb72535b2e1891a54a562b', '../continuation-v7/verify_validation_v1.py': '454301af87a525deca02a5a4076376c9f941c54d872b484be4da7944bc057355', '../continuation-v7/whole-body-contracts-v1.json': '42ae55073093a017cfde61a9611e242598718d32881aea881f8ca2507287767a', 'callback-proofs-v2.json': 'aa916e45e20c42315032ecc19b71889a7f390f199fdbe5b86e692bada46280d0', 'captures/packet-v1/Debug/A/results.tsv': 'ac75e34cad07e150bdc45f8020e64278e64c7080394252417bd3de0cbb690db5', 'captures/packet-v1/Debug/B/results.tsv': 'a30aa1a82f60fbf2cca762ece3bf79fbaf24291c5e6709e6896ef1163b35513b', 'captures/packet-v1/Debug/C/results.tsv': 'c036ff73ec9a6eb56fef45551fc69254bf8b66375ff913795cfd7647af40d16e', 'captures/packet-v1/Debug/D/results.tsv': 'fb1372bc77b3cb05b8e0c6964522e10ac6b94c04760eb1ba0e8c7d6490005f4c', 'captures/packet-v1/Debug/E/results.tsv': 'f4cabe1eaea62672ad4e54578431b31b98d354f5308d007dc2d10248393dbd6a', 'captures/packet-v1/Debug/F/results.tsv': '16f4d287c22ade963412832192f1ae2355ad87f1d1bcefa8ea54b946ce0a8722', 'captures/packet-v1/Debug/G/results.tsv': 'b41c738f41b7f94153d89cff62feb8753f1d316bd2324951f52acfed5ecb2279', 'captures/packet-v1/Debug/math-edges-v1/results.tsv': 'faba4ad9a633d88625388b627354d5cea916fcf60ba05e1e7ac6e06c8234b6d6', 'captures/packet-v1/Debug/model-inputs-v2/results.tsv': '3107a6d7b8116beb62f37665ed5d59f6df0cf3c1d2743cf5c28ddf59eaed0e58', 'captures/packet-v1/Debug/preedit-main/results.tsv': 'a33f7772c14a40cadd139258efde1960029503ae9294024efd7f66b23e3daf2e', 'captures/packet-v1/Debug/public-options-v1/results.tsv': '2d3705fffe38f46081e18c6218977b6402d38ef9ee0a882fd5f28e24a6c47b52', 'captures/packet-v1/Debug/public-primitives-v1/results.tsv': '0bc33bbf358d1abf79b93ffe6afbfd54f93c2d07d9c034f95686d83046c22b3b', 'captures/packet-v1/Release/A/results.tsv': 'ac75e34cad07e150bdc45f8020e64278e64c7080394252417bd3de0cbb690db5', 'captures/packet-v1/Release/B/results.tsv': '871d9a0c606015a705ded209429c8e425734148218fdc513997d60564543a24a', 'captures/packet-v1/Release/C/results.tsv': '6fcb91c225671fc99144334f23f58d7e96267c9989f9666e41270e4c5a8f949d', 'captures/packet-v1/Release/D/results.tsv': 'c41219aedcbb91614ae2f2c801edc19d08b6f89a0d05615889ac13d449323f05', 'captures/packet-v1/Release/E/results.tsv': '086ecf6cc806ad18fad68e19052eef7e523e35598b299939b861c730db871b8d', 'captures/packet-v1/Release/F/results.tsv': '8cf9ccf6448e7cf0b7a4aa60aff6e57a14b83e6e25ced602397543984fc7cdf1', 'captures/packet-v1/Release/G/results.tsv': 'f8ecca980945fdd2757caa8f0394693c0418a14f30bc5dd93f0d2d6a7b43e71e', 'captures/packet-v1/Release/math-edges-v1/results.tsv': 'faba4ad9a633d88625388b627354d5cea916fcf60ba05e1e7ac6e06c8234b6d6', 'captures/packet-v1/Release/model-inputs-v2/results.tsv': '3107a6d7b8116beb62f37665ed5d59f6df0cf3c1d2743cf5c28ddf59eaed0e58', 'captures/packet-v1/Release/preedit-main/results.tsv': '4d78707c8f2d0f2c5a431832be7c86a222db034958f16c540bb44b831b7ecccf', 'captures/packet-v1/Release/public-options-v1/results.tsv': '2d3705fffe38f46081e18c6218977b6402d38ef9ee0a882fd5f28e24a6c47b52', 'captures/packet-v1/Release/public-primitives-v1/results.tsv': '0bc33bbf358d1abf79b93ffe6afbfd54f93c2d07d9c034f95686d83046c22b3b', 'compatibility-packet-v1.json': '4d414e98822e6e8bfa289bfcfa53d1526be3824a0cb77bd40e44d4f94c536f32', 'constructed-context-scope-v1.json': 'dc4a7db12186404b18775277cbc0db61b319994a87b2f21e8c100942c513b3d9', 'current-ownership-v2.json': '45e96eab43081a79b7047eb340fc58c9316f66f3c24f852d46dc8226f5d33d92', 'current-universe-packet-v1.json': 'bed445fbe0a215ae2bed720cbadb9bde5fcda0ee9f2ff26c0ca0bc2df86c79a5', 'dependency-source-universe-v1.json': 'c60f05db6e1b0444d1ae70656db530f5a1a681b85495c112907e3496691d5565', 'enum-bridge-source-domain-v1.json': 'cd8ad93c1bded522ac7eaab09abd4b97bcdcb1fec70d4ca786681c53e15cab22', 'historical-callback-contracts-v1.json': '9a86d600a5a369907dedcabab5441c77e79c02757952963ba523dad718e0aa95', 'historical-ownership-v2.json': 'f07edb7ad14462d7ae51256aed0a6931957eb95c6b4039d8b986ec2d0a03dc6c', 'implementation-plan-v1.json': '99eba59e0dfd2c8ddddb83c00b0ee1a87011f6f34a7321251db975c766dc1c59', 'import-layout-extraction-v1.json': '9f7887b9d849b334bbbb58f1bf92aeb391757ce730c462d5a2ff5bf917efdfbb', 'inspection/packet-v1/Inspect38-Debug.json': '2643743fc58ae76b1d559130a8588a45aec8cc7d39c9bfe0c166ce8f71aaaf34', 'inspection/packet-v1/Inspect38-Release.json': 'c861196174faa528b3364dccf4f75c3a36a745ff492c7e554a1654a5078dec4d', 'managed-Debug-identities-v1.json': '427976ccdd97dcb55723396798216812c28f738a1a72946bbf3de32d0bf50cbf', 'managed-validation-v1.json': '1e34cecdacd5ba9e2a6973cc6aa249921cb4c6e642670184947da8e87cccf9ec', 'metadata-all/packet-v1-context-v3/current-Debug.json': 'c8fcca505f9725839ce686660caf948c24feaced8b3a358e9ae85038602b0d3c', 'metadata-all/packet-v1-context-v3/current-Release.json': 'e89293f18eab415648421e30b9dc18e51b0dd5b7b73afa628b491f06b4060f6a', 'metadata-all/packet-v1-context-v3/original-Debug.json': 'c076947fce3431de339fd5dfbf1f972fb71cb687ca59e8f469c53e9afe4ccc98', 'metadata-all/packet-v1-context-v3/original-Release.json': '5fe756510fc1c42a75b1425b5da061d858d82f463961c45a6235da884e359e64', 'new-parent-fragment-crosswalk-v2.json': '1823a3de1f3c08bde439a02228664802625b8032553f67f0ad58574a4aba4efe', 'node-identities-v1.json': '06ab6c238298ae754face96df93a220f96639106f626dc0773ae6dc9650cbcd8', 'numeric-copy-source-universe-v1.json': 'abd5bed8682253a9639642532f69a5b7bdedf9a958a8ba594389c7afa3b28603', 'original-fragment-parent-crosswalk-v1.json': '73ec1b4a67d458169a05efd7a01a1ea54b6fd55ac6c73eb8d4588cd553639044', 'ownership-summary-v1.json': '5e88402e2526553ac0cc91fa1f8cbf5dd003a166dc83bc20011c4d55363ca494', 'parallel-scanner-equivalence-v2.json': '51c6751b1520656b7fd6b623d6f24cc14e96d565568876f2e032a6a1077ce41f', 'parallel_scan_v2.py': 'a4bacd9a574ba5e73b3036aca9a4aeb0a21a1651b405cea6d67a537828e4726d', 'portable-api-contracts-v1.json': 'd7662edeb9e6a22d89147b48963098cbb5c27fc810c01ffb6a301191d2e89fa7', 'portable-universe-v1.json': '44c115af4ad7d0c9f1fb99db42e0225d4ab8a6761d155aac305bd0925584d894', 'privacy-authority-v3.json': '40b4c36ca33353bb0e0999c8bb37fb2727bbd0c6aba3ead82cfb595432e40aef', 'proof-authority-crosswalk-v1.json': 'f686922eda80e3a0b724330164645a908efdf952de50ec564d331d02e8a1d364', 'proof-inputs-v3.json': '1b564637430d09ace782d7dfd8dd7e4be50c7d74a152ca68dead0df376329ec2', 'publication-privacy-equivalence-v4.json': '7e1cde1c55b7072a09f4d2b7330db1204f7a2bcacae6c46e578c661eecee166a', 'publication_privacy_v3.py': 'f0476f4b158730331664dbad2459c0dde6959fccc63609b8624cac7fb8b9a927', 'python-discovery-inventory-v1.json': '624aa0621d8847f0daaf9f62ac200b79f9422164410ede57c705627a268c5f0b', 'python-effective-identities-v1.json': '253b699f3c407701f75a4944937005bdc199910824eec0740722ebfaa082d788', 'python-first-failures-v1.json': 'c9d6294b2fe7b11d883821b28911bae14200f0ed0915d79a6dc7d18377b7427a', 'python-frozen-input-postcheck-v1.json': '405adf3cc829cc6982657faf8d93690b998babdeefff705d503793268e2f69ca', 'python-message-delta-investigation-v1.json': 'daf66feb6bb320174d18f7336680b20a2e1b383cab3a52b899717bdf95d9a00d', 'python-optimization-authentication-v1.json': '6447072cab756427f303ddf300bbeb0e4ee4bee6371d0d214cc477ea1643fa2e', 'python-worker-accounting-v1.json': 'f582845e57b8fb65005ea476faf52d8cf2bdfc9573ddf93408ea341422ff756b', 'python-worker-receipts-v1.json': '6519487d4859d053dbe8cd2f9b2503363f9687b17dfa8d52931ce6ed1980a612', 'python_workers_v2.py': 'dd8dd73871ec11ab4692cb0750b3f779d1bb05296dfc1d48685858968a4c8d62', 'readiness-v1.json': '12fd5de7246ab8d7d3f4bb15255653b98f9fa7b9055774648f4f89461aae5808', 'remaining-work-queue-v1.json': '1297d3e41eeafbee3b601acfcd2b27dbfe85ebf59449535affb6419831cbf15b', 'sample-size-extraction-v1.json': '31f842b91ffab8a271a916e671cd69bbc501ad8b5be219c6bcd0b4aebc90c9b8', 'source-composition-v1.json': '2805b33ececb726f23d7bd42fedc9ebbac62f5f725ea127e5d688515c6a155a6', 'source-transitions-final-v1.json': 'be688648ff057171e9fecb77231be391f1449ba76f604eb091f18ac34e0f2eae', 'source_views.py': '3950c07aef8fe9bc950a5657c5f9275d3fd6b94461d30a4c9a33a571653f8bf4', 'text-frontier-carryforward-v1.json': 'd819ef2c2284d5ee5983ba176b6dc0ae826e88d1f0207fa5b4217a1047b7c686', 'toolchain-inputs-v1.json': 'c17bbcf6e4ce3de85780bbb247892a44abf09b0c6180787d93dfad3b8b5b9cf4', 'transitive-compiled-context-v1.json': '7578095fc1e08067dfcd43af88b03d73411622a9b6e414e4d501ac67fe1d770c', 'unrelated-field-source-qualification-v1.json': '701ccc073d3b8e48b0d0f8be61f3bd8512099a4149a82a5ce52d0daffd33ae6b', 'validation-v1.json': '8038b04e9dc3b79d184f7bd3c6ae9f835b2e0a9e05cb07d52fe18cc859495596', 'verify_c8_composition_v2.py': '6c1956ef31ab3e93a66587348186afe8bd46d35189cdcc22aee26e289cc78e58', 'verify_c8_validation_v1.py': 'b442e5a1455b5a98fa542af6652b8139088063282bf26735fec26f3436658cef', 'whole-body-contracts-v1.json': 'ec02592650e993c964495fd7ee3e984b8f75925f346c4e2e7556daeefd5e589d'} # Sealed version embeds exact authenticated proof-input hashes.
CHANGED={('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_BLANDALTMAN'), ('BESHStatNG.UdfDataImport', 'TryBuildPredictorDataFromUdfArgs'), ('BESHStatNG.UdfDataImport', 'TryGetAlignedClusterIdColumnObject'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_PROP_SINGLE'), ('BESHStatNG.UdfDataImport', 'TryBuildGlmDataFromUdfArgs'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_COX_BINARY'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_TTEST_UNPAIRED'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_ICC'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_PROP_INDEP'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_COX_CONTINUOUS'), ('BESHStatNG.UdfDataImport', 'TryBuildCoxDataFromUdfArgs'), ('BESHStatNG.UdfDataImport', 'TryBuildGeeDataFromUdfArgs'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_LOGRANK')}
NEW_TARGETS={('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_TTEST_UNPAIRED'): ('BESHStatNG.ExcelCommon.UdfSampleSizePreparation38', 'PrepareUnpaired'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_PROP_SINGLE'): ('BESHStatNG.ExcelCommon.UdfSampleSizePreparation38', 'PrepareSingleProportion'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_PROP_INDEP'): ('BESHStatNG.ExcelCommon.UdfSampleSizePreparation38', 'PrepareIndependentProportions'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_LOGRANK'): ('BESHStatNG.ExcelCommon.UdfSampleSizePreparation38', 'PrepareLogRank'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_COX_BINARY'): ('BESHStatNG.ExcelCommon.UdfSampleSizePreparation38', 'PrepareCoxBinary'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_COX_CONTINUOUS'): ('BESHStatNG.ExcelCommon.UdfSampleSizePreparation38', 'PrepareCoxContinuous'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_ICC'): ('BESHStatNG.ExcelCommon.UdfSampleSizePreparation38', 'PrepareIcc'), ('BESHStatNG.WorksheetFunctions.SampleSizeUDFs', 'SSIZE_BLANDALTMAN'): ('BESHStatNG.ExcelCommon.UdfSampleSizePreparation38', 'PrepareBlandAltman'), ('BESHStatNG.UdfDataImport', 'TryBuildGeeDataFromUdfArgs'): ('BESHStatNG.ExcelCommon.UdfModelImportPreparation38', 'InitializeResponsePredictorsPrepared'), ('BESHStatNG.UdfDataImport', 'TryBuildGlmDataFromUdfArgs'): ('BESHStatNG.ExcelCommon.UdfModelImportPreparation38', 'InitializeResponsePredictorsPrepared'), ('BESHStatNG.UdfDataImport', 'TryBuildPredictorDataFromUdfArgs'): ('BESHStatNG.ExcelCommon.UdfModelImportPreparation38', 'InitializePredictorsPrepared'), ('BESHStatNG.UdfDataImport', 'TryBuildCoxDataFromUdfArgs'): ('BESHStatNG.ExcelCommon.UdfModelImportPreparation38', 'InitializeCoxPrepared'), ('BESHStatNG.UdfDataImport', 'TryGetAlignedClusterIdColumnObject'): ('BESHStatNG.ExcelCommon.UdfNumericImportPreparation38', 'CopyTrimmedColumnPrepared')}

def need(value,message):
    if not value:raise ValueError(message)
def sha(data):return hashlib.sha256(data).hexdigest()
def semantic(row):return {k:v for k,v in row.items() if k!='raw_IL_sha256'}
def definitions(data):
    result={r['method']:r for r in data['methods']};need(len(result)==len(data['methods']),'Duplicate complete CLR method');return result
def assembly_of_method(key):return key.split(' @ ',1)[1].split(',',1)[0]
def closure(data,keys):
    methods=definitions(data);pending=list(keys);seen=set();external=set();edges=[]
    while pending:
        key=pending.pop()
        if key in seen:continue
        row=methods.get(key)
        if row is None:external.add(key);continue
        seen.add(key)
        for index,ins in enumerate(row['instructions']):
            if ins['op'] in ['call','callvirt','newobj','ldftn','ldvirtftn'] and isinstance(ins['operand'],str) and ins['operand'].startswith('METHOD:'):
                target=ins['operand'][7:];edges.append(dict(caller=key,instruction=index,op=ins['op'],target=target));pending.append(target)
    need(not any(assembly_of_method(key) in ['BESHStatNG','BESHStat.Core','BESHStat.Excel.Common'] for key in external),'Unresolved actual internal constructed/interface/compiler context')
    return dict(methods=sorted(seen),external_runtime_or_ExcelDna=sorted(external),edges=edges)
def headers(universe,closed_support):
    result={}
    for support in universe['supports']:
        if support['support_id'] not in closed_support or support['kind']!='ModuleBlock':continue
        for f in universe['fragments']:
            if f['member_id'] is None and f['path']==support['path'] and f['kind'] in ['ModuleStatement','EndModuleStatement'] and support['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=support['span_start']+support['span_length']:
                result[f['fragment_id']]=support['support_id']
    return result

def compute(root=ROOT,overrides=None,check_ledgers=True):
    root=Path(root);overrides=overrides or {};cache={}
    def raw(path):return overrides[path] if path in overrides else (root/path).read_bytes()
    def load(path):
        if path not in cache:cache[path]=json.loads(raw(path))
        return cache[path]
    def check(row):
        data=raw(row['path']);need((len(data),sha(data))==(row['bytes'],row['sha256']),'Source/evidence pin mismatch '+row['path'])
    for path,digest in AUTH.items():need(sha(raw(PREFIX+path))==digest,'Authenticated C8 input changed '+path)
    inputs=load(PREFIX+'proof-inputs-v3.json');contracts=load(PREFIX+'whole-body-contracts-v1.json')['contracts']
    transitions=load(PREFIX+'source-transitions-final-v1.json')['files'];projected=dict(overrides)
    need(len(transitions)==6 and len({r['path'] for r in transitions})==6,'Overlapping/missing source transition')
    for t in transitions:
        for key in ['after','snapshot']:check(t[key])
        if t['preimage'] is not None:check(t['preimage'])
        need(raw(t['after']['path'])==raw(t['snapshot']['path']),'Current after-image mismatch')
        if t['before'] is not None:
            need((t['before']['bytes'],t['before']['sha256'])==(t['preimage']['bytes'],t['preimage']['sha256']),'Forged exact entry preimage')
            projected[t['path']]=raw(t['preimage']['path'])
        else:need(t.get('entry_state')=='ABSENT' and t['path'] in {r['path'] for r in load(PREFIX+'implementation-plan-v1.json')['new_sources']},'Unapproved new source path')
    # Re-execute the prior fail-closed proof against the authenticated C7 entry view.
    sys.path.append(str(root/C6))
    from verify_c7_checkpoint_v1 import compute as c6_compute
    oldproof=c6_compute(root,projected)
    need(oldproof['historical']==dict(total=704,closed=274,OPEN=430),'C7 inherited historical proof changed')
    current=load(PREFIX+inputs['current_universe']);original=load(W1+'original-universe.json')
    oldcurrent=load(C6+'current-ownership-v1.json');oldhistory=load(C6+'historical-ownership-v1.json')['members']
    members={n['symbol_id']:n for n in current['members']};originals={n['symbol_id']:n for n in original['members']}
    need(len(members)==len(current['members'])==726 and len(oldhistory)==704,'Current/historical census identity multiplicity')
    for source in current['sources']:check(source)
    bykey=defaultdict(list);byowner=defaultdict(list)
    for n in current['members']:
        bykey[n['signature_key']].append(n)
        if n['parent_member'] is None:byowner[n['owner'],n['name']].append(n)
    inherited=set()
    for before in oldcurrent['members']:
        if before['review_status'].startswith('OPEN'):continue
        candidates=[n for n in bykey[before['signature_key']] if n['body_token_sha256']==before['body_token_sha256'] and (n['parent_member'] or '').split('@')[0]==(before['parent_member'] or '').split('@')[0]]
        need(len(candidates)==1,'Changed/lost inherited complete source body');need(candidates[0]['body_sha256']==before['body_sha256'],'Changed inherited complete body bytes');inherited.add(candidates[0]['symbol_id'])
    history_by_id={h['canonical_original_id']:h for h in oldhistory}
    reviewed={c['current_id'] for c in contracts};need(len(reviewed)==len(contracts)==49,'Overlapping/missing full contracts')
    newhistory=set();newcurrent=set();snapshots={r['source']['path']:r['snapshot'] for r in load('evidence/04A38/preedit/entry-source-snapshot.json')['files']}
    for c in contracts:
        n=members[c['current_id']];need(c['current_record']==n,'Full current body identity differs')
        check(c['current_source']);text,view=body(raw(n['path']),n)
        need((text,view)==(c['current_whole_body'],c['current_source_view']),'Full current source/UTF16 view differs')
        need(c['new_support']==(c['current_id'] not in inherited),'Invented new/current closure')
        if c['new_support']:newcurrent.add(c['current_id'])
        if c['original_id'] is not None:
            h=history_by_id[c['original_id']];old=originals[h['git_original_id']]
            need(c['original_record']==old and c['original_source']==snapshots[h['path']],'Canonical original/full source mapping differs')
            check(c['original_source']);text,view=body(raw(c['original_source']['path']),old)
            need((text,view)==(c['original_whole_body'],c['original_source_view']),'Original full body/source hash/view differs')
            need(c['new_support']==(not bool(h['proof'])),'Reopened/forged historical closure')
            if c['new_support']:newhistory.add(c['original_id'])
        else:need((n['name'] in ['ResolveSampleSizeHypothesisKind38','MaterializeSampleSizeResult38'] and c['new_support']) or (not c['new_support'] and c['current_id'] in inherited),'Invented historical/current-only identity')
        need(len(c['review'])>150,'Missing concrete whole-parent boundary review')
        for key,universe,parent in [('current_fragments',current,n['symbol_id']),('original_fragments',original,h['git_original_id'] if c['original_id'] else None)]:
            need(c[key]==[f for f in universe['fragments'] if parent is not None and f['member_id']==parent],'Missing individual statement/branch/inline fragment')
    historical_callbacks=load(PREFIX+'historical-callback-contracts-v1.json')['rows']
    need(len(historical_callbacks)==1,'Historical callback multiplicity')
    for row in historical_callbacks:
        h=history_by_id[row['original_id']];n=members[row['current_id']];o=originals[h['git_original_id']]
        need(not h['proof'] and n['symbol_id'] in inherited and row['new_historical_support'] and not row['new_current_support'],'Invented callback population change')
        need(row['original_record']==o and row['current_record']==n and row['original_source']==snapshots[h['path']],'Historical callback identity/source mismatch')
        check(row['original_source']);check(row['current_source'])
        for side,node in [('original',o),('current',n)]:
            text,view=body(raw(row[side+'_source']['path']),node)
            need((text,view)==(row[side+'_whole_body'],row[side+'_source_view']),'Changed historical callback source')
            universe=original if side=='original' else current
            need(row[side+'_fragments']==[f for f in universe['fragments'] if f['member_id']==node['symbol_id']],'Historical callback branch/inline omitted')
        need(row['original_whole_body']==row['current_whole_body'],'Historical callback body drift')
        newhistory.add(row['original_id'])
    need(newhistory==set(inputs['new_historical_ids']) and len(newhistory)==24,'Canonical newly supported set differs')
    priority=load(C6+'remaining-work-queue-v1.json')['first_members'];need(inputs['priority']==priority and len(priority)==24,'Changed precise handover queue')
    need({r['original_id'] for r in priority}<=newhistory,'Priority canonical body remains unsupported')
    closed=inherited|reviewed;callbacks=load(PREFIX+'callback-proofs-v2.json')['rows']
    need(len(callbacks)==len({r['current_id'] for r in callbacks})==11,'Callback multiplicity differs')
    closed.update(r['current_id'] for r in callbacks)
    need(set(inputs['reviewed_current'])==reviewed|{r['current_id'] for r in callbacks},'Current-only or callback omitted')
    contexts={};retention=[]
    for config in ['Debug','Release']:
        now=load(PREFIX+inputs['metadata_prefix']+'current-'+config+'.json');before=load(PREFIX+inputs['metadata_prefix']+'original-'+config+'.json');old=load(C6+'metadata-all/packet-v1-context-v3/current-'+config+'.json')
        need(now['metadata_only'] and before['metadata_only'] and before['original_semantic_calls']==0,'Original execution authority exceeded')
        methods=definitions(now);prior=definitions(old);original_methods=definitions(before);types={t['type']:t for t in now['types']}
        changed=set()
        for key,row in prior.items():
            need(key in methods,'Lost retained complete C7 CIL method')
            if semantic(row)!=semantic(methods[key]):changed.add((row['declaring_type'],row['member_name']))
        need(changed==CHANGED,'Unreviewed complete C7 CIL transition')
        for t in old['types']:
            if t['type'] is not None:
                measured=types.get(t['type'])
                if t['type']=='BESHStatNG.psmData+_Closure$__':
                    need(measured is not None and {f['name'] for f in t['fields']}=={'$I','$I84-0'} and all(f['is_static'] for f in t['fields']) and 'AutoLayout' in t['attributes'],'Unexpected unrelated cache context')
                    need(dict(t,fields=sorted(t['fields'],key=lambda f:f['name']))==dict(measured,fields=sorted(measured['fields'],key=lambda f:f['name'])),'Changed named unrelated static-cache field semantics')
                else:need(measured==t,'Changed retained C7 field/closure/type context')
        need(Counter(json.dumps(t,sort_keys=True) for t in old['types'] if t['type'] is None)==Counter(json.dumps(t,sort_keys=True) for t in now['types'] if t['type'] is None),'Changed retained unused open-constructed raw context')
        core_original={k:r for k,r in original_methods.items() if assembly_of_method(k)=='BESHStat.Core'}
        need(all(k in methods and semantic(r)==semantic(methods[k]) for k,r in core_original.items()),'Original Core engine/constructed-method drift')
        for t in before['types']:
            if t['assembly']=='BESHStat.Core' and t['type'] is not None:need(types.get(t['type'])==t,'Original Core field/closure context drift')
        for (owner,name),(target_owner,target_name) in NEW_TARGETS.items():
            caller=[r for r in methods.values() if (r['declaring_type'],r['member_name'])==(owner,name)];target=[r for r in methods.values() if (r['declaring_type'],r['member_name'])==(target_owner,target_name)]
            need(len(caller)==len(target)==1 and sum(i['op']=='call' and i['operand']=='METHOD:'+target[0]['method'] for i in caller[0]['instructions'])==1,'Changed actual Windows portable consumer')
        wrapper=[r for r in methods.values() if r['declaring_type']=='BESHStatNG.Matrix.Matrix' and r['member_name']=='MatCovar']
        need(len(wrapper)==1,'MatCovar facade missing');target='METHOD:'+next(r['method'] for r in methods.values() if r['declaring_type']=='BESHStatNG.Matrix.MatrixStatisticsCore' and r['member_name']=='SampleCovariance')
        need(sum(i['op']=='call' and i['operand']==target for i in wrapper[0]['instructions'])==1,'Changed MatCovar engine target')
        for row in historical_callbacks:
            bound=next(r for r in row['configurations'] if r['configuration']==config)
            b,a=bound['original'],bound['current']
            for part in ['parent','target']:
                need(b[part]==original_methods[b[part]['method']] and a[part]==methods[a[part]['method']],'Fabricated historical callback compiled member')
                need(semantic(b[part])==semantic(a[part]),'Historical callback caller/instructions changed')
            need(b['target']['member_name']=='_Lambda$__94-0' and b['target']['method']==a['target']['method'] and bound['actual_method_aliases']==[],'Invented callback alias')
            need(bound['required_field_names']==['$I','$I94-0'],'Wrong callback cache context')
            for side,meta,defs in [(b,before,original_methods),(a,now,methods)]:
                context=next(t for t in meta['types'] if t['type']==side['target']['declaring_type'])
                need(side['context']==context,'Changed complete callback field/type context')
                ctors=[r for r in defs.values() if r['declaring_type']==context['type'] and r['member_name'] in ['.ctor','.cctor']]
                need(side['constructors']==ctors,'Dropped callback constructor')
                fields=[i for i in side['parent']['instructions'] if isinstance(i['operand'],str) and i['operand'].startswith('FIELD:')]
                need(side['fields']==fields,'Dropped real callback cache access')
            need(b['fields']==a['fields'] and [semantic(c) for c in b['constructors']]==[semantic(c) for c in a['constructors']],'Callback cache/initialization drift')
            bf={f['name']:f for f in b['context']['fields']};af={f['name']:f for f in a['context']['fields']}
            need(all(bf[k]==af[k] for k in bound['required_field_names']),'Callback relevant field drift')
        root_keys=[]
        for c in contracts:
            bound=next(r for r in c['compiled'] if r['configuration']==config)
            expected=[r['method'] for r in methods.values() if (r['declaring_type'],r['member_name'])==(c['current_record']['owner'],c['current_record']['name'])]
            need(bound['roots']==expected,'Wrong overload/arity/array-rank compiled root')
            graph=closure(now,expected);need(bound['transitive']==graph,'Missing transitive branch/callback/catch dependency')
            candidates=sorted({n['symbol_id'] for key in graph['methods'] for n in byowner.get((methods[key]['declaring_type'],methods[key]['member_name']),[])})
            need(bound['source_dependencies']==candidates and set(candidates)<=closed,'OPEN native source dependency under claimed parent')
            if c['current_id'] in inputs['root_current_ids']:root_keys.extend(expected)
        root_members=[members[key] for key in inputs['root_current_ids']]
        root_keys=[r['method'] for r in methods.values() if any((r['declaring_type'],r['member_name'])==(n['owner'],n['name']) for n in root_members)]
        need(load(PREFIX+'transitive-compiled-context-v1.json')['configurations'][config]==closure(now,root_keys),'Root transitive proof differs')
        reached=closure(now,root_keys)['methods']
        need(not any(key.startswith('BESHStatNG.psmData+_Closure$__ @ ') for key in reached),'Unrelated field-order qualification entered claimed path')
        need(all(methods[key]['declaring_type'] is not None for key in reached),'Unresolved actual claimed declaring type')
        need(not any(isinstance(i['operand'],str) and i['operand'].startswith('FIELD:.') for key in reached for i in methods[key]['instructions']),'Unresolved actual claimed field owner')
        for row in callbacks:
            n=members[row['current_id']];need(row['current_record']==n and n['parent_member']==row['parent_id'] and row['parent_id'] in (reviewed|{r['current_id'] for r in callbacks}),'Orphaned callback')
            check(row['source']);text,view=body(raw(n['path']),n);need((text,view)==(row['whole_body'],row['source_view']),'Changed callback body')
            binding=next(r for r in row['configurations'] if r['configuration']==config)
            parent=next((c for c in contracts if c['current_id']==row['parent_id']),None)
            if parent is not None:bound=next(r for r in parent['compiled'] if r['configuration']==config)
            else:
                parent_callback=next(r for r in callbacks if r['current_id']==row['parent_id'])
                bound=dict(roots=[next(r['target'] for r in parent_callback['configurations'] if r['configuration']==config)])
            need(binding['parent_method'] in bound['roots'],'Wrong callback caller')
            siblings=sorted([m for m in current['members'] if m['parent_member']==row['parent_id']],key=lambda r:r['span_start'])
            targets=[i['operand'][7:] for i in methods[binding['parent_method']]['instructions'] if i['op']=='ldftn' and i['operand'][7:] in methods and methods[i['operand'][7:]]['member_name'].startswith('_Lambda$__')]
            need(len(targets)==len(siblings) and targets[siblings.index(n)]==binding['target'],'Lost real callback/source ordinal')
            need(binding['complete_method']==methods[binding['target']] and binding['declaring_type']==types[methods[binding['target']]['declaring_type']],'Changed complete callback/fields/instructions')
        need({r['current_id'] for r in callbacks}=={n['symbol_id'] for n in current['members'] if n['parent_member'] in (reviewed|{r['current_id'] for r in callbacks})},'Missing source callback')
        actual=load(PREFIX+'inspection/packet-v1/Inspect38-'+config+'.json')['exports'];frozen=load('evidence/04A38/preedit/compiled-'+config+'.json')['exports']
        need(len(actual)==268 and sorted(actual,key=lambda r:r['effective_excel_name'])==sorted(frozen,key=lambda r:r['effective_excel_name']),'Changed public export/default/attribute')
        # If retained compile artifacts are available, bind their exact bytes too.
        for a in now['assemblies']:
            library=root/'.test-rationalization-04A38.tmp/completion-w2-cont8/packet-v1'/config/'lib'/a['file_name']
            if library.is_file():
                blob=raw(library.relative_to(root).as_posix());need((len(blob),sha(blob))==(a['bytes'],a['sha256']),'Compiled current DLL bytes differ')
        contexts[config]=now;retention.append(dict(configuration=config,retained_C7_methods=len(prior)-13,changed=13,original_Core_methods=len(core_original),original_Core_types=sum(t['assembly']=='BESHStat.Core' for t in before['types']),exports=268,compiler_aliases=[]))
    portable=load(PREFIX+'portable-api-contracts-v1.json');pu=load(PREFIX+'portable-universe-v1.json')
    need(len(portable['contracts'])==len(pu['members'])==15 and portable['public_preparation_apis']==12,'Dropped portable declaration')
    need({r['source_record']['symbol_id'] for r in portable['contracts']}=={n['symbol_id'] for n in pu['members']},'Portable declaration identity mismatch')
    for s in pu['sources']:check(s)
    for row in portable['contracts']:
        n=next(n for n in pu['members'] if n['symbol_id']==row['source_record']['symbol_id']);need(row['source_record']==n,'Changed portable source/span')
        check(row['source']);text,view=body(raw(n['path']),n);need((text,view)==(row['whole_body'],row['source_view']),'Portable full body differs')
        need(row['fragments']==[f for f in pu['fragments'] if f['member_id']==n['symbol_id']],'Missing portable branch/inline')
        for bound in row['compiled']:
            meta=contexts[bound['configuration']];defs=definitions(meta);method=bound['method']
            need(defs.get(method['method'])==method and (method['declaring_type'],method['member_name'])==(n['owner'],n['name']),'Wrong portable compiled declaration')
            context=next(t for t in meta['types'] if t['type']==n['owner']);need(bound['declaring_type']==context and context['fields']==[],'New mutable portable state')
            if n['signature'].startswith('Public Shared'):
                callers=[m for m in meta['methods'] if assembly_of_method(m['method'])=='BESHStatNG' and any(i['op']=='call' and i['operand']=='METHOD:'+method['method'] for i in m['instructions'])]
                need(callers,'Unused extracted portable API lacks a real Windows consumer')
    need(len(portable['enum_types'])==2,'Dropped enum contract')
    for row in portable['enum_types']:
        for bound in row['compiled']:
            context=next(t for t in contexts[bound['configuration']]['types'] if t['type']=='BESHStatNG.ExcelCommon.'+row['name'])
            need(bound['context']==context and {f['name']:f['literal'] for f in context['fields'] if f['is_literal']}==row['values'],'Changed enum representation')
    dispositions=[]
    for h in oldhistory:
        keys={r.split('@')[0] for r in h['current_ids']}|{h['git_original_id'].split('@')[0]}
        mapped=sorted({n['symbol_id'] for key in keys for n in bykey.get(key,[])})
        supported=bool(h['proof']) or h['canonical_original_id'] in newhistory
        if supported:need(set(mapped)<=closed,'Lost supported canonical current mapping')
        dispositions.append(dict(id=h['canonical_original_id'],current_ids=mapped,supported=supported))
    old_support={(s['path'],s['kind'],s['owner'],s['declaration']):s for s in oldcurrent['support']};support=[]
    for s in current['supports']:
        old=old_support[s['path'],s['kind'],s['owner'],s['declaration']];supported=old['fully_reconciled'];children=[]
        if s['kind']=='ModuleBlock' and s['declaration'].split()[-1]=='SampleSizeUDFs':
            children=[n['symbol_id'] for n in current['members'] if n['path']==s['path'] and s['span_start']<=n['span_start'] and n['span_start']+n['span_length']<=s['span_start']+s['span_length']]
            need(children and set(children)<=closed,'Sample-size container has an OPEN child')
            for config in contexts:
                t=next(t for t in contexts[config]['types'] if t['type']=='BESHStatNG.WorksheetFunctions.SampleSizeUDFs')
                need({f['name'] for f in t['fields']}=={'HypothesisSuperiority','HypothesisNonInferiority','HypothesisEquivalence'} and all(f['is_literal'] and f['is_static'] for f in t['fields']),'Unexpected sample-size module state')
            supported=True
        support.append(dict(id=s['support_id'],supported=supported,distinct=not s['already_in_primary_population'],children=children))
    closed_support={s['id'] for s in support if s['supported']};current_headers=headers(current,closed_support)
    closed_support_keys={(s['path'],s['kind'],s['owner'],s['declaration']) for s in current['supports'] if s['support_id'] in closed_support}
    original_headers=headers(original,{s['support_id'] for s in original['supports'] if (s['path'],s['kind'],s['owner'],s['declaration']) in closed_support_keys})
    original_closed={h['git_original_id'] for h,d in zip(oldhistory,dispositions) if d['supported']}
    def count(fragments,covered):return dict(total=len(fragments),branches=sum(f['is_branch'] for f in fragments),inline=sum(f['is_inline'] for f in fragments),covered=sum(covered(f) for f in fragments),branches_covered=sum(f['is_branch'] and covered(f) for f in fragments),inline_covered=sum(f['is_inline'] and covered(f) for f in fragments))
    fragments=dict(current=count(current['fragments'],lambda f:f['member_id'] in closed or f['fragment_id'] in current_headers),original=count(original['fragments'],lambda f:f['member_id'] in original_closed or f['fragment_id'] in original_headers))
    hs=dict(total=704,closed=sum(d['supported'] for d in dispositions),OPEN=sum(not d['supported'] for d in dispositions));cs=dict(total=726,closed=len(closed),OPEN=726-len(closed));ss=dict(raw=len(support),distinct=sum(s['distinct'] for s in support),distinct_closed=sum(s['distinct'] and s['supported'] for s in support),distinct_OPEN=sum(s['distinct'] and not s['supported'] for s in support))
    need(hs['OPEN']>0 and cs['OPEN']>0 and ss['distinct']==303 and ss['distinct_OPEN']>0,'Unsupported READY/inventory change')
    totals=Counter();comparisons=load(PREFIX+'compatibility-packet-v1.json')['results'];oldcomparisons={(r['configuration'],r['corpus']):r for r in load(C6+'compatibility-packet-v1.json')['results']}
    for r in comparisons:
        previous=oldcomparisons[r['configuration'],r['corpus']]
        need(all(r[k]==previous[k] for k in ['original','inputs','recorder','cases']),'Changed original reference/input/recorder chronology')
        for key in ['current','original','inputs','recorder']:check(r[key])
        need(raw(r['current']['path'])==raw(r['original']['path']) and r['exact'],'Current frozen compatibility mismatch')
        need(len([s for s in raw(r['inputs']['path']).splitlines() if s.strip()])==r['cases'],'Changed frozen input population')
        totals[r['configuration'],'late' if r['corpus'] in list('ABCDEFG') else 'preedit']+=r['cases']
    need(len(comparisons)==24 and dict(totals)=={(c,p):n for c in ['Debug','Release'] for p,n in [('preedit',28966),('late',458)]},'Frozen population/provenance mismatch')
    composition=source_composition(root,overrides)
    result=dict(status='INDEPENDENT_C8_PARTIAL_SOURCE_CIL_BRANCH_FROZEN_PASS_NOT_READY',historical=hs,current=cs,support=ss,fragments=fragments,newly_supported_historical=sorted(newhistory),newly_supported_current=sorted(closed-inherited),newly_supported_function_bodies=sorted(newcurrent),newly_supported_existing_callbacks=sorted({r['current_id'] for r in callbacks}-inherited),closed_current=sorted(closed),reviewed_current=sorted(reviewed),callbacks=sorted(r['current_id'] for r in callbacks),historical_dispositions=dispositions,support_dispositions=support,header_fragments=current_headers,original_header_fragments=original_headers,compiled_retention=retention,source_composition=composition,W2='IN_PROGRESS',W3='NOT_STARTED',original_queries=0)
    if check_ledgers:
        summary=load(PREFIX+'ownership-summary-v1.json');hist=load(PREFIX+'historical-ownership-v2.json')['members'];cur=load(PREFIX+'current-ownership-v2.json')
        need(summary['status']=='NOT_READY_04A38' and summary['W2']=='IN_PROGRESS' and summary['W3']=='NOT_STARTED','False READY/W3 promotion')
        need(all(summary[k]==result[k] for k in ['historical','current','support','fragments']),'Forged ownership/fragment totals')
        need(len(hist)==704 and [r['canonical_original_id'] for r in hist]==[r['id'] for r in dispositions],'Canonical historical set/order altered')
        for r,d,old in zip(hist,dispositions,oldhistory):
            need(r['current_ids']==d['current_ids'] and bool(r['proof'])==d['supported'],'Unsupported historical closure')
            if old['proof']:need(r['proof']==old['proof'],'Inherited proof authority rewritten')
            need(all(r[k]==v for k,v in old.items() if k not in ['current_ids','proof','disposition','partial_implementation']),'Historical identity/source lineage changed')
        need(Counter(r['symbol_id'] for r in cur['members'])==Counter(members.keys()),'Current identities missing/duplicated')
        need({r['symbol_id'] for r in cur['members'] if not r['review_status'].startswith('OPEN')}==closed,'Current OPEN body falsely closed')
        need(all(all(r[k]==v for k,v in members[r['symbol_id']].items()) for r in cur['members']),'Current source metadata altered')
        need(Counter(r['support_id'] for r in cur['support'])==Counter(s['support_id'] for s in current['supports']),'Missing support declaration')
        need({r['support_id'] for r in cur['support'] if r['fully_reconciled']}==closed_support,'Unsupported support closure')
        support_source={r['support_id']:r for r in current['supports']}
        need(all(all(r[k]==v for k,v in support_source[r['support_id']].items()) for r in cur['support']),'Support source/span/field declaration altered')
        need(Counter(r['fragment_id'] for r in cur['fragments'])==Counter(f['fragment_id'] for f in current['fragments']),'Fragment missing/duplicated')
        fragment_source={r['fragment_id']:r for r in current['fragments']}
        for f in cur['fragments']:
            need(all(f[k]==v for k,v in fragment_source[f['fragment_id']].items()),'Fragment statement/branch/inline source altered')
            need((f['review_status']=='COVERED_BY_REVIEWED_BODY_OR_DECLARATION')==(f['member_id'] in closed or f['fragment_id'] in current_headers),'Branch/inline status forged')
        queue=load(PREFIX+'remaining-work-queue-v1.json')
        need(Counter(r['original_id'] for r in queue['members'])==Counter(r['id'] for r in dispositions if not r['supported']),'OPEN canonical queue differs')
        need(Counter(r['current_id'] for r in queue['current_OPEN'])==Counter(set(members)-closed),'OPEN current queue differs')
        old_rows=load(PREFIX+'original-fragment-parent-crosswalk-v1.json')['rows'];need(Counter(r['fragment_id'] for r in old_rows)==Counter(f['fragment_id'] for f in original['fragments']),'Original fragments missing/duplicate')
        omap={f['fragment_id']:f for f in original['fragments']}
        for r in old_rows:
            f=omap[r['fragment_id']];need(r['parent']==f['member_id'] and r['is_branch']==f['is_branch'] and r['is_inline']==f['is_inline'],'Original fragment source mapping changed')
            need((r['status']=='COVERED_BY_PROVEN_BODY_OR_DECLARATION')==(f['member_id'] in original_closed or f['fragment_id'] in original_headers),'Original branch/inline coverage forged')
    if check_ledgers:
        from verify_c8_validation_v1 import inspect as validation
        result['validation']=validation(load,PREFIX,raw)
    return result

if __name__=='__main__':
    try:
        result=compute();print(json.dumps({k:result[k] for k in ['status','historical','current','support','fragments','W2','W3']}))
    except Exception as error:
        print(json.dumps(dict(status='NOT_READY_04A38',error=type(error).__name__+': '+str(error))));raise SystemExit(1)
