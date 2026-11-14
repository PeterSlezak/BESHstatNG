from common import *
s=(HERE/'TextDomainProbe-v2.cs').read_text(encoding='utf-8')
s=s.replace('direct_sites=145,direct_source_lines=59,CaseToken_sites=63,CaseToken_choice_occurrences=733','direct_sites=calls.Count(c=>(string)c["kind"]=="EqualText"),direct_source_lines=calls.Where(c=>(string)c["kind"]=="EqualText").Select(c=>(string)c["line_site_id"]).Distinct().Count(),CaseToken_sites=calls.Count(c=>(string)c["kind"]=="CaseToken"),CaseToken_choice_occurrences=calls.Sum(c=>((System.Collections.IEnumerable)c["choices"]).Cast<object>().Count()),values_utf16=ordered.Select(Hex).ToArray()')
with (HERE/'TextDomainProbe-v3.cs').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'run_text_domains_v4.py').read_text(encoding='utf-8').replace('text-domains-v4','text-domains-v5').replace('text-domain-v4','text-domain-v5').replace("'-v4-'","'-v5-'").replace("'-v4.json'","'-v5.json'").replace('TextDomainProbe-v2.cs','TextDomainProbe-v3.cs')
with (HERE/'run_text_domains_v5.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'verify_text_domains.py').read_text(encoding='utf-8').replace('parametric-v4-v4-','parametric-v4-v5-').replace('==3771','==3849').replace('1576278','1608882')
needle="  rr=p['results'];"
assert needle in s
extra="  need(p['values_per_site']==3849 and len(p['values_utf16'])==len(set(p['values_utf16']))==3849,'Current value universe lost/duplicated')\n  need(p['direct_sites']==145 and p['direct_source_lines']==59 and p['CaseToken_sites']==64 and p['CaseToken_choice_occurrences']==750 and p['total_site_value_observations']==1608882,'Stale/forged top-level counters')\n"
s=s.replace(needle,extra+needle)
with (HERE/'verify_text_domains_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('attempts/text-reader-v1.json',dict(status='FAILED_INHERITED_VALUE_COUNT_ASSUMPTION',reason='Added17 MCP aliases expand generated current-only value set3771to3849; actual418 result rows all had0differences. Old observer counters63/733 were stale constants despite correct per-site iterations.',correction='V3 probe recomputes top counters from AST and publishes all3849 unique UTF16 values. ReaderV2 requires actual145/59/64/750,3849values,418culture/site keys and1608882observations; source policy unchanged.',original_queries=0))
