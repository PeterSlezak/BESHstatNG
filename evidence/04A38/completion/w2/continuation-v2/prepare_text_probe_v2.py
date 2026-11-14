from common import *
s=(HERE/'TextDomainProbe.cs').read_text().replace('((object[])doc["calls"])','((System.Collections.IEnumerable)doc["calls"])').replace('(object[])call["choices"]','((System.Collections.IEnumerable)call["choices"]).Cast<object>().ToArray()').replace('((object[])call["choices"]).Cast<string>()','((System.Collections.IEnumerable)call["choices"]).Cast<string>()')
# Keep the exact v1 probe/setup error and use separate files/labels/outputs.
with (HERE/'TextDomainProbe-v2.cs').open('x',encoding='utf-8') as f:f.write(s)
runner=(HERE/'run_text_domains.py').read_text().replace("/'text-domains'","/'text-domains-v2'").replace('TextDomainProbe.cs','TextDomainProbe-v2.cs').replace("+'-text-domain-'","+'-text-domain-v2-'").replace("'text-domains-'+stage+'-'","'text-domains-'+stage+'-v2-'").replace("'text-domain-execution-'+stage+'.json'","'text-domain-execution-'+stage+'-v2.json'")
with (HERE/'run_text_domains_v2.py').open('x',encoding='utf-8') as f:f.write(runner)
save('attempts/text-domain-json-reader-v1.json',dict(exit_code=1,source=pin(HERE/'TextDomainProbe.cs'),error='JavaScriptSerializer returned ArrayList, not Object[]',stage='Before any comparison observation',original_queries=0,correction='Accept IEnumerable transport arrays without changing any input/type/comparison expectation; new probe/command/output version'))
