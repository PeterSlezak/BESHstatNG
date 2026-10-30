"""Negative reconciliation fixtures modify copies only, never production source."""
import csv
import pathlib
import shutil
import tempfile
import unittest
import catalog
import copy
import json
import subprocess
import semantic
import continuation03_report
import hashlib
import continuation04_report
import continuation05_report
import continuation06_report
import continuation07_report
import continuation08_report
import continuation09_report
import continuation10_report
import continuation11_report
import continuation12_report
import continuation13_report


class ReconciliationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.temporary = tempfile.TemporaryDirectory(prefix="beshstat-04A01-")
        cls.ledger = pathlib.Path(cls.temporary.name)
        cls.required = ("files.csv", "symbols.csv", "ui-wiring.csv", "udf-exports.csv", "coverage.json",
                        "common-registry.csv", "ribbon-crosswalk.csv", "catalog-crosswalk.csv", "callback-index.csv",
                        "xml-callbacks.csv", "catalog-coverage.json", "external-callback-evidence.json", "state.csv",
                        "ui-controls.csv", "investigations.csv", "review-by-file.csv", "physical-source-census.csv", "review-coverage.json",
                        "semantic-review-coverage.json", "remaining-member-review.csv")
        for name in cls.required:
            shutil.copyfile(catalog.DEFAULT / name, cls.ledger / name)

    @classmethod
    def tearDownClass(cls):
        cls.temporary.cleanup()

    def test_unchanged_ledger_passes(self):
        self.assertEqual([], catalog.verify(self.ledger))

    def missing(self, name, expected):
        original = (self.ledger / name).read_bytes()
        rows = catalog.load(self.ledger, name)
        self.assertGreater(len(rows), 1)
        try:
            (self.ledger / name).write_text(catalog.render(rows[1:]), encoding="utf-8", newline="")
            self.assertTrue(any(expected in e for e in catalog.verify(self.ledger)))
        finally:
            (self.ledger / name).write_bytes(original)

    def test_missing_symbol_fails(self):
        self.missing("symbols.csv", "symbol IDs")

    def test_missing_event_fails(self):
        self.missing("ui-wiring.csv", "event or Designer/control edge")

    def test_missing_static_ui_handler_fails(self):
        name = "ui-wiring.csv"
        original = (self.ledger / name).read_bytes()
        rows = catalog.load(self.ledger, name)
        victim = next(r for r in rows if r["kind"] == "Handles" and r["event_or_control"] == "btCompute.Click")
        try:
            (self.ledger / name).write_text(catalog.render([r for r in rows if r["edge_id"] != victim["edge_id"]]), encoding="utf-8", newline="")
            self.assertTrue(any("event or Designer/control edge" in e for e in catalog.verify(self.ledger)))
        finally:
            (self.ledger / name).write_bytes(original)

    def test_missing_export_fails(self):
        self.missing("udf-exports.csv", "UDF export")

    def test_missing_file_fails(self):
        self.missing("files.csv", "compiled file ledger")

    def test_missing_ribbon_fails(self):
        self.missing("ribbon-crosswalk.csv", "catalog derived data drift")

    def test_missing_owner_fails(self):
        name = "symbols.csv"
        original = (self.ledger / name).read_bytes()
        rows = catalog.load(self.ledger, name)
        try:
            rows[0]["owner_role"] = ""
            (self.ledger / name).write_text(catalog.render(rows), encoding="utf-8", newline="")
            self.assertTrue(any("unclassified/incomplete" in e for e in catalog.verify(self.ledger)))
        finally:
            (self.ledger / name).write_bytes(original)


class SemanticReviewTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.symbols = catalog.load(catalog.DEFAULT, "symbols.csv")
        cls.edges = catalog.load(catalog.DEFAULT, "ui-wiring.csv")
        cls.controls = catalog.load(catalog.DEFAULT, "ui-controls.csv")
        cls.decisions = semantic.read_store("review-decisions.json")
        cls.wiring = semantic.read_store("wiring-review-decisions.json")
        cls.intents = semantic.read_store("control-review-decisions.json")

    def verify(self, symbols=None, edges=None, decisions=None, wiring=None, intents=None):
        return semantic.verify(self.symbols if symbols is None else symbols, self.edges if edges is None else edges, self.controls,
                               self.decisions if decisions is None else decisions, self.wiring if wiring is None else wiring,
                               self.intents if intents is None else intents)

    def test_pinned_source_decisions_pass(self):
        self.assertEqual([], self.verify())

    def test_removed_ownership_pin_fails(self):
        self.assertTrue(any("unbacked pinned ownership" in e for e in self.verify(decisions=self.decisions[1:])))

    def test_changed_body_pin_fails(self):
        changed = copy.deepcopy(self.decisions)
        changed[0]["body_sha256"] = "0" * 64
        self.assertTrue(any("stale/ambiguous pinned ownership" in e for e in self.verify(decisions=changed)))

    def continuation_decisions(self):
        changed = copy.deepcopy(self.decisions)
        victim = next(d for d in changed if d.get("review_revision") == "04A.01-continuation-02")
        return changed, victim

    def dm_decisions(self):
        changed=copy.deepcopy(self.decisions)
        victim=next(d for d in changed if d.get('review_revision')=='04A.01-continuation-03' and
                    json.loads(d['source_identity'])['path'].endswith('ExcelDnaSharedOutputPlanAdapter.vb') and
                    json.loads(d['source_identity'])['start_line']=='41')
        return changed,victim

    def test_removed_new_dm_review_fails(self):
        changed,victim=self.dm_decisions()
        self.assertTrue(any('unbacked pinned ownership' in e for e in self.verify(decisions=[d for d in changed if d is not victim])))

    def test_removed_new_dm_symbol_blocks_bounded_scope(self):
        _,victim=self.dm_decisions()
        symbols=[s for s in self.symbols if s['symbol_id']!=victim['symbol_id']]
        self.assertTrue(any('bounded DM source denominator' in e for e in continuation03_report.scope_errors(symbols,self.decisions)))

    def test_dm_corrupt_dependency_manifest_fails_after_matching_ledger_edit(self):
        changed,victim=self.dm_decisions()
        victim['required_dependency_paths']='[]'
        symbols=[dict(s,**victim) if s['symbol_id']==victim['symbol_id'] else s for s in self.symbols]
        self.assertTrue(any('required dependency' in e for e in self.verify(symbols=symbols,decisions=changed)))

    def test_false_whole_dm_equivalence_rejected(self):
        changed,victim=self.dm_decisions()
        self.assertEqual('helper-only',victim['equivalence_scope'])
        victim['equivalence_scope']='whole-method'
        symbols=[dict(s,**victim) if s['symbol_id']==victim['symbol_id'] else s for s in self.symbols]
        self.assertTrue(any('unsupported whole/helper equivalence' in e for e in self.verify(symbols=symbols,decisions=changed)))

    def test_analyzer_false_whole_dm_equivalence_is_integrity_failure(self):
        changed,victim=self.dm_decisions()
        victim['equivalence_scope']='whole-method'
        result=self.analyzer(changed)
        self.assertEqual(1,result.returncode,result.stdout+result.stderr)
        self.assertIn('Unsupported whole/helper equivalence assertion',result.stderr)

    def test_stale_current_checkpoint_hash_and_dropped_index_entry_fail(self):
        with tempfile.TemporaryDirectory(prefix='beshstat-checkpoint-negative-') as directory:
            root=pathlib.Path(directory)
            relative='docs/architecture/WindowsMigrationLedger/continuation03.md'
            checkpoint=root/relative
            checkpoint.parent.mkdir(parents=True)
            checkpoint.write_bytes(b'bounded DM source checkpoint\n')
            index={'artifacts':[{'path':relative,'bytes':checkpoint.stat().st_size,'sha256':hashlib.sha256(checkpoint.read_bytes()).hexdigest()}]}
            proposed=[relative,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation03_report.verify_index(root,index,proposed))
            # Same size: this must exercise SHA drift, not merely the size guard.
            checkpoint.write_bytes(b'B'+checkpoint.read_bytes()[1:])
            self.assertEqual(index['artifacts'][0]['bytes'],checkpoint.stat().st_size)
            self.assertTrue(any('stale current checkpoint artifact hash' in e for e in continuation03_report.verify_index(root,index,proposed)))
            self.assertTrue(any('unindexed/missing current audit paths' in e for e in continuation03_report.verify_index(root,{'artifacts':[]},[proposed[-1]])))

    def test_removed_body_dependency_fails_even_after_matching_ledger_edit(self):
        changed, victim = self.continuation_decisions()
        pins = json.loads(victim["dependency_pins"])
        del pins[next(p for p in pins if p != json.loads(victim["source_identity"])["path"])]
        victim["dependency_pins"] = json.dumps(pins, sort_keys=True)
        symbols = [dict(s, **victim) if s["symbol_id"] == victim["symbol_id"] else s for s in self.symbols]
        self.assertTrue(any("required dependency" in e for e in self.verify(symbols=symbols, decisions=changed)))

    def test_removed_continuation_pin_fails(self):
        changed, victim = self.continuation_decisions()
        changed = [d for d in changed if d is not victim]
        self.assertTrue(any("unbacked pinned ownership" in e for e in self.verify(decisions=changed)))

    def test_changed_dependency_hash_fails_even_after_matching_ledger_edit(self):
        changed, victim = self.continuation_decisions()
        pins = json.loads(victim["dependency_pins"])
        dependency = next(p for p in pins if p != json.loads(victim["source_identity"])["path"])
        pins[dependency] = "0" * 64
        victim["dependency_pins"] = json.dumps(pins, sort_keys=True)
        symbols = [dict(s, **victim) if s["symbol_id"] == victim["symbol_id"] else s for s in self.symbols]
        self.assertTrue(any("member stale dependency: " + dependency in e for e in self.verify(symbols=symbols, decisions=changed)))

    def test_removed_dependency_manifest_fails(self):
        changed, victim = self.continuation_decisions()
        del victim["required_dependency_paths"]
        self.assertTrue(any("required dependency" in e for e in self.verify(decisions=changed)))

    def test_changed_exact_source_identity_fails(self):
        changed, victim = self.continuation_decisions()
        identity = json.loads(victim["source_identity"])
        identity["containing_type"] = "invented"
        victim["source_identity"] = json.dumps(identity, sort_keys=True)
        self.assertTrue(any("source identity" in e for e in self.verify(decisions=changed)))

    def test_removed_detailed_field_fails(self):
        changed, victim = self.continuation_decisions()
        del victim["source_purpose"]
        self.assertTrue(any("incomplete semantic ownership" in e for e in self.verify(decisions=changed)))

    def test_complete_control_contract_removal_fails(self):
        intents = copy.deepcopy(self.intents)
        victim = intents[0]
        fields = {"review_contract":"04A.01-control-v1", "defaults_contract":"fixture designer literal",
                  "enabled_reset_contract":"fixture state transition", "validation_output_contract":"fixture help only",
                  "option_contract":"fixture no analysis option"}
        victim.update(fields)
        controls = copy.deepcopy(self.controls)
        control = next(c for c in controls if c["control_id"] == victim["control_id"])
        control.update(fields)
        # This synthetic fixture exercises contract persistence only, never source review coverage.
        self.assertEqual([], semantic.verify(self.symbols, self.edges, controls, self.decisions, self.wiring, intents))
        incomplete = copy.deepcopy(intents)
        del incomplete[0]["defaults_contract"]
        self.assertTrue(any("incomplete full control" in e for e in semantic.verify(self.symbols, self.edges, controls, self.decisions, self.wiring, incomplete)))
        self.assertTrue(any("unbacked full control" in e for e in semantic.verify(self.symbols, self.edges, controls, self.decisions, self.wiring, intents[1:])))

    def test_substantive_member_generic_triage_blocks_semantic_coverage(self):
        row = copy.deepcopy(next(s for s in self.symbols if s.get("review_contract") == "04A.01-member-v1" and s["family"] == "ExcelUDFs"))
        self.assertTrue(semantic.qualified(row))
        row["review_status"] = "static; semantic owner review pending"
        row["review_contract"] = "unreviewed"
        row["disposition"] = "retain-Windows-host-only"
        self.assertFalse(semantic.qualified(row), "A schema-valid host enum must not satisfy review coverage")
        symbols = [row if s["symbol_id"] == row["symbol_id"] else s for s in self.symbols]
        self.assertTrue(any("pinned ownership decision drift" in e for e in self.verify(symbols=symbols)))

    def test_documented_dynamic_target_reverting_to_unknown_fails(self):
        edges = copy.deepcopy(self.edges)
        row = next(e for e in edges if e.get("review_status") == "manually-reviewed-static-edge" and e["kind"] == "AddHandler")
        row["target_status"] = "unresolved-dynamic-or-external"
        row["target_ids"] = ""
        self.assertTrue(any("pinned wiring decision drift" in e for e in self.verify(edges=edges)))

    def test_removed_dynamic_pin_fails(self):
        self.assertTrue(any("unbacked wiring review" in e for e in self.verify(wiring=self.wiring[1:])))

    def test_changed_control_source_pin_fails(self):
        changed = copy.deepcopy(self.intents)
        changed[0]["declaration_line"] = "0"
        self.assertTrue(any("stale control intent" in e for e in self.verify(intents=changed)))

    def test_control_and_wiring_denominators_reconcile(self):
        summary = semantic.summary(self.symbols, self.edges, self.controls, catalog.load(catalog.DEFAULT, "files.csv"))
        explanation = summary["denominator_explanation"]
        self.assertEqual(2056, explanation["ui_controls"])
        self.assertEqual(2059, explanation["all_WithEvents_edges"])
        self.assertEqual(37, len(explanation["no_direct_Handles_but_prefix_matched_other_edge"]))
        self.assertEqual(3, len(explanation["non_ui_WithEvents_edges"]))
        outside = [e for e in self.edges if e["edge_id"] in explanation["non_ui_WithEvents_edges"]]
        self.assertTrue(all(e["target_status"].startswith("has-static-Handles") for e in outside))

    def test_routing_pin_does_not_complete_option_contract(self):
        self.assertTrue(self.intents)
        routing_only = [c for c in self.intents if c.get("review_contract") != "04A.01-control-v1"]
        self.assertTrue(routing_only)
        self.assertFalse(any(semantic.control_qualified(c) for c in routing_only), "Routing-only notes cannot satisfy full control acceptance")
        fixture = {"review_contract": "04A.01-control-v1", "defaults_contract": "initial Enabled=True",
                   "enabled_reset_contract": "no reset mutation", "validation_output_contract": "Help navigation; no analysis input/output",
                   "option_contract": "no options; source-traced Click only"}
        self.assertTrue(semantic.control_qualified(fixture))
        fixture["defaults_contract"] = "unreviewed"
        self.assertFalse(semantic.control_qualified(fixture))

    def analyzer(self, decisions):
        with tempfile.TemporaryDirectory(prefix="beshstat-review-negative-") as directory:
            path = pathlib.Path(directory) / "review.json"
            path.write_text(json.dumps(decisions), encoding="utf-8")
            result = subprocess.run(["dotnet", str(catalog.ROOT / "scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll"),
                                     "--check", "--strict", "--reviews=" + str(path)], cwd=catalog.ROOT, capture_output=True, text=True)
            return result

    def test_analyzer_missing_pin_is_integrity_failure_not_expected_blocker_exit(self):
        result = self.analyzer(self.decisions[1:])
        self.assertEqual(1, result.returncode, result.stdout + result.stderr)
        self.assertIn("source-derived data differs", result.stdout)

    def test_analyzer_stale_body_pin_fails_before_generation(self):
        changed = copy.deepcopy(self.decisions)
        changed[0]["body_sha256"] = "0" * 64
        result = self.analyzer(changed)
        self.assertEqual(1, result.returncode, result.stdout + result.stderr)
        self.assertIn("Stale or ambiguous review decision", result.stderr)

    def test_analyzer_incomplete_claimed_review_fails(self):
        changed = copy.deepcopy(self.decisions)
        victim = next(d for d in changed if d.get("review_contract") == "04A.01-member-v1")
        victim["source_purpose"] = ""
        result = self.analyzer(changed)
        self.assertEqual(1, result.returncode, result.stdout + result.stderr)
        self.assertIn("Incomplete semantic review", result.stderr)

    def test_analyzer_removed_dependency_fails_before_generation(self):
        changed, victim = self.continuation_decisions()
        pins = json.loads(victim["dependency_pins"])
        del pins[next(p for p in pins if p != json.loads(victim["source_identity"])["path"])]
        victim["dependency_pins"] = json.dumps(pins, sort_keys=True)
        result = self.analyzer(changed)
        self.assertEqual(1, result.returncode, result.stdout + result.stderr)
        self.assertIn("Incomplete required dependency", result.stderr)


