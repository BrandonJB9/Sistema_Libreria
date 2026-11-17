using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    public partial class DetallePrestamoForms : Form
    {
        private readonly DetallePrestamoNegocio detalleNegocio = new DetallePrestamoNegocio();
        private Prestamo prestamoSeleccionado;
        private List<DetallePrestamo> detalles = new List<DetallePrestamo>();
        private List<DetallePrestamo> detallesVisibles = new List<DetallePrestamo>();
        private DetallePrestamo detalleSeleccionado;
        private readonly bool modoCaptura;
        private bool modoEdicion;
        private bool modoNuevo;
        private Button btnVolver;

        public DetallePrestamoForms()
            : this(null, 0, 0, 0)
        {
        }

        public DetallePrestamoForms(Prestamo prestamo, int totalPrestamos, int prestamosActivos, int prestamosInactivos)
            : this(prestamo, totalPrestamos, prestamosActivos, prestamosInactivos, false)
        {
        }

        public DetallePrestamoForms(Prestamo prestamo)
            : this(prestamo, 0, 0, 0, true)
        {
        }

        public DetallePrestamoForms(Prestamo prestamo, int totalPrestamos, int prestamosActivos,
            int prestamosInactivos, bool modoCaptura)
        {
            prestamoSeleccionado = prestamo;
            this.modoCaptura = modoCaptura;
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            button2.Visible = false;
            ConfigurarLayoutResponsivo();
            ConfigurarConsulta();
            label13.Text = "Descuento (%):";
            nudDescuento.Maximum = 100;
            nudTotal.ReadOnly = true;
            nudTotal.Increment = 0;
            nudCantidad.ValueChanged += RecalcularTotal;
            nudPrecio.ValueChanged += RecalcularTotal;
            nudDescuento.ValueChanged += RecalcularTotal;
            RecalcularTotal(this, EventArgs.Empty);
            label12.Text = totalPrestamos.ToString();
            label3.Text = prestamosActivos.ToString();
            label6.Text = prestamosInactivos.ToString();
            if (prestamoSeleccionado != null)
            {
                txtCodigoPrestamo.Text = modoCaptura
                    ? "Se genera al guardar"
                    : prestamoSeleccionado.IDPrestamo.ToString();
                lblListado.Text = "Detalles del préstamo " + prestamoSeleccionado.IDPrestamo;
            }

            Load += DetallePrestamoForms_Load;
            button1.Click += buttonNuevoDetalle_Click;
            button2.Click += buttonLimpiarBusqueda_Click;
            btnBuscar.Click += buttonBuscar_Click;
            button3.Click += buttonEditarDetalle_Click;
            button4.Click += buttonCancelarDetalle_Click;
            button5.Click += buttonEliminarDetalle_Click;
            button7.Click += buttonLimpiarDetalle_Click;
            dataGridView1.CellClick += dataGridView1_CellClick;

            if (modoCaptura)
            {
                button6.Text = "Agregar detalle";
                button9.Text = "Guardar préstamo";
                button6.Click += buttonAgregarDetalle_Click;
                button9.Click += buttonConfirmarDetalles_Click;
                ConfigurarBotones(false);
                button6.Enabled = true;
            }
            else
            {
                button6.Text = "Guardar";
                button9.Click += (sender, e) => Close();
                button6.Enabled = false;
                if (prestamoSeleccionado == null || prestamoSeleccionado.IDPrestamo <= 0)
                {
                    button1.Enabled = false;
                    button3.Enabled = false;
                    button5.Enabled = false;
                }
                button6.Click += buttonGuardarDetalle_Click;
            }

            NavegacionPantallas.Configurar(this);
        }

        private void ConfigurarLayoutResponsivo()
        {
            MinimumSize = new Size(900, 650);
            Size = new Size(1050, 760);
            StartPosition = FormStartPosition.CenterParent;
            AutoScroll = true;
            BackColor = Color.FromArgb(10, 21, 37);
            pnlTop.Visible = false;
            pnlMenu.Visible = false;
            tlpStats.Visible = false;
            pnlContenido.Controls.Remove(tlpStats);
            pnlContenido.Padding = new Padding(18, 10, 18, 14);

            pnlTitulo.Dock = DockStyle.Fill;
            lblTitulo.Text = "Detalle del préstamo";
            lblTitulo.Font = new Font("Segoe UI", 21F, FontStyle.Bold);
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            lblSubtitulo.Text = modoCaptura
                ? "Agrega los libros y guarda el préstamo completo."
                : "Consulta y administra los libros del préstamo seleccionado.";
            lblSubtitulo.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            lblSubtitulo.TextAlign = ContentAlignment.MiddleLeft;
            btnVolver = new Button
            {
                BackColor = Color.FromArgb(7, 18, 32),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(250, 46),
                Text = "Volver a préstamos",
                TextAlign = ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor = false
            };
            btnVolver.FlatAppearance.BorderSize = 0;
            btnVolver.Click += (sender, e) => Close();
            pnlTitulo.Controls.Add(btnVolver);
            pnlTitulo.SizeChanged += (sender, e) => AjustarEncabezadoResponsivo();
            AjustarEncabezadoResponsivo();

            pnlListadoTitulo.Controls.Clear();
            pnlListadoTitulo.Dock = DockStyle.Top;
            pnlListadoTitulo.AutoSize = true;
            pnlListadoTitulo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            var barra = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = pnlListado.BackColor,
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = Padding.Empty,
                Padding = new Padding(8, 5, 8, 7),
                WrapContents = true
            };
            lblListado.Text = prestamoSeleccionado == null
                ? "Detalles del préstamo"
                : "Detalles del préstamo " + prestamoSeleccionado.IDPrestamo;
            lblListado.AutoSize = true;
            lblListado.AutoEllipsis = true;
            lblListado.MinimumSize = new Size(220, 40);
            lblListado.Margin = new Padding(3);
            lblListado.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblListado.TextAlign = ContentAlignment.MiddleLeft;
            barra.Controls.Add(lblListado);
            Button[] acciones = { button1, button6, button5, button4, button3, button7 };
            string[] textos = { "Nuevo", "Guardar", "Eliminar", "Cancelar", "Editar", "Limpiar" };
            for (int i = 0; i < acciones.Length; i++)
            {
                acciones[i].Text = textos[i];
                acciones[i].Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                acciones[i].Size = new Size(108, 40);
                acciones[i].MinimumSize = new Size(96, 40);
                acciones[i].TextAlign = ContentAlignment.MiddleCenter;
                acciones[i].Margin = new Padding(3);
                barra.Controls.Add(acciones[i]);
            }
            txtBuscarMedicamento.Size = new Size(170, 32);
            txtBuscarMedicamento.MinimumSize = new Size(130, 28);
            txtBuscarMedicamento.Margin = new Padding(3, 7, 3, 3);
            barra.Controls.Add(txtBuscarMedicamento);
            foreach (Button accion in new[] { btnBuscar, button2 })
            {
                accion.Text = accion == btnBuscar ? "Buscar" : "Limpiar búsqueda";
                accion.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                accion.Size = new Size(112, 40);
                accion.MinimumSize = new Size(96, 40);
                accion.TextAlign = ContentAlignment.MiddleCenter;
                accion.Margin = new Padding(3);
                barra.Controls.Add(accion);
            }
            pnlListadoTitulo.Controls.Add(barra);

            var campos = new TableLayoutPanel
            {
                BackColor = pnlListado.BackColor,
                ColumnCount = 4,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = new Padding(10, 6, 10, 6),
                RowCount = 3
            };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            for (int fila = 0; fila < 3; fila++)
                campos.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333F));
            AgregarCampo(campos, label7, txtCodigoDetallePrestamo, 0, 0);
            AgregarCampo(campos, label15, txtCodigoPrestamo, 2, 0);
            AgregarCampo(campos, label8, nudCantidad, 0, 1);
            AgregarCampo(campos, label9, nudPrecio, 2, 1);
            AgregarCampo(campos, label13, nudDescuento, 0, 2);
            AgregarCampo(campos, label14, nudTotal, 2, 2);

            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.MinimumSize = new Size(0, 120);
            dataGridView1.Margin = new Padding(4, 6, 4, 6);
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            var pie = new Panel { BackColor = pnlListado.BackColor, Dock = DockStyle.Fill };
            button8.Text = "Menu Principal";
            button8.Size = new Size(180, 48);
            button8.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            button8.BackColor = Color.FromArgb(16, 179, 199);
            button8.ForeColor = Color.White;
            button8.FlatStyle = FlatStyle.Flat;
            button8.FlatAppearance.BorderSize = 0;
            button8.Cursor = Cursors.Hand;
            button8.Dock = DockStyle.Left;
            button9.Text = modoCaptura ? "Guardar préstamo" : "Cerrar";
            button9.Visible = modoCaptura;
            button9.Size = new Size(190, 48);
            button9.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            button9.BackColor = Color.FromArgb(16, 179, 199);
            button9.ForeColor = Color.White;
            button9.FlatStyle = FlatStyle.Flat;
            button9.FlatAppearance.BorderSize = 0;
            button9.Cursor = Cursors.Hand;
            button9.Dock = DockStyle.Right;
            pie.Controls.Add(button8);
            pie.Controls.Add(button9);

            var listado = new TableLayoutPanel
            {
                BackColor = pnlListado.BackColor,
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = new Padding(12, 8, 12, 8),
                RowCount = 4
            };
            listado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            listado.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            listado.RowStyles.Add(new RowStyle(SizeType.Absolute, 208F));
            listado.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            listado.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            listado.Controls.Add(pnlListadoTitulo, 0, 0);
            listado.Controls.Add(campos, 0, 1);
            listado.Controls.Add(dataGridView1, 0, 2);
            listado.Controls.Add(pie, 0, 3);

            pnlListado.Controls.Clear();
            pnlListado.Dock = DockStyle.Fill;
            pnlListado.Padding = Padding.Empty;
            pnlListado.Controls.Add(listado);
            var principal = new TableLayoutPanel
            {
                BackColor = BackColor,
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                RowCount = 2
            };
            principal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            principal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            principal.Controls.Add(pnlTitulo, 0, 0);
            principal.Controls.Add(pnlListado, 0, 1);
            pnlContenido.Controls.Clear();
            pnlContenido.Padding = Padding.Empty;
            pnlContenido.Controls.Add(principal);
        }

        private void AjustarEncabezadoResponsivo()
        {
            if (pnlTitulo == null || lblTitulo == null || lblSubtitulo == null || btnVolver == null)
                return;
            int ancho = Math.Max(260, pnlTitulo.ClientSize.Width - btnVolver.Width - 140);
            lblTitulo.SetBounds(92, 2, ancho, 48);
            lblSubtitulo.SetBounds(94, 51, ancho, 30);
            btnVolver.SetBounds(Math.Max(8, pnlTitulo.ClientSize.Width - btnVolver.Width - 12), 22,
                btnVolver.Width, btnVolver.Height);
        }

        private static void AgregarCampo(TableLayoutPanel tabla, Control etiqueta, Control campo, int columna, int fila)
        {
            Label texto = etiqueta as Label;
            if (texto != null)
            {
                texto.AutoSize = false;
                texto.AutoEllipsis = true;
                texto.TextAlign = ContentAlignment.MiddleLeft;
            }
            etiqueta.Dock = DockStyle.Fill;
            etiqueta.Margin = new Padding(4, 3, 8, 3);
            campo.Dock = DockStyle.Fill;
            campo.Margin = new Padding(4, 7, 10, 7);
            campo.MinimumSize = new Size(70, 28);
            tabla.Controls.Add(etiqueta, columna, fila);
            tabla.Controls.Add(campo, columna + 1, fila);
        }

        public IList<DetallePrestamo> DetallesIngresados { get { return detalles.AsReadOnly(); } }

        private void ConfigurarConsulta()
        {
            txtCodigoDetallePrestamo.ReadOnly = true;
            txtCodigoPrestamo.ReadOnly = true;
            ConfigurarCamposDetalle(modoCaptura);
            nudCantidad.Minimum = 0;
            nudCantidad.Maximum = int.MaxValue;
            nudPrecio.Minimum = 0;
            nudPrecio.Maximum = 999999999999999999m;
            nudDescuento.Minimum = 0;
            nudDescuento.Maximum = 999999999999999999m;
            nudTotal.Minimum = 0;
            nudTotal.Maximum = 999999999999999999m;
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.BackgroundColor = Color.FromArgb(10, 21, 37);
            dataGridView1.GridColor = Color.FromArgb(35, 70, 96);
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(16, 179, 199);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(18, 45, 75);
            dataGridView1.DefaultCellStyle.ForeColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(11, 127, 149);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(14, 29, 49);
            dataGridView1.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
        }

        private void DetallePrestamoForms_Load(object sender, EventArgs e)
        {
            CargarDetalles();
        }

        private void CargarDetalles()
        {
            if (modoCaptura)
            {
                detalles = new List<DetallePrestamo>();
                ActualizarListaVisible();
                return;
            }
            try
            {
                if (prestamoSeleccionado == null || prestamoSeleccionado.IDPrestamo <= 0)
                {
                    detalles = new List<DetallePrestamo>();
                    ActualizarListaVisible();
                    return;
                }

                detalles = detalleNegocio.ConsultarPorPrestamo(prestamoSeleccionado.IDPrestamo);
                ActualizarListaVisible();
            }
            catch (Exception ex)
            {
                detalles = new List<DetallePrestamo>();
                ActualizarListaVisible();
                MessageBox.Show(ex.Message, "Consulta de detalles", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonNuevoDetalle_Click(object sender, EventArgs e)
        {
            if (!modoCaptura && (prestamoSeleccionado == null || prestamoSeleccionado.IDPrestamo <= 0))
            {
                MessageBox.Show("Abre el detalle desde un préstamo existente.", "Nuevo detalle",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            detalleSeleccionado = null;
            modoEdicion = false;
            modoNuevo = !modoCaptura;
            dataGridView1.ClearSelection();
            LimpiarCampos();
            ConfigurarCamposDetalle(true);
            ConfigurarBotones(false);
            button6.Enabled = true;
            button6.Text = modoCaptura ? "Agregar detalle" : "Guardar";
        }

        private void buttonAgregarDetalle_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos())
                return;

            DetallePrestamo detalle = LeerDetalle(0, 0);
            if (modoEdicion && detalleSeleccionado != null)
            {
                int indice = detalles.IndexOf(detalleSeleccionado);
                if (indice >= 0)
                {
                    detalle.IDDetallePrestamo = detalleSeleccionado.IDDetallePrestamo;
                    detalles[indice] = detalle;
                }
            }
            else
            {
                detalles.Add(detalle);
            }

            modoEdicion = false;
            modoNuevo = false;
            detalleSeleccionado = null;
            ActualizarListaVisible();
            LimpiarCampos();
            ConfigurarCamposDetalle(true);
            ConfigurarBotones(false);
            button6.Enabled = true;
            button6.Text = "Agregar detalle";
        }

        private void buttonGuardarDetalle_Click(object sender, EventArgs e)
        {
            if ((!modoNuevo && !modoEdicion) || prestamoSeleccionado == null)
                return;
            if (!ValidarCampos())
                return;

            int idOriginal = modoEdicion ? detalleSeleccionado.IDDetallePrestamo : 0;
            decimal? descuento = !modoNuevo && !detalleSeleccionado.Descuento.HasValue && nudDescuento.Value == 0m
                ? (decimal?)null
                : nudDescuento.Value;
            DetallePrestamo detalle = LeerDetalle(idOriginal, prestamoSeleccionado.IDPrestamo);
            detalle.Descuento = descuento;

            try
            {
                if (modoNuevo)
                {
                    detalle.IDDetallePrestamo = detalleNegocio.Agregar(detalle);
                    modoNuevo = false;
                    CargarDetalles();
                    LimpiarCampos();
                    ConfigurarCamposDetalle(false);
                    ConfigurarBotones(false);
                    MessageBox.Show("El detalle fue agregado.", "Detalle del préstamo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    detalleNegocio.Actualizar(detalle);
                    modoEdicion = false;
                    CargarDetalles();
                    detalleSeleccionado = detalles.Find(d => d.IDDetallePrestamo == detalle.IDDetallePrestamo);
                    ConfigurarCamposDetalle(false);
                    ConfigurarBotones(detalleSeleccionado != null);
                    MessageBox.Show("El detalle fue actualizado.", "Detalle del préstamo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + (ex.InnerException == null ? string.Empty :
                    Environment.NewLine + ex.InnerException.GetBaseException().Message),
                    "Guardar detalle", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonConfirmarDetalles_Click(object sender, EventArgs e)
        {
            if (detalles.Count == 0)
            {
                MessageBox.Show("Agregue al menos un detalle.", "Detalle", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonEditarDetalle_Click(object sender, EventArgs e)
        {
            if (detalleSeleccionado == null)
            {
                MessageBox.Show("Selecciona primero una fila del detalle.", "Editar detalle",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            modoEdicion = true;
            modoNuevo = false;
            MostrarDetalleSeleccionado();
            ConfigurarCamposDetalle(true);
            ConfigurarBotones(true);
            button6.Enabled = true;
            button6.Text = modoCaptura ? "Guardar línea" : "Guardar cambios";
        }

        private void buttonEliminarDetalle_Click(object sender, EventArgs e)
        {
            if (detalleSeleccionado == null)
            {
                MessageBox.Show("Selecciona primero una fila del detalle.", "Eliminar detalle",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("¿Deseas eliminar el detalle seleccionado?", "Confirmar eliminación",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            if (modoCaptura)
            {
                detalles.Remove(detalleSeleccionado);
                detalleSeleccionado = null;
                ActualizarListaVisible();
                LimpiarCampos();
                ConfigurarBotones(false);
                button6.Enabled = true;
                return;
            }

            try
            {
                if (!detalleNegocio.Eliminar(detalleSeleccionado.IDDetallePrestamo,
                    prestamoSeleccionado.IDPrestamo))
                    throw new InvalidOperationException("No se encontró el detalle que deseas eliminar.");
                detalleSeleccionado = null;
                CargarDetalles();
                LimpiarCampos();
                ConfigurarCamposDetalle(false);
                ConfigurarBotones(false);
                MessageBox.Show("El detalle fue eliminado.", "Detalle del préstamo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + (ex.InnerException == null ? string.Empty :
                    Environment.NewLine + ex.InnerException.GetBaseException().Message),
                    "Eliminar detalle", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonCancelarDetalle_Click(object sender, EventArgs e)
        {
            if (modoCaptura && !modoEdicion)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            modoEdicion = false;
            modoNuevo = false;
            if (detalleSeleccionado != null)
                MostrarDetalleSeleccionado();
            else
                LimpiarCampos();
            ConfigurarCamposDetalle(modoCaptura);
            ConfigurarBotones(detalleSeleccionado != null);
            if (modoCaptura)
            {
                button6.Enabled = true;
                button6.Text = "Agregar detalle";
            }
        }

        private void buttonLimpiarDetalle_Click(object sender, EventArgs e)
        {
            if (modoEdicion && detalleSeleccionado != null)
            {
                modoEdicion = false;
                modoNuevo = false;
                MostrarDetalleSeleccionado();
                ConfigurarCamposDetalle(modoCaptura);
                ConfigurarBotones(true);
                if (modoCaptura)
                {
                    button6.Enabled = true;
                    button6.Text = "Agregar detalle";
                }
                return;
            }

            if (!modoCaptura && modoNuevo)
            {
                modoNuevo = false;
                ConfigurarCamposDetalle(false);
            }
            detalleSeleccionado = null;
            dataGridView1.ClearSelection();
            LimpiarCampos();
            ConfigurarBotones(false);
        }

        private void buttonBuscar_Click(object sender, EventArgs e)
        {
            ActualizarListaVisible();
        }

        private void buttonLimpiarBusqueda_Click(object sender, EventArgs e)
        {
            txtBuscarMedicamento.Clear();
            ActualizarListaVisible();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (modoEdicion || e.RowIndex < 0 || dataGridView1.Rows[e.RowIndex].DataBoundItem == null)
                return;

            detalleSeleccionado = dataGridView1.Rows[e.RowIndex].DataBoundItem as DetallePrestamo;
            if (detalleSeleccionado == null)
                return;

            MostrarDetalleSeleccionado();
            ConfigurarBotones(true);
        }

        private void MostrarDetalleSeleccionado()
        {
            txtCodigoDetallePrestamo.Text = detalleSeleccionado.IDDetallePrestamo.ToString();
            txtCodigoPrestamo.Text = detalleSeleccionado.IDPrestamo > 0
                ? detalleSeleccionado.IDPrestamo.ToString()
                : "Se genera al guardar";
            nudCantidad.Value = detalleSeleccionado.Cantidad;
            nudPrecio.Value = detalleSeleccionado.Precio;
            nudDescuento.Value = detalleSeleccionado.Descuento ?? 0m;
            nudTotal.Value = detalleSeleccionado.Total;
        }

        private DetallePrestamo LeerDetalle(int idDetalle, int idPrestamo)
        {
            return new DetallePrestamo
            {
                IDDetallePrestamo = idDetalle,
                IDPrestamo = idPrestamo,
                Cantidad = Decimal.ToInt32(nudCantidad.Value),
                Precio = nudPrecio.Value,
                Descuento = nudDescuento.Value,
                Total = nudTotal.Value
            };
        }

        private bool ValidarCampos()
        {
            if (nudCantidad.Value <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor que cero.", "Detalle",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            try
            {
                nudTotal.Value = DetallePrestamoNegocio.CalcularTotal(
                    Decimal.ToInt32(nudCantidad.Value), nudPrecio.Value, nudDescuento.Value);
                return true;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Detalle", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void RecalcularTotal(object sender, EventArgs e)
        {
            try
            {
                nudTotal.Value = DetallePrestamoNegocio.CalcularTotal(
                    Decimal.ToInt32(nudCantidad.Value), nudPrecio.Value, nudDescuento.Value);
            }
            catch (ArgumentException) { nudTotal.Value = 0; }
        }

        private void LimpiarCampos()
        {
            txtCodigoDetallePrestamo.Clear();
            txtCodigoPrestamo.Text = prestamoSeleccionado == null || modoCaptura
                ? "Se genera al guardar"
                : prestamoSeleccionado.IDPrestamo.ToString();
            nudCantidad.Value = nudCantidad.Minimum;
            nudPrecio.Value = nudPrecio.Minimum;
            nudDescuento.Value = nudDescuento.Minimum;
            nudTotal.Value = nudTotal.Minimum;
        }

        private void ConfigurarCamposDetalle(bool habilitados)
        {
            nudCantidad.ReadOnly = !habilitados;
            nudPrecio.ReadOnly = !habilitados;
            nudDescuento.ReadOnly = !habilitados;
            nudTotal.ReadOnly = true;
        }

        private void ConfigurarBotones(bool haySeleccion)
        {
            button3.Enabled = haySeleccion && !modoEdicion;
            button5.Enabled = haySeleccion && !modoEdicion;
            if (!modoCaptura)
                button6.Enabled = modoNuevo || modoEdicion;
        }

        private void ActualizarListaVisible()
        {
            string filtro = txtBuscarMedicamento.Text.Trim();
            detallesVisibles = string.IsNullOrEmpty(filtro)
                ? detalles.ToList()
                : detalles.Where(d => Coincide(d.IDDetallePrestamo, filtro) ||
                    Coincide(d.IDPrestamo, filtro) || Coincide(d.Cantidad, filtro) ||
                    Coincide(d.Precio, filtro) || Coincide(d.Descuento, filtro) ||
                    Coincide(d.Total, filtro)).ToList();
            MostrarResultados();
        }

        private static bool Coincide(object valor, string filtro)
        {
            return valor != null && Convert.ToString(valor).IndexOf(filtro,
                StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void MostrarResultados()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = detallesVisibles;
            CambiarEncabezado("IDDetallePrestamo", "Código Detalle");
            CambiarEncabezado("IDPrestamo", "Código Préstamo");
            CambiarEncabezado("Cantidad", "Cantidad");
            CambiarEncabezado("Precio", "Precio");
            CambiarEncabezado("Descuento", "Descuento");
            CambiarEncabezado("Total", "Total");
            if (prestamoSeleccionado != null)
                lblListado.Text = "Detalles del préstamo " + prestamoSeleccionado.IDPrestamo;
        }

        private void CambiarEncabezado(string columna, string texto)
        {
            if (dataGridView1.Columns.Contains(columna))
                dataGridView1.Columns[columna].HeaderText = texto;
        }
    }
}
