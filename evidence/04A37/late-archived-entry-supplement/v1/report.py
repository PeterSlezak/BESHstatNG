"""Derived review indexes. Expected outcomes come only from sealed reference rows.
Run after captures; these indexes do not claim an earlier construction date.
"""
from collections import Counter, defaultdict
import csv
import json
import verify_supplement as reader

HERE=reader.HERE


def save(name,value):
    (HERE/name).write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n',encoding='utf-8',newline='\n')


def main():
    result,cases,rows=reader.verify()
    save('compatibility-and-coverage.json',result)
    dimensions=['cohort','family','operation','culture','components','scale','eigen','factors','trace','initial_eigen','matrices','multiple','structures','axis','row_side','row_ids','row_labels','groups']
    cells=defaultdict(Counter)
    with (HERE/'reference-dispatch-expectations.csv').open('w',encoding='utf-8',newline='') as f:
        writer=csv.DictWriter(f,fieldnames=['case_id','expected_dispatch','expected_source_phase','expected_prefix_completed','input_sha256','expected_result_sha256','expected_error','expectation_source'])
        writer.writeheader()
        for i,(c,r) in enumerate(zip(cases,rows)):
            writer.writerow(dict(case_id=c['case_id'],expected_dispatch=r['dispatch'],expected_source_phase=r['phase'],expected_prefix_completed=r['prefix_completed'],input_sha256=r['input_sha256'],expected_result_sha256=reader.digest(r['result'].encode()),expected_error=r['error'],expectation_source=('label-extension-v1/' if i>=18466 else '')+'original-Debug.tsv'))
            key=tuple(str(c.get(k,'')) for k in dimensions);cells[key][r['dispatch']]+=1
            cells[key]['finite_numeric_prefix_geometry']+=r['dispatch']=='GEOMETRY' and r['usable_numeric_prefix']=='true'
    fields=dimensions+['GUARD','GEOMETRY','EXCEPTION','finite_numeric_prefix_geometry','total']
    with (HERE/'coverage-cells.csv').open('w',encoding='utf-8',newline='') as f:
        writer=csv.DictWriter(f,fieldnames=fields);writer.writeheader()
        for key,counts in sorted(cells.items()):
            writer.writerow(dict(zip(dimensions,key))|dict((k,counts[k]) for k in fields[len(dimensions):-1])|dict(total=sum(counts[k] for k in ['GUARD','GEOMETRY','EXCEPTION'])))
    save('family-accounting.json',dict(families={family:dict(rows=sum(c['family']==family for c in cases),outcomes=dict(Counter(r['dispatch'] for c,r in zip(cases,rows) if c['family']==family))) for family in ['PCA','FACTOR','CA']},
         extension='3584 label cases supplement main18466; original6912/config retained separately',
         old_corpus=dict(rows_per_final_configuration=6912,plot_prefix_rows=5376,dendrogram_series_rows=1536,dendrogram_reference_execution='Original Debug only; unchanged bytes compared in both final configurations'),
         combined_rows_per_configuration=6912+len(cases),comparison='EXACT bytes; no numerical tolerance, normalization, exception filtering or expected-value refresh',
         geometry='Final pure source-expression/argument boundary only. Includes nonfinite prefixes and caught legacy errors; NOT Excel renderability.',
         finite_numeric_prefix_geometry='Conservative recursive finiteness check of first returned numeric prefix only; not a scientific/native success assertion.',
         expectation_index='Derived now from sealed original rows. Original row dispatch/source phase were sealed before new current comparisons; this derived index is not a new pre-edit freeze.',
         counts_by_configuration='Both configurations identical, each counted once; combined/config is not unique statistical models or native charts.',
         nonpasses='8212 expected original/current matching exceptions per configuration, plus guards; see exact per-case errors. Two initial new audit coding errors retained under attempts/audit-v1-failure.json.',
         native='NOT_RUN',scientific='NOT_GOLDEN'))
    print('Derived',len(cases),'reference expectation rows and',len(cells),'explicit coverage cells')


if __name__=='__main__':main()
