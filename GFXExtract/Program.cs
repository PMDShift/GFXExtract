using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;

namespace GFXExtract
{
    class Program
    {
        static int Main(string[] args) {
            var app = new CommandLineApplication(false);

            app.HelpOption("-h|--help|-?");

            app.Command("convert", c =>
            {
                var inputDirectoryOption = c.Option("-i|--input", "Input directory", CommandOptionType.SingleValue);

                c.OnExecute(() =>
                {
                    if (!inputDirectoryOption.HasValue()) {
                        Console.Error.WriteLine("No input directory specified.");
                        return 1;
                    }

                    var inputDirectory = inputDirectoryOption.Value();

                    var outputDirectory = Path.Combine(inputDirectory, "Output");
                    if (!Directory.Exists(outputDirectory)) {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    foreach (var file in Directory.EnumerateFiles(inputDirectory, "*.sprite", SearchOption.TopDirectoryOnly)) {
                        var outputFile = Path.ChangeExtension(Path.Combine(outputDirectory, Path.GetFileName(file)), ".zip");

                        var spriteExtractor = new SpriteExtractor(file, outputFile);

                        spriteExtractor.Extract();
                    }

                    return 0;
                });
            });

            return app.Execute(args);
        }
    }
}
