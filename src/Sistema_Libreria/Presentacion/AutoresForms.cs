using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    public partial class AutoresForms : Form
    {
        private readonly AutorNegocio negocio = new AutorNegocio();

        private readonly ComboBox cboLibros = new ComboBox();
        private readonly Button btnAnterior = new Button();
        private readonly Button btnSiguiente = new Button();
        private readonly Label lblPagina = new Label();
        private TableLayoutPanel tlpListadoResponsivo;
        private TableLayoutPanel tlpCamposResponsivo;
        private TableLayoutPanel tlpPieResponsivo;
        private FlowLayoutPanel flpAccionesResponsivo;
        private FlowLayoutPanel flpPaginacionResponsiva;
        private DataTable resultados = new DataTable();
        private int paginaActual;
        private const int FilasPorPagina = 10;
        private bool modoEdicion;
        private bool esNuevo;

        public AutoresForms()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            button2.Visible = false;
            button10.Visible = false;
            Shown += (s, e) => WindowState = FormWindowState.Maximized;
            PrepararControles();
            AplicarEstiloTablaAutores();
            ConfigurarLayoutResponsivo();
            ConfigurarBotonCerrarSesion();
            EnlazarEventos();
            ConfigurarModoConsulta();
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

        private void AplicarEstiloTablaAutores()
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
            txtIdAutor.ReadOnly = true;
            txtIdAutor.BackColor = ColorTranslator.FromHtml("#C0C4C8");
            txtIdAutor.ForeColor = ColorTranslator.FromHtml("#454B52");

            // Reutiliza el espacio del campo FK existente sin editar el archivo Designer.
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

            dtpFechaNacimiento.ShowCheckBox = true;
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;

            // Los controles de navegación se muestran solo si existen varias páginas.
            btnAnterior.Text = "◀ Anterior";
            btnSiguiente.Text = "Siguiente ▶";
            lblPagina.TextAlign = ContentAlignment.MiddleCenter;
            lblPagina.ForeColor = Color.White;
            lblPagina.AutoSize = false;
            lblPagina.Size = new Size(160, 34);
            btnAnterior.Size = new Size(130, 34);
            btnSiguiente.Size = new Size(130, 34);
            dataGridView1.Parent.Controls.Add(btnAnterior);
            dataGridView1.Parent.Controls.Add(lblPagina);
            dataGridView1.Parent.Controls.Add(btnSiguiente);
        }

        private void ConfigurarLayoutResponsivo()
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
            pnlListado.Padding = new Padding(18, 12, 18, 12);
            pnlListadoTitulo.Dock = DockStyle.Fill;
            lblListado.Dock = DockStyle.Top;
            lblListado.Height = 42;
            lblListado.TextAlign = ContentAlignment.MiddleLeft;

            flpAccionesResponsivo = new FlowLayoutPanel
            {
                AutoScroll = false,
                BackColor = pnlListadoTitulo.BackColor,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4),
                WrapContents = true
            };
            Control[] acciones = { button1, button6, button5, button4, button3, button7, button2, txtBuscarAutor, btnBuscar, button10 };
            foreach (Control accion in acciones)
            {
                accion.Margin = new Padding(3);
                flpAccionesResponsivo.Controls.Add(accion);
            }

            // Normaliza tamaños y textos para que las acciones no se expandan
            // con el escalado de Windows y sigan visibles al cambiar el ancho.
            Button[] botonesEdicion = { button1, button6, button5, button4, button3, button7, button2 };
            string[] textosEdicion = { "Nuevo", "Guardar", "Eliminar", "Cancelar", "Editar", "Limpiar", "Limpiar" };
            for (int i = 0; i < botonesEdicion.Length; i++)
                ConfigurarBotonAccion(botonesEdicion[i], textosEdicion[i], 108, 42);

            txtBuscarAutor.Multiline = false;
            txtBuscarAutor.TextAlign = HorizontalAlignment.Left;
            txtBuscarAutor.Size = new Size(176, 34);
            txtBuscarAutor.MinimumSize = new Size(130, 30);
            txtBuscarAutor.Margin = new Padding(3, 7, 3, 3);
            ConfigurarBotonAccion(btnBuscar, "Buscar", 108, 42);
            ConfigurarBotonAccion(button10, "Limpiar búsqueda", 132, 42);
            pnlListadoTitulo.Controls.Clear();
            pnlListadoTitulo.Padding = Padding.Empty;
            pnlListadoTitulo.Controls.Add(flpAccionesResponsivo);
            pnlListadoTitulo.Controls.Add(lblListado);

            tlpCamposResponsivo = CrearTablaCampos(3);
            AgregarCampo(tlpCamposResponsivo, label7, txtIdAutor, 0);
            AgregarCampo(tlpCamposResponsivo, label14, txtNacionalidad, 1);
            AgregarCampo(tlpCamposResponsivo, label15, cboLibros, 2);
            AgregarCampo(tlpCamposResponsivo, label8, txtBiografia, 3);
            AgregarCampo(tlpCamposResponsivo, label13, txtNombreAutor, 4);
            AgregarCampo(tlpCamposResponsivo, label9, dtpFechaNacimiento, 5);

            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Margin = new Padding(2, 6, 2, 6);
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            tlpPieResponsivo = CrearPieResponsivo(button8, button9);
            flpPaginacionResponsiva = CrearContenedorPaginacion();
            tlpPieResponsivo.Controls.Add(flpPaginacionResponsiva, 1, 0);

            tlpListadoResponsivo = CrearTablaListado(152);
            tlpListadoResponsivo.Controls.Add(pnlListadoTitulo, 0, 0);
            tlpListadoResponsivo.Controls.Add(tlpCamposResponsivo, 0, 1);
            tlpListadoResponsivo.Controls.Add(dataGridView1, 0, 2);
            tlpListadoResponsivo.Controls.Add(tlpPieResponsivo, 0, 3);
            pnlListado.Controls.Clear();
            pnlListado.Controls.Add(tlpListadoResponsivo);
            pnlListado.Controls.Add(txtIdLibro);
            pnlListado.Controls.Add(label16);
            pnlListado.Resize += (s, e) => AjustarBarraAcciones();
            AjustarBarraAcciones();
            ResumeLayout(true);
        }

        private static void ConfigurarBotonAccion(Button boton, string texto, int ancho, int alto)
        {
            boton.Text = texto;
            boton.AutoSize = false;
            boton.AutoEllipsis = true;
            boton.Size = new Size(ancho, alto);
            boton.MinimumSize = new Size(96, alto);
            boton.Margin = new Padding(3);
            boton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            boton.TextAlign = ContentAlignment.MiddleCenter;
            boton.UseVisualStyleBackColor = false;
        }

        private TableLayoutPanel CrearTablaCampos(int filas)
        {
            var tabla = new TableLayoutPanel
            {
                BackColor = pnlListado.BackColor,
                ColumnCount = 4,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = new Padding(4),
                RowCount = filas
            };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26F));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26F));
            for (int fila = 0; fila < filas; fila++)
                tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / filas));
            return tabla;
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
            campo.Margin = new Padding(4, 6, 8, 6);
            tabla.Controls.Add(etiqueta, columna, fila);
            tabla.Controls.Add(campo, columna + 1, fila);
        }

        private TableLayoutPanel CrearPieResponsivo(Control botonMenu, Control botonCerrar)
        {
            var pie = new TableLayoutPanel
            {
                BackColor = pnlListado.BackColor,
                ColumnCount = 3,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                RowCount = 1
            };
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            pie.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var izquierda = new Panel { BackColor = pnlListado.BackColor, Dock = DockStyle.Fill };
            var derecha = new Panel { BackColor = pnlListado.BackColor, Dock = DockStyle.Fill };
            botonMenu.Location = Point.Empty;
            botonMenu.Dock = DockStyle.Left;
            botonCerrar.Location = Point.Empty;
            botonCerrar.Dock = DockStyle.Right;
            izquierda.Controls.Add(botonMenu);
            derecha.Controls.Add(botonCerrar);
            pie.Controls.Add(izquierda, 0, 0);
            pie.Controls.Add(derecha, 2, 0);
            return pie;
        }

        private FlowLayoutPanel CrearContenedorPaginacion()
        {
            var paginacion = new FlowLayoutPanel
            {
                Anchor = AnchorStyles.None,
                AutoSize = true,
                BackColor = pnlListado.BackColor,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            btnAnterior.Anchor = lblPagina.Anchor = btnSiguiente.Anchor = AnchorStyles.None;
            paginacion.Controls.Add(btnAnterior);
            paginacion.Controls.Add(lblPagina);
            paginacion.Controls.Add(btnSiguiente);
            return paginacion;
        }

        private TableLayoutPanel CrearTablaListado(float altoCampos)
        {
            var tabla = new TableLayoutPanel
            {
                BackColor = pnlListado.BackColor,
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                RowCount = 4
            };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 112F));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, altoCampos));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
            return tabla;
        }

        private void AjustarBarraAcciones()
        {
            if (tlpListadoResponsivo == null || flpAccionesResponsivo == null)
                return;
            int anchoDisponible = Math.Max(1, pnlListado.ClientSize.Width - pnlListado.Padding.Horizontal - 8);
            int filas = 1;
            int anchoFila = 0;
            foreach (Control accion in flpAccionesResponsivo.Controls)
            {
                int anchoAccion = accion.Width + accion.Margin.Horizontal;
                if (anchoFila > 0 && anchoFila + anchoAccion > anchoDisponible)
                {
                    filas++;
                    anchoFila = 0;
                }
                anchoFila += anchoAccion;
            }
            tlpListadoResponsivo.RowStyles[0].Height = 46F + Math.Max(1, filas) * 54F + 8F;
        }

        private void EnlazarEventos()
        {
            Load += AutoresForms_Load;
            btnBuscar.Click += btnBuscar_Click;
            button10.Click += btnLimpiarBusqueda_Click;
            button1.Click += buttonNuevo_Click;
            button2.Click += buttonLimpiar_Click;
            button7.Click += buttonLimpiar_Click;
            button3.Click += buttonEditar_Click;
            button4.Click += buttonCancelar_Click;
            button5.Click += buttonEliminar_Click;
            button6.Click += buttonGuardar_Click;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            btnAnterior.Click += btnAnterior_Click;
            btnSiguiente.Click += btnSiguiente_Click;
        }

        private void AutoresForms_Load(object sender, EventArgs e)
        {
            try
            {
                cboLibros.DisplayMember = "Titulo";
                cboLibros.ValueMember = "IDLibro";
                cboLibros.DataSource = negocio.ConsultarCatalogoLibros();
                ActualizarTotales();
                CargarAutores(string.Empty);
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void CargarAutores(string filtro)
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
            for (int i = inicio; i < fin; i++) pagina.ImportRow(resultados.Rows[i]);
            dataGridView1.DataSource = pagina;

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
            // Estos conteos son globales y no cambian al navegar entre páginas o filtrar.
            int[] totales = negocio.ConsultarTotales();
            label12.Text = totales[0].ToString();
            label3.Text = totales[1].ToString();
            label6.Text = totales[2].ToString();
        }

        private AutorEntidad LeerFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNombreAutor.Text))
                throw new ArgumentException("Escribe el nombre del autor.");
            if (cboLibros.SelectedValue == null)
                throw new ArgumentException("Selecciona un libro para el autor.");

            int idLibro;
            if (!int.TryParse(cboLibros.SelectedValue.ToString(), out idLibro))
                throw new ArgumentException("El libro seleccionado no es válido.");

            int idAutor = 0;
            int.TryParse(txtIdAutor.Text, out idAutor);
            return new AutorEntidad
            {
                IDAutor = idAutor,
                IDLibro = idLibro,
                NombreAutor = txtNombreAutor.Text.Trim(),
                Nacionalidad = string.IsNullOrWhiteSpace(txtNacionalidad.Text) ? null : txtNacionalidad.Text.Trim(),
                Biografia = txtBiografia.Text.Trim(),
                FechaNacimiento = dtpFechaNacimiento.Checked ? (DateTime?)dtpFechaNacimiento.Value : null
            };
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                string filtro = txtBuscarAutor.Text.Trim();
                if (filtro == "🔍") filtro = string.Empty;
                CargarAutores(filtro);
                int cantidad = resultados.Rows.Count;
                DialogoSistema.Mostrar(this, "Búsqueda de autores",
                    cantidad == 0 ? "No se encontraron autores con ese criterio."
                        : "Se encontraron " + cantidad + (cantidad == 1 ? " autor." : " autores."),
                    cantidad == 0 ? EstiloDialogo.Normal : EstiloDialogo.Exito);
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void btnLimpiarBusqueda_Click(object sender, EventArgs e)
        {
            bool modoEdicionAnterior = modoEdicion;
            try
            {
                if (!string.IsNullOrWhiteSpace(txtBuscarAutor.Text) &&
                    DialogoSistema.Confirmar(this, "Limpiar búsqueda", "¿Deseas borrar el texto de búsqueda?",
                        "Sí, limpiar", "Cancelar", EstiloDialogo.Normal) != DialogResult.Yes)
                    return;

                modoEdicion = true;
                txtBuscarAutor.Clear();
                CargarAutores(string.Empty);
                dataGridView1.ClearSelection();
                dataGridView1.CurrentCell = null;
            }
            catch (Exception ex) { MostrarError(ex); }
            finally { modoEdicion = modoEdicionAnterior; }
        }

        private void buttonNuevo_Click(object sender, EventArgs e)
        {
            esNuevo = true;
            modoEdicion = true;
            LimpiarFormulario();
            ConfigurarModoEdicion();
            if (cboLibros.Items.Count > 0) cboLibros.SelectedIndex = 0;
            txtNombreAutor.Focus();
        }

        private void buttonEditar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                DialogoSistema.Mostrar(this, "Editar autor", "Selecciona un autor de la lista.", EstiloDialogo.Normal);
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
                AutorEntidad autor = LeerFormulario();
                if (esNuevo) negocio.Agregar(autor);
                else
                {
                    if (DialogoSistema.Confirmar(this, "Confirmar cambios",
                        "¿Deseas guardar los cambios de “" + autor.NombreAutor +
                        "” (código " + autor.IDAutor + ")?",
                        "Sí, guardar", "Cancelar", EstiloDialogo.Normal) != DialogResult.Yes)
                        return;
                    negocio.Editar(autor);
                }

                DialogoSistema.Mostrar(this, esNuevo ? "Autor agregado" : "Cambios guardados",
                    esNuevo ? "El autor se agregó correctamente." : "Los cambios se guardaron correctamente.",
                    EstiloDialogo.Exito);
                modoEdicion = false;
                esNuevo = false;
                ConfigurarModoConsulta();
                ActualizarTotales();
                CargarAutores(FiltroActual());
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void buttonEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    DialogoSistema.Mostrar(this, "Eliminar autor", "Selecciona un autor de la lista.", EstiloDialogo.Normal);
                    return;
                }

                int idAutor = Convert.ToInt32(dataGridView1.CurrentRow.Cells["IDAutor"].Value);
                string nombreAutor = Convert.ToString(dataGridView1.CurrentRow.Cells["NombreAutor"].Value);
                DialogResult respuesta = DialogoSistema.Confirmar(this, "Confirmar eliminación",
                    "¿Confirmas eliminar al autor “" + nombreAutor + "” (código " + idAutor + ")?",
                    "Sí, eliminar", "No", EstiloDialogo.Peligro);
                if (respuesta != DialogResult.Yes) return;

                negocio.Eliminar(idAutor);
                ActualizarTotales();
                CargarAutores(FiltroActual());
                LimpiarFormulario();
                ConfigurarModoConsulta();
                DialogoSistema.Mostrar(this, "Autor eliminado", "El autor se eliminó correctamente.", EstiloDialogo.Exito);
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (!modoEdicion) CargarSeleccionEnFormulario();
        }

        private void CargarSeleccionEnFormulario()
        {
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.Cells["IDAutor"].Value == null) return;
            DataGridViewRow fila = dataGridView1.CurrentRow;
            txtIdAutor.Text = Convert.ToString(fila.Cells["IDAutor"].Value);
            txtNombreAutor.Text = Convert.ToString(fila.Cells["NombreAutor"].Value);
            txtNacionalidad.Text = fila.Cells["Nacionalidad"].Value == DBNull.Value
                ? string.Empty : Convert.ToString(fila.Cells["Nacionalidad"].Value);
            txtBiografia.Text = Convert.ToString(fila.Cells["Biografia"].Value);
            cboLibros.SelectedValue = Convert.ToInt32(fila.Cells["IDLibro"].Value);
            if (fila.Cells["FechaNacimiento"].Value == DBNull.Value)
                dtpFechaNacimiento.Checked = false;
            else
            {
                dtpFechaNacimiento.Value = Convert.ToDateTime(fila.Cells["FechaNacimiento"].Value);
                dtpFechaNacimiento.Checked = true;
            }
        }

        private void LimpiarFormulario()
        {
            txtIdAutor.Clear();
            txtIdLibro.Clear();
            txtNombreAutor.Clear();
            txtNacionalidad.Clear();
            txtBiografia.Clear();
            dtpFechaNacimiento.Checked = false;
        }

        private void buttonLimpiar_Click(object sender, EventArgs e)
        {
            bool hayDatos = !string.IsNullOrWhiteSpace(txtIdAutor.Text) ||
                !string.IsNullOrWhiteSpace(txtNombreAutor.Text) ||
                !string.IsNullOrWhiteSpace(txtNacionalidad.Text) ||
                !string.IsNullOrWhiteSpace(txtBiografia.Text) || dtpFechaNacimiento.Checked ||
                cboLibros.SelectedIndex >= 0 || !string.IsNullOrWhiteSpace(txtBuscarAutor.Text);
            if (hayDatos && DialogoSistema.Confirmar(this, "Limpiar formulario",
                "¿Deseas borrar los datos escritos en los campos?", "Sí, limpiar", "Cancelar",
                EstiloDialogo.Normal) != DialogResult.Yes)
                return;

            modoEdicion = false;
            esNuevo = false;
            ConfigurarModoConsulta();
            txtBuscarAutor.Clear();
            LimpiarFormulario();
            cboLibros.SelectedIndex = -1;
            dataGridView1.ClearSelection();
            dataGridView1.CurrentCell = null;
        }

        private void buttonCancelar_Click(object sender, EventArgs e)
        {
            bool estabaAgregando = esNuevo;
            modoEdicion = false;
            esNuevo = false;
            ConfigurarModoConsulta();
            if (estabaAgregando) LimpiarFormulario();
            else CargarSeleccionEnFormulario();
            DialogoSistema.Mostrar(this, "Operación cancelada", "No se realizaron cambios.", EstiloDialogo.Normal);
        }

        private string FiltroActual()
        {
            string filtro = txtBuscarAutor.Text.Trim();
            return filtro == "🔍" ? string.Empty : filtro;
        }

        private void ConfigurarModoConsulta()
        {
            txtIdAutor.ReadOnly = true;
            txtNombreAutor.ReadOnly = true;
            txtNacionalidad.ReadOnly = true;
            txtBiografia.ReadOnly = true;
            cboLibros.Enabled = false;
            dtpFechaNacimiento.Enabled = false;
            button6.Enabled = false;
            button4.Enabled = false;
            button1.Enabled = true;
            button3.Enabled = true;
            button5.Enabled = true;
        }

        private void ConfigurarModoEdicion()
        {
            txtIdAutor.ReadOnly = true;
            txtNombreAutor.ReadOnly = false;
            txtNacionalidad.ReadOnly = false;
            txtBiografia.ReadOnly = false;
            cboLibros.Enabled = true;
            dtpFechaNacimiento.Enabled = true;
            button6.Enabled = true;
            button4.Enabled = true;
        }

        private void btnAnterior_Click(object sender, EventArgs e)
        {
            if (paginaActual > 0) { paginaActual--; MostrarPagina(); }
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
