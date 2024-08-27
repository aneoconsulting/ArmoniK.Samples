#!/bin/bash
ln -sf /usr/lib/x86_64-linux-gnu/libnvidia-ml.so.535.183.01 /usr/lib/x86_64-linux-gnu/libnvidia-ml.so.1
ln -sf /usr/lib/x86_64-linux-gnu/libcuda.so.545.23.08 /usr/lib/x86_64-linux-gnu/libcuda.so.1
sudo -E -u armonikuser python3 /app/worker.py
