using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Presentacion
{
    /// <summary>Presenta confirmaciones y avisos con una tarjeta opaca.</summary>
    public static class MensajesForms
    {
        private static readonly Color Fondo = Color.FromArgb(20, 42, 80);
        private static readonly Color Superficie = Color.FromArgb(14, 31, 61);
        private static readonly Color Texto = Color.FromArgb(221, 232, 249);

        public static DialogResult Mostrar(Form propietario, string mensaje, string titulo,
            MessageBoxButtons botones, MessageBoxIcon icono)
        {
            switch (botones)
            {
                case MessageBoxButtons.OK:
                    return MostrarTarjeta(propietario, mensaje, titulo, icono, false,
                        ("Aceptar", DialogResult.OK));
                case MessageBoxButtons.OKCancel:
                    return MostrarTarjeta(propietario, mensaje, titulo, icono, false,
                        ("Aceptar", DialogResult.OK), ("Cancelar", DialogResult.Cancel));
                case MessageBoxButtons.YesNo:
                    return MostrarTarjeta(propietario, mensaje, titulo, icono, false,
                        ("Sí", DialogResult.Yes), ("No", DialogResult.No));
                case MessageBoxButtons.YesNoCancel:
                    return MostrarTarjeta(propietario, mensaje, titulo, icono, false,
                        ("Sí", DialogResult.Yes), ("No", DialogResult.No), ("Cancelar", DialogResult.Cancel));
                case MessageBoxButtons.RetryCancel:
                    return MostrarTarjeta(propietario, mensaje, titulo, icono, false,
                        ("Reintentar", DialogResult.Retry), ("Cancelar", DialogResult.Cancel));
                case MessageBoxButtons.AbortRetryIgnore:
                    return MostrarTarjeta(propietario, mensaje, titulo, icono, false,
                        ("Anular", DialogResult.Abort), ("Reintentar", DialogResult.Retry), ("Ignorar", DialogResult.Ignore));
                default:
                    return System.Windows.Forms.MessageBox.Show(propietario, mensaje, titulo, botones, icono);
            }
        }

        internal static DialogResult MostrarTarjeta(Form propietario, string mensaje, string titulo,
            MessageBoxIcon icono, bool exito, params (string Texto, DialogResult Resultado)[] opciones)
        {
            bool tienePropietario = propietario != null && !propietario.IsDisposed;
            Rectangle area = (tienePropietario ? Screen.FromControl(propietario) : Screen.FromPoint(Cursor.Position)).WorkingArea;
            int ancho = Math.Min(568, area.Width - 24);
            int anchoTexto = ancho - 56;
            Color acento = ObtenerAcento(icono, exito);

            using (var dialogo = new Form
            {
                Text = titulo,
                StartPosition = tienePropietario ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                BackColor = Fondo,
                ForeColor = Texto,
                Padding = new Padding(1)
            })
            using (var fuenteTitulo = new Font("Segoe UI Semibold", 17F))
            using (var fuenteMensaje = new Font("Segoe UI", 11F))
            using (var fuenteIcono = new Font("Segoe UI Symbol", 18F))
            {
                const TextFormatFlags formato = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix;
                var texto = mensaje ?? string.Empty;
                int altoMensaje = TextRenderer.MeasureText(texto, fuenteMensaje,
                    new Size(anchoTexto - SystemInformation.VerticalScrollBarWidth, int.MaxValue), formato).Height + 8;
                int altoTitulo = Math.Max(48, TextRenderer.MeasureText(titulo ?? string.Empty, fuenteTitulo,
                    new Size(anchoTexto - 64, int.MaxValue), formato).Height + 4);
                int altoVisible = Math.Min(altoMensaje, Math.Max(70, area.Height - altoTitulo - 156));
                int alto = altoTitulo + Math.Max(64, altoVisible) + 124;
                dialogo.ClientSize = new Size(ancho, alto);
                using (GraphicsPath ruta = CrearRuta(new Rectangle(0, 0, ancho, alto), 16))
                    dialogo.Region = new Region(ruta);

                var insignia = new Label
                {
                    Text = ObtenerSimbolo(icono, exito), ForeColor = acento, BackColor = Superficie,
                    Font = fuenteIcono, Size = new Size(48, 48), Location = new Point(26, 24),
                    TextAlign = ContentAlignment.MiddleCenter, UseMnemonic = false
                };
                using (GraphicsPath ruta = CrearRuta(new Rectangle(Point.Empty, insignia.Size), 12))
                    insignia.Region = new Region(ruta);

                var encabezado = new Label
                {
                    Text = titulo, ForeColor = Color.White, Font = fuenteTitulo, UseMnemonic = false,
                    Bounds = new Rectangle(90, 24, ancho - 118, altoTitulo),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                var cuerpo = new Panel
                {
                    Bounds = new Rectangle(28, altoTitulo + 42, anchoTexto, Math.Max(64, altoVisible)),
                    AutoScroll = true, TabStop = false
                };
                cuerpo.Controls.Add(new Label
                {
                    Text = texto, ForeColor = Texto, Font = fuenteMensaje, UseMnemonic = false,
                    Size = new Size(anchoTexto - SystemInformation.VerticalScrollBarWidth, altoMensaje)
                });

                var pie = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom, Height = 66, FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false, Padding = new Padding(18, 11, 18, 7), BackColor = Superficie
                };
                var separador = new Panel
                {
                    BackColor = Color.FromArgb(64, 92, 137),
                    Bounds = new Rectangle(28, alto - 68, anchoTexto, 1)
                };
                for (int indice = 0; indice < opciones.Length; indice++)
                {
                    var opcion = opciones[indice];
                    var boton = new Button
                    {
                        Text = opcion.Texto, DialogResult = opcion.Resultado, AutoSize = true,
                        MinimumSize = new Size(opciones.Length == 3 ? 88 : 104, 38),
                        Margin = new Padding(8, 0, 0, 0), Padding = new Padding(12, 0, 12, 0),
                        FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = fuenteMensaje,
                        BackColor = indice == 0 ? Color.FromArgb(103, 168, 255) : Color.FromArgb(34, 58, 97),
                        ForeColor = indice == 0 ? Color.FromArgb(8, 24, 51) : Color.White,
                        UseVisualStyleBackColor = false
                    };
                    boton.FlatAppearance.BorderSize = 0;
                    pie.Controls.Add(boton);
                    if (indice == 0)
                        dialogo.AcceptButton = boton;
                    if (indice == opciones.Length - 1)
                        dialogo.CancelButton = boton;
                }
                // En una confirmación, Enter y Escape eligen la opción de salida por defecto.
                if (opciones.Length > 1)
                    dialogo.AcceptButton = dialogo.CancelButton;

                dialogo.Controls.Add(insignia);
                dialogo.Controls.Add(encabezado);
                dialogo.Controls.Add(cuerpo);
                dialogo.Controls.Add(pie);
                dialogo.Controls.Add(separador);
                return tienePropietario ? dialogo.ShowDialog(propietario) : dialogo.ShowDialog();
            }
        }

        private static Color ObtenerAcento(MessageBoxIcon icono, bool exito)
        {
            if (exito) return Color.FromArgb(103, 225, 172);
            if (icono == MessageBoxIcon.Error) return Color.FromArgb(255, 150, 158);
            if (icono == MessageBoxIcon.Warning) return Color.FromArgb(255, 209, 127);
            return Color.FromArgb(141, 195, 255);
        }

        private static string ObtenerSimbolo(MessageBoxIcon icono, bool exito)
        {
            if (exito) return "✓";
            if (icono == MessageBoxIcon.Error) return "×";
            if (icono == MessageBoxIcon.Warning) return "!";
            if (icono == MessageBoxIcon.Question) return "?";
            return "ℹ";
        }

        private static GraphicsPath CrearRuta(Rectangle limites, int radio)
        {
            int diametro = radio * 2;
            var ruta = new GraphicsPath();
            ruta.AddArc(limites.Left, limites.Top, diametro, diametro, 180, 90);
            ruta.AddArc(limites.Right - diametro, limites.Top, diametro, diametro, 270, 90);
            ruta.AddArc(limites.Right - diametro, limites.Bottom - diametro, diametro, diametro, 0, 90);
            ruta.AddArc(limites.Left, limites.Bottom - diametro, diametro, diametro, 90, 90);
            ruta.CloseFigure();
            return ruta;
        }
    }
}
