using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using Microsoft.Extensions.Logging;


namespace assignment.services
{
    public class ImageService
    {
        private readonly HttpClient _httpClient;

        public ImageService()
        {
            _httpClient = new HttpClient();
        }
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

        public async Task<byte[]?> GetImage(ILogger log)
        {
            string picsumUrl = "https://picsum.photos/600/800";
            HttpResponseMessage response = await _httpClient.GetAsync(picsumUrl);

            if (response.IsSuccessStatusCode)
            {
                byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                log.LogInformation("Successfully fetched an image from Lorem picsum.");
                return imageBytes;
            }
            else
            {
                log.LogError("Failed to fetch an image.");
                return null;
            }
        }
    }
}