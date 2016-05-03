﻿using System;
using System.Drawing;

namespace engenious.Content.Pipeline
{
    [ContentImporterAttribute(".bmp", ".jpg", ".png", DisplayName = "Bitmap Importer", DefaultProcessor = "PassthroughProcessor")]
    public class BitmapImporter : ContentImporter<Bitmap>
    {
        public BitmapImporter()
        {
        }

        public override Bitmap Import(string filename, ContentImporterContext context)
        {
            if (!System.IO.File.Exists(filename))
                return null;
            return new Bitmap(filename);
        }
    }
}
