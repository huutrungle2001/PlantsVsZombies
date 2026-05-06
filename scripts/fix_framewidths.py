#!/usr/bin/env python3
"""
fix_framewidths.py
==================
After a normalize_sprites run, ensure every _sheet.json frameWidth matches
actual_png_width // frameCount so Unity never gets an out-of-bounds slice.
Run from the project root: python3 scripts/fix_framewidths.py
"""

import json
import os
import subprocess

SHEET_ROOT = "Assets/Art/SpriteSheets"
fixed = 0

for dirpath, _, files in os.walk(SHEET_ROOT):
    for fname in sorted(files):
        if not fname.endswith("_sheet.json"):
            continue
        json_path = os.path.join(dirpath, fname)
        png_path = json_path.replace(".json", ".png")
        if not os.path.exists(png_path):
            continue

        with open(json_path) as f:
            info = json.load(f)

        count = info["frameCount"]
        r = subprocess.run(
            [
                "ffprobe",
                "-v",
                "error",
                "-select_streams",
                "v:0",
                "-show_entries",
                "stream=width,height",
                "-of",
                "csv=p=0",
                png_path,
            ],
            capture_output=True,
            text=True,
        )
        parts = r.stdout.strip().split(",")
        if len(parts) < 2:
            continue
        actual_w = int(parts[0])

        correct_fw = actual_w // count
        if info["frameWidth"] != correct_fw:
            print(
                f"FIX  {fname:45s}  frameWidth {info['frameWidth']} -> {correct_fw}  (sheet_w={actual_w}, count={count})"
            )
            info["frameWidth"] = correct_fw
            with open(json_path, "w") as f:
                json.dump(info, f, indent=2)
            fixed += 1
        else:
            print(f"OK   {fname:45s}  frameWidth={correct_fw} correct")

print(f"\n{fixed} JSON files corrected.")
