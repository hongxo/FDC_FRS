"""Portable artifact checks, not runtime/security acceptance or independent approval."""
from pathlib import Path
import hashlib
import json
import subprocess

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
REQUIRED = [
    "Phase4_3B_Baseline_Report.md", "Authentication_Runtime_Report.md", "MFA_Runtime_Report.md",
    "SQL_Authority_Integration_Report.md", "Dynamic_RBAC_Implementation_Report.md",
    "Approval_Workflow_Report.md", "D09_Transaction_Report.md", "D10_Audit_Recovery_Report.md",
    "Human_Machine_Integration_Report.md", "UI_S01_S08_Implementation_Report.md",
    "E2E_Execution_Report.md", "DR01_DR29_Results.md", "Security_Regression_Report.md",
    "Performance_Baseline_Report.md", "Blocker_Closure_Report.md", "Phase4_3B_Final_Assessment.md",
    "GitHub_Delivery_Report.md", "PR_Description.md",
]


def require(condition, message):
    if not condition:
        raise SystemExit(message)


for name in REQUIRED:
    text = (HERE / name).read_text(encoding="utf-8")
    require("Source commit:" in text and "Environment:" in text, f"Missing provenance: {name}")
    require("NOT_PROVIDED" in text and "evidence" in text.lower(), f"Missing review/evidence limit: {name}")

source = json.loads((HERE / "Source_Manifest.json").read_text(encoding="utf-8"))
for entry in source["files"]:
    path = ROOT / entry["path"]
    require(path.resolve().is_relative_to(ROOT), "Source manifest escaped repository")
    # Preserve exact inspected bytes; Git's canonical blob also supports a CRLF checkout.
    content = path.read_bytes()
    if hashlib.sha256(content).hexdigest() != entry["sha256"]:
        blob = subprocess.check_output(["git", "hash-object", str(path)], cwd=ROOT, text=True).strip()
        require(blob == entry["git_blob"], f"Source changed: {entry['path']}")

runs = json.loads((HERE / "evidence/Test_Execution.json").read_text(encoding="utf-8"))["runs"]
iam = [r for r in runs if "/final-iam/" in r["raw_path"]]
center = [r for r in runs if "/final-lab/Fdc.Center.Tests/" in r["raw_path"]]
require(len(iam) == len(center) == 1, "Final execution ambiguous")
require(iam[0]["counters"]["passed"] == "243" and iam[0]["counters"]["failed"] == "0", "IAM count mismatch")
require(center[0]["counters"]["passed"] == "22" and center[0]["counters"]["failed"] == "0", "Center count mismatch")
require(any(r["counters"]["failed"] != "0" for r in runs), "Initial failures must remain recorded")
dr = (HERE / "DR01_DR29_Results.md").read_text(encoding="utf-8")
require(sum(line.startswith("| DR") for line in dr.splitlines()) == 29, "Incomplete DR matrix")
require("29 BLOCKED" in dr, "Full DR acceptance status changed")
package = json.loads((HERE / "Package_Manifest.json").read_text(encoding="utf-8"))
for entry in package["files"]:
    path = (HERE / entry["path"]).resolve()
    require(path.is_relative_to(HERE), "Package manifest escaped directory")
    content = path.read_bytes()
    require(hashlib.sha256(content).hexdigest() == entry["sha256"] or
            hashlib.sha256(content.replace(b"\r\n", b"\n")).hexdigest() == entry["lf_sha256"],
            f"Artifact changed: {entry['path']}")
print("PASS:18 required reports; source/artifact hashes; actual243 IAM/22 Center; initial FAIL preserved;29 DR BLOCKED. No acceptance/approval implied.")
