using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    public partial class EditorialesForms : Form
    {
        private const int RegistrosPorPagina = 10;
        private readonly EditorialNegocio editorialNegocio = new EditorialNegocio();
        private readonly LibroNegocio libroNegocio = new LibroNegocio();
        private readonly Button btnPaginaAnterior = new Button();
        private readonly Button btnPaginaSiguiente = new Button();
        private List<Editorial> editorialesFiltradas = new List<Editorial>();
        private int paginaActual;
        private bool modoEdicion;

        public EditorialesForms()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            button2.Visible = false;
            ConfigurarFormulario();
            ConfigurarBarraAcciones();
            ConfigurarCerrarSesion();
            ConfigurarAnclajesListado();
            ConectarEventos();
            CargarLibros();
            CargarEditoriales();
        }

        private void ConfigurarFormulario()
        {
            // Se reutilizan los botones y espacios existentes para mantener el diseño.
            button2.Text = "🧹 Limpiar";
            button2.BackColor = System.Drawing.Color.Goldenrod;
            button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGoldenrod;
            button4.Text = "❌ Cancelar";
            button7.Text = "🧹 Limpiar";
            label2.Text = "Inactivas";
            label5.Text = "Activas";
            label11.Text = "Total de Editoriales";

            txtCodigoEditorial.ReadOnly = true;
            txtCodigoLibro.ForeColor = System.Drawing.SystemColors.ControlText;
            txtCodigoLibro.DropDownStyle = ComboBoxStyle.DropDownList;
            txtNombreEditorial.MaxLength = 100;
            txtPais.MaxLength = 50;
            txtDireccion.MaxLength = 100;

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ScrollBars = ScrollBars.Both;
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

            MinimumSize = new System.Drawing.Size(900, 650);
            AutoScroll = true;
            tlpStats.ColumnCount = 3;
            tlpStats.RowCount = 1;
            tlpStats.Dock = DockStyle.Top;
            tlpStats.Height = 132;
            tlpStats.MinimumSize = new System.Drawing.Size(0, 132);
            tlpStats.ColumnStyles.Clear();
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));

            PrepararTarjeta(panel4, label10, label11, label12);
            PrepararTarjeta(panel2, label4, label5, label6);
            PrepararTarjeta(panel1, label1, label2, label3);
        }

        private static void PrepararTarjeta(Panel tarjeta, Label icono, Label titulo, Label valor)
        {
            tarjeta.Dock = DockStyle.Fill;
            tarjeta.Margin = new Padding(6);
            Action ajustar = () =>
            {
                int anchoTexto = Math.Max(90, tarjeta.ClientSize.Width - 104);
                icono.SetBounds(14, 18, 58, Math.Max(54, tarjeta.ClientSize.Height - 36));
                titulo.SetBounds(84, 18, anchoTexto, 28);
                valor.SetBounds(84, 50, anchoTexto, Math.Max(38, tarjeta.ClientSize.Height - 58));
                titulo.AutoEllipsis = true;
            };
            tarjeta.SizeChanged += (sender, e) => ajustar();
            ajustar();
        }

        private void ConfigurarBarraAcciones()
        {
            var barra = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = false,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(0),
                BackColor = pnlListadoTitulo.BackColor
            };

            Button[] botones = { button1, button6, button5, button4, button3, button7 };
            pnlListadoTitulo.Controls.Remove(lblListado);
            foreach (Button boton in botones)
                pnlListadoTitulo.Controls.Remove(boton);
            pnlListadoTitulo.Controls.Remove(txtBuscarEditorial);
            pnlListadoTitulo.Controls.Remove(btnBuscar);
            pnlListadoTitulo.Controls.Remove(button2);

            lblListado.AutoSize = false;
            lblListado.Dock = DockStyle.None;
            lblListado.MinimumSize = new System.Drawing.Size(420, 44);
            lblListado.Size = new System.Drawing.Size(460, 44);
            lblListado.Margin = new Padding(3);
            lblListado.AutoEllipsis = false;
            lblListado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            barra.Controls.Add(lblListado);

            foreach (Button boton in botones)
            {
                boton.Size = new System.Drawing.Size(132, 40);
                boton.MinimumSize = new System.Drawing.Size(112, 40);
                boton.Margin = new Padding(3);
                barra.Controls.Add(boton);
            }

            txtBuscarEditorial.Size = new System.Drawing.Size(180, 32);
            txtBuscarEditorial.MinimumSize = new System.Drawing.Size(145, 28);
            txtBuscarEditorial.Margin = new Padding(3, 7, 3, 3);
            barra.Controls.Add(txtBuscarEditorial);
            btnBuscar.Size = new System.Drawing.Size(132, 40);
            btnBuscar.MinimumSize = new System.Drawing.Size(112, 40);
            btnBuscar.Margin = new Padding(3);
            barra.Controls.Add(btnBuscar);
            button2.Size = new System.Drawing.Size(132, 40);
            button2.MinimumSize = new System.Drawing.Size(112, 40);
            button2.Margin = new Padding(3);
            barra.Controls.Add(button2);

            pnlListadoTitulo.Controls.Clear();
            pnlListadoTitulo.Padding = new Padding(0);
            pnlListadoTitulo.AutoSize = true;
            pnlListadoTitulo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlListadoTitulo.MinimumSize = new System.Drawing.Size(0, 52);
            pnlListadoTitulo.Controls.Add(barra);
        }

        private void ConfigurarCerrarSesion()
        {
            LayoutPantallasTablas.AgregarBotonCerrarSesion(pnlTitulo);
        }

        private void ConfigurarAnclajesListado()
        {
            var campos = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(12, 8, 12, 8),
                Margin = new Padding(0),
                BackColor = pnlListado.BackColor
            };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17F));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            for (int fila = 0; fila < 4; fila++)
                campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

            var estado = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(3),
                Padding = new Padding(4, 6, 0, 0),
                BackColor = Color.Transparent
            };
            foreach (Control control in new Control[] { rdbActivo, rdbInactivo })
            {
                pnlListado.Controls.Remove(control);
                control.Margin = new Padding(4, 3, 12, 3);
                estado.Controls.Add(control);
            }

            Control[] camposCaptura = { label7, txtCodigoEditorial, label13, dtpAnoPublicacion,
                label15, txtCodigoLibro, label14, txtDireccion,
                label8, txtNombreEditorial, label17, estado,
                label9, txtPais, label16, new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent } };
            foreach (Control control in camposCaptura)
            {
                pnlListado.Controls.Remove(control);
                control.Margin = new Padding(5, 4, 5, 4);
                if (control is Label)
                {
                    control.Dock = DockStyle.Fill;
                    ((Label)control).AutoSize = false;
                    ((Label)control).TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                }
                else if (control is TextBox || control is ComboBox || control is DateTimePicker)
                    control.Dock = DockStyle.Fill;
            }
            for (int i = 0; i < camposCaptura.Length; i++)
                campos.Controls.Add(camposCaptura[i], i % 4, i / 4);

            var inferior = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 68,
                Padding = new Padding(12, 6, 12, 6),
                BackColor = pnlListado.BackColor
            };
            button8.Dock = DockStyle.Left;
            button9.Dock = DockStyle.Right;
            button8.Margin = new Padding(3);
            button9.Margin = new Padding(3);
            inferior.Controls.Add(button8);
            inferior.Controls.Add(button9);
            ConfigurarBotonesPaginacion(inferior);

            pnlListado.Controls.Clear();
            pnlListado.Padding = new Padding(14);
            pnlListado.AutoScroll = true;
            pnlListado.AutoScrollMinSize = new System.Drawing.Size(900, 500);
            pnlListado.Controls.Add(dataGridView1);
            pnlListado.Controls.Add(inferior);
            pnlListado.Controls.Add(campos);
            pnlListado.Controls.Add(pnlListadoTitulo);
            pnlListadoTitulo.Dock = DockStyle.Top;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.MinimumSize = new System.Drawing.Size(0, 120);
        }

        private void ConectarEventos()
        {
            Load += EditorialesForms_Load;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            btnBuscar.Click += btnBuscar_Click;
            button3.Click += button3_Click;
            button4.Click += button4_Click;
            button5.Click += button5_Click;
            button6.Click += button6_Click;
            button7.Click += button7_Click;
        }

        private void ConfigurarBotonesPaginacion(Panel inferior)
        {
            // La paginación usa controles propios para conservar todos los botones originales.
            ConfigurarBotonPaginacion(btnPaginaAnterior, "◀ Anterior");
            ConfigurarBotonPaginacion(btnPaginaSiguiente, "Siguiente ▶");
            btnPaginaAnterior.Click += (sender, e) => { paginaActual--; MostrarPagina(); };
            btnPaginaSiguiente.Click += (sender, e) => { paginaActual++; MostrarPagina(); };
            inferior.Controls.Add(btnPaginaAnterior);
            inferior.Controls.Add(btnPaginaSiguiente);
        }

        private void ConfigurarBotonPaginacion(Button boton, string texto)
        {
            boton.BackColor = System.Drawing.Color.FromArgb(16, 179, 199);
            boton.Cursor = Cursors.Hand;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            boton.ForeColor = System.Drawing.Color.White;
            boton.Dock = DockStyle.Right;
            boton.Size = new System.Drawing.Size(141, 50);
            boton.Text = texto;
            boton.UseVisualStyleBackColor = false;
        }

        private void EditorialesForms_Load(object sender, EventArgs e)
        {
            MostrarPagina();
        }

        private void CargarLibros()
        {
            try
            {
                List<Libro> libros = libroNegocio.Consultar();
                txtCodigoLibro.DataSource = null;
                txtCodigoLibro.DisplayMember = "Descripcion";
                txtCodigoLibro.ValueMember = "IDLibro";
                txtCodigoLibro.DataSource = libros;
                txtCodigoLibro.SelectedIndex = -1;
                txtCodigoLibro.Enabled = true;
                if (libros.Count == 0)
                    MessageBox.Show("No hay libros disponibles. No se pueden registrar editoriales sin seleccionar un libro.",
                        "Libros", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                txtCodigoLibro.DataSource = null;
                txtCodigoLibro.Enabled = false;
                MessageBox.Show(ex.Message, "Libros", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CargarEditoriales()
        {
            try
            {
                editorialesFiltradas = editorialNegocio.Consultar();
                paginaActual = 0;
                ActualizarTotales();
                MostrarPagina();
            }
            catch (Exception ex)
            {
                editorialesFiltradas = new List<Editorial>();
                ActualizarTotales();
                MostrarPagina();
                MessageBox.Show(ex.Message, "Editoriales", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarPagina()
        {
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(editorialesFiltradas.Count / (double)RegistrosPorPagina));
            if (paginaActual >= totalPaginas)
                paginaActual = totalPaginas - 1;
            if (paginaActual < 0)
                paginaActual = 0;

            dataGridView1.DataSource = editorialesFiltradas
                .Skip(paginaActual * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToList();
            LayoutPantallasTablas.ConfigurarColumnasBooleanas(dataGridView1);
            dataGridView1.ClearSelection();

            AjustarEncabezados();
            AjustarPesosColumnas();
            lblListado.Text = "🏢 Listado de Editoriales — Página " + (paginaActual + 1) + " de " + totalPaginas;
            btnPaginaAnterior.Enabled = paginaActual > 0;
            btnPaginaSiguiente.Enabled = paginaActual + 1 < totalPaginas;
        }

        private void AjustarEncabezados()
        {
            CambiarEncabezado("IDEditorial", "Código Editorial");
            CambiarEncabezado("IDLibro", "Código Libro");
            CambiarEncabezado("NombreEditorial", "Nombre Editorial");
            CambiarEncabezado("Pais", "País");
            CambiarEncabezado("Direccion", "Dirección");
            CambiarEncabezado("Estado", "Activo");
            CambiarEncabezado("TituloLibro", "Libro");
            if (dataGridView1.Columns.Contains("IDLibro"))
                dataGridView1.Columns["IDLibro"].Visible = false;
        }

        private void CambiarEncabezado(string columna, string texto)
        {
            if (dataGridView1.Columns.Contains(columna))
                dataGridView1.Columns[columna].HeaderText = texto;
        }

        private void AjustarPesosColumnas()
        {
            AsignarPeso("IDEditorial", 65F);
            AsignarPeso("IDLibro", 65F);
            AsignarPeso("NombreEditorial", 130F);
            AsignarPeso("Pais", 80F);
            AsignarPeso("Direccion", 155F);
            AsignarPeso("Estado", 65F);
            AsignarPeso("TituloLibro", 125F);
        }

        private void AsignarPeso(string columna, float peso)
        {
            if (dataGridView1.Columns.Contains(columna))
                dataGridView1.Columns[columna].FillWeight = peso;
        }

        private void ActualizarTotales()
        {
            // Los totales usan todos los resultados filtrados, no solo la página visible.
            label3.Text = editorialesFiltradas.Count(e => !e.Estado).ToString();
            label6.Text = editorialesFiltradas.Count(e => e.Estado).ToString();
            label12.Text = editorialesFiltradas.Count.ToString();
        }

        private Editorial EditorialDesdeFormulario()
        {
            int idLibro;
            if (txtCodigoLibro.SelectedValue == null ||
                !int.TryParse(Convert.ToString(txtCodigoLibro.SelectedValue), out idLibro) || idLibro <= 0)
                throw new ArgumentException("Escribe o selecciona un código de libro válido.");
            if (!rdbActivo.Checked && !rdbInactivo.Checked)
                throw new ArgumentException("Selecciona si la editorial estará activa o inactiva.");

            int idEditorial = 0;
            if (!string.IsNullOrWhiteSpace(txtCodigoEditorial.Text) &&
                !int.TryParse(txtCodigoEditorial.Text, out idEditorial))
                throw new ArgumentException("El código de editorial no es válido.");

            return new Editorial
            {
                IDEditorial = idEditorial,
                IDLibro = idLibro,
                NombreEditorial = txtNombreEditorial.Text,
                Pais = txtPais.Text,
                Direccion = txtDireccion.Text,
                Estado = rdbActivo.Checked
            };
        }

        private static int ObtenerCodigoLibro(string texto)
        {
            string codigo = (texto ?? string.Empty).Trim();
            int separador = codigo.IndexOf(" - ", StringComparison.Ordinal);
            if (separador >= 0)
                codigo = codigo.Substring(0, separador).Trim();

            int idLibro;
            return int.TryParse(codigo, out idLibro) ? idLibro : 0;
        }

        private Editorial EditorialSeleccionada()
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return null;
            return dataGridView1.SelectedRows[0].DataBoundItem as Editorial;
        }

        private void SeleccionarEditorial(Editorial editorial)
        {
            if (editorial == null)
                return;

            txtCodigoEditorial.Text = editorial.IDEditorial.ToString();
            txtCodigoLibro.SelectedValue = editorial.IDLibro;
            txtNombreEditorial.Text = editorial.NombreEditorial;
            txtPais.Text = editorial.Pais;
            txtDireccion.Text = editorial.Direccion;
            rdbActivo.Checked = editorial.Estado;
            rdbInactivo.Checked = !editorial.Estado;
        }

        private void LimpiarFormulario()
        {
            modoEdicion = false;
            button6.Text = "Guardar";
            txtCodigoEditorial.Clear();
            txtCodigoLibro.SelectedIndex = -1;
            txtNombreEditorial.Clear();
            txtPais.Clear();
            txtDireccion.Clear();
            rdbActivo.Checked = false;
            rdbInactivo.Checked = false;
            dataGridView1.ClearSelection();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            txtNombreEditorial.Focus();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            txtNombreEditorial.Focus();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                string criterio = txtBuscarEditorial.Text.Trim();
                editorialesFiltradas = string.IsNullOrEmpty(criterio)
                    ? editorialNegocio.Consultar()
                    : editorialNegocio.Buscar(criterio);
                paginaActual = 0;
                ActualizarTotales();
                MostrarPagina();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buscar editoriales", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Editorial editorial = EditorialSeleccionada();
            if (editorial == null)
            {
                MessageBox.Show("Selecciona una fila del listado y pulsa Editar.", "Editar editorial",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SeleccionarEditorial(editorial);
            modoEdicion = true;
            button6.Text = "Actualizar";
            txtNombreEditorial.Focus();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Editorial editorial = EditorialSeleccionada();
            int idEditorial = editorial == null ? 0 : editorial.IDEditorial;
            if (idEditorial == 0 && !int.TryParse(txtCodigoEditorial.Text, out idEditorial))
            {
                MessageBox.Show("Selecciona una editorial para eliminar.", "Editoriales",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (editorial != null)
                idEditorial = editorial.IDEditorial;

            if (MessageBox.Show("¿Deseas eliminar la editorial seleccionada?", "Confirmar eliminación",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                if (editorialNegocio.Eliminar(idEditorial))
                {
                    CargarEditoriales();
                    LimpiarFormulario();
                    MessageBox.Show("Editorial eliminada.", "Editoriales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No se encontró la editorial.", "Editoriales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Eliminar editorial", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                Editorial editorial = EditorialDesdeFormulario();
                if (!modoEdicion)
                {
                    editorialNegocio.Agregar(editorial);
                    MessageBox.Show("Editorial agregada.", "Editoriales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (!editorialNegocio.Editar(editorial))
                        throw new InvalidOperationException("No se encontró la editorial que deseas editar.");
                    MessageBox.Show("Editorial actualizada.", "Editoriales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                CargarEditoriales();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Guardar editorial", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            txtBuscarEditorial.Clear();
            CargarEditoriales();
            txtBuscarEditorial.Focus();
        }

    }

    /// <summary>Comparte el mismo acomodo visual en las pantallas de tablas restantes.</summary>
    internal static class LayoutPantallasTablas
    {
        public static void Configurar(
            Form formulario,
            Panel encabezado,
            Panel barraTitulo,
            Label tituloListado,
            TableLayoutPanel resumen,
            Panel tarjetaTotal,
            Panel tarjetaActivas,
            Panel tarjetaInactivas,
            DataGridView tabla,
            Button botonMenu,
            Button botonCerrar,
            Button[] acciones,
            TextBox busqueda,
            Button botonBuscar,
            Button botonLimpiarBusqueda,
            string textoBotonEncabezado = null,
            EventHandler clickBotonEncabezado = null)
        {
            formulario.MinimumSize = new System.Drawing.Size(1500, 900);

            resumen.ColumnCount = 3;
            resumen.RowCount = 1;
            resumen.Dock = DockStyle.Top;
            resumen.Height = 116;
            resumen.ColumnStyles.Clear();
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
            PrepararTarjeta(tarjetaTotal);
            PrepararTarjeta(tarjetaActivas);
            PrepararTarjeta(tarjetaInactivas);

            tabla.ReadOnly = true;
            tabla.AllowUserToAddRows = false;
            tabla.RowHeadersVisible = false;
            tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tabla.ScrollBars = ScrollBars.Both;
            tabla.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabla.AutoGenerateColumns = true;
            tabla.BackgroundColor = System.Drawing.Color.FromArgb(10, 21, 37);
            tabla.GridColor = System.Drawing.Color.FromArgb(35, 70, 96);
            tabla.EnableHeadersVisualStyles = false;
            tabla.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(16, 179, 199);
            tabla.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            tabla.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(18, 45, 75);
            tabla.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
            tabla.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(11, 127, 149);
            tabla.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            tabla.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(14, 29, 49);
            tabla.AlternatingRowsDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            tabla.DataBindingComplete += (sender, e) =>
            {
                ConfigurarColumnasBooleanas(tabla);
                AjustarPesoColumnas(tabla);
            };
            ConfigurarColumnasBooleanas(tabla);
            AjustarPesoColumnas(tabla);

            var barra = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(12, 0, 12, 0),
                Margin = new Padding(0),
                BackColor = barraTitulo.BackColor
            };

            barraTitulo.Controls.Clear();
            barraTitulo.Padding = new Padding(0);
            barraTitulo.Height = 54;
            tituloListado.AutoSize = false;
            tituloListado.Dock = DockStyle.None;
            tituloListado.Size = new System.Drawing.Size(340, 38);
            tituloListado.Margin = new Padding(3);
            tituloListado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            barra.Controls.Add(tituloListado);

            foreach (Button accion in acciones)
            {
                accion.Size = new System.Drawing.Size(142, 38);
                accion.Margin = new Padding(3);
                barra.Controls.Add(accion);
            }
            busqueda.Size = new System.Drawing.Size(240, 32);
            busqueda.Margin = new Padding(3, 6, 3, 3);
            barra.Controls.Add(busqueda);
            botonBuscar.Size = new System.Drawing.Size(142, 38);
            botonBuscar.Margin = new Padding(3);
            barra.Controls.Add(botonBuscar);
            botonLimpiarBusqueda.Size = new System.Drawing.Size(142, 38);
            botonLimpiarBusqueda.Margin = new Padding(3);
            barra.Controls.Add(botonLimpiarBusqueda);
            barraTitulo.Controls.Add(barra);

            Action ajustarBarra = () =>
            {
                int cantidadBotones = acciones.Length + 2;
                int anchoUtil = barra.ClientSize.Width - barra.Padding.Horizontal - (acciones.Length + 4) * 6;
                int anchoTitulo = Math.Min(360, Math.Max(280, (int)(anchoUtil * 0.24)));
                int anchoBusqueda = Math.Min(190, Math.Max(145, (int)(anchoUtil * 0.12)));
                int anchoBoton = Math.Max(90,
                    (anchoUtil - anchoTitulo - anchoBusqueda) / cantidadBotones);

                tituloListado.Width = anchoTitulo;
                foreach (Button accion in acciones)
                    accion.Width = anchoBoton;
                busqueda.Width = anchoBusqueda;
                botonBuscar.Width = anchoBoton;
                botonLimpiarBusqueda.Width = anchoBoton;
            };
            barra.SizeChanged += (sender, e) => ajustarBarra();
            ajustarBarra();

            botonMenu.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            botonCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Control listado = tabla.Parent;
            Action ajustarListado = () =>
            {
                const int margen = 23;
                const int espacio = 12;
                int menuY = listado.ClientSize.Height - botonMenu.Height - 13;
                int cerrarY = listado.ClientSize.Height - botonCerrar.Height - 13;
                botonMenu.Location = new System.Drawing.Point(margen, menuY);
                botonCerrar.Location = new System.Drawing.Point(
                    listado.ClientSize.Width - botonCerrar.Width - margen, cerrarY);
                int inicioBotones = Math.Min(menuY, cerrarY);
                foreach (Control control in listado.Controls)
                {
                    if (control.Name == "btnPaginaAnterior" || control.Name == "btnPaginaSiguiente")
                    {
                        control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                        control.Top = listado.ClientSize.Height - control.Height - 13;
                        inicioBotones = Math.Min(inicioBotones, control.Top);
                    }
                }
                tabla.Location = new System.Drawing.Point(margen, 414);
                tabla.Size = new System.Drawing.Size(
                    Math.Max(100, listado.ClientSize.Width - margen * 2),
                    Math.Max(100, inicioBotones - tabla.Top - espacio));
            };
            listado.Resize += (sender, e) => ajustarListado();
            ajustarListado();

            AgregarBotonCerrarSesion(encabezado, textoBotonEncabezado, clickBotonEncabezado);
        }

        public static void AgregarBotonCerrarSesion(
            Control encabezado, string texto = null, EventHandler accion = null)
        {
            var botonSesion = new Button
            {
                Name = "btnCerrarSesion",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = System.Drawing.Color.FromArgb(7, 18, 32),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Regular),
                Location = new System.Drawing.Point(encabezado.ClientSize.Width - 275, 17),
                Size = new System.Drawing.Size(255, 50),
                Text = texto ?? "\u23FB  Cerrar sesi\u00F3n",
                UseVisualStyleBackColor = false
            };
            botonSesion.FlatAppearance.BorderSize = 0;
            botonSesion.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(16, 35, 57);
            botonSesion.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(22, 43, 67);
            botonSesion.Cursor = Cursors.Hand;
            if (accion != null)
                botonSesion.Click += accion;
            encabezado.Controls.Add(botonSesion);
        }
        private static void PrepararTarjeta(Panel tarjeta)
        {
            tarjeta.Dock = DockStyle.Fill;
            tarjeta.Margin = new Padding(8);
        }

        private static void AjustarPesoColumnas(DataGridView tabla)
        {
            foreach (DataGridViewColumn columna in tabla.Columns)
            {
                string nombre = columna.Name.ToLowerInvariant();
                if (nombre.Contains("nombre") || nombre.Contains("direccion") ||
                    nombre.Contains("titulo") || nombre.Contains("observacion") ||
                    nombre.Contains("comprobante"))
                    columna.FillWeight = 160F;
                else if (nombre.StartsWith("id") || nombre.Contains("codigo") ||
                    nombre.Contains("estado") || nombre.Contains("activo"))
                    columna.FillWeight = 65F;
                else
                    columna.FillWeight = 100F;
            }
        }

        public static void ConfigurarColumnasBooleanas(DataGridView tabla)
        {
            for (int indice = 0; indice < tabla.Columns.Count; indice++)
            {
                DataGridViewColumn columna = tabla.Columns[indice];
                Type tipo = columna.ValueType;
                bool esBooleano = tipo == typeof(bool) || Nullable.GetUnderlyingType(tipo) == typeof(bool);
                if (!esBooleano && columna.DataPropertyName != "Estado")
                    continue;
                if (columna is DataGridViewCheckBoxColumn)
                    continue;

                int displayIndex = columna.DisplayIndex;
                var casilla = new DataGridViewCheckBoxColumn
                {
                    Name = columna.Name,
                    DataPropertyName = columna.DataPropertyName,
                    HeaderText = columna.HeaderText,
                    FillWeight = columna.FillWeight,
                    Visible = columna.Visible,
                    ReadOnly = true,
                    ThreeState = true,
                    TrueValue = true,
                    FalseValue = false,
                    IndeterminateValue = null,
                    ValueType = typeof(bool),
                    SortMode = DataGridViewColumnSortMode.Automatic
                };
                casilla.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                tabla.Columns.RemoveAt(indice);
                tabla.Columns.Insert(indice, casilla);
                casilla.DisplayIndex = displayIndex;
            }
        }
    }

    /// <summary>Presenta los avisos del sistema con el tema visual de la aplicación.</summary>
    internal static class MessageBox
    {
        public static DialogResult Show(string texto, string titulo, MessageBoxButtons botones, MessageBoxIcon icono)
        {
            return MensajesForms.Mostrar(Form.ActiveForm, texto, titulo, botones, icono);
        }
    }
}
