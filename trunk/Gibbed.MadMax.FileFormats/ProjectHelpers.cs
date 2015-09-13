/* Copyright (c) 2015 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gibbed.MadMax.FileFormats
{
    public static class ProjectHelpers
    {
        private static uint FileHasher(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException();
            }

            return source.HashJenkins();
        }

        public static string FileModifier(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException();
            }

            return Path.GetFileName(source).ToLowerInvariant();
        }

        public static ProjectData.HashList<uint> LoadFileLists(
            this ProjectData.Manager manager,
            Action<uint, string, string> extra)
        {
            return manager.LoadLists(
                "*.filelist",
                FileHasher,
                FileModifier,
                extra);
        }

        public static ProjectData.HashList<uint> LoadFileLists(
            this ProjectData.Project project,
            Action<uint, string, string> extra)
        {
            return project.LoadLists(
                "*.filelist",
                FileHasher,
                FileModifier,
                extra);
        }

        public static Dictionary<string, List<string>> LoadDirectoryList(this ProjectData.Manager manager)
        {
            return manager.ActiveProject.LoadDirectoryList();
        }

        public static Dictionary<string, List<string>> LoadDirectoryList(this ProjectData.Project project)
        {
            var mapping = new Dictionary<string, List<string>>();
            if (project == null)
            {
                return mapping;
            }

            var inputPath = Path.Combine(project.ListsPath, "files", "master.dirlist");
            if (File.Exists(inputPath) == false)
            {
                return mapping;
            }

            using (var input = File.OpenRead(inputPath))
            using (var reader = new StreamReader(input))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    if (line.StartsWith(";") == true)
                    {
                        continue;
                    }

                    line = line.Trim();
                    if (line.Length <= 0)
                    {
                        continue;
                    }

                    var parts = line.Split(':');
                    if (parts.Length < 1)
                    {
                        continue;
                    }

                    var source = parts[0];

                    var paths = new List<string>();
                    foreach (var path in parts.Skip(1))
                    {
                        if (string.IsNullOrEmpty(path) == true)
                        {
                            continue;
                        }

                        var name = Path.GetFileName(path);
                        if (name != source)
                        {
                            throw new InvalidOperationException();
                        }

                        paths.Add(path);
                    }

                    if (paths.Count > 0)
                    {
                        mapping.Add(source, paths);
                    }
                }
            }
            return mapping;
        }
    }
}
