import argparse
import os
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path
import xml.etree.ElementTree as ET

def write_inherited_config(path: Path, inherit_from: str, site_url: str, site_dir: str) -> None:
    content = (
        f"INHERIT: {inherit_from}\n\n"
        f"site_url: {site_url}\n"
        f"site_dir: {site_dir}\n"
    )
    path.write_text(content, encoding="utf-8")


def generate_configs(project_root: Path, version: str, base_url: str) -> tuple[Path, Path, str]:
    # user preference: full 4-part version
    version_tag = f"v{version}"  # e.g. v0.0.4.0

    inherit_from = "mkdocs.base.yml"  # or "mkdocs.base.yml" if you rename later

    latest_cfg = project_root / "mkdocs.latest.yml"
    version_cfg = project_root / "mkdocs.version.yml"

    write_inherited_config(
        latest_cfg,
        inherit_from=inherit_from,
        site_url=f"{base_url}/latest/",
        site_dir=".site/latest",
    )

    write_inherited_config(
        version_cfg,
        inherit_from=inherit_from,
        site_url=f"{base_url}/{version_tag}/",
        site_dir=f".site/{version_tag}",
    )

    return latest_cfg, version_cfg, version_tag


def read_version_from_props(props_path: Path) -> str:
    """
    Reads <BeshVersion> from Directory.Build.props.
    Returns the version string, e.g. '0.0.4.0'.
    """
    tree = ET.parse(props_path)
    root = tree.getroot()

    # Try namespaced version first
    ns = {"msb": "http://schemas.microsoft.com/developer/msbuild/2003"}
    version_elem = root.find(".//msb:BeshVersion", ns)

    # If not found, try non-namespaced version
    if version_elem is None:
        version_elem = root.find(".//BeshVersion")

    if version_elem is None:
        raise RuntimeError("Could not find <BeshVersion> in Directory.Build.props")

    return version_elem.text.strip()




def run(cmd, cwd: Path) -> int:
    print(f"\n[RUN] {' '.join(cmd)}")
    print(f"[CWD] {cwd}\n")
    return subprocess.call(cmd, cwd=str(cwd))


def ensure_mkdocs(project_dir: Path) -> None:
    # Try to run `mkdocs --version` from PATH
    rc = run(["mkdocs", "--version"], cwd=project_dir)
    if rc == 0:
        return

    # Fallback: run mkdocs as a Python module
    rc2 = run([sys.executable, "-m", "mkdocs", "--version"], cwd=project_dir)
    if rc2 == 0:
        return

    raise RuntimeError(
        "MkDocs does not seem to be available.\n"
        "Try: pip install mkdocs mkdocs-material\n"
        "Then re-run this script."
    )


def unzip_if_needed(zip_path: Path, out_dir: Path, force: bool) -> None:
    if not zip_path.exists():
        raise FileNotFoundError(f"Zip not found: {zip_path}")

    if out_dir.exists():
        if force:
            print(f"[INFO] Removing existing folder (force): {out_dir}")
            shutil.rmtree(out_dir)
        else:
            print(f"[INFO] Folder already exists, not unzipping: {out_dir}")
            return

    out_dir.mkdir(parents=True, exist_ok=True)
    print(f"[INFO] Unzipping {zip_path} -> {out_dir}")
    with zipfile.ZipFile(zip_path, "r") as z:
        z.extractall(out_dir)


def locate_project_root(folder: Path) -> Path:
    """
    Finds the folder that contains mkdocs.yml.
    Allows you to point at a parent folder.
    """
    folder = folder.resolve()
    if (folder / "mkdocs.base.yml").exists():
        return folder

    for p in folder.rglob("mkdocs.base.yml"):
        return p.parent

    raise FileNotFoundError(
        f"Could not find mkdocs.base.yml in: {folder}\n"
        "Point --project to the folder that contains mkdocs.base.yml."
    )


def main():
    ap = argparse.ArgumentParser(
        description="Run MkDocs for the BESHStatNG help site (serve/build)."
    )
    ap.add_argument(
        "--mode",
        choices=["serve", "build"],
        default="serve",
        help="serve = local preview, build = generate static site",
    )
    ap.add_argument(
        "--project",
        default=".",
        help="Path to the folder containing mkdocs.base.yml (or a parent folder).",
    )
    ap.add_argument(
        "--zip",
        default="",
        help="Optional: path to the MkDocs starter zip to unzip before running.",
    )
    ap.add_argument(
        "--out",
        default="BESHStatNG_Help",
        help="Where to unzip the project (only used if --zip is given).",
    )
    ap.add_argument(
        "--force-unzip",
        action="store_true",
        help="If set, deletes --out folder before unzipping.",
    )
    ap.add_argument(
        "--addr",
        default="127.0.0.1:8000",
        help="Address for mkdocs serve, e.g. 127.0.0.1:8000",
    )
    ap.add_argument( 
        "--props", 
        default="../Directory.Build.props", 
        help="Path to Directory.Build.props to read version from.", 
    )
    ap.add_argument(
        "--base-url",
        default="https://www.beshstat.eu/beshstatng/help",
        help="Base published URL (no trailing slash), e.g. https://www.beshstat.eu/beshstatng/help",
    )

    args = ap.parse_args()

    props_path = Path(args.props).resolve()
    version = read_version_from_props(props_path)

    # Build versioned output folder
    versioned_outdir = f".site/v{version}"


    project_path = Path(args.project)

    # If user provided a zip, unzip to --out and use that as project root
    if args.zip:
        zip_path = Path(args.zip)
        out_dir = Path(args.out)
        unzip_if_needed(zip_path, out_dir, args.force_unzip)
        project_path = out_dir
    
    project_root = locate_project_root(project_path)
    print(f"[INFO] Project root: {project_root}")

    ensure_mkdocs(project_root)
    latest_cfg, version_cfg, version_tag = generate_configs(project_root, version, args.base_url)

    print(f"[INFO] Generated configs:")
    print(f"       - {latest_cfg.name}  -> {args.base_url}/latest/")
    print(f"       - {version_cfg.name} -> {args.base_url}/{version_tag}/")

    if args.mode == "serve":
        rc = run(["mkdocs", "serve", "-f", str(latest_cfg), "-a", args.addr], cwd=project_root)
        if rc != 0:
            rc = run([sys.executable, "-m", "mkdocs", "serve", "-f", str(latest_cfg), "-a", args.addr], cwd=project_root)
        sys.exit(rc)
    
    if args.mode == "build":
        # Build versioned
        rc_v = run(["mkdocs", "build", "-f", str(version_cfg)], cwd=project_root)
        if rc_v != 0:
            rc_v = run([sys.executable, "-m", "mkdocs", "build", "-f", str(version_cfg)], cwd=project_root)
    
        # Build latest
        rc_l = run(["mkdocs", "build", "-f", str(latest_cfg)], cwd=project_root)
        if rc_l != 0:
            rc_l = run([sys.executable, "-m", "mkdocs", "build", "-f", str(latest_cfg)], cwd=project_root)
    
        print("\n[INFO] Build complete.")
        print(f"       Versioned output: .site/{version_tag} -> {args.base_url}/{version_tag}/")
        print(f"       Latest output:    .site/latest       -> {args.base_url}/latest/")
        sys.exit(rc_v or rc_l)




if __name__ == "__main__":
    main()
