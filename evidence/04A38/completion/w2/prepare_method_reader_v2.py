"""Extend passive IL inventory to constructors; retain v1 tool and outputs."""
from capture_current import *
s=(HERE/'MethodBodies.cs').read_text(encoding='utf-8').replace('static object Read(MethodInfo method)','static object Read(MethodBase method)')
s=s.replace('t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly)',
 't.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).Cast<MethodBase>().Concat(t.GetConstructors(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly))')
with (HERE/'MethodBodies-v2.cs').open('x',encoding='utf-8') as f:f.write(s)
s=(HERE/'read_method_bodies.py').read_text(encoding='utf-8').replace("'completion-w2/method-reader'","'completion-w2/method-reader-v2'").replace("'MethodBodies.cs'","'MethodBodies-v2.cs'").replace("'w2-method-reader-","'w2-method-reader-v2-").replace("'method-bodies-'","'method-bodies-v2-'").replace("'method-reader-receipts.json'","'method-reader-v2-receipts.json'")
with (HERE/'read_method_bodies_v2.py').open('x',encoding='utf-8') as f:f.write(s)
