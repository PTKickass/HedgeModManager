﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgeModManager
{
    public class ModInfo : IniFile
    {
        public string RootDirectory;

        [PropertyIgnore]
        public bool Enabled { get; set; }

        [PropertyIgnore]
        public bool HasUpdates { get; set; }

        [PropertyIgnore]
        public bool SupportsSave => !string.IsNullOrEmpty(SaveFile);

        // Desc
        public string Title { get { return this["Desc"]["Title", string.IsNullOrEmpty(RootDirectory) ? string.Empty : Path.GetFileName(RootDirectory)]; } set { this["Desc"]["Title"] = value; } }

        public List<string> IncludeDirs { get; set; } = new List<string>();

        public string UpdateServer { get { return this["Main"]["UpdateServer", ""]; } set { this["Main"]["UpdateServer", ""] = value; } }

        public string SaveFile { get { return this["Main"]["SaveFile", ""]; } set { this["Main"]["SaveFile", ""] = value; } }

        public string Description { get { return this["Desc"]["Description", "None"]; } set { this["Desc"]["Description"] = value; } }

        public string Version { get { return this["Desc"]["Version", "None"]; } set { this["Desc"]["Version"] = value; } }

        public string Date { get { return this["Desc"]["Date", "None"]; } set { this["Desc"]["Date"] = value; } }

        public string Author { get { return this["Desc"]["Author", "None"]; } set { this["Desc"]["Author"] = value; } }

        public ModInfo()
        {

        }

        public ModInfo(string modPath)
        {
            RootDirectory = modPath;
            using (var stream = File.OpenRead(Path.Combine(modPath, "mod.ini")))
                Read(stream);

        }

        public override void Read(Stream stream)
        {
            base.Read(stream);
            if (!Groups.ContainsKey("Main"))
            {
                var messagebox = new HedgeMessageBox("Invalid Mod Detected!", $"Mod \"{Path.GetFileName(RootDirectory)}\" doesnt contain a Main Group\nThis is required for all ModLoaders!");
                messagebox.AddButton("  Abort Loading  ", () =>
                {
                    messagebox.Close();
                });
                messagebox.ShowDialog();
                throw new Exceptions.ModLoadException(RootDirectory, "Mod doesnt contain a Main Group, Please check mod.ini");
            }
            if (!Groups.ContainsKey("Desc"))
            {
                var messagebox = new HedgeMessageBox("Invalid Mod Detected!", $"Mod \"{Path.GetFileName(RootDirectory)}\" doesnt contain a Desc Group\nThis is required for HedgeModManager to work!");
                messagebox.AddButton("  Abort Loading  ", () =>
                {
                    messagebox.Close();
                });
                messagebox.ShowDialog();
                throw new Exceptions.ModLoadException(RootDirectory, "Mod doesnt contain a Desc Group, Please check mod.ini");
            }

            if (!string.IsNullOrEmpty(this["Main"]["IncludeDirCount", ""]))
            {
                var includeDirCount = int.Parse(this["Main"]["IncludeDirCount", "0"]);
                for (int i = 0; i < includeDirCount; i++)
                {
                    IncludeDirs.Add(this["Main"][$"IncludeDir{i}"]);
                }
            }
            Description = Description.Replace("\\n", "\n");
        }

        public void Save()
        {
            using(var stream = File.Create(Path.Combine(RootDirectory, "mod.ini")))
            {
                Write(stream);
            }
        }
    }
}