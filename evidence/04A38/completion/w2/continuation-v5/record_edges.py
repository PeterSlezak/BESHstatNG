from common import *
contracts = json.loads((HERE / 'body-contracts.json').read_bytes())['contracts']
records = []
for c in ['Debug','Release']:
    original = json.loads((HERE / 'dependency-metadata-v7' / ('original-'+c+'.json')).read_bytes())
    current = json.loads((HERE / 'dependency-metadata-v7' / ('current-'+c+'.json')).read_bytes())
    rows = []
    for r in contracts:
        a = [m for m in original['methods'] if m['declaring_type']==r['owner'] and m['member_name']==r['name']]
        b = [m for m in current['methods'] if m['declaring_type']==r['owner'] and m['member_name']==r['name']]
        if r['name']=='PrepareResultTableForUdf':
            a = [m for m in a if m['parameter_types']==['BESHStatNG.ResultTable']]
            b = [m for m in b if m['parameter_types']==['BESHStatNG.ResultTable']]
        assert len(a)==len(b)==1,(r['name'],len(a),len(b))
        rows.append(dict(original_id=r['original_id'], original=a[0],current=b[0],
            call_edges=[i for i in b[0]['instructions'] if i['op'] in ['call','callvirt','ldftn','newobj']],
            whole_source_contract='body-contracts.json', closure='Complete source and context, not merely a portable edge'))
    edges=[]
    for m in current['methods']:
        if m['declaring_type'].startswith(('BESHStatNG.ExcelCommon.','BESHStatNG.WorksheetCompatibility.')):continue
        for i,instruction in enumerate(m['instructions']):
            if instruction['op'] in ['call','callvirt','ldftn','newobj'] and any(s in str(instruction['operand']) for s in ['BESHStatNG.ExcelCommon.','BESHStatNG.WorksheetCompatibility.']):
                edges.append(dict(caller=m['method'],instruction=i,opcode=instruction['op'],target=instruction['operand']))
    records.append(dict(configuration=c,reviewed_bodies=rows,actual_portable_edges=edges,
        callback_bodies=[m for m in current['methods'] if '_Lambda$' in m['member_name'] and m['declaring_type'].startswith('BESHStatNG.UdfDataImport')],
        original_assemblies=original['assemblies'],current_assemblies=current['assemblies']))
save('production-consumer-edges.json',dict(configurations=records,original_calls=0,original_compilations=0))
save('entry-discrepancies.json',dict(resolved=[
    dict(subject='Entry branch/inline labels',reported='Original70/3678 branches;19/1093 inline',
         observed=json.loads((PREVIOUS/'fragment-coverage.json').read_bytes()),
         resolution='Reported fractions are the v4 CURRENT syntax universe. Immutable original crosswalk separately contains941/5511 covered branches and136/1397 covered inline. Neither universe relabeled or rewritten.'),
    dict(subject='v4 validation text-domain string',reported='1568736 checks/config in validation.json',
         resolution='Stale descriptive string only; v4 owner report and actual final v5 observer record1608882 checks/config,3849 values,418 observations. Exact v4 bytes retained. No global EqualText/CaseToken policy change.'),
    dict(subject='Long v4 publication entry reader',resolution='3777 payload hashes and individual semantic readers passed before C5 edits; late combined reader reached live source after edits. Failed receipt retained; exact authenticated historical-view replay passed.'),
    dict(subject='Reader executable MVID',resolution='Only newly compiled metadata-reader executable identity differed; original DLLs, complete methods/types and other loaded modules match exactly; initial rejection retained.')],
    unresolved_mandatory_entry_failures=[]))
