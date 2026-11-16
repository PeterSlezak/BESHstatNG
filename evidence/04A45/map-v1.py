from common45 import *
import csv,re,collections,xml.etree.ElementTree as ET
lock();target=PRIVATE/'syntax-current-v3'
def table(n):return list(csv.DictReader((target/n).open(encoding='utf-8-sig',newline='')))
symbols=table('symbols.csv');states=table('state.csv');edges=table('ui-wiring.csv');types=table('types.csv')
ui=lambda p:p.startswith('BESHstatNG/src/UI/') or p.startswith('BESHstatNG/src/AppInfrastructure/') or p=='BESHstatNG/BESHStatAddIn.vb'
sources={p:pin(p) for p in sorted({r['path'] for r in symbols if ui(r['path'])})}
xml=ET.parse(ROOT/'BESHstatNG/Resources/RibbonXML.xml').getroot();parents={c:p for p in xml.iter() for c in p};controls=[];callbacks=[]
for order,e in enumerate(xml.iter()):
 ancestry=[];parent=parents.get(e)
 while parent is not None:
  if 'id' in parent.attrib:ancestry.append(parent.attrib['id'])
  parent=parents.get(parent)
 if 'id' in e.attrib:controls.append(dict(order=order,tag=e.tag.split('}')[-1],id=e.attrib['id'],ancestors=list(reversed(ancestry)),enabled=e.attrib.get('enabled','default true'),visible=e.attrib.get('visible','default true'),attributes=dict(e.attrib)))
 for attribute,name in e.attrib.items():
  if not (attribute.startswith(('on','get')) or attribute=='loadImage'):continue
  candidates=[r for r in symbols if r['containing_type']=='BESHStatNG.Ribbon' and r['name']==name]
  inherited=attribute=='loadImage' and name=='LoadImage';assert len(candidates)==1 or inherited
  owner=candidates[0] if candidates else None
  body='' if not owner else '\n'.join((ROOT/owner['path']).read_text(encoding='utf-8-sig').splitlines()[int(owner['start_line'])-1:int(owner['end_line'])])
  destinations=re.findall(r'New (Ui\w+)\(',body)
  if 'RegStart(' in body:destinations=['UiGLM']
  callbacks.append(dict(control=e.attrib.get('id','customUI'),order=order,attribute=attribute,callback=name,owner='ExcelDna.Integration.CustomUI.ExcelRibbon' if inherited else owner['containing_type'],signature='Object LoadImage(String)' if inherited else owner['signature'],source=sources.get(owner['path']) if owner else pin('packages/ExcelDna.Integration.1.9.0/lib/net462/ExcelDna.Integration.dll'),member_id=None if inherited else owner['symbol_id'],body_sha256=None if inherited else owner['body_sha256'],destinations=destinations,immediate_host_actions=[x for x in ['Workbooks.Add','ActiveSheet','Populate','QueueAsMacro','WindowHandle','OpenUrl','Show'] if x in body],classification='WINDOWS_HOST_ONLY_NATIVE_PENDING' if inherited else 'FAKE_HOST_EXECUTED',reason='Inherited image/resource/native callback inspected as compiled metadata only' if inherited else 'Original class slice compiled against closed fake types;XML ID dispatch executed;actual native form and COM effects pending',evidence='BESHStatNG.Test/src/WindowsHarness45Tests.vb',next_gate='Exact owner Windows/COM approval;N1/N2 same-session loaded-code proof'))
