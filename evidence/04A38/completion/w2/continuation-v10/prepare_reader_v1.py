"""Version C9's fail-closed full source/CIL reader for the exact C10 proof frontier."""
from common import *
source=(PREVIOUS/'verify_c9_checkpoint_template_v2.py').read_text(encoding='utf-8')
source=source.replace('C9 full-source','C10 full-source').replace('from proof_kernel_v1 import','from proof_kernel_v2 import').replace('from verify_c9_composition_v1 import','from verify_c10_composition_v1 import').replace("OLD='evidence/04A38/completion/w2/continuation-v8/'","OLD='evidence/04A38/completion/w2/continuation-v9/'")
start=source.index('CHANGED=');end=source.index('\ndef sha(',start)
source=source[:start]+"CHANGED={('BESHStatNG.WorksheetFunctions.ContingencyTableUDFs',n) for n in ['CT_MCNEMAR_EXACT','CT_ODDS_RATIO','CT_RISK_RATIO','CT_TREND','CT_ORDINAL_ASSOC','CT_MANTEL_HAENSZEL','CT_SINGLE_PROPORTION','CT_TWO_INDEPENDENT_PROPORTIONS','CT_PAIRED_PROPORTIONS','CT_TWO_INDEPENDENT_PROPORTIONS_NI','CT_TWO_INDEPENDENT_PROPORTIONS_EQUIV']}|{('BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs',n) for n in ['PCA_SUMMARY','PCA_EIGEN','FA_SUMMARY','FA_EIGEN']}\n"+source[end:]
source=source.replace("    def raw(path):return overrides[path] if path in overrides else (root/path).read_bytes()", "    import posixpath\n    normalized={posixpath.normpath(k):v for k,v in overrides.items()}\n    def raw(path):\n        key=posixpath.normpath(path)\n        if key in normalized:\n            if normalized[key] is None:raise FileNotFoundError(key)\n            return normalized[key]\n        return (root/key).read_bytes()")
source=source.replace('==8,\'Missing/overlapping source transitions\'','==6,\'Missing/overlapping source transitions\'').replace('from verify_c8_publication_v4 import compute as prior_compute','from verify_c9_publication_v1 import compute as prior_compute').replace('dict(total=704,closed=298,OPEN=406)','dict(total=704,closed=343,OPEN=361)').replace('C8 historical proof drift','C9 historical proof drift').replace('current-universe-packet-v1.json','current-universe-packet-v2.json').replace('==99 and len(callbacks)==len({c[\'current_id\'] for c in callbacks})==18','==74 and len(callbacks)==len({c[\'current_id\'] for c in callbacks})==8').replace("else:need(n['parent_member'] is not None,'Invented historical parent identity')","else:need(n['parent_member'] is not None or (n['name']=='MaterializePortableReport38' and not c['new_support']),'Invented historical parent identity')").replace('len(newh)==45 and len(newcurrent)==49','len(newh)==26 and len(newcurrent)==26').replace('==15 and {c[\'source_record\']','==21 and {c[\'source_record\']')
source=source.replace("oldmeta=load(OLD+'metadata-all/packet-v1-context-v3/current-'","oldmeta=load(OLD+'metadata-all/packet-v1-context-v5/current-'")
source=source.replace("need(record['original_method']==originaldefs[record['original_method']['method']],'Forged original compiled member')", "need(record['original_method']==originaldefs[record['original_method']['method']] if c['original_id'] else record['original_method'] is None,'Forged original/current-only compiled member')")
source=source.replace("completion-w2-cont9/packet-v1/","completion-w2-cont10/packet-v2/")
source=source.replace('len(olddefs)-12','len(olddefs)-15').replace('changed_existing=12','changed_existing=15').replace('new_source_methods=15','new_source_methods=16').replace('len(defs)-len(olddefs)-15','len(defs)-len(olddefs)-16')
source=source.replace("newmodules={'BESHStatNG.WorksheetFunctions.AgreementMethodUDFs','BESHStatNG.WorksheetFunctions.ClassificationReportUDFs','BESHStatNG.UDFhelpers'}", "newmodules={'BESHStatNG.WorksheetFunctions.ContingencyTableUDFs'}\n    host=load(PREFIX+'host-cache-contexts-v1.json');need(len(host['rows'])==2 and len(host['configurations'])==2,'Missing host field/cache/copy contexts')")
needle="        need(s['fully_reconciled']==expected,'False support/container closure')";assert needle in source
source=source.replace(needle,"""        if not expected and s['kind']=='ClassBlock' and s['owner']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs' and s['declaration'] in ['Private Class PcaHandle','Private Class FactorAnalysisHandle']:
            row=next(r for r in host['rows'] if r['source_support']==source_support[s['support_id']]);need(len(row['children'])==7 and all(v['kind']=='PropertyStatement' for v in row['children']),'Missing source handle property');expected=True
        need(s['fully_reconciled']==expected,'False support/container closure')""")
start=source.index('    def headers(universe,keys):');end=source.index('    keys=',start)
source=source[:start]+"""    def headers(universe,keys):
        found=set()
        for s in universe['supports']:
            if (s['path'],s['kind'],s['owner'],s['declaration']) not in keys:continue
            if s['kind']=='ModuleBlock':kinds=['ModuleStatement','EndModuleStatement']
            elif s['kind']=='ClassBlock' and s['declaration'] in ['Private Class PcaHandle','Private Class FactorAnalysisHandle']:kinds=['ClassStatement','EndClassStatement']
            elif s['kind']=='PropertyStatement' and s['owner'] in ['BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs.PcaHandle','BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs.FactorAnalysisHandle']:kinds=['PropertyStatement']
            else:continue
            found.update(f['fragment_id'] for f in universe['fragments'] if f['member_id'] is None and f['path']==s['path'] and f['kind'] in kinds and s['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length'])
        return found
"""+source[end:]
source=source.replace("len(individual['rows'])==1252","len(individual['rows'])==716")
source=source.replace("from verify_c9_validation_v1 import inspect as validation","from verify_c10_validation_v1 import inspect as validation")
source=source.replace("'Authenticated C9 proof input changed '","'Authenticated C10 proof input changed '")
with (HERE/'verify_c10_checkpoint_template_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('reader-source-crosswalk-v1.json',dict(before=pin(PREVIOUS/'verify_c9_checkpoint_template_v2.py'),after=pin(HERE/'verify_c10_checkpoint_template_v1.py'),scope='C10 exact source/CIL populations and explicit inherited current-only materializer; no historical IDs reconstructed from positions. Native cache classes require their complete source/CIL field/ctor/accessor/writer contexts. Hashes and semantic predicates remain fail closed; final validation binding follows actual campaigns.',original_queries=0))
print('C10 partial reader template versioned')
