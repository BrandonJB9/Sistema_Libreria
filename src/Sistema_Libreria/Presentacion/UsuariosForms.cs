using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    public partial class UsuariosForms : Form
    {
        private const int RegistrosPorPagina = 10;
        private readonly UsuariosNegocio usuariosNegocio = new UsuariosNegocio();
        private List<Usuario> todosLosUsuarios = new List<Usuario>();
        private List<Usuario> usuariosFiltrados = new List<Usuario>();
        private List<Sucursal> catalogoSucursales = new List<Sucursal>();
        private int paginaActual;
        private int? idUsuarioSeleccionado;
        private bool modoAgregarUsuario;
        private bool modoEditarUsuario;

        public UsuariosForms()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            btnLimpiarBusquedaUsuario.Visible = false;
            ConfigurarLayoutResponsivoUsuarios();
            Load += UsuariosForms_Load;
            btnAgregarUsuario.Click += btnAgregarUsuario_Click;
            btnGuardarUsuario.Click += btnGuardarUsuario_Click;
            btnEditarUsuario.Click += btnEditarUsuario_Click;
            btnEliminarUsuario.Click += btnEliminarUsuario_Click;
            btnCancelarUsuario.Click += btnCancelarUsuario_Click;
            btnLimpiarFormularioUsuario.Click += btnLimpiarFormularioUsuario_Click;
            btnBuscarUsuario.Click += btnBuscarUsuario_Click;
            btnLimpiarBusquedaUsuario.Click += btnLimpiarBusquedaUsuario_Click;
            txtBuscarUsuario.KeyDown += txtBuscarUsuario_KeyDown;
            dgvTbl_Usuario.CellClick += dgvTbl_Usuario_CellClick;
            dgvTbl_Usuario.KeyDown += dgvTbl_Usuario_KeyDown;
        }

        private void ConfigurarLayoutResponsivoUsuarios()
        {
            MinimumSize = new Size(900, 650);
            AutoScroll = true;

            tlpStats.ColumnCount = 3;
            tlpStats.RowCount = 1;
            tlpStats.Dock = DockStyle.Top;
            tlpStats.Height = 150;
            tlpStats.MinimumSize = new Size(0, 150);
            tlpStats.ColumnStyles.Clear();
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
            PrepararTarjeta(panel4, label10, label11, label12);
            PrepararTarjeta(panel2, label4, label5, label6);
            PrepararTarjeta(panel1, label1, label2, label3);

            dgvTbl_Usuario.Dock = DockStyle.Fill;
            dgvTbl_Usuario.MinimumSize = new Size(0, 120);
            dgvTbl_Usuario.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            var barra = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(0),
                BackColor = pnlListadoTitulo.BackColor
            };

            lblListado.AutoSize = false;
            lblListado.Dock = DockStyle.None;
            lblListado.Size = new Size(340, 44);
            lblListado.MinimumSize = new Size(260, 44);
            lblListado.AutoEllipsis = true;
            lblListado.TextAlign = ContentAlignment.MiddleLeft;
            lblListado.Margin = new Padding(3);
            barra.Controls.Add(lblListado);

            foreach (Button boton in new[] { btnAgregarUsuario, btnGuardarUsuario, btnEliminarUsuario,
                btnCancelarUsuario, btnEditarUsuario, btnLimpiarFormularioUsuario })
            {
                boton.Size = new Size(122, 40);
                boton.MinimumSize = new Size(108, 40);
                boton.Margin = new Padding(3);
                barra.Controls.Add(boton);
            }

            txtBuscarUsuario.Size = new Size(180, 32);
            txtBuscarUsuario.MinimumSize = new Size(145, 28);
            txtBuscarUsuario.Margin = new Padding(3, 7, 3, 3);
            barra.Controls.Add(txtBuscarUsuario);
            foreach (Button boton in new[] { btnBuscarUsuario, btnLimpiarBusquedaUsuario })
            {
                boton.Size = new Size(122, 40);
                boton.MinimumSize = new Size(108, 40);
                boton.Margin = new Padding(3);
                barra.Controls.Add(boton);
            }

            pnlListadoTitulo.Controls.Clear();
            pnlListadoTitulo.Padding = new Padding(0);
            pnlListadoTitulo.AutoSize = true;
            pnlListadoTitulo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlListadoTitulo.MinimumSize = new Size(0, 52);
            pnlListadoTitulo.Controls.Add(barra);

            var campos = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(12, 8, 12, 8),
                Margin = new Padding(0),
                BackColor = pnlListado.BackColor
            };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            for (int fila = 0; fila < 3; fila++)
                campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));

            var estado = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(3),
                Padding = new Padding(4, 6, 0, 0),
                BackColor = Color.Transparent
            };
            foreach (Control control in new Control[] { rdbEstadoActivo, rdbEstadoInactivo })
            {
                pnlListado.Controls.Remove(control);
                control.Margin = new Padding(4, 3, 12, 3);
                estado.Controls.Add(control);
            }

            Control[] controles = { label7, txtIDUsuario, label9, txtContrasena,
                label15, cboSucursal, label14, txtTelefono,
                label8, txtNombreUsuario, label17, estado };
            foreach (Control control in controles)
            {
                pnlListado.Controls.Remove(control);
                control.Margin = new Padding(5, 4, 5, 4);
                if (control is Label etiqueta)
                {
                    etiqueta.Dock = DockStyle.Fill;
                    etiqueta.AutoSize = false;
                    etiqueta.TextAlign = ContentAlignment.MiddleLeft;
                }
                else if (control is TextBox || control is ComboBox)
                    control.Dock = DockStyle.Fill;
            }
            for (int indice = 0; indice < controles.Length; indice++)
                campos.Controls.Add(controles[indice], indice % 4, indice / 4);

            var pie = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 68,
                Padding = new Padding(12, 6, 12, 6),
                BackColor = pnlListado.BackColor
            };
            btnMenuPrincipal.Dock = DockStyle.Left;
            pie.Controls.Add(btnMenuPrincipal);

            pnlListado.Controls.Clear();
            pnlListado.Padding = new Padding(14);
            pnlListado.AutoScroll = true;
            pnlListado.AutoScrollMinSize = new Size(900, 450);
            pnlListado.Controls.Add(dgvTbl_Usuario);
            pnlListado.Controls.Add(pie);
            pnlListado.Controls.Add(campos);
            pnlListado.Controls.Add(pnlListadoTitulo);
        }

        private static void PrepararTarjeta(Panel tarjeta, Label icono, Label titulo, Label valor)
        {
            tarjeta.Dock = DockStyle.Fill;
            tarjeta.Margin = new Padding(6);
            icono.AutoSize = false;
            titulo.AutoSize = false;
            valor.AutoSize = false;
            valor.TextAlign = ContentAlignment.MiddleLeft;
            Action ajustar = () =>
            {
                int anchoTexto = Math.Max(90, tarjeta.ClientSize.Width - 104);
                icono.SetBounds(14, 18, 58, Math.Max(54, tarjeta.ClientSize.Height - 36));
                titulo.SetBounds(84, 16, anchoTexto, 30);
                valor.SetBounds(84, 48, anchoTexto, Math.Max(56, tarjeta.ClientSize.Height - 54));
                titulo.AutoEllipsis = true;
            };
            tarjeta.SizeChanged += (sender, e) => ajustar();
            ajustar();
        }

        private void UsuariosForms_Load(object sender, EventArgs e)
        {
            try
            {
                CargarCatalogoSucursales();
                label5.Text = "Usuarios activos";
                label2.Text = "Usuarios inactivos";
                string detalleError;
                if (!ConsultarUsuarios(out detalleError))
                {
                    MostrarMensaje("No se pudieron consultar los usuarios. " + detalleError,
                        "Consulta de usuarios", MessageBoxIcon.Error);
                }

                EstablecerCamposEditables(false);
            }
            catch (Exception ex)
            {
                MostrarMensaje("No se pudo cargar el catálogo de sucursales. " + ex.Message,
                    "Consulta de sucursales", MessageBoxIcon.Error);
            }
        }

        private void CargarCatalogoSucursales()
        {
            try
            {
                catalogoSucursales = usuariosNegocio.ConsultarSucursalesCatalogo();
                cboSucursal.DisplayMember = "NombreSucursal";
                cboSucursal.ValueMember = "IDCodigoSucursal";
                cboSucursal.DataSource = catalogoSucursales;
                cboSucursal.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo cargar el catálogo de sucursales.", ex);
            }
        }

        private bool ConsultarUsuarios(out string detalleError)
        {
            try
            {
                todosLosUsuarios = usuariosNegocio.ConsultarUsuarios();
                usuariosFiltrados = new List<Usuario>(todosLosUsuarios);
                paginaActual = 0;
                ActualizarContadores();
                MostrarPaginaActual();
                detalleError = null;
                return true;
            }
            catch (Exception ex)
            {
                detalleError = ex.Message;
                return false;
            }
        }

        private void ActualizarContadores()
        {
            // Los conteos se calculan con la lista completa, nunca con las diez filas visibles.
            label12.Text = todosLosUsuarios.Count.ToString();
            label6.Text = todosLosUsuarios.Count(usuario => usuario.Estado).ToString();
            label3.Text = todosLosUsuarios.Count(usuario => !usuario.Estado).ToString();
            label20.Text = todosLosUsuarios
                .Select(usuario => usuario.IDCodigoSucursal)
                .Distinct()
                .Count()
                .ToString();
        }

        private void MostrarPaginaActual()
        {
            // La paginación pertenece solo a Presentación.
            List<Usuario> filasPagina = usuariosFiltrados
                .Skip(paginaActual * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToList();

            dgvTbl_Usuario.ReadOnly = true;
            dgvTbl_Usuario.AllowUserToAddRows = false;
            dgvTbl_Usuario.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTbl_Usuario.MultiSelect = false;
            dgvTbl_Usuario.AutoGenerateColumns = true;
            dgvTbl_Usuario.DataSource = filasPagina;
            ConfigurarEstiloGrilla();

            if (dgvTbl_Usuario.Columns["Contrasena"] != null)
                dgvTbl_Usuario.Columns["Contrasena"].Visible = false;
            if (dgvTbl_Usuario.Columns["IDUsuario"] != null)
                dgvTbl_Usuario.Columns["IDUsuario"].HeaderText = "IDUsuario";
            if (dgvTbl_Usuario.Columns["IDCodigoSucursal"] != null)
                dgvTbl_Usuario.Columns["IDCodigoSucursal"].HeaderText = "Código de sucursal";
            if (dgvTbl_Usuario.Columns["NombreSucursal"] != null)
                dgvTbl_Usuario.Columns["NombreSucursal"].HeaderText = "Sucursal";
            if (dgvTbl_Usuario.Columns["NombreUsuario"] != null)
                dgvTbl_Usuario.Columns["NombreUsuario"].HeaderText = "Nombre de usuario";
            if (dgvTbl_Usuario.Columns["Telefono"] != null)
                dgvTbl_Usuario.Columns["Telefono"].HeaderText = "Teléfono";
            if (dgvTbl_Usuario.Columns["Estado"] != null)
                dgvTbl_Usuario.Columns["Estado"].HeaderText = "Activo";
        }

        private void ConfigurarEstiloGrilla()
        {
            dgvTbl_Usuario.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTbl_Usuario.RowHeadersWidth = 42;
            dgvTbl_Usuario.ColumnHeadersHeight = 38;
            dgvTbl_Usuario.RowTemplate.Height = 34;
            dgvTbl_Usuario.EnableHeadersVisualStyles = false;
            dgvTbl_Usuario.BackgroundColor = Color.FromArgb(18, 45, 75);
            dgvTbl_Usuario.GridColor = Color.FromArgb(68, 111, 151);
            dgvTbl_Usuario.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(23, 76, 114);
            dgvTbl_Usuario.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTbl_Usuario.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvTbl_Usuario.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvTbl_Usuario.DefaultCellStyle.BackColor = Color.FromArgb(21, 51, 78);
            dgvTbl_Usuario.DefaultCellStyle.ForeColor = Color.White;
            dgvTbl_Usuario.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 145, 210);
            dgvTbl_Usuario.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvTbl_Usuario.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvTbl_Usuario.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(29, 62, 91);
            dgvTbl_Usuario.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
        }

        private void btnAgregarUsuario_Click(object sender, EventArgs e)
        {
            modoAgregarUsuario = true;
            modoEditarUsuario = false;
            idUsuarioSeleccionado = null;
            LimpiarFormularioUsuario();
            EstablecerCamposEditables(true);
            rdbEstadoActivo.Checked = true;
            txtNombreUsuario.Focus();
        }

        private void btnGuardarUsuario_Click(object sender, EventArgs e)
        {
            if (modoAgregarUsuario)
                GuardarNuevoUsuario();
            else if (modoEditarUsuario && idUsuarioSeleccionado.HasValue)
                GuardarEdicionUsuario();
            else
                MostrarMensaje("Presiona Nuevo o selecciona un usuario y pulsa Editar antes de guardar.",
                    "Usuarios", MessageBoxIcon.Information);
        }

        private void GuardarNuevoUsuario()
        {
            try
            {
                Usuario usuario = CrearUsuarioDesdeFormulario();
                int idGenerado = usuariosNegocio.AgregarUsuario(usuario);
                modoAgregarUsuario = false;
                idUsuarioSeleccionado = idGenerado;
                txtIDUsuario.Text = idGenerado.ToString();
                EstablecerCamposEditables(false);

                string detalleError;
                if (!ConsultarUsuarios(out detalleError))
                {
                    MostrarMensaje("El usuario se agregó, pero no se pudo actualizar la lista. " + detalleError,
                        "Usuario agregado", MessageBoxIcon.Warning);
                    return;
                }

                MostrarRegistroEnGrilla(idGenerado);
                MostrarMensaje("Usuario agregado correctamente. Código: " + idGenerado,
                    "Agregar usuario", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MostrarMensaje(ex.Message, "No se pudo agregar el usuario", MessageBoxIcon.Error);
            }
        }

        private void btnEditarUsuario_Click(object sender, EventArgs e)
        {
            if (!idUsuarioSeleccionado.HasValue)
            {
                MostrarMensaje("Selecciona un usuario de la grilla antes de editar.",
                    "Editar usuario", MessageBoxIcon.Information);
                return;
            }

            modoAgregarUsuario = false;
            modoEditarUsuario = true;
            EstablecerCamposEditables(true);
            txtNombreUsuario.Focus();
        }

        private void GuardarEdicionUsuario()
        {
            try
            {
                Usuario usuario = CrearUsuarioDesdeFormulario();
                usuario.IDUsuario = idUsuarioSeleccionado.Value;
                usuariosNegocio.EditarUsuario(usuario);
                int idActualizado = usuario.IDUsuario;
                modoEditarUsuario = false;
                EstablecerCamposEditables(false);

                string detalleError;
                if (!ConsultarUsuarios(out detalleError))
                {
                    MostrarMensaje("El usuario se actualizó, pero no se pudo actualizar la lista. " + detalleError,
                        "Usuario actualizado", MessageBoxIcon.Warning);
                    return;
                }

                MostrarRegistroEnGrilla(idActualizado);
                MostrarMensaje("Usuario actualizado correctamente.", "Editar usuario", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MostrarMensaje(ex.Message, "No se pudo editar el usuario", MessageBoxIcon.Error);
            }
        }

        private void btnEliminarUsuario_Click(object sender, EventArgs e)
        {
            if (!idUsuarioSeleccionado.HasValue)
            {
                MostrarMensaje("Selecciona un usuario de la grilla antes de eliminar.",
                    "Eliminar usuario", MessageBoxIcon.Information);
                return;
            }

            int idEliminar = idUsuarioSeleccionado.Value;
            if (MostrarConfirmacion("¿Confirmas eliminar al usuario " + txtNombreUsuario.Text +
                " (código " + idEliminar + ")?", "Confirmar eliminación") != DialogResult.Yes)
            {
                return;
            }

            try
            {
                usuariosNegocio.EliminarUsuario(idEliminar);
                modoAgregarUsuario = false;
                modoEditarUsuario = false;
                LimpiarFormularioUsuario();
                EstablecerCamposEditables(false);

                string detalleError;
                if (!ConsultarUsuarios(out detalleError))
                {
                    MostrarMensaje("El usuario se eliminó, pero no se pudo actualizar la lista. " + detalleError,
                        "Usuario eliminado", MessageBoxIcon.Warning);
                    return;
                }

                MostrarMensaje("Usuario eliminado correctamente.", "Eliminar usuario", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MostrarMensaje("No se pudo eliminar el usuario. " + ex.Message,
                    "Eliminar usuario", MessageBoxIcon.Error);
            }
        }

        private void btnCancelarUsuario_Click(object sender, EventArgs e)
        {
            if (!modoAgregarUsuario && !modoEditarUsuario)
                return;

            bool eraAgregar = modoAgregarUsuario;
            modoAgregarUsuario = false;
            modoEditarUsuario = false;
            if (eraAgregar)
                LimpiarFormularioUsuario();
            else
                RestaurarUsuarioSeleccionado();

            EstablecerCamposEditables(false);
            MostrarMensaje("La operación fue cancelada; no se guardaron cambios.",
                "Cancelar operación", MessageBoxIcon.Information);
        }

        private void btnLimpiarFormularioUsuario_Click(object sender, EventArgs e)
        {
            if (!modoAgregarUsuario && !modoEditarUsuario)
            {
                idUsuarioSeleccionado = null;
                LimpiarFormularioUsuario();
                EstablecerCamposEditables(false);
                return;
            }

            txtNombreUsuario.Clear();
            txtContrasena.Clear();
            txtTelefono.Clear();
            cboSucursal.SelectedIndex = -1;
            rdbEstadoActivo.Checked = false;
            rdbEstadoInactivo.Checked = false;
            txtNombreUsuario.Focus();
        }

        private void btnBuscarUsuario_Click(object sender, EventArgs e)
        {
            BuscarUsuarios();
        }

        private void btnLimpiarBusquedaUsuario_Click(object sender, EventArgs e)
        {
            txtBuscarUsuario.Clear();
            usuariosFiltrados = new List<Usuario>(todosLosUsuarios);
            paginaActual = 0;
            MostrarPaginaActual();
        }

        private void BuscarUsuarios()
        {
            try
            {
                string criterio = txtBuscarUsuario.Text.Trim();
                usuariosFiltrados = string.IsNullOrWhiteSpace(criterio)
                    ? new List<Usuario>(todosLosUsuarios)
                    : usuariosNegocio.BuscarUsuarios(criterio);
                paginaActual = 0;
                MostrarPaginaActual();
            }
            catch (Exception ex)
            {
                MostrarMensaje("No se pudieron buscar los usuarios. " + ex.Message,
                    "Buscar usuario", MessageBoxIcon.Error);
            }
        }

        private void txtBuscarUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BuscarUsuarios();
                e.SuppressKeyPress = true;
            }
        }

        private void dgvTbl_Usuario_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            Usuario usuario = dgvTbl_Usuario.Rows[e.RowIndex].DataBoundItem as Usuario;
            if (usuario == null)
                return;

            modoAgregarUsuario = false;
            modoEditarUsuario = false;
            idUsuarioSeleccionado = usuario.IDUsuario;
            CargarUsuarioEnFormulario(usuario);
            EstablecerCamposEditables(false);
        }

        private Usuario CrearUsuarioDesdeFormulario()
        {
            if (cboSucursal.SelectedValue == null)
                throw new InvalidOperationException("Selecciona una sucursal del catálogo.");
            if (!rdbEstadoActivo.Checked && !rdbEstadoInactivo.Checked)
                throw new InvalidOperationException("Selecciona el estado del usuario.");

            return new Usuario
            {
                IDCodigoSucursal = Convert.ToInt32(cboSucursal.SelectedValue),
                NombreUsuario = txtNombreUsuario.Text.Trim(),
                Contrasena = txtContrasena.Text,
                Telefono = txtTelefono.Text.Trim(),
                Estado = rdbEstadoActivo.Checked
            };
        }

        private void CargarUsuarioEnFormulario(Usuario usuario)
        {
            txtIDUsuario.Text = usuario.IDUsuario.ToString();
            cboSucursal.SelectedValue = usuario.IDCodigoSucursal;
            txtNombreUsuario.Text = usuario.NombreUsuario;
            txtContrasena.Clear();
            txtTelefono.Text = usuario.Telefono;
            rdbEstadoActivo.Checked = usuario.Estado;
            rdbEstadoInactivo.Checked = !usuario.Estado;
        }

        private void RestaurarUsuarioSeleccionado()
        {
            if (!idUsuarioSeleccionado.HasValue)
            {
                LimpiarFormularioUsuario();
                return;
            }

            Usuario usuario = todosLosUsuarios.FirstOrDefault(
                elemento => elemento.IDUsuario == idUsuarioSeleccionado.Value);
            if (usuario == null)
                LimpiarFormularioUsuario();
            else
                CargarUsuarioEnFormulario(usuario);
        }

        private void MostrarRegistroEnGrilla(int idUsuario)
        {
            int indice = todosLosUsuarios.FindIndex(usuario => usuario.IDUsuario == idUsuario);
            if (indice < 0)
                return;

            usuariosFiltrados = new List<Usuario>(todosLosUsuarios);
            paginaActual = indice / RegistrosPorPagina;
            MostrarPaginaActual();
            int filaPagina = indice % RegistrosPorPagina;
            if (filaPagina < dgvTbl_Usuario.Rows.Count)
            {
                dgvTbl_Usuario.ClearSelection();
                dgvTbl_Usuario.Rows[filaPagina].Selected = true;
                dgvTbl_Usuario.CurrentCell = dgvTbl_Usuario.Rows[filaPagina].Cells[0];
                idUsuarioSeleccionado = idUsuario;
                CargarUsuarioEnFormulario(todosLosUsuarios[indice]);
            }
        }

        private void LimpiarFormularioUsuario()
        {
            idUsuarioSeleccionado = null;
            txtIDUsuario.Clear();
            cboSucursal.SelectedIndex = -1;
            txtNombreUsuario.Clear();
            txtContrasena.Clear();
            txtTelefono.Clear();
            rdbEstadoActivo.Checked = false;
            rdbEstadoInactivo.Checked = false;
        }

        private void EstablecerCamposEditables(bool editables)
        {
            txtContrasena.UseSystemPasswordChar = true;
            txtContrasena.MaxLength = 50;
            txtIDUsuario.ReadOnly = true;
            txtNombreUsuario.ReadOnly = !editables;
            txtContrasena.ReadOnly = !editables;
            txtTelefono.ReadOnly = !editables;
            cboSucursal.Enabled = editables;
            rdbEstadoActivo.Enabled = editables;
            rdbEstadoInactivo.Enabled = editables;

            Color fondo = editables ? Color.White : Color.Gainsboro;
            Color texto = editables ? Color.Black : Color.DimGray;
            txtNombreUsuario.BackColor = fondo;
            txtNombreUsuario.ForeColor = texto;
            txtContrasena.BackColor = fondo;
            txtContrasena.ForeColor = texto;
            txtTelefono.BackColor = fondo;
            txtTelefono.ForeColor = texto;
            cboSucursal.BackColor = fondo;
            cboSucursal.ForeColor = texto;
        }

        private void dgvTbl_Usuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.PageDown &&
                (paginaActual + 1) * RegistrosPorPagina < usuariosFiltrados.Count)
            {
                paginaActual++;
                MostrarPaginaActual();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.PageUp && paginaActual > 0)
            {
                paginaActual--;
                MostrarPaginaActual();
                e.Handled = true;
            }
        }

        private DialogResult MostrarMensaje(string mensaje, string titulo, MessageBoxIcon icono)
        {
            return MensajesForms.Mostrar(this, mensaje, titulo, MessageBoxButtons.OK, icono);
        }

        private DialogResult MostrarConfirmacion(string mensaje, string titulo)
        {
            return MensajesForms.Mostrar(this, mensaje, titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }

    }
}
