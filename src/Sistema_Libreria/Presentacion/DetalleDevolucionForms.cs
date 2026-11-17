using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    /// <summary>Presenta únicamente las filas de detalle del encabezado seleccionado.</summary>
    public partial class DetalleDevolucionForms : Form
    {
        private readonly DetalleDevolucionNegocio negocio = new DetalleDevolucionNegocio();
        private List<DetalleDevolucionEntidad> resultados = new List<DetalleDevolucionEntidad>();
        private readonly DevolucionEntidad encabezadoPendiente;
        private int idDevolucionSeleccionada;
        private bool edicionIniciada;

        public List<DetalleDevolucionEntidad> DetallesPendientes { get; private set; }

        public DetalleDevolucionForms() : this(null, null)
        {
        }

        public DetalleDevolucionForms(DevolucionEntidad encabezado,
            IList<DetalleDevolucionEntidad> detallesIniciales)
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            button7.Visible = false;
            button10.Visible = false;
            AplicarEstiloTabla();
            encabezadoPendiente = encabezado;
            DetallesPendientes = detallesIniciales == null
                ? new List<DetalleDevolucionEntidad>()
                : new List<DetalleDevolucionEntidad>(detallesIniciales);
            EstablecerCamposDetalleEditables(encabezadoPendiente != null);
            ConfigurarConsulta();
            btnCerrarSesion.Click += VolverADevoluciones;
            btnCerrarSesion.Text = "↩   Volver a Devoluciones";
            if (encabezadoPendiente != null)
            {
                txtIdDevolucion.Text = "Pendiente";
                lblTitulo.Text = "Detalle Devoluciones";
                lblSubtitulo.Text = "Agrega uno o más detalles y guarda la operación completa.";
                button9.Text = "Guardar todo";
                button9.Visible = true;
                button3.Enabled = false;
                button5.Enabled = false;
                button8.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                button9.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                PosicionarBotonGuardarTodo();
                pnlListado.Resize += (s, e) => PosicionarBotonGuardarTodo();
                Shown += (s, e) => PosicionarBotonGuardarTodo();
                resultados = new List<DetalleDevolucionEntidad>(DetallesPendientes);
                MostrarResultados();
            }
            ConfigurarDisenoDetalle();
            pnlListado.Resize += (s, e) => AjustarDisenoDetalle();
            Load += (s, e) =>
            {
                AjustarTamanioSobrePropietario();
                AjustarDisenoDetalle();
                if (encabezadoPendiente != null) PosicionarBotonGuardarTodo();
            };
            NavegacionPantallas.Configurar(this);
        }

        /// <summary>Abre el detalle como ventana centrada y proporcional sobre la tabla madre.</summary>
        private void AjustarTamanioSobrePropietario()
        {
            if (Owner == null) return;

            int ancho = Math.Max(MinimumSize.Width,
                Math.Min(1400, (int)(Owner.ClientSize.Width * 0.70)));
            int alto = Math.Max(MinimumSize.Height,
                Math.Min(850, (int)(Owner.ClientSize.Height * 0.70)));

            Size = new Size(ancho, alto);
            CenterToParent();
        }

        /// <summary>Ordena los controles existentes para presentar el detalle como formulario y tabla.</summary>
        private void ConfigurarDisenoDetalle()
        {
            // Se conserva el mismo formulario y los mismos controles; solo cambia su distribución visual.
            pnlContenido.Controls.Remove(tlpStats);
            tlpStats.Visible = false;
            pnlMenu.Visible = false;
            pnlContenido.Padding = new Padding(20, 12, 20, 14);
            pnlTitulo.Height = 96;
            pnlTitulo.BackColor = ColorTranslator.FromHtml("#0F1E3A");
            pnlListado.Padding = Padding.Empty;
            pnlListado.BackColor = ColorTranslator.FromHtml("#0F1E3A");
            pnlListadoTitulo.Visible = false;
            pnlListadoTitulo.Dock = DockStyle.None;
            pnlListadoTitulo.Height = 0;
            lblListado.Dock = DockStyle.None;

            // Coloca la barra de acciones y la búsqueda dentro del panel principal.
            pnlListado.Controls.Add(button1);
            pnlListado.Controls.Add(button2);
            pnlListado.Controls.Add(button3);
            pnlListado.Controls.Add(button4);
            pnlListado.Controls.Add(button5);
            pnlListado.Controls.Add(button6);
            pnlListado.Controls.Add(button7);
            pnlListado.Controls.Add(button10);
            pnlListado.Controls.Add(btnBuscar);
            pnlListado.Controls.Add(txtBuscarDetalleDevolucion);
            pnlListado.Controls.Add(lblListado);

            lblTitulo.Text = "Detalle devoluciones";
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTituloIcono.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTituloIcono.BorderStyle = BorderStyle.FixedSingle;
            lblSubtitulo.Text = encabezadoPendiente == null
                ? "Selecciona una devolución para consultar sus detalles."
                : "Agrega detalles y guarda la devolución completa.";

            label7.Text = "Código detalle devolución";
            label15.Text = "Código devolución";
            label9.Text = "Cantidad";
            label14.Text = "Multa";
            lblListado.Text = "Lista de detalles";
            Label[] etiquetasDetalle = { label7, label15, label9, label14 };
            foreach (Label etiqueta in etiquetasDetalle)
                etiqueta.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            button1.Text = "Nuevo";
            button6.Text = "Guardar";
            button3.Text = "Editar";
            button5.Text = "Eliminar";
            button2.Text = "Limpiar";
            button4.Text = "Cancelar";
            button7.Visible = false; // Es el segundo botón que ejecuta la misma acción de Limpiar.
            button9.Visible = encabezadoPendiente != null
                || System.ComponentModel.LicenseManager.UsageMode
                    == System.ComponentModel.LicenseUsageMode.Designtime;
            button10.Text = "×";
            button10.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnBuscar.Text = "🔍  Buscar";
            btnBuscar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnBuscar.TextAlign = ContentAlignment.MiddleCenter;
            txtBuscarDetalleDevolucion.Text = string.Empty;
            txtBuscarDetalleDevolucion.Multiline = false;
            txtBuscarDetalleDevolucion.TextAlign = HorizontalAlignment.Left;
            txtBuscarDetalleDevolucion.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            txtBuscarDetalleDevolucion.BorderStyle = BorderStyle.FixedSingle;
            TextBox[] camposDetalle =
            {
                txtIdDetalleDevolucion, txtIdDevolucion, txtMulta, txtCantidad
            };
            foreach (TextBox campo in camposDetalle)
            {
                campo.AutoSize = false;
                campo.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
                campo.BorderStyle = BorderStyle.FixedSingle;
            }
            Color fondoCampoBloqueado = ColorTranslator.FromHtml("#C0C4C8");
            Color textoCampoBloqueado = ColorTranslator.FromHtml("#454B52");
            txtIdDetalleDevolucion.ReadOnly = true;
            txtIdDetalleDevolucion.BackColor = fondoCampoBloqueado;
            txtIdDetalleDevolucion.ForeColor = textoCampoBloqueado;
            txtIdDevolucion.ReadOnly = true;
            txtIdDevolucion.BackColor = fondoCampoBloqueado;
            txtIdDevolucion.ForeColor = textoCampoBloqueado;

            button8.Text = "Menú principal";
            button8.BackColor = ColorTranslator.FromHtml("#35D4C8");
            button8.ForeColor = ColorTranslator.FromHtml("#08243A");
            button8.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button8.Size = new Size(160, 38);
            button8.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button9.Size = button8.Size;
            button9.Font = button8.Font;
            button9.BackColor = button8.BackColor;
            button9.ForeColor = button8.ForeColor;
            button9.Cursor = button8.Cursor;
            button9.FlatStyle = button8.FlatStyle;
            button9.FlatAppearance.BorderSize = button8.FlatAppearance.BorderSize;
            button9.FlatAppearance.MouseOverBackColor = button8.FlatAppearance.MouseOverBackColor;
            button9.TextAlign = ContentAlignment.MiddleCenter;

            btnCerrarSesion.Text = "Volver a Devoluciones";
            btnCerrarSesion.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnCerrarSesion.Size = new Size(280, 44);
            btnCerrarSesion.TextAlign = ContentAlignment.MiddleCenter;
            btnCerrarSesion.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersHeight = 38;
            dataGridView1.RowTemplate.Height = 34;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#19385D");
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#35D4C8");
            dataGridView1.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#10203E");
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#142747");
            dataGridView1.GridColor = ColorTranslator.FromHtml("#243B5D");
            dataGridView1.BorderStyle = BorderStyle.None;

            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(860, 580);
            AutoScroll = true;
            Size = new Size(900, 600);
            WindowState = FormWindowState.Normal;
            AjustarDisenoDetalle();
        }

        /// <summary>Ajusta los campos, acciones y tabla al tamaño actual del formulario.</summary>
        private void AjustarDisenoDetalle()
        {
            int ancho = pnlListado.ClientSize.Width;
            int alto = pnlListado.ClientSize.Height;
            if (ancho < 400 || alto < 300) return;

            lblTituloIcono.SetBounds(16, 14, 60, 60);
            int anchoTitulo = Math.Max(280, pnlTitulo.ClientSize.Width - btnCerrarSesion.Width - 150);
            lblTitulo.SetBounds(92, 3, anchoTitulo, 44);
            lblSubtitulo.SetBounds(94, 48, anchoTitulo, 28);
            btnCerrarSesion.SetBounds(Math.Max(12, pnlTitulo.ClientSize.Width - btnCerrarSesion.Width - 28),
                22, btnCerrarSesion.Width, btnCerrarSesion.Height);

            int margen = 28;
            int separacionColumnas = 32;
            int anchoColumna = Math.Max(160, (ancho - (margen * 2) - separacionColumnas) / 2);
            int derecha = margen + anchoColumna + separacionColumnas;
            int altoEtiqueta = 22;
            int altoCampo = 38;

            label7.SetBounds(margen, 8, anchoColumna, altoEtiqueta);
            txtIdDetalleDevolucion.SetBounds(margen, 32, anchoColumna, altoCampo);
            label15.SetBounds(derecha, 8, anchoColumna, altoEtiqueta);
            txtIdDevolucion.SetBounds(derecha, 32, anchoColumna, altoCampo);
            label9.SetBounds(margen, 78, anchoColumna, altoEtiqueta);
            txtMulta.SetBounds(margen, 102, anchoColumna, altoCampo);
            label14.SetBounds(derecha, 78, anchoColumna, altoEtiqueta);
            txtCantidad.SetBounds(derecha, 102, anchoColumna, altoCampo);

            Button[] acciones = { button1, button6, button3, button5, button2, button4 };
            int altoBoton = 40;
            int espacioBoton = 8;
            int anchoBoton = Math.Max(96, Math.Min(120,
                (ancho - margen * 2 - espacioBoton * (acciones.Length - 1)) / acciones.Length));
            int anchoFilaBotones = anchoBoton * acciones.Length
                + espacioBoton * (acciones.Length - 1);
            int inicioBotones = Math.Max(margen, (ancho - anchoFilaBotones) / 2);
            for (int i = 0; i < acciones.Length; i++)
            {
                acciones[i].SetBounds(inicioBotones + i * (anchoBoton + espacioBoton), 151,
                    anchoBoton, altoBoton);
                acciones[i].Text = new[] { "Nuevo", "Guardar", "Editar", "Eliminar", "Limpiar", "Cancelar" }[i];
                acciones[i].Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                acciones[i].TextAlign = ContentAlignment.MiddleCenter;
                acciones[i].AutoEllipsis = false;
                acciones[i].ForeColor = Color.White;
                acciones[i].Cursor = Cursors.Hand;
                acciones[i].FlatStyle = FlatStyle.Flat;
                acciones[i].FlatAppearance.BorderSize = 0;
                acciones[i].FlatAppearance.MouseOverBackColor = Color.FromArgb(11, 127, 149);
            }
            button1.BackColor = Color.ForestGreen;
            button6.BackColor = Color.ForestGreen;
            button3.BackColor = Color.Goldenrod;
            button5.BackColor = Color.DarkRed;
            button2.BackColor = Color.FromArgb(230, 180, 35);
            button4.BackColor = Color.DarkRed;

            int filaBusqueda = 203;
            lblListado.SetBounds(margen, filaBusqueda, Math.Max(200, ancho / 3), 36);
            lblListado.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblListado.ForeColor = Color.White;
            int anchoLimpiarBusqueda = 96;
            int anchoBotonBuscar = 120;
            int separacionBusqueda = 8;
            int anchoDisponibleBusqueda = ancho - margen - lblListado.Right - 16
                - anchoLimpiarBusqueda - anchoBotonBuscar - (separacionBusqueda * 2);
            int anchoBuscar = Math.Min(300, Math.Max(130, anchoDisponibleBusqueda));
            int xBuscar = ancho - margen - anchoBuscar - anchoLimpiarBusqueda
                - anchoBotonBuscar - (separacionBusqueda * 2);
            txtBuscarDetalleDevolucion.SetBounds(xBuscar, filaBusqueda, anchoBuscar, 36);
            button10.Text = "Limpiar";
            button10.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button10.TextAlign = ContentAlignment.MiddleCenter;
            button10.BackColor = btnBuscar.BackColor;
            button10.ForeColor = Color.White;
            button10.Cursor = Cursors.Hand;
            button10.FlatStyle = FlatStyle.Flat;
            button10.FlatAppearance.BorderSize = 0;
            button10.FlatAppearance.MouseOverBackColor = Color.FromArgb(11, 127, 149);
            button10.SetBounds(xBuscar + anchoBuscar + separacionBusqueda,
                filaBusqueda, anchoLimpiarBusqueda, 36);
            btnBuscar.SetBounds(xBuscar + anchoBuscar + anchoLimpiarBusqueda + (separacionBusqueda * 2),
                filaBusqueda, anchoBotonBuscar, 36);

            int inicioTabla = 250;
            int espacioInferior = 62;
            dataGridView1.SetBounds(margen, inicioTabla,
                Math.Max(300, ancho - margen * 2), Math.Max(100, alto - inicioTabla - espacioInferior));
            button8.Location = new Point(margen, alto - button8.Height - 12);
            button9.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            if (encabezadoPendiente == null)
                button9.SetBounds(ancho - margen - button9.Width, alto - button9.Height - 12,
                    button9.Width, button9.Height);

            if (encabezadoPendiente != null) PosicionarBotonGuardarTodo();
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

        private void PosicionarBotonGuardarTodo()
        {
            // Reserva el pie para las acciones finales sin cubrir la tabla.
            const int espacioTablaABotones = 12;
            int altoTablaDisponible = button8.Top - dataGridView1.Top - espacioTablaABotones;
            dataGridView1.Height = Math.Max(100, altoTablaDisponible);

            // En el alta, Guardar todo queda en la franja inferior derecha, fuera de la tabla.
            int margenDerecho = 28;
            int x = pnlListado.ClientSize.Width - button9.Width - margenDerecho;
            int y = button8.Top + (button8.Height - button9.Height) / 2;
            button9.Location = new Point(x, Math.Max(0, y));
            button9.BringToFront();
        }

        private void VolverADevoluciones(object sender, EventArgs e)
        {
            string mensaje = encabezadoPendiente != null
                ? "¿Volver a Devoluciones? Se descartará el alta y los detalles que aún no se hayan guardado."
                : edicionIniciada
                    ? "¿Volver a Devoluciones? Se descartarán los cambios del detalle que aún no se hayan guardado."
                    : "¿Volver a Devoluciones?";

            if (DialogoSistema.Confirmar(this, "Volver a Devoluciones", mensaje,
                "Sí, volver", "Continuar aquí", EstiloDialogo.Normal) != DialogResult.Yes) return;

            if (encabezadoPendiente != null)
            {
                DetallesPendientes.Clear();
                DialogResult = DialogResult.Cancel;
            }
            else if (edicionIniciada)
            {
                edicionIniciada = false;
                LimpiarCamposDetalle();
            }

            Close();
        }

        private void EstablecerCamposDetalleEditables(bool editables)
        {
            // Los códigos se muestran como referencia; cantidad y multa se editan por acción explícita.
            txtIdDetalleDevolucion.ReadOnly = true;
            txtIdDevolucion.ReadOnly = true;
            txtCantidad.ReadOnly = !editables;
            txtMulta.ReadOnly = !editables;
        }

        private void ConfigurarConsulta()
        {
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            if (encabezadoPendiente == null)
            {
                btnBuscar.Click += (s, e) => BuscarDetalles(txtBuscarDetalleDevolucion.Text.Trim());
                button10.Click += (s, e) =>
                {
                    txtBuscarDetalleDevolucion.Clear();
                    CargarDetalles(string.Empty);
                };
                dataGridView1.CellClick += SeleccionarDetalle;
                button3.Click += IniciarEdicion;
                button5.Click += EliminarSeleccionado;
                button6.Click += GuardarEdicion;
                button4.Click += CancelarOperacion;
                button9.Click += (s, e) => Close();
            }
            else
            {
                button1.Click += LimpiarDetalleActual;
                button4.Click += CancelarOperacion;
                button6.Click += AgregarDetallePendiente;
                button9.Click += ConfirmarOperacion;
            }
            button2.Click += ConfirmarLimpiarDetalle;
            button7.Click += ConfirmarLimpiarDetalle;
        }

        /// <summary>Actualiza los datos del encabezado seleccionado en la cuadrícula existente.</summary>
        public void CargarPorDevolucion(int idDevolucion)
        {
            if (encabezadoPendiente != null)
                throw new InvalidOperationException("El formulario está capturando detalles para un encabezado nuevo.");
            if (idDevolucion <= 0)
                throw new ArgumentException("El código del encabezado debe ser válido.");

            idDevolucionSeleccionada = idDevolucion;
            edicionIniciada = false;
            txtIdDevolucion.Text = idDevolucion.ToString();
            lblSubtitulo.Text = "Devolución #" + idDevolucion;
            txtIdDetalleDevolucion.Clear();
            txtCantidad.Clear();
            txtMulta.Clear();
            txtBuscarDetalleDevolucion.Clear();
            CargarDetalles(string.Empty);
        }

        private void BuscarDetalles(string filtro)
        {
            if (!CargarDetalles(filtro)) return;
            int cantidad = resultados.Count;
            MostrarMensaje("Búsqueda de detalles",
                cantidad == 0 ? "No se encontraron detalles con ese criterio en esta devolución." :
                "Se encontraron " + cantidad + " detalle(s) en esta devolución.",
                cantidad == 0 ? EstiloDialogo.Normal : EstiloDialogo.Exito);
        }

        private bool CargarDetalles(string filtro)
        {
            if (idDevolucionSeleccionada <= 0)
            {
                dataGridView1.DataSource = null;
                return false;
            }

            try
            {
                // El id del encabezado siempre limita la consulta de las filas hijas.
                resultados = negocio.ConsultarPorDevolucion(idDevolucionSeleccionada,
                    filtro == "🔍" ? string.Empty : filtro);
                MostrarResultados();
                AcomodarColumnas();
                return true;
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error de consulta", ex.Message, EstiloDialogo.Error);
                return false;
            }
        }

        private void MostrarResultados()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = resultados;
            AcomodarColumnas();
        }

        private void SeleccionarDetalle(object sender, DataGridViewCellEventArgs e)
        {
            if (encabezadoPendiente != null) return;
            if (e.RowIndex < 0) return;
            var detalle = dataGridView1.Rows[e.RowIndex].DataBoundItem as DetalleDevolucionEntidad;
            if (detalle == null) return;

            txtIdDetalleDevolucion.Text = detalle.IDDetalleDevolucion.ToString();
            txtIdDevolucion.Text = detalle.IDDevoluciones.ToString();
            // Las cajas existentes quedaron ubicadas frente a la etiqueta opuesta en el diseñador.
            txtMulta.Text = detalle.Cantidad.ToString();
            txtCantidad.Text = detalle.Multa.HasValue ? detalle.Multa.Value.ToString("0") : string.Empty;
        }

        private void AgregarDetallePendiente(object sender, EventArgs e)
        {
            try
            {
                int cantidad;
                if (!int.TryParse(txtMulta.Text.Trim(), out cantidad))
                    throw new ArgumentException("Escribe una cantidad válida.");

                decimal multa;
                bool tieneMulta = !string.IsNullOrWhiteSpace(txtCantidad.Text);
                if (tieneMulta && !decimal.TryParse(txtCantidad.Text.Trim(), out multa))
                    throw new ArgumentException("Escribe una multa válida.");

                var detalle = new DetalleDevolucionEntidad
                {
                    Cantidad = cantidad,
                    Multa = tieneMulta ? decimal.Parse(txtCantidad.Text.Trim()) : (decimal?)null
                };
                negocio.ValidarParaNuevaDevolucion(detalle);
                DetallesPendientes.Add(detalle);
                resultados = new List<DetalleDevolucionEntidad>(DetallesPendientes);
                MostrarResultados();
                LimpiarDetalleActual(sender, e);
                MostrarMensaje("Detalle agregado a la lista", "Todavía no se guardó en la base. Agrega otro o elige Guardar todo.", EstiloDialogo.Normal);
            }
            catch (Exception ex)
            {
                MostrarMensaje("Datos de detalle inválidos", ex.Message, EstiloDialogo.Error);
            }
        }

        private void LimpiarDetalleActual(object sender, EventArgs e)
        {
            txtIdDetalleDevolucion.Clear();
            txtCantidad.Clear();
            txtMulta.Clear();
            EstablecerCamposDetalleEditables(encabezadoPendiente != null);
            txtMulta.Focus();
        }

        private void ConfirmarOperacion(object sender, EventArgs e)
        {
            if (DetallesPendientes.Count == 0)
            {
                MostrarMensaje("Falta el detalle", "Agrega al menos una fila de detalle antes de guardar todo.", EstiloDialogo.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelarOperacion(object sender, EventArgs e)
        {
            if (encabezadoPendiente == null && !edicionIniciada)
            {
                LimpiarCamposDetalle();
                EstablecerCamposDetalleEditables(false);
                MostrarMensaje("Sin edición activa", "Selecciona un detalle y pulsa Editar para iniciar cambios.", EstiloDialogo.Normal);
                return;
            }
            string titulo = encabezadoPendiente == null ? "Cancelar edición" : "Cancelar devolución";
            string mensaje = encabezadoPendiente == null
                ? "¿Descartar los cambios del detalle?"
                : "¿Cancelar el alta de la devolución y descartar todos sus detalles pendientes?";
            if (DialogoSistema.Confirmar(this, titulo, mensaje,
                "Sí, cancelar", "Continuar", EstiloDialogo.Normal) != DialogResult.Yes) return;

            DetallesPendientes.Clear();
            if (encabezadoPendiente != null)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            edicionIniciada = false;
            LimpiarCamposDetalle();
            EstablecerCamposDetalleEditables(false);
            MostrarMensaje("Edición cancelada", "No se guardaron cambios en el detalle.", EstiloDialogo.Normal);
        }

        private void IniciarEdicion(object sender, EventArgs e)
        {
            if (edicionIniciada)
            {
                MostrarMensaje("Edición en curso", "Guarda o cancela la edición actual antes de iniciar otra.", EstiloDialogo.Normal);
                return;
            }
            int idDetalle;
            int idDevolucion;
            if (!int.TryParse(txtIdDetalleDevolucion.Text, out idDetalle) || idDetalle <= 0 ||
                !int.TryParse(txtIdDevolucion.Text, out idDevolucion) || idDevolucion != idDevolucionSeleccionada)
            {
                MostrarMensaje("Editar detalle", "Selecciona un detalle de la devolución actual.", EstiloDialogo.Normal);
                return;
            }
            edicionIniciada = true;
            EstablecerCamposDetalleEditables(true);
            txtMulta.Focus();
        }

        private void GuardarEdicion(object sender, EventArgs e)
        {
            try
            {
                if (!edicionIniciada)
                    throw new InvalidOperationException("Pulsa Editar y selecciona un detalle antes de guardar.");
                int idDetalle;
                if (!int.TryParse(txtIdDetalleDevolucion.Text, out idDetalle))
                    throw new ArgumentException("Selecciona un detalle válido.");
                int cantidad;
                if (!int.TryParse(txtMulta.Text.Trim(), out cantidad))
                    throw new ArgumentException("Escribe una cantidad entera válida.");
                decimal multa;
                bool tieneMulta = !string.IsNullOrWhiteSpace(txtCantidad.Text);
                if (tieneMulta && !decimal.TryParse(txtCantidad.Text.Trim(), out multa))
                    throw new ArgumentException("Escribe una multa válida.");

                var detalle = new DetalleDevolucionEntidad
                {
                    IDDetalleDevolucion = idDetalle,
                    IDDevoluciones = idDevolucionSeleccionada,
                    Cantidad = cantidad,
                    Multa = tieneMulta ? decimal.Parse(txtCantidad.Text.Trim()) : (decimal?)null
                };
                if (DialogoSistema.Confirmar(this, "Confirmar edición",
                    "¿Guardar los cambios del detalle " + idDetalle + "?",
                    "Sí, guardar", "Cancelar", EstiloDialogo.Normal) != DialogResult.Yes) return;

                negocio.Editar(detalle);
                edicionIniciada = false;
                EstablecerCamposDetalleEditables(false);
                CargarDetalles(txtBuscarDetalleDevolucion.Text.Trim());
                LimpiarCamposDetalle();
                MostrarMensaje("Detalle actualizado", "Los cambios se guardaron correctamente.", EstiloDialogo.Exito);
            }
            catch (Exception ex)
            {
                MostrarMensaje("No se pudo editar el detalle", ex.Message, EstiloDialogo.Error);
            }
        }

        private void EliminarSeleccionado(object sender, EventArgs e)
        {
            if (edicionIniciada)
            {
                MostrarMensaje("Edición en curso", "Guarda o cancela los cambios antes de eliminar.", EstiloDialogo.Normal);
                return;
            }
            int idDetalle;
            if (!int.TryParse(txtIdDetalleDevolucion.Text, out idDetalle) || idDetalle <= 0 || idDevolucionSeleccionada <= 0)
            {
                MostrarMensaje("Eliminar detalle", "Selecciona un detalle de la devolución actual.", EstiloDialogo.Normal);
                return;
            }
            if (DialogoSistema.Confirmar(this, "Confirmar eliminación",
                "¿Eliminar el detalle " + idDetalle + " de la devolución " + idDevolucionSeleccionada + "?",
                "Sí, eliminar", "No", EstiloDialogo.Peligro) != DialogResult.Yes) return;
            try
            {
                negocio.Eliminar(idDevolucionSeleccionada, idDetalle);
                CargarDetalles(txtBuscarDetalleDevolucion.Text.Trim());
                LimpiarCamposDetalle();
                MostrarMensaje("Detalle eliminado", "El detalle se eliminó correctamente.", EstiloDialogo.Exito);
            }
            catch (Exception ex)
            {
                MostrarMensaje("No se pudo eliminar el detalle", ex.Message, EstiloDialogo.Error);
            }
        }

        private void ConfirmarLimpiarDetalle(object sender, EventArgs e)
        {
            if (DialogoSistema.Confirmar(this, "Limpiar campos del detalle",
                "¿Borrar los datos escritos en los campos del detalle?", "Sí, limpiar", "Cancelar", EstiloDialogo.Normal)
                != DialogResult.Yes) return;
            edicionIniciada = false;
            EstablecerCamposDetalleEditables(encabezadoPendiente != null);
            LimpiarCamposDetalle();
            MostrarMensaje("Campos limpiados", "Se limpiaron los campos; el encabezado y la lista de detalles se conservaron.", EstiloDialogo.Exito);
        }

        private void LimpiarCamposDetalle()
        {
            txtIdDetalleDevolucion.Clear();
            txtCantidad.Clear();
            txtMulta.Clear();
        }

        private void AcomodarColumnas()
        {
            ConfigurarColumna("IDDetalleDevolucion", "Código detalle", 24,
                DataGridViewContentAlignment.MiddleCenter);
            ConfigurarColumna("IDDevoluciones", "Código devolución", 24,
                DataGridViewContentAlignment.MiddleCenter);
            ConfigurarColumna("Cantidad", "Cantidad devuelta", 26,
                DataGridViewContentAlignment.MiddleRight);
            ConfigurarColumna("Multa", "Multa", 26,
                DataGridViewContentAlignment.MiddleRight);

            if (encabezadoPendiente != null)
            {
                if (dataGridView1.Columns.Contains("IDDetalleDevolucion"))
                    dataGridView1.Columns["IDDetalleDevolucion"].Visible = false;
                if (dataGridView1.Columns.Contains("IDDevoluciones"))
                    dataGridView1.Columns["IDDevoluciones"].Visible = false;
            }

            if (dataGridView1.Columns.Contains("Cantidad"))
                dataGridView1.Columns["Cantidad"].DefaultCellStyle.Format = "N0";
            if (dataGridView1.Columns.Contains("Multa"))
                dataGridView1.Columns["Multa"].DefaultCellStyle.Format = "N0";
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

        private void DetalleDevolucionForms_Load(object sender, EventArgs e)
        {

        }
    }
}
