using System;
using System.Security.Cryptography;
using System.Text;

namespace Datos
{
    /// <summary>Almacena una derivación con sal aleatoria; nunca una contraseña recuperable.</summary>
    internal static class SeguridadContrasenas
    {
        private const string Prefijo = "PBKDF2-SHA256$";
        private const int Iteraciones = 210000;

        internal static string CrearHash(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
                throw new ArgumentException("La contraseña es obligatoria.");
            byte[] sal = new byte[16];
            using (var aleatorio = RandomNumberGenerator.Create()) aleatorio.GetBytes(sal);
            using (var derivador = new Rfc2898DeriveBytes(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256))
                return Prefijo + Iteraciones + "$" + Convert.ToBase64String(sal) + "$" +
                    Convert.ToBase64String(derivador.GetBytes(32));
        }

        internal static bool EsHash(string valor)
        {
            return valor != null && valor.StartsWith(Prefijo, StringComparison.Ordinal);
        }

        internal static bool Verificar(string contrasena, string almacenada)
        {
            if (contrasena == null || almacenada == null) return false;
            // Compatibilidad con cuentas antiguas: se migran después de autenticar correctamente.
            if (!EsHash(almacenada))
                return Comparar(Encoding.UTF8.GetBytes(contrasena), Encoding.UTF8.GetBytes(almacenada));
            string[] partes = almacenada.Split('$');
            int repeticiones;
            if (partes.Length != 4 || !int.TryParse(partes[1], out repeticiones) ||
                repeticiones < 10000 || repeticiones > 1000000) return false;
            try
            {
                byte[] sal = Convert.FromBase64String(partes[2]);
                byte[] esperado = Convert.FromBase64String(partes[3]);
                if (sal.Length != 16 || esperado.Length != 32) return false;
                using (var derivador = new Rfc2898DeriveBytes(contrasena, sal, repeticiones, HashAlgorithmName.SHA256))
                    return Comparar(derivador.GetBytes(32), esperado);
            }
            catch (FormatException) { return false; }
        }

        private static bool Comparar(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diferencia = 0;
            for (int i = 0; i < a.Length; i++) diferencia |= a[i] ^ b[i];
            return diferencia == 0;
        }
    }
}
