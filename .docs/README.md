# ArmoniK.Samples Docs

Docs for ArmoniK.Samples

Each sample is assumed to have its own README.md file, those README files are assembled
together with the `tools/preprocess_doc.py` in order to produce the full documentation.

## Installation

> Be aware to be at the root of the repository

```bash
python -m venv .venv-doc
```

Then activate the virtual environment:

```bash
source .venv-doc/bin/activate
```

And install dependencies:

```bash
pip install -r .docs/requirements.txt
```

## Usage

To build the docs locally, run the following command:

```bash
python3 tools/preprocess_doc.py
```

```bash
sphinx-build -M html .docs build
```

Outputs can be found in `build/html/index.html`.
