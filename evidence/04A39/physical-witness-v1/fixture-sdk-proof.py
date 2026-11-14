"""Recheck exact25 keys and the original SDK/HEAD exception; never extend allowances."""
from witness_lib import *
from physical import *
import collections,re,zipfile

verify_frozen();physical=load(HERE/'scanner-physical-results.json');entry=load(HERE/'entry.json')
main=ROOT/entry['input_archives'][0]['path']
source_pins={r['path']:r for r in load(OLD/'entry/source-lock.json')['files']}
with zipfile.ZipFile(main) as z:
    # Only exact existingSDK literal equality in live git show HEAD can release these two generic matches.
    path='BESHstatNG/BESHStatNG.vbproj';head=git('show','HEAD:'+path);headtext=head.decode('utf-8-sig');data=z.read(path);value=unicodedata.normalize('NFKC',data.decode('utf-8-sig'))
    inspected=value.replace('\\\\','\\');sdk=[]
    pattern=r'(?i)\b[a-z]:[/\\]+[A-Za-z0-9_]'
    sdk_pattern=r'[Cc]:\\Program Files \(x86\)\\Windows Kits\\10\\bin\\[0-9.]+\\x64\\signtool\.exe'
    for match in re.finditer(pattern,inspected):
        full=re.match(sdk_pattern,inspected[match.start():],re.I)
        assert full and full[0] in headtext,'SDK_LITERAL_NOT_PROVEN_IN_HEAD'
        line=inspected.count('\n',0,match.start())+1
        headlines=[i+1 for i,text in enumerate(headtext.splitlines()) if full[0] in text]
        assert line in headlines
        sdk.append(dict(path=path,line=line,offset=match.start(),category='appdata_or_drive_path',origin='content',
            generic_match_sha256=sha(match.group().replace('\\\\','\\').casefold().encode()),full_SDK_literal_sha256=sha(full[0].encode()),
            matching_HEAD_lines=headlines,reason='ORIGINAL_CONDITIONAL_INHERITED_PUBLIC_WINDOWS_SDK_TOOL_PATH',new_allowance=False))
    assert len(sdk)==2 and {r['line'] for r in sdk}=={603,606}
    controls=[]
    for phase in ('13','14'):
        m=scanner(phase);m.ROOT=ZipRoot(z,'utf-8-sig');normal=m.scan([path]);assert not normal[0] and normal[1]==[(path,'inherited_public_windows_sdk_tool_path')]
        original_git=m.git
        # Adverse-only absence of the exact SDK literal: same scanner must emit both findings.
        m.git=lambda *args:''
        generic=m.scan([path]);m.git=original_git
        assert len(generic[0])==2 and {r['line'] for r in generic[0]}=={603,606} and not generic[1]
        controls.append(dict(scanner=phase,live_HEAD_findings=0,SDK_reviewed_groups=1,missing_HEAD_literal_findings=2,wildcard_not_used=True))
    dest=PRIVATE/'HEAD-source/BESHStatNG.vbproj';dest.parent.mkdir(parents=True,exist_ok=True)
    with dest.open('xb') as f:f.write(head)
    save('sdk-HEAD-proof.json',dict(HEAD=MAIN,command='git -c safe.directory=<LIVE_REPO> --no-optional-locks show HEAD:BESHstatNG/BESHStatNG.vbproj',
        HEAD_source=dict(bytes=len(head),sha256=sha(head)),HEAD_snapshot=pin(dest),physical_main_source=dict(bytes=len(data),sha256=sha(data)),
        occurrences=sdk,controls=controls,new_synthetic_allowances=0,independent_generic_main_matches=27,original_scanner_main_findings=25,
        public_snapshot_transport='HEAD source is stored at canonical BESHstatNG/BESHStatNG.vbproj in the small final review ZIP, so the original SDK condition can be inspected against authenticated bytes. Metadata stores hashes rather than duplicating drive-path strings.'))
    # Step5 starts only after the step4 SDK/HEAD proof has been sealed.
    index=json.loads(z.read('.review-inventory.json'))
    allowed=index['source_dispositions'];assert len(allowed)==25
    assert allowed==load(ROOT/'.test-rationalization-04A39-correction-v1.tmp/publication-selection-v2-with-exclusions.json')['source_dispositions']
    def key(row,hash):return (row['path'],hash,row['origin'],row.get('line'),row['category'],row['offset'],row['match_sha256'],row['encoding_view'])
    expected={tuple(r['key']):r for r in allowed};assert len(expected)==25
    actual=[];public=[]
    for phase in ('13','14'):
        m=scanner(phase);details=[]
        for name,codec in sorted({(r['key'][0],r['key'][7]) for r in allowed}):
            m.ROOT=ZipRoot(z,codec);f,r,d=detailed(m,[name]);details.extend(dict(row,encoding_view=codec) for row in d)
        keys=[key(row,sha(z.read(row['path']))) for row in details]
        assert len(keys)==len(set(keys))==25 and set(keys)==set(expected),'EXACT_FIXTURE_KEYS_CHANGED'
        direct=next(r['findings'] for r in physical['reports'] if r['scanner_phase']==phase and r['physical_members']==2060)
        projection=[{k:row[k] for k in ('path','origin','line','category','encoding_view')} for row in details]
        assert collections.Counter(json.dumps(r,sort_keys=True) for r in projection)==collections.Counter(json.dumps(r,sort_keys=True) for r in direct)
        for row,k in zip(details,keys):
            prior=expected[k];source=source_pins[row['path']]
            assert pin(row['path'])==source==prior['source'] and source['sha256']==k[1]
            assert prior['disposition']=='REVIEWED_PUBLIC_OR_SYNTHETIC_SOURCE_LITERAL' and prior['proof']
            assert prior['evidence']['source']==source
            actual.append(dict(scanner=phase,key=k,raw_match=row['raw_match']))
            if phase=='13':public.append(dict(key=k,source=source,proof=prior['proof'],original_justification=prior['evidence'],original_justification_sha256=sha(json.dumps(prior,sort_keys=True).encode()),
                original_line=row['line'],physical_line=row['line'],raw_match_sha256=sha(row['raw_match'].encode()),status='EXACT_ALREADY_JUSTIFIED_NEGATIVE_TEST_FIXTURE'))
save(PRIVATE/'exact-fixture-matches-raw.json',dict(matches=actual))
save('exact-25-fixtures.json',dict(status='EXACT_25_KEYS_REVERIFIED_WITH_BOTH_SCANNERS',fixtures=public,count=25,
    observed_key_checks=50,missing=[],changed=[],additional=[],new_synthetic_entries=0,source_and_physical_hashes_exact=True,
    private_match_values=pin(PRIVATE/'exact-fixture-matches-raw.json')))
print('Exact25 fixture keys PASS for both scanners; original SDK exception proven at HEAD lines603/606; missing-HEAD controls expose both matches',flush=True)
