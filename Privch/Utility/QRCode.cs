using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace PrivCh.Utility
{
    internal static class QRCode
    {
        public static Result DecodeScreen()
        {
            // copy screen
            Bitmap bitmapScreen = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmapScreen))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, bitmapScreen.Size, CopyPixelOperation.SourceCopy);
            }

            BitmapLuminanceSource sourceScreen = new BitmapLuminanceSource(bitmapScreen);
            BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(sourceScreen));
            bitmapScreen.Dispose();

            QRCodeReader reader = new QRCodeReader();
            Result result = reader.decode(bitmap);

            return result;
        }
    }
}
