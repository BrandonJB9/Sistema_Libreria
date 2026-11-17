using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    public partial class PedidosForms : Form
    {
        private readonly PedidosNegocio pedidosNegocio = new PedidosNegocio();
        private readonly SucursalesNegocio sucursalesNegocio = new SucursalesNegocio();
        private readonly ClientesNegocio clientesNegocio = new ClientesNegocio();
        private List<Pedido> pedidos = new List<Pedido>();
        private List<Pedido> pedidosFiltrados = new List<Pedido>();
        private bool cargandoPagina;
        private bool modoAgregarPedido;
        private bool modoEditarPedido;
        private int idPedidoSeleccionado;
        private DetallePedidofomrs formularioDetalle;

        public PedidosForms()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            btnLimpiarBusquedaPedido.Visible = false;
            ConfigurarConsulta();
            ConfigurarDisenoResponsive();
        }

        private void ConfigurarConsulta()
        {
            dgvTbl_Pedidos.AutoGenerateColumns = true;
            dgvTbl_Pedidos.ReadOnly = true;
            dgvTbl_Pedidos.AllowUserToAddRows = false;
            dgvTbl_Pedidos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTbl_Pedidos.MultiSelect = false;
            label15.Text = "Sucursal:";
            label8.Text = "Cliente:";
            Load += PedidosForms_Load;
            btnBuscarPedido.Click += btnBuscarPedido_Click;
            btnLimpiarBusquedaPedido.Click += btnLimpiarBusquedaPedido_Click;
            txtBuscarPedido.KeyDown += txtBuscarPedido_KeyDown;
            dgvTbl_Pedidos.SelectionChanged += dgvTbl_Pedidos_SelectionChanged;
            dgvTbl_Pedidos.CellDoubleClick += dgvTbl_Pedidos_CellDoubleClick;
            btnLimpiarFormularioPedido.Click += btnLimpiarFormularioPedido_Click;
            btnAgregarPedido.Click += btnAgregarPedido_Click;
            btnGuardarPedido.Click += btnGuardarPedido_Click;
            btnCancelarPedido.Click += btnCancelarPedido_Click;
            btnEditarPedido.Click += btnEditarPedido_Click;
            btnEliminarPedido.Click += btnEliminarPedido_Click;
        }

        private void ConfigurarDisenoResponsive()
        {
            SuspendLayout();
            pnlTop.Visible = false;
            pnlContenido.Padding = new Padding(18, 12, 18, 16);
            tlpStats.Height = 150;
            tlpStats.MinimumSize = new Size(0, 142);
            tlpStats.ColumnCount = 3;
            tlpStats.RowCount = 1;
            tlpStats.ColumnStyles.Clear();
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
            PrepararTarjeta(panel4, label10, label11, label12);
            PrepararTarjeta(panel2, label4, label5, label6);
            PrepararTarjeta(panel1, label1, label2, label3);

            pnlListado.Padding = new Padding(16, 10, 16, 12);
            pnlListadoTitulo.Controls.Clear();
            pnlListadoTitulo.AutoSize = true;
            pnlListadoTitulo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlListadoTitulo.Padding = Padding.Empty;
            var acciones = new FlowLayoutPanel
            {
                Name = "flpAccionesPedidos",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = pnlListadoTitulo.BackColor,
                Margin = Padding.Empty,
                Padding = new Padding(4, 4, 4, 8)
            };
            lblListado.AutoSize = false;
            lblListado.Dock = DockStyle.None;
            lblListado.Size = new Size(270, 44);
            lblListado.MinimumSize = new Size(210, 44);
            lblListado.AutoEllipsis = true;
            lblListado.TextAlign = ContentAlignment.MiddleLeft;
            lblListado.Margin = new Padding(3);
            txtBuscarPedido.Multiline = false;
            txtBuscarPedido.Width = 180;
            txtBuscarPedido.Height = 32;
            txtBuscarPedido.MinimumSize = new Size(145, 28);
            txtBuscarPedido.Margin = new Padding(3, 7, 3, 3);

            Button[] botonesAccion =
            {
                btnAgregarPedido, btnGuardarPedido, btnEliminarPedido,
                btnCancelarPedido, btnEditarPedido, btnLimpiarFormularioPedido
            };
            foreach (Button boton in botonesAccion)
            {
                boton.Size = new Size(122, 40);
                boton.MinimumSize = new Size(108, 40);
                boton.Margin = new Padding(3);
                boton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }
            acciones.Controls.Add(lblListado);
            acciones.Controls.AddRange(botonesAccion);
            acciones.Controls.Add(txtBuscarPedido);
            foreach (Button boton in new[] { btnBuscarPedido, btnLimpiarBusquedaPedido })
            {
                boton.Size = new Size(122, 40);
                boton.MinimumSize = new Size(108, 40);
                boton.Margin = new Padding(3);
                acciones.Controls.Add(boton);
            }
            pnlListadoTitulo.Padding = Padding.Empty;
            pnlListadoTitulo.Controls.Add(acciones);

            var campos = new TableLayoutPanel
            {
                Name = "tlpCamposPedidos",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(4, 6, 4, 10),
                Margin = Padding.Empty,
                BackColor = pnlListado.BackColor
            };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            for (int fila = 0; fila < 3; fila++)
                campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            Label[] etiquetas = { label7, label9, label15, label14, label8, label17 };
            foreach (Label etiqueta in etiquetas)
            {
                etiqueta.Dock = DockStyle.Fill;
                etiqueta.Margin = new Padding(4, 3, 8, 3);
                etiqueta.TextAlign = ContentAlignment.MiddleLeft;
                etiqueta.AutoSize = false;
            }
            Control[] entradas = { txtIDPedido, cboTipoPedido, cboCodigoSucursal, dtpFechaPedido, cboCliente };
            foreach (Control entrada in entradas)
            {
                entrada.Dock = DockStyle.Fill;
                entrada.Margin = new Padding(4, 8, 12, 8);
                entrada.MinimumSize = new Size(70, 28);
            }

            var estado = new FlowLayoutPanel
            {
                Name = "flpEstadoPedido",
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = pnlListado.BackColor,
                Margin = new Padding(4, 3, 4, 3),
                Padding = new Padding(6, 8, 0, 0)
            };
            rdbEstadoActivo.AutoSize = true;
            rdbEstadoInactivo.AutoSize = true;
            rdbEstadoActivo.ForeColor = Color.White;
            rdbEstadoInactivo.ForeColor = Color.White;
            rdbEstadoActivo.Margin = new Padding(4, 0, 18, 0);
            rdbEstadoInactivo.Margin = new Padding(4, 0, 8, 0);
            estado.Controls.Add(rdbEstadoActivo);
            estado.Controls.Add(rdbEstadoInactivo);

            campos.Controls.Add(label7, 0, 0);
            campos.Controls.Add(txtIDPedido, 1, 0);
            campos.Controls.Add(label9, 2, 0);
            campos.Controls.Add(cboTipoPedido, 3, 0);
            campos.Controls.Add(label15, 0, 1);
            campos.Controls.Add(cboCodigoSucursal, 1, 1);
            campos.Controls.Add(label14, 2, 1);
            campos.Controls.Add(dtpFechaPedido, 3, 1);
            campos.Controls.Add(label8, 0, 2);
            campos.Controls.Add(cboCliente, 1, 2);
            campos.Controls.Add(label17, 2, 2);
            campos.Controls.Add(estado, 3, 2);

            var pie = new TableLayoutPanel
            {
                Name = "tlpPiePedidos",
                Dock = DockStyle.Bottom,
                Height = 58,
                ColumnCount = 1,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = pnlListado.BackColor
            };
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pie.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            btnMenuPrincipal.Dock = DockStyle.Left;
            btnMenuPrincipal.Size = new Size(190, 50);
            btnMenuPrincipal.MinimumSize = new Size(160, 44);
            btnMenuPrincipal.Margin = new Padding(2);
            pie.Controls.Add(btnMenuPrincipal, 0, 0);

            dgvTbl_Pedidos.Dock = DockStyle.Fill;
            dgvTbl_Pedidos.Margin = new Padding(2, 5, 2, 5);
            dgvTbl_Pedidos.MinimumSize = new Size(0, 120);
            dgvTbl_Pedidos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            pnlListado.Controls.Clear();
            pnlListado.Controls.Add(dgvTbl_Pedidos);
            pnlListado.Controls.Add(pie);
            pnlListado.Controls.Add(campos);
            pnlListado.Controls.Add(pnlListadoTitulo);

            btnCerrar.Visible = true;
            Action ajustarCerrar = () =>
            {
                btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnCerrar.Size = new Size(255, 50);
                btnCerrar.Location = new Point(Math.Max(12, pnlTitulo.ClientSize.Width - btnCerrar.Width - 18), 16);
                btnCerrar.BringToFront();
            };
            pnlTitulo.SizeChanged += (s, e) => ajustarCerrar();
            ajustarCerrar();
            MinimumSize = new Size(1024, 650);
            AutoScroll = true;
            ResumeLayout(true);
        }

        private static void PrepararTarjeta(Panel tarjeta, Label icono, Label titulo, Label valor)
        {
            tarjeta.Dock = DockStyle.Fill;
            tarjeta.Margin = new Padding(6);
            Action ajustar = () =>
            {
                int anchoTexto = Math.Max(90, tarjeta.ClientSize.Width - 104);
                int alto = tarjeta.ClientSize.Height;
                icono.SetBounds(14, 18, 58, Math.Max(54, alto - 36));
                titulo.SetBounds(84, 16, anchoTexto, 30);
                valor.SetBounds(84, 48, anchoTexto, Math.Max(42, alto - 54));
                titulo.AutoEllipsis = true;
                valor.AutoEllipsis = true;
                valor.TextAlign = ContentAlignment.MiddleLeft;
            };
            tarjeta.SizeChanged += (sender, e) => ajustar();
            ajustar();
        }

        private void PedidosForms_Load(object sender, EventArgs e)
        {
            try
            {
                CargarListasDeSeleccion();
                pedidos = pedidosNegocio.ConsultarPedidos();
                pedidosFiltrados = new List<Pedido>(pedidos);
                ActualizarTotales();
                MostrarPagina();
            }
            catch (Exception ex)
            {
                MensajesForms.Mostrar(this, ex.Message, "Consulta de pedidos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarListasDeSeleccion()
        {
            cboCodigoSucursal.Format += (sender, args) =>
            {
                Sucursal sucursal = args.ListItem as Sucursal;
                if (sucursal != null)
                    args.Value = sucursal.IDCodigoSucursal + " - " + sucursal.NombreSucursal;
            };
            cboCodigoSucursal.DataSource = sucursalesNegocio.ConsultarSucursales();
            cboCodigoSucursal.DisplayMember = "NombreSucursal";
            cboCodigoSucursal.ValueMember = "IDCodigoSucursal";
            cboCodigoSucursal.SelectedIndex = -1;

            cboCliente.Format += (sender, args) =>
            {
                Cliente cliente = args.ListItem as Cliente;
                if (cliente != null)
                    args.Value = cliente.IDCliente + " - " + cliente.NombreCliente;
            };
            cboCliente.DataSource = clientesNegocio.ConsultarClientes();
            cboCliente.DisplayMember = "NombreCliente";
            cboCliente.ValueMember = "IDCliente";
            cboCliente.SelectedIndex = -1;
        }

        private void ActualizarTotales()
        {
            // Las tarjetas reflejan todo el resultado cargado, no solo la página actual.
            label12.Text = pedidos.Count.ToString();
            label3.Text = pedidos.Count(p => p.Estado).ToString();
            label6.Text = pedidos.Count(p => !p.Estado).ToString();
        }

        private void MostrarPagina()
        {
            cargandoPagina = true;
            dgvTbl_Pedidos.DataSource = null;
            dgvTbl_Pedidos.DataSource = pedidosFiltrados.ToList();
            AjustarEncabezadosGrilla();
            ConfigurarEstiloGrilla();
            cargandoPagina = false;
        }

        private void dgvTbl_Pedidos_SelectionChanged(object sender, EventArgs e)
        {
            if (modoAgregarPedido || modoEditarPedido || cargandoPagina || dgvTbl_Pedidos.CurrentRow == null) return;
            Pedido pedido = dgvTbl_Pedidos.CurrentRow.DataBoundItem as Pedido;
            if (pedido == null) return;

            MostrarDatosPedido(pedido);
        }

        private void AjustarEncabezadosGrilla()
        {
            // Los títulos muestran los nombres de las columnas de Tbl_Pedidos.
            CambiarTituloColumna("IDPedido", "IDPedido");
            CambiarTituloColumna("IDCodigoSucursal", "IDCodigoSucursal");
            CambiarTituloColumna("IDCliente", "IDCliente");
            CambiarTituloColumna("TipoPedido", "TipoPedido");
            CambiarTituloColumna("FechaPedido", "FechaPedido");
            CambiarTituloColumna("Estado", "Estado");
        }

        private void ConfigurarEstiloGrilla()
        {
            // Aplica a Pedidos el formato de tabla usado por el listado de Sucursales.
            dgvTbl_Pedidos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTbl_Pedidos.RowHeadersWidth = 42;
            dgvTbl_Pedidos.ColumnHeadersHeight = 38;
            dgvTbl_Pedidos.RowTemplate.Height = 34;
            dgvTbl_Pedidos.EnableHeadersVisualStyles = false;
            dgvTbl_Pedidos.BackgroundColor = Color.FromArgb(18, 45, 75);
            dgvTbl_Pedidos.GridColor = Color.FromArgb(68, 111, 151);

            dgvTbl_Pedidos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(23, 76, 114);
            dgvTbl_Pedidos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTbl_Pedidos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvTbl_Pedidos.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dgvTbl_Pedidos.DefaultCellStyle.BackColor = Color.FromArgb(21, 51, 78);
            dgvTbl_Pedidos.DefaultCellStyle.ForeColor = Color.White;
            dgvTbl_Pedidos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 145, 210);
            dgvTbl_Pedidos.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvTbl_Pedidos.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvTbl_Pedidos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(29, 62, 91);
            dgvTbl_Pedidos.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;

            AsignarPesoColumna("IDPedido", 65F);
            AsignarPesoColumna("IDCodigoSucursal", 100F);
            AsignarPesoColumna("IDCliente", 80F);
            AsignarPesoColumna("TipoPedido", 115F);
            AsignarPesoColumna("FechaPedido", 145F);
            AsignarPesoColumna("Estado", 65F);

            if (dgvTbl_Pedidos.Columns["FechaPedido"] != null)
                dgvTbl_Pedidos.Columns["FechaPedido"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            if (dgvTbl_Pedidos.Columns["Estado"] != null)
                dgvTbl_Pedidos.Columns["Estado"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void AsignarPesoColumna(string nombre, float peso)
        {
            if (dgvTbl_Pedidos.Columns[nombre] != null)
                dgvTbl_Pedidos.Columns[nombre].FillWeight = peso;
        }

        private void CambiarTituloColumna(string nombre, string titulo)
        {
            if (dgvTbl_Pedidos.Columns.Contains(nombre))
                dgvTbl_Pedidos.Columns[nombre].HeaderText = titulo;
        }

        private void MostrarDatosPedido(Pedido pedido)
        {
            idPedidoSeleccionado = pedido.IDPedido;
            txtIDPedido.Text = pedido.IDPedido.ToString();
            cboCodigoSucursal.SelectedValue = pedido.IDCodigoSucursal;
            cboCliente.SelectedValue = pedido.IDCliente;
            cboTipoPedido.Text = pedido.TipoPedido;
            dtpFechaPedido.Value = pedido.FechaPedido;
            rdbEstadoActivo.Checked = pedido.Estado;
            rdbEstadoInactivo.Checked = !pedido.Estado;

        }

        private void dgvTbl_Pedidos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (modoAgregarPedido || e.RowIndex < 0) return;
            Pedido pedido = dgvTbl_Pedidos.Rows[e.RowIndex].DataBoundItem as Pedido;
            if (pedido == null) return;

            MostrarDatosPedido(pedido);
            try
            {
                List<DetallePedido> detalles = pedidosNegocio.ConsultarDetallesPorPedido(pedido.IDPedido);
                if (formularioDetalle == null || formularioDetalle.IsDisposed)
                    formularioDetalle = new DetallePedidofomrs();

                formularioDetalle.MostrarPedido(pedido.IDPedido, detalles);
                if (!formularioDetalle.Visible)
                    formularioDetalle.Show(this);
                else
                    formularioDetalle.BringToFront();
            }
            catch (Exception ex)
            {
                MensajesForms.Mostrar(this, ex.Message, "Consulta de detalle", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBuscarPedido_Click(object sender, EventArgs e)
        {
            BuscarPedidosEnGrilla();
        }

        private void BuscarPedidosEnGrilla()
        {
            try { idPedidoSeleccionado = 0; pedidosFiltrados = pedidosNegocio.BuscarPedidos(txtBuscarPedido.Text); MostrarPagina(); }
            catch (Exception ex) { MensajesForms.Mostrar(this, ex.Message, "Búsqueda de pedidos", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void txtBuscarPedido_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            BuscarPedidosEnGrilla();
            e.SuppressKeyPress = true;
        }

        private void btnLimpiarBusquedaPedido_Click(object sender, EventArgs e)
        {
            idPedidoSeleccionado = 0;
            txtBuscarPedido.Clear();
            BuscarPedidosEnGrilla();
        }

        private void btnEditarPedido_Click(object sender, EventArgs e)
        {
            if (idPedidoSeleccionado <= 0 || modoAgregarPedido)
            { MensajesForms.Mostrar(this, "Selecciona un pedido de la tabla para editar.", "Editar pedido", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            modoEditarPedido = true;
            cboCodigoSucursal.Enabled = true; cboCliente.Enabled = true; cboTipoPedido.Enabled = true;
            dtpFechaPedido.Enabled = true; rdbEstadoActivo.Enabled = true; rdbEstadoInactivo.Enabled = true;
        }

        private void btnEliminarPedido_Click(object sender, EventArgs e)
        {
            if (idPedidoSeleccionado <= 0)
            { MensajesForms.Mostrar(this, "Selecciona un pedido de la tabla para eliminar.", "Eliminar pedido", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MensajesForms.Mostrar(this, "Se eliminarán el encabezado y sus detalles. ¿Deseas continuar?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                int id = idPedidoSeleccionado;
                pedidosNegocio.EliminarPedido(id);
                if (formularioDetalle != null && !formularioDetalle.IsDisposed && formularioDetalle.Visible)
                    formularioDetalle.LimpiarPedido();
                pedidos = pedidosNegocio.ConsultarPedidos(); pedidosFiltrados = new List<Pedido>(pedidos);
                ActualizarTotales(); idPedidoSeleccionado = 0; MostrarPagina(); btnLimpiarFormularioPedido_Click(sender, e);
                MensajesForms.Mostrar(this, "El pedido y sus detalles se eliminaron correctamente.", "Pedido eliminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MensajesForms.Mostrar(this, ex.Message, "No se pudo eliminar el pedido", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnLimpiarFormularioPedido_Click(object sender, EventArgs e)
        {
            idPedidoSeleccionado = 0;
            txtIDPedido.Clear();
            cboCodigoSucursal.SelectedIndex = -1;
            cboCliente.SelectedIndex = -1;
            cboTipoPedido.SelectedIndex = -1;
            dtpFechaPedido.Value = DateTime.Today;
            rdbEstadoActivo.Checked = false;
            rdbEstadoInactivo.Checked = false;
            cargandoPagina = true;
            dgvTbl_Pedidos.ClearSelection();
            cargandoPagina = false;
        }

        private void btnAgregarPedido_Click(object sender, EventArgs e)
        {
            if (modoAgregarPedido || modoEditarPedido)
            {
                MensajesForms.Mostrar(this, "Completa o cancela el pedido que estás preparando.", "Pedido nuevo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            modoAgregarPedido = true;
            modoEditarPedido = false;
            cboCodigoSucursal.Enabled = true; cboCliente.Enabled = true; cboTipoPedido.Enabled = true;
            dtpFechaPedido.Enabled = true; rdbEstadoActivo.Enabled = true; rdbEstadoInactivo.Enabled = true;
            txtIDPedido.Clear();
            cboCodigoSucursal.SelectedIndex = -1;
            cboCliente.SelectedIndex = -1;
            cboTipoPedido.SelectedIndex = -1;
            dtpFechaPedido.Value = DateTime.Now;
            rdbEstadoActivo.Checked = false;
            rdbEstadoInactivo.Checked = false;
            dgvTbl_Pedidos.ClearSelection();

        }

        private void btnGuardarPedido_Click(object sender, EventArgs e)
        {
            if (!modoAgregarPedido && !modoEditarPedido)
            {
                MensajesForms.Mostrar(this, "Presiona Nuevo para preparar un pedido.", "Guardar pedido",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (cboCodigoSucursal.SelectedValue == null)
            {
                MensajesForms.Mostrar(this, "Selecciona una sucursal.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCodigoSucursal.Focus();
                return;
            }
            if (cboCliente.SelectedValue == null)
            {
                MensajesForms.Mostrar(this, "Selecciona un cliente.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCliente.Focus();
                return;
            }
            if (cboTipoPedido.SelectedItem == null)
            {
                MensajesForms.Mostrar(this, "Selecciona el tipo de pedido.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboTipoPedido.Focus();
                return;
            }
            if (!rdbEstadoActivo.Checked && !rdbEstadoInactivo.Checked)
            {
                MensajesForms.Mostrar(this, "Selecciona el estado del pedido.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Pedido pedido = new Pedido
                {
                    IDCodigoSucursal = Convert.ToInt32(cboCodigoSucursal.SelectedValue),
                    IDCliente = Convert.ToInt32(cboCliente.SelectedValue),
                    TipoPedido = cboTipoPedido.SelectedItem.ToString(),
                    FechaPedido = dtpFechaPedido.Value,
                    Estado = rdbEstadoActivo.Checked
                };
                if (modoEditarPedido) pedido.IDPedido = idPedidoSeleccionado;
                int idGenerado = modoEditarPedido ? pedido.IDPedido : pedidosNegocio.AgregarPedido(pedido);
                if (modoEditarPedido) pedidosNegocio.EditarPedido(pedido);
                bool fueEdicion = modoEditarPedido;
                modoAgregarPedido = false;
                modoEditarPedido = false;
                cboCodigoSucursal.Enabled = false; cboCliente.Enabled = false; cboTipoPedido.Enabled = false;
                dtpFechaPedido.Enabled = false; rdbEstadoActivo.Enabled = false; rdbEstadoInactivo.Enabled = false;
                try
                {
                    RecargarYMostrarPedido(idGenerado);
                }
                catch (Exception ex)
                {
                    if (formularioDetalle != null && !formularioDetalle.IsDisposed)
                        formularioDetalle.CancelarCapturaDetalle();
                    MensajesForms.Mostrar(this, "El pedido se guardó, pero no se pudo actualizar la lista. Código: " +
                        idGenerado + Environment.NewLine + ex.Message, "Pedido guardado",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MensajesForms.Mostrar(this, (fueEdicion ? "El encabezado se actualizó correctamente. Código: " : "El encabezado se guardó correctamente. Código: ") + idGenerado +
                    ". Haz doble clic en el pedido y luego pulsa Nuevo en Detalles para agregar sus líneas.",
                    "Pedido agregado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MensajesForms.Mostrar(this, ex.Message, "No se pudo guardar el pedido", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void RecargarYMostrarPedido(int idPedido)
        {
            pedidos = pedidosNegocio.ConsultarPedidos();
            pedidosFiltrados = new List<Pedido>(pedidos);
            ActualizarTotales();
            txtBuscarPedido.Clear();

            int indice = pedidos.FindIndex(p => p.IDPedido == idPedido);
            MostrarPagina();

            if (indice >= 0)
            {
                int fila = indice;
                if (fila < dgvTbl_Pedidos.Rows.Count)
                {
                    dgvTbl_Pedidos.ClearSelection();
                    dgvTbl_Pedidos.Rows[fila].Selected = true;
                    dgvTbl_Pedidos.CurrentCell = dgvTbl_Pedidos.Rows[fila].Cells[0];
                    MostrarDatosPedido(pedidos[indice]);
                }
            }
        }

        private void btnCancelarPedido_Click(object sender, EventArgs e)
        {
            modoAgregarPedido = false;
            modoEditarPedido = false;
            cboCodigoSucursal.Enabled = false; cboCliente.Enabled = false; cboTipoPedido.Enabled = false;
            dtpFechaPedido.Enabled = false; rdbEstadoActivo.Enabled = false; rdbEstadoInactivo.Enabled = false;
            btnLimpiarFormularioPedido_Click(sender, e);
        }
    }
}
