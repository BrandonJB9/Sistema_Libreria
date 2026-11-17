using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    public partial class FormsClientes : Form
    {
        private const int RegistrosPorPagina = 10;
        private readonly ClientesNegocio clientesNegocio = new ClientesNegocio();
        private List<Cliente> todosLosClientes = new List<Cliente>();
        private List<Cliente> clientesFiltrados = new List<Cliente>();
        private int paginaActual;
        private bool modoAgregarCliente;
        private bool modoEditarCliente;
        private int? idClienteSeleccionado;

        public FormsClientes()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            btnLimpiarBusquedaCliente.Visible = false;

            // Las tres tarjetas usan los datos reales de Tbl_Cliente y los colores de Sucursales.
            label11.Text = "Total de clientes";
            label5.Text = "Clientes con correo";
            label2.Text = "Clientes sin correo";
            label14.Text = "Correos únicos";
            label10.Text = "👥";
            label4.Text = "✉️";
            label1.Text = "👤";
            label13.Text = "✉️";
            panel4.BackColor = Color.FromArgb(18, 45, 75);
            label10.BackColor = Color.FromArgb(23, 66, 110);
            label11.ForeColor = Color.FromArgb(45, 155, 255);
            panel2.BackColor = Color.FromArgb(18, 52, 62);
            label4.BackColor = Color.FromArgb(23, 83, 80);
            label5.ForeColor = Color.FromArgb(46, 211, 154);
            panel1.BackColor = Color.FromArgb(44, 46, 47);
            label1.BackColor = Color.FromArgb(83, 69, 45);
            label2.ForeColor = Color.FromArgb(245, 165, 36);
            // La tarjeta nueva conserva sus posiciones y usa el acento de Departamentos en Sucursales.
            panel3.BackColor = Color.FromArgb(25, 62, 83);
            label13.BackColor = Color.FromArgb(29, 105, 135);
            label14.ForeColor = Color.FromArgb(91, 207, 238);

            ConfigurarLayoutResponsivoClientes();

            Load += FormsClientes_Load;
            btnBuscarCliente.Click += btnBuscarCliente_Click;
            btnLimpiarBusquedaCliente.Click += btnLimpiarBusquedaCliente_Click;
            btnAgregarCliente.Click += btnAgregarCliente_Click;
            btnGuardarCliente.Click += btnGuardarCliente_Click;
            btnLimpiarFormularioCliente.Click += btnLimpiarFormularioCliente_Click;
            btnEditarCliente.Click += btnEditarCliente_Click;
            btnEliminarCliente.Click += btnEliminarCliente_Click;
            btnCancelarCliente.Click += btnCancelarCliente_Click;
            txtBuscarCliente.KeyDown += txtBuscarCliente_KeyDown;
            dgvTbl_Cliente.CellClick += dgvTbl_Cliente_CellClick;
            dgvTbl_Cliente.KeyDown += dgvTbl_Cliente_KeyDown;
        }

        private void ConfigurarLayoutResponsivoClientes()
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

            dgvTbl_Cliente.Dock = DockStyle.Fill;
            dgvTbl_Cliente.MinimumSize = new Size(0, 120);
            dgvTbl_Cliente.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

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

            foreach (Button boton in new[] { btnAgregarCliente, btnGuardarCliente, btnEliminarCliente,
                btnCancelarCliente, btnEditarCliente, btnLimpiarFormularioCliente })
            {
                boton.Size = new Size(122, 40);
                boton.MinimumSize = new Size(108, 40);
                boton.Margin = new Padding(3);
                barra.Controls.Add(boton);
            }

            txtBuscarCliente.Size = new Size(180, 32);
            txtBuscarCliente.MinimumSize = new Size(145, 28);
            txtBuscarCliente.Margin = new Padding(3, 7, 3, 3);
            barra.Controls.Add(txtBuscarCliente);
            foreach (Button boton in new[] { btnBuscarCliente, btnLimpiarBusquedaCliente })
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
                RowCount = 2,
                Padding = new Padding(12, 8, 12, 8),
                Margin = new Padding(0),
                BackColor = pnlListado.BackColor
            };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));

            Control[] controles = { label7, txtIDCliente, label8, txtTelefono,
                label15, txtNombreCliente, label9, txtCorreoElectronico };
            for (int indice = 0; indice < controles.Length; indice++)
            {
                Control control = controles[indice];
                pnlListado.Controls.Remove(control);
                control.Margin = new Padding(5, 4, 5, 4);
                if (control is Label etiqueta)
                {
                    etiqueta.Dock = DockStyle.Fill;
                    etiqueta.AutoSize = false;
                    etiqueta.TextAlign = ContentAlignment.MiddleLeft;
                }
                else
                    control.Dock = DockStyle.Fill;
                campos.Controls.Add(control, indice % 4, indice / 4);
            }

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
            pnlListado.AutoScrollMinSize = new Size(900, 420);
            pnlListado.Controls.Add(dgvTbl_Cliente);
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

        private void FormsClientes_Load(object sender, EventArgs e)
        {
            string detalleError;
            if (!ConsultarClientes(out detalleError))
            {
                MostrarMensaje("No se pudieron consultar los clientes. " + detalleError,
                    "Consulta de clientes", MessageBoxIcon.Error);
            }
            EstablecerCamposEditables(false);
        }

        private bool ConsultarClientes(out string detalleError)
        {
            try
            {
                todosLosClientes = clientesNegocio.ConsultarClientes();
                clientesFiltrados = new List<Cliente>(todosLosClientes);
                ActualizarContadores();
                paginaActual = 0;
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

        private void BuscarClientes()
        {
            try
            {
                string criterio = txtBuscarCliente.Text.Trim();
                clientesFiltrados = string.IsNullOrWhiteSpace(criterio)
                    ? new List<Cliente>(todosLosClientes)
                    : clientesNegocio.BuscarClientes(criterio);
                paginaActual = 0;
                MostrarPaginaActual();
            }
            catch (Exception ex)
            {
                MostrarMensaje("No se pudieron buscar los clientes. " + ex.Message,
                    "Buscar cliente", MessageBoxIcon.Error);
            }
        }

        private void MostrarPaginaActual()
        {
            // Solo se enlaza una pagina para mantener manejable la grilla.
            List<Cliente> filasPagina = clientesFiltrados
                .Skip(paginaActual * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToList();

            dgvTbl_Cliente.ReadOnly = true;
            dgvTbl_Cliente.AllowUserToAddRows = false;
            dgvTbl_Cliente.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTbl_Cliente.MultiSelect = false;
            dgvTbl_Cliente.AutoGenerateColumns = true;
            idClienteSeleccionado = null;
            dgvTbl_Cliente.DataSource = filasPagina;
            ConfigurarEstiloGrilla();

            if (dgvTbl_Cliente.Columns["IDCliente"] != null)
            {
                dgvTbl_Cliente.Columns["IDCliente"].HeaderText = "Código";
            }
            if (dgvTbl_Cliente.Columns["NombreCliente"] != null)
            {
                dgvTbl_Cliente.Columns["NombreCliente"].HeaderText = "Nombre del cliente";
            }
            if (dgvTbl_Cliente.Columns["Telefono"] != null)
            {
                dgvTbl_Cliente.Columns["Telefono"].HeaderText = "Teléfono";
            }
            if (dgvTbl_Cliente.Columns["CorreoElectronico"] != null)
            {
                dgvTbl_Cliente.Columns["CorreoElectronico"].HeaderText = "Correo electrónico";
            }
        }

        private void ConfigurarEstiloGrilla()
        {
            dgvTbl_Cliente.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTbl_Cliente.RowHeadersWidth = 42;
            dgvTbl_Cliente.ColumnHeadersHeight = 38;
            dgvTbl_Cliente.RowTemplate.Height = 34;
            dgvTbl_Cliente.EnableHeadersVisualStyles = false;
            dgvTbl_Cliente.BackgroundColor = Color.FromArgb(18, 45, 75);
            dgvTbl_Cliente.GridColor = Color.FromArgb(68, 111, 151);

            dgvTbl_Cliente.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(23, 76, 114);
            dgvTbl_Cliente.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTbl_Cliente.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvTbl_Cliente.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvTbl_Cliente.DefaultCellStyle.BackColor = Color.FromArgb(21, 51, 78);
            dgvTbl_Cliente.DefaultCellStyle.ForeColor = Color.White;
            dgvTbl_Cliente.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 145, 210);
            dgvTbl_Cliente.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvTbl_Cliente.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvTbl_Cliente.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(29, 62, 91);
            dgvTbl_Cliente.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
        }

        private void btnBuscarCliente_Click(object sender, EventArgs e)
        {
            BuscarClientes();
        }

        private void btnLimpiarBusquedaCliente_Click(object sender, EventArgs e)
        {
            txtBuscarCliente.Clear();
            BuscarClientes();
        }

        private void btnAgregarCliente_Click(object sender, EventArgs e)
        {
            modoAgregarCliente = true;
            modoEditarCliente = false;
            idClienteSeleccionado = null;
            txtIDCliente.Clear();
            txtNombreCliente.Clear();
            txtTelefono.Clear();
            txtCorreoElectronico.Clear();
            EstablecerCamposEditables(true);
            txtNombreCliente.Focus();
        }

        private void btnGuardarCliente_Click(object sender, EventArgs e)
        {
            if (modoAgregarCliente)
            {
                GuardarNuevoCliente();
            }
            else if (modoEditarCliente && idClienteSeleccionado.HasValue)
            {
                GuardarEdicionCliente();
            }
            else
            {
                MostrarMensaje("Presiona Nuevo para agregar o selecciona un cliente y pulsa Editar antes de guardar.",
                    "Clientes", MessageBoxIcon.Information);
            }
        }

        private void GuardarNuevoCliente()
        {
            try
            {
                Cliente cliente = CrearClienteDesdeFormulario();
                int idGenerado = clientesNegocio.AgregarCliente(cliente);
                modoAgregarCliente = false;
                idClienteSeleccionado = idGenerado;
                txtIDCliente.Text = idGenerado.ToString();
                EstablecerCamposEditables(false);
                string detalleError;
                if (!ConsultarClientes(out detalleError))
                {
                    MostrarMensaje("El cliente se agregó, pero no se pudo actualizar la lista. " + detalleError,
                        "Cliente agregado", MessageBoxIcon.Warning);
                    return;
                }
                MostrarRegistroEnGrilla(idGenerado);
                MostrarMensaje("Cliente agregado correctamente. Código: " + idGenerado,
                    "Agregar cliente", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MostrarMensaje(ex.Message, "No se pudo agregar el cliente", MessageBoxIcon.Error);
            }
        }

        private void btnEditarCliente_Click(object sender, EventArgs e)
        {
            if (!idClienteSeleccionado.HasValue)
            {
                MostrarMensaje("Selecciona un cliente de la grilla antes de editar.",
                    "Editar cliente", MessageBoxIcon.Information);
                return;
            }

            modoAgregarCliente = false;
            modoEditarCliente = true;
            EstablecerCamposEditables(true);
            txtNombreCliente.Focus();
        }

        private void GuardarEdicionCliente()
        {
            try
            {
                Cliente cliente = CrearClienteDesdeFormulario();
                cliente.IDCliente = idClienteSeleccionado.Value;
                clientesNegocio.EditarCliente(cliente);
                int idActualizado = cliente.IDCliente;
                modoEditarCliente = false;
                EstablecerCamposEditables(false);
                string detalleError;
                if (!ConsultarClientes(out detalleError))
                {
                    MostrarMensaje("El cliente se actualizó, pero no se pudo actualizar la lista. " + detalleError,
                        "Cliente actualizado", MessageBoxIcon.Warning);
                    return;
                }
                MostrarRegistroEnGrilla(idActualizado);
                MostrarMensaje("Cliente actualizado correctamente.", "Editar cliente", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MostrarMensaje(ex.Message, "No se pudo editar el cliente", MessageBoxIcon.Error);
            }
        }

        private void btnEliminarCliente_Click(object sender, EventArgs e)
        {
            if (!idClienteSeleccionado.HasValue)
            {
                MostrarMensaje("Selecciona un cliente de la grilla antes de eliminar.",
                    "Eliminar cliente", MessageBoxIcon.Information);
                return;
            }

            int idEliminar = idClienteSeleccionado.Value;
            if (MostrarConfirmacion("¿Confirmas eliminar a " + txtNombreCliente.Text +
                " (código " + idEliminar + ")?", "Confirmar eliminación") != DialogResult.Yes)
            {
                return;
            }

            try
            {
                clientesNegocio.EliminarCliente(idEliminar);
                modoAgregarCliente = false;
                modoEditarCliente = false;
                LimpiarFormularioCliente();
                string detalleError;
                if (!ConsultarClientes(out detalleError))
                {
                    MostrarMensaje("El cliente se eliminó, pero no se pudo actualizar la lista. " + detalleError,
                        "Cliente eliminado", MessageBoxIcon.Warning);
                    return;
                }
                MostrarMensaje("Cliente eliminado correctamente.", "Eliminar cliente", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MostrarMensaje("No se pudo eliminar el cliente. Puede tener pedidos o préstamos relacionados. " + ex.Message,
                    "Eliminar cliente", MessageBoxIcon.Error);
            }
        }

        private void btnCancelarCliente_Click(object sender, EventArgs e)
        {
            if (!modoAgregarCliente && !modoEditarCliente)
            {
                return;
            }

            bool eraAgregar = modoAgregarCliente;
            modoAgregarCliente = false;
            modoEditarCliente = false;
            if (eraAgregar)
            {
                LimpiarFormularioCliente();
            }
            else
            {
                RestaurarClienteSeleccionado();
            }

            EstablecerCamposEditables(false);
            MostrarMensaje("La operación fue cancelada; no se guardaron cambios.",
                "Cancelar operación", MessageBoxIcon.Information);
        }

        private void btnLimpiarFormularioCliente_Click(object sender, EventArgs e)
        {
            if (!modoAgregarCliente && !modoEditarCliente)
            {
                idClienteSeleccionado = null;
                LimpiarFormularioCliente();
                EstablecerCamposEditables(false);
                return;
            }

            txtNombreCliente.Clear();
            txtTelefono.Clear();
            txtCorreoElectronico.Clear();
            txtNombreCliente.Focus();
        }

        private void dgvTbl_Cliente_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            Cliente cliente = dgvTbl_Cliente.Rows[e.RowIndex].DataBoundItem as Cliente;
            if (cliente == null)
            {
                return;
            }

            modoAgregarCliente = false;
            modoEditarCliente = false;
            idClienteSeleccionado = cliente.IDCliente;
            CargarClienteEnFormulario(cliente);
            EstablecerCamposEditables(false);
        }

        private Cliente CrearClienteDesdeFormulario()
        {
            return new Cliente
            {
                NombreCliente = txtNombreCliente.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                CorreoElectronico = txtCorreoElectronico.Text.Trim()
            };
        }

        private void CargarClienteEnFormulario(Cliente cliente)
        {
            txtIDCliente.Text = cliente.IDCliente.ToString();
            txtNombreCliente.Text = cliente.NombreCliente;
            txtTelefono.Text = cliente.Telefono;
            txtCorreoElectronico.Text = cliente.CorreoElectronico;
        }

        private void RestaurarClienteSeleccionado()
        {
            if (!idClienteSeleccionado.HasValue)
            {
                LimpiarFormularioCliente();
                return;
            }

            Cliente cliente = todosLosClientes.FirstOrDefault(
                elemento => elemento.IDCliente == idClienteSeleccionado.Value);
            if (cliente == null)
            {
                LimpiarFormularioCliente();
                return;
            }

            CargarClienteEnFormulario(cliente);
        }

        private void MostrarRegistroEnGrilla(int idCliente)
        {
            int indice = todosLosClientes.FindIndex(cliente => cliente.IDCliente == idCliente);
            if (indice < 0)
            {
                return;
            }

            paginaActual = indice / RegistrosPorPagina;
            MostrarPaginaActual();
            int filaPagina = indice % RegistrosPorPagina;
            if (filaPagina < dgvTbl_Cliente.Rows.Count)
            {
                dgvTbl_Cliente.ClearSelection();
                dgvTbl_Cliente.Rows[filaPagina].Selected = true;
                dgvTbl_Cliente.CurrentCell = dgvTbl_Cliente.Rows[filaPagina].Cells[0];
                idClienteSeleccionado = idCliente;
                CargarClienteEnFormulario(todosLosClientes[indice]);
            }
        }

        private void LimpiarFormularioCliente()
        {
            idClienteSeleccionado = null;
            txtIDCliente.Clear();
            txtNombreCliente.Clear();
            txtTelefono.Clear();
            txtCorreoElectronico.Clear();
        }

        private void EstablecerCamposEditables(bool editables)
        {
            txtNombreCliente.ReadOnly = !editables;
            txtTelefono.ReadOnly = !editables;
            txtCorreoElectronico.ReadOnly = !editables;
            Color fondo = editables ? Color.White : Color.Gainsboro;
            Color texto = editables ? Color.Black : Color.DimGray;
            txtNombreCliente.BackColor = fondo;
            txtNombreCliente.ForeColor = texto;
            txtTelefono.BackColor = fondo;
            txtTelefono.ForeColor = texto;
            txtCorreoElectronico.BackColor = fondo;
            txtCorreoElectronico.ForeColor = texto;
        }

        private void ActualizarContadores()
        {
            // Los totales representan toda la tabla, aunque la grilla tenga un filtro.
            label12.Text = todosLosClientes.Count.ToString();
            label6.Text = todosLosClientes.Count(cliente =>
                !string.IsNullOrWhiteSpace(cliente.CorreoElectronico)).ToString();
            label3.Text = todosLosClientes.Count(cliente =>
                string.IsNullOrWhiteSpace(cliente.CorreoElectronico)).ToString();
            label17.Text = todosLosClientes
                .Select(cliente => (cliente.CorreoElectronico ?? string.Empty).Trim())
                .Where(correo => correo.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count()
                .ToString();
        }

        private DialogResult MostrarMensaje(string mensaje, string titulo, MessageBoxIcon icono)
        {
            return MensajesForms.Mostrar(this, mensaje, titulo, MessageBoxButtons.OK, icono);
        }

        private DialogResult MostrarConfirmacion(string mensaje, string titulo)
        {
            return MensajesForms.Mostrar(this, mensaje, titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }

        private void txtBuscarCliente_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BuscarClientes();
                e.SuppressKeyPress = true;
            }
        }

        private void dgvTbl_Cliente_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.PageDown &&
                (paginaActual + 1) * RegistrosPorPagina < clientesFiltrados.Count)
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
    }
}
