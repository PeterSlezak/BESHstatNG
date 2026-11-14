from common import *
for name in ['preservation.json','git-entry.json','full-managed-Debug-public-v2.json']:
    data=(HERE/name).read_bytes()
    print(name,len(data),data[:4].hex(), 'UTF8_OK' if not data.startswith((b'\xff\xfe',b'\xfe\xff')) else 'UTF16')
