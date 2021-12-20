using ImageGlass.Settings;

namespace ImageGlass;


public partial class FrmMain
{
    /// <summary>
    /// Open an image from file picker
    /// </summary>
    /// <returns></returns>
    private bool IG_OpenFile()
    {
        var of = new OpenFileDialog()
        {
            Multiselect = false,
            CheckFileExists = true,
        };


        if (of.ShowDialog() == DialogResult.OK)
        {
            _viewer.Image = Config.Codec.Load(of.FileName);
            _viewer.CurrentZoom = 1f;

            return true;
        }

        return false;
    }
}

