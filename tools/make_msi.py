#!/usr/bin/env python3
"""Builds a Windows .msi installer from the exported Windows build.

Godot exports a plain .exe plus a .pck and a folder of .NET assemblies - about
190 files that must stay together. This walks that folder, writes a WiX source
describing every file, and hands it to wixl (msitools), which can produce an MSI
on macOS without needing Windows.

Component GUIDs are derived from the file path, so rebuilding the same version
yields the same GUIDs and Windows treats it as the same product rather than a
parallel install.
"""

import re
import subprocess
import sys
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "build" / "windows"
OUTPUT = ROOT / "build" / "ChronoShift - Setup.msi"
WXS = ROOT / "build" / "installer.wxs"

NAME = "ChronoShift"

# Bump this for every build handed out. The product code is derived from it, and
# the Upgrade block below uses it to remove whatever older version is installed —
# without a bump Windows treats the installer as a repair of the same product.
VERSION = "1.1.0"
MANUFACTURER = "Humoyun Ochilov"
LAUNCHER = "ChronoShift.exe"

# Fixed for the product line: Windows uses UpgradeCode to recognise that a newer
# installer replaces this one rather than installing alongside it.
NAMESPACE = uuid.UUID("6f1d2c7a-5b3e-4a91-8c0d-2e7f4b9a1c63")
UPGRADE_CODE = str(uuid.uuid5(NAMESPACE, "upgrade-code")).upper()
PRODUCT_CODE = str(uuid.uuid5(NAMESPACE, f"product-{VERSION}")).upper()


def ident(prefix: str, value: str) -> str:
    """A WiX identifier: letters, digits and underscores, stable per path."""
    clean = re.sub(r"[^A-Za-z0-9_]", "_", value)
    digest = uuid.uuid5(NAMESPACE, value).hex[:8]
    return f"{prefix}_{clean[:40]}_{digest}"


def guid(value: str) -> str:
    return str(uuid.uuid5(NAMESPACE, value)).upper()


def collect(folder: Path, relative: str = ""):
    """Yields (directory_elements, component_elements, component_ids)."""
    dirs, comps, refs = [], [], []

    for entry in sorted(folder.iterdir(), key=lambda p: (p.is_dir(), p.name)):
        if entry.name == ".DS_Store":
            continue

        rel = f"{relative}/{entry.name}" if relative else entry.name

        if entry.is_dir():
            child_dirs, child_comps, child_refs = collect(entry, rel)
            dirs.append(
                f'<Directory Id="{ident("dir", rel)}" Name="{entry.name}">\n'
                + "\n".join(child_dirs)
                + "\n</Directory>"
            )
            comps.extend(child_comps)
            refs.extend(child_refs)
            continue

        comp_id = ident("cmp", rel)
        # Every file is its own component so a partial install cannot leave a
        # half-written component behind.
        dirs.append(
            f'<Component Id="{comp_id}" Guid="{guid(rel)}" Win64="yes">\n'
            f'  <File Id="{ident("fil", rel)}" Source="{entry}" '
            f'Name="{entry.name}" KeyPath="yes"/>\n'
            f"</Component>"
        )
        refs.append(comp_id)

    return dirs, comps, refs


def main() -> int:
    if not SOURCE.is_dir():
        print(f"missing {SOURCE} - export the Windows build first", file=sys.stderr)
        return 1

    tree, _, refs = collect(SOURCE)

    shortcut_id = ident("cmp", "__shortcuts")
    refs.append(shortcut_id)

    wxs = f"""<?xml version="1.0" encoding="utf-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="{PRODUCT_CODE}" Name="{NAME}" Language="1033" Version="{VERSION}"
           Manufacturer="{MANUFACTURER}" UpgradeCode="{UPGRADE_CODE}">
    <Package InstallerVersion="200" Compressed="yes"
             InstallScope="perMachine" Platform="x64"
             Description="{NAME} {VERSION}" Manufacturer="{MANUFACTURER}"/>
    <Media Id="1" Cabinet="game.cab" EmbedCab="yes"/>

    <!-- Replace an older install instead of sitting beside it. Without this the
         new build lands in a second folder and the Start menu gains a duplicate. -->
    <Upgrade Id="{UPGRADE_CODE}">
      <UpgradeVersion Minimum="0.0.0" IncludeMinimum="yes"
                      Maximum="{VERSION}" IncludeMaximum="no"
                      Property="OLDVERSIONFOUND"/>
    </Upgrade>
    <InstallExecuteSequence>
      <RemoveExistingProducts After="InstallInitialize"/>
    </InstallExecuteSequence>

    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFiles64Folder">
        <Directory Id="INSTALLDIR" Name="{NAME}">
{chr(10).join(tree)}
        </Directory>
      </Directory>

      <Directory Id="ProgramMenuFolder">
        <Directory Id="AppMenuDir" Name="{NAME}">
          <Component Id="{shortcut_id}" Guid="{guid("__shortcuts")}" Win64="yes">
            <Shortcut Id="StartMenuShortcut" Name="{NAME}"
                      Target="[INSTALLDIR]{LAUNCHER}"
                      WorkingDirectory="INSTALLDIR"/>
            <RemoveFolder Id="AppMenuDir" On="uninstall"/>
            <RegistryValue Root="HKCU" Key="Software\\{MANUFACTURER}\\{NAME}"
                           Name="installed" Type="integer" Value="1" KeyPath="yes"/>
          </Component>
        </Directory>
      </Directory>
    </Directory>

    <Feature Id="Complete" Title="{NAME}" Level="1">
{chr(10).join(f'      <ComponentRef Id="{r}"/>' for r in refs)}
    </Feature>
  </Product>
</Wix>
"""
    WXS.write_text(wxs, encoding="utf-8")
    print(f"wxs: {WXS}  ({len(refs)} component)")

    OUTPUT.unlink(missing_ok=True)
    result = subprocess.run(
        ["wixl", "-a", "x64", "-o", str(OUTPUT), str(WXS)],
        capture_output=True, text=True,
    )
    if result.returncode != 0:
        print(result.stdout[-3000:], file=sys.stderr)
        print(result.stderr[-3000:], file=sys.stderr)
        return result.returncode

    size = OUTPUT.stat().st_size / (1024 * 1024)
    print(f"msi: {OUTPUT}  ({size:.0f} MB)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
