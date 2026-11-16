from common47 import *
lock();current=load(HERE/'build-and-tests/Release-identities-v1.json');paths=['evidence/04A46/build-and-tests/Release-identities-v1.json','evidence/04A41/build-and-tests/Release-identities-v1.json','evidence/04A38/completion/w3/build-and-tests/Release-identities-v1.json'];rows=[]
reports=[(p,load(ROOT/p)) for p in paths]
for r in current['nonpasses']:
 candidates=[(p,b) for p,d in reports for b in d['nonpasses'] if all(b[k]==r[k] for k in ['suite','identity_sha256','occurrence'])]
 assert candidates and all(b['message_sha256']==r['message_sha256'] for _,b in candidates)
 exacts=[(p,b) for p,b in candidates if b['traceback_sha256']==r['traceback_sha256']]
 rows.append(dict(identity_sha256=r['identity_sha256'],suite=r['suite'],method=r['method'],message_sha256=r['message_sha256'],traceback_sha256=r['traceback_sha256'],message_exact=True,optimized_trace_exact=bool(exacts),matching_historical_reports=[pin(p) for p,_ in exacts],classification='INHERITED_RELEASE_FAIL_EXACT_MESSAGE_AND_OPTIMIZED_TRACE' if exacts else 'INHERITED_MESSAGE_RELEASE_TRACE_DRIFT_REQUIRES_REVIEW',waived=False))
save('build-and-tests/Release-trace-disposition-v2.json',dict(status='INHERITED_RELEASE_FAILURES_RETAINED_UNWAIVED',nonpasses=rows,failures=len(rows),all_messages_exact=True,all_optimized_traces_exact=all(r['optimized_trace_exact'] for r in rows),new_test_nonpasses=0,new47_tests_passed=len(current['added']),full_Release='NOT_RUN',waived=False,scope='Broader affected selection than46;not a full Release run. All14 Debug failures remain independently preserved. Historical full W3 Release identities consulted without re-execution or promotion.',gates=GATES))
print('Release',len(rows),'inherited failures;',sum(r['optimized_trace_exact'] for r in rows),'exact historical optimized traces;141 new PASS',flush=True)
