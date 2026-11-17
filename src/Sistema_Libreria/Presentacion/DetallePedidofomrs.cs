using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    public partial class DetallePedidofomrs : Form
    {
        private readonly PedidosNegocio pedidosNegocio = new PedidosNegocio();
        private List<DetallePedido> detalles = new List<DetallePedido>();
        private List<DetallePedido> detallesFiltrados = new List<DetallePedido>();
        private bool modoAgregarDetalle;
        private bool modoEditarDetalle;
        private int idPedidoActual;
        private bool recalculandoTotal;

        public DetallePedidofomrs()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            btnLimpiarBusquedaDetallePedido.Visible = false;
            ConfigurarConsulta();
            ConfigurarLayoutResponsivo();
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Normal;
            NavegacionPantallas.Configurar(this);
            Resize += (sender, e) => AjustarEncabezadoResponsivo();
            AjustarEncabezadoResponsivo();
        }

        private void ConfigurarLayoutResponsivo()
        {
            MinimumSize = new Size(900, 650);
            AutoScroll = true;
            BackColor = Color.FromArgb(10, 21, 37);
            pnlMenu.Visible = false;
            pnlTop.Visible = false;

            pnlTitulo.Dock = DockStyle.Fill;
            pnlTitulo.Height = 96;
            lblTitulo.Text = "DETALLE DEL PEDIDO";
            lblTitulo.AutoSize = false;
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            lblTitulo.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblSubtitulo.Text = "Gestión de los detalles del pedido seleccionado.";
            lblSubtitulo.AutoSize = false;
            lblSubtitulo.TextAlign = ContentAlignment.MiddleLeft;
            lblSubtitulo.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.Size = new Size(255, 50);

            // Las tarjetas de resumen se ocultan para mantener la vista compacta,
            // siguiendo el diseño de DetalleDevolucionForms.
            tlpStats.Visible = false;
            panel4.Visible = panel2.Visible = panel1.Visible = false;

            pnlListadoTitulo.Controls.Clear();
            pnlListadoTitulo.Dock = DockStyle.Top;
            pnlListadoTitulo.AutoSize = true;
            var barra = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(6),
                Margin = Padding.Empty,
                BackColor = pnlListado.BackColor
            };
            lblListado.Text = "Detalle del pedido";
            lblListado.AutoSize = true;
            lblListado.AutoEllipsis = false;
            lblListado.MinimumSize = new Size(210, 42);
            lblListado.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblListado.TextAlign = ContentAlignment.MiddleLeft;
            barra.Controls.Add(lblListado);

            Button[] botonesEdicion = {
                btnAgregarDetallePedido, btnGuardarDetallePedido, btnEliminarDetallePedido,
                btnCancelarDetallePedido, btnEditarDetallePedido, btnLimpiarFormularioDetallePedido
            };
            string[] textosEdicion = { "Nuevo", "Guardar", "Eliminar", "Cancelar", "Editar", "Limpiar" };
            for (int i = 0; i < botonesEdicion.Length; i++)
            {
                botonesEdicion[i].Text = textosEdicion[i];
                botonesEdicion[i].Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                botonesEdicion[i].Size = new Size(108, 40);
                botonesEdicion[i].MinimumSize = new Size(96, 40);
                botonesEdicion[i].Margin = new Padding(3);
                barra.Controls.Add(botonesEdicion[i]);
            }
            txtBuscarDetallePedido.Size = new Size(170, 30);
            txtBuscarDetallePedido.MinimumSize = new Size(130, 28);
            txtBuscarDetallePedido.Margin = new Padding(3, 8, 3, 3);
            barra.Controls.Add(txtBuscarDetallePedido);
            foreach (Button accion in new[] { btnBuscarDetallePedido, btnLimpiarBusquedaDetallePedido })
            {
                accion.Text = accion == btnBuscarDetallePedido ? "Buscar" : "Limpiar";
                accion.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                accion.Size = new Size(108, 40);
                accion.MinimumSize = new Size(96, 40);
                accion.Margin = new Padding(3);
                barra.Controls.Add(accion);
            }
            pnlListadoTitulo.Controls.Add(barra);

            var campos = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(8, 4, 8, 4),
                Margin = Padding.Empty,
                BackColor = pnlListado.BackColor
            };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            for (int fila = 0; fila < 3; fila++)
                campos.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333F));
            AgregarCampo(campos, label7, txtIDDetallePedido, 0, 0);
            AgregarCampo(campos, label9, nudPrecio, 2, 0);
            AgregarCampo(campos, label15, txtIDPedido, 0, 1);
            AgregarCampo(campos, label14, txtObservaciones, 2, 1);
            AgregarCampo(campos, label8, nudCantidad, 0, 2);
            AgregarCampo(campos, label17, nudTotal, 2, 2);

            dgvTbl_DetallePedido.Dock = DockStyle.Fill;
            dgvTbl_DetallePedido.MinimumSize = new Size(0, 120);
            dgvTbl_DetallePedido.Margin = new Padding(4, 6, 4, 4);
            dgvTbl_DetallePedido.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            var listado = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10),
                Margin = Padding.Empty,
                BackColor = pnlListado.BackColor
            };
            listado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            listado.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            listado.RowStyles.Add(new RowStyle(SizeType.Absolute, 190F));
            listado.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            listado.Controls.Add(pnlListadoTitulo, 0, 0);
            listado.Controls.Add(campos, 0, 1);
            listado.Controls.Add(dgvTbl_DetallePedido, 0, 2);

            pnlListado.Controls.Clear();
            pnlListado.Dock = DockStyle.Fill;
            pnlListado.Padding = Padding.Empty;
            pnlListado.Controls.Add(listado);

            var pie = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = pnlListado.BackColor,
                Padding = new Padding(2)
            };
            btnMenuPrincipal.Visible = true;
            btnMenuPrincipal.Text = "Menu Principal";
            btnMenuPrincipal.Dock = DockStyle.Left;
            btnMenuPrincipal.Size = new Size(190, 48);
            btnMenuPrincipal.MinimumSize = new Size(160, 44);
            btnMenuPrincipal.Margin = new Padding(2);
            pie.Controls.Add(btnMenuPrincipal);

            var principal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(18, 10, 18, 14),
                BackColor = BackColor
            };
            principal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            principal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            principal.Controls.Add(pnlTitulo, 0, 0);
            principal.Controls.Add(pnlListado, 0, 1);
            principal.Controls.Add(pie, 0, 2);
            pnlContenido.Controls.Clear();
            pnlContenido.Dock = DockStyle.Fill;
            pnlContenido.Padding = Padding.Empty;
            pnlContenido.Controls.Add(principal);
            pnlDerecha.Controls.Clear();
            pnlDerecha.Dock = DockStyle.Fill;
            pnlDerecha.Controls.Add(pnlContenido);
            Controls.Clear();
            Controls.Add(pnlDerecha);
        }

        private void AjustarEncabezadoResponsivo()
        {
            if (pnlTitulo == null || lblTitulo == null || lblSubtitulo == null || btnCerrar == null)
                return;

            int anchoDisponible = Math.Max(280, pnlTitulo.ClientSize.Width - 430);
            lblTitulo.SetBounds(100, 0, anchoDisponible, 54);
            lblSubtitulo.SetBounds(102, 54, anchoDisponible, 32);
            btnCerrar.SetBounds(Math.Max(0, pnlTitulo.ClientSize.Width - btnCerrar.Width - 18), 18,
                btnCerrar.Width, btnCerrar.Height);
        }

        private static void AgregarCampo(TableLayoutPanel tabla, Control etiqueta, Control campo, int columna, int fila)
        {
            Label texto = etiqueta as Label;
            if (texto != null)
            {
                texto.AutoSize = false;
                texto.Dock = DockStyle.Fill;
                texto.TextAlign = ContentAlignment.MiddleLeft;
            }
            etiqueta.Margin = new Padding(4, 3, 8, 3);
            campo.Dock = DockStyle.Fill;
            campo.Margin = new Padding(4, 7, 10, 7);
            campo.MinimumSize = new Size(70, 28);
            tabla.Controls.Add(etiqueta, columna, fila);
            tabla.Controls.Add(campo, columna + 1, fila);
        }

        private void AjustarEncabezadoCompacto()
        {
            // Reserva alto suficiente para que titulo y subtitulo queden visibles juntos.
            pnlTitulo.Height = 76;

            lblTitulo.Text = "DETALLE DEL PEDIDO";
            lblTitulo.Location = new Point(80, 3);
            lblTitulo.Size = new Size(460, 32);
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;

            lblSubtitulo.Location = new Point(81, 38);
            lblSubtitulo.Size = new Size(700, 26);
            lblSubtitulo.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            lblSubtitulo.TextAlign = ContentAlignment.MiddleLeft;

            lblListado.Text = "🛒  Detalle Pedido";
            lblListado.Dock = DockStyle.None;
            lblListado.Location = new Point(0, 5);
            lblListado.Size = new Size(275, 53);
            lblListado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            // Mantiene los iconos en la vista de ejecución igual que en el diseñador.
            btnAgregarDetallePedido.Text = "➕  Nuevo";
            btnGuardarDetallePedido.Text = "💾  Guardar";
            btnEliminarDetallePedido.Text = "🗑️  Eliminar";
            btnCancelarDetallePedido.Text = "❌  Cancelar";
            btnEditarDetallePedido.Text = "✏️  Editar";
            btnLimpiarFormularioDetallePedido.Text = "🖌️  Limpiar";
            btnBuscarDetallePedido.Text = "🔍  Buscar";
            btnLimpiarBusquedaDetallePedido.Text = "🖌️  Limpiar";

            // El formulario se reduce al abrir; se fija una fuente legible para conservar los nombres.
            Button[] botonesAccion = {
                btnAgregarDetallePedido, btnGuardarDetallePedido, btnEliminarDetallePedido,
                btnCancelarDetallePedido, btnEditarDetallePedido, btnLimpiarFormularioDetallePedido,
                btnBuscarDetallePedido, btnLimpiarBusquedaDetallePedido
            };
            foreach (Button boton in botonesAccion)
            {
                boton.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                boton.TextAlign = ContentAlignment.MiddleCenter;
                boton.AutoEllipsis = false;
                boton.Padding = Padding.Empty;
            }
        }

        private void ConfigurarConsulta()
        {
            // Las claves solo se asignan desde la selección y la base de datos, no por digitación.
            txtIDDetallePedido.ReadOnly = true;
            txtIDPedido.ReadOnly = true;
            txtIDDetallePedido.TabStop = false;
            txtIDPedido.TabStop = false;

            dgvTbl_DetallePedido.AutoGenerateColumns = true;
            dgvTbl_DetallePedido.ReadOnly = true;
            dgvTbl_DetallePedido.AllowUserToAddRows = false;
            dgvTbl_DetallePedido.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTbl_DetallePedido.MultiSelect = false;

            label11.Text = "Total de detalles";
            label5.Text = "Unidades totales";
            label2.Text = "Importe total";

            decimal maximoDecimalSql = 999999999999999999m;
            nudCantidad.Maximum = int.MaxValue;
            nudPrecio.Maximum = maximoDecimalSql;
            nudTotal.Maximum = maximoDecimalSql;
            nudTotal.ReadOnly = true;
            nudCantidad.Enabled = false;
            nudPrecio.Enabled = false;
            txtObservaciones.Enabled = false;
            txtObservaciones.MaxLength = 100;
            nudCantidad.ValueChanged += nudDetalle_ValueChanged;
            nudPrecio.ValueChanged += nudDetalle_ValueChanged;

            dgvTbl_DetallePedido.CellClick += dgvTbl_DetallePedido_CellClick;
            btnBuscarDetallePedido.Click += btnBuscarDetallePedido_Click;
            btnLimpiarBusquedaDetallePedido.Click += btnLimpiarBusquedaDetallePedido_Click;
            txtBuscarDetallePedido.KeyDown += txtBuscarDetallePedido_KeyDown;
            btnLimpiarFormularioDetallePedido.Click += btnLimpiarFormularioDetallePedido_Click;
            btnAgregarDetallePedido.Click += btnAgregarDetallePedido_Click;
            btnGuardarDetallePedido.Click += btnGuardarDetallePedido_Click;
            btnCancelarDetallePedido.Click += btnCancelarDetallePedido_Click;
            btnEditarDetallePedido.Click += btnEditarDetallePedido_Click;
            btnEliminarDetallePedido.Click += btnEliminarDetallePedido_Click;
            btnCerrar.Click += btnCerrar_Click;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Form formularioPedidos = Owner;
            Close();
            if (formularioPedidos != null && !formularioPedidos.IsDisposed)
            {
                formularioPedidos.Show();
                if (formularioPedidos.WindowState == FormWindowState.Minimized)
                    formularioPedidos.WindowState = FormWindowState.Normal;
                formularioPedidos.BringToFront();
                formularioPedidos.Activate();
            }
        }

        /// <summary>Muestra los detalles que pertenecen al encabezado indicado.</summary>
        public void MostrarPedido(int idPedido, List<DetallePedido> filas)
        {
            modoAgregarDetalle = false;
            modoEditarDetalle = false;
            idPedidoActual = idPedido;
            nudCantidad.Enabled = false;
            nudPrecio.Enabled = false;
            txtObservaciones.Enabled = false;
            txtIDPedido.Text = idPedido.ToString();
            detalles = filas ?? new List<DetallePedido>();
            detallesFiltrados = new List<DetallePedido>(detalles);
            txtBuscarDetallePedido.Clear();
            ActualizarTotales();
            MostrarResultados();
            LimpiarCamposDetalle();
            txtIDPedido.Text = idPedido.ToString();
        }

        private void ActualizarTotales()
        {
            label12.Text = detalles.Count.ToString();
            label6.Text = detalles.Sum(d => d.Cantidad).ToString();
            label3.Text = detalles.Sum(d => d.Total).ToString("0.##");
        }

        private void MostrarResultados()
        {
            dgvTbl_DetallePedido.DataSource = null;
            dgvTbl_DetallePedido.DataSource = detallesFiltrados;
            ConfigurarEstiloGrilla();
            CambiarTituloColumna("IDDetallePedido", "Detalle");
            CambiarTituloColumna("IDPedido", "Pedido");
            CambiarTituloColumna("Cantidad", "Cantidad");
            CambiarTituloColumna("Precio", "Precio");
            CambiarTituloColumna("Observaciones", "Observaciones");
            CambiarTituloColumna("Total", "Total");
        }

        private void ConfigurarEstiloGrilla()
        {
            dgvTbl_DetallePedido.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTbl_DetallePedido.RowHeadersWidth = 42;
            dgvTbl_DetallePedido.ColumnHeadersHeight = 38;
            dgvTbl_DetallePedido.RowTemplate.Height = 34;
            dgvTbl_DetallePedido.EnableHeadersVisualStyles = false;
            dgvTbl_DetallePedido.BackgroundColor = Color.FromArgb(18, 45, 75);
            dgvTbl_DetallePedido.GridColor = Color.FromArgb(68, 111, 151);

            dgvTbl_DetallePedido.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(23, 76, 114);
            dgvTbl_DetallePedido.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTbl_DetallePedido.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvTbl_DetallePedido.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dgvTbl_DetallePedido.DefaultCellStyle.BackColor = Color.FromArgb(21, 51, 78);
            dgvTbl_DetallePedido.DefaultCellStyle.ForeColor = Color.White;
            dgvTbl_DetallePedido.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 145, 210);
            dgvTbl_DetallePedido.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvTbl_DetallePedido.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvTbl_DetallePedido.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(29, 62, 91);
            dgvTbl_DetallePedido.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;

            AsignarPesoColumna("IDDetallePedido", 72F);
            AsignarPesoColumna("IDPedido", 72F);
            AsignarPesoColumna("Cantidad", 72F);
            AsignarPesoColumna("Precio", 90F);
            AsignarPesoColumna("Observaciones", 190F);
            AsignarPesoColumna("Total", 90F);
        }

        private void AsignarPesoColumna(string nombre, float peso)
        {
            if (dgvTbl_DetallePedido.Columns[nombre] != null)
                dgvTbl_DetallePedido.Columns[nombre].FillWeight = peso;
        }

        private void CambiarTituloColumna(string nombre, string titulo)
        {
            if (dgvTbl_DetallePedido.Columns.Contains(nombre))
                dgvTbl_DetallePedido.Columns[nombre].HeaderText = titulo;
        }

        private void dgvTbl_DetallePedido_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (modoAgregarDetalle || e.RowIndex < 0) return;
            DetallePedido detalle = dgvTbl_DetallePedido.Rows[e.RowIndex].DataBoundItem as DetallePedido;
            if (detalle == null) return;

            txtIDDetallePedido.Text = detalle.IDDetallePedido.ToString();
            txtIDPedido.Text = detalle.IDPedido.ToString();
            nudCantidad.Value = AjustarValor(nudCantidad, detalle.Cantidad);
            nudPrecio.Value = AjustarValor(nudPrecio, detalle.Precio);
            txtObservaciones.Text = detalle.Observaciones;
            nudTotal.Value = AjustarValor(nudTotal, detalle.Total);
        }

        private decimal AjustarValor(NumericUpDown control, decimal valor)
        {
            if (valor < control.Minimum) return control.Minimum;
            if (valor > control.Maximum) return control.Maximum;
            return valor;
        }

        private void btnBuscarDetallePedido_Click(object sender, EventArgs e)
        {
            BuscarDetallesEnGrilla();
        }

        private void BuscarDetallesEnGrilla()
        {
            try { txtIDDetallePedido.Clear(); detallesFiltrados = pedidosNegocio.BuscarDetallesPorPedido(idPedidoActual, txtBuscarDetallePedido.Text); MostrarResultados(); }
            catch (Exception ex) { MensajesForms.Mostrar(this, ex.Message, "Búsqueda de detalles", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void txtBuscarDetallePedido_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            BuscarDetallesEnGrilla();
            e.SuppressKeyPress = true;
        }

        private void btnLimpiarBusquedaDetallePedido_Click(object sender, EventArgs e)
        {
            txtIDDetallePedido.Clear();
            txtBuscarDetallePedido.Clear();
            BuscarDetallesEnGrilla();
        }

        private void btnLimpiarFormularioDetallePedido_Click(object sender, EventArgs e)
        {
            LimpiarCamposDetalle();
        }

        private void btnAgregarDetallePedido_Click(object sender, EventArgs e)
        {
            int idPedido;
            if (!int.TryParse(txtIDPedido.Text, out idPedido) || idPedido <= 0)
            {
                MensajesForms.Mostrar(this, "Primero guarda o selecciona un encabezado de pedido.", "Detalle de pedido",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            modoEditarDetalle = false;
            modoAgregarDetalle = true;
            nudCantidad.Enabled = true;
            nudPrecio.Enabled = true;
            txtObservaciones.Enabled = true;
            LimpiarCamposDetalle();
            txtIDPedido.Text = idPedido.ToString();
            nudCantidad.Focus();
        }

        private void btnGuardarDetallePedido_Click(object sender, EventArgs e)
        {
            if (!modoAgregarDetalle && !modoEditarDetalle)
            {
                MensajesForms.Mostrar(this, "Presiona Nuevo en el detalle antes de guardar.", "Detalle de pedido",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int idPedido;
            if (!int.TryParse(txtIDPedido.Text, out idPedido) || idPedido <= 0)
                return;
            if (nudCantidad.Value <= 0)
            {
                MensajesForms.Mostrar(this, "La cantidad debe ser mayor que cero.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudCantidad.Focus();
                return;
            }

            try
            {
                DetallePedido detalle = new DetallePedido
                {
                    IDPedido = idPedido,
                    Cantidad = Convert.ToInt32(nudCantidad.Value),
                    Precio = nudPrecio.Value,
                    Observaciones = txtObservaciones.Text.Trim(),
                    Total = nudCantidad.Value * nudPrecio.Value
                };

                int idDetalle;
                if (modoEditarDetalle)
                {
                    if (!int.TryParse(txtIDDetallePedido.Text, out idDetalle)) throw new InvalidOperationException("Selecciona un detalle válido.");
                    detalle.IDDetallePedido = idDetalle;
                    pedidosNegocio.EditarDetallePedido(detalle);
                }
                else idDetalle = pedidosNegocio.AgregarDetallePedido(detalle);
                bool fueEdicion = modoEditarDetalle;
                modoAgregarDetalle = false;
                modoEditarDetalle = false;
                nudCantidad.Enabled = false;
                nudPrecio.Enabled = false;
                txtObservaciones.Enabled = false;
                try
                {
                    MostrarPedido(idPedido, pedidosNegocio.ConsultarDetallesPorPedido(idPedido));
                }
                catch (Exception ex)
                {
                    MensajesForms.Mostrar(this, "El detalle quedó guardado con código " + idDetalle +
                        ", pero no se pudo actualizar la grilla. " + ex.Message,
                        "Detalle guardado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MensajesForms.Mostrar(this, fueEdicion ? "El detalle " + idDetalle + " se actualizó correctamente." : "El detalle " + idDetalle + " se guardó correctamente para el pedido " + idPedido + ".",
                    fueEdicion ? "Detalle actualizado" : "Detalle agregado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MensajesForms.Mostrar(this, ex.Message, "No se pudo guardar el detalle", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnEditarDetallePedido_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(txtIDDetallePedido.Text, out id) || id <= 0)
            { MensajesForms.Mostrar(this, "Selecciona un detalle de la tabla para editar.", "Editar detalle", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            modoEditarDetalle = true; modoAgregarDetalle = false;
            nudCantidad.Enabled = true; nudPrecio.Enabled = true; txtObservaciones.Enabled = true;
        }

        private void btnEliminarDetallePedido_Click(object sender, EventArgs e)
        {
            int idDetalle;
            if (!int.TryParse(txtIDDetallePedido.Text, out idDetalle) || idDetalle <= 0 || idPedidoActual <= 0)
            { MensajesForms.Mostrar(this, "Selecciona un detalle de la tabla para eliminar.", "Eliminar detalle", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MensajesForms.Mostrar(this, "¿Deseas eliminar el detalle seleccionado?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                pedidosNegocio.EliminarDetallePedido(idPedidoActual, idDetalle);
                MostrarPedido(idPedidoActual, pedidosNegocio.ConsultarDetallesPorPedido(idPedidoActual));
                MensajesForms.Mostrar(this, "El detalle se eliminó correctamente.", "Detalle eliminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MensajesForms.Mostrar(this, ex.Message, "No se pudo eliminar el detalle", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnCancelarDetallePedido_Click(object sender, EventArgs e)
        {
            CancelarCapturaDetalle();
        }

        public void CancelarCapturaDetalle()
        {
            int idPedido;
            modoAgregarDetalle = false;
            modoEditarDetalle = false;
            if (int.TryParse(txtIDPedido.Text, out idPedido) && idPedido > 0)
            {
                try
                {
                    MostrarPedido(idPedido, pedidosNegocio.ConsultarDetallesPorPedido(idPedido));
                }
                catch (Exception ex)
                {
                    MensajesForms.Mostrar(this, ex.Message, "Consulta de detalle", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                LimpiarCamposDetalle();
            }
        }

        private void nudDetalle_ValueChanged(object sender, EventArgs e)
        {
            if ((!modoAgregarDetalle && !modoEditarDetalle) || recalculandoTotal) return;
            decimal total = nudCantidad.Value * nudPrecio.Value;
            recalculandoTotal = true;
            nudTotal.Value = total > nudTotal.Maximum ? nudTotal.Maximum : total;
            recalculandoTotal = false;
        }

        private void LimpiarCamposDetalle()
        {
            txtIDDetallePedido.Clear();
            nudCantidad.Value = nudCantidad.Minimum;
            nudPrecio.Value = nudPrecio.Minimum;
            txtObservaciones.Clear();
            nudTotal.Value = nudTotal.Minimum;
            dgvTbl_DetallePedido.ClearSelection();
        }

        public void LimpiarPedido()
        {
            idPedidoActual = 0;
            detalles.Clear(); detallesFiltrados.Clear();
            txtIDPedido.Clear(); LimpiarCamposDetalle(); ActualizarTotales(); MostrarResultados();
        }
    }
}
