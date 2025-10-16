using OpenCvSharp;
using System.Drawing.Imaging;

public static class TemplateHelper
{

    private const int TEMPLATE_SIZE = 500;
    private static readonly string ProjectRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, @"..\..\..\")
    );

    private static readonly string TmpDir = Path.Combine(ProjectRoot, "tmp");
    private static readonly Dictionary<string, Mat> TemplateCache = new();


    /// <summary>
    /// Kiểm tra xem template có còn hiển thị tại vị trí (x,y) không
    /// </summary>
    public static bool IsTemplateVisible(int x, int y, string templateName, double threshold = 0.5)
    {
        string fullPath = Path.Combine(TmpDir, $"{templateName}.png");

        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"[WARN] Template '{templateName}' not exist, can not check yet.");
            return false;
        }

        // Dùng cache nếu đã có
        if (!TemplateCache.TryGetValue(templateName, out Mat? template))
        {
            template = Cv2.ImRead(fullPath, ImreadModes.Color);
            TemplateCache[templateName] = template;
        }

        using var screenMat = CaptureScreenRegion(x, y);
        using var result = new Mat();
        // chỗ này chỉ việc check templateName đã được lưu vào tmp có còn trên màn hình
        Cv2.MatchTemplate(screenMat, template, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out double maxVal);

        Console.WriteLine($"[CHECK] '{templateName}' match={maxVal:F3}");

        return maxVal >= threshold;
    }

    /// <summary>
    /// Chụp lại vùng quanh (x,y) và lưu làm template (tmp/{templateName}.png)
    /// Nếu template đã tồn tại thì sẽ GHI ĐÈ bằng ảnh mới.
    /// </summary>
    public static void CaptureItem(int x, int y, string templateName = "unknown", int size = TEMPLATE_SIZE)
    {
        try
        {
            using var mat = CaptureScreenRegion(x, y, size);
            string path = Path.Combine(TmpDir, $"{templateName}.png");

            // Nếu đã tồn tại thì xóa trước (đảm bảo luôn là ảnh mới nhất)
            if (File.Exists(path))
            {
                Console.WriteLine($"[INFO] Template '{templateName}' is exist.");
            }

            // Ghi lại ảnh mới
            Cv2.ImWrite(path, mat);

            // Cache lại template trong RAM
            TemplateCache[templateName] = mat.Clone();

            Console.WriteLine($"[CAPTURE] Template '{templateName}' saved at {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERR] CaptureItem fail: {ex.Message}");
        }
    }


    /// <summary>
    /// Chụp màn hình vùng quanh (x,y) – tốc độ cao, không cần MemoryStream
    /// </summary>
    private static Mat CaptureScreenRegion(int x, int y, int size = TEMPLATE_SIZE)
    {
        var screen = Screen.PrimaryScreen?.Bounds;

        if (screen.HasValue)
        {
            var screenValue = screen.Value;
            int rx = Math.Max(0, x - size / 2);
            int ry = Math.Max(0, y - size / 2);
            int rw = Math.Min(size, screenValue.Width - rx);
            int rh = Math.Min(size, screenValue.Height - ry);

            var rect = new Rectangle(rx, ry, rw, rh);
            using var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);

            using (var g = Graphics.FromImage(bmp))
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size);

            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb
            );

            var mat = Mat.FromPixelData(bmp.Height, bmp.Width, MatType.CV_8UC3, bmpData.Scan0, bmpData.Stride);
            var clone = mat.Clone();

            bmp.UnlockBits(bmpData);
            return clone;
        }
        return new Mat();
    }
}


