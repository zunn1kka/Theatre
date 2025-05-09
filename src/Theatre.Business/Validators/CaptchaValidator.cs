using SkiaSharp;
using System.Text;


namespace Theatre.Business.Validators
{
    public static class CaptchaValidator
    {
        private static Random random = new();
        private static readonly string[] fonts = { "Arial", "Verdana", "Courier New" };
        private static readonly string charSet = "ABDEFGHJKLMNPQRSTUVWXYZabdefghjkmnpqrstuvwxyz23456789";
        public static (string Code, byte[] imageData) GenerateCaptcha(int width = 200,int height = 80)
        {
            var captchaCode = GenerateRandomCode(5 + random.Next(2));
            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.WhiteSmoke);
            AddNoise(canvas, width, height);

            DrawText(canvas, captchaCode, width, height);

            using var stream =new MemoryStream();
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            data.SaveTo(stream);
            
            return (captchaCode, stream.ToArray());
        }
        private static string GenerateRandomCode(int length)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(charSet[random.Next(charSet.Length)]);
            }
            return sb.ToString();
        }

        private static void AddNoise(SKCanvas canvas, int width, int height)
        {
            for(int i = 0; i < 10; i++)
            {
                var paint = new SKPaint
                {
                    Color = GetRandomColor(),
                    StrokeWidth = 1,
                    IsAntialias = true
                };
                canvas.DrawLine(
                    random.Next(width), random.Next(height),
                    random.Next(width), random.Next(height), paint);
            }
            for(int i = 0;i < 100; i++)
            {
                var paint = new SKPaint { Color = GetRandomColor() };
                canvas.DrawPoint(random.Next(width), random.Next(height), paint);
            }
            // Фоновые круги
            for (int i = 0; i < 15; i++)
            {
                var paint = new SKPaint
                {
                    Color = GetRandomLightColor().WithAlpha(50),
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };
                canvas.DrawCircle(
                    random.Next(width), random.Next(height),
                    random.Next(5, 20), paint);
            }

            // Перекрещивающиеся линии
            for (int i = 0; i < 5; i++)
            {
                var paint = new SKPaint
                {
                    Color = GetRandomLightColor(),
                    StrokeWidth = 1 + random.Next(0, 2),
                    IsAntialias = true
                };
                canvas.DrawLine(
                    random.Next(width), random.Next(height),
                    random.Next(width), random.Next(height), paint);
            }
        }
        private static void DrawText(SKCanvas canvas, string text, int width, int height)
        {
            var fontName = fonts[random.Next(fonts.Length)];
            var typeface = SKTypeface.FromFamilyName(fontName);

            var font = new SKFont(typeface, 24)
            {
                Embolden = true,
                SkewX = -0.2f
            };

            var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            };

            for (int i = 0; i < text.Length; i++)
            {
                var x = 10 + i * 30 + random.Next(-3, 3);
                var y = 40 + random.Next(-5, 5);

                canvas.DrawText(text[i].ToString(), x, y, font, paint);
            }
        }
        private static SKColor GetRandomColor()
        {
            return new SKColor(
                (byte)random.Next(150, 255),
                (byte)random.Next(150, 255),
                (byte)random.Next(150, 255));
        }
        private static SKColor GetRandomLightColor()
        {
            return new SKColor(
                (byte)random.Next(150, 255),
                (byte)random.Next(150, 255),
                (byte)random.Next(150, 255));
        }
    }
}

