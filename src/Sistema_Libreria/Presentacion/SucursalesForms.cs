using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entidad;
using Negocio;

namespace Presentacion
{
    public partial class SucursalesForms : Form
    {
        private const int RegistrosPorPagina = 10;
        private readonly SucursalesNegocio sucursalesNegocio = new SucursalesNegocio();
        private List<Sucursal> todasLasSucursales = new List<Sucursal>();
        private List<Sucursal> sucursalesFiltradas = new List<Sucursal>();
        private int paginaActual;
        private bool modoAgregarSucursal;
        private bool modoEditarSucursal;
        private int? idSucursalSeleccionada;

        public SucursalesForms()
        {
            InitializeComponent();
            LogoEncabezado.Aplicar(lblTituloIcono);
            btnTienda.Visible = false;
            btnLimpiarBusquedaSucursal.Visible = false;

            // Las tarjetas de este formulario resumen sucursales por su Estado.
            label5.Text = "Sucursales activas";
            label2.Text = "Sucursales inactivas";

            // La tarjeta de Departamentos conserva el diseño y usa un acento celeste.
            panel3.BackColor = Color.FromArgb(25, 62, 83);
            label19.BackColor = Color.FromArgb(29, 105, 135);
            label20.ForeColor = Color.FromArgb(91, 207, 238);

            ConfigurarLayoutResponsivoSucursales();

            // Los eventos se conectan aquí para conservar intacto el archivo Designer.
            Load += SucursalesForms_Load;
            btnBuscarSucursal.Click += btnBuscarSucursal_Click;
            btnLimpiarBusquedaSucursal.Click += btnLimpiarBusquedaSucursal_Click;
            btnAgregarSucursal.Click += btnAgregarSucursal_Click;
            btnGuardarSucursal.Click += btnGuardarSucursal_Click;
            btnLimpiarFormularioSucursal.Click += btnLimpiarFormularioSucursal_Click;
            btnEditarSucursal.Click += btnEditarSucursal_Click;
            btnEliminarSucursal.Click += btnEliminarSucursal_Click;
            btnCancelarSucursal.Click += btnCancelarSucursal_Click;
            txtBuscarSucursal.KeyDown += txtBuscarSucursal_KeyDown;
            dgvTbl_Sucursales.CellClick += dgvTbl_Sucursales_CellClick;
            dgvTbl_Sucursales.KeyDown += dgvTbl_Sucursales_KeyDown;
        }

        private void ConfigurarLayoutResponsivoSucursales()
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

            dgvTbl_Sucursales.Dock = DockStyle.Fill;
            dgvTbl_Sucursales.MinimumSize = new Size(0, 120);
            dgvTbl_Sucursales.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

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

            foreach (Button boton in new[] { btnAgregarSucursal, btnGuardarSucursal, btnEliminarSucursal,
                btnCancelarSucursal, btnEditarSucursal, btnLimpiarFormularioSucursal })
            {
                boton.Size = new Size(122, 40);
                boton.MinimumSize = new Size(108, 40);
                boton.Margin = new Padding(3);
                barra.Controls.Add(boton);
            }

            txtBuscarSucursal.Size = new Size(180, 32);
            txtBuscarSucursal.MinimumSize = new Size(145, 28);
            txtBuscarSucursal.Margin = new Padding(3, 7, 3, 3);
            barra.Controls.Add(txtBuscarSucursal);
            foreach (Button boton in new[] { btnBuscarSucursal, btnLimpiarBusquedaSucursal })
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
            foreach (Control control in new Control[] { rdbEstadoActivo, rdbEstadoInactivo })
            {
                pnlListado.Controls.Remove(control);
                control.Margin = new Padding(4, 3, 12, 3);
                estado.Controls.Add(control);
            }

            Control[] controles = { label7, txtIDCodigoSucursal, label14, txtDireccion,
                label15, txtNombreSucursal, label17, txtTelefono,
                label8, txtDepartamento, label13, dtpHorarioAtencion,
                label9, txtMunicipio, label18, estado };
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
                else if (control is TextBox || control is DateTimePicker)
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
            btnMenuPrincipal.Dock = DockStyle.Left;
            pie.Controls.Add(btnMenuPrincipal);

