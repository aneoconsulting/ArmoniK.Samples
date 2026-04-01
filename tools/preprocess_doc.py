import sys
from pathlib import Path


def find_readmes(root: Path) -> list[Path]:
    """Find all README.md files under root directory."""
    return sorted(root.glob("**/README.md"))


def main() -> None:
    script_path = Path(__file__).parent
    working_dir = script_path.parent
    doc_root = working_dir / ".docs"
    content_path = doc_root / "content"

    index_rst = doc_root / "index.rst"

    # Write header
    with open(index_rst, "w") as f:
        f.write("ArmoniK.Samples documentation\n")
        f.write("=============================\n")

    # Process each language sample
    for sample in ["csharp", "cpp", "java", "python"]:
        src_dir = working_dir / sample

        if not src_dir.exists():
            continue

        # Append section header
        with open(index_rst, "a") as f:
            f.write(f"\n.. toctree::\n")
            f.write(f"  :maxdepth: 1\n")
            f.write(f"  :caption: {sample.capitalize()} Samples\n")
            f.write(f"  :glob:\n\n")

        # Process README files
        for readme_path in find_readmes(src_dir):
            rel_path = readme_path.relative_to(src_dir)
            dest_path = content_path / sample / rel_path

            dest_path.parent.mkdir(parents=True, exist_ok=True)
            dest_path.write_bytes(readme_path.read_bytes())

            with open(index_rst, "a") as f:
                f.write(f"  /content/{sample}/{rel_path}\n")


if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        sys.exit(1)
