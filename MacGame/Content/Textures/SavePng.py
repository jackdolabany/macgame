#!/usr/bin/env python3
"""
pdn_watch.py — Watches multiple .pdn files and exports named layers as PNG on every save.

Usage:
    python pdn_watch.py <file.pdn> [\"Layer Name\"] <file2.pdn> [\"Layer Name\"] ...

If no layer name follows a .pdn, it exports the flattened image for that file.

Examples:
    python pdn_watch.py A.pdn "Sprite" B.pdn "Background" C.pdn
    python pdn_watch.py A.pdn B.pdn C.pdn   (flattens all three)

Requirements:
    pip install pypdn watchdog pillow
"""

import sys
import time
import pypdn
from PIL import Image
from pathlib import Path
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler


def export_layer(pdn_path: Path, layer_name: str | None):
    try:
        img = pypdn.read(str(pdn_path))
        out_path = pdn_path.with_suffix(".png")

        if layer_name is None:
            flat = img.flatten(asByte=True)
            result = Image.fromarray(flat)
        else:
            layer = next(
                (l for l in img.layers if l.name.lower() == layer_name.lower()),
                None
            )
            if layer is None:
                names = [l.name for l in img.layers]
                print(f"  ✗ Layer '{layer_name}' not found in {pdn_path.name}. Available: {names}")
                return
            result = Image.fromarray(layer.image)

        result.save(out_path)
        print(f"  ✓ Exported → {out_path.name}  ({result.width}×{result.height})")

    except Exception as e:
        print(f"  ✗ Error reading {pdn_path.name}: {e}")


class PdnHandler(FileSystemEventHandler):
    def __init__(self, pdn_path, layer_name):
        self.pdn_path = Path(pdn_path).resolve()
        self.layer_name = layer_name
        self._last_export = 0

    def on_modified(self, event):
        if Path(event.src_path).resolve() != self.pdn_path:
            return
        now = time.time()
        if now - self._last_export < 1.5:
            return
        self._last_export = now
        print(f"\n[{time.strftime('%H:%M:%S')}] Change detected in {self.pdn_path.name}")
        export_layer(self.pdn_path, self.layer_name)


def parse_args(args):
    """Parse alternating pdn/layer-name arguments into a list of (path, layer) tuples."""
    watches = []
    i = 0
    while i < len(args):
        pdn_path = Path(args[i])
        if not str(args[i]).endswith(".pdn"):
            print(f"Expected a .pdn file, got: {args[i]}")
            sys.exit(1)
        # Check if the next arg is a layer name (doesn't end in .pdn)
        if i + 1 < len(args) and not args[i + 1].endswith(".pdn"):
            layer_name = args[i + 1]
            i += 2
        else:
            layer_name = None
            i += 1
        watches.append((pdn_path, layer_name))
    return watches


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print('Usage: python pdn_watch.py <file.pdn> ["Layer Name"] <file2.pdn> ["Layer Name"] ...')
        sys.exit(1)

    watches = parse_args(sys.argv[1:])

    observer = Observer()

    for pdn_path, layer_name in watches:
        if not pdn_path.exists():
            print(f"File not found: {pdn_path}")
            sys.exit(1)
        label = f'layer "{layer_name}"' if layer_name else "flattened"
        print(f"Watching {pdn_path.name} → {label}")
        export_layer(pdn_path, layer_name)
        handler = PdnHandler(pdn_path, layer_name)
        observer.schedule(handler, str(pdn_path.parent), recursive=False)

    print("\nWatching for changes. Press Ctrl+C to stop.\n")
    observer.start()

    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        observer.stop()
    observer.join()
    print("Stopped.")