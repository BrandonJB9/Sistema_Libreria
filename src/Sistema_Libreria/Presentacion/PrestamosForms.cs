using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    public partial class PrestamosForms : Form
    {
        private const int RegistrosPorPagina = 10;
        private readonly PrestamoNegocio prestamoNegocio = new PrestamoNegocio();
        private List<Prestamo> prestamos = new List<Prestamo>();
        private List<Prestamo> prestamosFiltrados = new List<Prestamo>();
        private readonly Button btnPaginaAnterior = new Button();
        private readonly Button btnPaginaSiguiente = new Button();
        private Prestamo prestamoSeleccionado;
        private bool modoEdicion;
        private int paginaActual;

        public PrestamosForms()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            button2.Visible = false;
            ConfigurarLayoutResponsivo();
            Resize += (sender, e) => AjustarEncabezadoResponsivo();
            AjustarEncabezadoResponsivo();
            ConfigurarConsulta();
            dataGridView1.CellClick += dataGridView1_CellClick;
            button6.Click += buttonGuardar_Click;
            button3.Click += buttonEditar_Click;
            button1.Click += buttonNuevo_Click;
            button4.Click += buttonCancelar_Click;
            button7.Click += buttonNuevo_Click;
            button5.Click += buttonEliminar_Click;
            btnBuscar.Click += (sender, e) => AplicarFiltro();
            button2.Click += (sender, e) => { txtBuscarMedicamento.Clear(); AplicarFiltro(); };
            txtBuscarMedicamento.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter) { AplicarFiltro(); e.SuppressKeyPress = true; }
            };
            btnPaginaAnterior.Click += (sender, e) => { paginaActual--; MostrarPagina(); };
            btnPaginaSiguiente.Click += (sender, e) => { paginaActual++; MostrarPagina(); };
            txtCodigoCliente.DropDownStyle = ComboBoxStyle.DropDownList;
            txtCodigoSucursal.DropDownStyle = ComboBoxStyle.DropDownList;
            CargarRelaciones();
            Load += PrestamosForms_Load;
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }

        private void ConfigurarLayoutResponsivo()
        {
            MinimumSize = new Size(900, 650);
            AutoScroll = true;
            pnlTop.Visible = false;
            pnlContenido.Padding = new Padding(18, 10, 18, 14);

            tlpStats.Dock = DockStyle.Fill;
            tlpStats.Height = 156;
            tlpStats.Padding = Padding.Empty;
            tlpStats.Margin = new Padding(0, 4, 0, 8);
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
            pnlTitulo.Dock = DockStyle.Fill;
            lblTitulo.Text = "Préstamos";
            lblTitulo.AutoSize = false;
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            lblTitulo.Font = new Font("Segoe UI", 23F, FontStyle.Bold);
            lblSubtitulo.Text = "Gestión del catálogo de préstamos de todas las sucursales.";
            lblSubtitulo.AutoSize = false;
            lblSubtitulo.TextAlign = ContentAlignment.MiddleLeft;
            lblSubtitulo.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            pnlListadoTitulo.Controls.Clear();
            pnlListadoTitulo.Dock = DockStyle.Top;
            pnlListadoTitulo.AutoSize = true;
            pnlListadoTitulo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
            lblListado.Text = "Listado de Préstamos";
            lblListado.AutoSize = true;
            lblListado.AutoEllipsis = false;
            lblListado.MinimumSize = new Size(220, 42);
            lblListado.Margin = new Padding(3);
            lblListado.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblListado.TextAlign = ContentAlignment.MiddleLeft;
            barra.Controls.Add(lblListado);

            Button[] botonesEdicion = { button1, button6, button5, button4, button3, button7 };
            string[] textosEdicion = { "➕ Nuevo", "💾 Guardar", "🗑️ Eliminar", "❌ Cancelar", "✏️ Editar", "🧹 Limpiar" };
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
            foreach (Button accion in new[] { btnBuscar, button2 })
            {
                accion.Text = accion == btnBuscar ? "🔍 Buscar" : "🧹 Limpiar";
                accion.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                accion.Size = new Size(104, 40);
                accion.MinimumSize = new Size(96, 40);
                accion.Margin = new Padding(3);
                barra.Controls.Add(accion);
            }
            pnlListadoTitulo.Controls.Add(barra);
            Action ajustarBarra = () =>
            {
                int ancho = Math.Max(300, pnlListadoTitulo.ClientSize.Width);
                int alto = Math.Max(54, barra.GetPreferredSize(new Size(ancho, 0)).Height);
                if (pnlListadoTitulo.MinimumSize.Height != alto)
                    pnlListadoTitulo.MinimumSize = new Size(0, alto);
            };
            pnlListadoTitulo.SizeChanged += (sender, e) => ajustarBarra();
            barra.Layout += (sender, e) => ajustarBarra();
            ajustarBarra();

            var estado = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(4, 8, 4, 4),
                Margin = Padding.Empty,
                BackColor = pnlListado.BackColor
            };
            rdbActivo.Margin = new Padding(4, 3, 18, 3);
            rdbInactivo.Margin = new Padding(4, 3, 4, 3);
            estado.Controls.Add(rdbActivo);
            estado.Controls.Add(rdbInactivo);

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
            AgregarCampo(campos, label7, txtCodigoPrestamo, 0, 0);
            AgregarCampo(campos, label13, nudDiasPrestamo, 2, 0);
            AgregarCampo(campos, label15, txtCodigoCliente, 0, 1);
            AgregarCampo(campos, label14, txtObservaciones, 2, 1);
            AgregarCampo(campos, label8, txtCodigoSucursal, 0, 2);
            AgregarCampo(campos, label17, estado, 2, 2);
            AgregarCampo(campos, label9, dtpFechaPrestamo, 0, 3);

            var pie = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = pnlListado.BackColor,
                Padding = new Padding(4)
            };
            button8.Dock = DockStyle.Left;
            button8.Text = "Menu Principal";
            button8.Size = new Size(190, 50);
            button8.MinimumSize = new Size(160, 44);
            button8.Margin = new Padding(2);
            button9.Dock = DockStyle.Right;
            button9.Size = new Size(150, 50);
            button9.Margin = new Padding(2);
            pie.Controls.Add(button8);
            pie.Controls.Add(button9);
            var paginacion = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false, Padding = new Padding(4)
            };
            foreach (Button boton in new[] { btnPaginaSiguiente, btnPaginaAnterior })
            {
                boton.Size = new Size(115, 42);
                boton.FlatStyle = FlatStyle.Flat;
                boton.BackColor = button8.BackColor;
                boton.ForeColor = button8.ForeColor;
                paginacion.Controls.Add(boton);
            }
            btnPaginaAnterior.Text = "Anterior";
            btnPaginaSiguiente.Text = "Siguiente";
            pie.Controls.Add(paginacion);
            paginacion.BringToFront();

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
            listado.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F));
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
            principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 136F));
            principal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            principal.Controls.Add(pnlTitulo, 0, 0);
            principal.Controls.Add(tlpStats, 0, 1);
            principal.Controls.Add(pnlListado, 0, 2);
            pnlContenido.Controls.Clear();
            pnlContenido.AutoScroll = true;
            principal.MinimumSize = new Size(0, 720);
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
                ConfigurarComboRelacion(txtCodigoCliente, prestamoNegocio.ConsultarClientesRelacionados());
                ConfigurarComboRelacion(txtCodigoSucursal, prestamoNegocio.ConsultarSucursalesRelacionadas());
                if (txtCodigoCliente.Items.Count == 0 || txtCodigoSucursal.Items.Count == 0)
                    MessageBox.Show("No hay clientes o sucursales disponibles. No se pueden registrar préstamos hasta que existan ambos.",
                        "Relaciones del préstamo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                txtCodigoCliente.Enabled = false;
                txtCodigoSucursal.Enabled = false;
                MessageBox.Show(ex.Message, "Relaciones del préstamo", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void buttonNuevo_Click(object sender, EventArgs e)
        {
            modoEdicion = false;
            prestamoSeleccionado = null;
            button6.Text = "💾 Guardar";
            button3.Enabled = false;
            dataGridView1.ClearSelection();
            txtCodigoPrestamo.Clear();
            txtCodigoCliente.SelectedIndex = -1;
            txtCodigoSucursal.SelectedIndex = -1;
            dtpFechaPrestamo.Value = DateTime.Today;
            nudDiasPrestamo.Value = nudDiasPrestamo.Minimum;
            txtObservaciones.Clear();
            rdbActivo.Checked = true;
            rdbInactivo.Checked = false;
        }

        private void buttonCancelar_Click(object sender, EventArgs e)
        {
            if (modoEdicion && prestamoSeleccionado != null)
            {
                modoEdicion = false;
                MostrarPrestamoEnControles(prestamoSeleccionado);
                button6.Text = "💾 Guardar";
                button3.Enabled = true;
                return;
            }
            buttonNuevo_Click(sender, e);
        }

        private void buttonGuardar_Click(object sender, EventArgs e)
        {
            int idCliente;
            int idSucursal;
            if (!ObtenerIdSeleccionado(txtCodigoCliente, out idCliente) ||
                !ObtenerIdSeleccionado(txtCodigoSucursal, out idSucursal))
            {
                MessageBox.Show("Ingrese códigos numéricos válidos de cliente y sucursal.",
                    "Datos del préstamo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var nuevo = new Prestamo
            {
                IDPrestamo = prestamoSeleccionado == null ? 0 : prestamoSeleccionado.IDPrestamo,
                IDCliente = idCliente,
                IDCodigoSucursal = idSucursal,
                FechaPrestamo = dtpFechaPrestamo.Value,
                DiasPrestamo = Decimal.ToInt32(nudDiasPrestamo.Value),
                Observaciones = txtObservaciones.Text,
                Estado = rdbActivo.Checked
            };

            if (modoEdicion)
            {
                try
                {
                    prestamoNegocio.Actualizar(nuevo);
                    modoEdicion = false;
                    CargarPrestamos();
                    SeleccionarPrestamoEnTabla(nuevo.IDPrestamo);
                    button6.Text = "💾 Guardar";
                    button3.Enabled = true;
                    MessageBox.Show("El préstamo fue actualizado.", "Préstamo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + (ex.InnerException == null ? string.Empty :
                        Environment.NewLine + ex.InnerException.GetBaseException().Message),
                        "Editar préstamo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            using (var formularioDetalles = new DetallePrestamoForms(nuevo, prestamos.Count,
                prestamos.Count(p => p.Estado), prestamos.Count(p => !p.Estado), true))
            {
                if (formularioDetalles.ShowDialog(this) != DialogResult.OK)
                    return;
                try
                {
                    int idGenerado = prestamoNegocio.AgregarConDetalles(nuevo, formularioDetalles.DetallesIngresados);
                    CargarPrestamos();
                    SeleccionarPrestamoEnTabla(idGenerado);
                    txtCodigoPrestamo.Text = idGenerado.ToString();
                    MessageBox.Show("Préstamo " + idGenerado + " guardado con sus detalles.",
                        "Préstamo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Guardar préstamo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static bool ObtenerIdSeleccionado(ComboBox combo, out int id)
        {
            id = 0;
            return combo.SelectedValue != null &&
                int.TryParse(Convert.ToString(combo.SelectedValue), out id) && id > 0;
        }

        private void ConfigurarConsulta()
        {
            // Se conservan los controles del diseñador; la pantalla solo permite consultar.
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = Color.FromArgb(10, 21, 37);
            dataGridView1.GridColor = Color.FromArgb(35, 70, 96);
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(16, 179, 199);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(18, 45, 75);
            dataGridView1.DefaultCellStyle.ForeColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(11, 127, 149);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(14, 29, 49);
            dataGridView1.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(14, 29, 49);
            dataGridView1.RowHeadersDefaultCellStyle.ForeColor = Color.White;
            nudDiasPrestamo.Minimum = 1;
            nudDiasPrestamo.Maximum = int.MaxValue;
        }

        private void PrestamosForms_Load(object sender, EventArgs e)
        {
            CargarPrestamos();
        }

        private void CargarPrestamos()
        {
            try
            {
                prestamos = prestamoNegocio.Consultar();
                ActualizarTotales();
                AplicarFiltro();
            }
            catch (Exception ex)
            {
                prestamos = new List<Prestamo>();
                prestamosFiltrados.Clear();
                ActualizarTotales();
                btnPaginaAnterior.Enabled = false;
                btnPaginaSiguiente.Enabled = false;
                dataGridView1.DataSource = null;
                MessageBox.Show(ex.Message, "Consulta de préstamos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarPagina()
        {
            // La paginación se aplica solo en Presentación; el total usa la lista completa.
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(prestamosFiltrados.Count / (double)RegistrosPorPagina));
            paginaActual = Math.Max(0, Math.Min(paginaActual, totalPaginas - 1));
            dataGridView1.DataSource = prestamosFiltrados.Skip(paginaActual * RegistrosPorPagina)
                .Take(RegistrosPorPagina).ToList();
            AjustarEncabezados();
            lblListado.Text = "Préstamos — página " + (paginaActual + 1) + " de " + totalPaginas;
            btnPaginaAnterior.Enabled = paginaActual > 0;
            btnPaginaSiguiente.Enabled = paginaActual + 1 < totalPaginas;
            prestamoSeleccionado = null;
            dataGridView1.ClearSelection();
            button3.Enabled = false;
        }

        private void AplicarFiltro()
        {
            string filtro = txtBuscarMedicamento.Text.Trim();
            prestamosFiltrados = prestamos.Where(p => string.IsNullOrEmpty(filtro) ||
                p.IDPrestamo.ToString() == filtro || p.IDCliente.ToString() == filtro ||
                p.IDCodigoSucursal.ToString() == filtro ||
                Contiene(p.NombreCliente, filtro) || Contiene(p.NombreSucursal, filtro) ||
                Contiene(p.Observaciones, filtro)).ToList();
            paginaActual = 0;
            MostrarPagina();
        }

        private static bool Contiene(string texto, string filtro)
        {
            return texto != null && texto.IndexOf(filtro, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void buttonEliminar_Click(object sender, EventArgs e)
        {
            if (prestamoSeleccionado == null)
            {
                MessageBox.Show("Selecciona primero un préstamo.", "Eliminar préstamo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int id = prestamoSeleccionado.IDPrestamo;
            if (MessageBox.Show("¿Eliminar el préstamo " + id + " y sus detalles?", "Confirmar eliminación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                prestamoNegocio.Eliminar(id);
                CargarPrestamos();
                buttonNuevo_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetBaseException().Message, "No se pudo eliminar el préstamo",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarTotales()
        {
            // Se calculan con todos los encabezados, no solo con las diez filas de la página.
            label12.Text = prestamos.Count.ToString();
            label3.Text = prestamos.Count(p => p.Estado).ToString();
            label6.Text = prestamos.Count(p => !p.Estado).ToString();
        }

        private void AjustarEncabezados()
        {
            CambiarEncabezado("IDPrestamo", "Código Préstamo");
            CambiarEncabezado("IDCliente", "Código Cliente");
            CambiarEncabezado("IDCodigoSucursal", "Código Sucursal");
            CambiarEncabezado("FechaPrestamo", "Fecha Préstamo");
            CambiarEncabezado("DiasPrestamo", "Días Préstamo");
            CambiarEncabezado("Observaciones", "Observaciones");
            CambiarEncabezado("Estado", "Activo");
            CambiarEncabezado("NombreCliente", "Cliente");
            CambiarEncabezado("NombreSucursal", "Sucursal");
            if (dataGridView1.Columns.Contains("IDCliente"))
                dataGridView1.Columns["IDCliente"].Visible = false;
            if (dataGridView1.Columns.Contains("IDCodigoSucursal"))
                dataGridView1.Columns["IDCodigoSucursal"].Visible = false;
        }

        private void CambiarEncabezado(string columna, string texto)
        {
            if (dataGridView1.Columns.Contains(columna))
                dataGridView1.Columns[columna].HeaderText = texto;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dataGridView1.Rows[e.RowIndex].DataBoundItem == null)
                return;

            Prestamo prestamo = dataGridView1.Rows[e.RowIndex].DataBoundItem as Prestamo;
            if (prestamo == null)
                return;

            prestamoSeleccionado = prestamo;
            MostrarPrestamoEnControles(prestamo);
            button3.Enabled = true;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dataGridView1.Rows[e.RowIndex].DataBoundItem == null)
                return;

            Prestamo prestamo = dataGridView1.Rows[e.RowIndex].DataBoundItem as Prestamo;
            if (prestamo == null)
                return;

            prestamoSeleccionado = prestamo;
            MostrarPrestamoEnControles(prestamo);
            button3.Enabled = true;
            using (var formularioDetalles = new DetallePrestamoForms(prestamo,
                prestamos.Count, prestamos.Count(p => p.Estado), prestamos.Count(p => !p.Estado)))
            {
                if (formularioDetalles.ShowDialog(this) == DialogResult.OK)
                    SeleccionarPrestamoEnTabla(prestamo.IDPrestamo);
            }
        }

        private void MostrarPrestamoEnControles(Prestamo prestamo)
        {
            txtCodigoPrestamo.Text = prestamo.IDPrestamo.ToString();
            txtCodigoCliente.SelectedValue = prestamo.IDCliente;
            txtCodigoSucursal.SelectedValue = prestamo.IDCodigoSucursal;
            dtpFechaPrestamo.Value = prestamo.FechaPrestamo;
            nudDiasPrestamo.Value = Math.Max(nudDiasPrestamo.Minimum,
                Math.Min(nudDiasPrestamo.Maximum, prestamo.DiasPrestamo));
            txtObservaciones.Text = prestamo.Observaciones ?? string.Empty;
            rdbActivo.Checked = prestamo.Estado;
            rdbInactivo.Checked = !prestamo.Estado;
        }

        private void buttonEditar_Click(object sender, EventArgs e)
        {
            if (prestamoSeleccionado == null)
            {
                MessageBox.Show("Selecciona primero una fila de préstamo.", "Editar préstamo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            modoEdicion = true;
            button6.Text = "💾 Guardar cambios";
            button3.Enabled = false;
        }

        private void SeleccionarPrestamoEnTabla(int idPrestamo)
        {
            int indice = prestamosFiltrados.FindIndex(p => p.IDPrestamo == idPrestamo);
            if (indice < 0)
            {
                txtBuscarMedicamento.Clear();
                AplicarFiltro();
                indice = prestamosFiltrados.FindIndex(p => p.IDPrestamo == idPrestamo);
            }
            if (indice < 0) return;
            paginaActual = indice / RegistrosPorPagina;
            MostrarPagina();
            foreach (DataGridViewRow fila in dataGridView1.Rows)
            {
                Prestamo filaPrestamo = fila.DataBoundItem as Prestamo;
                if (filaPrestamo == null || filaPrestamo.IDPrestamo != idPrestamo)
                    continue;

                dataGridView1.ClearSelection();
                fila.Selected = true;
                if (fila.Cells.Count > 0)
                    dataGridView1.CurrentCell = fila.Cells[0];
                prestamoSeleccionado = filaPrestamo;
                MostrarPrestamoEnControles(filaPrestamo);
                button3.Enabled = true;
                return;
            }
        }
    }
}
