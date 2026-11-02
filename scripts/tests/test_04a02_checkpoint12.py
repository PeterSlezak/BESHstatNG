"""C12 tests use exact owner-measured C11 bytes, real read-only Windows identity.

No R or Excel execution. Junction tests create/remove only a temporary link and
target; they never point at an installation or a retained owner attempt.
"""
from contextlib import ExitStack
import copy
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import patch

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('operator12',ROOT/'scripts/04a02/oracle_operator_v6.py')
r=importlib.util.module_from_spec(spec);spec.loader.exec_module(r)
FIXTURE=ROOT/'scripts/tests/fixtures/04a02-c12-retained-c11-stdout.json'
RAW_SHA='f9ca4049ba9be04d2ac405aa35f1fa2d636ee44a5de5eab09ed2e47ca88d855b'
JOB=Path('C:/Users/peter/AppData/Local/BESHStatNG-04A02-C09-reference-20261101-7d4a/c11-startup-20261101-v1')
MANIFEST=ROOT/'docs/architecture/04A02/oracle-proposals/manifest-v6-c12-v1.json'
def namespace_policy():return r.v2.strict_json((ROOT/r.POLICY).read_bytes())
def path_policy():return r.v2.strict_json((ROOT/r.PATH_POLICY).read_bytes())
def actual():return r.v2.strict_json(FIXTURE.read_bytes())
def validate(value):return r.validate_probe(value,JOB,namespace_policy(),path_policy())


class Diagnosis12Tests(unittest.TestCase):
    def test_exact_owner_bytes_negative_for_frozen_v5(self):
        raw=FIXTURE.read_bytes();self.assertEqual(RAW_SHA,hashlib.sha256(raw).hexdigest())
        with self.assertRaisesRegex(r.v5.Refused,'Compiler namespace/package provenance mismatch'):
            r.v5.validate_probe(r.v2.strict_json(raw),JOB,namespace_policy())
        self.assertEqual(raw,FIXTURE.read_bytes())

    def test_exact_owner_bytes_pass_successor_with_real_held_handle_identity(self):
        before=FIXTURE.read_bytes();value=actual();saved=copy.deepcopy(value);proof=validate(value)
        self.assertEqual(3,len(proof));self.assertEqual(saved,value);self.assertEqual(before,FIXTURE.read_bytes())
        self.assertTrue(all(p['held_deny_delete'] for p in proof))
        self.assertEqual({'compiler.namespace_path','compiler.package_path','environment.R_HOME'},{p['field'] for p in proof})

    def test_four_compiler_fields_have_only_package_path_difference(self):
        compiler=actual()['compiler'];package=namespace_policy()['compiler']['package_path']
        expected=dict(namespace_path=package,namespace_version='4.5.1',package_path=package,package_version='4.5.1')
        self.assertEqual(['package_path'],[k for k in expected if compiler[k]!=expected[k]])

    def test_second_real_r_home_alias_was_masked_by_first_v5_error(self):
        value=actual();value['compiler']['package_path']=namespace_policy()['compiler']['package_path']
        with self.assertRaisesRegex(r.v5.Refused,'Private environment escape'):r.v5.validate_probe(value,JOB,namespace_policy())
        value['environment']['R_HOME']=r.R_HOME.as_posix();r.v5.validate_probe(value,JOB,namespace_policy())

    def test_frozen_C11_synthetic_long_fixture_passes_but_did_not_cover_owner_aliases(self):
        value=actual();value['compiler']['package_path']=namespace_policy()['compiler']['package_path'];value['environment']['R_HOME']=r.R_HOME.as_posix()
        r.v5.validate_probe(value,JOB,namespace_policy())
        self.assertNotEqual(actual()['compiler']['package_path'],value['compiler']['package_path'])

    def test_both_authorized_case_pins_keep_original_compiler_and_scientific_policy(self):
        for case in ['C30-SYN-ANOVA','C30-SYN-KW']:
            manifest,decision,_,_,policy=r.pins(MANIFEST,r.digest(MANIFEST),case)
            self.assertEqual('C08-OWNER-20261030-S-R-N-v1',decision)
            self.assertEqual('NO',manifest['P']);self.assertIs(policy['runtime_proposal']['scientific_method_approved'],False)
            self.assertEqual(namespace_policy(),policy['namespace_validation'])


