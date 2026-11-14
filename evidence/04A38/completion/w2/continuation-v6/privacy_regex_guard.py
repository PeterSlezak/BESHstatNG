"""Equivalent necessary-condition guard for the historical email regex.

For E = local @ domain, every match necessarily contains a match of @ domain.
If that suffix has no match, E.finditer is exactly empty. Otherwise use E itself
and return its original Match objects, positions and order. All other patterns
and every historical assertion remain untouched. No filtering/waiver is used.
"""
import re
EMAIL=r'\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b'
SUFFIX=re.compile(r'@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b')
class RegexProxy:
 def __init__(self):self.skips=0;self.fallbacks=0
 def __getattr__(self,name):return getattr(re,name)
 def finditer(self,pattern,string,flags=0):
  if pattern==EMAIL and not flags:
   if not SUFFIX.search(string):self.skips+=1;return iter(())
   self.fallbacks+=1
  return re.finditer(pattern,string,flags)
