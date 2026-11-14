"""Version C6 offline tools for C7; retain originals, no execution here."""
from common import *
copies={
 'build_v1.py':('build_v2.py',[('implementation-plan.json','implementation-plan-v2.json')]),
 'focused_v1.py':('focused.py',[("[('Common',", "[('Core','BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj',False,'FullyQualifiedName~UdfNumericPreparation38Tests'),('Common',"),('len(results)==4','len(results)==6')]),
 'inspect_stage_v1.py':('inspect_stage.py',[('implementation-plan.json','implementation-plan-v2.json')]),
 'compatibility_v1.py':('compatibility.py',[]),
 'source_views.py':('source_views.py',[]),
}
rows=[]
for name,(old,changes) in copies.items():
    source=(PREVIOUS/old).read_text(encoding='utf-8-sig')
    for before,after in changes:
        assert before in source,(old,before)
        source=source.replace(before,after)
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
    rows.append(dict(prior=pin(PREVIOUS/old),current=pin(HERE/name),changes=changes))
metadata=(PREVIOUS/'MethodMetadata-v9.cs').read_text(encoding='utf-8-sig')
needle='name=="BESHStat.Core"?('
assert metadata.count(needle)==1
metadata=metadata.replace(needle,'name=="BESHStat.Core"?(true||')
with (HERE/'MethodMetadata-v10.cs').open('x',encoding='utf-8',newline='\n') as stream:stream.write(metadata)
with (HERE/'native-metadata-owners-v1.txt').open('x',encoding='utf-8',newline='\n') as stream:stream.write((PREVIOUS/'native-metadata-owners-full.txt').read_text(encoding='utf-8-sig'))
save('versioned-tool-crosswalk-v1.json',dict(rows=rows,metadata=dict(prior=pin(PREVIOUS/'MethodMetadata-v9.cs'),current=pin(HERE/'MethodMetadata-v10.cs'),scope='Passive complete Core transitive engine/type/field proof; original assemblies read as metadata only; no original calls or static constructors'),original_queries=0))