class WindowsIdentity12Tests(unittest.TestCase):
    def check_equivalent(self,path):
        entry=path_policy()['trusted']['compiler']
        with r.identity.same_trusted_directory(path,entry['path'],entry['identity']) as proof:
            self.assertTrue(os.path.samefile(path,entry['path']))
            self.assertEqual(entry['identity'],proof['identity'])
            self.assertEqual(proof['observed_canonical'],proof['expected_canonical'])

    def test_real_owner_short_path_resolves_to_pinned_compiler(self):self.check_equivalent(actual()['compiler']['package_path'])
    def test_real_backslash_spelling_same_identity(self):self.check_equivalent(namespace_policy()['compiler']['package_path'].replace('/','\\'))
    def test_real_case_equivalent_same_identity(self):self.check_equivalent(namespace_policy()['compiler']['package_path'].lower())
    def test_real_uppercase_spelling_same_identity(self):self.check_equivalent(namespace_policy()['compiler']['package_path'].upper())

    def test_different_existing_directory_on_same_volume_is_not_trusted(self):
        entry=path_policy()['trusted']['compiler']
        with self.assertRaisesRegex(r.identity.PathIdentityError,'not the same trusted directory'):
            with r.identity.same_trusted_directory((r.R_HOME/'library/base').as_posix(),entry['path'],entry['identity']):self.fail()

    def test_same_file_ids_are_manifest_pinned_not_just_current_path_names(self):
        entry=copy.deepcopy(path_policy()['trusted']['compiler']);entry['identity']['file_index']+=1
        with self.assertRaisesRegex(r.identity.PathIdentityError,'identity changed'):
            with r.identity.same_trusted_directory(entry['path'],entry['path'],entry['identity']):self.fail()

    def test_native_api_failure_has_no_string_comparison_fallback(self):
        with patch.object(r.identity.WindowsApi,'local_ntfs',side_effect=OSError('injected query failure')),self.assertRaises(r.identity.PathIdentityError):
            with r.identity.same_trusted_directory(r.R_HOME.as_posix(),r.R_HOME.as_posix()):self.fail()

    def test_non_ntfs_volume_is_not_qualified(self):
        api=r.identity.WindowsApi()
        with patch.object(api,'volume',return_value=True),self.assertRaisesRegex(r.identity.PathIdentityError,'NTFS'):api.local_ntfs()

    def test_injected_reparse_metadata_rejects_symlink_and_other_tags(self):
        api=r.identity.WindowsApi()
        def reparse(handle,pointer):
            pointer._obj.attributes=0x410;return True
        with patch.object(api,'create',return_value=123),patch.object(api,'close_checked'),patch.object(api,'info',side_effect=reparse),ExitStack() as stack,self.assertRaisesRegex(r.identity.PathIdentityError,'Reparse'):
            api.open_directory('C:\\link',stack,0)

    def test_real_temporary_junction_leaf_rejected_even_when_target_identity_matches(self):
        with tempfile.TemporaryDirectory() as temp:
            base=Path(temp).resolve();target=base/'target';target.mkdir();alias=base/'alias'
            ps=Path(os.environ['SystemRoot'])/'System32/WindowsPowerShell/v1.0/powershell.exe'
            quote=lambda p:"'"+str(p).replace("'","''")+"'"
            command="$ErrorActionPreference='Stop'; New-Item -ItemType Junction -Path "+quote(alias)+" -Target "+quote(target)+" | Out-Null"
            result=subprocess.run([str(ps),'-NoProfile','-Command',command],cwd=ROOT,capture_output=True,creationflags=subprocess.CREATE_NO_WINDOW)
            self.assertEqual(0,result.returncode,result.stderr.decode(errors='replace'))
            try:
                self.assertTrue(os.path.samefile(alias,target))
                # The sandbox cannot hold the profile ancestor of TemporaryDirectory.
                # Exercise the actual OPEN_REPARSE_POINT leaf guard directly, rather
                # than mislabel an earlier profile Access Denied as junction evidence.
                api=r.identity.WindowsApi();volume=api.local_ntfs()
                with ExitStack() as stack,self.assertRaisesRegex(r.identity.PathIdentityError,'Reparse'):
                    api.open_directory(str(alias),stack,volume)
            finally:
                # Only remove this verified temporary junction itself, never recurse.
                self.assertEqual(base,alias.parent.resolve());self.assertTrue(alias.lstat().st_file_attributes & 0x400)
                os.rmdir(alias)
            self.assertTrue(target.is_dir())


