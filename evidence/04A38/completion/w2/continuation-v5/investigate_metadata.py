from common import *
left = json.loads((PREVIOUS / 'metadata-v6/parametric-v4/original-Debug.json').read_bytes())
right = json.loads((HERE / 'inspection/final/original-Debug.json').read_bytes())
different = [key for key in left if left[key] != right.get(key)]
details = {}
for key in different:
    if key == 'loaded_modules':
        a = {row['file_name']: row for row in left[key]}
        b = {row['file_name']: row for row in right[key]}
        details[key] = dict(removed=[a[k] for k in a.keys() - b.keys()], added=[b[k] for k in b.keys() - a.keys()],
                           changed=[dict(before=a[k], after=b[k]) for k in a.keys() & b.keys() if a[k] != b[k]])
    else:
        details[key] = 'UNRESOLVED'
save('attempts/original-metadata-comparison.json', dict(different_keys=different, details=details,
    production_assemblies_identical=left['assemblies'] == right['assemblies'],
    complete_method_bodies_identical=left['methods'] == right['methods'],
    complete_type_context_identical=left['types'] == right['types']))
print(json.dumps(dict(different_keys=different, details=details)))
