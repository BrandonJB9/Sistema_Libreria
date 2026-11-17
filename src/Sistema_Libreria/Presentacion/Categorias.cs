using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    public partial class Categorias : Form
    {
        private readonly CategoriaNegocio negocio = new CategoriaNegocio();
        private readonly ComboBox cboLibros = new ComboBox();
        private readonly Button btnAnterior = new Button();
        private readonly Button btnSiguiente = new Button();
        private readonly Label lblPagina = new Label();
        private DataTable resultados = new DataTable();
        private int paginaActual;
        private const int FilasPorPagina = 10;
        private bool modoEdicion;
        private bool esNuevo;
        private bool suspenderCargaSeleccion;

        public Categorias()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            button2.Visible = false;
            button10.Visible = false;
            Shown += (s, e) => WindowState = FormWindowState.Maximized;
            AplicarEstiloTablaCategorias();
            PrepararControles();
            ConfigurarDisenoResponsive();
            ConfigurarBotonCerrarSesion();
            EnlazarEventos();
            ConfigurarModoConsulta();
        }

        private void ConfigurarDisenoResponsive()
        {
            SuspendLayout();
            pnlTop.Visible = false;
            pnlContenido.Padding = new Padding(18, 12, 18, 16);
            tlpStats.Height = 162;
            tlpStats.MinimumSize = new Size(0, 150);
            tlpStats.ColumnCount = 3;
            tlpStats.RowCount = 1;
            tlpStats.ColumnStyles.Clear();
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
            PrepararTarjeta(panel4, label10, label11, label12);
            PrepararTarjeta(panel2, label4, label5, label6);
            PrepararTarjeta(panel1, label1, label2, label3);
            pnlListado.Padding = new Padding(18, 12, 18, 14);

            // La barra de acciones permite que sus controles pasen a otra línea
            // cuando el ancho de la ventana no alcanza.
            pnlListadoTitulo.Controls.Clear();
            pnlListadoTitulo.AutoSize = true;
            pnlListadoTitulo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlListadoTitulo.Padding = new Padding(0, 4, 0, 10);

            var acciones = new FlowLayoutPanel
            {
                Name = "flpAccionesCategorias",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = pnlListadoTitulo.BackColor,
                Margin = Padding.Empty,
                Padding = new Padding(10, 6, 10, 6)
            };

            lblListado.AutoSize = false;
            lblListado.Dock = DockStyle.None;
            lblListado.Size = new Size(260, 44);
            lblListado.MinimumSize = new Size(220, 44);
            lblListado.AutoEllipsis = true;
            lblListado.TextAlign = ContentAlignment.MiddleLeft;
            lblListado.Margin = new Padding(3);
            txtBuscarCategoria.Multiline = false;
            txtBuscarCategoria.TextAlign = HorizontalAlignment.Left;
            txtBuscarCategoria.Width = 180;
            txtBuscarCategoria.Height = 32;
            txtBuscarCategoria.MinimumSize = new Size(145, 28);
            txtBuscarCategoria.Margin = new Padding(3, 7, 3, 3);

            Button[] botonesAccion = { button1, button6, button5, button4, button3, button2, button7 };
            foreach (Button boton in botonesAccion)
            {
                boton.Size = new Size(122, 40);
                boton.MinimumSize = new Size(108, 40);
                boton.Margin = new Padding(3);
                boton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }

            acciones.Controls.Add(lblListado);
            acciones.Controls.AddRange(botonesAccion);
            acciones.Controls.Add(txtBuscarCategoria);
            foreach (Button boton in new[] { btnBuscar, button10 })
            {
                boton.Size = new Size(122, 40);
                boton.MinimumSize = new Size(108, 40);
                boton.Margin = new Padding(3);
                acciones.Controls.Add(boton);
            }
            pnlListadoTitulo.Padding = Padding.Empty;
            pnlListadoTitulo.MinimumSize = new Size(0, 52);
            pnlListadoTitulo.Controls.Add(acciones);

            var campos = new TableLayoutPanel
            {
                Name = "tlpCamposCategorias",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(4, 8, 4, 12),
                Margin = new Padding(0),
                BackColor = pnlListado.BackColor
            };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            for (int fila = 0; fila < 3; fila++)
                campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

            Label[] etiquetas = { label7, label13, label15, label14, label17 };
            foreach (Label etiqueta in etiquetas)
            {
                etiqueta.Dock = DockStyle.Fill;
                etiqueta.Margin = new Padding(4, 4, 8, 4);
                etiqueta.TextAlign = ContentAlignment.MiddleLeft;
                etiqueta.AutoSize = false;
            }

            Control[] entradas = { txtIdCategoria, txtCategoria, cboLibros, txtDescripcion };
            foreach (Control entrada in entradas)
            {
                entrada.Dock = DockStyle.Fill;
                entrada.Margin = new Padding(4, 7, 12, 7);
                entrada.MinimumSize = new Size(80, 30);
            }

            var estado = new FlowLayoutPanel
            {
                Name = "flpEstadoCategoria",
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = pnlListado.BackColor,
                Margin = new Padding(4, 4, 4, 4),
                Padding = new Padding(8, 8, 0, 0)
            };
            rbEstadoActivo.AutoSize = true;
            rbEstadoInactivo.AutoSize = true;
            rbEstadoActivo.ForeColor = Color.White;
            rbEstadoInactivo.ForeColor = Color.White;
            rbEstadoActivo.Margin = new Padding(4, 0, 20, 0);
            rbEstadoInactivo.Margin = new Padding(4, 0, 12, 0);
            estado.Controls.Add(rbEstadoActivo);
            estado.Controls.Add(rbEstadoInactivo);

            campos.Controls.Add(label7, 0, 0);
            campos.Controls.Add(txtIdCategoria, 1, 0);
            campos.Controls.Add(label13, 2, 0);
            campos.Controls.Add(txtCategoria, 3, 0);
            campos.Controls.Add(label15, 0, 1);
            campos.Controls.Add(cboLibros, 1, 1);
            campos.Controls.Add(label14, 2, 1);
            campos.Controls.Add(txtDescripcion, 3, 1);
            campos.Controls.Add(label17, 0, 2);
            campos.Controls.Add(estado, 1, 2);
            campos.SetColumnSpan(estado, 3);

            var pie = new TableLayoutPanel
            {
                Name = "tlpPieCategorias",
                Dock = DockStyle.Bottom,
                Height = 58,
                ColumnCount = 3,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = pnlListado.BackColor
            };
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pie.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            button8.Dock = DockStyle.Left;
            button8.Size = new Size(190, 50);
            button8.MinimumSize = new Size(160, 44);
            button8.Margin = new Padding(2, 2, 2, 2);
            button9.Dock = DockStyle.Right;
            button9.Size = new Size(150, 50);
            button9.Margin = new Padding(2, 2, 2, 2);

            var paginacion = new FlowLayoutPanel
            {
                Name = "flpPaginacionCategorias",
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Anchor = AnchorStyles.None,
                BackColor = pnlListado.BackColor,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            paginacion.Controls.Add(btnAnterior);
            paginacion.Controls.Add(lblPagina);
            paginacion.Controls.Add(btnSiguiente);
            pie.Controls.Add(button8, 0, 0);
            pie.Controls.Add(paginacion, 1, 0);
            pie.Controls.Add(button9, 2, 0);

            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Margin = new Padding(2, 6, 2, 6);
            dataGridView1.MinimumSize = new Size(0, 120);
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            pnlListado.Controls.Clear();
            pnlListado.Controls.Add(dataGridView1);
            pnlListado.Controls.Add(pie);
            pnlListado.Controls.Add(campos);
            pnlListado.Controls.Add(pnlListadoTitulo);

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

        private void ConfigurarBotonCerrarSesion()
        {
            Action ajustar = () =>
            {
                btnCerrarSesion.Visible = true;
                btnCerrarSesion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnCerrarSesion.Size = new Size(255, 50);
                btnCerrarSesion.Location = new Point(
                    Math.Max(12, pnlTitulo.ClientSize.Width - btnCerrarSesion.Width - 18), 16);
                btnCerrarSesion.BringToFront();
            };
            pnlTitulo.SizeChanged += (sender, e) => ajustar();
            ajustar();
        }

        private void AplicarEstiloTablaCategorias()
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
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void PrepararControles()
        {
            txtIdCategoria.ReadOnly = true;
            txtIdCategoria.BackColor = ColorTranslator.FromHtml("#C0C4C8");
            txtIdCategoria.ForeColor = ColorTranslator.FromHtml("#454B52");

            cboLibros.Name = "cboLibros";
            cboLibros.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLibros.Location = txtIdLibro.Location;
            cboLibros.Size = txtIdLibro.Size;
            cboLibros.Margin = txtIdLibro.Margin;
            cboLibros.BackColor = txtIdLibro.BackColor;
            cboLibros.ForeColor = txtIdLibro.ForeColor;
            cboLibros.Font = txtIdLibro.Font;
            cboLibros.Anchor = txtIdLibro.Anchor;
            txtIdLibro.Visible = false;
            txtIdLibro.Parent.Controls.Add(cboLibros);
            txtIdLibro.Parent.Controls.SetChildIndex(cboLibros, txtIdLibro.Parent.Controls.GetChildIndex(txtIdLibro));
            label15.Text = "📖 Libro asociado";

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            btnAnterior.Text = "◀ Anterior";
            btnSiguiente.Text = "Siguiente ▶";
            lblPagina.TextAlign = ContentAlignment.MiddleCenter;
            lblPagina.AutoSize = false;
            lblPagina.Size = new Size(150, 34);
            btnAnterior.Size = new Size(130, 34);
            btnSiguiente.Size = new Size(130, 34);
        }

        private void EnlazarEventos()
        {
            Load += Categorias_Load;
            btnBuscar.Click += btnBuscar_Click;
            button10.Click += btnLimpiarBusqueda_Click;
            button1.Click += buttonNuevo_Click;
            button2.Click += buttonLimpiar_Click;
            button7.Click += buttonLimpiar_Click;
            button3.Click += buttonEditar_Click;
            button4.Click += buttonCancelar_Click;
            button5.Click += buttonEliminar_Click;
            button6.Click += buttonGuardar_Click;
            button9.Click += buttonCerrar_Click;
            btnCerrarSesion.Click += buttonCerrar_Click;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            btnAnterior.Click += btnAnterior_Click;
            btnSiguiente.Click += btnSiguiente_Click;
        }

        private void Categorias_Load(object sender, EventArgs e)
        {
            try
            {
                cboLibros.DisplayMember = "Titulo";
                cboLibros.ValueMember = "IDLibro";
                cboLibros.DataSource = negocio.ConsultarCatalogoLibros();
                ActualizarTotales();
                CargarCategorias(string.Empty);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void CargarCategorias(string filtro)
        {
            resultados = negocio.Consultar(filtro);
            paginaActual = 0;
            MostrarPagina();
        }

        private void MostrarPagina()
        {
            DataTable pagina = resultados.Clone();
            int inicio = paginaActual * FilasPorPagina;
            int fin = Math.Min(inicio + FilasPorPagina, resultados.Rows.Count);
            for (int i = inicio; i < fin; i++)
                pagina.ImportRow(resultados.Rows[i]);

            dataGridView1.DataSource = pagina;
            if (dataGridView1.Columns.Contains("IDLibro"))
                dataGridView1.Columns["IDLibro"].Visible = false;
            if (dataGridView1.Columns.Contains("Titulo"))
                dataGridView1.Columns["Titulo"].HeaderText = "Libro";
            if (dataGridView1.Columns.Contains("IDCategoria"))
                dataGridView1.Columns["IDCategoria"].HeaderText = "Código categoría";
            if (dataGridView1.Columns.Contains("Categoria"))
                dataGridView1.Columns["Categoria"].HeaderText = "Categoría";
            if (dataGridView1.Columns.Contains("Descripcion"))
                dataGridView1.Columns["Descripcion"].HeaderText = "Descripción";
            if (dataGridView1.Columns.Contains("Estado"))
                dataGridView1.Columns["Estado"].HeaderText = "Activa";

            int paginas = Math.Max(1, (int)Math.Ceiling(resultados.Rows.Count / (double)FilasPorPagina));
            bool hayVariasPaginas = paginas > 1;
            btnAnterior.Visible = hayVariasPaginas;
            btnSiguiente.Visible = hayVariasPaginas;
            lblPagina.Visible = hayVariasPaginas;
            lblPagina.Text = "Página " + (paginaActual + 1) + " de " + paginas;
            btnAnterior.Enabled = paginaActual > 0;
            btnSiguiente.Enabled = paginaActual + 1 < paginas;
        }

        private void ActualizarTotales()
        {
            int[] totales = negocio.ConsultarTotales();
            label12.Text = totales[0].ToString();
            label6.Text = totales[1].ToString();
            label3.Text = totales[2].ToString();
        }

        private CategoriaEntidad LeerFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtCategoria.Text))
                throw new ArgumentException("Escribe el nombre de la categoría.");
            if (cboLibros.SelectedValue == null)
                throw new ArgumentException("Selecciona un libro asociado.");

            int idLibro;
            if (!int.TryParse(cboLibros.SelectedValue.ToString(), out idLibro))
                throw new ArgumentException("El libro seleccionado no es válido.");
            if (!rbEstadoActivo.Checked && !rbEstadoInactivo.Checked)
                throw new ArgumentException("Selecciona si la categoría está activa o inactiva.");

            int idCategoria = 0;
            int.TryParse(txtIdCategoria.Text, out idCategoria);
            return new CategoriaEntidad
            {
                IDCategoria = idCategoria,
                IDLibro = idLibro,
                Categoria = txtCategoria.Text.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim(),
                Estado = rbEstadoActivo.Checked
            };
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                CargarCategorias(FiltroActual());
                int cantidad = resultados.Rows.Count;
                DialogoSistema.Mostrar(this, "Búsqueda de categorías",
                    cantidad == 0 ? "No se encontraron categorías con ese criterio."
                        : "Se encontraron " + cantidad + (cantidad == 1 ? " categoría." : " categorías."),
                    cantidad == 0 ? EstiloDialogo.Normal : EstiloDialogo.Exito);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void btnLimpiarBusqueda_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtBuscarCategoria.Text) &&
                    DialogoSistema.Confirmar(this, "Limpiar búsqueda", "¿Deseas borrar el texto de búsqueda?",
                        "Sí, limpiar", "Cancelar", EstiloDialogo.Normal) != DialogResult.Yes)
                    return;

                suspenderCargaSeleccion = true;
                txtBuscarCategoria.Clear();
                CargarCategorias(string.Empty);
                dataGridView1.ClearSelection();
                dataGridView1.CurrentCell = null;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
            finally
            {
                suspenderCargaSeleccion = false;
            }
        }

        private void buttonNuevo_Click(object sender, EventArgs e)
        {
            esNuevo = true;
            modoEdicion = true;
            LimpiarFormulario();
            ConfigurarModoEdicion();
            if (cboLibros.Items.Count > 0)
                cboLibros.SelectedIndex = 0;
            rbEstadoActivo.Checked = true;
            txtCategoria.Focus();
        }

        private void buttonEditar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                DialogoSistema.Mostrar(this, "Editar categoría", "Selecciona una categoría de la lista.", EstiloDialogo.Normal);
                return;
            }
            CargarSeleccionEnFormulario();
            esNuevo = false;
            modoEdicion = true;
            ConfigurarModoEdicion();
        }

        private void buttonGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                CategoriaEntidad categoria = LeerFormulario();
                if (esNuevo)
                    negocio.Agregar(categoria);
                else
                {
                    if (DialogoSistema.Confirmar(this, "Confirmar cambios",
                        "¿Deseas guardar los cambios de la categoría “" + categoria.Categoria +
                        "” (código " + categoria.IDCategoria + ")?",
                        "Sí, guardar", "Cancelar", EstiloDialogo.Normal) != DialogResult.Yes)
                        return;
                    negocio.Editar(categoria);
                }

                DialogoSistema.Mostrar(this, esNuevo ? "Categoría agregada" : "Cambios guardados",
                    esNuevo ? "La categoría se agregó correctamente." : "Los cambios se guardaron correctamente.",
                    EstiloDialogo.Exito);
                modoEdicion = false;
                esNuevo = false;
                ConfigurarModoConsulta();
                ActualizarTotales();
                CargarCategorias(FiltroActual());
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void buttonEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    DialogoSistema.Mostrar(this, "Eliminar categoría", "Selecciona una categoría de la lista.", EstiloDialogo.Normal);
                    return;
                }

                int idCategoria = Convert.ToInt32(dataGridView1.CurrentRow.Cells["IDCategoria"].Value);
                string nombreCategoria = Convert.ToString(dataGridView1.CurrentRow.Cells["Categoria"].Value);
                DialogResult respuesta = DialogoSistema.Confirmar(this, "Confirmar eliminación",
                    "¿Confirmas eliminar la categoría “" + nombreCategoria + "” (código " + idCategoria + ")?",
                    "Sí, eliminar", "No", EstiloDialogo.Peligro);
                if (respuesta != DialogResult.Yes)
                    return;

                negocio.Eliminar(idCategoria);
                ActualizarTotales();
                CargarCategorias(FiltroActual());
                LimpiarFormulario();
                ConfigurarModoConsulta();
                DialogoSistema.Mostrar(this, "Categoría eliminada", "La categoría se eliminó correctamente.", EstiloDialogo.Exito);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (!modoEdicion && !suspenderCargaSeleccion)
                CargarSeleccionEnFormulario();
        }

        private void CargarSeleccionEnFormulario()
        {
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.Cells["IDCategoria"].Value == null)
                return;

            DataGridViewRow fila = dataGridView1.CurrentRow;
            txtIdCategoria.Text = Convert.ToString(fila.Cells["IDCategoria"].Value);
            txtCategoria.Text = Convert.ToString(fila.Cells["Categoria"].Value);
            txtDescripcion.Text = fila.Cells["Descripcion"].Value == DBNull.Value
                ? string.Empty : Convert.ToString(fila.Cells["Descripcion"].Value);
            cboLibros.SelectedValue = Convert.ToInt32(fila.Cells["IDLibro"].Value);
            bool estado = Convert.ToBoolean(fila.Cells["Estado"].Value);
            rbEstadoActivo.Checked = estado;
            rbEstadoInactivo.Checked = !estado;
        }

        private void LimpiarFormulario()
        {
            txtIdCategoria.Clear();
            txtIdLibro.Clear();
            txtCategoria.Clear();
            txtDescripcion.Clear();
            cboLibros.SelectedIndex = -1;
            rbEstadoActivo.Checked = false;
            rbEstadoInactivo.Checked = false;
        }

        private void buttonLimpiar_Click(object sender, EventArgs e)
        {
            try
            {
                bool hayDatos = !string.IsNullOrWhiteSpace(txtIdCategoria.Text) ||
                    !string.IsNullOrWhiteSpace(txtCategoria.Text) ||
                    !string.IsNullOrWhiteSpace(txtDescripcion.Text) ||
                    cboLibros.SelectedIndex >= 0 || rbEstadoActivo.Checked || rbEstadoInactivo.Checked ||
                    !string.IsNullOrWhiteSpace(txtBuscarCategoria.Text);
                if (hayDatos && DialogoSistema.Confirmar(this, "Limpiar formulario",
                    "¿Deseas borrar los datos escritos en los campos?", "Sí, limpiar", "Cancelar",
                    EstiloDialogo.Normal) != DialogResult.Yes)
                    return;

                modoEdicion = false;
                esNuevo = false;
                ConfigurarModoConsulta();
                suspenderCargaSeleccion = true;
                txtBuscarCategoria.Clear();
                LimpiarFormulario();
                dataGridView1.ClearSelection();
                dataGridView1.CurrentCell = null;
                CargarCategorias(string.Empty);
                dataGridView1.ClearSelection();
                dataGridView1.CurrentCell = null;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
            finally
            {
                suspenderCargaSeleccion = false;
            }
        }

        private void buttonCancelar_Click(object sender, EventArgs e)
        {
            bool estabaAgregando = esNuevo;
            modoEdicion = false;
            esNuevo = false;
            ConfigurarModoConsulta();
            if (estabaAgregando)
                LimpiarFormulario();
            else
                CargarSeleccionEnFormulario();
            DialogoSistema.Mostrar(this, "Operación cancelada", "No se realizaron cambios.", EstiloDialogo.Normal);
        }

        private void buttonCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private string FiltroActual()
        {
            string filtro = txtBuscarCategoria.Text.Trim();
            return filtro == "🔍" ? string.Empty : filtro;
        }

        private void ConfigurarModoConsulta()
        {
            txtIdCategoria.ReadOnly = true;
            txtCategoria.ReadOnly = true;
            txtDescripcion.ReadOnly = true;
            cboLibros.Enabled = false;
            rbEstadoActivo.Enabled = false;
            rbEstadoInactivo.Enabled = false;
            button6.Enabled = false;
            button4.Enabled = false;
            button1.Enabled = true;
            button3.Enabled = true;
            button5.Enabled = true;
        }

        private void ConfigurarModoEdicion()
        {
            txtIdCategoria.ReadOnly = true;
            txtCategoria.ReadOnly = false;
            txtDescripcion.ReadOnly = false;
            cboLibros.Enabled = true;
            rbEstadoActivo.Enabled = true;
            rbEstadoInactivo.Enabled = true;
            button6.Enabled = true;
            button4.Enabled = true;
        }

        private void btnAnterior_Click(object sender, EventArgs e)
        {
            if (paginaActual > 0)
            {
                paginaActual--;
                MostrarPagina();
            }
        }

        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            if ((paginaActual + 1) * FilasPorPagina < resultados.Rows.Count)
            {
                paginaActual++;
                MostrarPagina();
            }
        }

        private void MostrarError(Exception ex)
        {
            DialogoSistema.Mostrar(this, "No se pudo completar la operación", ex.Message, EstiloDialogo.Error);
        }
    }
}
