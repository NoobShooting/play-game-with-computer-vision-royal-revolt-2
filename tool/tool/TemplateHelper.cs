using OpenCvSharp;
using System;
using System.Drawing.Imaging;
using tool;

public static class TemplateHelper
{

    private const int TEMPLATE_SIZE = 500;
    private static readonly string ProjectRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, @"..\..\..\")
    );

    private static readonly string TmpDir = Path.Combine(ProjectRoot, "tmp");
    private static readonly Dictionary<string, Mat> TemplateCache = new();
    public static GameState CheckCurrentFrame()
    {
        try
        {
            var gameInstance = GameApplication.GetInstance();
            var width = gameInstance.width;
            var height = gameInstance.height;
            if (IsTemplateVisible(GameAction.Battle)) return GameState.InMainMenu;
            if (IsTemplateVisible(GameAction.Attack) || IsTemplateVisible(GameAction.Collect))  return GameState.JoiningBattle;
            if (IsTemplateVisible(GameAction.Pause) || IsTemplateVisible(GameAction.RetreatLostGame) || IsTemplateVisible(GameAction.ContinuePauseGame) || IsTemplateVisible(GameAction.ContinueEndGame)) return GameState.InBattle;
            if (IsTemplateVisible(GameAction.Chest) ) return GameState.OpeningChest;
            if (IsTemplateVisible(GameAction.RetreatPauseGame) ) return GameState.Idle;
            return GameState.Idle;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERR] CheckCurrentFrame() lỗi: {ex.Message}");
            return GameState.Idle;
        }
    } 
    public static bool IsTemplateVisible(GameAction action, double threshold = 0.7)
    {
        var templateName = action.GetName();
        var (ix, iy) = GameApplication.ActionCoordinate.GetCoordinate(action);
        string fullPath = Path.Combine(TmpDir, $"{templateName}.png");

        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"[WARN] Template '{templateName}' not exist, can not check yet.");
            return false;
        }

        if (!TemplateCache.TryGetValue(templateName, out Mat? template))
        {
            template = Cv2.ImRead(fullPath, ImreadModes.Color);
            TemplateCache[templateName] = template;
        }

        using var screenMat = CaptureScreenRegion(ix, iy);
        using var result = new Mat();
        Cv2.MatchTemplate(screenMat, template, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out double maxVal);

        Console.WriteLine($"[CHECK] '{templateName}' match={maxVal:F3}");

        return maxVal >= threshold;
    } 
    public static void CaptureItem(GameAction action, int size = TEMPLATE_SIZE)
    {
        try
        {
            var templateName = action.GetName();
            var (ix, iy) = GameApplication.ActionCoordinate.GetCoordinate(action);
            using var mat = CaptureScreenRegion(ix, iy, size);
            string path = Path.Combine(TmpDir, $"{templateName}.png");

            if (File.Exists(path))
            {
                Console.WriteLine($"[INFO] Template '{templateName}' is exist.");
                return;
            }
            Cv2.ImWrite(path, mat);

            TemplateCache[templateName] = mat.Clone();

            Console.WriteLine($"[CAPTURE] Template '{templateName}' saved at {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERR] CaptureItem fail: {ex.Message}");
        }
    } 
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