assert len({r['id'] for r in controls})==len(controls)
save('scope/ribbon-v1.json',dict(source=pin('BESHstatNG/Resources/RibbonXML.xml'),controls=controls,callbacks=callbacks,action_occurrences=sum(r['attribute']=='onAction' for r in callbacks),distinct_actions=len({r['callback'] for r in callbacks if r['attribute']=='onAction'}),getEnabled=sum(r['attribute']=='getEnabled' for r in callbacks),getVisible=sum(r['attribute']=='getVisible' for r in callbacks),dynamic_menu_callbacks=sum(r['attribute'] in ['getContent','getItemCount','getItemID','getItemLabel'] for r in callbacks),unsupported_availability='No availability callbacks are deployed;no feature availability correction authorized. Native runtime availability unproved.',registration='Excel-DNA IExcelAddIn/ComVisible Ribbon;GetCustomUI returns My.Resources.RibbonXML;resource entry and compiled metadata independently checked',source_order_preserved=True,native='NOT_RUN'))
fake_members={r['method'] for r in load(HERE/'fixtures/option-projection-v1.json')['members']}
all_edges=[]
for e in edges:
 if not ui(e['path']):continue
 owner=next((r for r in symbols if r['symbol_id']==e['owner_id']),None)
 fake=e['containing_type']=='BESHStatNG.UibyID' and owner is not None and owner['name'] in fake_members and e['kind']=='Handles'
 unresolved='unresolved' in e.get('target_status','') and e['kind'] not in ['WithEvents-control-or-source','Designer-configuration']
 status='FAKE_HOST_EXECUTED' if fake else 'UNRESOLVED_WIRING' if unresolved else 'STATIC_SOURCE_VERIFIED'
 all_edges.append(dict(edge_id=e['edge_id'],path=e['path'],owner=e['containing_type'],owner_id=e['owner_id'],kind=e['kind'],event_or_control=e['event_or_control'][:500],target_ids=e.get('target_ids',''),target_expression_sha256=sha(e['target'].encode()),start_line=int(e['start_line']),end_line=int(e['end_line']),source_sha256=sources[e['path']]['sha256'],classification=status,reason='Exact Handles compiled against fake WithEvents;tests raise Click' if fake else 'Syntax cannot establish runtime target/overload/instance;retained visibly unresolved' if unresolved else 'Current Roslyn syntax declaration/configuration/candidate edge;no assertion of event execution',evidence='evidence/04A45/fixtures/option-projection-v1.json' if fake else 'evidence/04A45/scope/census-tool-provenance-v3.json',next_gate='Source/compiled binding review or qualified native observation;family46/47 exercise remains separate'))
fields=[]
for s in states:
 if not ui(s['path']):continue
 ref='RefEdit' in s['declared_type'] or 'Refedit' in s['declared_type']
 fields.append(dict(owner=s['containing_type'],name=s['name'],path=s['path'],line=int(s['start_line']),declared_type=s['declared_type'],with_events=s['with_events']=='True',declaration_sha256=sha(s['declaration'].encode()),source_sha256=sources[s['path']]['sha256'],refedit=ref,edges=[e['edge_id'] for e in all_edges if e['owner']==s['containing_type'] and (e['event_or_control'].casefold().startswith(s['name'].casefold()+'.') or e['event_or_control'].casefold()==s['name'].casefold())],classification='WINDOWS_HOST_ONLY_NATIVE_PENDING' if ref else 'STATIC_SOURCE_VERIFIED',reason='Declaration and candidate bindings only;inherited/base controls included by owner',evidence='evidence/04A45/scope/event-map-v1.json',next_gate='Native focus/address/selection/escape/acquisition for RefEdit;family46/47 behavior'))
forms={}
for t in types:
 if t['kind']!='ClassBlock' or not ui(t['path']):continue
 text='\n'.join((ROOT/t['path']).read_text(encoding='utf-8-sig').splitlines()[int(t['start_line'])-1:int(t['end_line'])])
 bases=re.findall(r'^\s*Inherits (.+)$',text,re.M)
 if not bases or not any(re.search(r'\bForm\b|\bUserControl\b',b) for b in bases):continue
 form=forms.setdefault(t['qualified_type'],dict(owner=t['qualified_type'],base=bases[0].strip(),parts=[],classification='STATIC_SOURCE_VERIFIED',reason='Source-backed form/control declaration;constructors not invoked',evidence='evidence/04A45/scope/controls-v1.json',next_gate='Separate46/47 family exercise and exact native approval'))
 form['parts'].append(dict(path=t['path'],start=int(t['start_line']),end=int(t['end_line']),source=sources[t['path']]))