class Isolation12Tests(unittest.TestCase):
    def test_original_compiler_hash_membership_and_loader_are_unchanged(self):r.v5.verify_compiler_files(namespace_policy())

    def test_changed_package_hash_rejected(self):
        policy=namespace_policy();policy['compiler']['files'][0]['sha256']='0'*64
        with self.assertRaisesRegex(r.Refused,'hash drift'):r.v5.verify_compiler_files(policy)

    def test_changed_package_membership_rejected(self):
        policy=namespace_policy();policy['compiler']['files']=[p for p in policy['compiler']['files'] if p['relative_path']!='library/compiler/INDEX']
        with self.assertRaisesRegex(r.Refused,'membership drift'):r.v5.verify_compiler_files(policy)

    def test_missing_compiler_metadata_rejected(self):
        value=actual();value.pop('compiler')
        with self.assertRaisesRegex(r.Refused,'metadata'):validate(value)

    def test_missing_compiler_version_rejected(self):
        value=actual();value['compiler'].pop('package_version')
        with self.assertRaisesRegex(r.Refused,'metadata'):validate(value)

    def test_unapproved_namespace_rejected(self):
        value=actual();value['namespaces'].append('personal')
        with self.assertRaisesRegex(r.Refused,'Unapproved loaded package'):validate(value)

    def test_promoted_manifest_rejected(self):
        value=json.loads(MANIFEST.read_bytes());value['P']='YES'
        with tempfile.TemporaryDirectory() as temp:
            p=Path(temp)/'manifest.json';p.write_bytes(r.canonical(value))
            with self.assertRaises(r.Refused):r.pins(p,r.digest(p),'C30-SYN-ANOVA')

    def test_both_consumed_attempt_names_refused_before_any_r(self):
        with tempfile.TemporaryDirectory() as temp,patch.object(r.subprocess,'run') as launch:
            for name in ['c10-startup-20261101-v1','c11-startup-20261101-v1']:
                with self.subTest(name=name),self.assertRaises(r.Refused):r.probe(MANIFEST,r.digest(MANIFEST),Path(temp),name,Path(temp)/'review','0'*64)
            launch.assert_not_called();self.assertEqual([],list(Path(temp).iterdir()))

    def test_output_collision_retains_existing_failure_and_bytes_metadata(self):
        with tempfile.TemporaryDirectory() as temp,patch.object(r.subprocess,'run') as launch:
            job=Path(temp)/'c12-used';job.mkdir();p=job/'failure.json';p.write_bytes(b'RETAINED');before=p.stat()
            with self.assertRaises(r.Refused):r.probe(MANIFEST,r.digest(MANIFEST),Path(temp),job.name,p,'0'*64)
            self.assertEqual(b'RETAINED',p.read_bytes());self.assertEqual(before.st_mtime_ns,p.stat().st_mtime_ns);launch.assert_not_called()

    def test_exit_zero_alone_never_qualifies_a_pending_or_failed_startup(self):
        with tempfile.TemporaryDirectory() as temp,patch.object(r.subprocess,'run') as launch:
            p=Path(temp)/'go.json';p.write_bytes(r.canonical(dict(status='FAILED_RETAINED_NO_RETRY',exit_code=0)))
            with self.assertRaises(r.Refused):r.qualification(p,r.digest(p),'0'*64,'C30-SYN-ANOVA',Path(temp),{},namespace_policy(),path_policy())
            launch.assert_not_called()

    def test_frozen_v5_manifest_is_not_v6_authority(self):
        p=ROOT/r.FROZEN_MANIFEST
        with self.assertRaises(r.Refused):r.pins(p,r.digest(p),'C30-SYN-ANOVA')


def add_negative(name,mutate,diagnostic=None):
    def test(self):
        value=actual();mutate(value)
        context=self.assertRaisesRegex(r.Refused,diagnostic) if diagnostic else self.assertRaises(r.Refused)
        with context:validate(value)
    setattr(Isolation12Tests,'test_reject_'+name,test)

for field in ['namespace_version','package_version']:
    add_negative('wrong_'+field,lambda v,k=field:v['compiler'].__setitem__(k,'4.6.2'),'compiler.'+field)
for field in ['namespace_path','package_path']:
    for label,bad in [('personal','C:/Users/peter/R/library/compiler'),('site','C:/site-library/compiler'),('other_version','C:/Program Files/R/R-4.6.2/library/compiler'),('spoofed_short','C:/PROGRA~1/R/R-99~9.9/library/compiler'),('unicode_case_spoof','C:/Pr\u043egram Files/R/R-4.5.1/library/compiler')]:
        add_negative(label+'_'+field,lambda v,k=field,b=bad:v['compiler'].__setitem__(k,b),'compiler.'+field)
for label,bad in [('relative','compiler'),('traversal','C:/Program Files/R/R-4.5.1/../R-4.5.1/library/compiler'),('dot','C:/Program Files/R/./R-4.5.1/library/compiler'),('alternate_drive','D:/Program Files/R/R-4.5.1/library/compiler'),('UNC','//server/share/compiler'),('extended','\\\\?\\C:\\Program Files\\R\\R-4.5.1\\library\\compiler'),('device','\\\\.\\C:\\compiler'),('ADS','C:/Program Files/R/R-4.5.1/library/compiler:stream'),('trailing_dot','C:/Program Files/R/R-4.5.1/library/compiler.'),('spoofed_casing','C:/PROGRAM-FILES/R/R-4.5.1/library/compiler')]:
    add_negative(label,lambda v,b=bad:v['compiler'].__setitem__('package_path',b),'compiler.package_path')
for field,bad in [('r','4.6.2'),('stats','4.6.2'),('arch','i386'),('locale','en_US'),('home','C:/personal'),('r_profile','C:/personal/profile'),('r_environ','C:/personal/environ')]:
    add_negative(field,lambda v,k=field,b=bad:v.__setitem__(k,b))
add_negative('library_escape',lambda v:v['libraries'].append('C:/personal/library'))
for field in ['HOME','R_PROFILE_USER','R_ENVIRON_USER','R_LIBS','R_LIBS_USER','R_LIBS_SITE','R_DEFAULT_PACKAGES']:
    add_negative('environment_'+field.lower(),lambda v,k=field:v['environment'].__setitem__(k,'PERSONAL_ESCAPE'))
add_negative('environment_r_home',lambda v:v['environment'].__setitem__('R_HOME','C:/personal'),'environment.R_HOME')

if __name__=='__main__':unittest.main(verbosity=2)
