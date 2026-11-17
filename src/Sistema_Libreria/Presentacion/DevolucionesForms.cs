using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    /// <summary>Consulta encabezados y coordina la vista del detalle seleccionado.</summary>
    public partial class DevolucionesForms : Form
    {
        private const int FilasPorPagina = 10;
        private readonly DevolucionNegocio negocio = new DevolucionNegocio();
        private List<DetalleDevolucionEntidad> detallesPendientes = new List<DetalleDevolucionEntidad>();
        private readonly Button btnAnterior = new Button();
        private readonly Button btnSiguiente = new Button();
        private readonly Label lblPagina = new Label();
        private TableLayoutPanel tlpListadoLayout;
        private TableLayoutPanel tlpCampos;
        private TableLayoutPanel tlpPieListado;
        private FlowLayoutPanel flpPaginacion;
        private List<DevolucionEntidad> resultados = new List<DevolucionEntidad>();
        private int paginaActual;
        private DetalleDevolucionForms ventanaDetalle;
        private bool altaIniciada;
        private bool edicionIniciada;

        public DevolucionesForms()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            button2.Visible = false;
            button10.Visible = false;
            ConfigurarLayoutResponsivo();
            Resize += (s, e) => AjustarEncabezadoResponsivo();
            AjustarEncabezadoResponsivo();
            Shown += (s, e) => WindowState = FormWindowState.Maximized;
            ConfigurarLayoutResponsivo();
            AplicarEstiloTabla();
            ConfigurarConsulta();
            ConfigurarPaginacion();
            Load += (s, e) => CargarEncabezados(string.Empty);
        }

        /// <summary>Ordena los controles existentes en regiones que se ajustan al tamaño de la ventana.</summary>
        private void ConfigurarLayoutResponsivo()
        {
            MinimumSize = new Size(900, 650);
            AutoScroll = true;
            pnlTop.Visible = false;
            pnlContenido.Padding = new Padding(18, 10, 18, 14);
            tlpStats.Dock = DockStyle.Fill;
            tlpStats.Height = 164;
            tlpStats.Padding = Padding.Empty;
            tlpStats.Margin = new Padding(0, 4, 0, 8);
            tlpStats.ColumnStyles.Clear();
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
            PrepararTarjeta(panel4, label10, label11, label12);
            PrepararTarjeta(panel2, label4, label5, label6);
            PrepararTarjeta(panel1, label1, label2, label3);

            pnlTitulo.Dock = DockStyle.Fill;
            lblTitulo.Text = "Devoluciones";
            lblTitulo.AutoSize = false;
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            lblTitulo.Font = new Font("Segoe UI", 23F, FontStyle.Bold);
            lblSubtitulo.Text = "Gestión de devoluciones de préstamos de todas las sucursales.";
            lblSubtitulo.AutoSize = false;
            lblSubtitulo.TextAlign = ContentAlignment.MiddleLeft;
            lblSubtitulo.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            pnlListadoTitulo.Controls.Clear();
            pnlListadoTitulo.Dock = DockStyle.Top;
            pnlListadoTitulo.AutoSize = true;
            pnlListadoTitulo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            var barra = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = pnlListadoTitulo.BackColor,
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = Padding.Empty,
                Padding = new Padding(8, 5, 8, 7),
                WrapContents = true
            };
            lblListado.Text = "Listado de Devoluciones";
            lblListado.Dock = DockStyle.None;
            lblListado.AutoSize = true;
            lblListado.AutoEllipsis = false;
            lblListado.MinimumSize = new Size(220, 40);
            lblListado.Margin = new Padding(3);
            lblListado.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblListado.TextAlign = ContentAlignment.MiddleLeft;
            barra.Controls.Add(lblListado);
            Button[] botones = { button1, button6, button5, button4, button3, button7, button2 };
            string[] textos = { "Nuevo", "Guardar", "Eliminar", "Cancelar", "Editar", "Limpiar", "Limpiar" };
            for (int i = 0; i < botones.Length; i++)
            {
                botones[i].Text = textos[i];
                botones[i].Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                botones[i].Size = new Size(104, 40);
                botones[i].MinimumSize = new Size(96, 40);
                botones[i].Margin = new Padding(3);
                barra.Controls.Add(botones[i]);
            }
            txtBuscarDevolucion.Size = new Size(180, 32);
            txtBuscarDevolucion.MinimumSize = new Size(145, 28);
            txtBuscarDevolucion.Margin = new Padding(3, 7, 3, 3);
            barra.Controls.Add(txtBuscarDevolucion);
            foreach (Button accion in new[] { btnBuscar, button10 })
            {
                accion.Text = accion == btnBuscar ? "Buscar" : "Limpiar búsqueda";
                accion.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                accion.Size = new Size(112, 40);
                accion.MinimumSize = new Size(96, 40);
                accion.Margin = new Padding(3);
                barra.Controls.Add(accion);
            }
            pnlListadoTitulo.Controls.Add(barra);

            tlpCampos = new TableLayoutPanel
            {
                BackColor = pnlListado.BackColor,
                ColumnCount = 4,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = new Padding(4),
                RowCount = 3
            };
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            for (int fila = 0; fila < 3; fila++)
                tlpCampos.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333F));

            AgregarCampo(tlpCampos, label7, txtIdDevolucion, 0);
            AgregarCampo(tlpCampos, label15, txtIdPrestamo, 1);
            AgregarCampo(tlpCampos, label9, dtpFechaDevolucion, 2);
            AgregarCampo(tlpCampos, label14, txtDiasRetraso, 3);
            AgregarCampo(tlpCampos, label8, txtObservaciones, 4);
            AgregarCampo(tlpCampos, label13, txtRecibidoPor, 5);

            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.MinimumSize = new Size(0, 120);
            dataGridView1.Margin = new Padding(2, 6, 2, 6);
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            tlpPieListado = new TableLayoutPanel
            {
                BackColor = pnlListado.BackColor,
                ColumnCount = 3,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                RowCount = 1
            };
            tlpPieListado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlpPieListado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlpPieListado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlpPieListado.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Panel panelMenu = new Panel { BackColor = pnlListado.BackColor, Dock = DockStyle.Fill };
            Panel panelCerrar = new Panel { BackColor = pnlListado.BackColor, Dock = DockStyle.Fill };
            flpPaginacion = new FlowLayoutPanel
            {
                Anchor = AnchorStyles.None,
                AutoSize = true,
                BackColor = pnlListado.BackColor,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            button8.Location = Point.Empty;
            button8.Dock = DockStyle.Left;
            button9.Location = Point.Empty;
            button9.Dock = DockStyle.Right;
            btnAnterior.Anchor = lblPagina.Anchor = btnSiguiente.Anchor = AnchorStyles.None;
            panelMenu.Controls.Add(button8);
            panelCerrar.Controls.Add(button9);
            flpPaginacion.Controls.Add(btnAnterior);
            flpPaginacion.Controls.Add(lblPagina);
            flpPaginacion.Controls.Add(btnSiguiente);
            tlpPieListado.Controls.Add(panelMenu, 0, 0);
            tlpPieListado.Controls.Add(flpPaginacion, 1, 0);
            tlpPieListado.Controls.Add(panelCerrar, 2, 0);

            tlpListadoLayout = new TableLayoutPanel
            {
                BackColor = pnlListado.BackColor,
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                RowCount = 4
            };
            tlpListadoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpListadoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpListadoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 210F));
            tlpListadoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpListadoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
            tlpListadoLayout.Controls.Add(pnlListadoTitulo, 0, 0);
            tlpListadoLayout.Controls.Add(tlpCampos, 0, 1);
            tlpListadoLayout.Controls.Add(dataGridView1, 0, 2);
            tlpListadoLayout.Controls.Add(tlpPieListado, 0, 3);

            button8.Text = "Menu Principal";
            button8.Size = new Size(190, 50);
            button9.Size = new Size(150, 50);
            pnlListado.Controls.Clear();
            pnlListado.Dock = DockStyle.Fill;
            pnlListado.Padding = new Padding(12, 8, 12, 8);
            pnlListado.Controls.Add(tlpListadoLayout);

            var principal = new TableLayoutPanel
            {
                BackColor = pnlContenido.BackColor,
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                RowCount = 3
            };
            principal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 164F));
            principal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            principal.Controls.Add(pnlTitulo, 0, 0);
            principal.Controls.Add(tlpStats, 0, 1);
            principal.Controls.Add(pnlListado, 0, 2);
            pnlContenido.Controls.Clear();
            pnlContenido.Controls.Add(principal);
        }

        private void AjustarEncabezadoResponsivo()
        {
            if (pnlTitulo == null || lblTitulo == null || lblSubtitulo == null)
                return;
            int anchoDisponible = Math.Max(260, pnlTitulo.ClientSize.Width - 390);
            lblTitulo.SetBounds(100, 0, anchoDisponible, 56);
            lblSubtitulo.SetBounds(102, 55, anchoDisponible, 32);
            btnCerrarSesion.SetBounds(Math.Max(0, pnlTitulo.ClientSize.Width - 275), 18, 255, 52);
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
            tarjeta.SizeChanged += (s, e) => ajustar();
            ajustar();
        }

        private static void AgregarCampo(TableLayoutPanel tabla, Control etiqueta, Control campo, int indice)
        {
            int fila = indice / 2;
            int columna = (indice % 2) * 2;
            etiqueta.Dock = DockStyle.Fill;
            etiqueta.Margin = new Padding(4);
            if (etiqueta is Label label)
            {
                label.AutoSize = false;
                label.TextAlign = ContentAlignment.MiddleLeft;
            }
            campo.Dock = DockStyle.Fill;
            campo.Margin = new Padding(4, 8, 8, 8);
            tabla.Controls.Add(etiqueta, columna, fila);
            tabla.Controls.Add(campo, columna + 1, fila);
        }

        private void AplicarEstiloTabla()
        {
            Color fondoGeneral = ColorTranslator.FromHtml("#0A192D");
            Color fondoEncabezado = ColorTranslator.FromHtml("#144673");
            Color filaImpar = ColorTranslator.FromHtml("#102A44");
            Color filaPar = ColorTranslator.FromHtml("#183655");
            Color colorSeleccion = ColorTranslator.FromHtml("#0078D7");

            dataGridView1.BackgroundColor = fondoGeneral;
            dataGridView1.ForeColor = Color.White;
            dataGridView1.GridColor = ColorTranslator.FromHtml("#24435F");
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = fondoEncabezado;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = fondoEncabezado;
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.DefaultCellStyle.BackColor = filaImpar;
            dataGridView1.DefaultCellStyle.ForeColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionBackColor = colorSeleccion;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = filaPar;
            dataGridView1.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.AlternatingRowsDefaultCellStyle.SelectionBackColor = colorSeleccion;
            dataGridView1.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.RowHeadersDefaultCellStyle.BackColor = fondoEncabezado;
            dataGridView1.RowHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.RowHeadersDefaultCellStyle.SelectionBackColor = colorSeleccion;
            dataGridView1.RowHeadersDefaultCellStyle.SelectionForeColor = Color.White;
        }

        private void ConfigurarConsulta()
        {
            EstablecerCamposEncabezadoEditables(false);
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            button1.Click += IniciarAlta;
            button3.Click += IniciarEdicion;
            button4.Click += CancelarOperacion;
            button5.Click += EliminarSeleccionada;
            button6.Click += GuardarDevolucion;
            button2.Click += ConfirmarLimpiar;
            button7.Click += ConfirmarLimpiar;
            btnBuscar.Click += (s, e) => BuscarEncabezados(txtBuscarDevolucion.Text.Trim());
            button10.Click += (s, e) =>
            {
                txtBuscarDevolucion.Clear();
                CargarEncabezados(string.Empty);
            };
            dataGridView1.CellClick += SeleccionarEncabezado;
            dataGridView1.CellDoubleClick += AbrirDetalleEncabezado;
        }

        private void ConfigurarPaginacion()
        {
            btnAnterior.Text = "◀ Anterior";
            btnSiguiente.Text = "Siguiente ▶";
            btnAnterior.Size = btnSiguiente.Size = new Size(130, 34);
            lblPagina.Size = new Size(160, 34);
            lblPagina.TextAlign = ContentAlignment.MiddleCenter;
            lblPagina.ForeColor = Color.White;
            lblPagina.BackColor = Color.Transparent;
            EstilizarBotonPagina(btnAnterior);
            EstilizarBotonPagina(btnSiguiente);

            int x = Math.Max(10, (dataGridView1.Width - 420) / 2);
            int y = dataGridView1.Bottom + 8;
            btnAnterior.Location = new Point(x, y);
            lblPagina.Location = new Point(x + 135, y);
            btnSiguiente.Location = new Point(x + 290, y);
            btnAnterior.Anchor = btnSiguiente.Anchor = lblPagina.Anchor = AnchorStyles.Bottom;
            btnAnterior.Click += (s, e) => CambiarPagina(-1);
            btnSiguiente.Click += (s, e) => CambiarPagina(1);
        }

        private static void EstilizarBotonPagina(Button boton)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.BackColor = Color.FromArgb(42, 100, 145);
            boton.ForeColor = Color.White;
            boton.Cursor = Cursors.Hand;
        }

        private void BuscarEncabezados(string filtro)
        {
            if (!CargarEncabezados(filtro)) return;
            int cantidad = resultados.Count;
            MostrarMensaje("Búsqueda de devoluciones",
                cantidad == 0 ? "No se encontraron devoluciones con ese criterio." :
                "Se encontraron " + cantidad + " devolución(es).",
                cantidad == 0 ? EstiloDialogo.Normal : EstiloDialogo.Exito);
        }

        private bool CargarEncabezados(string filtro)
        {
            try
            {
                resultados = negocio.Consultar(filtro == "🔍" ? string.Empty : filtro);
                paginaActual = 0;
                ActualizarTotales();
                MostrarPagina();
                AcomodarColumnas();
                return true;
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error de consulta", ex.Message, EstiloDialogo.Error);
                return false;
            }
        }

        private void ActualizarTotales()
        {
            try
            {
                // Los totales vienen de toda la tabla, separados del filtro y la paginación.
                int[] totales = negocio.ConsultarTotales();
                label12.Text = totales[0].ToString();
                label3.Text = totales[1].ToString();
                label6.Text = totales[2].ToString();
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al consultar totales", ex.Message, EstiloDialogo.Error);
            }
        }

        private void MostrarPagina()
        {
            int inicio = paginaActual * FilasPorPagina;
            int cantidad = Math.Max(0, Math.Min(FilasPorPagina, resultados.Count - inicio));
            dataGridView1.DataSource = resultados.GetRange(inicio, cantidad);
            AcomodarColumnas();

            int paginas = Math.Max(1, (int)Math.Ceiling(resultados.Count / (double)FilasPorPagina));
            bool mostrarNavegacion = paginas > 1;
            btnAnterior.Visible = btnSiguiente.Visible = lblPagina.Visible = mostrarNavegacion;
            btnAnterior.Enabled = paginaActual > 0;
            btnSiguiente.Enabled = paginaActual + 1 < paginas;
            lblPagina.Text = "Página " + (paginaActual + 1) + " de " + paginas;
        }

        private void CambiarPagina(int desplazamiento)
        {
            int paginas = Math.Max(1, (int)Math.Ceiling(resultados.Count / (double)FilasPorPagina));
            int destino = paginaActual + desplazamiento;
            if (destino < 0 || destino >= paginas) return;
            paginaActual = destino;
            MostrarPagina();
        }

        private void SeleccionarEncabezado(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (altaIniciada || edicionIniciada)
            {
                MostrarMensaje("Operación en curso", "Guarda o cancela los cambios antes de seleccionar otro encabezado.", EstiloDialogo.Normal);
                return;
            }
            var encabezado = dataGridView1.Rows[e.RowIndex].DataBoundItem as DevolucionEntidad;
            if (encabezado == null) return;

            // Muestra el encabezado en los controles existentes.
            txtIdDevolucion.Text = encabezado.IDDevoluciones.ToString();
            txtIdPrestamo.Text = encabezado.IDPrestamo.ToString();
            dtpFechaDevolucion.Value = encabezado.FechaDevolucion;
            txtDiasRetraso.Text = encabezado.DiasRetraso.HasValue ? encabezado.DiasRetraso.Value.ToString() : string.Empty;
            txtObservaciones.Text = encabezado.Observaciones ?? string.Empty;
            txtRecibidoPor.Text = encabezado.RecibidoPor ?? string.Empty;

        }

        private void AbrirDetalleEncabezado(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || altaIniciada || edicionIniciada) return;
            var encabezado = dataGridView1.Rows[e.RowIndex].DataBoundItem as DevolucionEntidad;
            if (encabezado == null) return;

            try
            {
                if (ventanaDetalle == null || ventanaDetalle.IsDisposed)
                    ventanaDetalle = new DetalleDevolucionForms();
                ventanaDetalle.CargarPorDevolucion(encabezado.IDDevoluciones);
                if (!ventanaDetalle.Visible) ventanaDetalle.Show(this);
                else ventanaDetalle.Activate();
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar el detalle", ex.Message, EstiloDialogo.Error);
            }
        }

        private void IniciarAlta(object sender, EventArgs e)
        {
            if (altaIniciada || edicionIniciada)
            {
                MostrarMensaje("Operación en curso", "Termina o cancela la operación actual antes de iniciar otra.", EstiloDialogo.Normal);
                return;
            }
            altaIniciada = true;
            edicionIniciada = false;
            detallesPendientes.Clear();
            txtIdDevolucion.Clear();
            txtIdPrestamo.Clear();
            txtIdPrestamo.ReadOnly = false;
            dtpFechaDevolucion.Value = DateTime.Today;
            txtDiasRetraso.Clear();
            txtObservaciones.Clear();
            txtRecibidoPor.Clear();
            EstablecerCamposEncabezadoEditables(true);
            txtIdPrestamo.Focus();
        }

        private void IniciarEdicion(object sender, EventArgs e)
        {
            if (altaIniciada || edicionIniciada)
            {
                MostrarMensaje("Operación en curso", "Termina o cancela la operación actual antes de editar.", EstiloDialogo.Normal);
                return;
            }
            int id;
            if (!int.TryParse(txtIdDevolucion.Text, out id) || id <= 0)
            {
                MostrarMensaje("Editar devolución", "Selecciona una devolución de la lista.", EstiloDialogo.Normal);
                return;
            }
            edicionIniciada = true;
            EstablecerCamposEncabezadoEditables(true);
            txtIdPrestamo.Focus();
        }

        private void GuardarDevolucion(object sender, EventArgs e)
        {
            try
            {
                if (!altaIniciada && !edicionIniciada)
                    throw new InvalidOperationException("Pulsa Nuevo o Editar antes de guardar.");

                int idPrestamo;
                if (!int.TryParse(txtIdPrestamo.Text.Trim(), out idPrestamo) || idPrestamo <= 0)
                    throw new ArgumentException("Escribe un código de préstamo válido.");

                int dias;
                if (!string.IsNullOrWhiteSpace(txtDiasRetraso.Text) &&
                    (!int.TryParse(txtDiasRetraso.Text.Trim(), out dias) || dias < 0))
                    throw new ArgumentException("Escribe días de retraso como número entero igual o mayor que cero.");

                var encabezado = new DevolucionEntidad
                {
                    IDDevoluciones = ObtenerIdEncabezadoActual(),
                    IDPrestamo = idPrestamo,
                    FechaDevolucion = dtpFechaDevolucion.Value,
                    DiasRetraso = string.IsNullOrWhiteSpace(txtDiasRetraso.Text) ? (int?)null : int.Parse(txtDiasRetraso.Text.Trim()),
                    Observaciones = string.IsNullOrWhiteSpace(txtObservaciones.Text) ? null : txtObservaciones.Text.Trim(),
                    RecibidoPor = string.IsNullOrWhiteSpace(txtRecibidoPor.Text) ? null : txtRecibidoPor.Text.Trim()
                };

                if (edicionIniciada)
                {
                    if (DialogoSistema.Confirmar(this, "Confirmar edición",
                        "¿Guardar los cambios de la devolución " + encabezado.IDDevoluciones + "?",
                        "Sí, guardar", "Cancelar", EstiloDialogo.Normal) != DialogResult.Yes) return;
                    negocio.Editar(encabezado);
                    edicionIniciada = false;
                    EstablecerCamposEncabezadoEditables(false);
                    MostrarMensaje("Devolución actualizada", "Los cambios se guardaron; los detalles se conservaron.", EstiloDialogo.Exito);
                    LimpiarCamposEncabezado();
                    CargarEncabezados(string.Empty);
                    ActualizarTotales();
                    return;
                }

                using (var formularioDetalle = new DetalleDevolucionForms(encabezado, detallesPendientes))
                {
                    if (formularioDetalle.ShowDialog(this) != DialogResult.OK)
                    {
                        RestablecerAlta();
                        LimpiarCamposEncabezado();
                        return;
                    }
                    detallesPendientes = new List<DetalleDevolucionEntidad>(formularioDetalle.DetallesPendientes);
                }

                // La capa Negocio delega una única transacción que inserta encabezado y detalles.
                int idGenerado = negocio.AgregarConDetalles(encabezado, detallesPendientes);
                MostrarMensaje("Devolución agregada", "Se guardó el encabezado " + idGenerado +
                    " con " + detallesPendientes.Count + " fila(s) de detalle.", EstiloDialogo.Exito);
                RestablecerAlta();
                CargarEncabezados(string.Empty);
                ActualizarTotales();
            }
            catch (Exception ex)
            {
                MostrarMensaje("No se pudo guardar la devolución", ex.Message, EstiloDialogo.Error);
            }
        }

        private void EliminarSeleccionada(object sender, EventArgs e)
        {
            if (altaIniciada || edicionIniciada)
            {
                MostrarMensaje("Operación en curso", "Guarda o cancela los cambios antes de eliminar.", EstiloDialogo.Normal);
                return;
            }
            int id;
            if (!int.TryParse(txtIdDevolucion.Text, out id) || id <= 0)
            {
                MostrarMensaje("Eliminar devolución", "Selecciona una devolución de la lista.", EstiloDialogo.Normal);
                return;
            }
            if (DialogoSistema.Confirmar(this, "Confirmar eliminación",
                "¿Eliminar la devolución " + id + " y todos sus detalles? Si tiene pagos relacionados, SQL Server rechazará la operación.",
                "Sí, eliminar", "No", EstiloDialogo.Peligro) != DialogResult.Yes) return;

            try
            {
                negocio.Eliminar(id);
                if (ventanaDetalle != null && !ventanaDetalle.IsDisposed)
                {
                    ventanaDetalle.Close();
                    ventanaDetalle = null;
                }
                LimpiarCamposEncabezado();
                CargarEncabezados(string.Empty);
                ActualizarTotales();
                MostrarMensaje("Devolución eliminada", "Se eliminaron la devolución y sus detalles.", EstiloDialogo.Exito);
            }
            catch (Exception ex)
            {
                MostrarMensaje("No se pudo eliminar", ex.Message, EstiloDialogo.Error);
            }
        }

        private void ConfirmarLimpiar(object sender, EventArgs e)
        {
            if (DialogoSistema.Confirmar(this, "Limpiar campos",
                "¿Borrar los datos escritos en el formulario?", "Sí, limpiar", "Cancelar", EstiloDialogo.Normal)
                != DialogResult.Yes) return;
            RestablecerAlta();
            edicionIniciada = false;
            LimpiarCamposEncabezado();
            MostrarMensaje("Campos limpiados", "Se limpiaron los campos del formulario.", EstiloDialogo.Exito);
        }

        private void CancelarOperacion(object sender, EventArgs e)
        {
            if ((altaIniciada || edicionIniciada) &&
                DialogoSistema.Confirmar(this, "Cancelar operación",
                    "¿Descartar los cambios de la devolución?", "Sí, cancelar", "Seguir editando", EstiloDialogo.Normal)
                    != DialogResult.Yes) return;
            RestablecerAlta();
            edicionIniciada = false;
            LimpiarCamposEncabezado();
            MostrarMensaje("Operación cancelada", "No se guardaron cambios en la devolución.", EstiloDialogo.Normal);
        }

        private void RestablecerAlta()
        {
            altaIniciada = false;
            detallesPendientes.Clear();
            EstablecerCamposEncabezadoEditables(false);
        }

        private void EstablecerCamposEncabezadoEditables(bool editables)
        {
            // El código lo genera la base; los demás campos solo se editan en Nuevo o Editar.
            txtIdDevolucion.ReadOnly = true;
            txtIdDevolucion.BackColor = ColorTranslator.FromHtml("#C0C4C8");
            txtIdDevolucion.ForeColor = ColorTranslator.FromHtml("#454B52");
            txtIdPrestamo.ReadOnly = !editables;
            txtDiasRetraso.ReadOnly = !editables;
            txtObservaciones.ReadOnly = !editables;
            txtRecibidoPor.ReadOnly = !editables;
            dtpFechaDevolucion.Enabled = editables;
        }

        private void LimpiarCamposEncabezado()
        {
            txtIdDevolucion.Clear();
            txtIdPrestamo.Clear();
            dtpFechaDevolucion.Value = DateTime.Today;
            txtDiasRetraso.Clear();
            txtObservaciones.Clear();
            txtRecibidoPor.Clear();
        }

        private int ObtenerIdEncabezadoActual()
        {
            int id;
            return int.TryParse(txtIdDevolucion.Text, out id) ? id : 0;
        }

        private void AcomodarColumnas()
        {
            ConfigurarColumna("IDDevoluciones", "Código devolución", 14,
                DataGridViewContentAlignment.MiddleCenter);
            ConfigurarColumna("IDPrestamo", "Código préstamo", 14,
                DataGridViewContentAlignment.MiddleCenter);
            ConfigurarColumna("FechaDevolucion", "Fecha de devolución", 20,
                DataGridViewContentAlignment.MiddleCenter);
            ConfigurarColumna("DiasRetraso", "Días de retraso", 13,
                DataGridViewContentAlignment.MiddleCenter);
            ConfigurarColumna("Observaciones", "Observaciones", 22,
                DataGridViewContentAlignment.MiddleLeft);
            ConfigurarColumna("RecibidoPor", "Recibido por", 17,
                DataGridViewContentAlignment.MiddleLeft);

            if (dataGridView1.Columns.Contains("FechaDevolucion"))
                dataGridView1.Columns["FechaDevolucion"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            if (dataGridView1.Columns.Contains("Observaciones"))
                dataGridView1.Columns["Observaciones"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            if (dataGridView1.Columns.Contains("DiasRetraso"))
                dataGridView1.Columns["DiasRetraso"].DefaultCellStyle.Format = "N0";
        }

        private void ConfigurarColumna(string nombre, string encabezado, float peso,
            DataGridViewContentAlignment alineacion)
        {
            if (!dataGridView1.Columns.Contains(nombre)) return;
            DataGridViewColumn columna = dataGridView1.Columns[nombre];
            columna.HeaderText = encabezado;
            columna.FillWeight = peso;
            columna.MinimumWidth = 90;
            columna.DefaultCellStyle.Alignment = alineacion;
        }

        private void MostrarMensaje(string titulo, string mensaje, EstiloDialogo estilo)
        {
            DialogoSistema.Mostrar(this, titulo, mensaje, estilo);
        }

    }
}
