using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    public partial class PagosForms : Form
    {
        private const int RegistrosPorPagina = 10;
        private readonly PagoNegocio pagoNegocio = new PagoNegocio();
        private readonly PedidoNegocio pedidoNegocio = new PedidoNegocio();
        private readonly Button btnPaginaAnterior = new Button();
        private readonly Button btnPaginaSiguiente = new Button();
        private List<Pago> pagosFiltrados = new List<Pago>();
        private int paginaActual;
        private bool modoEdicion;

        public PagosForms()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            button2.Visible = false;
            ConfigurarFormulario();
            ConfigurarLayoutResponsivo();
            Resize += (sender, e) => AjustarEncabezadoResponsivo();
            AjustarEncabezadoResponsivo();
            ConectarEventos();
            CargarRelaciones();
            CargarPagos();
        }

        private void ConfigurarFormulario()
        {
            // Los ComboBox conservan sus opciones originales del diseñador.
            txtCodigoPagos.ReadOnly = true;
            txtCodigoPagos.ForeColor = System.Drawing.SystemColors.ControlText;
            txtCodigoPedido.DropDownStyle = ComboBoxStyle.DropDownList;
            txtCodigoPrestamo.DropDownStyle = ComboBoxStyle.DropDownList;
            txtCodigoDevoluciones.DropDownStyle = ComboBoxStyle.DropDownList;
            txtCodigoPedido.ForeColor = System.Drawing.SystemColors.ControlText;
            txtCodigoPrestamo.ForeColor = System.Drawing.SystemColors.ControlText;
            txtCodigoDevoluciones.ForeColor = System.Drawing.SystemColors.ControlText;
            txtComprobante.MaxLength = 50;
            txtBuscarMedicamento.MaxLength = 100;

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(10, 21, 37);
            dataGridView1.GridColor = System.Drawing.Color.FromArgb(35, 70, 96);
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(16, 179, 199);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dataGridView1.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(18, 45, 75);
            dataGridView1.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dataGridView1.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(11, 127, 149);
            dataGridView1.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(14, 29, 49);
            dataGridView1.AlternatingRowsDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dataGridView1.RowHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(14, 29, 49);
            dataGridView1.RowHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
        }

        private void ConfigurarLayoutResponsivo()
        {
            MinimumSize = new Size(900, 650);
            AutoScroll = true;
            pnlTop.Visible = false;
            pnlContenido.Padding = new Padding(18, 10, 18, 14);

            tlpStats.Dock = DockStyle.Top;
            tlpStats.Height = 156;
            tlpStats.MinimumSize = new Size(0, 156);
            tlpStats.Padding = Padding.Empty;
            tlpStats.ColumnCount = 3;
            tlpStats.RowCount = 1;
            tlpStats.ColumnStyles.Clear();
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
            PrepararTarjeta(panel4, label10, label11, label12);
            PrepararTarjeta(panel2, label4, label5, label6);
            PrepararTarjeta(panel1, label1, label2, label3);

            LayoutPantallasTablas.AgregarBotonCerrarSesion(pnlTitulo);
            lblTitulo.Text = "Pagos";
            lblTitulo.AutoSize = false;
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            lblTitulo.Font = new Font("Segoe UI", 23F, FontStyle.Bold);
            lblSubtitulo.Text = "Gestión del catálogo de pagos de todas las sucursales.";
            lblSubtitulo.AutoSize = false;
            lblSubtitulo.TextAlign = ContentAlignment.MiddleLeft;
            lblSubtitulo.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            pnlListadoTitulo.Controls.Clear();
            pnlListadoTitulo.Dock = DockStyle.Top;
            pnlListadoTitulo.AutoSize = true;
            pnlListadoTitulo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlListadoTitulo.Padding = Padding.Empty;
            var barra = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(8, 5, 8, 7),
                Margin = Padding.Empty,
                BackColor = pnlListadoTitulo.BackColor
            };
            lblListado.Text = "Listado de Pagos";
            lblListado.AutoSize = true;
            lblListado.MinimumSize = new Size(220, 42);
            lblListado.AutoEllipsis = false;
            lblListado.TextAlign = ContentAlignment.MiddleLeft;
            lblListado.Margin = new Padding(3);
            lblListado.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            barra.Controls.Add(lblListado);

            Button[] botonesEdicion = { button1, button6, button5, button4, button3, button7 };
            string[] textosEdicion = { "Nuevo", "Guardar", "Eliminar", "Cancelar", "Editar", "Limpiar" };
            for (int i = 0; i < botonesEdicion.Length; i++)
            {
                botonesEdicion[i].Text = textosEdicion[i];
                botonesEdicion[i].Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                botonesEdicion[i].Size = new Size(104, 40);
                botonesEdicion[i].MinimumSize = new Size(96, 40);
                botonesEdicion[i].Margin = new Padding(3);
                barra.Controls.Add(botonesEdicion[i]);
            }
            txtBuscarMedicamento.Size = new Size(180, 32);
            txtBuscarMedicamento.MinimumSize = new Size(145, 28);
            txtBuscarMedicamento.Margin = new Padding(3, 7, 3, 3);
            barra.Controls.Add(txtBuscarMedicamento);
            Button[] botonesBusqueda = { btnBuscar, button2 };
            string[] textosBusqueda = { "Buscar", "Limpiar" };
            for (int i = 0; i < botonesBusqueda.Length; i++)
            {
                botonesBusqueda[i].Text = textosBusqueda[i];
                botonesBusqueda[i].Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                botonesBusqueda[i].Size = new Size(104, 40);
                botonesBusqueda[i].MinimumSize = new Size(96, 40);
                botonesBusqueda[i].Margin = new Padding(3);
                barra.Controls.Add(botonesBusqueda[i]);
            }
            pnlListadoTitulo.Controls.Add(barra);

            var campos = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(10, 6, 10, 6),
                Margin = Padding.Empty,
                BackColor = pnlListado.BackColor
            };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            for (int fila = 0; fila < 4; fila++)
                campos.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            AgregarCampo(campos, label7, txtCodigoPagos, 0, 0);
            AgregarCampo(campos, label9, cbocTipoPago, 2, 0);
            AgregarCampo(campos, label15, txtCodigoPedido, 0, 1);
            AgregarCampo(campos, label13, cboxTipoServicio, 2, 1);
            AgregarCampo(campos, label8, txtCodigoPrestamo, 0, 2);
            AgregarCampo(campos, label14, txtComprobante, 2, 2);
            AgregarCampo(campos, label16, txtCodigoDevoluciones, 0, 3);

            var pie = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = pnlListado.BackColor,
                Padding = new Padding(4, 4, 4, 4)
            };
            button8.Dock = DockStyle.Left;
            button8.Size = new Size(180, 50);
            button8.MinimumSize = new Size(150, 44);
            button8.Margin = new Padding(2);
            button9.Dock = DockStyle.Right;
            button9.Size = new Size(150, 50);
            button9.Margin = new Padding(2);
            pie.Controls.Add(button8);
            pie.Controls.Add(button9);
            ConfigurarPaginacion(pie);

            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.MinimumSize = new Size(0, 120);
            dataGridView1.Margin = new Padding(2, 6, 2, 6);
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            var listado = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = pnlListado.BackColor
            };
            listado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            listado.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            listado.RowStyles.Add(new RowStyle(SizeType.Absolute, 224F));
            listado.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            listado.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            listado.Controls.Add(pnlListadoTitulo, 0, 0);
            listado.Controls.Add(campos, 0, 1);
            listado.Controls.Add(dataGridView1, 0, 2);
            listado.Controls.Add(pie, 0, 3);

            pnlListado.Controls.Clear();
            pnlListado.Dock = DockStyle.Fill;
            pnlListado.Padding = new Padding(12, 8, 12, 8);
            pnlListado.Controls.Add(listado);

            pnlTitulo.Dock = DockStyle.Fill;
            var principal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                BackColor = pnlContenido.BackColor
            };
            principal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 164F));
            principal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            principal.Controls.Add(pnlTitulo, 0, 0);
            principal.Controls.Add(tlpStats, 0, 1);
            principal.Controls.Add(pnlListado, 0, 2);
            pnlContenido.Controls.Clear();
            pnlContenido.Padding = new Padding(18, 10, 18, 14);
            pnlContenido.Controls.Add(principal);
        }

        private void AjustarEncabezadoResponsivo()
        {
            if (pnlTitulo == null || lblTitulo == null || lblSubtitulo == null)
                return;

            int anchoDisponible = Math.Max(280, pnlTitulo.ClientSize.Width - 430);
            lblTitulo.SetBounds(100, 0, anchoDisponible, 54);
            lblSubtitulo.SetBounds(102, 54, anchoDisponible, 32);
        }

        private static void PrepararTarjeta(Panel tarjeta, Label icono, Label titulo, Label valor)
        {
            tarjeta.Dock = DockStyle.Fill;
            tarjeta.Margin = new Padding(4);
            Action ajustar = () =>
            {
                int ancho = Math.Max(90, tarjeta.ClientSize.Width - 108);
                icono.SetBounds(14, 22, 58, 66);
                titulo.SetBounds(84, 19, ancho, 30);
                valor.SetBounds(84, 50, ancho, 62);
                titulo.AutoEllipsis = true;
                valor.AutoSize = false;
                valor.Font = new Font("Segoe UI", 23F, FontStyle.Bold);
                valor.TextAlign = ContentAlignment.MiddleLeft;
            };
            tarjeta.SizeChanged += (sender, e) => ajustar();
            ajustar();
        }

        private static void AgregarCampo(TableLayoutPanel tabla, Control etiqueta, Control campo, int columna, int fila)
        {
            etiqueta.Dock = DockStyle.Fill;
            etiqueta.Margin = new Padding(4, 3, 8, 3);
            Label etiquetaLabel = etiqueta as Label;
            if (etiquetaLabel != null)
            {
                etiquetaLabel.AutoSize = false;
                etiquetaLabel.TextAlign = ContentAlignment.MiddleLeft;
            }
            campo.Dock = DockStyle.Fill;
            campo.Margin = new Padding(4, 7, 10, 7);
            campo.MinimumSize = new Size(70, 28);
            tabla.Controls.Add(etiqueta, columna, fila);
            tabla.Controls.Add(campo, columna + 1, fila);
        }

        private void CargarRelaciones()
        {
            try
            {
                var pedidos = pedidoNegocio.Consultar();
                txtCodigoPedido.DataSource = null;
                txtCodigoPedido.DisplayMember = "Descripcion";
                txtCodigoPedido.ValueMember = "IDPedido";
                txtCodigoPedido.DataSource = pedidos;
                txtCodigoPedido.SelectedIndex = -1;
                txtCodigoPedido.Enabled = true;

                ConfigurarComboRelacion(txtCodigoPrestamo, pagoNegocio.ConsultarPrestamosRelacionados());
                ConfigurarComboRelacion(txtCodigoDevoluciones, pagoNegocio.ConsultarDevolucionesRelacionadas());
                if (pedidos.Count == 0 || txtCodigoPrestamo.Items.Count == 0 || txtCodigoDevoluciones.Items.Count == 0)
                    MessageBox.Show("Faltan pedidos, préstamos o devoluciones disponibles. No se pueden registrar pagos hasta que existan las relaciones requeridas.",
                        "Relaciones del pago", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                txtCodigoPedido.Enabled = false;
                txtCodigoPrestamo.Enabled = false;
                txtCodigoDevoluciones.Enabled = false;
                MessageBox.Show(ex.Message, "Relaciones del pago", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ConfigurarComboRelacion(ComboBox combo, List<KeyValuePair<int, string>> opciones)
        {
            combo.DataSource = null;
            combo.DisplayMember = "Value";
            combo.ValueMember = "Key";
            combo.DataSource = opciones;
            combo.SelectedIndex = -1;
            combo.Enabled = true;
        }

        private void ConfigurarPaginacion(Panel pie)
        {
            ConfigurarBotonPaginacion(btnPaginaAnterior, "◀ Anterior");
            ConfigurarBotonPaginacion(btnPaginaSiguiente, "Siguiente ▶");
            pie.Controls.Add(btnPaginaAnterior);
            pie.Controls.Add(btnPaginaSiguiente);
        }

        private static void ConfigurarBotonPaginacion(Button boton, string texto)
        {
            boton.BackColor = System.Drawing.Color.FromArgb(16, 179, 199);
            boton.Cursor = Cursors.Hand;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(11, 127, 149);
            boton.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            boton.ForeColor = System.Drawing.Color.White;
            boton.Dock = DockStyle.Right;
            boton.Size = new System.Drawing.Size(141, 50);
            boton.Text = texto;
            boton.UseVisualStyleBackColor = false;
        }

        private void ConectarEventos()
        {
            button1.Click += buttonNuevo_Click;
            button2.Click += buttonLimpiarBusqueda_Click;
            btnBuscar.Click += buttonBuscar_Click;
            button3.Click += buttonEditar_Click;
            button4.Click += buttonCancelar_Click;
            button5.Click += buttonEliminar_Click;
            button6.Click += buttonGuardar_Click;
            button7.Click += buttonLimpiarCampos_Click;
            btnPaginaAnterior.Click += buttonPaginaAnterior_Click;
            btnPaginaSiguiente.Click += buttonPaginaSiguiente_Click;
        }

        private void CargarPagos()
        {
            try
            {
                pagosFiltrados = pagoNegocio.Consultar();
                paginaActual = 0;
                ActualizarTotales();
                MostrarPagina();
            }
            catch (Exception ex)
            {
                pagosFiltrados = new List<Pago>();
                ActualizarTotales();
                MostrarPagina();
                MessageBox.Show(ex.Message, "Pagos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuscarPagos()
        {
            try
            {
                pagosFiltrados = pagoNegocio.Buscar(txtBuscarMedicamento.Text.Trim());
                paginaActual = 0;
                ActualizarTotales();
                MostrarPagina();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buscar pagos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarPagina()
        {
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(pagosFiltrados.Count / (double)RegistrosPorPagina));
            paginaActual = Math.Max(0, Math.Min(paginaActual, totalPaginas - 1));
            dataGridView1.DataSource = pagosFiltrados
                .Skip(paginaActual * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToList();
            dataGridView1.ClearSelection();
            AjustarEncabezados();
            lblListado.Text = "Listado de Pagos";
            btnPaginaAnterior.Enabled = paginaActual > 0;
            btnPaginaSiguiente.Enabled = paginaActual + 1 < totalPaginas;
        }

        private void AjustarEncabezados()
        {
            CambiarEncabezado("IDagos", "Código Pago");
            CambiarEncabezado("IDPedido", "Código Pedido");
            CambiarEncabezado("IDPrestamo", "Código Préstamo");
            CambiarEncabezado("IDDevoluciones", "Código Devolución");
            CambiarEncabezado("FechaPrestamo", "Fecha Préstamo");
            CambiarEncabezado("FechaDevolucion", "Fecha Devolución");
            CambiarEncabezado("TipoPedido", "Tipo Pedido");
            if (dataGridView1.Columns.Contains("IDPedido"))
                dataGridView1.Columns["IDPedido"].Visible = false;
            if (dataGridView1.Columns.Contains("IDPrestamo"))
                dataGridView1.Columns["IDPrestamo"].Visible = false;
            if (dataGridView1.Columns.Contains("IDDevoluciones"))
                dataGridView1.Columns["IDDevoluciones"].Visible = false;
        }

        private void CambiarEncabezado(string columna, string texto)
        {
            if (dataGridView1.Columns.Contains(columna))
                dataGridView1.Columns[columna].HeaderText = texto;
        }

        private void ActualizarTotales()
        {
            // El total se calcula con todos los resultados, no solo la página visible.
            label12.Text = pagosFiltrados.Count.ToString(CultureInfo.CurrentCulture);
            label5.Text = "Pagos de préstamo";
            label6.Text = pagosFiltrados.Count(pago => pago.TipoServicio == "Prestamo")
                .ToString(CultureInfo.CurrentCulture);
            label2.Text = "Pagos de devolución";
            label3.Text = pagosFiltrados.Count(pago => pago.TipoServicio == "Devolucion")
                .ToString(CultureInfo.CurrentCulture);
        }

        private Pago PagoSeleccionado()
        {
            return dataGridView1.SelectedRows.Count == 0
                ? null
                : dataGridView1.SelectedRows[0].DataBoundItem as Pago;
        }

        private void CargarPagoEnFormulario(Pago pago)
        {
            txtCodigoPagos.Text = pago.IDagos.ToString(CultureInfo.CurrentCulture);
            SeleccionarOpcion(cbocTipoPago, pago.TipoPago);
            SeleccionarOpcion(cboxTipoServicio, pago.TipoServicio);
            txtComprobante.Text = pago.Comprobante;
            txtCodigoPedido.SelectedValue = pago.IDPedido;
            txtCodigoPrestamo.SelectedValue = pago.IDPrestamo;
            txtCodigoDevoluciones.SelectedValue = pago.IDDevoluciones;
        }

        private static void SeleccionarOpcion(ComboBox combo, string valor)
        {
            // No cambia las opciones: selecciona el valor ya existente en el ComboBox.
            combo.SelectedIndex = combo.FindStringExact(valor);
        }

        private Pago LeerPagoDesdeFormulario()
        {
            int idPago, idPedido, idPrestamo, idDevolucion;
            int.TryParse(txtCodigoPagos.Text, out idPago);
            if (!ObtenerIdSeleccionado(txtCodigoPedido, out idPedido) ||
                !ObtenerIdSeleccionado(txtCodigoPrestamo, out idPrestamo) ||
                !ObtenerIdSeleccionado(txtCodigoDevoluciones, out idDevolucion))
                throw new ArgumentException("Escribe códigos numéricos válidos para pedido, préstamo y devolución.");

            if (cbocTipoPago.SelectedItem == null || cboxTipoServicio.SelectedItem == null)
                throw new ArgumentException("Selecciona las opciones de tipo de pago y tipo de servicio ya disponibles.");

            return new Pago
            {
                IDagos = idPago,
                TipoPago = cbocTipoPago.SelectedItem.ToString(),
                TipoServicio = cboxTipoServicio.SelectedItem.ToString(),
                Comprobante = txtComprobante.Text,
                IDPedido = idPedido,
                IDPrestamo = idPrestamo,
                IDDevoluciones = idDevolucion
            };
        }

        private static bool ObtenerIdSeleccionado(ComboBox combo, out int id)
        {
            id = 0;
            return combo.SelectedValue != null &&
                int.TryParse(Convert.ToString(combo.SelectedValue), out id) && id > 0;
        }

        private void LimpiarFormulario()
        {
            modoEdicion = false;
            button6.Text = "Guardar";
            txtCodigoPagos.Clear();
            cbocTipoPago.SelectedIndex = -1;
            cboxTipoServicio.SelectedIndex = -1;
            txtComprobante.Clear();
            txtCodigoPedido.SelectedIndex = -1;
            txtCodigoPrestamo.SelectedIndex = -1;
            txtCodigoDevoluciones.SelectedIndex = -1;
            dataGridView1.ClearSelection();
        }

        private void buttonNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            cbocTipoPago.Focus();
        }

        private void buttonLimpiarCampos_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void buttonLimpiarBusqueda_Click(object sender, EventArgs e)
        {
            txtBuscarMedicamento.Clear();
            CargarPagos();
            txtBuscarMedicamento.Focus();
        }

        private void buttonBuscar_Click(object sender, EventArgs e) { BuscarPagos(); }
        private void buttonPaginaAnterior_Click(object sender, EventArgs e) { paginaActual--; MostrarPagina(); }
        private void buttonPaginaSiguiente_Click(object sender, EventArgs e) { paginaActual++; MostrarPagina(); }

        private void buttonEditar_Click(object sender, EventArgs e)
        {
            Pago pago = PagoSeleccionado();
            if (pago == null)
            {
                MessageBox.Show("Selecciona una fila del listado para editar.", "Editar pago",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            CargarPagoEnFormulario(pago);
            modoEdicion = true;
            button6.Text = "Actualizar";
        }

        private void buttonCancelar_Click(object sender, EventArgs e) { LimpiarFormulario(); }

        private void buttonGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                Pago pago = LeerPagoDesdeFormulario();
                if (modoEdicion)
                {
                    if (!pagoNegocio.Editar(pago))
                        throw new InvalidOperationException("No se encontró el pago que deseas editar.");
                    MessageBox.Show("Pago actualizado.", "Pagos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    pagoNegocio.Agregar(pago);
                    MessageBox.Show("Pago agregado.", "Pagos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                CargarPagos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Guardar pago", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonEliminar_Click(object sender, EventArgs e)
        {
            Pago pago = PagoSeleccionado();
            if (pago == null)
            {
                MessageBox.Show("Selecciona una fila del listado para eliminar.", "Eliminar pago",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("¿Deseas eliminar el pago seleccionado?", "Confirmar eliminación",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                if (pagoNegocio.Eliminar(pago.IDagos))
                {
                    CargarPagos();
                    LimpiarFormulario();
                    MessageBox.Show("Pago eliminado.", "Pagos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show("No se encontró el pago.", "Pagos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Eliminar pago", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pnlListado_Paint(object sender, PaintEventArgs e)
        {
            // Evento generado por el diseñador; el listado se configura al cargar el formulario.
        }
    }
}
