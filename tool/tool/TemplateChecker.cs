using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenCvSharp;
using System.Windows.Forms;

public static class TemplateChecker
{
    private static readonly string ProjectRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, @"..\..\..\")
    );

    // Giảm overhead bằng cách cache template đã load
    private static readonly Dictionary<string, Mat> TemplateCache = new();

    /// <summary>
    /// Kiểm tra xem template có còn hiển thị trên màn hình không.
    /// </summary>
    public static bool IsTemplateVisible(int x, int y, string templateName, double threshold = 0.5)
    {
        string fullTemplatePath = Path.Combine(ProjectRoot, "tmp", $"{templateName}.png");

        if (!File.Exists(fullTemplatePath))
        {
            Console.WriteLine($"[ERR] Không tìm thấy file template: {fullTemplatePath}");
            return false;
        }

        // ⚡ Dùng cache để tránh load file từ disk mỗi lần
        if (!TemplateCache.TryGetValue(templateName, out Mat? template))
        {
            template = Cv2.ImRead(fullTemplatePath, ImreadModes.Color);
            TemplateCache[templateName] = template;
        }

        // ✅ Resize template về đúng 500x500
        const int targetSize = 500;
        //Mat templateForMatch = new Mat();
        //Cv2.Resize(template, templateForMatch, new OpenCvSharp.Size(template.Width > targetSize ? targetSize : template.Width, template.Height > targetSize ? targetSize : template.Height), 0, 0, InterpolationFlags.Lanczos4);

        using var screenMat = CaptureScreenRegion(x,y, targetSize, targetSize);

        using var result = new Mat();
        Cv2.MatchTemplate(screenMat, template, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out double maxVal);

        Console.WriteLine($"[CHECK] Template '{templateName}' match={maxVal:F3}");

        return maxVal >= threshold;
    }

    /// <summary>
    /// Chụp màn hình nhanh hơn, tránh MemoryStream
    /// </summary>
    private static Mat CaptureScreenRegion(int x, int y, int width = 500, int height = 500)
    {
        // Đảm bảo không vượt khỏi màn hình
        var screen = Screen.PrimaryScreen.Bounds;
        int rx = Math.Max(0, x - width / 2);
        int ry = Math.Max(0, y - height / 2);
        int rw = Math.Min(width, screen.Width - rx);
        int rh = Math.Min(height, screen.Height - ry);

        var rect = new Rectangle(rx, ry, rw, rh);
        using var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
        using (var g = Graphics.FromImage(bmp))
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size);

        // Dùng FromPixelData thay vì constructor cũ
        var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                   ImageLockMode.ReadOnly,
                                   PixelFormat.Format24bppRgb);

        var mat = Mat.FromPixelData(bmp.Height, bmp.Width, MatType.CV_8UC3, bmpData.Scan0, bmpData.Stride);
        var clone = mat.Clone();
        bmp.UnlockBits(bmpData);

        return clone;
    }


}