class Continuation04Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv')
        cls.decisions=semantic.read_store('review-decisions.json')
        cls.queue=catalog.load(continuation04_report.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger','continuation03-remaining-dm.csv')
        cls.ids={r['symbol_id'] for r in cls.queue}
        cls.selected=[s for s in cls.symbols if s['symbol_id'] in cls.ids]

    def new_decision(self):
        return copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-04'))

    def test_all104_have_individual_current_source_contracts(self):
        self.assertEqual(104,len(self.ids))
        self.assertEqual([],continuation04_report.scope_errors(self.symbols,self.decisions,self.queue))

    def test_new04_missing_review_or_source_id_fails(self):
        d=self.new_decision()
        self.assertTrue(continuation04_report.scope_errors(self.symbols,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']],self.queue))
        self.assertTrue(continuation04_report.scope_errors([x for x in self.symbols if x['symbol_id']!=d['symbol_id']],self.decisions,self.queue))

    def test_new04_stale_source_identity_fails(self):
        d=self.new_decision();source=next(s for s in self.symbols if s['symbol_id']==d['symbol_id'])
        identity=json.loads(d['source_identity']);identity['body_sha256']='0'*64;d['source_identity']=json.dumps(identity)
        self.assertTrue(any('incorrect exact source identity' in e for e in semantic.verify_member_contract(d,source)))

    def test_new04_dependency_corruption_fails(self):
        d=self.new_decision();pins=json.loads(d['dependency_pins']);pins[next(iter(pins))]='0'*64;d['dependency_pins']=json.dumps(pins)
        self.assertTrue(any('stale dependency' in e for e in semantic.verify_pins([d],'04 fixture')))

    def test_new04_portable_comparison_pin_corruption_fails(self):
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-04' and json.loads(d['compared_portable_declarations'])))
        comparisons=json.loads(d['compared_portable_declarations']);comparisons[0]['body_sha256']='0'*64;d['compared_portable_declarations']=json.dumps(comparisons)
        source=next(s for s in self.symbols if s['symbol_id']==d['symbol_id'])
        self.assertTrue(any('stale portable equivalent declaration' in e for e in semantic.verify_member_contract(d,source)))

    def test_new04_operation_mapping_drift_fails(self):
        operations=continuation04_report.operation_rows(self.selected)
        self.assertEqual([],continuation04_report.operation_errors(self.selected,operations))
        changed=copy.deepcopy(operations);changed[0]['source_ids']='invented-id'
        self.assertTrue(continuation04_report.operation_errors(self.selected,changed))
        changed=copy.deepcopy(operations);changed[0]['approved_batch']='04A.99'
        self.assertTrue(continuation04_report.operation_errors(self.selected,changed))

    def test_historical_checkpoint_same_size_alteration_fails(self):
        with tempfile.TemporaryDirectory(prefix='beshstat-04history-') as directory:
            root=pathlib.Path(directory);path='docs/architecture/WindowsMigrationLedger/continuation03.md'
            target=root/path;target.parent.mkdir(parents=True);target.write_bytes(b'historical03')
            records=[dict(path=path,sha256=hashlib.sha256(target.read_bytes()).hexdigest())]
            self.assertEqual([],continuation04_report.historical_errors(root,records))
            target.write_bytes(b'Historical03')
            self.assertTrue(continuation04_report.historical_errors(root,records))

    def test_new04_index_same_size_hash_drift_fails(self):
        with tempfile.TemporaryDirectory(prefix='beshstat-04index-') as directory:
            root=pathlib.Path(directory);relative='docs/architecture/WindowsMigrationLedger/continuation04.md'
            target=root/relative;target.parent.mkdir(parents=True);target.write_bytes(b'checkpoint04')
            index=dict(artifacts=[dict(path=relative,bytes=12,sha256=hashlib.sha256(target.read_bytes()).hexdigest())])
            proposed=[relative,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation03_report.verify_index(root,index,proposed))
            target.write_bytes(b'Checkpoint04')
            self.assertTrue(any('artifact hash' in e for e in continuation03_report.verify_index(root,index,proposed)))

    def test_new04_strict_blockers_are2_but_deleted_pin_is1(self):
        valid=SemanticReviewTests.analyzer(self,self.decisions)
        self.assertEqual(2,valid.returncode,valid.stdout+valid.stderr)
        d=self.new_decision();invalid=SemanticReviewTests.analyzer(self,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']])
        self.assertEqual(1,invalid.returncode,invalid.stdout+invalid.stderr)


class Continuation05Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv');cls.decisions=semantic.read_store('review-decisions.json')
        cls.queue=[s for s in catalog.load(continuation05_report.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger','remaining-member-review.csv') if s['path'] in continuation05_report.TARGETS]
        cls.canonical=catalog.load(catalog.DEFAULT,'ribbon-crosswalk.csv')

    def new_review(self):
        return copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-05' and json.loads(d['source_identity'])['path'].endswith('BinaryClassificationReporting.vb')))

    def test_complete155_ABC_and_generated_contracts(self):
        self.assertEqual(155,len(self.queue))
        self.assertEqual([],continuation05_report.scope_errors(self.symbols,self.decisions,self.queue))
        self.assertEqual([],continuation05_report.generated_errors(self.symbols,self.decisions))

    def test_missing_regmodels_pin_and_incorrect_ABC_fail(self):
        d=self.new_review();missing=[x for x in self.decisions if x['symbol_id']!=d['symbol_id']]
        self.assertTrue(continuation05_report.scope_errors(self.symbols,missing,self.queue))
        d['review_group']='C';changed=[d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
        self.assertTrue(any('incorrect A/B/C' in e for e in continuation05_report.scope_errors(self.symbols,changed,self.queue)))

    def test_stale_regmodels_source_and_dependency_pin_fail(self):
        d=self.new_review();s=next(s for s in self.symbols if s['symbol_id']==d['symbol_id'])
        identity=json.loads(d['source_identity']);identity['body_sha256']='0'*64;d['source_identity']=json.dumps(identity)
        self.assertTrue(semantic.verify_member_contract(d,s))
        d=self.new_review();pins=json.loads(d['dependency_pins']);pins[next(iter(pins))]='0'*64;d['dependency_pins']=json.dumps(pins)
        self.assertTrue(semantic.verify_pins([d],'C05 fixture'))

    def test_stale_forwarded_Core_equivalent_fails(self):
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-05' and d['equivalence_scope']=='whole-method'))
        target=json.loads(d['portable_equivalent_pins']);target[0]['body_sha256']='0'*64;d['portable_equivalent_pins']=json.dumps(target)
        s=next(s for s in self.symbols if s['symbol_id']==d['symbol_id'])
        self.assertTrue(any('stale portable equivalent' in e for e in semantic.verify_member_contract(d,s)))

    def test_AutoOpen_required_Core_settings_dependency_cannot_be_removed(self):
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-05' and json.loads(d['source_identity'])['path']=='BESHstatNG/BESHStatAddIn.vb' and json.loads(d['source_identity'])['start_line']=='14'))
        dependency='BESHStat.Core/src/AppInfrastructure/AppSettings.vb'
        pins=json.loads(d['dependency_pins']);self.assertIn(dependency,pins);del pins[dependency]
        d['dependency_pins']=json.dumps(pins);d['required_dependency_paths']=json.dumps(sorted(pins))
        changed=[d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
        self.assertTrue(any('missing file-backed Core settings' in e for e in continuation05_report.scope_errors(self.symbols,changed,self.queue)))

    def test_omitted_or_double_counted_Ribbon_callbacks_fail(self):
        routes=continuation05_report.ribbon_rows(self.symbols,self.decisions,self.canonical)
        self.assertEqual(84,len(routes));self.assertEqual(81,len({r['action'] for r in routes}))
        self.assertEqual([],continuation05_report.ribbon_errors(routes,self.symbols,self.decisions,self.canonical))
        for broken in (routes[1:],routes+[routes[0]]):self.assertTrue(continuation05_report.ribbon_errors(broken,self.symbols,self.decisions,self.canonical))

    def test_invalid_generated_ownership_or_provenance_fails(self):
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-05' and d['review_group']=='C'))
        d['disposition']='extract-to-Core';d['generated_provenance']=''
        changed=[d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
        self.assertTrue(continuation05_report.generated_errors(self.symbols,changed))

    def test_same_size_index_hash_corruption_fails(self):
        with tempfile.TemporaryDirectory(prefix='beshstat-05index-') as directory:
            root=pathlib.Path(directory);relative='docs/architecture/WindowsMigrationLedger/continuation05.md'
            p=root/relative;p.parent.mkdir(parents=True);p.write_bytes(b'checkpoint05')
            index=dict(artifacts=[dict(path=relative,bytes=12,sha256=hashlib.sha256(p.read_bytes()).hexdigest())])
            proposed=[relative,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation03_report.verify_index(root,index,proposed));p.write_bytes(b'Checkpoint05')
            self.assertTrue(continuation03_report.verify_index(root,index,proposed))

    def test_historical04_same_size_corruption_fails(self):
        with tempfile.TemporaryDirectory(prefix='beshstat-05history-') as directory:
            root=pathlib.Path(directory);relative='docs/architecture/WindowsMigrationLedger/continuation04-validation.json'
            p=root/relative;p.parent.mkdir(parents=True);p.write_bytes(b'historical04')
            records=[dict(path=relative,sha256=hashlib.sha256(p.read_bytes()).hexdigest())]
            self.assertEqual([],continuation05_report.historical_errors(root,records));p.write_bytes(b'Historical04')
            self.assertTrue(continuation05_report.historical_errors(root,records))

    def test_strict_open2_but_missing_new_reg_pin1(self):
        valid=SemanticReviewTests.analyzer(self,self.decisions);self.assertEqual(2,valid.returncode,valid.stdout+valid.stderr)
        d=self.new_review();invalid=SemanticReviewTests.analyzer(self,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']])
        self.assertEqual(1,invalid.returncode,invalid.stdout+invalid.stderr)


class Continuation06Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv');cls.decisions=semantic.read_store('review-decisions.json')
        cls.queue=[s for s in catalog.load(continuation06_report.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger','remaining-member-review.csv') if s['path'] in continuation06_report.TARGETS]
        cls.ids={s['symbol_id'] for s in cls.queue};cls.selected=sorted((s for s in cls.symbols if s['symbol_id'] in cls.ids),key=lambda s:(s['path'],int(s['span_start'])))
        cls.exports=catalog.load(catalog.DEFAULT,'udf-exports.csv')
    def new_review(self):return copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-06'))
    def test_complete108_exact_source_and_helper_mapping(self):
        self.assertEqual(108,len(self.ids));self.assertEqual([],continuation06_report.scope_errors(self.symbols,self.decisions,self.queue))
        rows=continuation06_report.export_rows(self.selected,self.exports)
        self.assertEqual(268,len(self.exports));self.assertTrue(all(r['excel_function_export']=='no' for r in rows))
        self.assertEqual([],continuation06_report.export_errors(self.selected,self.exports,rows))
    def test_cache_owner_sites_cannot_be_omitted_or_unpinned(self):
        self.assertEqual([],continuation06_report.cache_owner_errors(self.symbols,self.decisions))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('cache_owner_sites')));sites=json.loads(d['cache_owner_sites']);d['cache_owner_sites']=json.dumps(sites[1:])
        changed=[d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
        self.assertTrue(continuation06_report.cache_owner_errors(self.symbols,changed))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('cache_owner_sites')));pins=json.loads(d['dependency_pins']);del pins[sites[0]['path']];d['dependency_pins']=json.dumps(pins)
        changed=[d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
        self.assertTrue(continuation06_report.cache_owner_errors(self.symbols,changed))
    def test_missing_source_or_pin_and_wrong_group_fail(self):
        d=self.new_review();ident=d['symbol_id']
        self.assertTrue(continuation06_report.scope_errors(self.symbols,[x for x in self.decisions if x['symbol_id']!=ident],self.queue))
        self.assertTrue(continuation06_report.scope_errors([s for s in self.symbols if s['symbol_id']!=ident],self.decisions,self.queue))
        d['review_group']='unreviewed';changed=[d if x['symbol_id']==ident else x for x in self.decisions]
        self.assertTrue(continuation06_report.scope_errors(self.symbols,changed,self.queue))
    def test_source_dependency_and_portable_pins_fail_when_stale(self):
        d=self.new_review();s=next(s for s in self.symbols if s['symbol_id']==d['symbol_id']);identity=json.loads(d['source_identity'])
        identity['body_sha256']='0'*64;d['source_identity']=json.dumps(identity);self.assertTrue(semantic.verify_member_contract(d,s))
        d=self.new_review();pins=json.loads(d['dependency_pins']);pins[next(iter(pins))]='0'*64;d['dependency_pins']=json.dumps(pins)
        self.assertTrue(semantic.verify_pins([d],'C06 fixture'))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-06' and json.loads(d['portable_equivalent_pins'])))
        pins=json.loads(d['portable_equivalent_pins']);pins[0]['body_sha256']='0'*64;d['portable_equivalent_pins']=json.dumps(pins)
        s=next(s for s in self.symbols if s['symbol_id']==d['symbol_id']);self.assertTrue(semantic.verify_member_contract(d,s))
    def test_generic_host_label_does_not_complete_review(self):
        d=self.new_review();d['review_contract']='unreviewed';d['review_status']='static triage';d['disposition']='retain-Windows-host-only'
        changed=[d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
        self.assertFalse(semantic.qualified(d));self.assertTrue(continuation06_report.scope_errors(self.symbols,changed,self.queue))
    def test_public_helper_cannot_be_fabricated_as_export(self):
        rows=continuation06_report.export_rows(self.selected,self.exports);self.assertTrue(any('Public' in r['signature'] for r in rows))
        rows[0]['excel_function_export']='yes';rows[0]['export_names']='invented'
        self.assertTrue(continuation06_report.export_errors(self.selected,self.exports,rows))
    def test_operation_rows_and_membership_cannot_drift(self):
        rows=continuation06_report.operation_rows(self.selected);self.assertEqual([],continuation06_report.operation_errors(self.selected,rows))
        broken=copy.deepcopy(rows);broken[0]['source_ids']='invented';self.assertTrue(continuation06_report.operation_errors(self.selected,broken))
        self.assertTrue(continuation06_report.operation_errors(self.selected,rows[1:]))
    def test_history05_and_index_same_size_hash_drift_fail(self):
        with tempfile.TemporaryDirectory(prefix='beshstat-06hash-') as directory:
            root=pathlib.Path(directory);relative='docs/architecture/WindowsMigrationLedger/continuation05-validation.json'
            p=root/relative;p.parent.mkdir(parents=True);p.write_bytes(b'historical05');digest=hashlib.sha256(p.read_bytes()).hexdigest()
            records=[dict(path=relative,sha256=digest)];index=dict(artifacts=[dict(path=relative,bytes=12,sha256=digest)])
            proposed=[relative,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation06_report.historical_errors(root,records));self.assertEqual([],continuation03_report.verify_index(root,index,proposed))
            p.write_bytes(b'Historical05');self.assertTrue(continuation06_report.historical_errors(root,records));self.assertTrue(continuation03_report.verify_index(root,index,proposed))
    def test_strict2_is_distinct_from_corruption1(self):
        good=SemanticReviewTests.analyzer(self,self.decisions);self.assertEqual(2,good.returncode,good.stdout+good.stderr)
        d=self.new_review();bad=SemanticReviewTests.analyzer(self,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']])
        self.assertEqual(1,bad.returncode,bad.stdout+bad.stderr)


class Continuation07Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv');cls.decisions=semantic.read_store('review-decisions.json')
        cls.queue=[s for s in catalog.load(continuation07_report.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger','remaining-member-review.csv') if s['path'] in continuation07_report.TARGETS]
        cls.ids={s['symbol_id'] for s in cls.queue};cls.selected=sorted((s for s in cls.symbols if s['symbol_id'] in cls.ids),key=lambda s:(s['path'],int(s['span_start'])))
        cls.exports=catalog.load(catalog.DEFAULT,'udf-exports.csv')
    def new_review(self):return copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-07'))
    def changed(self,d):return [d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
    def test_complete77_exact_groups_and_no_exports(self):
        self.assertEqual(77,len(self.ids));self.assertEqual([],continuation07_report.scope_errors(self.symbols,self.decisions,self.queue))
        self.assertEqual({'A':18,'B':18,'C':28,'D':13},dict(__import__('collections').Counter(d['review_group'] for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-07')))
        rows=continuation07_report.export_rows(self.selected,self.exports)
        self.assertTrue(all(r['excel_function_export']=='no' for r in rows));self.assertEqual([],continuation07_report.export_errors(self.selected,self.exports,rows))
    def test_missing_source_review_and_group_fail(self):
        d=self.new_review();ident=d['symbol_id']
        self.assertTrue(continuation07_report.scope_errors(self.symbols,[x for x in self.decisions if x['symbol_id']!=ident],self.queue))
        self.assertTrue(continuation07_report.scope_errors([s for s in self.symbols if s['symbol_id']!=ident],self.decisions,self.queue))
        d['review_group']='C';self.assertTrue(continuation07_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_stale_source_dependency_portable_and_enclosing_fail(self):
        d=self.new_review();s=next(s for s in self.symbols if s['symbol_id']==d['symbol_id'])
        d['body_sha256']='0'*64;self.assertTrue(continuation07_report.scope_errors(self.symbols,self.changed(d),self.queue))
        d=self.new_review();p=json.loads(d['dependency_pins']);p[next(iter(p))]='0'*64;d['dependency_pins']=json.dumps(p);self.assertTrue(semantic.verify_pins([d],'C07 fixture'))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-07' and json.loads(d['portable_equivalent_pins'])))
        p=json.loads(d['portable_equivalent_pins']);p[0]['body_sha256']='0'*64;d['portable_equivalent_pins']=json.dumps(p)
        s=next(s for s in self.symbols if s['symbol_id']==d['symbol_id']);self.assertTrue(semantic.verify_member_contract(d,s))
        d=self.new_review();d['source_enclosing_symbol_id']='invented';self.assertTrue(continuation07_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_generic_disposition_cannot_qualify(self):
        d=self.new_review();d['review_contract']='unreviewed';d['review_status']='static triage';d['disposition']='retain-Windows-host-only'
        self.assertFalse(semantic.qualified(d));self.assertTrue(continuation07_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_forged_export_projection_or_annotation_fail(self):
        rows=continuation07_report.export_rows(self.selected,self.exports);bad=copy.deepcopy(rows);bad[0]['excel_function_export']='yes'
        self.assertTrue(continuation07_report.export_errors(self.selected,self.exports,bad))
        exports=self.exports+[dict(symbol_id=self.selected[0]['symbol_id'],effective_name='FORGED')]
        mapped=continuation07_report.export_rows(self.selected,exports);self.assertTrue(continuation07_report.export_errors(self.selected,exports,mapped))
    def test_missing_candidate_or_unknown_prior_operation_fail(self):
        rows=continuation07_report.operation_rows(self.selected);self.assertEqual([],continuation07_report.operation_errors(self.selected,rows))
        self.assertTrue(continuation07_report.operation_errors(self.selected,rows[1:]))
        bad=copy.deepcopy(rows);bad[0]['source_ids']='invented';self.assertTrue(continuation07_report.operation_errors(self.selected,bad))
        bad=copy.deepcopy(self.selected);victim=next(s for s in bad if s['disposition'].startswith('extract-to-'));victim['candidate_operation_id']='C06-invented'
        self.assertTrue(continuation07_report.operation_errors(bad,continuation07_report.operation_rows(bad)))
    def test_historical_and_same_size_index_hash_fail(self):
        with tempfile.TemporaryDirectory(prefix='beshstat07-hash-') as directory:
            root=pathlib.Path(directory);rel='docs/architecture/WindowsMigrationLedger/continuation06-validation.json';p=root/rel;p.parent.mkdir(parents=True);p.write_bytes(b'prior06')
            digest=hashlib.sha256(p.read_bytes()).hexdigest();records=[dict(path=rel,bytes=7,sha256=digest)]
            idx=dict(artifacts=records);proposed=[rel,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation07_report.historical_errors(root,records));self.assertEqual([],continuation07_report.raw_history_errors(root,records))
            self.assertEqual([],continuation03_report.verify_index(root,idx,proposed))
            p.write_bytes(b'Prior06');self.assertTrue(continuation07_report.historical_errors(root,records));self.assertTrue(continuation07_report.raw_history_errors(root,records));self.assertTrue(continuation03_report.verify_index(root,idx,proposed))
    def test_incomplete_strict2_and_missing_pin_integrity1(self):
        good=SemanticReviewTests.analyzer(self,self.decisions);self.assertEqual(2,good.returncode,good.stdout+good.stderr)
        d=self.new_review();bad=SemanticReviewTests.analyzer(self,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']]);self.assertEqual(1,bad.returncode,bad.stdout+bad.stderr)


class Continuation08Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv');cls.decisions=semantic.read_store('review-decisions.json')
        cls.oldroot=continuation08_report.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
        cls.queue=[s for s in catalog.load(cls.oldroot,'remaining-member-review.csv') if s['path'] in continuation08_report.TARGETS]
        cls.ids={s['symbol_id'] for s in cls.queue};cls.selected=sorted((s for s in cls.symbols if s['symbol_id'] in cls.ids),key=lambda s:(s['path'],int(s['span_start'])))
        cls.exports=catalog.load(catalog.DEFAULT,'udf-exports.csv');cls.original_exports=catalog.load(cls.oldroot,'udf-exports.csv')
    def new_review(self):return copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-08'))
    def changed(self,d):return [d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
    def test_94_ids_71_exports_23_helpers_exact_groups(self):
        self.assertEqual(94,len(self.ids));self.assertEqual([],continuation08_report.scope_errors(self.symbols,self.decisions,self.queue))
        self.assertEqual({'A':30,'B':29,'C':35},dict(__import__('collections').Counter(d['review_group'] for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-08')))
        rows=continuation08_report.export_rows(self.selected,self.exports);self.assertEqual(71,sum(r['excel_function_export']=='yes' for r in rows));self.assertEqual(23,sum(r['excel_function_export']=='no' for r in rows))
        self.assertEqual([],continuation08_report.export_errors(self.selected,self.exports,rows));self.assertEqual([],continuation08_report.registration_errors(self.exports,self.original_exports))
    def test_late_alpha_boundary_notes_remain_source_pinned(self):
        records=json.loads((continuation08_report.EVIDENCE/'alpha-boundary-refinement.json').read_text(encoding='utf-8'))['records']
        self.assertEqual([],continuation08_report.alpha_errors(self.decisions,records))
        self.assertTrue(continuation08_report.alpha_errors(self.decisions,records[:-1]))
        bad=copy.deepcopy(records);bad[0]['source_observation']='generic family rule';self.assertTrue(continuation08_report.alpha_errors(self.decisions,bad))
        bad=copy.deepcopy(self.decisions);d=next(d for d in bad if d.get('symbol_id')==records[0]['symbol_id']);d['compared_portable_declarations']='[]'
        self.assertTrue(continuation08_report.alpha_errors(bad,records))
    def test_export_helper_flip_name_signature_attribute_drift_fail(self):
        rows=continuation08_report.export_rows(self.selected,self.exports)
        for role in ('yes','no'):
            bad=copy.deepcopy(rows);next(r for r in bad if r['excel_function_export']==role)['excel_function_export']='no' if role=='yes' else 'yes'
            self.assertTrue(continuation08_report.export_errors(self.selected,self.exports,bad))
        for key in ('effective_name','signature','attribute'):
            bad=copy.deepcopy(self.exports);next(r for r in bad if r['symbol_id'] in self.ids)[key]+=' CHANGED'
            self.assertTrue(continuation08_report.registration_errors(bad,self.original_exports))
        self.assertTrue(continuation08_report.registration_errors(self.exports[:-1],self.original_exports))
        d=self.new_review();records=json.loads(d['registration_evidence']);self.assertTrue(records)
        records[0]['effective_name']='FORGED';d['registration_evidence']=json.dumps(records)
        self.assertTrue(continuation08_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_missing_duplicate_member_source_or_group_fail(self):
        d=self.new_review();ident=d['symbol_id']
        self.assertTrue(continuation08_report.scope_errors(self.symbols,[x for x in self.decisions if x['symbol_id']!=ident],self.queue))
        self.assertTrue(continuation08_report.scope_errors(self.symbols,self.decisions+[d],self.queue))
        self.assertTrue(continuation08_report.scope_errors([s for s in self.symbols if s['symbol_id']!=ident],self.decisions,self.queue))
        d['review_group']='unreviewed';self.assertTrue(continuation08_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_body_signature_dependency_and_portable_pins_fail(self):
        for field in ('body_sha256','signature_key'):
            d=self.new_review();d[field]='corrupt';self.assertTrue(continuation08_report.scope_errors(self.symbols,self.changed(d),self.queue))
        d=self.new_review();pins=json.loads(d['dependency_pins']);pins[next(iter(pins))]='0'*64;d['dependency_pins']=json.dumps(pins);self.assertTrue(semantic.verify_pins([d],'C08 fixture'))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-08' and json.loads(d['portable_equivalent_pins'])))
        pins=json.loads(d['portable_equivalent_pins']);pins[0]['body_sha256']='0'*64;d['portable_equivalent_pins']=json.dumps(pins)
        self.assertTrue(semantic.verify_member_contract(d,next(s for s in self.symbols if s['symbol_id']==d['symbol_id'])))
    def test_prior_review_object_bytes_cannot_change(self):
        old='[\n  {"symbol_id": "old", "rationale": "source"}\n]';good='[\n  {"symbol_id": "old", "rationale": "source"},\n  {"symbol_id": "new"}\n]'
        self.assertEqual([],continuation08_report.prior_decision_errors(good,old))
        self.assertTrue(continuation08_report.prior_decision_errors(good.replace('source','guess'),old))
        self.assertTrue(continuation08_report.prior_decision_errors(good.replace('"rationale": ','"rationale":  '),old))
        actual=(semantic.TOOLING/'review-decisions.json').read_text(encoding='utf-8-sig');original=(continuation08_report.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_text(encoding='utf-8-sig')
        self.assertEqual([],continuation08_report.prior_decision_errors(actual,original))
    def test_candidate_members_omissions_duplicates_and_unknown_groups_fail(self):
        rows=continuation08_report.operation_rows(self.selected);self.assertEqual([],continuation08_report.operation_errors(self.selected,rows))
        self.assertTrue(continuation08_report.operation_errors(self.selected,rows[1:]))
        bad=copy.deepcopy(rows);bad[0]['source_ids']+='; '+bad[0]['source_ids'].split('; ')[0];self.assertTrue(continuation08_report.operation_errors(self.selected,bad))
        selected=copy.deepcopy(self.selected);next(s for s in selected if s['disposition'].startswith('extract-to-'))['candidate_operation_id']='C07-forged'
        self.assertTrue(continuation08_report.operation_errors(selected,continuation08_report.operation_rows(selected)))
    def test_historical07_and_same_size_current_manifest_drift_fail(self):
        with tempfile.TemporaryDirectory(prefix='beshstat08-hash-') as directory:
            root=pathlib.Path(directory);rel='docs/architecture/WindowsMigrationLedger/continuation07-validation.json';p=root/rel;p.parent.mkdir(parents=True);p.write_bytes(b'prior07')
            digest=hashlib.sha256(p.read_bytes()).hexdigest();records=[dict(path=rel,bytes=7,sha256=digest)];idx=dict(artifacts=records);proposed=[rel,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation08_report.historical_errors(root,records));self.assertEqual([],continuation03_report.verify_index(root,idx,proposed))
            p.write_bytes(b'Prior07');self.assertTrue(continuation08_report.historical_errors(root,records));self.assertTrue(continuation03_report.verify_index(root,idx,proposed))
            self.assertTrue(continuation03_report.verify_index(root,dict(artifacts=[]),proposed))
    def test_strict_incomplete2_distinct_from_missing_pin_integrity1(self):
        good=SemanticReviewTests.analyzer(self,self.decisions);self.assertEqual(2,good.returncode,good.stdout+good.stderr)
        d=self.new_review();bad=SemanticReviewTests.analyzer(self,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']]);self.assertEqual(1,bad.returncode,bad.stdout+bad.stderr)


class Continuation09Tests(unittest.TestCase):
    def test_09_numeric_comparison_declarations_are_individually_pinned(self):
        rows=catalog.load(catalog.DEFAULT,'continuation09-numeric-interfaces.csv')
        self.assertEqual([],continuation09_report.numeric_interface_errors(rows))
        self.assertTrue(continuation09_report.numeric_interface_errors(rows[1:]))
        bad=copy.deepcopy(rows);bad[0]['body_excerpt']+=' guessed';self.assertTrue(continuation09_report.numeric_interface_errors(bad))
    def test_09_misleading_ready_status_rejected(self):
        summary={'unreviewed_for_acceptance':2730,'qualified_full_control_contracts':1,'control_review_denominator':2056,'status':'NOT READY FOR ACCEPTANCE'}
        index={'acceptance_status':'NOT READY FOR ACCEPTANCE'}
        self.assertEqual([],continuation09_report.acceptance_errors(summary,index))
        self.assertTrue(continuation09_report.acceptance_errors(summary,{'acceptance_status':'READY'}))
        summary['status']='READY';self.assertTrue(continuation09_report.acceptance_errors(summary,index))
    def test_09_alignment_stale_missing_and_false_runtime_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation09-gui-udf-alignment.csv')
        self.assertEqual([],continuation09_report.alignment_errors(rows))
        self.assertTrue(continuation09_report.alignment_errors(rows[1:]))
        for field in ('udf_pin','core_pin'):
            bad=copy.deepcopy(rows);pin=json.loads(bad[0][field]);pin['body_sha256']='0'*64;bad[0][field]=json.dumps(pin)
            self.assertTrue(continuation09_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['runtime_status']='PASS';self.assertTrue(continuation09_report.alignment_errors(bad))
    def test_09_candidate_owner_cannot_be_removed(self):
        selected=copy.deepcopy(self.selected)
        next(s for s in selected if s['disposition'].startswith('extract-to-'))['owner_role']=''
        self.assertTrue(continuation09_report.operation_errors(selected,continuation09_report.operation_rows(selected)))
    @classmethod
    def setUpClass(cls):
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv');cls.decisions=semantic.read_store('review-decisions.json')
        cls.oldroot=continuation09_report.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
        cls.queue=[s for s in catalog.load(cls.oldroot,'remaining-member-review.csv') if s['path'] in continuation09_report.TARGETS]
        cls.ids={s['symbol_id'] for s in cls.queue};cls.selected=sorted((s for s in cls.symbols if s['symbol_id'] in cls.ids),key=lambda s:(s['path'],int(s['span_start'])))
        cls.exports=catalog.load(catalog.DEFAULT,'udf-exports.csv');cls.original_exports=catalog.load(cls.oldroot,'udf-exports.csv')
    def new_review(self):return copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-09'))
    def changed(self,d):return [d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
    def test_09_84_ids_38_exports_46_helpers_exact_groups(self):
        self.assertEqual(84,len(self.ids));self.assertEqual([],continuation09_report.scope_errors(self.symbols,self.decisions,self.queue))
        self.assertEqual({'A':32,'B':39,'C':13},dict(__import__('collections').Counter(d['review_group'] for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-09')))
        rows=continuation09_report.export_rows(self.selected,self.exports);self.assertEqual(38,sum(r['excel_function_export']=='yes' for r in rows));self.assertEqual(46,sum(r['excel_function_export']=='no' for r in rows))
        self.assertEqual([],continuation09_report.export_errors(self.selected,self.exports,rows));self.assertEqual([],continuation09_report.registration_errors(self.exports,self.original_exports))
    def test_09_export_helper_flip_name_signature_attribute_drift_fail(self):
        rows=continuation09_report.export_rows(self.selected,self.exports)
        for role in ('yes','no'):
            bad=copy.deepcopy(rows);next(r for r in bad if r['excel_function_export']==role)['excel_function_export']='no' if role=='yes' else 'yes'
            self.assertTrue(continuation09_report.export_errors(self.selected,self.exports,bad))
        for key in ('effective_name','signature','attribute'):
            bad=copy.deepcopy(self.exports);next(r for r in bad if r['symbol_id'] in self.ids)[key]+=' CHANGED'
            self.assertTrue(continuation09_report.registration_errors(bad,self.original_exports))
        self.assertTrue(continuation09_report.registration_errors(self.exports[:-1],self.original_exports))
        d=self.new_review();records=json.loads(d['registration_evidence']);self.assertTrue(records)
        records[0]['effective_name']='FORGED';d['registration_evidence']=json.dumps(records)
        self.assertTrue(continuation09_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_09_missing_duplicate_member_source_or_group_fail(self):
        d=self.new_review();ident=d['symbol_id']
        self.assertTrue(continuation09_report.scope_errors(self.symbols,[x for x in self.decisions if x['symbol_id']!=ident],self.queue))
        self.assertTrue(continuation09_report.scope_errors(self.symbols,self.decisions+[d],self.queue))
        self.assertTrue(continuation09_report.scope_errors([s for s in self.symbols if s['symbol_id']!=ident],self.decisions,self.queue))
        d['review_group']='unreviewed';self.assertTrue(continuation09_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_09_body_signature_dependency_and_portable_pins_fail(self):
        for field in ('body_sha256','signature_key'):
            d=self.new_review();d[field]='corrupt';self.assertTrue(continuation09_report.scope_errors(self.symbols,self.changed(d),self.queue))
        d=self.new_review();pins=json.loads(d['dependency_pins']);pins[next(iter(pins))]='0'*64;d['dependency_pins']=json.dumps(pins);self.assertTrue(semantic.verify_pins([d],'C08 fixture'))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-09' and json.loads(d['portable_equivalent_pins'])))
        pins=json.loads(d['portable_equivalent_pins']);pins[0]['body_sha256']='0'*64;d['portable_equivalent_pins']=json.dumps(pins)
        self.assertTrue(semantic.verify_member_contract(d,next(s for s in self.symbols if s['symbol_id']==d['symbol_id'])))
    def test_09_prior_review_object_bytes_cannot_change(self):
        old='[\n  {"symbol_id": "old", "rationale": "source"}\n]';good='[\n  {"symbol_id": "old", "rationale": "source"},\n  {"symbol_id": "new"}\n]'
        self.assertEqual([],continuation09_report.prior_decision_errors(good,old))
        self.assertTrue(continuation09_report.prior_decision_errors(good.replace('source','guess'),old))
        self.assertTrue(continuation09_report.prior_decision_errors(good.replace('"rationale": ','"rationale":  '),old))
        actual=(semantic.TOOLING/'review-decisions.json').read_text(encoding='utf-8-sig');original=(continuation09_report.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_text(encoding='utf-8-sig')
        self.assertEqual([],continuation09_report.prior_decision_errors(actual,original))
    def test_09_candidate_members_omissions_duplicates_and_unknown_groups_fail(self):
        rows=continuation09_report.operation_rows(self.selected);self.assertEqual([],continuation09_report.operation_errors(self.selected,rows))
        self.assertTrue(continuation09_report.operation_errors(self.selected,rows[1:]))
        bad=copy.deepcopy(rows);bad[0]['source_ids']+='; '+bad[0]['source_ids'].split('; ')[0];self.assertTrue(continuation09_report.operation_errors(self.selected,bad))
        selected=copy.deepcopy(self.selected);next(s for s in selected if s['disposition'].startswith('extract-to-'))['candidate_operation_id']='C07-forged'
        self.assertTrue(continuation09_report.operation_errors(selected,continuation09_report.operation_rows(selected)))
    def test_09_historical07_and_same_size_current_manifest_drift_fail(self):
        with tempfile.TemporaryDirectory(prefix='beshstat08-hash-') as directory:
            root=pathlib.Path(directory);rel='docs/architecture/WindowsMigrationLedger/continuation07-validation.json';p=root/rel;p.parent.mkdir(parents=True);p.write_bytes(b'prior07')
            digest=hashlib.sha256(p.read_bytes()).hexdigest();records=[dict(path=rel,bytes=7,sha256=digest)];idx=dict(artifacts=records);proposed=[rel,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation09_report.historical_errors(root,records));self.assertEqual([],continuation03_report.verify_index(root,idx,proposed))
            p.write_bytes(b'Prior07');self.assertTrue(continuation09_report.historical_errors(root,records));self.assertTrue(continuation03_report.verify_index(root,idx,proposed))
            self.assertTrue(continuation03_report.verify_index(root,dict(artifacts=[]),proposed))
    def test_09_strict_incomplete2_distinct_from_missing_pin_integrity1(self):
        good=SemanticReviewTests.analyzer(self,self.decisions);self.assertEqual(2,good.returncode,good.stdout+good.stderr)
        d=self.new_review();bad=SemanticReviewTests.analyzer(self,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']]);self.assertEqual(1,bad.returncode,bad.stdout+bad.stderr)


class Continuation10Tests(unittest.TestCase):
    def test_10_misleading_ready_status_rejected(self):
        summary={'unreviewed_for_acceptance':2730,'qualified_full_control_contracts':1,'control_review_denominator':2056,'status':'NOT READY FOR ACCEPTANCE'}
        index={'acceptance_status':'NOT READY FOR ACCEPTANCE'}
        self.assertEqual([],continuation10_report.acceptance_errors(summary,index))
        self.assertTrue(continuation10_report.acceptance_errors(summary,{'acceptance_status':'READY'}))
        summary['status']='READY';self.assertTrue(continuation10_report.acceptance_errors(summary,index))
    def test_10_alignment_stale_missing_and_false_runtime_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation10-gui-udf-alignment.csv')
        self.assertEqual([],continuation10_report.alignment_errors(rows))
        self.assertTrue(continuation10_report.alignment_errors(rows[1:]))
        for field in ('udf_pin','core_pin'):
            bad=copy.deepcopy(rows);pin=json.loads(bad[0][field]);pin['body_sha256']='0'*64;bad[0][field]=json.dumps(pin)
            self.assertTrue(continuation10_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['runtime_status']='PASS';self.assertTrue(continuation10_report.alignment_errors(bad))
    def test_10_candidate_owner_cannot_be_removed(self):
        selected=copy.deepcopy(self.selected)
        next(s for s in selected if s['disposition'].startswith('extract-to-'))['owner_role']=''
        self.assertTrue(continuation10_report.operation_errors(selected,continuation10_report.operation_rows(selected)))
    @classmethod
    def setUpClass(cls):
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv');cls.decisions=semantic.read_store('review-decisions.json')
        cls.oldroot=continuation10_report.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
        cls.queue=[s for s in catalog.load(cls.oldroot,'remaining-member-review.csv') if s['path'] in continuation10_report.TARGETS]
        cls.ids={s['symbol_id'] for s in cls.queue};cls.selected=sorted((s for s in cls.symbols if s['symbol_id'] in cls.ids),key=lambda s:(s['path'],int(s['span_start'])))
        cls.exports=catalog.load(catalog.DEFAULT,'udf-exports.csv');cls.original_exports=catalog.load(cls.oldroot,'udf-exports.csv')
    def new_review(self):return copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-10' and json.loads(d.get('registration_evidence','[]'))))
    def changed(self,d):return [d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
    def test_10_63_ids_27_exports_36_helpers_exact_groups(self):
        self.assertEqual(63,len(self.ids));self.assertEqual([],continuation10_report.scope_errors(self.symbols,self.decisions,self.queue))
        self.assertEqual({'A':39,'B':11,'C':13},dict(__import__('collections').Counter(d['review_group'] for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-10')))
        rows=continuation10_report.export_rows(self.selected,self.exports);self.assertEqual(27,sum(r['excel_function_export']=='yes' for r in rows));self.assertEqual(36,sum(r['excel_function_export']=='no' for r in rows))
        self.assertEqual([],continuation10_report.export_errors(self.selected,self.exports,rows));self.assertEqual([],continuation10_report.registration_errors(self.exports,self.original_exports))
    def test_10_export_helper_flip_name_signature_attribute_drift_fail(self):
        rows=continuation10_report.export_rows(self.selected,self.exports)
        for role in ('yes','no'):
            bad=copy.deepcopy(rows);next(r for r in bad if r['excel_function_export']==role)['excel_function_export']='no' if role=='yes' else 'yes'
            self.assertTrue(continuation10_report.export_errors(self.selected,self.exports,bad))
        for key in ('effective_name','signature','attribute'):
            bad=copy.deepcopy(self.exports);next(r for r in bad if r['symbol_id'] in self.ids)[key]+=' CHANGED'
            self.assertTrue(continuation10_report.registration_errors(bad,self.original_exports))
        self.assertTrue(continuation10_report.registration_errors(self.exports[:-1],self.original_exports))
        d=self.new_review();records=json.loads(d['registration_evidence']);self.assertTrue(records)
        records[0]['effective_name']='FORGED';d['registration_evidence']=json.dumps(records)
        self.assertTrue(continuation10_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_10_missing_duplicate_member_source_or_group_fail(self):
        d=self.new_review();ident=d['symbol_id']
        self.assertTrue(continuation10_report.scope_errors(self.symbols,[x for x in self.decisions if x['symbol_id']!=ident],self.queue))
        self.assertTrue(continuation10_report.scope_errors(self.symbols,self.decisions+[d],self.queue))
        self.assertTrue(continuation10_report.scope_errors([s for s in self.symbols if s['symbol_id']!=ident],self.decisions,self.queue))
        d['review_group']='unreviewed';self.assertTrue(continuation10_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_10_body_signature_dependency_and_portable_pins_fail(self):
        for field in ('body_sha256','signature_key'):
            d=self.new_review();d[field]='corrupt';self.assertTrue(continuation10_report.scope_errors(self.symbols,self.changed(d),self.queue))
        d=self.new_review();pins=json.loads(d['dependency_pins']);pins[next(iter(pins))]='0'*64;d['dependency_pins']=json.dumps(pins);self.assertTrue(semantic.verify_pins([d],'C08 fixture'))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-10' and json.loads(d['portable_equivalent_pins'])))
        pins=json.loads(d['portable_equivalent_pins']);pins[0]['body_sha256']='0'*64;d['portable_equivalent_pins']=json.dumps(pins)
        self.assertTrue(semantic.verify_member_contract(d,next(s for s in self.symbols if s['symbol_id']==d['symbol_id'])))
    def test_10_prior_review_object_bytes_cannot_change(self):
        old='[\n  {"symbol_id": "old", "rationale": "source"}\n]';good='[\n  {"symbol_id": "old", "rationale": "source"},\n  {"symbol_id": "new"}\n]'
        self.assertEqual([],continuation10_report.prior_decision_errors(good,old))
        self.assertTrue(continuation10_report.prior_decision_errors(good.replace('source','guess'),old))
        self.assertTrue(continuation10_report.prior_decision_errors(good.replace('"rationale": ','"rationale":  '),old))
        actual=(semantic.TOOLING/'review-decisions.json').read_text(encoding='utf-8-sig');original=(continuation10_report.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_text(encoding='utf-8-sig')
        self.assertEqual([],continuation10_report.prior_decision_errors(actual,original))
    def test_10_candidate_members_omissions_duplicates_and_unknown_groups_fail(self):
        rows=continuation10_report.operation_rows(self.selected);self.assertEqual([],continuation10_report.operation_errors(self.selected,rows))
        self.assertTrue(continuation10_report.operation_errors(self.selected,rows[1:]))
        bad=copy.deepcopy(rows);bad[0]['source_ids']+='; '+bad[0]['source_ids'].split('; ')[0];self.assertTrue(continuation10_report.operation_errors(self.selected,bad))
        selected=copy.deepcopy(self.selected);next(s for s in selected if s['disposition'].startswith('extract-to-'))['candidate_operation_id']='C07-forged'
        self.assertTrue(continuation10_report.operation_errors(selected,continuation10_report.operation_rows(selected)))
    def test_10_historical07_and_same_size_current_manifest_drift_fail(self):
        with tempfile.TemporaryDirectory(prefix='beshstat08-hash-') as directory:
            root=pathlib.Path(directory);rel='docs/architecture/WindowsMigrationLedger/continuation07-validation.json';p=root/rel;p.parent.mkdir(parents=True);p.write_bytes(b'prior07')
            digest=hashlib.sha256(p.read_bytes()).hexdigest();records=[dict(path=rel,bytes=7,sha256=digest)];idx=dict(artifacts=records);proposed=[rel,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation10_report.historical_errors(root,records));self.assertEqual([],continuation03_report.verify_index(root,idx,proposed))
            p.write_bytes(b'Prior07');self.assertTrue(continuation10_report.historical_errors(root,records));self.assertTrue(continuation03_report.verify_index(root,idx,proposed))
            self.assertTrue(continuation03_report.verify_index(root,dict(artifacts=[]),proposed))
    def test_10_strict_incomplete2_distinct_from_missing_pin_integrity1(self):
        good=SemanticReviewTests.analyzer(self,self.decisions);self.assertEqual(2,good.returncode,good.stdout+good.stderr)
        d=self.new_review();bad=SemanticReviewTests.analyzer(self,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']]);self.assertEqual(1,bad.returncode,bad.stdout+bad.stderr)

    def test_10_fresh_runtime_and_retired_interpreter_rejection(self):
        runtime=continuation10_report.read(catalog.DEFAULT/'continuation10-runtime.json')
        self.assertEqual([],continuation10_report.runtime_errors(runtime,[]))
        bad=copy.deepcopy(runtime);bad['executable']='C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe'
        self.assertTrue(continuation10_report.runtime_errors(bad,[]))
        self.assertTrue(continuation10_report.runtime_errors(runtime,[{'command':['powershell','-PythonExe',bad['executable']]}]))
    def test_10_candidate_batch_cannot_be_removed(self):
        selected=copy.deepcopy(self.selected);next(s for s in selected if s['disposition'].startswith('extract-to-'))['future_batch']=''
        self.assertTrue(continuation10_report.operation_errors(selected,continuation10_report.operation_rows(selected)))


class Continuation11Tests(unittest.TestCase):
    def test_11_misleading_ready_status_rejected(self):
        summary={'unreviewed_for_acceptance':2730,'qualified_full_control_contracts':1,'control_review_denominator':2056,'status':'NOT READY FOR ACCEPTANCE'}
        index={'acceptance_status':'NOT READY FOR ACCEPTANCE'}
        self.assertEqual([],continuation11_report.acceptance_errors(summary,index))
        self.assertTrue(continuation11_report.acceptance_errors(summary,{'acceptance_status':'READY'}))
        summary['status']='READY';self.assertTrue(continuation11_report.acceptance_errors(summary,index))
    def test_11_alignment_stale_missing_and_false_runtime_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation11-gui-udf-alignment.csv')
        self.assertEqual([],continuation11_report.alignment_errors(rows))
        self.assertTrue(continuation11_report.alignment_errors(rows[1:]))
        for field in ('udf_pin','core_pin'):
            bad=copy.deepcopy(rows);pin=json.loads(bad[0][field]);pin['body_sha256']='0'*64;bad[0][field]=json.dumps(pin)
            self.assertTrue(continuation11_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['runtime_status']='PASS';self.assertTrue(continuation11_report.alignment_errors(bad))
    def test_11_candidate_owner_cannot_be_removed(self):
        selected=copy.deepcopy(self.selected)
        next(s for s in selected if s['disposition'].startswith('extract-to-'))['owner_role']=''
        self.assertTrue(continuation11_report.operation_errors(selected,continuation11_report.operation_rows(selected)))
    @classmethod
    def setUpClass(cls):
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv');cls.decisions=semantic.read_store('review-decisions.json')
        cls.oldroot=continuation11_report.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
        cls.queue=[s for s in catalog.load(cls.oldroot,'remaining-member-review.csv') if s['path'] in continuation11_report.TARGETS]
        cls.ids={s['symbol_id'] for s in cls.queue};cls.selected=sorted((s for s in cls.symbols if s['symbol_id'] in cls.ids),key=lambda s:(s['path'],int(s['span_start'])))
        cls.exports=catalog.load(catalog.DEFAULT,'udf-exports.csv');cls.original_exports=catalog.load(cls.oldroot,'udf-exports.csv')
    def new_review(self):return copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-11' and json.loads(d.get('registration_evidence','[]'))))
    def changed(self,d):return [d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
    def test_11_63_ids_30_exports_33_helpers_exact_groups(self):
        self.assertEqual(63,len(self.ids));self.assertEqual([],continuation11_report.scope_errors(self.symbols,self.decisions,self.queue))
        self.assertEqual({'A':38,'B':25},dict(__import__('collections').Counter(d['review_group'] for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-11')))
        rows=continuation11_report.export_rows(self.selected,self.exports);self.assertEqual(30,sum(r['excel_function_export']=='yes' for r in rows));self.assertEqual(33,sum(r['excel_function_export']=='no' for r in rows))
        self.assertEqual([],continuation11_report.export_errors(self.selected,self.exports,rows));self.assertEqual([],continuation11_report.registration_errors(self.exports,self.original_exports))
    def test_11_export_helper_flip_name_signature_attribute_drift_fail(self):
        rows=continuation11_report.export_rows(self.selected,self.exports)
        for role in ('yes','no'):
            bad=copy.deepcopy(rows);next(r for r in bad if r['excel_function_export']==role)['excel_function_export']='no' if role=='yes' else 'yes'
            self.assertTrue(continuation11_report.export_errors(self.selected,self.exports,bad))
        for key in ('effective_name','signature','attribute'):
            bad=copy.deepcopy(self.exports);next(r for r in bad if r['symbol_id'] in self.ids)[key]+=' CHANGED'
            self.assertTrue(continuation11_report.registration_errors(bad,self.original_exports))
        self.assertTrue(continuation11_report.registration_errors(self.exports[:-1],self.original_exports))
        d=self.new_review();records=json.loads(d['registration_evidence']);self.assertTrue(records)
        records[0]['effective_name']='FORGED';d['registration_evidence']=json.dumps(records)
        self.assertTrue(continuation11_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_11_missing_duplicate_member_source_or_group_fail(self):
        d=self.new_review();ident=d['symbol_id']
        self.assertTrue(continuation11_report.scope_errors(self.symbols,[x for x in self.decisions if x['symbol_id']!=ident],self.queue))
        self.assertTrue(continuation11_report.scope_errors(self.symbols,self.decisions+[d],self.queue))
        self.assertTrue(continuation11_report.scope_errors([s for s in self.symbols if s['symbol_id']!=ident],self.decisions,self.queue))
        d['review_group']='unreviewed';self.assertTrue(continuation11_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_11_body_signature_dependency_and_portable_pins_fail(self):
        for field in ('body_sha256','signature_key'):
            d=self.new_review();d[field]='corrupt';self.assertTrue(continuation11_report.scope_errors(self.symbols,self.changed(d),self.queue))
        d=self.new_review();pins=json.loads(d['dependency_pins']);pins[next(iter(pins))]='0'*64;d['dependency_pins']=json.dumps(pins);self.assertTrue(semantic.verify_pins([d],'C08 fixture'))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-11' and json.loads(d['portable_equivalent_pins'])))
        pins=json.loads(d['portable_equivalent_pins']);pins[0]['body_sha256']='0'*64;d['portable_equivalent_pins']=json.dumps(pins)
        self.assertTrue(semantic.verify_member_contract(d,next(s for s in self.symbols if s['symbol_id']==d['symbol_id'])))
    def test_11_prior_review_object_bytes_cannot_change(self):
        old='[\n  {"symbol_id": "old", "rationale": "source"}\n]';good='[\n  {"symbol_id": "old", "rationale": "source"},\n  {"symbol_id": "new"}\n]'
        self.assertEqual([],continuation11_report.prior_decision_errors(good,old))
        self.assertTrue(continuation11_report.prior_decision_errors(good.replace('source','guess'),old))
        self.assertTrue(continuation11_report.prior_decision_errors(good.replace('"rationale": ','"rationale":  '),old))
        actual=(semantic.TOOLING/'review-decisions.json').read_text(encoding='utf-8-sig');original=(continuation11_report.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_text(encoding='utf-8-sig')
        self.assertEqual([],continuation11_report.prior_decision_errors(actual,original))
    def test_11_candidate_members_omissions_duplicates_and_unknown_groups_fail(self):
        rows=continuation11_report.operation_rows(self.selected);self.assertEqual([],continuation11_report.operation_errors(self.selected,rows))
        self.assertTrue(continuation11_report.operation_errors(self.selected,rows[1:]))
        bad=copy.deepcopy(rows);bad[0]['source_ids']+='; '+bad[0]['source_ids'].split('; ')[0];self.assertTrue(continuation11_report.operation_errors(self.selected,bad))
        selected=copy.deepcopy(self.selected);next(s for s in selected if s['disposition'].startswith('extract-to-'))['candidate_operation_id']='C07-forged'
        self.assertTrue(continuation11_report.operation_errors(selected,continuation11_report.operation_rows(selected)))
    def test_11_historical07_and_same_size_current_manifest_drift_fail(self):
        with tempfile.TemporaryDirectory(prefix='beshstat08-hash-') as directory:
            root=pathlib.Path(directory);rel='docs/architecture/WindowsMigrationLedger/continuation07-validation.json';p=root/rel;p.parent.mkdir(parents=True);p.write_bytes(b'prior07')
            digest=hashlib.sha256(p.read_bytes()).hexdigest();records=[dict(path=rel,bytes=7,sha256=digest)];idx=dict(artifacts=records);proposed=[rel,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation11_report.historical_errors(root,records));self.assertEqual([],continuation03_report.verify_index(root,idx,proposed))
            p.write_bytes(b'Prior07');self.assertTrue(continuation11_report.historical_errors(root,records));self.assertTrue(continuation03_report.verify_index(root,idx,proposed))
            self.assertTrue(continuation03_report.verify_index(root,dict(artifacts=[]),proposed))
    def test_11_strict_incomplete2_distinct_from_missing_pin_integrity1(self):
        good=SemanticReviewTests.analyzer(self,self.decisions);self.assertEqual(2,good.returncode,good.stdout+good.stderr)
        d=self.new_review();bad=SemanticReviewTests.analyzer(self,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']]);self.assertEqual(1,bad.returncode,bad.stdout+bad.stderr)

    def test_11_fresh_runtime_and_retired_interpreter_rejection(self):
        runtime=continuation11_report.read(catalog.DEFAULT/'continuation11-runtime.json')
        self.assertEqual([],continuation11_report.runtime_errors(runtime,[]))
        bad=copy.deepcopy(runtime);bad['executable']='C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe'
        self.assertTrue(continuation11_report.runtime_errors(bad,[]))
        self.assertTrue(continuation11_report.runtime_errors(runtime,[{'command':['powershell','-PythonExe',bad['executable']]}]))
    def test_11_candidate_batch_cannot_be_removed(self):
        selected=copy.deepcopy(self.selected);next(s for s in selected if s['disposition'].startswith('extract-to-'))['future_batch']=''
        self.assertTrue(continuation11_report.operation_errors(selected,continuation11_report.operation_rows(selected)))

    def test_11_false_whole_core_wrapper_claim_rejected(self):
        d=self.new_review();d['equivalence_scope']='whole-method'
        self.assertTrue(continuation11_report.scope_errors(self.symbols,self.changed(d),self.queue))
        d=self.new_review();d['whole_wrapper_equivalence']='complete Core forward'
        self.assertTrue(continuation11_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_11_prior_raw_crlf_bytes_preserved_and_newline_drift_rejected(self):
        old='[\r\n {"symbol_id":"old",\r\n "rationale":"source"}\r\n]'
        self.assertTrue(continuation11_report.prior_decision_errors(old.replace('\r\n','\r\r\n'),old))
        actual=(semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig')
        original=(continuation11_report.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
        self.assertEqual([],continuation11_report.prior_decision_errors(actual,original))


    def test_11_missing_stale_comparison_body_and_file_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation11-portable-comparisons.csv')
        self.assertEqual([],continuation11_report.comparison_errors(rows))
        self.assertTrue(continuation11_report.comparison_errors(rows[1:]))
        for key in ('body_sha256','source_sha256','body_excerpt'):
            bad=copy.deepcopy(rows);bad[0][key]='stale'
            self.assertTrue(continuation11_report.comparison_errors(bad))

    def test_11_gui_alignment_empty_stale_and_policy_batch_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation11-gui-udf-alignment.csv')
        for field in ('gui_entry','future_batch','candidate_shared_policy','current_gui_behavior','current_core_behavior'):
            bad=copy.deepcopy(rows);bad[0][field]=''
            self.assertTrue(continuation11_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['gui_pins']='[]';self.assertTrue(continuation11_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);pins=json.loads(bad[0]['gui_pins']);pins[0]['body_sha256']='0'*64;bad[0]['gui_pins']=json.dumps(pins)
        self.assertTrue(continuation11_report.alignment_errors(bad))

    def test_11_existing_test_and_reference_fixture_pins_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation11-fixture-crosswalk.csv')
        self.assertEqual([],continuation11_report.fixture_errors(rows))
        self.assertTrue(continuation11_report.fixture_errors(rows[1:]))
        for field in ('source_sha256','declaration_and_body'):
            bad=copy.deepcopy(rows);bad[0][field]='stale';self.assertTrue(continuation11_report.fixture_errors(bad))
        bad=copy.deepcopy(rows);pins=json.loads(bad[0]['dependency_file_pins']);pins[next(iter(pins))]='0'*64;bad[0]['dependency_file_pins']=json.dumps(pins)
        self.assertTrue(continuation11_report.fixture_errors(bad))

class Continuation12Tests(unittest.TestCase):
    def test_12_misleading_ready_status_rejected(self):
        summary={'unreviewed_for_acceptance':2730,'qualified_full_control_contracts':1,'control_review_denominator':2056,'status':'NOT READY FOR ACCEPTANCE'}
        index={'acceptance_status':'NOT READY FOR ACCEPTANCE'}
        self.assertEqual([],continuation12_report.acceptance_errors(summary,index))
        self.assertTrue(continuation12_report.acceptance_errors(summary,{'acceptance_status':'READY'}))
        summary['status']='READY';self.assertTrue(continuation12_report.acceptance_errors(summary,index))
    def test_12_alignment_stale_missing_and_false_runtime_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation12-gui-udf-alignment.csv')
        self.assertEqual([],continuation12_report.alignment_errors(rows))
        self.assertTrue(continuation12_report.alignment_errors(rows[1:]))
        for field in ('udf_pin','core_pin'):
            bad=copy.deepcopy(rows);pin=json.loads(bad[0][field]);pin['body_sha256']='0'*64;bad[0][field]=json.dumps(pin)
            self.assertTrue(continuation12_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['runtime_status']='PASS';self.assertTrue(continuation12_report.alignment_errors(bad))
    def test_12_candidate_owner_cannot_be_removed(self):
        selected=copy.deepcopy(self.selected)
        next(s for s in selected if s['disposition'].startswith('extract-to-'))['owner_role']=''
        self.assertTrue(continuation12_report.operation_errors(selected,continuation12_report.operation_rows(selected)))
    @classmethod
    def setUpClass(cls):
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv');cls.decisions=semantic.read_store('review-decisions.json')
        cls.oldroot=continuation12_report.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
        cls.queue=[s for s in catalog.load(cls.oldroot,'remaining-member-review.csv') if s['path'] in continuation12_report.TARGETS]
        cls.ids={s['symbol_id'] for s in cls.queue};cls.selected=sorted((s for s in cls.symbols if s['symbol_id'] in cls.ids),key=lambda s:(s['path'],int(s['span_start'])))
        cls.exports=catalog.load(catalog.DEFAULT,'udf-exports.csv');cls.original_exports=catalog.load(cls.oldroot,'udf-exports.csv')
    def new_review(self):return copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-12' and json.loads(d.get('registration_evidence','[]'))))
    def changed(self,d):return [d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
    def test_12_50_ids_20_exports_30_helpers_exact_groups(self):
        self.assertEqual(50,len(self.ids));self.assertEqual([],continuation12_report.scope_errors(self.symbols,self.decisions,self.queue))
        self.assertEqual({'A':5,'B':13,'C':32},dict(__import__('collections').Counter(d['review_group'] for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-12')))
        rows=continuation12_report.export_rows(self.selected,self.exports);self.assertEqual(20,sum(r['excel_function_export']=='yes' for r in rows));self.assertEqual(30,sum(r['excel_function_export']=='no' for r in rows))
        self.assertEqual([],continuation12_report.export_errors(self.selected,self.exports,rows));self.assertEqual([],continuation12_report.registration_errors(self.exports,self.original_exports))
    def test_12_export_helper_flip_name_signature_attribute_drift_fail(self):
        rows=continuation12_report.export_rows(self.selected,self.exports)
        for role in ('yes','no'):
            bad=copy.deepcopy(rows);next(r for r in bad if r['excel_function_export']==role)['excel_function_export']='no' if role=='yes' else 'yes'
            self.assertTrue(continuation12_report.export_errors(self.selected,self.exports,bad))
        for key in ('effective_name','signature','attribute'):
            bad=copy.deepcopy(self.exports);next(r for r in bad if r['symbol_id'] in self.ids)[key]+=' CHANGED'
            self.assertTrue(continuation12_report.registration_errors(bad,self.original_exports))
        self.assertTrue(continuation12_report.registration_errors(self.exports[:-1],self.original_exports))
        d=self.new_review();records=json.loads(d['registration_evidence']);self.assertTrue(records)
        records[0]['effective_name']='FORGED';d['registration_evidence']=json.dumps(records)
        self.assertTrue(continuation12_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_12_missing_duplicate_member_source_or_group_fail(self):
        d=self.new_review();ident=d['symbol_id']
        self.assertTrue(continuation12_report.scope_errors(self.symbols,[x for x in self.decisions if x['symbol_id']!=ident],self.queue))
        self.assertTrue(continuation12_report.scope_errors(self.symbols,self.decisions+[d],self.queue))
        self.assertTrue(continuation12_report.scope_errors([s for s in self.symbols if s['symbol_id']!=ident],self.decisions,self.queue))
        d['review_group']='unreviewed';self.assertTrue(continuation12_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_12_body_signature_dependency_and_portable_pins_fail(self):
        for field in ('body_sha256','signature_key'):
            d=self.new_review();d[field]='corrupt';self.assertTrue(continuation12_report.scope_errors(self.symbols,self.changed(d),self.queue))
        d=self.new_review();pins=json.loads(d['dependency_pins']);pins[next(iter(pins))]='0'*64;d['dependency_pins']=json.dumps(pins);self.assertTrue(semantic.verify_pins([d],'C08 fixture'))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-12' and json.loads(d['portable_equivalent_pins'])))
        pins=json.loads(d['portable_equivalent_pins']);pins[0]['body_sha256']='0'*64;d['portable_equivalent_pins']=json.dumps(pins)
        self.assertTrue(semantic.verify_member_contract(d,next(s for s in self.symbols if s['symbol_id']==d['symbol_id'])))
    def test_12_prior_review_object_bytes_cannot_change(self):
        old='[\n  {"symbol_id": "old", "rationale": "source"}\n]';good='[\n  {"symbol_id": "old", "rationale": "source"},\n  {"symbol_id": "new"}\n]'
        self.assertEqual([],continuation12_report.prior_decision_errors(good,old))
        self.assertTrue(continuation12_report.prior_decision_errors(good.replace('source','guess'),old))
        self.assertTrue(continuation12_report.prior_decision_errors(good.replace('"rationale": ','"rationale":  '),old))
        actual=(semantic.TOOLING/'review-decisions.json').read_text(encoding='utf-8-sig');original=(continuation12_report.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_text(encoding='utf-8-sig')
        self.assertEqual([],continuation12_report.prior_decision_errors(actual,original))
    def test_12_candidate_members_omissions_duplicates_and_unknown_groups_fail(self):
        rows=continuation12_report.operation_rows(self.selected);self.assertEqual([],continuation12_report.operation_errors(self.selected,rows))
        self.assertTrue(continuation12_report.operation_errors(self.selected,rows[1:]))
        bad=copy.deepcopy(rows);bad[0]['source_ids']+='; '+bad[0]['source_ids'].split('; ')[0];self.assertTrue(continuation12_report.operation_errors(self.selected,bad))
        selected=copy.deepcopy(self.selected);next(s for s in selected if s['disposition'].startswith('extract-to-'))['candidate_operation_id']='C07-forged'
        self.assertTrue(continuation12_report.operation_errors(selected,continuation12_report.operation_rows(selected)))
    def test_12_historical07_and_same_size_current_manifest_drift_fail(self):
        with tempfile.TemporaryDirectory(prefix='beshstat08-hash-') as directory:
            root=pathlib.Path(directory);rel='docs/architecture/WindowsMigrationLedger/continuation07-validation.json';p=root/rel;p.parent.mkdir(parents=True);p.write_bytes(b'prior07')
            digest=hashlib.sha256(p.read_bytes()).hexdigest();records=[dict(path=rel,bytes=7,sha256=digest)];idx=dict(artifacts=records);proposed=[rel,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation12_report.historical_errors(root,records));self.assertEqual([],continuation03_report.verify_index(root,idx,proposed))
            p.write_bytes(b'Prior07');self.assertTrue(continuation12_report.historical_errors(root,records));self.assertTrue(continuation03_report.verify_index(root,idx,proposed))
            self.assertTrue(continuation03_report.verify_index(root,dict(artifacts=[]),proposed))
    def test_12_strict_incomplete2_distinct_from_missing_pin_integrity1(self):
        good=SemanticReviewTests.analyzer(self,self.decisions);self.assertEqual(2,good.returncode,good.stdout+good.stderr)
        d=self.new_review();bad=SemanticReviewTests.analyzer(self,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']]);self.assertEqual(1,bad.returncode,bad.stdout+bad.stderr)

    def test_12_fresh_runtime_and_retired_interpreter_rejection(self):
        runtime=continuation12_report.read(catalog.DEFAULT/'continuation12-runtime.json')
        self.assertEqual([],continuation12_report.runtime_errors(runtime,[]))
        bad=copy.deepcopy(runtime);bad['executable']='C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe'
        self.assertTrue(continuation12_report.runtime_errors(bad,[]))
        self.assertTrue(continuation12_report.runtime_errors(runtime,[{'command':['powershell','-PythonExe',bad['executable']]}]))
    def test_12_candidate_batch_cannot_be_removed(self):
        selected=copy.deepcopy(self.selected);next(s for s in selected if s['disposition'].startswith('extract-to-'))['future_batch']=''
        self.assertTrue(continuation12_report.operation_errors(selected,continuation12_report.operation_rows(selected)))

    def test_12_false_whole_core_wrapper_claim_rejected(self):
        d=self.new_review();d['equivalence_scope']='whole-method'
        self.assertTrue(continuation12_report.scope_errors(self.symbols,self.changed(d),self.queue))
        d=self.new_review();d['whole_wrapper_equivalence']='complete Core forward'
        self.assertTrue(continuation12_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_12_prior_raw_crlf_bytes_preserved_and_newline_drift_rejected(self):
        old='[\r\n {"symbol_id":"old",\r\n "rationale":"source"}\r\n]'
        self.assertTrue(continuation12_report.prior_decision_errors(old.replace('\r\n','\r\r\n'),old))
        actual=(semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig')
        original=(continuation12_report.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
        self.assertEqual([],continuation12_report.prior_decision_errors(actual,original))


    def test_12_missing_stale_comparison_body_and_file_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation12-portable-comparisons.csv')
        self.assertEqual([],continuation12_report.comparison_errors(rows))
        self.assertTrue(continuation12_report.comparison_errors(rows[1:]))
        for key in ('body_sha256','source_sha256','body_excerpt'):
            bad=copy.deepcopy(rows);bad[0][key]='stale'
            self.assertTrue(continuation12_report.comparison_errors(bad))

    def test_12_gui_alignment_empty_stale_and_policy_batch_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation12-gui-udf-alignment.csv')
        for field in ('gui_entry','future_batch','candidate_shared_policy','current_gui_behavior','current_core_behavior'):
            bad=copy.deepcopy(rows);bad[0][field]=''
            self.assertTrue(continuation12_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['gui_pins']='[]';self.assertTrue(continuation12_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);pins=json.loads(bad[0]['gui_pins']);pins[0]['body_sha256']='0'*64;bad[0]['gui_pins']=json.dumps(pins)
        self.assertTrue(continuation12_report.alignment_errors(bad))

    def test_12_existing_test_and_reference_fixture_pins_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation12-fixture-crosswalk.csv')
        self.assertEqual([],continuation12_report.fixture_errors(rows))
        self.assertTrue(continuation12_report.fixture_errors(rows[1:]))
        for field in ('source_sha256','declaration_and_body'):
            bad=copy.deepcopy(rows);bad[0][field]='stale';self.assertTrue(continuation12_report.fixture_errors(bad))
        bad=copy.deepcopy(rows);pins=json.loads(bad[0]['dependency_file_pins']);pins[next(iter(pins))]='0'*64;bad[0]['dependency_file_pins']=json.dumps(pins)
        self.assertTrue(continuation12_report.fixture_errors(bad))

    def test_12_psm_matches_exact_registration_spelling(self):
        row=next(e for e in self.exports if e['effective_name']=='BESH.PSM.MATCHES')
        self.assertIn(row['symbol_id'],self.ids)
        bad=copy.deepcopy(self.exports);next(e for e in bad if e['symbol_id']==row['symbol_id'])['effective_name']='BESH.PS.MATCHES'
        self.assertTrue(continuation12_report.registration_errors(bad,self.original_exports))

class Continuation13Tests(unittest.TestCase):
    def test_13_misleading_ready_status_rejected(self):
        summary={'unreviewed_for_acceptance':2730,'qualified_full_control_contracts':1,'control_review_denominator':2056,'status':'NOT READY FOR ACCEPTANCE'}
        index={'acceptance_status':'NOT READY FOR ACCEPTANCE'}
        self.assertEqual([],continuation13_report.acceptance_errors(summary,index))
        self.assertTrue(continuation13_report.acceptance_errors(summary,{'acceptance_status':'READY'}))
        summary['status']='READY';self.assertTrue(continuation13_report.acceptance_errors(summary,index))
    def test_13_alignment_stale_missing_and_false_runtime_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation13-gui-udf-alignment.csv')
        self.assertEqual([],continuation13_report.alignment_errors(rows))
        self.assertTrue(continuation13_report.alignment_errors(rows[1:]))
        for field in ('udf_pin','core_pin'):
            bad=copy.deepcopy(rows);pin=json.loads(bad[0][field]);pin['body_sha256']='0'*64;bad[0][field]=json.dumps(pin)
            self.assertTrue(continuation13_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['runtime_status']='PASS';self.assertTrue(continuation13_report.alignment_errors(bad))
    def test_13_candidate_owner_cannot_be_removed(self):
        selected=copy.deepcopy(self.selected)
        next(s for s in selected if s['disposition'].startswith('extract-to-'))['owner_role']=''
        self.assertTrue(continuation13_report.operation_errors(selected,continuation13_report.operation_rows(selected)))
    @classmethod
    def setUpClass(cls):
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv');cls.decisions=semantic.read_store('review-decisions.json')
        cls.oldroot=continuation13_report.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
        cls.queue=[s for s in catalog.load(cls.oldroot,'remaining-member-review.csv') if s['path'] in continuation13_report.TARGETS]
        cls.ids={s['symbol_id'] for s in cls.queue};cls.selected=sorted((s for s in cls.symbols if s['symbol_id'] in cls.ids),key=lambda s:(s['path'],int(s['span_start'])))
        cls.exports=catalog.load(catalog.DEFAULT,'udf-exports.csv');cls.original_exports=catalog.load(cls.oldroot,'udf-exports.csv')
    def new_review(self):return copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-13' and json.loads(d.get('registration_evidence','[]'))))
    def changed(self,d):return [d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
    def test_13_134_ids_70_exports_64_helpers_exact_groups(self):
        self.assertEqual(134,len(self.ids));self.assertEqual([],continuation13_report.scope_errors(self.symbols,self.decisions,self.queue))
        self.assertEqual({'A':25,'B':15,'C':31,'D':15,'E':48},dict(__import__('collections').Counter(d['review_group'] for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-13')))
        rows=continuation13_report.export_rows(self.selected,self.exports);self.assertEqual(70,sum(r['excel_function_export']=='yes' for r in rows));self.assertEqual(64,sum(r['excel_function_export']=='no' for r in rows))
        self.assertEqual([],continuation13_report.export_errors(self.selected,self.exports,rows));self.assertEqual([],continuation13_report.registration_errors(self.exports,self.original_exports))
    def test_13_export_helper_flip_name_signature_attribute_drift_fail(self):
        rows=continuation13_report.export_rows(self.selected,self.exports)
        for role in ('yes','no'):
            bad=copy.deepcopy(rows);next(r for r in bad if r['excel_function_export']==role)['excel_function_export']='no' if role=='yes' else 'yes'
            self.assertTrue(continuation13_report.export_errors(self.selected,self.exports,bad))
        for key in ('effective_name','signature','attribute'):
            bad=copy.deepcopy(self.exports);next(r for r in bad if r['symbol_id'] in self.ids)[key]+=' CHANGED'
            self.assertTrue(continuation13_report.registration_errors(bad,self.original_exports))
        self.assertTrue(continuation13_report.registration_errors(self.exports[:-1],self.original_exports))
        d=self.new_review();records=json.loads(d['registration_evidence']);self.assertTrue(records)
        records[0]['effective_name']='FORGED';d['registration_evidence']=json.dumps(records)
        self.assertTrue(continuation13_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_13_missing_duplicate_member_source_or_group_fail(self):
        d=self.new_review();ident=d['symbol_id']
        self.assertTrue(continuation13_report.scope_errors(self.symbols,[x for x in self.decisions if x['symbol_id']!=ident],self.queue))
        self.assertTrue(continuation13_report.scope_errors(self.symbols,self.decisions+[d],self.queue))
        self.assertTrue(continuation13_report.scope_errors([s for s in self.symbols if s['symbol_id']!=ident],self.decisions,self.queue))
        d['review_group']='unreviewed';self.assertTrue(continuation13_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_13_body_signature_dependency_and_portable_pins_fail(self):
        for field in ('body_sha256','signature_key'):
            d=self.new_review();d[field]='corrupt';self.assertTrue(continuation13_report.scope_errors(self.symbols,self.changed(d),self.queue))
        d=self.new_review();pins=json.loads(d['dependency_pins']);pins[next(iter(pins))]='0'*64;d['dependency_pins']=json.dumps(pins);self.assertTrue(semantic.verify_pins([d],'C08 fixture'))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-13' and json.loads(d['portable_equivalent_pins'])))
        pins=json.loads(d['portable_equivalent_pins']);pins[0]['body_sha256']='0'*64;d['portable_equivalent_pins']=json.dumps(pins)
        self.assertTrue(semantic.verify_member_contract(d,next(s for s in self.symbols if s['symbol_id']==d['symbol_id'])))
    def test_13_prior_review_object_bytes_cannot_change(self):
        old='[\n  {"symbol_id": "old", "rationale": "source"}\n]';good='[\n  {"symbol_id": "old", "rationale": "source"},\n  {"symbol_id": "new"}\n]'
        self.assertEqual([],continuation13_report.prior_decision_errors(good,old))
        self.assertTrue(continuation13_report.prior_decision_errors(good.replace('source','guess'),old))
        self.assertTrue(continuation13_report.prior_decision_errors(good.replace('"rationale": ','"rationale":  '),old))
        actual=(semantic.TOOLING/'review-decisions.json').read_text(encoding='utf-8-sig');original=(continuation13_report.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_text(encoding='utf-8-sig')
        self.assertEqual([],continuation13_report.prior_decision_errors(actual,original))
    def test_13_candidate_members_omissions_duplicates_and_unknown_groups_fail(self):
        rows=continuation13_report.operation_rows(self.selected);self.assertEqual([],continuation13_report.operation_errors(self.selected,rows))
        self.assertTrue(continuation13_report.operation_errors(self.selected,rows[1:]))
        bad=copy.deepcopy(rows);bad[0]['source_ids']+='; '+bad[0]['source_ids'].split('; ')[0];self.assertTrue(continuation13_report.operation_errors(self.selected,bad))
        selected=copy.deepcopy(self.selected);next(s for s in selected if s['disposition'].startswith('extract-to-'))['candidate_operation_id']='C07-forged'
        self.assertTrue(continuation13_report.operation_errors(selected,continuation13_report.operation_rows(selected)))
    def test_13_historical07_and_same_size_current_manifest_drift_fail(self):
        with tempfile.TemporaryDirectory(prefix='beshstat08-hash-') as directory:
            root=pathlib.Path(directory);rel='docs/architecture/WindowsMigrationLedger/continuation07-validation.json';p=root/rel;p.parent.mkdir(parents=True);p.write_bytes(b'prior07')
            digest=hashlib.sha256(p.read_bytes()).hexdigest();records=[dict(path=rel,bytes=7,sha256=digest)];idx=dict(artifacts=records);proposed=[rel,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation13_report.historical_errors(root,records));self.assertEqual([],continuation03_report.verify_index(root,idx,proposed))
            p.write_bytes(b'Prior07');self.assertTrue(continuation13_report.historical_errors(root,records));self.assertTrue(continuation03_report.verify_index(root,idx,proposed))
            self.assertTrue(continuation03_report.verify_index(root,dict(artifacts=[]),proposed))
    def test_13_strict_incomplete2_distinct_from_missing_pin_integrity1(self):
        good=SemanticReviewTests.analyzer(self,self.decisions);self.assertEqual(2,good.returncode,good.stdout+good.stderr)
        d=self.new_review();bad=SemanticReviewTests.analyzer(self,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']]);self.assertEqual(1,bad.returncode,bad.stdout+bad.stderr)

    def test_13_fresh_runtime_and_retired_interpreter_rejection(self):
        runtime=continuation13_report.read(catalog.DEFAULT/'continuation13-runtime.json')
        self.assertEqual([],continuation13_report.runtime_errors(runtime,[]))
        bad=copy.deepcopy(runtime);bad['executable']='C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe'
        self.assertTrue(continuation13_report.runtime_errors(bad,[]))
        self.assertTrue(continuation13_report.runtime_errors(runtime,[{'command':['powershell','-PythonExe',bad['executable']]}]))
    def test_13_candidate_batch_cannot_be_removed(self):
        selected=copy.deepcopy(self.selected);next(s for s in selected if s['disposition'].startswith('extract-to-'))['future_batch']=''
        self.assertTrue(continuation13_report.operation_errors(selected,continuation13_report.operation_rows(selected)))

    def test_13_false_whole_core_wrapper_claim_rejected(self):
        d=self.new_review();d['equivalence_scope']='whole-method'
        self.assertTrue(continuation13_report.scope_errors(self.symbols,self.changed(d),self.queue))
        d=self.new_review();d['whole_wrapper_equivalence']='complete Core forward'
        self.assertTrue(continuation13_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_13_prior_raw_crlf_bytes_preserved_and_newline_drift_rejected(self):
        old='[\r\n {"symbol_id":"old",\r\n "rationale":"source"}\r\n]'
        self.assertTrue(continuation13_report.prior_decision_errors(old.replace('\r\n','\r\r\n'),old))
        actual=(semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig')
        original=(continuation13_report.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
        self.assertEqual([],continuation13_report.prior_decision_errors(actual,original))


    def test_13_missing_stale_comparison_body_and_file_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation13-portable-comparisons.csv')
        self.assertEqual([],continuation13_report.comparison_errors(rows))
        self.assertTrue(continuation13_report.comparison_errors(rows[1:]))
        for key in ('body_sha256','source_sha256','body_excerpt'):
            bad=copy.deepcopy(rows);bad[0][key]='stale'
            self.assertTrue(continuation13_report.comparison_errors(bad))

    def test_13_gui_alignment_empty_stale_and_policy_batch_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation13-gui-udf-alignment.csv')
        for field in ('gui_entry','future_batch','candidate_shared_policy','current_gui_behavior','current_core_behavior'):
            bad=copy.deepcopy(rows);bad[0][field]=''
            self.assertTrue(continuation13_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['gui_pins']='[]';self.assertTrue(continuation13_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);pins=json.loads(bad[0]['gui_pins']);pins[0]['body_sha256']='0'*64;bad[0]['gui_pins']=json.dumps(pins)
        self.assertTrue(continuation13_report.alignment_errors(bad))

    def test_13_existing_test_and_reference_fixture_pins_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation13-fixture-crosswalk.csv')
        self.assertEqual([],continuation13_report.fixture_errors(rows))
        self.assertTrue(continuation13_report.fixture_errors(rows[1:]))
        for field in ('source_sha256','declaration_and_body'):
            bad=copy.deepcopy(rows);bad[0][field]='stale';self.assertTrue(continuation13_report.fixture_errors(bad))
        bad=copy.deepcopy(rows);pins=json.loads(bad[0]['dependency_file_pins']);pins[next(iter(pins))]='0'*64;bad[0]['dependency_file_pins']=json.dumps(pins)
        self.assertTrue(continuation13_report.fixture_errors(bad))


    def test_13_multivariate_exact_registration_spelling(self):
        row=next(e for e in self.exports if e['effective_name']=='BESH.MULTI.PCA_FIT')
        self.assertIn(row['symbol_id'],self.ids)
        bad=copy.deepcopy(self.exports);next(e for e in bad if e['symbol_id']==row['symbol_id'])['effective_name']='BESH.MULT.PCA_FIT'
        self.assertTrue(continuation13_report.registration_errors(bad,self.original_exports))
    def test_13_missing_state_and_lambda_records_rejected(self):
        self.assertEqual({'FunctionBlock':118,'SingleLineFunctionLambdaExpression':9,'VariableDeclarator':7},dict(__import__('collections').Counter(s['kind'] for s in self.selected)))
        for kind in ('SingleLineFunctionLambdaExpression','VariableDeclarator'):
            s=next(s for s in self.selected if s['kind']==kind)
            self.assertTrue(continuation13_report.scope_errors(self.symbols,[d for d in self.decisions if d['symbol_id']!=s['symbol_id']],self.queue))
            bad=copy.deepcopy(self.symbols);next(r for r in bad if r['symbol_id']==s['symbol_id'])['kind']='FunctionBlock'
            self.assertTrue(continuation13_report.scope_errors(bad,self.decisions,self.queue))
    def test_13_prior_order_change_rejected_even_with_exact_object_bytes(self):
        original='[{"symbol_id":"a"},{"symbol_id":"b"}]'
        self.assertEqual([],continuation13_report.prior_decision_errors(original,original))
        self.assertTrue(continuation13_report.prior_decision_errors('[{"symbol_id":"b"},{"symbol_id":"a"}]',original))


import continuation14_report

class Continuation14Tests(unittest.TestCase):
    def test_14_misleading_ready_status_rejected(self):
        summary={'unreviewed_for_acceptance':2730,'qualified_full_control_contracts':1,'control_review_denominator':2056,'status':'NOT READY FOR ACCEPTANCE'}
        index={'acceptance_status':'NOT READY FOR ACCEPTANCE'}
        self.assertEqual([],continuation14_report.acceptance_errors(summary,index))
        self.assertTrue(continuation14_report.acceptance_errors(summary,{'acceptance_status':'READY'}))
        summary['status']='READY';self.assertTrue(continuation14_report.acceptance_errors(summary,index))
    def test_14_alignment_stale_missing_and_false_runtime_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation14-gui-udf-alignment.csv')
        self.assertEqual([],continuation14_report.alignment_errors(rows))
        self.assertTrue(continuation14_report.alignment_errors(rows[1:]))
        for field in ('udf_pin','core_pin'):
            bad=copy.deepcopy(rows);pin=json.loads(bad[0][field]);pin['body_sha256']='0'*64;bad[0][field]=json.dumps(pin)
            self.assertTrue(continuation14_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['runtime_status']='PASS';self.assertTrue(continuation14_report.alignment_errors(bad))
    def test_14_candidate_owner_cannot_be_removed(self):
        selected=copy.deepcopy(self.selected)
        next(s for s in selected if s['disposition'].startswith('extract-to-'))['owner_role']=''
        self.assertTrue(continuation14_report.operation_errors(selected,continuation14_report.operation_rows(selected)))
    @classmethod
    def setUpClass(cls):
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv');cls.decisions=semantic.read_store('review-decisions.json')
        cls.oldroot=continuation14_report.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
        cls.queue=[s for s in catalog.load(cls.oldroot,'remaining-member-review.csv') if s['path'] in continuation14_report.TARGETS]
        cls.ids={s['symbol_id'] for s in cls.queue};cls.selected=sorted((s for s in cls.symbols if s['symbol_id'] in cls.ids),key=lambda s:(s['path'],int(s['span_start'])))
        cls.exports=catalog.load(catalog.DEFAULT,'udf-exports.csv');cls.original_exports=catalog.load(cls.oldroot,'udf-exports.csv')
    def new_review(self):return copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-14'))
    def changed(self,d):return [d if x['symbol_id']==d['symbol_id'] else x for x in self.decisions]
    def test_14_216_pending_graphics_ids_zero_exports_exact_groups(self):
        self.assertEqual(216,len(self.ids));self.assertEqual(24,len(continuation14_report.TARGETS));self.assertEqual([],continuation14_report.scope_errors(self.symbols,self.decisions,self.queue))
        self.assertEqual({'A':26,'B':55,'C':38,'D':41,'E':56},dict(__import__('collections').Counter(d['review_group'] for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-14')))
        rows=continuation14_report.export_rows(self.selected,self.exports);self.assertEqual(216,sum(r['excel_function_export']=='no' for r in rows));self.assertEqual(0,sum(r['excel_function_export']=='yes' for r in rows))
        self.assertEqual([],continuation14_report.export_errors(self.selected,self.exports,rows));self.assertEqual([],continuation14_report.registration_errors(self.exports,self.original_exports))

    def test_14_false_export_or_global_annotation_drift_rejected(self):
        rows=continuation14_report.export_rows(self.selected,self.exports);bad=copy.deepcopy(rows);bad[0]['excel_function_export']='yes';self.assertTrue(continuation14_report.export_errors(self.selected,self.exports,bad))
        for key in ('effective_name','signature','attribute'):
            bad=copy.deepcopy(self.exports);bad[0][key]+=' CHANGED';self.assertTrue(continuation14_report.registration_errors(bad,self.original_exports))
        self.assertTrue(continuation14_report.registration_errors(self.exports[:-1],self.original_exports))
        d=self.new_review();d['registration_evidence']='[{"effective_name":"FAKE"}]';self.assertTrue(continuation14_report.scope_errors(self.symbols,self.changed(d),self.queue))

    def test_14_missing_duplicate_member_source_or_group_fail(self):
        d=self.new_review();ident=d['symbol_id']
        self.assertTrue(continuation14_report.scope_errors(self.symbols,[x for x in self.decisions if x['symbol_id']!=ident],self.queue))
        self.assertTrue(continuation14_report.scope_errors(self.symbols,self.decisions+[d],self.queue))
        self.assertTrue(continuation14_report.scope_errors([s for s in self.symbols if s['symbol_id']!=ident],self.decisions,self.queue))
        d['review_group']='unreviewed';self.assertTrue(continuation14_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_14_body_signature_dependency_and_portable_pins_fail(self):
        for field in ('body_sha256','signature_key'):
            d=self.new_review();d[field]='corrupt';self.assertTrue(continuation14_report.scope_errors(self.symbols,self.changed(d),self.queue))
        d=self.new_review();pins=json.loads(d['dependency_pins']);pins[next(iter(pins))]='0'*64;d['dependency_pins']=json.dumps(pins);self.assertTrue(semantic.verify_pins([d],'C08 fixture'))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-14' and json.loads(d['portable_equivalent_pins'])))
        pins=json.loads(d['portable_equivalent_pins']);pins[0]['body_sha256']='0'*64;d['portable_equivalent_pins']=json.dumps(pins)
        self.assertTrue(semantic.verify_member_contract(d,next(s for s in self.symbols if s['symbol_id']==d['symbol_id'])))
    def test_14_prior_review_object_bytes_cannot_change(self):
        old='[\n  {"symbol_id": "old", "rationale": "source"}\n]';good='[\n  {"symbol_id": "old", "rationale": "source"},\n  {"symbol_id": "new"}\n]'
        self.assertEqual([],continuation14_report.prior_decision_errors(good,old))
        self.assertTrue(continuation14_report.prior_decision_errors(good.replace('source','guess'),old))
        self.assertTrue(continuation14_report.prior_decision_errors(good.replace('"rationale": ','"rationale":  '),old))
        actual=(semantic.TOOLING/'review-decisions.json').read_text(encoding='utf-8-sig');original=(continuation14_report.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_text(encoding='utf-8-sig')
        self.assertEqual([],continuation14_report.prior_decision_errors(actual,original))
    def test_14_candidate_members_omissions_duplicates_and_unknown_groups_fail(self):
        rows=continuation14_report.operation_rows(self.selected);self.assertEqual([],continuation14_report.operation_errors(self.selected,rows))
        self.assertTrue(continuation14_report.operation_errors(self.selected,rows[1:]))
        bad=copy.deepcopy(rows);bad[0]['source_ids']+='; '+bad[0]['source_ids'].split('; ')[0];self.assertTrue(continuation14_report.operation_errors(self.selected,bad))
        selected=copy.deepcopy(self.selected);next(s for s in selected if s['disposition'].startswith('extract-to-'))['candidate_operation_id']='C07-forged'
        self.assertTrue(continuation14_report.operation_errors(selected,continuation14_report.operation_rows(selected)))
    def test_14_historical07_and_same_size_current_manifest_drift_fail(self):
        with tempfile.TemporaryDirectory(prefix='beshstat08-hash-') as directory:
            root=pathlib.Path(directory);rel='docs/architecture/WindowsMigrationLedger/continuation07-validation.json';p=root/rel;p.parent.mkdir(parents=True);p.write_bytes(b'prior07')
            digest=hashlib.sha256(p.read_bytes()).hexdigest();records=[dict(path=rel,bytes=7,sha256=digest)];idx=dict(artifacts=records);proposed=[rel,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation14_report.historical_errors(root,records));self.assertEqual([],continuation03_report.verify_index(root,idx,proposed))
            p.write_bytes(b'Prior07');self.assertTrue(continuation14_report.historical_errors(root,records));self.assertTrue(continuation03_report.verify_index(root,idx,proposed))
            self.assertTrue(continuation03_report.verify_index(root,dict(artifacts=[]),proposed))
    def test_14_strict_incomplete2_distinct_from_missing_pin_integrity1(self):
        good=SemanticReviewTests.analyzer(self,self.decisions);self.assertEqual(2,good.returncode,good.stdout+good.stderr)
        d=self.new_review();bad=SemanticReviewTests.analyzer(self,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']]);self.assertEqual(1,bad.returncode,bad.stdout+bad.stderr)

    def test_14_fresh_runtime_and_retired_interpreter_rejection(self):
        runtime=continuation14_report.read(catalog.DEFAULT/'continuation14-runtime.json')
        self.assertEqual([],continuation14_report.runtime_errors(runtime,[]))
        bad=copy.deepcopy(runtime);bad['executable']='C:/unapproved/python.exe'
        self.assertTrue(continuation14_report.runtime_errors(bad,[]))
        self.assertTrue(continuation14_report.runtime_errors(runtime,[{'command':['powershell','-PythonExe',bad['executable']]}]))
    def test_14_candidate_batch_cannot_be_removed(self):
        selected=copy.deepcopy(self.selected);next(s for s in selected if s['disposition'].startswith('extract-to-'))['future_batch']=''
        self.assertTrue(continuation14_report.operation_errors(selected,continuation14_report.operation_rows(selected)))

    def test_14_false_whole_core_wrapper_claim_rejected(self):
        d=self.new_review();d['equivalence_scope']='whole-method'
        self.assertTrue(continuation14_report.scope_errors(self.symbols,self.changed(d),self.queue))
        d=self.new_review();d['whole_wrapper_equivalence']='complete Core forward'
        self.assertTrue(continuation14_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_14_prior_raw_crlf_bytes_preserved_and_newline_drift_rejected(self):
        old='[\r\n {"symbol_id":"old",\r\n "rationale":"source"}\r\n]'
        self.assertTrue(continuation14_report.prior_decision_errors(old.replace('\r\n','\r\r\n'),old))
        actual=(semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig')
        original=(continuation14_report.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
        self.assertEqual([],continuation14_report.prior_decision_errors(actual,original))


    def test_14_missing_stale_comparison_body_and_file_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation14-portable-comparisons.csv')
        self.assertEqual([],continuation14_report.comparison_errors(rows))
        self.assertTrue(continuation14_report.comparison_errors(rows[1:]))
        for key in ('body_sha256','source_sha256','body_excerpt'):
            bad=copy.deepcopy(rows);bad[0][key]='stale'
            self.assertTrue(continuation14_report.comparison_errors(bad))

    def test_14_gui_alignment_empty_stale_and_policy_batch_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation14-gui-udf-alignment.csv')
        for field in ('gui_entry','future_batch','candidate_shared_policy','current_gui_behavior','current_core_behavior'):
            bad=copy.deepcopy(rows);bad[0][field]=''
            self.assertTrue(continuation14_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['gui_pins']='[]';self.assertTrue(continuation14_report.alignment_errors(bad))
        bad=copy.deepcopy(rows);pins=json.loads(bad[0]['gui_pins']);pins[0]['body_sha256']='0'*64;bad[0]['gui_pins']=json.dumps(pins)
        self.assertTrue(continuation14_report.alignment_errors(bad))

    def test_14_existing_test_and_reference_fixture_pins_fail(self):
        rows=catalog.load(catalog.DEFAULT,'continuation14-fixture-crosswalk.csv')
        self.assertEqual([],continuation14_report.fixture_errors(rows))
        self.assertTrue(continuation14_report.fixture_errors(rows[1:]))
        for field in ('source_sha256','declaration_and_body'):
            bad=copy.deepcopy(rows);bad[0][field]='stale';self.assertTrue(continuation14_report.fixture_errors(bad))
        bad=copy.deepcopy(rows);pins=json.loads(bad[0]['dependency_file_pins']);pins[next(iter(pins))]='0'*64;bad[0]['dependency_file_pins']=json.dumps(pins)
        self.assertTrue(continuation14_report.fixture_errors(bad))


    def test_14_previously_qualified_facades_excluded(self):
        previous=[s for s in self.symbols if s['path'] in continuation14_report.TARGETS and s['symbol_id'] not in self.ids]
        self.assertEqual(2,len(previous));self.assertTrue(all(semantic.qualified(s) for s in previous))
        self.assertEqual({'CumulativeDistributionPlot.vb','CategoricalHistogram.vb'},{s['path'].rsplit('/',1)[-1] for s in previous})
        self.assertTrue(continuation14_report.scope_errors(self.symbols,self.decisions,self.queue+previous))

    def test_14_missing_initializer_lambda_event_or_accessor_rejected(self):
        picks=[next(s for s in self.selected if s['kind']==kind) for kind in ('VariableDeclarator','SingleLineFunctionLambdaExpression','SingleLineSubLambdaExpression','GetAccessorBlock','SetAccessorBlock')]
        picks.append(next(s for s in self.selected if s['name']=='Chart_Resize'))
        for s in picks:
            self.assertTrue(continuation14_report.scope_errors(self.symbols,[d for d in self.decisions if d['symbol_id']!=s['symbol_id']],self.queue))
            bad=copy.deepcopy(self.symbols);next(x for x in bad if x['symbol_id']==s['symbol_id'])['body_sha256']='0'*64;self.assertTrue(continuation14_report.scope_errors(bad,self.decisions,self.queue))

    def test_14_prior_order_change_rejected_even_with_exact_object_bytes(self):
        original='[{"symbol_id":"a"},{"symbol_id":"b"}]'
        self.assertEqual([],continuation14_report.prior_decision_errors(original,original))
        self.assertTrue(continuation14_report.prior_decision_errors('[{"symbol_id":"b"},{"symbol_id":"a"}]',original))


    def test_14_same_line_lambdas_have_distinct_span_identity(self):
        same=[s for s in self.selected if s['path'].endswith('PsmLovePlotExcel.vb') and s['start_line']=='88' and s['kind']=='SingleLineFunctionLambdaExpression']
        self.assertEqual(2,len(same));self.assertNotEqual(same[0]['symbol_id'],same[1]['symbol_id']);self.assertNotEqual(same[0]['span_start'],same[1]['span_start'])
        d=copy.deepcopy(next(d for d in self.decisions if d['symbol_id']==same[0]['symbol_id']));d['source_span_start']=same[1]['span_start'];self.assertTrue(continuation14_report.scope_errors(self.symbols,self.changed(d),self.queue))
    def test_14_missing_force_add_or_index_path_rejected(self):
        with tempfile.TemporaryDirectory(prefix='beshstat14-index-') as directory:
            root=pathlib.Path(directory);rel='docs/architecture/WindowsMigrationLedger/continuation14.md';p=root/rel;p.parent.mkdir(parents=True);p.write_bytes(b'audit')
            record=dict(path=rel,bytes=5,sha256=hashlib.sha256(p.read_bytes()).hexdigest());index=dict(artifacts=[record]);paths=[rel,'docs/architecture/WindowsMigrationLedger/index.json']
            self.assertEqual([],continuation03_report.verify_index(root,index,paths));self.assertTrue(continuation03_report.verify_index(root,index,paths[1:]));p.unlink();self.assertTrue(continuation03_report.verify_index(root,index,paths))
    def test_14_prior13_evidence_same_size_corruption_rejected(self):
        with tempfile.TemporaryDirectory(prefix='beshstat14-history-') as directory:
            root=pathlib.Path(directory);rel='docs/architecture/WindowsMigrationLedger/continuation13.md';p=root/rel;p.parent.mkdir(parents=True);p.write_bytes(b'old13')
            records=[dict(path=rel,bytes=5,sha256=hashlib.sha256(p.read_bytes()).hexdigest())];self.assertEqual([],continuation14_report.historical_errors(root,records));p.write_bytes(b'Old13');self.assertTrue(continuation14_report.historical_errors(root,records))


    def test_14_force_add_proposals_individual_complete_and_no_caches(self):
        rel='docs/architecture/WindowsMigrationLedger/continuation14.md';index={'artifacts':[{'path':rel}]};paths=[rel,'docs/architecture/WindowsMigrationLedger/index.json']
        commands='\n'.join('git add -f -- '+json.dumps(p) for p in sorted(paths))
        self.assertEqual([],continuation14_report.tracking_errors(index,paths,commands))
        self.assertTrue(continuation14_report.tracking_errors(index,paths,commands.replace('git add -f --','git add --',1)))
        self.assertTrue(continuation14_report.tracking_errors(index,paths,commands.splitlines()[0]))
        self.assertTrue(continuation14_report.tracking_errors(index,paths+['scripts/__pycache__/bad.pyc'],commands))


    def test_14_actual_core_fault_boundary_requires_pin_and_owner(self):
        self.assertEqual([],continuation14_report.source_fault_errors(self.symbols,self.decisions))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-14' and 'CoreInfrastructure.vb' in d.get('dependency_pins','')))
        path='BESHStat.Core/src/AppInfrastructure/CoreInfrastructure.vb';pins=json.loads(d['dependency_pins']);pins.pop(path);d['dependency_pins']=json.dumps(pins)
        self.assertTrue(continuation14_report.source_fault_errors(self.symbols,self.changed(d)))
        d=copy.deepcopy(next(d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-14' and 'CoreInfrastructure.vb' in d.get('dependency_pins','')));d['exception_tags']='none'
        self.assertTrue(continuation14_report.source_fault_errors(self.symbols,self.changed(d)))

class Continuation15Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        import continuation15_report
        cls.c=continuation15_report
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv')
        cls.decisions=semantic.read_store('review-decisions.json')
        cls.selected=[d for d in cls.decisions if d.get('checkpoint_id')=='04A.01-continuation-15']

    def test_15_exact_232_groups_and_previously_reviewed_violin_excluded(self):
        self.assertEqual([38,45,46,44,59],[len(g['records']) for g in self.c.GROUPS])
        self.assertEqual(232,len(self.selected))
        self.assertFalse(any(self.c.SOURCE[d['symbol_id']]['name']=='Compute' and self.c.SOURCE[d['symbol_id']]['path'].endswith('/ViolinPlot.vb') for d in self.selected))
        self.assertEqual([],self.c.scope_errors(self.symbols,self.decisions))

    def test_15_missing_duplicate_and_cross_file_ids_rejected(self):
        victim=self.selected[0]
        removed=[d for d in self.decisions if d['symbol_id']!=victim['symbol_id']]
        self.assertTrue(self.c.scope_errors(self.symbols,removed))
        self.assertTrue(self.c.scope_errors(self.symbols,self.decisions+[victim]))
        forged=copy.deepcopy(victim);forged['symbol_id']='fabricated'
        self.assertTrue(self.c.scope_errors(self.symbols,removed+[forged]))

    def test_15_initializer_accessor_constructor_and_lambda_ids_required(self):
        for kind in ('PropertyStatement','VariableDeclarator','ConstructorBlock','GetAccessorBlock','SetAccessorBlock','SingleLineFunctionLambdaExpression'):
            victim=next(d for d in self.selected if d['source_kind']==kind)
            self.assertTrue(self.c.scope_errors(self.symbols,[d for d in self.decisions if d['symbol_id']!=victim['symbol_id']]))

    def test_15_body_signature_profile_span_dependency_drift_rejected(self):
        original=self.selected[0];s=next(s for s in self.symbols if s['symbol_id']==original['symbol_id'])
        for field in ('source_profile','source_span_start','source_span_length','source_identity'):
            d=copy.deepcopy(original);d[field]='stale'
            self.assertTrue(semantic.verify_member_contract(d,s))
        d=copy.deepcopy(original);pins=json.loads(d['dependency_pins']);pins[s['path']]='0'*64;d['dependency_pins']=json.dumps(pins)
        self.assertTrue(semantic.verify_pins([d],'C15 negative'))

    def test_15_rich_member_contract_removal_rejected(self):
        for field in semantic.FIELDS:
            d=copy.deepcopy(self.selected[0]);d[field]='';self.assertFalse(semantic.qualified(d),field)

    def test_15_snapshot_or_refinement_change_rejected(self):
        self.assertEqual([],self.c.snapshot_errors(self.decisions))
        ds=copy.deepcopy(self.decisions);next(d for d in ds if d.get('checkpoint_id')=='04A.01-continuation-15')['source_purpose']='generic host label'
        self.assertTrue(self.c.snapshot_errors(ds))

    def test_15_no_whole_excel_renderer_equivalence(self):
        self.assertFalse(any(d['equivalence_scope']=='whole-method' and 'Excel' in self.c.SOURCE[d['symbol_id']]['containing_type'] for d in self.selected))
        ds=copy.deepcopy(self.decisions);d=next(d for d in ds if d.get('checkpoint_id')=='04A.01-continuation-15' and self.c.SOURCE[d['symbol_id']]['name']=='AddChart');d['equivalence_scope']='whole-method'
        self.assertTrue(self.c.scope_errors(self.symbols,ds))

    def test_15_portable_declaration_body_and_source_pin_drift(self):
        rows=self.c.comparison_rows(self.selected);self.assertEqual([],self.c.comparison_errors(rows))
        for field in ('body_sha256','span_start','source_sha256','body_excerpt'):
            mutated=copy.deepcopy(rows[:1]);mutated[0][field]='stale';self.assertTrue(self.c.comparison_errors(mutated))

    def test_15_operation_members_not_operations_and_existing_extensions(self):
        ops,members=self.c.operations(self.selected)
        self.assertEqual(232,len(members));self.assertGreater(sum(m['member_class']=='candidate' for m in members),len(ops))
        self.assertEqual(4,sum(m['member_class']=='already-ported whole numeric forwarder' for m in members))
        self.assertEqual(15,sum(m['member_class']=='compatibility facade/accessor' for m in members))
        self.assertEqual(49,sum(m['member_class']=='retained host' for m in members))
        self.assertTrue(any(o['operation_id']=='C14-A01-chart-style-policy' and o['status'].startswith('EXTENSION') for o in ops))
        self.assertTrue(any(o['operation_id']=='C14-B02-histogram-axis-geometry' and o['status'].startswith('EXTENSION') for o in ops))
        jitter=next(o for o in ops if o['operation_id']=='C15-A02-observation-jitter');self.assertEqual(2,jitter['member_rows'])

    def test_15_candidate_operation_and_unknown_prior_rejected(self):
        ds=copy.deepcopy(self.selected);d=next(d for d in ds if d['disposition'].startswith('extract-to-'));d['candidate_operation_id']=''
        with self.assertRaises(AssertionError):self.c.operations(ds)
        d['candidate_operation_id']='nonexistent-prior'
        with self.assertRaises(AssertionError):self.c.operations(ds)

    def test_15_alignment_missing_stale_owner_and_runtime_claims_rejected(self):
        rows=self.c.read(self.c.EVIDENCE/'alignment-reviewed.json');self.assertEqual([],self.c.alignment_errors(rows))
        self.assertTrue(self.c.alignment_errors(rows[:1]))
        for key in ('owner','approved_batch','future_fixture'):
            bad=copy.deepcopy(rows);bad[0][key]='';self.assertTrue(self.c.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['runtime_status']='native parity passed';self.assertTrue(self.c.alignment_errors(bad))
        bad=copy.deepcopy(rows);pins=json.loads(bad[0]['source_pins']);pins[next(iter(pins))]='0'*64;bad[0]['source_pins']=json.dumps(pins);self.assertTrue(self.c.alignment_errors(bad))

    def test_15_prior_1662_ordered_literal_bytes_preserved(self):
        old=(self.c.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
        current=(semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig')
        self.assertEqual(1662,len(json.loads(old)))
        self.assertEqual([],self.c.prior.prior_decision_errors(current,old))
        self.assertTrue(self.c.prior.prior_decision_errors(json.dumps(list(reversed(json.loads(current)))),old))

    def test_15_index_and_individual_force_add_guards(self):
        rel='docs/architecture/WindowsMigrationLedger/continuation15.md';paths=[rel,'docs/architecture/WindowsMigrationLedger/index.json'];index={'artifacts':[{'path':rel}]}
        commands='\n'.join('git add -f -- '+json.dumps(p) for p in sorted(paths))
        self.assertEqual([],self.c.prior.tracking_errors(index,paths,commands))
        self.assertTrue(self.c.prior.tracking_errors(index,paths,commands.replace('git add -f','git add',1)))
        self.assertTrue(self.c.prior.tracking_errors(index,paths+['__pycache__/bad.pyc'],commands))

    def test_15_existing_fixture_source_and_body_cannot_drift(self):
        rows=self.c.read(self.c.EVIDENCE/'fixtures-reviewed.json');self.assertEqual([],self.c.fixture_errors(rows))
        self.assertTrue(self.c.fixture_errors(rows[:-1]))
        for field in ('source_sha256','declaration_and_body'):
            bad=copy.deepcopy(rows);bad[0][field]='stale';self.assertTrue(self.c.fixture_errors(bad))

    def test_15_report_preserves_embedded_crlf_and_rejects_byte_drift(self):
        original_out,original_check=self.c.OUT,self.c.CHECK
        try:
            with tempfile.TemporaryDirectory(prefix='beshstat15-exact-text-') as directory:
                self.c.OUT=pathlib.Path(directory);self.c.CHECK=False
                value='"declaration","body"\n"method","first\r\nsecond"\n'
                self.c.write('fixture.csv',value)
                self.assertEqual(value.encode('utf-8'),(self.c.OUT/'fixture.csv').read_bytes())
                self.c.CHECK=True;self.c.write('fixture.csv',value)
                (self.c.OUT/'fixture.csv').write_bytes(value.replace('\r\n','\n').encode('utf-8'))
                with self.assertRaises(AssertionError):self.c.write('fixture.csv',value)
        finally:self.c.OUT,self.c.CHECK=original_out,original_check

class Continuation16EvidenceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        import continuation16_report
        cls.c=continuation16_report
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv')
        cls.decisions=semantic.read_store('review-decisions.json')
        cls.selected=[{**cls.c.SOURCE[d['symbol_id']],**d} for d in cls.decisions if d.get('checkpoint_id')=='04A.01-continuation-16']
        cls.mutation_index=max(i for i,d in enumerate(cls.decisions) if d.get('checkpoint_id')=='04A.01-continuation-16')

    def test_16_exact_groups_and_no_adjacent_credit(self):
        self.assertEqual([38,40,46,33,49,44],[len(g['records']) for g in self.c.GROUPS])
        self.assertEqual(250,len(self.selected));self.assertEqual([],self.c.scope_errors(self.symbols,self.decisions))
        self.assertEqual(3,len({d['path'] for d in self.selected}))
        for g in self.c.GROUPS:
            self.assertTrue(all((int(s['start_line'])<200)==(g['group'] in 'ACE') for s in g['records']))

    def test_16_missing_duplicate_and_foreign_review_rejected(self):
        self.assertTrue(self.c.scope_errors(self.symbols,[d for i,d in enumerate(self.decisions) if i!=self.mutation_index]))
        self.assertTrue(self.c.scope_errors(self.symbols,self.decisions+[self.selected[0]]))
        bad=copy.deepcopy(self.decisions);bad[self.mutation_index]['symbol_id']='foreign'
        self.assertTrue(self.c.scope_errors(self.symbols,bad))

    def test_16_initializers_constructor_lambdas_required(self):
        for kind in ('PropertyStatement','VariableDeclarator','ConstructorBlock','SingleLineFunctionLambdaExpression'):
            victim=next(d for d in self.selected if d['source_kind']==kind)
            self.assertTrue(self.c.scope_errors(self.symbols,[d for d in self.decisions if d['symbol_id']!=victim['symbol_id']]))

    def test_16_exact_source_excerpt_profile_hash_and_span(self):
        for field in ('source_excerpt','source_sha256','source_profile','source_span_start','source_span_length','source_enclosing_symbol_id'):
            bad=copy.deepcopy(self.decisions);bad[self.mutation_index][field]='stale'
            self.assertTrue(self.c.scope_errors(self.symbols,bad),field)

    def test_16_rich_contract_each_field_required(self):
        for field in semantic.FIELDS:
            d=copy.deepcopy(self.selected[0]);d[field]='';self.assertFalse(semantic.qualified(d),field)

    def test_16_whole_numeric_renderer_claim_rejected(self):
        self.assertFalse(any(d['equivalence_scope']=='whole-method' for d in self.selected))
        bad=copy.deepcopy(self.decisions);bad[self.mutation_index]['equivalence_scope']='whole-method'
        self.assertTrue(self.c.scope_errors(self.symbols,bad))

    def test_16_portable_exact_declaration_body_source(self):
        rows=self.c.comparison_rows(self.selected);self.assertEqual([],self.c.shared.comparison_errors(rows))
        for field in ('body_sha256','span_start','source_sha256','body_excerpt'):
            bad=copy.deepcopy(rows[:1]);bad[0][field]='stale';self.assertTrue(self.c.shared.comparison_errors(bad))

    def test_16_snapshots_and_explicit_refinements(self):
        self.assertEqual([],self.c.snapshot_errors(self.decisions))
        bad=copy.deepcopy(self.decisions);bad[self.mutation_index]['source_purpose']='generic template'
        self.assertTrue(self.c.snapshot_errors(bad))

    def test_16_operations_extensions_and_host_separation(self):
        ops,members=self.c.operations(self.selected)
        self.assertEqual(250,len(members));self.assertEqual(6,len(ops))
        self.assertGreater(sum(m['member_class']=='candidate' for m in members),len(ops))
        self.assertTrue(any(m['member_class']=='retained host' for m in members))
        self.assertEqual({'C14-A01-chart-style-policy','C14-A02-xy-chart-specification','C14-B02-histogram-axis-geometry'},{o['operation_id'] for o in ops if o['status'].startswith('EXTENSION')})
        self.assertTrue(any(d['disposition']=='extract-to-Core' for d in self.selected))

    def test_16_missing_operation_and_unknown_extension(self):
        bad=copy.deepcopy(self.selected);d=next(d for d in bad if d['disposition'].startswith('extract-to-'));d['candidate_operation_id']=''
        with self.assertRaises(AssertionError):self.c.operations(bad)
        d['candidate_operation_id']='unverified-old-operation'
        with self.assertRaises(AssertionError):self.c.operations(bad)
        d['candidate_operation_id']='C16-B01-paired-chart-series-specification'
        with self.assertRaises(AssertionError):self.c.operations(bad)

    def test_16_prior1894_exact_literal_order(self):
        old=(self.c.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
        current=(semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig')
        self.assertEqual(1894,len(json.loads(old)))
        self.assertEqual([],self.c.prior.prior_decision_errors(current,old))
        self.assertTrue(self.c.prior.prior_decision_errors(json.dumps(list(reversed(json.loads(current)))),old))

    def test_16_alignment_owner_pin_and_no_runtime_claim(self):
        rows=self.c.read(self.c.EVIDENCE/'alignment-reviewed.json');self.assertEqual([],self.c.shared.alignment_errors(rows))
        for key in ('owner','future_fixture','approved_batch'):
            bad=copy.deepcopy(rows);bad[0][key]='';self.assertTrue(self.c.shared.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['runtime_status']='native PASS';self.assertTrue(self.c.shared.alignment_errors(bad))

    def test_16_fixtures_future_not_run_and_exact_existing_source(self):
        rows=self.c.read(self.c.EVIDENCE/'fixtures-reviewed.json');self.assertEqual([],self.c.fixture_errors(rows))
        bad=copy.deepcopy(rows);next(r for r in bad if not r['source'])['status']='PASS';self.assertTrue(self.c.fixture_errors(bad))
        for field in ('source_sha256','declaration_and_body','body_excerpt_sha256'):
            bad=copy.deepcopy(rows);bad[0][field]='stale';self.assertTrue(self.c.fixture_errors(bad))

    def test_16_exact_crlf_and_mutation(self):
        old_out,old_check=self.c.OUT,self.c.CHECK
        try:
            with tempfile.TemporaryDirectory(prefix='beshstat16-') as directory:
                self.c.OUT=pathlib.Path(directory);self.c.CHECK=False;value='"body"\n"first\r\nsecond"\n'
                self.c.write('pin.csv',value);self.assertEqual(value.encode(),(self.c.OUT/'pin.csv').read_bytes())
                self.c.CHECK=True;self.c.write('pin.csv',value)
                (self.c.OUT/'pin.csv').write_bytes(value.replace('\r\n','\n').encode())
                with self.assertRaises(AssertionError):self.c.write('pin.csv',value)
        finally:self.c.OUT,self.c.CHECK=old_out,old_check

    def test_16_individual_ignored_force_add_guard(self):
        rel='docs/architecture/WindowsMigrationLedger/continuation16.md';paths=[rel,'docs/architecture/WindowsMigrationLedger/index.json'];index={'artifacts':[{'path':rel}]}
        commands='\n'.join('git add -f -- '+json.dumps(p) for p in sorted(paths))
        self.assertEqual([],self.c.prior.tracking_errors(index,paths,commands))
        self.assertTrue(self.c.prior.tracking_errors(index,paths,commands.replace('git add -f','git add',1)))

class Continuation17EvidenceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        import continuation17_report
        cls.c=continuation17_report
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv')
        cls.decisions=semantic.read_store('review-decisions.json')
        cls.selected=[{**cls.c.SOURCE[d['symbol_id']],**d} for d in cls.decisions if d.get('checkpoint_id')=='04A.01-continuation-17']
        cls.mutation_index=max(i for i,d in enumerate(cls.decisions) if d.get('checkpoint_id')=='04A.01-continuation-17')

    def test_17_frozen_live_scope_thresholds_and_kinds(self):
        self.assertEqual([53,51,51,51],[len(g['records']) for g in self.c.GROUPS])
        self.assertEqual(206,len(self.selected));self.assertEqual([],self.c.scope_errors(self.symbols,self.decisions))
        self.assertEqual(2,len({d['path'] for d in self.selected}))
        for g in self.c.GROUPS:
            self.assertTrue(all((int(s['start_line'])<186)==(g['group'] in 'AC') for s in g['records']))
        self.assertEqual({'PropertyStatement','FunctionBlock','SubBlock','VariableDeclarator','ConstructorBlock','PropertyBlock','GetAccessorBlock'},{d['source_kind'] for d in self.selected})

    def test_17_missing_duplicate_foreign_and_wrong_group_rejected(self):
        self.assertTrue(self.c.scope_errors(self.symbols,[d for i,d in enumerate(self.decisions) if i!=self.mutation_index]))
        self.assertTrue(self.c.scope_errors(self.symbols,self.decisions+[self.selected[0]]))
        for field,value in [('symbol_id','foreign'),('review_group','A')]:
            bad=copy.deepcopy(self.decisions);bad[self.mutation_index][field]=value
            self.assertTrue(self.c.scope_errors(self.symbols,bad))

    def test_17_every_executable_kind_required(self):
        for kind in {d['source_kind'] for d in self.selected}:
            victim=next(d for d in self.selected if d['source_kind']==kind)
            self.assertTrue(self.c.scope_errors(self.symbols,[d for d in self.decisions if d['symbol_id']!=victim['symbol_id']]))

    def test_17_exact_source_profile_span_lines_excerpt_and_hash(self):
        for field in ('source_sha256','source_excerpt','source_profile','source_span_start','source_span_length','source_declared_start_line','source_body_end_line','source_enclosing_symbol_id'):
            bad=copy.deepcopy(self.decisions);bad[self.mutation_index][field]='stale'
            self.assertTrue(self.c.scope_errors(self.symbols,bad),field)

    def test_17_rich_member_contract_fields_and_false_export(self):
        for field in semantic.FIELDS:
            d=copy.deepcopy(self.selected[0]);d[field]='';self.assertFalse(semantic.qualified(d),field)
        bad=copy.deepcopy(self.decisions);bad[self.mutation_index]['registration_class']='parsed ExcelFunction export'
        self.assertTrue(self.c.scope_errors(self.symbols,bad))

    def test_17_no_whole_renderer_forward_claim(self):
        self.assertFalse(any(d['equivalence_scope']=='whole-method' for d in self.selected))
        bad=copy.deepcopy(self.decisions);bad[self.mutation_index]['equivalence_scope']='whole-method'
        self.assertTrue(self.c.scope_errors(self.symbols,bad))

    def test_17_actual_member_batch_context_not_generic_family_stamp(self):
        self.assertTrue(all(d['future_crosscutting_batches']==d['future_batch']+'; proposed responsibility only, no implementation authorization' for d in self.selected))
        bad=copy.deepcopy(self.decisions);bad[self.mutation_index]['future_crosscutting_batches']='04A.44 generic UI; 04A.36 all graphics'
        self.assertTrue(self.c.scope_errors(self.symbols,bad))

    def test_17_portable_body_and_comparison_membership(self):
        rows=self.c.comparison_rows(self.selected);self.assertEqual([],self.c.comparison_errors(rows))
        self.assertTrue(self.c.comparison_errors(rows[:-1]))
        for field in ('body_sha256','span_start','source_sha256','body_excerpt'):
            bad=copy.deepcopy(rows);bad[0][field]='stale';self.assertTrue(self.c.comparison_errors(bad))

    def test_17_immutable_group_snapshots(self):
        self.assertEqual([],self.c.snapshot_errors(self.decisions))
        bad=copy.deepcopy(self.decisions);bad[self.mutation_index]['source_purpose']='generic template'
        self.assertTrue(self.c.snapshot_errors(bad))

    def test_17_prior2144_literal_order_preserved(self):
        old=(self.c.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
        current=(semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig')
        self.assertEqual(2144,len(json.loads(old)))
        self.assertEqual([],self.c.c16.prior.prior_decision_errors(current,old))
        self.assertTrue(self.c.c16.prior.prior_decision_errors(json.dumps(list(reversed(json.loads(current)))),old))

    def test_17_previous_checkpoint_mutation_with_appended_tail(self):
        earlier=max(i for i,d in enumerate(self.decisions) if d.get('checkpoint_id')=='04A.01-continuation-16')
        self.assertEqual('04A.01-continuation-17',self.decisions[self.mutation_index]['checkpoint_id'])
        self.assertGreater(self.mutation_index,earlier)
        bad=copy.deepcopy(self.decisions);bad[earlier]['source_excerpt']='stale C16 body despite later C17 records'
        self.assertTrue(self.c.c16.scope_errors(self.symbols,bad))
        self.assertTrue(self.c.c16.scope_errors(self.symbols,[d for i,d in enumerate(self.decisions) if i!=earlier]))

    def test_17_operation_extensions_numeric_host_facade_separation(self):
        ops,members=self.c.operations(self.selected)
        self.assertEqual(206,len(members));self.assertEqual(6,len(ops))
        self.assertEqual(2,sum(m['member_class']=='compatibility facade' for m in members))
        self.assertTrue(any(m['member_class']=='retained host' for m in members))
        self.assertGreater(sum(m['member_class']=='candidate' for m in members),len(ops))
        self.assertEqual({'C14-A01-chart-style-policy','C14-B02-histogram-axis-geometry','C16-A02-graphics-label-format-policy'},{o['operation_id'] for o in ops if o['status'].startswith('EXTENSION')})
        self.assertTrue(any(d['disposition']=='extract-to-Core' for d in self.selected))

    def test_17_operation_missing_and_unverified_duplicate_rejected(self):
        bad=copy.deepcopy(self.selected);d=next(d for d in bad if d['disposition'].startswith('extract-to-'))
        for operation in ('','unknown-old-operation','C17-duplicate-bullet-scale-algorithm'):
            d['candidate_operation_id']=operation
            with self.assertRaises(AssertionError):self.c.operations(bad)

    def test_17_alignment_pin_owner_and_no_false_runtime_claim(self):
        rows=self.c.read(self.c.EVIDENCE/'alignment-reviewed.json');self.assertEqual([],self.c.alignment_errors(rows))
        self.assertTrue(self.c.alignment_errors(rows[:-1]))
        for field in ('owner','approved_batch','future_fixture'):
            bad=copy.deepcopy(rows);bad[0][field]='';self.assertTrue(self.c.alignment_errors(bad))
        bad=copy.deepcopy(rows);bad[0]['runtime_status']='native PASS';self.assertTrue(self.c.alignment_errors(bad))
        bad=copy.deepcopy(rows);pins=json.loads(bad[0]['source_pins']);pins[next(iter(pins))]='0'*64;bad[0]['source_pins']=json.dumps(pins)
        self.assertTrue(self.c.alignment_errors(bad))

    def test_17_fixtures_exact_existing_body_and_future_not_run(self):
        rows=self.c.read(self.c.EVIDENCE/'fixtures-reviewed.json');self.assertEqual([],self.c.fixture_errors(rows))
        self.assertTrue(self.c.fixture_errors(rows[:-1]))
        for field in ('source_sha256','declaration_and_body','body_excerpt_sha256'):
            bad=copy.deepcopy(rows);bad[0][field]='stale';self.assertTrue(self.c.fixture_errors(bad))
        bad=copy.deepcopy(rows);next(r for r in bad if not r['source'])['status']='PASS';self.assertTrue(self.c.fixture_errors(bad))

    def test_17_embedded_crlf_byte_roundtrip_and_mutation(self):
        old_out,old_check=self.c.OUT,self.c.CHECK
        try:
            with tempfile.TemporaryDirectory(prefix='beshstat17-') as directory:
                self.c.OUT=pathlib.Path(directory);self.c.CHECK=False;value='"body"\n"first\r\nsecond"\n'
                self.c.write('pin.csv',value);self.assertEqual(value.encode(),(self.c.OUT/'pin.csv').read_bytes())
                self.c.CHECK=True;self.c.write('pin.csv',value)
                (self.c.OUT/'pin.csv').write_bytes(value.replace('\r\n','\n').encode())
                with self.assertRaises(AssertionError):self.c.write('pin.csv',value)
        finally:self.c.OUT,self.c.CHECK=old_out,old_check

    def test_17_individual_ignored_force_add_guard(self):
        rel='docs/architecture/WindowsMigrationLedger/continuation17.md';paths=[rel,'docs/architecture/WindowsMigrationLedger/index.json'];index={'artifacts':[{'path':rel}]}
        commands='\n'.join('git add -f -- '+json.dumps(p) for p in sorted(paths))
        self.assertEqual([],self.c.c16.prior.tracking_errors(index,paths,commands))
        self.assertTrue(self.c.c16.prior.tracking_errors(index,paths,commands.replace('git add -f','git add',1)))

class Continuation18EvidenceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        import continuation18_report as c
        cls.c=c
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv')
        cls.decisions=semantic.read_store('review-decisions.json')
        cls.selected=[{**c.SOURCE[d['symbol_id']],**d} for d in cls.decisions if d.get('checkpoint_id')=='04A.01-continuation-18']
        cls.indices=[i for i,d in enumerate(cls.decisions) if d.get('checkpoint_id')=='04A.01-continuation-18']

    def test_18_exact_disjoint_six_groups(self):
        self.assertEqual([40,23,11,20,29,21],[len(g['records']) for g in self.c.GROUPS])
        self.assertEqual(144,len(self.selected))
        self.assertEqual([],self.c.scope_errors(self.symbols,self.decisions))
        bounds=[(0,210),(211,540),(541,920),(921,99999),(0,550),(551,99999)]
        for g,(lo,hi) in zip(self.c.GROUPS,bounds):
            self.assertTrue(all(lo<=int(s['start_line'])<=hi for s in g['records']))

    def test_18_missing_duplicate_foreign_each_file(self):
        for suffix in ('SankeyPlotExcel.vb','XYZscatter.vb'):
            i=next(i for i in self.indices if self.c.SOURCE[self.decisions[i]['symbol_id']]['path'].endswith(suffix))
            self.assertTrue(self.c.scope_errors(self.symbols,[d for j,d in enumerate(self.decisions) if i!=j]))
            self.assertTrue(self.c.scope_errors(self.symbols,self.decisions+[self.decisions[i]]))
            bad=copy.deepcopy(self.decisions);bad[i]['symbol_id']='stale-ID'
            self.assertTrue(self.c.scope_errors(self.symbols,bad))

    def test_18_body_hash_utf16_excerpt_and_source_pins(self):
        for field in ('body_sha256','source_excerpt','source_span_start','source_span_length','source_profile','source_sha256','source_body_start_line','source_declared_end_line','source_enclosing_symbol_id'):
            bad=copy.deepcopy(self.decisions);bad[self.indices[-1]][field]='tampered'
            self.assertTrue(self.c.scope_errors(self.symbols,bad),field)

    def test_18_lambdas_accessors_interface_and_initializers_required(self):
        for kind in {d['source_kind'] for d in self.selected}:
            d=next(d for d in self.selected if d['source_kind']==kind)
            self.assertTrue(self.c.scope_errors(self.symbols,[x for x in self.decisions if x['symbol_id']!=d['symbol_id']]))

    def test_18_unverified_whole_core_forwarding_rejected(self):
        bad=copy.deepcopy(self.decisions);bad[self.indices[0]]['equivalence_scope']='whole-method'
        self.assertTrue(self.c.scope_errors(self.symbols,bad))
        self.assertFalse(any(d['equivalence_scope']=='whole-method' for d in self.selected))

    def test_18_com_draw_interface_cannot_be_portable(self):
        i=next(i for i in self.indices if self.c.SOURCE[self.decisions[i]['symbol_id']]['start_line']=='9')
        bad=copy.deepcopy(self.decisions);bad[i]['disposition']='extract-to-Core';bad[i]['candidate_operation_id']='C18-B02-xyz-projected-series-geometry'
        self.assertTrue(self.c.scope_errors(self.symbols,bad))

    def test_18_missing_operation_or_owner_rejected(self):
        i=next(i for i in self.indices if self.decisions[i]['disposition'].startswith('extract-to-'))
        for field in ('candidate_operation_id','owner_role','future_batch'):
            bad=copy.deepcopy(self.decisions);bad[i][field]=''
            self.assertTrue(self.c.scope_errors(self.symbols,bad))
        bad=copy.deepcopy(self.selected);next(d for d in bad if d['disposition'].startswith('extract-to-'))['candidate_operation_id']='unknown-operation'
        with self.assertRaises(AssertionError):self.c.operations(bad)

    def test_18_false_export_registration_rejected(self):
        for field,value in [('registration_class','parsed ExcelFunction export'),('registration_evidence','[{"invented":true}]')]:
            bad=copy.deepcopy(self.decisions);bad[self.indices[0]][field]=value
            self.assertTrue(self.c.scope_errors(self.symbols,bad))

    def test_18_historical2350_literal_order_and_c17_mutation(self):
        old=(self.c.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
        now=(semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig')
        self.assertEqual(2350,len(json.loads(old)))
        self.assertEqual([],self.c.c16.prior.prior_decision_errors(now,old))
        victim=next(d for d in json.loads(old) if d.get('checkpoint_id')=='04A.01-continuation-17')
        altered=now.replace(json.dumps(victim['source_excerpt'],ensure_ascii=False),json.dumps(victim['source_excerpt']+'tampered',ensure_ascii=False),1)
        self.assertNotEqual(now,altered)
        self.assertTrue(self.c.c16.prior.prior_decision_errors(altered,old))
        self.assertTrue(self.c.c16.prior.prior_decision_errors(json.dumps(list(reversed(json.loads(now)))),old))

    def test_18_false_ready_with_ui_control_wiring_rejected(self):
        self.assertTrue(self.c.readiness_errors(dict(status='READY',unreviewed_for_acceptance=1372,unresolved_edge_rows=1604,qualified_full_control_contracts=1)))
        self.assertEqual([],self.c.readiness_errors(dict(status='NOT READY FOR ACCEPTANCE',unreviewed_for_acceptance=1372)))

    def test_18_snapshots_portable_comparisons_and_tamper(self):
        self.assertEqual([],self.c.snapshot_errors(self.decisions))
        bad=copy.deepcopy(self.decisions);bad[self.indices[0]]['source_purpose']='unrecorded'
        self.assertTrue(self.c.snapshot_errors(bad))
        rows=self.c.comparison_rows(self.selected);self.assertEqual([],self.c.comparison_errors(rows))
        self.assertTrue(self.c.comparison_errors(rows[:-1]))
        bad=copy.deepcopy(rows);bad[0]['body_excerpt']='tampered'
        self.assertTrue(self.c.comparison_errors(bad))

    def test_18_alignment_fixtures_no_native_claims(self):
        a=self.c.read(self.c.EVIDENCE/'alignment-reviewed.json');f=self.c.read(self.c.EVIDENCE/'fixtures-reviewed.json')
        self.assertEqual([],self.c.alignment_errors(a));self.assertEqual([],self.c.fixture_errors(f))
        self.assertTrue(self.c.alignment_errors(a[:-1]));self.assertTrue(self.c.fixture_errors(f[:-1]))
        bad=copy.deepcopy(a);bad[0]['runtime_status']='native PASS';self.assertTrue(self.c.alignment_errors(bad))
        bad=copy.deepcopy(f);next(r for r in bad if not r['source'])['status']='PASS';self.assertTrue(self.c.fixture_errors(bad))

class Continuation19SourceControlTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        import continuation19_report as c
        import continuation19_controls as guard
        cls.c,cls.guard=c,guard
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv');cls.controls=catalog.load(catalog.DEFAULT,'ui-controls.csv');cls.edges=catalog.load(catalog.DEFAULT,'ui-wiring.csv')
        cls.decisions=semantic.read_store('review-decisions.json');cls.intents=semantic.read_store('control-review-decisions.json')
        cls.new=[d for d in cls.intents if d.get('checkpoint_id')=='04A.01-continuation-19']

    def verify(self,intents=None,symbols=None,controls=None,edges=None):
        return self.guard.verify(self.intents if intents is None else intents,self.symbols if symbols is None else symbols,self.controls if controls is None else controls,self.edges if edges is None else edges)

    def test_19_exact_member_and_control_scope(self):
        self.assertEqual([24,28,30,40],[len(g['records']) for g in self.c.GROUPS]);self.assertEqual([25,39,77,19],[len(g['controls']) for g in self.c.GROUPS])
        self.assertEqual(122,len(self.c.SOURCE));self.assertEqual(160,len(self.c.CONTROL_SCOPE))
        self.assertEqual([],self.c.scope_errors(self.symbols,self.decisions));self.assertEqual([],self.verify())

    def test_19_nonempty_full_fields_do_not_prove_source(self):
        bad=copy.deepcopy(self.intents);d=next(d for d in bad if d.get('checkpoint_id')=='04A.01-continuation-19');d['defaults_contract']='Enabled False; fabricated but nonempty'
        self.assertTrue(semantic.control_qualified(d));self.assertTrue(self.verify(intents=bad))

    def test_19_missing_constructor_proof_rejected(self):
        bad=copy.deepcopy(self.intents);d=next(d for d in bad if d.get('checkpoint_id')=='04A.01-continuation-19');ids=json.loads(d['required_constructor_initialization_ids']);d['required_constructor_initialization_ids']=json.dumps(ids[1:]);self.assertTrue(self.verify(intents=bad))

    def test_19_removed_handler_target_rejected(self):
        d=next(d for d in self.new if d['control_id']=='BESHStatNG.Ui10ShowLog.btnErrors');ident=d['handler_symbol_ids'].split('; ')[0]
        self.assertTrue(self.verify(symbols=[s for s in self.symbols if s['symbol_id']!=ident]))

    def test_19_stale_constructor_body_or_utf16_span_rejected(self):
        ident=json.loads(self.new[0]['required_constructor_initialization_ids'])[0]
        for field in ('body_sha256','span_start','span_length','signature'):
            bad=copy.deepcopy(self.symbols);next(s for s in bad if s['symbol_id']==ident)[field]='tampered';self.assertTrue(self.verify(symbols=bad),field)

    def test_19_changed_actual_default_or_control_declaration_rejected(self):
        ident=self.new[0]['control_id']
        for field in ('initialization_defaults','type_declaration','line'):
            bad=copy.deepcopy(self.controls);next(c for c in bad if c['control_id']==ident)[field]='changed';self.assertTrue(self.verify(controls=bad),field)

    def test_19_changed_option_or_delegate_binding_rejected(self):
        d=next(d for d in self.new if d['control_id']=='BESHStatNG.UiTwoInputRefedits.cmbDemingVarianceModel');binding=next(b for b in json.loads(d['binding_edge_pins']) if b['kind']=='Handles')
        bad=copy.deepcopy(self.edges);next(e for e in bad if e['edge_id']==binding['edge_id'])['target']='different option callback';self.assertTrue(self.verify(edges=bad))
        self.assertTrue(self.verify(edges=[e for e in self.edges if e['edge_id']!=binding['edge_id']]))

    def test_19_missing_queued_delegate_body_rejected(self):
        victim=next(s for s in self.symbols if s['path'].endswith('Ui99ExportChart.vb') and s['kind']=='MultiLineSubLambdaExpression')
        self.assertTrue(self.verify(symbols=[s for s in self.symbols if s['symbol_id']!=victim['symbol_id']]))

    def test_19_invalid_control_dependency_bytes_rejected(self):
        d=next(d for d in self.new if d['control_id']=='BESHStatNG.Ui10ShowLog.btnErrors');deps=json.loads(d['dependency_pins']);oldroot=self.guard.ROOT
        try:
            with tempfile.TemporaryDirectory(prefix='beshstat19-control-') as directory:
                root=pathlib.Path(directory)
                for path in deps:
                    p=root/path;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes((oldroot/path).read_bytes())
                self.guard.ROOT=root;self.assertEqual([],self.guard.verify([d],self.symbols,self.controls,self.edges))
                path='BESHstatNG/src/UI/Ui10ShowLog.vb';p=root/path;p.write_bytes(p.read_bytes()+b'\n')
                self.assertTrue(any('invalid control dependency' in e and path in e for e in self.guard.verify([d],self.symbols,self.controls,self.edges)))
        finally:self.guard.ROOT=oldroot

    def test_19_deferred_refedit_cannot_be_promoted_by_nonempty_fields(self):
        d=copy.deepcopy(self.new[0]);d['control_id']='BESHStatNG.UiTwoInputRefedits.RefEditDemingSDx'
        self.assertTrue(semantic.control_qualified(d));self.assertTrue(self.verify(intents=self.intents+[d]))

    def test_19_all_old_member_control_wiring_literal_prefixes_preserved(self):
        for name,n in [('review-decisions',2494),('control-review-decisions',54),('wiring-review-decisions',134)]:
            old=(self.c.EVIDENCE/'original/scripts/windows-migration-audit'/(name+'.json')).read_bytes().decode('utf-8-sig');now=(semantic.TOOLING/(name+'.json')).read_bytes().decode('utf-8-sig')
            self.assertEqual(n,len(json.loads(old)));self.assertEqual([],self.c.prefix_errors(now,old));self.assertTrue(self.c.prefix_errors(json.dumps(list(reversed(json.loads(now)))),old))

    def test_19_missing_duplicate_stale_and_false_whole_member_rejected(self):
        own=[d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-19'];victim=own[0]['symbol_id']
        self.assertTrue(self.c.scope_errors(self.symbols,[d for d in self.decisions if d['symbol_id']!=victim]));self.assertTrue(self.c.scope_errors(self.symbols,self.decisions+[own[0]]))
        for field,value in [('source_excerpt','tampered'),('body_sha256','0'*64),('equivalence_scope','whole-method'),('registration_class','parsed ExcelFunction export')]:
            bad=copy.deepcopy(self.decisions);next(d for d in bad if d['symbol_id']==victim)[field]=value;self.assertTrue(self.c.scope_errors(self.symbols,bad),field)

    def test_19_operations_owned_deduplicated_and_existing_batch_preserved(self):
        selected=[d for d in self.decisions if d.get('checkpoint_id')=='04A.01-continuation-19'];ops,members=self.c.operations(selected);self.assertEqual(122,len(members));self.assertLess(len(ops),sum(m['member_class']=='candidate' for m in members))
        self.assertEqual('04A.28',next(o for o in ops if o['operation_id']=='C07-C05-agreement-output-projection')['approved_batch'])
        bad=copy.deepcopy(selected);next(d for d in bad if d['disposition'].startswith('extract-to-'))['candidate_operation_id']='invented-operation'
        with self.assertRaises(AssertionError):self.c.operations(bad)

    def test_19_snapshots_and_incomplete_ready_rejected(self):
        self.assertEqual([],self.c.snapshot_errors(self.decisions));bad=copy.deepcopy(self.decisions);next(d for d in bad if d.get('checkpoint_id')=='04A.01-continuation-19')['source_purpose']='unrecorded edit';self.assertTrue(self.c.snapshot_errors(bad))
        self.assertTrue(self.c.readiness_errors(dict(status='READY',unreviewed_for_acceptance=1250,unresolved_edge_rows=1,qualified_full_control_contracts=148)))

class Continuation20SourceControlTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        import continuation20_report as c
        import continuation20_controls as guard
        cls.c,cls.guard=c,guard
        cls.symbols=catalog.load(catalog.DEFAULT,'symbols.csv');cls.controls=catalog.load(catalog.DEFAULT,'ui-controls.csv');cls.edges=catalog.load(catalog.DEFAULT,'ui-wiring.csv')
        cls.decisions=semantic.read_store('review-decisions.json');cls.intents=semantic.read_store('control-review-decisions.json');cls.own=[d for d in cls.decisions if d.get('checkpoint_id')=='04A.01-continuation-20'];cls.new=[d for d in cls.intents if d.get('checkpoint_id')=='04A.01-continuation-20']

    def verify(self,intents=None,symbols=None,controls=None,edges=None):
        return self.guard.verify(self.intents if intents is None else intents,self.symbols if symbols is None else symbols,self.controls if controls is None else controls,self.edges if edges is None else edges)

    def test_20_exact_live_five_groups(self):
        self.assertEqual([25,19,26,12,39],[len(g['records']) for g in self.c.GROUPS]);self.assertEqual([54,51,20,39,64],[len(g['controls']) for g in self.c.GROUPS]);self.assertEqual(121,len(self.own));self.assertEqual([],self.c.scope_errors(self.symbols,self.decisions));self.assertEqual([],self.verify())

    def test_20_missing_duplicate_foreign_and_initializers_rejected(self):
        for g in self.c.GROUPS:
            victim=g['records'][0]['symbol_id'];self.assertTrue(self.c.scope_errors(self.symbols,[d for d in self.decisions if d['symbol_id']!=victim]))
        self.assertTrue(self.c.scope_errors(self.symbols,self.decisions+[self.own[0]]))
        bad=copy.deepcopy(self.decisions);next(d for d in bad if d['symbol_id']==self.own[0]['symbol_id'])['symbol_id']='foreign-C20';self.assertTrue(self.c.scope_errors(self.symbols,bad))
        initializers=[d for d in self.own if d['source_kind']=='VariableDeclarator'];self.assertEqual(4,len(initializers))
        self.assertEqual(4,len({d['symbol_id'] for d in initializers}))

    def test_20_member_source_body_utf16_profile_and_excerpt(self):
        for key in ('source_excerpt','source_sha256','body_sha256','source_span_start','source_span_length','source_profile','source_enclosing_symbol_id','source_body_end_line'):
            bad=copy.deepcopy(self.decisions);next(d for d in bad if d['symbol_id']==self.own[0]['symbol_id'])[key]='tampered';self.assertTrue(self.c.scope_errors(self.symbols,bad),key)

    def test_20_false_whole_gui_or_export_rejected(self):
        for key,value in [('equivalence_scope','whole-method'),('registration_class','parsed ExcelFunction export'),('registration_evidence','["invented"]')]:
            bad=copy.deepcopy(self.decisions);next(d for d in bad if d['symbol_id']==self.own[0]['symbol_id'])[key]=value;self.assertTrue(self.c.scope_errors(self.symbols,bad))

    def test_20_all_prior_literals_and_twelve_c19_deferrals_preserved(self):
        for name,n in [('review-decisions',2616),('control-review-decisions',201),('wiring-review-decisions',315)]:
            old=(self.c.EVIDENCE/'original/scripts/windows-migration-audit'/(name+'.json')).read_bytes().decode('utf-8-sig');now=(semantic.TOOLING/(name+'.json')).read_bytes().decode('utf-8-sig');self.assertEqual(n,len(json.loads(old)));self.assertEqual([],self.c.prefix_errors(now,old));self.assertTrue(self.c.prefix_errors(json.dumps(list(reversed(json.loads(now)))),old))
        before=self.c.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger/continuation19-control-contracts.csv';self.assertEqual(before.read_bytes(),(catalog.DEFAULT/'continuation19-control-contracts.csv').read_bytes())
        self.assertEqual(12,len(self.c.a.read(self.c.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger/continuation19-count-deltas.json')['deferred_control_ids']))

    def test_20_universal_newline_normalization_is_not_literal_preservation(self):
        old=(self.c.EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
        normalized=old.replace('\r\n','\n');self.assertNotEqual(old,normalized);self.assertEqual(json.loads(old),json.loads(normalized));self.assertTrue(self.c.prefix_errors(normalized,old))

    def test_20_immutable_snapshots_and_explicit_operation_refinements(self):
        self.assertEqual([],self.c.snapshot_errors(self.decisions));bad=copy.deepcopy(self.decisions);next(d for d in bad if d['symbol_id']==self.own[0]['symbol_id'])['source_purpose']='unrecorded edit';self.assertTrue(self.c.snapshot_errors(bad))
        changes=self.c.a.read(self.c.EVIDENCE/'member-source-refinements.json');self.assertEqual(8,len(changes));self.assertTrue(all(r['after']['candidate_operation_id']=='C08-A03-planning-spill-projection' and r['after']['future_batch']=='04A.28' for r in changes))

    def test_20_existing_operation_owner_batch_and_missing_candidate(self):
        ops,members=self.c.operations(self.own);self.assertEqual(121,len(members));self.assertLess(len(ops),sum(m['member_class']=='candidate' for m in members))
        for op,batch in self.c.EXTENSIONS.items():self.assertEqual(batch,next(r for r in ops if r['operation_id']==op)['approved_batch'])
        for field,value in [('candidate_operation_id',''),('owner_role',''),('future_batch','04A.75')]:
            bad=copy.deepcopy(self.own);next(d for d in bad if d.get('candidate_operation_id')=='C08-A03-planning-spill-projection')[field]=value
            with self.assertRaises(AssertionError):self.c.operations(bad)

    def test_20_bogus_nonempty_or_duplicate_control_rejected(self):
        bad=copy.deepcopy(self.intents);d=next(d for d in bad if d.get('checkpoint_id')=='04A.01-continuation-20');d['defaults_contract']='All modes identical and checked true';self.assertTrue(semantic.control_qualified(d));self.assertTrue(self.verify(intents=bad));self.assertTrue(self.verify(intents=self.intents+[self.new[0]]))

    def test_20_constructor_initializecomponent_dispose_proof_required(self):
        for field in ('required_constructor_initialization_ids','source_witnesses'):
            bad=copy.deepcopy(self.intents);d=next(d for d in bad if d.get('checkpoint_id')=='04A.01-continuation-20');rows=json.loads(d[field]);d[field]=json.dumps(rows[1:]);self.assertTrue(self.verify(intents=bad))
        victim=next(s for s in self.symbols if s['path'].endswith('/Ui8Proportions.Designer.vb') and s['name']=='InitializeComponent');self.assertTrue(self.verify(symbols=[s for s in self.symbols if s['symbol_id']!=victim['symbol_id']]))

    def test_20_lost_real_handler_or_addhandler_binding_rejected(self):
        d=next(d for d in self.new if d['control_id']=='BESHStatNG.Ui8Proportions.cbHypothesisType');b=next(e for e in json.loads(d['binding_edge_pins']) if e['kind']=='AddHandler')
        self.assertTrue(self.verify(edges=[e for e in self.edges if e['edge_id']!=b['edge_id']]))
        bad=copy.deepcopy(self.edges);next(e for e in bad if e['edge_id']==b['edge_id'])['target']='wrong default/mode delegate';self.assertTrue(self.verify(edges=bad))
        victim=next(s for s in self.symbols if s['path'].endswith('/Ui8Proportions.vb') and s['name']=='HypothesisTypeChanged');self.assertTrue(self.verify(symbols=[s for s in self.symbols if s['symbol_id']!=victim['symbol_id']]))

    def test_20_wrong_constructor_default_or_dynamic_mode_source_rejected(self):
        bad=copy.deepcopy(self.controls);next(c for c in bad if c['control_id']=='BESHStatNG.Ui8Proportions.optSingle')['initialization_defaults']='Checked=False';self.assertTrue(self.verify(controls=bad))
        for name in ('.ctor','ApplyIndependentProportionsLayout'):
            bad=copy.deepcopy(self.symbols);next(s for s in bad if s['path'].endswith('/Ui12SampleSizeTtestSingleProp.vb') and s['name']==name)['body_sha256']='0'*64;self.assertTrue(self.verify(symbols=bad))

    def test_20_missing_real_validation_or_output_path_rejected(self):
        for path,name in [('Ui82x2.vb','GetResultWriter'),('Ui12SampleSizeTtestSingleProp.vb','CheckData'),('Ui12SampleSizeTtestSingleProp.vb','btnSaveToSheet_Click'),('Ui4Cox.vb','ValidateExpandedInitialValuesCount')]:
            victim=next(s for s in self.symbols if s['path'].endswith('/'+path) and s['name']==name);self.assertTrue(self.verify(symbols=[s for s in self.symbols if s['symbol_id']!=victim['symbol_id']]))
        bad=copy.deepcopy(self.intents);next(d for d in bad if d.get('checkpoint_id')=='04A.01-continuation-20')['control_flow_pins']='[]';self.assertTrue(self.verify(intents=bad))

    def test_20_cox_closure_cancel_and_expanded_count_source_required(self):
        for name in ('RegressionForm_FormClosing','BeginRegressionComputation','EndRegressionComputation','BuildExpandedCoxInputs','BuildExpandedSurvivalRecords'):
            victim=next(s for s in self.symbols if s['path'].endswith('/Ui4Cox.vb') and s['name']==name);self.assertTrue(self.verify(symbols=[s for s in self.symbols if s['symbol_id']!=victim['symbol_id']]))
        bad=copy.deepcopy(self.intents);next(d for d in bad if d.get('control_id')=='BESHStatNG.Ui4Cox.tbInitValues')['form_lifecycle_method_ids']='[]';self.assertTrue(self.verify(intents=bad))
        d=next(d for d in self.own if self.c.SOURCE[d['symbol_id']]['path'].endswith('/Ui4Cox.vb') and self.c.SOURCE[d['symbol_id']]['name']=='btCalculate_Click');body=d['source_excerpt'];self.assertLess(body.index('BuildExpandedCoxInputs(MyData'),body.index('ValidateExpandedInitialValuesCount(fitVarNames.Length)'))

    def test_20_actual_copied_dependency_bytes_rejected(self):
        d=next(d for d in self.new if d['control_id']=='BESHStatNG.Ui8Proportions.cbHypothesisType');deps=json.loads(d['dependency_pins']);old=self.guard.ROOT
        try:
            with tempfile.TemporaryDirectory(prefix='beshstat20-control-') as directory:
                root=pathlib.Path(directory)
                for path in deps:
                    p=root/path;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes((old/path).read_bytes())
                self.guard.ROOT=root;self.assertEqual([],self.guard.verify([d],self.symbols,self.controls,self.edges));p=root/'BESHstatNG/src/UI/Ui8Proportions.vb';p.write_bytes(p.read_bytes().replace(b'SelectedIndex = 0',b'SelectedIndex = 1',1));self.assertTrue(self.guard.verify([d],self.symbols,self.controls,self.edges))
        finally:self.guard.ROOT=old

    def test_20_incomplete_controls_cannot_be_promoted_by_filled_fields(self):
        for ident in ('BESHStatNG.Ui82x2.RefEdit1_WorksheetData','BESHStatNG.Ui4Cox.ProgressBar1','BESHStatNG.Ui4Cox.ckResidualPlots'):
            d=copy.deepcopy(self.new[0]);d['control_id']=ident;self.assertTrue(semantic.control_qualified(d));self.assertTrue(self.verify(intents=self.intents+[d]))

    def test_20_incomplete_acceptance_cannot_be_ready(self):
        self.assertTrue(self.c.readiness_errors(dict(status='READY',unreviewed_for_acceptance=1129,unresolved_edge_rows=1,qualified_full_control_contracts=353)));self.assertEqual([],self.c.readiness_errors(dict(status='NOT READY FOR ACCEPTANCE',unreviewed_for_acceptance=1129)))

from test_continuation21 import Continuation21SourceControlTests
from test_continuation22 import Continuation22SourceControlTests
from test_continuation23 import Continuation23Tests
from test_continuation24 import Continuation24Tests
from test_continuation25 import Continuation25Tests
from test_continuation26 import Continuation26Tests
from test_continuation27 import Continuation27Tests
from test_continuation28 import Continuation28Tests
from test_continuation29 import Continuation29Tests

if __name__ == "__main__":
    unittest.main(verbosity=2)
