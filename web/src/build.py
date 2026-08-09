#!/usr/bin/env python3
"""Build the landing page: inline fonts and screenshots into one HTML file.

Run from anywhere:

    python3 web/src/build.py

Writes:
    web/index.html          full document — this is what GitHub Pages serves
    web/src/artifact.html   body-only copy, for publishing as a Claude Artifact

Edit web/src/template.html (text, links, sections), then re-run this script.
"""
import base64
import pathlib
import re
import sys

SRC = pathlib.Path(__file__).resolve().parent
WEB = SRC.parent

ASSETS = {
    "__F_OSWALD__":  ("oswald.woff2",   "font/woff2"),
    "__F_ARCHIVO__": ("archivo.woff2",  "font/woff2"),
    "__F_PLEX__":    ("plex.woff2",     "font/woff2"),
    "__F_PLEXSB__":  ("plexsb.woff2",   "font/woff2"),
    "__IMG_MENU__":  ("menu.jpg",       "image/jpeg"),
    "__IMG_HUB__":   ("hub.jpg",        "image/jpeg"),
    "__IMG_HUD__":   ("crop_hud.jpg",   "image/jpeg"),
    "__IMG_BRIEF__": ("crop_brief.jpg", "image/jpeg"),
}

html = (SRC / "template.html").read_text(encoding="utf-8")

for token, (fname, mime) in ASSETS.items():
    data = base64.b64encode((SRC / "assets" / fname).read_bytes()).decode("ascii")
    html = html.replace(token, f"data:{mime};base64,{data}")

leftover = sorted(set(re.findall(r"__[A-Z_]+__", html)))
if leftover:
    sys.exit(f"unresolved tokens: {leftover}")

page = WEB / "index.html"
page.write_text(html, encoding="utf-8")

body = re.search(r"<!--CONTENT-->(.*)<!--/CONTENT-->", html, re.S)
if not body:
    sys.exit("CONTENT markers missing from template.html")
artifact = SRC / "artifact.html"
artifact.write_text(
    "<title>ChronoShift — Harbiy-muhandislik o'yini</title>\n" + body.group(1).strip(),
    encoding="utf-8",
)

print(f"web/index.html        {page.stat().st_size / 1024:7.0f} KB")
print(f"web/src/artifact.html {artifact.stat().st_size / 1024:7.0f} KB")
