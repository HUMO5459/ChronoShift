using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// Lists the resource files in a folder in a way that survives exporting.
/// </summary>
/// <remarks>
/// In the editor a data folder contains <c>.tres</c> files, so scanning for that
/// extension works. An export rewrites them: text resources are packed as binary
/// <c>.res</c>, and anything remapped gains a <c>.remap</c> suffix. Code that
/// filters on <c>.tres</c> alone therefore finds nothing once exported, and the
/// game starts with no missions, items or tech at all.
/// </remarks>
public static class ResourceFolder
{
    private static readonly string[] Extensions = { ".tres", ".res" };

    /// <summary>Full resource paths for every data file in <paramref name="folder"/>.</summary>
    public static List<string> Paths(string folder)
    {
        var paths = new List<string>();
        if (!DirAccess.DirExistsAbsolute(folder))
        {
            GD.PushWarning($"ResourceFolder: missing folder {folder}");
            return paths;
        }

        foreach (string file in DirAccess.GetFilesAt(folder))
        {
            // A remapped resource is listed under its original name plus .remap;
            // loading the name without that suffix is what actually resolves.
            string name = file.EndsWith(".remap") ? file.GetBaseName() : file;

            bool known = false;
            foreach (string extension in Extensions)
            {
                if (name.EndsWith(extension))
                {
                    known = true;
                    break;
                }
            }

            if (known)
            {
                paths.Add($"{folder}/{name}");
            }
        }

        paths.Sort();
        return paths;
    }
}
