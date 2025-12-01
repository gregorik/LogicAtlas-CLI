using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace LogicAtlas_CLI
{
    // USAGE: LogicAtlas-CLI.exe <input_folder> [-o output_name] [--padding 2] [--pivot BC]
    class Program
    {
        public class SpriteNode
        {
            public string Name;
            public string Path;
            public int Width, Height;
            public Rectangle Rect; // Packed position
        }

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("LogicAtlas-CLI v1.0 [GitHub Edition]");
            Console.WriteLine("Headless Sprite Packer by GregOrigin");
            Console.ResetColor();

            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            // --- Default Config ---
            string inputFolder = args[0];
            string outputName = "atlas";
            int padding = 2;
            string pivotMode = "C"; // C=Center, TL=TopLeft, BC=BottomCenter

            // --- Parse Args ---
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-o" && i + 1 < args.Length) outputName = args[++i];
                if (args[i] == "--padding" && i + 1 < args.Length) int.TryParse(args[++i], out padding);
                if (args[i] == "--pivot" && i + 1 < args.Length) pivotMode = args[++i].ToUpper();
            }

            if (!Directory.Exists(inputFolder))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Input directory '{inputFolder}' does not exist.");
                Console.ResetColor();
                return;
            }

            ProcessPack(inputFolder, outputName, padding, pivotMode);
        }

        static void ProcessPack(string folder, string outName, int padding, string pivotMode)
        {
            // 1. Gather Files
            string[] extensions = { "*.png", "*.jpg", "*.bmp" };
            List<SpriteNode> sprites = new List<SpriteNode>();

            foreach (var ext in extensions)
            {
                foreach (var file in Directory.GetFiles(folder, ext))
                {
                    using (var img = Image.FromFile(file))
                    {
                        sprites.Add(new SpriteNode
                        {
                            Name = Path.GetFileNameWithoutExtension(file),
                            Path = file,
                            Width = img.Width,
                            Height = img.Height
                        });
                    }
                }
            }

            if (sprites.Count == 0)
            {
                Console.WriteLine("No images found in folder.");
                return;
            }

            Console.WriteLine($"Found {sprites.Count} sprites. Packing...");

            // 2. Sort by Height (Shelf Algorithm Heuristic)
            sprites = sprites.OrderByDescending(s => s.Height).ToList();

            // 3. Pack
            double totalArea = sprites.Sum(s => s.Width * s.Height);
            int canvasWidth = (int)Math.Max(512, Math.Sqrt(totalArea) * 1.5); // Heuristic width
            int currentX = 0;
            int currentY = 0;
            int rowHeight = 0;

            foreach (var s in sprites)
            {
                if (currentX + s.Width > canvasWidth)
                {
                    currentY += rowHeight + padding;
                    currentX = 0;
                    rowHeight = 0;
                }

                s.Rect = new Rectangle(currentX, currentY, s.Width, s.Height);

                currentX += s.Width + padding;
                rowHeight = Math.Max(rowHeight, s.Height);
            }

            // 4. Determine Final Canvas Size
            int finalW = 0, finalH = 0;
            foreach (var s in sprites)
            {
                finalW = Math.Max(finalW, s.Rect.Right);
                finalH = Math.Max(finalH, s.Rect.Bottom);
            }

            // 5. Render & Export
            using (Bitmap atlas = new Bitmap(finalW, finalH))
            using (Graphics g = Graphics.FromImage(atlas))
            {
                // Clear
                g.Clear(Color.Transparent);

                // Draw
                foreach (var s in sprites)
                {
                    using (var img = Image.FromFile(s.Path))
                    {
                        g.DrawImage(img, s.Rect);
                    }
                }

                // Save Image
                string pngPath = Path.Combine(Environment.CurrentDirectory, outName + ".png");
                atlas.Save(pngPath, ImageFormat.Png);
                Console.WriteLine($"Generated Atlas: {pngPath} ({finalW}x{finalH})");

                // Save Logic (JSON)
                ExportJson(sprites, outName, pivotMode, finalW, finalH);
            }
        }

        static void ExportJson(List<SpriteNode> sprites, string name, string pivotMode, int w, int h)
        {
            // Determine pivot values
            double px = 0.5, py = 0.5; // Default Center
            if (pivotMode == "TL") { px = 0.0; py = 0.0; }
            if (pivotMode == "BC") { px = 0.5; py = 1.0; }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"atlas\": \"{name}.png\",");
            sb.AppendLine($"  \"width\": {w},");
            sb.AppendLine($"  \"height\": {h},");
            sb.AppendLine("  \"sprites\": [");

            for (int i = 0; i < sprites.Count; i++)
            {
                var s = sprites[i];
                sb.AppendLine("    {");
                sb.AppendLine($"      \"name\": \"{s.Name}\",");
                sb.AppendLine($"      \"x\": {s.Rect.X},");
                sb.AppendLine($"      \"y\": {s.Rect.Y},");
                sb.AppendLine($"      \"w\": {s.Rect.Width},");
                sb.AppendLine($"      \"h\": {s.Rect.Height},");
                sb.AppendLine($"      \"pivot_x\": {px.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},");
                sb.AppendLine($"      \"pivot_y\": {py.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}");
                sb.Append("    }");
                if (i < sprites.Count - 1) sb.Append(",");
                sb.AppendLine();
            }

            sb.AppendLine("  ]");
            sb.AppendLine("}");

            string jsonPath = Path.Combine(Environment.CurrentDirectory, name + ".json");
            File.WriteAllText(jsonPath, sb.ToString());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Generated Data:  {jsonPath}");
            Console.ResetColor();
        }

        static void PrintHelp()
        {
            Console.WriteLine("\nUsage:");
            Console.WriteLine("  LogicAtlas-CLI.exe <input_folder> [options]");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("  -o <name>       : Output filename (default 'atlas')");
            Console.WriteLine("  --padding <px>  : Padding between sprites (default 2)");
            Console.WriteLine("  --pivot <mode>  : Global pivot setting. Options: C, TL, BC (default C)");
            Console.WriteLine("\nExample:");
            Console.WriteLine("  LogicAtlas-CLI.exe \"C:\\MySprites\" -o HeroSheet --pivot BC");
        }
    }
}