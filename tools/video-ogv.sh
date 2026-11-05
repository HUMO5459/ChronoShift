#!/usr/bin/env bash
# Buxoro 1238 videolari: Torobiy'ning H.264 .mp4 fayllari → Godot o'ynaydigan Ogg Theora (.ogv).
# Godot faqat Theora o'ynaydi (videolar.js → scripts/buxoro/ui/Videolar.cs).
#
#   tools/video-ogv.sh <torobiy/src/assets/video> [ffmpeg]
#
# Sifat: Theora -q:v 6 (0..10) — 1280x720 manba bilan ko'z bilan farqi yo'q (2026-10-21 tekshirildi),
# Vorbis -q:a 4. kirish.mp4 da audio yo'q — ffmpeg uni o'zi o'tkazib yuboradi.
set -euo pipefail

manba="${1:?manba papka: Torobiy src/assets/video}"
ffmpeg="${2:-ffmpeg}"
chiqish="$(cd "$(dirname "$0")/.." && pwd)/assets/video/buxoro"
mkdir -p "$chiqish"

for f in 1 2 3 4 5 6 7 8 9 game-over game-over-1 kirish; do
  echo "→ $f.ogv"
  "$ffmpeg" -v error -y -i "$manba/$f.mp4" -c:v libtheora -q:v 6 -c:a libvorbis -q:a 4 "$chiqish/$f.ogv"
done
ls -la "$chiqish"
