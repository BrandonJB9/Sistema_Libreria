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
    public partial class LibreriaForms : Form
    {
        private const int RegistrosPorPagina = int.MaxValue;
        private readonly LibroNegocio libroNegocio = new LibroNegocio();
        private readonly PedidoNegocio pedidoNegocio = new PedidoNegocio();
        private List<Libro> librosFiltrados = new List<Libro>();
        private int paginaActual;
        private bool modoEdicion;

        public LibreriaForms()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            button2.Visible = false;
            ConfigurarFormulario();
            ConfigurarLayoutResponsivoLibros();
            ConectarEventos();
            CargarPedidos();
            CargarLibros();
        }

        private void ConfigurarFormulario()
        {
            button2.Text = "🧹 Limpiar";
            button7.Text = "🧹 Limpiar";
            button6.Text = "Guardar";

            txtCodigoMedicamento.ReadOnly = true;
            txtCodigoMedicamento.ForeColor = System.Drawing.SystemColors.ControlText;
            txtCodigoPedido.ForeColor = System.Drawing.SystemColors.ControlText;
            txtCodigoPedido.DropDownStyle = ComboBoxStyle.DropDownList;
            txtTitulo.MaxLength = 100;
            txtISBN.MaxLength = 50;
            txtPrecio.MaxLength = 20;
            txtBuscarMedicamento.MaxLength = 100;
            tdpAnoPublicacion.Format = DateTimePickerFormat.Short;
            tdpAnoPublicacion.ShowCheckBox = true;
            tdpAnoPublicacion.Checked = false;

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

        private void ConfigurarLayoutResponsivoLibros()
        {
            MinimumSize = new Size(900, 650);
            AutoScroll = true;

            tlpStats.ColumnCount = 3;
            tlpStats.RowCount = 1;
            tlpStats.Dock = DockStyle.Top;
            tlpStats.Height = 150;
            tlpStats.MinimumSize = new Size(0, 150);
            tlpStats.ColumnStyles.Clear();
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
            PrepararTarjeta(panel4, label10, label11, label12);
            PrepararTarjeta(panel2, label4, label5, label6);
            PrepararTarjeta(panel1, label1, label2, label3);

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.MinimumSize = new Size(0, 120);
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.MultiSelect = false;
            dataGridView1.ScrollBars = ScrollBars.Both;
            dataGridView1.DataBindingComplete += (sender, e) =>
            {
                LayoutPantallasTablas.ConfigurarColumnasBooleanas(dataGridView1);
                foreach (DataGridViewColumn columna in dataGridView1.Columns)
                {
                    string nombre = columna.Name.ToLowerInvariant();
                    columna.FillWeight = nombre.Contains("titulo") ? 160F :
                        nombre.StartsWith("id") || nombre.Contains("estado") ? 65F : 100F;
                }
            };

            var barra = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(0),
                BackColor = pnlListadoTitulo.BackColor
            };

            lblListado.AutoSize = false;
            lblListado.Dock = DockStyle.None;
            lblListado.Size = new Size(340, 44);
            lblListado.MinimumSize = new Size(260, 44);
            lblListado.AutoEllipsis = true;
            lblListado.TextAlign = ContentAlignment.MiddleLeft;
            lblListado.Margin = new Padding(3);
            barra.Controls.Add(lblListado);

            foreach (Button boton in new[] { button1, button6, button5, button4, button3, button7 })
            {
                boton.Size = new Size(122, 40);
                boton.MinimumSize = new Size(108, 40);
                boton.Margin = new Padding(3);
                barra.Controls.Add(boton);
            }

            txtBuscarMedicamento.Size = new Size(180, 32);
            txtBuscarMedicamento.MinimumSize = new Size(145, 28);
            txtBuscarMedicamento.Margin = new Padding(3, 7, 3, 3);
            barra.Controls.Add(txtBuscarMedicamento);
            foreach (Button boton in new[] { btnBuscar, button2 })
            {
                boton.Size = new Size(122, 40);
                boton.MinimumSize = new Size(108, 40);
                boton.Margin = new Padding(3);
                barra.Controls.Add(boton);
            }

            pnlListadoTitulo.Controls.Clear();
            pnlListadoTitulo.Padding = new Padding(0);
            pnlListadoTitulo.AutoSize = true;
            pnlListadoTitulo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlListadoTitulo.MinimumSize = new Size(0, 52);
            pnlListadoTitulo.Controls.Add(barra);

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

            Control[] controles = { label7, txtCodigoMedicamento, label13, tdpAnoPublicacion,
                label15, txtCodigoPedido, label14, txtPrecio,
                label8, txtTitulo, label17, estado,
                label9, txtISBN, label16, new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent } };
            foreach (Control control in controles)
            {
                pnlListado.Controls.Remove(control);
                control.Margin = new Padding(5, 4, 5, 4);
                if (control is Label etiqueta)
                {
                    etiqueta.Dock = DockStyle.Fill;
                    etiqueta.AutoSize = false;
                    etiqueta.TextAlign = ContentAlignment.MiddleLeft;
                }
                else if (control is TextBox || control is ComboBox || control is DateTimePicker)
                    control.Dock = DockStyle.Fill;
            }
            for (int indice = 0; indice < controles.Length; indice++)
                campos.Controls.Add(controles[indice], indice % 4, indice / 4);

            var pie = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 68,
                Padding = new Padding(12, 6, 12, 6),
                BackColor = pnlListado.BackColor
            };
            button8.Dock = DockStyle.Left;
            button9.Dock = DockStyle.Right;
            pie.Controls.Add(button8);
            pie.Controls.Add(button9);

            pnlListado.Controls.Clear();
            pnlListado.Padding = new Padding(14);
            pnlListado.AutoScroll = true;
            pnlListado.AutoScrollMinSize = new Size(900, 500);
            pnlListado.Controls.Add(dataGridView1);
            pnlListado.Controls.Add(pie);
            pnlListado.Controls.Add(campos);
            pnlListado.Controls.Add(pnlListadoTitulo);

            LayoutPantallasTablas.AgregarBotonCerrarSesion(pnlTitulo);
        }

        private static void PrepararTarjeta(Panel tarjeta, Label icono, Label titulo, Label valor)
        {
            tarjeta.Dock = DockStyle.Fill;
            tarjeta.Margin = new Padding(6);
            icono.AutoSize = false;
            titulo.AutoSize = false;
            valor.AutoSize = false;
            valor.TextAlign = ContentAlignment.MiddleLeft;
            Action ajustar = () =>
            {
                int anchoTexto = Math.Max(90, tarjeta.ClientSize.Width - 104);
                icono.SetBounds(14, 18, 58, Math.Max(54, tarjeta.ClientSize.Height - 36));
                titulo.SetBounds(84, 16, anchoTexto, 30);
                valor.SetBounds(84, 48, anchoTexto, Math.Max(56, tarjeta.ClientSize.Height - 54));
                titulo.AutoEllipsis = true;
            };
            tarjeta.SizeChanged += (sender, e) => ajustar();
            ajustar();
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
        }

        private void CargarPedidos()
        {
            try
            {
                List<Pedido> pedidos = pedidoNegocio.Consultar();
                txtCodigoPedido.DataSource = null;
                txtCodigoPedido.DisplayMember = "Descripcion";
                txtCodigoPedido.ValueMember = "IDPedido";
                txtCodigoPedido.DataSource = pedidos;
                txtCodigoPedido.SelectedIndex = -1;
                txtCodigoPedido.Enabled = true;
                if (pedidos.Count == 0)
                    MessageBox.Show("No hay pedidos disponibles. No se pueden registrar libros sin seleccionar un pedido.",
                        "Pedidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                txtCodigoPedido.DataSource = null;
                txtCodigoPedido.Enabled = false;
                MessageBox.Show(ex.Message, "Pedidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CargarLibros()
        {
            try
            {
                librosFiltrados = libroNegocio.ConsultarCatalogo();
                paginaActual = 0;
                ActualizarTotales();
                MostrarPagina();
            }
            catch (Exception ex)
            {
                librosFiltrados = new List<Libro>();
                ActualizarTotales();
                MostrarPagina();
                MessageBox.Show(ex.Message, "Libros", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuscarLibros()
        {
            try
            {
                string criterio = txtBuscarMedicamento.Text.Trim();
                librosFiltrados = string.IsNullOrWhiteSpace(criterio)
                    ? libroNegocio.ConsultarCatalogo()
                    : libroNegocio.Buscar(criterio);
                paginaActual = 0;
                ActualizarTotales();
                MostrarPagina();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buscar libros", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarPagina()
        {
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(librosFiltrados.Count / (double)RegistrosPorPagina));
            if (paginaActual >= totalPaginas)
                paginaActual = totalPaginas - 1;
            if (paginaActual < 0)
                paginaActual = 0;

            dataGridView1.DataSource = librosFiltrados
                .Skip(paginaActual * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToList();
            dataGridView1.ClearSelection();

            AjustarEncabezados();
            lblListado.Text = "📖  Listado de Libros — Página " + (paginaActual + 1) + " de " + totalPaginas;
        }

        private void AjustarEncabezados()
        {
            CambiarEncabezado("IDLibro", "Código Libro");
            CambiarEncabezado("IDPedido", "Código Pedido");
            CambiarEncabezado("TipoPedido", "Tipo Pedido");
            CambiarEncabezado("AnioPublicacion", "Año Publicación");
            CambiarEncabezado("Estado", "Disponible");
            if (dataGridView1.Columns.Contains("IDPedido"))
                dataGridView1.Columns["IDPedido"].Visible = false;
            if (dataGridView1.Columns.Contains("Descripcion"))
                dataGridView1.Columns["Descripcion"].Visible = false;
        }

        private void CambiarEncabezado(string columna, string texto)
        {
            if (dataGridView1.Columns.Contains(columna))
                dataGridView1.Columns[columna].HeaderText = texto;
        }

        private void ActualizarTotales()
        {
            // Los indicadores usan todos los resultados filtrados, nunca solo la pagina visible.
            label12.Text = librosFiltrados.Count.ToString(CultureInfo.CurrentCulture);
            label6.Text = librosFiltrados.Count(libro => libro.Estado == true).ToString(CultureInfo.CurrentCulture);
            label3.Text = librosFiltrados.Count(libro => libro.Estado == false).ToString(CultureInfo.CurrentCulture);
        }

        private Libro LibroSeleccionado()
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return null;
            return dataGridView1.SelectedRows[0].DataBoundItem as Libro;
        }

        private void CargarLibroEnFormulario(Libro libro)
        {
            txtCodigoMedicamento.Text = libro.IDLibro.ToString(CultureInfo.CurrentCulture);
            txtCodigoPedido.SelectedValue = libro.IDPedido;
            txtTitulo.Text = libro.Titulo;
            txtISBN.Text = libro.ISBN;
            txtPrecio.Text = libro.Precio.ToString("0", CultureInfo.CurrentCulture);
            tdpAnoPublicacion.Checked = libro.AnioPublicacion.HasValue;
            if (libro.AnioPublicacion.HasValue)
                tdpAnoPublicacion.Value = libro.AnioPublicacion.Value;
            rdbActivo.Checked = libro.Estado == true;
            rdbInactivo.Checked = libro.Estado == false;
        }

        private Libro LeerLibroDesdeFormulario()
        {
            int idLibro;
            int.TryParse(txtCodigoMedicamento.Text, out idLibro);

            int idPedido;
            if (txtCodigoPedido.SelectedValue == null ||
                !int.TryParse(Convert.ToString(txtCodigoPedido.SelectedValue, CultureInfo.CurrentCulture), out idPedido) || idPedido <= 0)
                throw new ArgumentException("Selecciona un pedido disponible.");
            decimal precio;
            if (!decimal.TryParse(txtPrecio.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out precio))
                throw new ArgumentException("Escribe un precio numerico valido.");

            bool? estado = rdbActivo.Checked ? (bool?)true : rdbInactivo.Checked ? (bool?)false : null;
            return new Libro
            {
                IDLibro = idLibro,
                IDPedido = idPedido,
                Titulo = txtTitulo.Text,
                ISBN = txtISBN.Text,
                AnioPublicacion = tdpAnoPublicacion.Checked ? (DateTime?)tdpAnoPublicacion.Value : null,
                Precio = precio,
                Estado = estado
            };
        }

        private void LimpiarFormulario()
        {
            modoEdicion = false;
            button6.Text = "Guardar";
            txtCodigoMedicamento.Clear();
            txtCodigoPedido.SelectedIndex = -1;
            txtTitulo.Clear();
            txtISBN.Clear();
            txtPrecio.Clear();
            tdpAnoPublicacion.Checked = false;
            rdbActivo.Checked = false;
            rdbInactivo.Checked = false;
            dataGridView1.ClearSelection();
        }

        private void buttonNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            txtTitulo.Focus();
        }

        private void buttonLimpiarCampos_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void buttonLimpiarBusqueda_Click(object sender, EventArgs e)
        {
            txtBuscarMedicamento.Clear();
            CargarLibros();
            txtBuscarMedicamento.Focus();
        }

        private void buttonBuscar_Click(object sender, EventArgs e)
        {
            BuscarLibros();
        }

        private void buttonEditar_Click(object sender, EventArgs e)
        {
            Libro libro = LibroSeleccionado();
            if (libro == null)
            {
                MessageBox.Show("Selecciona una fila del listado y pulsa Editar.", "Editar libro",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CargarLibroEnFormulario(libro);
            modoEdicion = true;
            button6.Text = "Actualizar";
            txtTitulo.Focus();
        }

        private void buttonCancelar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void buttonGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                Libro libro = LeerLibroDesdeFormulario();
                if (modoEdicion)
                {
                    if (!libroNegocio.Editar(libro))
                        throw new InvalidOperationException("No se encontro el libro que deseas editar.");
                    MessageBox.Show("Libro actualizado.", "Libros", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    libroNegocio.Agregar(libro);
                    MessageBox.Show("Libro agregado.", "Libros", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                CargarLibros();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Guardar libro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonEliminar_Click(object sender, EventArgs e)
        {
            Libro libro = LibroSeleccionado();
            if (libro == null)
            {
                MessageBox.Show("Selecciona una fila del listado para eliminar.", "Eliminar libro",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("¿Deseas eliminar el libro seleccionado?", "Confirmar eliminacion",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                if (libroNegocio.Eliminar(libro.IDLibro))
                {
                    CargarLibros();
                    LimpiarFormulario();
                    MessageBox.Show("Libro eliminado.", "Libros", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No se encontro el libro.", "Libros", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Eliminar libro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
