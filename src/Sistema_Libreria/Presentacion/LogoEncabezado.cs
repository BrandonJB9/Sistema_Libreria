using System.Windows.Forms;

namespace Presentacion
{
    internal static class LogoEncabezado
    {
        internal static void Aplicar(Label icono)
        {
            icono.AutoSize = false;
            icono.Text = string.Empty;
            icono.BackgroundImage = Properties.Resources.logo_libreria;
            icono.BackgroundImageLayout = ImageLayout.Zoom;
            icono.AccessibleName = "Logo del Sistema Librería";
        }
    }
}
