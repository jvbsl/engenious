﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ContentTool
{
    public enum Configuration
    {
        Debug,
        Release
    }

    [Serializable()]
    [System.Xml.Serialization.XmlRoot("Content")]
    public class ContentProject : ContentFolder
    {
        public ContentProject()
            : this("Content")
        {
        }

        public ContentProject(string file)
            : base(System.IO.Path.GetFileNameWithoutExtension(file))
        {
            OutputDir = "bin/{Configuration}/";
            References = null;
        }


        private string name;
        [System.ComponentModel.DefaultValue("Content")]
        public override string Name
        { 
            get{ return name; }
            set
            { 
                name = System.IO.Path.GetFileNameWithoutExtension(value);
            }
        }
        [System.ComponentModel.DefaultValue(Configuration.Debug)]
        public Configuration Configuration{ get; set; }
        [System.ComponentModel.DefaultValue("bin/{Configuration}/")]
        public string OutputDir{ get; set; }

        [System.Xml.Serialization.XmlIgnore()]
        [System.ComponentModel.Browsable(false)]
        public string File{get;private set;}

        [System.ComponentModel.Editor(typeof(Dialog.ReferenceCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public List<string> References{ get; set; }


        private static void SearchParents(ContentFolder folder)
        {
            if (folder == null)
                return;
            foreach (var item in folder.Contents)
            {
                item.Parent = folder;
                SearchParents(item as ContentFolder);
            }
        }

        public static ContentProject Load(string filename)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ContentProject));
            using (StreamReader fs = new StreamReader(filename, System.Text.Encoding.UTF8))
            {
                ContentProject proj = (ContentProject)serializer.Deserialize(fs);
                proj.File = filename;
                SearchParents(proj);
                return proj;
            }
        }

        public static void Save(string filename, ContentProject project)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ContentProject));
            using (StreamWriter fs = new StreamWriter(filename, false, System.Text.Encoding.UTF8))
            {
                
                serializer.Serialize(fs, project);
            }

        }
    }
}