for f in forms.values():
 f['non_designer']=not any(p['path'].endswith('.Designer.vb') for p in f['parts'])
 f['fields']=[s['owner']+'.'+s['name'] for s in fields if s['owner']==f['owner']]
 f['inherited_controls']='System.Windows.Forms base fields/properties via compiled ancestry;no claim every inherited framework event executes'
 f['dynamic_creation_candidates']=[s['symbol_id'] for s in symbols if s['containing_type']==f['owner'] and s['kind'] in ['FieldInitializer','ConstructorBlock']]
save('scope/event-map-v1.json',dict(edges=all_edges,counts=dict(collections.Counter(e['classification'] for e in all_edges)),limits='Lexical targets remain candidates until compiled/event binding is proved. No designer declaration counted as executed. Unresolved wiring is a software review blocker,not success.',native='NOT_RUN'))
save('scope/controls-v1.json',dict(fields=fields,forms=list(forms.values()),refedit_fields=sum(s['refedit'] for s in fields),non_designer_forms=[f['owner'] for f in forms.values() if f['non_designer']],sources=list(sources.values()),binding_limits='Includes all current explicit source fields and all Roslyn event/configuration edges. Runtime creations/property/lambda candidates are indexed separately;actual control trees and resource/runtime defaults unproved.',native='NOT_RUN'))
obligations=load(ROOT/'docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json');assert len(obligations)==1514
register=load(ROOT/'evidence/04A39/register/defects-v7.json');assert len(register['issues'])==1169 and len(register['crosswalk'])==2967
original=[]
for o in obligations:
 ev=o.get('source_evidence',{});p=ev.get('codebehind_path') or ev.get('declaration_path');p=p.replace('\\','/') if p else None
 original.append(dict(obligation_id=o['obligation_id'],original_id=o['original_id'],id_kind=o['id_kind'],record_sha256=sha(json.dumps(o,sort_keys=True,separators=(',',':'),ensure_ascii=False).encode()),source_evidence=ev,classification='WINDOWS_HOST_ONLY_NATIVE_PENDING',reason='Original restricted obligation retained;fake/static evidence never discharges native control',current_source=pin(p) if p and (ROOT/p).is_file() else None,evidence='evidence/04A45/scope/original-obligations-v1.json',next_gate='Owner exact original-control approval and same-session loaded-code/native proof;04A46/47 family scope',native_executed=False,discharged=False))
save('scope/original-obligations-v1.json',dict(authority=pin('docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json'),rows=original,count=1514,restricted_controls=802,discharged=0,native='NOT_RUN'))
save('register/interim-delta-v1.json',dict(kind='04A45_INTERIM_NO_04A48_EXIT',canonical=pin('evidence/04A39/register/defects-v7.json'),canonical_ids=[i['canonical_id'] for i in register['issues']],crosswalk=register['crosswalk'],accepted_deltas=[pin(p) for p in ['evidence/04A40/register/issue-delta-v4.json','evidence/04A41/register/issue-delta-v2.json','evidence/04A42/register/issue-delta-v1.json','evidence/04A43/register/issue-delta-v1.json','evidence/04A44/register/issue-delta-v2.json']],counts=dict(canonical_entries=1169,original_obligations=1514,restricted_controls=802,mappings=2967),original_controls_discharged=0,original_obligations_discharged=0,new_canonical_ids=0,original48_exit_status=None,carry=['C09','C11','C12','04A.13','04A.27','C25-REENTRANT-REQUEST-SNAPSHOT-01','04A.43 session-global caches','04A.44 settings/Help/progress/updater','native chart series/renderers'],historical04A08_scope_FAIL='RETAINED_UNWAIVED;current45 assertion outcome separately recorded',next_owner_decision='Review safe harness and unresolved edges;exact independent Windows/COM packet remains NOT_APPROVED',gates=GATES))
print('Mapped',len(callbacks),'callback occurrences;',len(fields),'fields;',len(forms),'form/control owners;',len(all_edges),'edges;',sum(e['classification']=='UNRESOLVED_WIRING' for e in all_edges),'unresolved;',len(original),'original obligations',flush=True)
