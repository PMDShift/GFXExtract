using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace GFXExtract
{
    public class SpriteExtractor
    {
        private readonly string path;
        private readonly string outputPath;

        int frameWidth = -1;
        int frameHeight = -1;

        public SpriteExtractor(string path, string outputPath) {
            this.path = path;
            this.outputPath = outputPath;
        }

        public void Extract() {
            Dictionary<string, int[]> formData = new Dictionary<string, int[]>();

            using (var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite)) {
                using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create)) {

                    using (var fileStream = File.OpenRead(path)) {
                        using (BinaryReader reader = new BinaryReader(fileStream)) {
                            int formCount = reader.ReadInt32();

                            for (int i = 0; i < formCount; i++) {
                                // Read the form name
                                string formName = reader.ReadString();

                                int[] formIntData = new int[2];

                                // Load form position
                                formIntData[0] = reader.ReadInt32();
                                // Load form size
                                formIntData[1] = reader.ReadInt32();

                                // Add form data to collection
                                formData.Add(formName, formIntData);
                            }

                            var offset = fileStream.Position;

                            foreach (var formKvp in formData) {
                                var formName = formKvp.Key;
                                var formInt = formKvp.Value;

                                // Jump to the correct position
                                fileStream.Seek(offset + formInt[0], SeekOrigin.Begin);

                                foreach (FrameType frameType in Enum.GetValues(typeof(FrameType))) {
                                    if (!FrameTypeHelper.IsFrameTypeDirectionless(frameType)) {
                                        for (var i = 0; i < 8; i++) {
                                            var direction = GetAnimIntDir(i);

                                            var image = ExtractFrameImage(reader, frameType, direction);

                                            if (image != null) {
                                                var entry = archive.CreateEntry($"Forms/{formName}/{frameType}-{direction}.png");
                                                using (var entryStream = entry.Open()) {
                                                    image.SaveAsPng(entryStream);
                                                }
                                            }
                                        }
                                    } else {
                                        var image = ExtractFrameImage(reader, frameType, Direction.Down);

                                        if (image != null) {
                                            var entry = archive.CreateEntry($"Forms/{formName}/{frameType}-Down.png");
                                            using (var entryStream = entry.Open()) {
                                                image.SaveAsPng(entryStream);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var metaEntry = archive.CreateEntry("Meta.xml");
                    using (var metaEntryStream = metaEntry.Open()) {
                        var options = new XmlWriterSettings()
                        {
                            Indent = true,
                            IndentChars = "    "
                        };
                        using (var xmlWriter = XmlWriter.Create(metaEntryStream, options)) {
                            xmlWriter.WriteStartDocument();

                            xmlWriter.WriteStartElement("FrameData");
                            xmlWriter.WriteElementString("FrameWidth", frameWidth.ToString());
                            xmlWriter.WriteElementString("FrameHeight", frameHeight.ToString());
                            xmlWriter.WriteEndElement();

                            xmlWriter.WriteEndDocument();
                        }
                    }
                }
            }
        }

        private Image<Rgba32> ExtractFrameImage(BinaryReader reader, FrameType frameType, Direction direction) {
            int frameCount = reader.ReadInt32();

            int size = reader.ReadInt32();
            if (size > 0) {
                byte[] imgData = reader.ReadBytes(size);

                var image = Image.Load(imgData);

                if (frameWidth == -1 && frameHeight == -1) {
                    frameWidth = image.Width / frameCount;
                    frameHeight = image.Height;
                }

                return image;
            } else {
                return null;
            }
        }

        private Direction GetAnimIntDir(int dir) {
            switch (dir) {
                case 0:
                    return Direction.Down;
                case 1:
                    return Direction.Left;
                case 2:
                    return Direction.Up;
                case 3:
                    return Direction.Right;
                case 4:
                    return Direction.DownLeft;
                case 5:
                    return Direction.UpLeft;
                case 6:
                    return Direction.UpRight;
                case 7:
                    return Direction.DownRight;
                default:
                    return Direction.Down;
            }
        }
    }
}
