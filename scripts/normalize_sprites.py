#!/usr/bin/env python3
"""
normalize_sprites.py
====================
Resize every sprite-sheet PNG in Assets/Art/SpriteSheets/ to a uniform target
height, then update the matching JSON metadata so GifSpriteSheetProcessor can
re-slice everything correctly.

Usage (run from the project root):
    python3 scripts/normalize_sprites.py            # target height = 200 px
    python3 scripts/normalize_sprites.py 150         # custom target height

After running this script, execute in Unity:
    make reprocess-sprites
or via the menu:
    PvZ > Art > Process All Sprite Sheets
"""

import json
import math
import os
import subprocess
import sys

# ---------------------------------------------------------------------------
# Config
# ---------------------------------------------------------------------------

TARGET_H = int(sys.argv[1]) if len(sys.argv) > 1 else 200
SHEET_ROOT = os.path.join("Assets", "Art", "SpriteSheets")

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


def even(n: int) -> int:
    """Round up to nearest even integer (safe for all image formats)."""
    return int(math.ceil(n / 2)) * 2


def resize_png(src: str, dst: str, new_w: int, new_h: int) -> bool:
    """Resize src PNG to new_w x new_h using ffmpeg with Lanczos filter."""
    result = subprocess.run(
        [
            "ffmpeg",
            "-y",
            "-i",
            src,
            "-vf",
            f"scale={new_w}:{new_h}:flags=lanczos",
            dst,
        ],
        capture_output=True,
    )
    if result.returncode != 0:
        print(f"  ffmpeg error:\n{result.stderr.decode()[:400]}")
    return result.returncode == 0


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------


def main():
    if not os.path.isdir(SHEET_ROOT):
        print(f"ERROR: sprite sheet root not found: {SHEET_ROOT}")
        sys.exit(1)

    changed = 0
    skipped = 0
    errors = 0

    for dirpath, _, filenames in os.walk(SHEET_ROOT):
        for fname in sorted(filenames):
            if not fname.endswith("_sheet.json"):
                continue

            json_path = os.path.join(dirpath, fname)
            png_path = json_path.replace(".json", ".png")

            if not os.path.exists(png_path):
                print(f"SKIP  {fname}  (no PNG found)")
                skipped += 1
                continue

            with open(json_path) as fh:
                info = json.load(fh)

            old_fw = info["frameWidth"]
            old_fh = info["frameHeight"]
            count = info["frameCount"]

            if old_fh == TARGET_H:
                print(f"OK    {fname:45s}  already {old_fw}x{old_fh}")
                skipped += 1
                continue

            # Scale the full sheet width first (keeping it even for image codecs),
            # then derive per-frame width via floor division so frames tile exactly:
            #   new_fw * count <= sheet_new_w  (always true with floor division)
            scale = TARGET_H / old_fh
            sheet_new_w = max(even(count), even(round(old_fw * count * scale)))
            sheet_new_h = TARGET_H
            new_fw = sheet_new_w // count  # floor – guarantees no overflow
            new_fh = TARGET_H

            tmp_path = png_path + ".normalizing.tmp.png"
            print(
                f"RESIZE {fname:43s}  {old_fw}x{old_fh} -> {new_fw}x{new_fh}  "
                f"(sheet {sheet_new_w}x{sheet_new_h})  ",
                end="",
                flush=True,
            )

            ok = resize_png(png_path, tmp_path, sheet_new_w, sheet_new_h)
            if not ok:
                print("FAILED")
                errors += 1
                if os.path.exists(tmp_path):
                    os.remove(tmp_path)
                continue

            # Commit: replace original with resized version.
            os.replace(tmp_path, png_path)

            # Update JSON metadata.
            info["frameWidth"] = new_fw
            info["frameHeight"] = new_fh
            with open(json_path, "w") as fh:
                json.dump(info, fh, indent=2)

            print("OK")
            changed += 1

    print()
    print(
        f"Done. {changed} resized, {skipped} already correct/skipped, {errors} errors."
    )
    if changed:
        print(
            "Next step: run 'make reprocess-sprites' to rebuild Unity sprite slices and animations."
        )


if __name__ == "__main__":
    main()
