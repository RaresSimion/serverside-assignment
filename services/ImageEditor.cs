using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using System.IO;
using System.Numerics;

namespace assignment.services
{
    public class ImageEditor
    {
        public static Stream AddTextToImage(Stream imageStream, params (string text, (float x, float y) position, int fontSize, string colorHex)[] texts)
        {
            var memoryStream = new MemoryStream();
            var image = Image.Load(imageStream);

            image.Mutate(img =>
            {
                var textGraphicsOptions = new TextGraphicsOptions
                {
                    TextOptions = { WrapTextWidth = image.Width - 10 }
                };

                foreach (var (text, (x, y), fontSize, colorHex) in texts)
                {
                    var font = SystemFonts.CreateFont("Verdana", fontSize);
                    var color = Rgba32.ParseHex(colorHex);
                    var textSize = TextMeasurer.Measure(text, new RendererOptions(font));

                    var backgroundRectangle = new RectangleF(x, y, textSize.Width, textSize.Height * 2);
                    img.Fill(Color.White, backgroundRectangle);

                    img.DrawText(textGraphicsOptions, text, font, color, new PointF(x, y));
                }
            });

            image.SaveAsPng(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
