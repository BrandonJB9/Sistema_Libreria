using System;
using System.Drawing;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    public partial class LoginForms : Form
    {
        private const string PlaceholderUsuario = "Ingrese su usuario";
        private const string PlaceholderClave = "Ingrese su contraseña";
        private readonly Color colorPlaceholder = Color.FromArgb(122, 132, 156);
        private bool usuarioEsPlaceholder;
        private bool claveEsPlaceholder;
        private bool mostrarClave;

        public LoginForms()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblLogoIcono);
            ConfigurarLogin();
        }

        private void ConfigurarLogin()
        {
            txtUsuario.MaxLength = 50;
            txtClave.MaxLength = 50;
            MostrarPlaceholderUsuario();
            MostrarPlaceholderClave();

            txtUsuario.Enter += Usuario_Enter;
            txtUsuario.Leave += Usuario_Leave;
            txtClave.Enter += Clave_Enter;
            txtClave.Leave += Clave_Leave;
            btnIniciar.Click += Iniciar_Click;
            btnLimpiar.Click += Limpiar_Click;
            lblOjo.Click += Ojo_Click;
            lblOjo.Cursor = Cursors.Hand;
            lnkOlvido.LinkClicked += Olvido_LinkClicked;
            AcceptButton = btnIniciar;

            string usuarioGuardado = string.Empty;
            try { usuarioGuardado = Properties.Settings.Default.UsuarioRecordado; }
            catch (System.Configuration.ConfigurationErrorsException)
            {
                // Una preferencia local dañada no debe impedir abrir el Login.
            }
            if (!string.IsNullOrWhiteSpace(usuarioGuardado))
            {
                txtUsuario.Text = usuarioGuardado;
                txtUsuario.ForeColor = Color.White;
                usuarioEsPlaceholder = false;
                chkRecordarme.Checked = true;
            }
        }

        private void MostrarPlaceholderUsuario()
        {
            txtUsuario.Text = PlaceholderUsuario;
            txtUsuario.ForeColor = colorPlaceholder;
            usuarioEsPlaceholder = true;
        }

        private void MostrarPlaceholderClave()
        {
            txtClave.UseSystemPasswordChar = false;
            txtClave.Text = PlaceholderClave;
            txtClave.ForeColor = colorPlaceholder;
            claveEsPlaceholder = true;
        }

        private void Usuario_Enter(object sender, EventArgs e)
        {
            if (usuarioEsPlaceholder)
            {
                txtUsuario.Clear();
                txtUsuario.ForeColor = Color.White;
                usuarioEsPlaceholder = false;
            }
        }

        private void Usuario_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
                MostrarPlaceholderUsuario();
        }

        private void Clave_Enter(object sender, EventArgs e)
        {
            if (claveEsPlaceholder)
            {
                txtClave.Clear();
                txtClave.ForeColor = Color.White;
                claveEsPlaceholder = false;
                txtClave.UseSystemPasswordChar = !mostrarClave;
            }
        }

        private void Clave_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtClave.Text))
                MostrarPlaceholderClave();
        }

        private void Ojo_Click(object sender, EventArgs e)
        {
            mostrarClave = !mostrarClave;
            if (!claveEsPlaceholder)
                txtClave.UseSystemPasswordChar = !mostrarClave;
            txtClave.Focus();
            txtClave.SelectionStart = txtClave.TextLength;
        }

        private void Limpiar_Click(object sender, EventArgs e)
        {
            MostrarPlaceholderUsuario();
            MostrarPlaceholderClave();
            chkRecordarme.Checked = false;
            mostrarClave = false;
            GuardarUsuarioRecordado(string.Empty);
            txtUsuario.Focus();
        }

        private void Olvido_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(
                "Solicita al administrador de la librería que restablezca tu contraseña.",
                "Recuperación de contraseña",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void Iniciar_Click(object sender, EventArgs e)
        {
            string nombreUsuario = usuarioEsPlaceholder ? string.Empty : txtUsuario.Text.Trim();
            string clave = claveEsPlaceholder ? string.Empty : txtClave.Text;
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(clave))
            {
                MessageBox.Show("Escribe tu usuario y contraseña para continuar.", "Datos requeridos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnIniciar.Enabled = false;
            Cursor = Cursors.WaitCursor;
            bool cerrarAlTerminar = false;
            try
            {
                Usuario usuario = new UsuariosNegocio().AutenticarUsuario(nombreUsuario, clave);
                if (usuario == null)
                {
                    MessageBox.Show("El usuario o la contraseña no son correctos, o la cuenta está inactiva.",
                        "No se pudo iniciar sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtClave.Focus();
                    return;
                }

                GuardarUsuarioRecordado(chkRecordarme.Checked
                    ? usuario.NombreUsuario
                    : string.Empty);

                Hide();
                DialogResult resultadoMenu;
                using (MenuPrincipalForms menu = new MenuPrincipalForms())
                    resultadoMenu = menu.ShowDialog(this);

                cerrarAlTerminar = resultadoMenu != DialogResult.Retry;
                if (!cerrarAlTerminar)
                {
                    MostrarPlaceholderClave();
                    Show();
                    Activate();
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Datos de acceso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                if (!Visible) Show();
                MessageBox.Show("No fue posible validar el acceso. Revisa la conexión con el servidor e inténtalo de nuevo.",
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnIniciar.Enabled = true;
            }

            if (cerrarAlTerminar)
                Close();
        }

        private static void GuardarUsuarioRecordado(string nombre)
        {
            // Recordar el usuario es opcional; los permisos de user.config no afectan la sesión.
            try
            {
                Properties.Settings.Default.UsuarioRecordado = nombre;
                Properties.Settings.Default.Save();
            }
            catch (System.Configuration.ConfigurationErrorsException) { }
            catch (UnauthorizedAccessException) { }
            catch (System.IO.IOException) { }
        }
    }
}
