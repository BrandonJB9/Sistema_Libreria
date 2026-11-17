using System.Windows.Forms;

namespace Presentacion
{
    internal enum EstiloDialogo
    {
        Normal,
        Exito,
        Peligro,
        Error
    }

    /// <summary>Conserva los textos y resultados de las confirmaciones del sistema.</summary>
    internal static class DialogoSistema
    {
        public static void Mostrar(Form propietario, string titulo, string mensaje, EstiloDialogo estilo)
        {
            MensajesForms.MostrarTarjeta(propietario, mensaje, titulo, ObtenerIcono(estilo),
                estilo == EstiloDialogo.Exito, ("Entendido", DialogResult.OK));
        }

        public static DialogResult Confirmar(Form propietario, string titulo, string mensaje,
            string textoAceptar, string textoCancelar, EstiloDialogo estilo)
        {
            DialogResult resultado = MensajesForms.MostrarTarjeta(propietario, mensaje, titulo,
                estilo == EstiloDialogo.Normal ? MessageBoxIcon.Question : ObtenerIcono(estilo), false,
                (textoAceptar, DialogResult.Yes), (textoCancelar, DialogResult.No));

            if (resultado != DialogResult.Yes)
                Mostrar(propietario, "Operación cancelada", "No se realizaron cambios.", EstiloDialogo.Normal);
            return resultado;
        }

        private static MessageBoxIcon ObtenerIcono(EstiloDialogo estilo)
        {
            if (estilo == EstiloDialogo.Error) return MessageBoxIcon.Error;
            if (estilo == EstiloDialogo.Peligro) return MessageBoxIcon.Warning;
            return MessageBoxIcon.Information;
        }
    }
}
