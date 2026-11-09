"""Validate the Phase4.3A documentation package without changing source,SQL orprivileges."""
from pathlib import Path
import hashlib,json,re,subprocess,sys
BASE='d5ebc2acc18265786bae5af96ef3ba58c223e522'
PACKAGE=Path(__file__).resolve().parent
ROOT=PACKAGE.parents[2]
REQUIRED=['Executive_Decision_Summary.md','Architecture_Decision_Register.md','Security_Policy_Decision_Register.md','D01_Runtime_Trust_Boundary_ADR.md','D02_D03_D05_D06_Authentication_ADR.md','D04_RBAC_Governance_ADR.md','D04_Operation_Permission_Matrix.md','D04_Seed_Custody_Decision.md','D09_Transaction_Authority_ADR.md','D09_Writer_Coverage_Matrix.md','D10_Audit_Archive_Restore_ADR.md','Database_Architecture_Decision.md','Database_Compatibility_Matrix.md','B01_B05_Decision_Dependency_Matrix.md','Phase4_3B_Implementation_Backlog.md','Phase4_3A_Final_Assessment.md','Phase4_3A_Baseline_Inventory.md']
def check(condition,message):
 if not condition:raise AssertionError(message)
def main():
 check(all((PACKAGE/name).is_file() for name in REQUIRED),'Required document missing')
 records=json.loads((PACKAGE/'Decision_Records.json').read_text(encoding='utf-8'))['records']
 expected={f'D{i:02}' for i in range(1,13)}|{f'E{i:02}' for i in range(1,9)}
 check(len(records)==20 and {r['id'] for r in records}==expected,'Decision denominator/IDs mismatch')
 check(all(r['current_status']=='PENDING' and r['approval_status']=='PENDING' for r in records),'Unsubstantiated status change')
 check(all(r['approval_date'] is None and r['approval_evidence'] is None and r['named_approver'] is None and r['selected_option'] is None for r in records),'Approval fields unexpectedly filled')
 source=json.loads((PACKAGE/'Input_Manifest.json').read_text(encoding='utf-8'))
 for item in source['files']:
  parent=ROOT if item['origin']=='OFFICIAL_CHECKOUT' else ROOT.parent.parent
  path=parent/item['path'];check(path.is_file(),f'Input missing:{item["origin"]}/{item["path"]}')
  check(hashlib.sha256(path.read_bytes()).hexdigest()==item['sha256'],f'Input changed:{item["path"]}')
 catalog=ROOT.parent.parent/'docs/auth-d12-backend-implementation/Dynamic_RBAC_Permission_Catalog.md'
 baseline=set(re.findall(r'^\| `([^`]+)` \|',catalog.read_text(encoding='utf-8-sig'),re.M));check(len(baseline)==25,'Unexpected baseline permission catalog')
 mappings=json.loads((PACKAGE/'Mapping_Proposals.json').read_text(encoding='utf-8'));check(mappings['not_runtime_configuration'] is True,'Proposal incorrectly executable')
 rows=mappings['rows'];check(len(rows)==69,'Unexpected operation proposal count')
 check({r['PermissionCode'] for r in rows if r['PermissionCode'] in baseline}==baseline,'Baseline catalog coverage mismatch')
 for row in rows:
  code=row['PermissionCode'];check('*' not in code,'Wildcard permission proposal')
  if code=='ied.configuration.write':check(row['CurrentStatus'].startswith('MANDATORY_DENY'),'OT deny not preserved')
  elif code not in baseline and not code.startswith('—'):check(row['CurrentStatus'].startswith('NEW_CODE_PROPOSED_NOT_CATALOGUED'),'New permission treated as existing/approved')
 d01=(PACKAGE/'D01_Runtime_Trust_Boundary_ADR.md').read_text(encoding='utf-8')
 check(set(re.findall(r'^\| A(\d\d) \|',d01,re.M))=={f'{i:02}' for i in range(1,22)},'A01–A21 ownership map incomplete')
 for name in REQUIRED:
  text=(PACKAGE/name).read_text(encoding='utf-8');check(BASE in text,'Missing/corrupt immutable baseline SHA:'+name)
  check('NOT_PROVIDED' in text and 'PENDING' in text,'Missing approval/reviewer scope:'+name)
  check(text.count('```')%2==0,'Unbalanced Markdown fence:'+name)
 # This phase permits changes only in this documentation package,including this artifact test.
 changed=subprocess.check_output(['git','diff','--name-only',BASE],cwd=ROOT,text=True).splitlines()
 check(all(p.startswith('docs/auth-d12-backend-implementation/phase-4.3A/') for p in changed),'Runtime/config/source change outside package')
 untracked=subprocess.check_output(['git','ls-files','--others','--exclude-standard'],cwd=ROOT,text=True).splitlines()
 check(all(p.startswith('docs/auth-d12-backend-implementation/phase-4.3A/') for p in untracked),'Unexpected untracked file outside package')
 # Simple leak patterns complement human review;never print matching content.
 leak=re.compile(r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----|gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}|Password\s*=\s*[^<>\s;]{4,}',re.I)
 for path in PACKAGE.iterdir():
  if path.is_file() and path.name!='verify_package.py':check(not leak.search(path.read_text(encoding='utf-8')),f'Secret pattern in:{path.name}')
 print('PASS:17 required files;20 PENDING decisions;69 proposal rows;25 baseline codes;A01-A21;input hashes;source unchanged;no secret pattern.')
if __name__=='__main__':main()
