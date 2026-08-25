#!/usr/bin/env python3
"""
ArmoniK Benchmark CLI

Command-line tool for benchmarking ArmoniK scheduling overhead.
"""

import os
import sys

# Insert the current directory (where main.py is located) into the PYTHONPATH
current_dir = os.path.abspath(os.path.dirname(__file__))
sys.path.insert(0, current_dir)

from main import main

if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