            pnlListado.Controls.Clear();
            pnlListado.Padding = new Padding(14);
            pnlListado.AutoScroll = true;
            pnlListado.AutoScrollMinSize = new Size(900, 500);
            pnlListado.Controls.Add(dgvTbl_Sucursales);
            pnlListado.Controls.Add(pie);
            pnlListado.Controls.Add(campos);
            pnlListado.Controls.Add(pnlListadoTitulo);
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

        private void SucursalesForms_Load(object sender, System.EventArgs e)
        {
            ConsultarSucursales();
        }

        private void ConsultarSucursales()
        {
            try
            {
                todasLasSucursales = sucursalesNegocio.ConsultarSucursales();
                sucursalesFiltradas = new List<Sucursal>(todasLasSucursales);

                // Los tres contadores salen de la lista completa, no de la página visible.
                ActualizarContadores();
                paginaActual = 0;
                MostrarPaginaActual();
            }
            catch (System.Exception ex)
            {
                MostrarMensaje(
                    "No se pudieron consultar las sucursales. " + ex.Message,
                    "Consulta de sucursales",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void MostrarPaginaActual()
        {
            // La lista completa permanece en Presentación; solo se enlazan hasta 10 filas.
            List<Sucursal> filasPagina = sucursalesFiltradas
                .Skip(paginaActual * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToList();

            dgvTbl_Sucursales.ReadOnly = true;
            dgvTbl_Sucursales.AllowUserToAddRows = false;
            dgvTbl_Sucursales.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTbl_Sucursales.MultiSelect = false;
            dgvTbl_Sucursales.AutoGenerateColumns = true;
            idSucursalSeleccionada = null;
            dgvTbl_Sucursales.DataSource = filasPagina;

            ConfigurarEstiloGrilla();

            if (dgvTbl_Sucursales.Columns["Estado"] != null)
            {
                dgvTbl_Sucursales.Columns["Estado"].HeaderText = "Estado";
            }
        }

        private void ConfigurarEstiloGrilla()
        {
            // Las columnas se reparten por todo el ancho disponible de la grilla.
            dgvTbl_Sucursales.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTbl_Sucursales.RowHeadersWidth = 42;
            dgvTbl_Sucursales.ColumnHeadersHeight = 38;
            dgvTbl_Sucursales.RowTemplate.Height = 34;
            dgvTbl_Sucursales.EnableHeadersVisualStyles = false;
            dgvTbl_Sucursales.BackgroundColor = Color.FromArgb(18, 45, 75);
            dgvTbl_Sucursales.GridColor = Color.FromArgb(68, 111, 151);

            dgvTbl_Sucursales.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(23, 76, 114);
            dgvTbl_Sucursales.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTbl_Sucursales.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvTbl_Sucursales.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dgvTbl_Sucursales.DefaultCellStyle.BackColor = Color.FromArgb(21, 51, 78);
            dgvTbl_Sucursales.DefaultCellStyle.ForeColor = Color.White;
            dgvTbl_Sucursales.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 145, 210);
            dgvTbl_Sucursales.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvTbl_Sucursales.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvTbl_Sucursales.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(29, 62, 91);
            dgvTbl_Sucursales.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;

            // Se definen proporciones para que los datos usen el espacio sin recortarse.
            AsignarPesoColumna("IDCodigoSucursal", 78F);
            AsignarPesoColumna("NombreSucursal", 125F);
            AsignarPesoColumna("Departamento", 105F);
            AsignarPesoColumna("Municipio", 105F);
            AsignarPesoColumna("Direccion", 165F);
            AsignarPesoColumna("Telefono", 90F);
            AsignarPesoColumna("HorarioAtencion", 135F);
            AsignarPesoColumna("Estado", 65F);

            if (dgvTbl_Sucursales.Columns["HorarioAtencion"] != null)
            {
                dgvTbl_Sucursales.Columns["HorarioAtencion"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }
        }

        private void AsignarPesoColumna(string nombre, float peso)
        {
            if (dgvTbl_Sucursales.Columns[nombre] != null)
            {
                dgvTbl_Sucursales.Columns[nombre].FillWeight = peso;
            }
        }

        private void BuscarEnMemoria()
        {
            string texto = txtBuscarSucursal.Text.Trim();

            try
            {
                if (texto.Length == 0)
                {
                    sucursalesFiltradas = new List<Sucursal>(todasLasSucursales);
                }
                else
                {
                    // El criterio se consulta parametrizado; la paginación sigue en Presentación.
                    sucursalesFiltradas = sucursalesNegocio.BuscarSucursales(texto);
                }

                paginaActual = 0;
                MostrarPaginaActual();
            }
            catch (System.Exception ex)
            {
                MostrarMensaje(
                    "No se pudieron buscar las sucursales. " + ex.Message,
                    "Buscar sucursal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnBuscarSucursal_Click(object sender, System.EventArgs e)
        {
            BuscarEnMemoria();
        }

        private void btnLimpiarBusquedaSucursal_Click(object sender, System.EventArgs e)
        {
            txtBuscarSucursal.Clear();
            BuscarEnMemoria();
        }

        private void btnAgregarSucursal_Click(object sender, System.EventArgs e)
        {
            // Nuevo prepara el formulario; Guardar confirma la inserción.
            modoAgregarSucursal = true;
            modoEditarSucursal = false;
            idSucursalSeleccionada = null;
            txtIDCodigoSucursal.Clear();
            txtNombreSucursal.Clear();
            // El diseñador deja este campo bloqueado; se habilita solo para Agregar.
            txtNombreSucursal.ReadOnly = false;
            txtNombreSucursal.BackColor = Color.White;
            txtNombreSucursal.ForeColor = Color.Black;
            txtDepartamento.Clear();
            txtMunicipio.Clear();
            txtDireccion.Clear();
            txtTelefono.Clear();
            dtpHorarioAtencion.Value = System.DateTime.Now;
            rdbEstadoActivo.Checked = false;
            rdbEstadoInactivo.Checked = false;
            txtNombreSucursal.Focus();
        }

        private void btnLimpiarFormularioSucursal_Click(object sender, System.EventArgs e)
        {
            // Limpia la entrada actual sin modificar ningún registro de SQL Server.
            txtNombreSucursal.Clear();
            txtDepartamento.Clear();
            txtMunicipio.Clear();
            txtDireccion.Clear();
            txtTelefono.Clear();
            dtpHorarioAtencion.Value = System.DateTime.Now;
            rdbEstadoActivo.Checked = false;
            rdbEstadoInactivo.Checked = false;

            // Fuera de Agregar/Editar también se quita la selección del formulario.
            if (!modoAgregarSucursal && !modoEditarSucursal)
            {
                idSucursalSeleccionada = null;
                txtIDCodigoSucursal.Clear();
                txtNombreSucursal.ReadOnly = true;
                return;
            }

            txtNombreSucursal.Focus();
        }

        private void btnGuardarSucursal_Click(object sender, System.EventArgs e)
        {
            if (modoAgregarSucursal)
            {
                GuardarNuevaSucursal();
                return;
            }

            if (modoEditarSucursal && idSucursalSeleccionada.HasValue)
            {
                GuardarEdicionSucursal();
                return;
            }


            MostrarMensaje(
                "Presiona Nuevo para agregar o selecciona una sucursal y pulsa Editar antes de guardar.",
                "Sucursales",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void GuardarNuevaSucursal()
        {
            if (!rdbEstadoActivo.Checked && !rdbEstadoInactivo.Checked)
            {
                MostrarMensaje(
                    "Selecciona si la sucursal está activa o inactiva.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Sucursal nuevaSucursal = CrearSucursalDesdeFormulario();
                int idGenerado = sucursalesNegocio.AgregarSucursal(nuevaSucursal);
                txtIDCodigoSucursal.Text = idGenerado.ToString();
                modoAgregarSucursal = false;
                idSucursalSeleccionada = idGenerado;
                txtNombreSucursal.ReadOnly = true;

                // Recarga los datos para que el total y la grilla incluyan el registro nuevo.
                ConsultarSucursales();
                paginaActual = Math.Max(0, (todasLasSucursales.Count - 1) / RegistrosPorPagina);
                MostrarPaginaActual();

                MostrarMensaje(
                    "Sucursal agregada correctamente. Código: " + idGenerado,
                    "Agregar sucursal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MostrarMensaje(
                    ex.Message,
                    "No se pudo agregar la sucursal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnEditarSucursal_Click(object sender, System.EventArgs e)
        {
            if (!idSucursalSeleccionada.HasValue)
            {
                MostrarMensaje(
                    "Selecciona una sucursal de la grilla antes de editar.",
                    "Editar sucursal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            modoAgregarSucursal = false;
            modoEditarSucursal = true;
            txtNombreSucursal.ReadOnly = false;
            txtNombreSucursal.BackColor = Color.White;
            txtNombreSucursal.ForeColor = Color.Black;
            txtNombreSucursal.Focus();
        }

        private void GuardarEdicionSucursal()
        {
            if (!rdbEstadoActivo.Checked && !rdbEstadoInactivo.Checked)
            {
                MostrarMensaje(
                    "Selecciona si la sucursal está activa o inactiva.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Sucursal sucursal = CrearSucursalDesdeFormulario();
                sucursal.IDCodigoSucursal = idSucursalSeleccionada.Value;
                sucursalesNegocio.EditarSucursal(sucursal);

                int idActualizado = sucursal.IDCodigoSucursal;
                modoEditarSucursal = false;
                txtNombreSucursal.ReadOnly = true;
                ConsultarSucursales();
                MostrarRegistroEnGrilla(idActualizado);

                MostrarMensaje(
                    "Sucursal actualizada correctamente.",
                    "Editar sucursal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MostrarMensaje(
                    ex.Message,
                    "No se pudo editar la sucursal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnEliminarSucursal_Click(object sender, System.EventArgs e)
        {
            if (!idSucursalSeleccionada.HasValue)
            {
                MostrarMensaje(
                    "Selecciona una sucursal de la grilla antes de eliminar.",
                    "Eliminar sucursal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            modoAgregarSucursal = false;
            modoEditarSucursal = false;
            int idEliminar = idSucursalSeleccionada.Value;
            DialogResult confirmacion = MostrarConfirmacionEliminacion(
                "¿Confirmas eliminar la sucursal " + txtNombreSucursal.Text + " (código " + idEliminar + ")?");

            if (confirmacion != DialogResult.Yes)
            {
                return;
            }

            try
            {
                sucursalesNegocio.EliminarSucursal(idEliminar);
                modoAgregarSucursal = false;
                modoEditarSucursal = false;
                LimpiarFormularioSucursal();
                ConsultarSucursales();

                MostrarMensaje(
                    "Sucursal eliminada correctamente.",
                    "Eliminar sucursal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MostrarMensaje(
                    "No se pudo eliminar la sucursal. Puede tener registros relacionados. " + ex.Message,
                    "Eliminar sucursal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnCancelarSucursal_Click(object sender, System.EventArgs e)
        {
            if (!modoAgregarSucursal && !modoEditarSucursal)
            {
                return;
            }

            CancelarOperacionSucursal();
            MostrarMensaje(
                "La operación fue cancelada; no se guardaron cambios.",
                "Cancelar operación",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void CancelarOperacionSucursal()
        {
            bool eraAgregar = modoAgregarSucursal;
            modoAgregarSucursal = false;
            modoEditarSucursal = false;

            if (eraAgregar)
            {
                LimpiarFormularioSucursal();
                return;
            }

            RestaurarSucursalSeleccionada();
        }

        private void dgvTbl_Sucursales_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            Sucursal sucursal = dgvTbl_Sucursales.Rows[e.RowIndex].DataBoundItem as Sucursal;
            if (sucursal == null)
            {
                return;
            }

            modoAgregarSucursal = false;
            modoEditarSucursal = false;
            idSucursalSeleccionada = sucursal.IDCodigoSucursal;
            CargarSucursalEnFormulario(sucursal);
        }

        private void CargarSucursalEnFormulario(Sucursal sucursal)
        {
            txtIDCodigoSucursal.Text = sucursal.IDCodigoSucursal.ToString();
            txtNombreSucursal.Text = sucursal.NombreSucursal;
            txtNombreSucursal.ReadOnly = true;
            txtDepartamento.Text = sucursal.Departamento;
            txtMunicipio.Text = sucursal.Municipio;
            txtDireccion.Text = sucursal.Direccion;
            txtTelefono.Text = sucursal.Telefono;
            dtpHorarioAtencion.Value = sucursal.HorarioAtencion;
            rdbEstadoActivo.Checked = sucursal.Estado;
            rdbEstadoInactivo.Checked = !sucursal.Estado;
        }

        private void RestaurarSucursalSeleccionada()
        {
            if (!idSucursalSeleccionada.HasValue)
            {
                LimpiarFormularioSucursal();
                return;
            }

            Sucursal guardada = todasLasSucursales.FirstOrDefault(
                s => s.IDCodigoSucursal == idSucursalSeleccionada.Value);
            if (guardada == null)
            {
                LimpiarFormularioSucursal();
                return;
            }

            CargarSucursalEnFormulario(guardada);
        }

        private Sucursal CrearSucursalDesdeFormulario()
        {
            Sucursal sucursal = new Sucursal();
            sucursal.NombreSucursal = txtNombreSucursal.Text.Trim();
            sucursal.Departamento = txtDepartamento.Text.Trim();
            sucursal.Municipio = txtMunicipio.Text.Trim();
            sucursal.Direccion = txtDireccion.Text.Trim();
            sucursal.Telefono = txtTelefono.Text.Trim();
            sucursal.HorarioAtencion = dtpHorarioAtencion.Value;
            sucursal.Estado = rdbEstadoActivo.Checked;
            return sucursal;
        }

        private void MostrarRegistroEnGrilla(int idSucursal)
        {
            int indice = todasLasSucursales.FindIndex(s => s.IDCodigoSucursal == idSucursal);
            if (indice < 0)
            {
                return;
            }

            paginaActual = indice / RegistrosPorPagina;
            MostrarPaginaActual();

            int filaPagina = indice % RegistrosPorPagina;
            if (filaPagina < dgvTbl_Sucursales.Rows.Count)
            {
                dgvTbl_Sucursales.ClearSelection();
                dgvTbl_Sucursales.Rows[filaPagina].Selected = true;
                dgvTbl_Sucursales.CurrentCell = dgvTbl_Sucursales.Rows[filaPagina].Cells[0];
                idSucursalSeleccionada = idSucursal;
            }
        }

        private void LimpiarFormularioSucursal()
        {
            idSucursalSeleccionada = null;
            txtIDCodigoSucursal.Clear();
            txtNombreSucursal.Clear();
            txtDepartamento.Clear();
            txtMunicipio.Clear();
            txtDireccion.Clear();
            txtTelefono.Clear();
            rdbEstadoActivo.Checked = false;
            rdbEstadoInactivo.Checked = false;
            txtNombreSucursal.ReadOnly = true;
        }

        // Muestra avisos con una apariencia suave, similar a las tarjetas de una página web.
        private DialogResult MostrarMensaje(string mensaje, string titulo, MessageBoxButtons botones, MessageBoxIcon icono)
        {
            return MensajesForms.Mostrar(this, mensaje, titulo, botones, icono);
        }

        private DialogResult MostrarConfirmacionEliminacion(string mensaje)
        {
            return MensajesForms.Mostrar(this, mensaje, "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }

        private void ActualizarContadores()
        {
            label12.Text = todasLasSucursales.Count.ToString();
            label6.Text = todasLasSucursales.Count(s => s.Estado).ToString();
            label3.Text = todasLasSucursales.Count(s => !s.Estado).ToString();
            label21.Text = todasLasSucursales
                .Select(s => s.Departamento.Trim())
                .Where(departamento => departamento.Length > 0)
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .Count()
                .ToString();
        }

        private void txtBuscarSucursal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BuscarEnMemoria();
                e.SuppressKeyPress = true;
            }
        }

        private void dgvTbl_Sucursales_KeyDown(object sender, KeyEventArgs e)
        {
            // RePág/AvPág permiten cambiar página sin añadir ni mover controles visuales.
            if (e.KeyCode == Keys.PageDown && (paginaActual + 1) * RegistrosPorPagina < sucursalesFiltradas.Count)
            {
                paginaActual++;
                MostrarPaginaActual();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.PageUp && paginaActual > 0)
            {
                paginaActual--;
                MostrarPaginaActual();
                e.Handled = true;
            }
        }
    }
}
