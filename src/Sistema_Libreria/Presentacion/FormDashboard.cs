using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Entidad;
using Negocio;
using Presentacion;

namespace Libreria
{
    // ======================== FORMULARIO PRINCIPAL ========================
    public partial class FormDashboard : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

        static readonly CultureInfo CI = CultureInfo.GetCultureInfo("en-US");
        readonly DashboardNegocio dashboardNegocio = new DashboardNegocio();

        readonly Panel host = new Panel();
        Panel lateral, filtros;
        Label titulo, aStock, aVenc;
        TarjetaKpi kInv, kPed, kPre, kIng;
        ComboBox cFecha, cSuc, cRango;
        GraficoLineas lineas;
        GraficoBarras barras;
        MiniGrafico miniIngresos;
        DataGridView gMov, gCli, gPed;
        TextBox qMov, qCli, qPed;
        TableLayoutPanel dashboardLayout;
        Control tarjetaLineas, tarjetaBarras, tablaClientes, tablaPedidos;
        int modoLayout = -1;
        List<object[]> dMov = new List<object[]>();
        private bool mostrarTodosLosMovimientos;
        List<object[]> dCli = new List<object[]>();
        List<object[]> dPed = new List<object[]>();
        bool cargandoFiltros;
        float tamTitulo;

        public FormDashboard()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            // La vista previa del diseñador no forma parte del dashboard en ejecución.
            foreach (Control control in Controls.Cast<Control>().ToArray())
                control.Dispose();
            Controls.Clear();

            Text = "Librería - Dashboard de Operaciones";
            BackColor = Tema.Fondo;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1350, 780);
            MinimumSize = new Size(640, 520);
            ForeColor = Tema.Texto;
            Font = Tema.Normal;
            DoubleBuffered = true;

            // Orden de acoplamiento: primero el que rellena, al final los que van en los bordes.
            Controls.Add(CrearCentro());
            Controls.Add(CrearLateral());
            Controls.Add(CrearEncabezado());

            cFecha.SelectedIndexChanged += (s, e) => { if (!cargandoFiltros && IsHandleCreated) Aplicar(); };
            cSuc.SelectedIndexChanged += (s, e) => { if (!cargandoFiltros && IsHandleCreated) Aplicar(); };
            cRango.SelectedIndexChanged += (s, e) => { if (!cargandoFiltros && IsHandleCreated) Aplicar(); };
            qMov.TextChanged += (s, e) => LlenarTablas();
            qCli.TextChanged += (s, e) => LlenarTablas();
            qPed.TextChanged += (s, e) => LlenarTablas();
            Resize += (s, e) => Adaptar();
            host.SizeChanged += (s, e) => ReorganizarDashboard();
            Load += (s, e) => { CargarFiltrosYDatos(); Adaptar(); };
        }

        void CargarFiltrosYDatos()
        {
            cargandoFiltros = true;
            try
            {
                var sucursales = new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(0, "Todas las sucursales") };
                sucursales.AddRange(dashboardNegocio.ConsultarSucursales());
                cSuc.DisplayMember = "Value";
                cSuc.ValueMember = "Key";
                cSuc.DataSource = sucursales;
                cSuc.SelectedValue = 0;
            }
            catch (Exception ex)
            {
                MostrarErrorDatos(ex);
                return;
            }
            finally { cargandoFiltros = false; }

            Aplicar();
        }

        void MostrarErrorDatos(Exception ex)
        {
            dMov.Clear(); dCli.Clear(); dPed.Clear();
            titulo.Text = "Dashboard de Operaciones - No se pudieron cargar los datos";
            kInv.Valor.Text = "—"; kPed.Valor.Text = "—"; kPre.Valor.Text = "—"; kIng.Valor.Text = "—";
            aStock.Text = "N/D"; aVenc.Text = "—";
            if (kPre.Extra != null) kPre.Extra.Text = "Sin conexión a los datos";
            lineas.SerieA = new float[0]; lineas.SerieB = new float[0];
            barras.Categorias = new string[0]; barras.Datos = new float[0][];
            LlenarTablas();
            titulo.AccessibleDescription = ex.GetBaseException().Message;
        }
        // ---------- ENCABEZADO ----------
        Panel CrearEncabezado()
        {
            var h = new Panel { Dock = DockStyle.Top, Height = 76, Padding = new Padding(14, 10, 14, 6) };
            titulo = new Label { Dock = DockStyle.Fill, Text = "Dashboard de Operaciones de Librería", ForeColor = Color.FromArgb(201, 214, 242), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };

            var logo = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 78,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Presentacion.Properties.Resources.logo_libreria,
                AccessibleName = "Logo del Sistema Librería"
            };

            cFecha = NuevoCombo("Hoy", "Última semana", "Este mes");
            cSuc = NuevoCombo();
            cFecha.Location = new Point(0, 28);
            cSuc.Location = new Point(160, 28);
            filtros = new Panel { Dock = DockStyle.Right, Width = 330 };
            filtros.Controls.Add(Tema.Etiqueta("Filtros globales", Tema.Pequena, Tema.Suave, DockStyle.None, 18));
            filtros.Controls[0].Location = new Point(0, 4);
            filtros.Controls.Add(cFecha);
            filtros.Controls.Add(cSuc);

            var botonMenuPrincipal = new Button
            {
                Dock = DockStyle.Right,
                Width = 170,
                Height = 44,
                Text = "🏡 Menú Principal",
                Visible = true,
                BackColor = Color.FromArgb(10, 25, 42),
                ForeColor = Color.FromArgb(0, 255, 220),
                FlatStyle = FlatStyle.Flat,
                Font = Tema.Negrita,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            botonMenuPrincipal.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 220);
            botonMenuPrincipal.FlatAppearance.BorderSize = 2;
            botonMenuPrincipal.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 90, 90);
            botonMenuPrincipal.Click += (s, e) => Presentacion.NavegacionPantallas.VolverAlMenuPrincipal(this);

            h.Controls.Add(titulo);
            h.Controls.Add(logo);
            h.Controls.Add(filtros);
            h.Controls.Add(botonMenuPrincipal);
            return h;
        }

        // ---------- BARRA LATERAL ----------
        Panel CrearLateral()
        {
            lateral = new Panel { Dock = DockStyle.Left, Width = 310, Padding = new Padding(8, 0, 0, 8) };
            var mov = TarjetaTabla("Movimientos recientes", Tema.Verde, new[] { "ID", "Fecha", "Unidades", "Tipo" }, true, out qMov, out gMov);

            var alertas = new Tarjeta { Acento = Tema.Ambar, Dock = DockStyle.Top, Height = 122 };
            alertas.Controls.Add(Fila("Préstamos vencidos:", Tema.Rojo, out aVenc));
            alertas.Controls.Add(Fila("Stock crítico (sin dato):", Tema.Ambar, out aStock));
            alertas.Controls.Add(Tema.Etiqueta("Alertas de operación", Tema.Subtitulo, Tema.Texto, DockStyle.Top, 30));

            lateral.Controls.Add(mov);
            lateral.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10 });
            lateral.Controls.Add(alertas);
            return lateral;
        }

        static Panel Fila(string texto, Color color, out Label valor)
        {
            var p = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = Color.Transparent };
            valor = new Label { Dock = DockStyle.Right, Width = 44, TextAlign = ContentAlignment.MiddleRight, ForeColor = color, Font = Tema.Negrita, BackColor = Color.Transparent };
            p.Controls.Add(Tema.Etiqueta(texto, Tema.Normal, Tema.Texto, DockStyle.Fill, 0));
            p.Controls.Add(valor);
            return p;
        }

        // ---------- CENTRO: KPIs + gráficos + tablas ----------
        Control CrearCentro()
        {
            host.Dock = DockStyle.Fill;
            host.AutoScroll = true;
            host.AutoScrollMinSize = new Size(720, 720); // evita que se desconfigure en ventanas pequeñas
            host.Padding = new Padding(0, 0, 8, 8);

            var t = new TableLayoutPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(host.Padding.Left, host.Padding.Top),
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize
            };
            dashboardLayout = t;

            miniIngresos = new MiniGrafico { Height = 66, Tinte = Tema.Dorado, Barras = true, Valores = new float[8], Etiquetas = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Mes" } };

            kInv = new TarjetaKpi("Libros en catálogo", Tema.Cian, null, Color.Empty, 0, null) { Dock = DockStyle.Fill };
            kPed = new TarjetaKpi("Pedidos del periodo", Tema.Ambar, null, Color.Empty, 0, null) { Dock = DockStyle.Fill };
            kPre = new TarjetaKpi("Préstamos activos", Tema.Azul, "Cargando vencimientos...", Color.FromArgb(183, 196, 221), 72, null) { Dock = DockStyle.Fill };
            kIng = new TarjetaKpi("Importe de operaciones", Tema.Dorado, null, Color.Empty, 0, miniIngresos) { Dock = DockStyle.Fill };
            var cl = CrearTarjetaLineas();
            var cb = CrearTarjetaBarras();
            tarjetaLineas = cl;
            tarjetaBarras = cb;

            var tc = TarjetaTabla("Clientes registrados", Tema.Verde, new[] { "Nombre", "Teléfono", "Correo" }, false, out qCli, out gCli);
            var tp = TarjetaTabla("Pedidos recibidos", Tema.Ambar, new[] { "ID", "Fecha", "Cliente", "Total", "Estatus" }, false, out qPed, out gPed);
            tablaClientes = tc;
            tablaPedidos = tp;

            host.Controls.Add(t);
            ReorganizarDashboard();
            return host;
        }

        // Cambia la cantidad de columnas según el ancho disponible y mantiene
        // el desplazamiento vertical cuando una pantalla es pequeña.
        void ReorganizarDashboard()
        {
            if (dashboardLayout == null || host.IsDisposed) return;

            int ancho = Math.Max(320, host.ClientSize.Width - host.Padding.Horizontal);
            int altoVisible = Math.Max(500, host.ClientSize.Height - host.Padding.Vertical);
            int nuevoModo = ancho >= 1250 ? 4 : ancho >= 790 ? 2 : 1;
            int alto;

            if (nuevoModo == 4)
            {
                alto = Math.Max(840, altoVisible);
                if (modoLayout != nuevoModo)
                {
                    dashboardLayout.SuspendLayout();
                    dashboardLayout.Controls.Clear();
                    dashboardLayout.ColumnCount = 4;
                    dashboardLayout.RowCount = 3;
                    dashboardLayout.ColumnStyles.Clear();
                    dashboardLayout.RowStyles.Clear();
                    for (int i = 0; i < 4; i++) dashboardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
                    dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, alto * 0.27f));
                    dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, alto * 0.365f));
                    dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, alto * 0.365f));
                    dashboardLayout.Controls.Add(kInv, 0, 0); dashboardLayout.Controls.Add(kPed, 1, 0);
                    dashboardLayout.Controls.Add(kPre, 2, 0); dashboardLayout.Controls.Add(kIng, 3, 0);
                    dashboardLayout.Controls.Add(tarjetaLineas, 0, 1); dashboardLayout.SetColumnSpan(tarjetaLineas, 2);
                    dashboardLayout.Controls.Add(tarjetaBarras, 2, 1); dashboardLayout.SetColumnSpan(tarjetaBarras, 2);
                    dashboardLayout.Controls.Add(tablaClientes, 0, 2); dashboardLayout.SetColumnSpan(tablaClientes, 2);
                    dashboardLayout.Controls.Add(tablaPedidos, 2, 2); dashboardLayout.SetColumnSpan(tablaPedidos, 2);
                    dashboardLayout.ResumeLayout(true);
                    modoLayout = nuevoModo;
                }
                else
                {
                    dashboardLayout.RowStyles[0].Height = alto * 0.27f;
                    dashboardLayout.RowStyles[1].Height = alto * 0.365f;
                    dashboardLayout.RowStyles[2].Height = alto * 0.365f;
                }
            }
            else if (nuevoModo == 2)
            {
                alto = 1025;
                if (modoLayout != nuevoModo)
                {
                    dashboardLayout.SuspendLayout();
                    dashboardLayout.Controls.Clear();
                    dashboardLayout.ColumnCount = 2;
                    dashboardLayout.RowCount = 4;
                    dashboardLayout.ColumnStyles.Clear();
                    dashboardLayout.RowStyles.Clear();
                    dashboardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                    dashboardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                    foreach (int fila in new[] { 185, 220, 300, 320 })
                        dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, fila));
                    dashboardLayout.Controls.Add(kInv, 0, 0); dashboardLayout.Controls.Add(kPed, 1, 0);
                    dashboardLayout.Controls.Add(kPre, 0, 1); dashboardLayout.Controls.Add(kIng, 1, 1);
                    dashboardLayout.Controls.Add(tarjetaLineas, 0, 2); dashboardLayout.Controls.Add(tarjetaBarras, 1, 2);
                    dashboardLayout.Controls.Add(tablaClientes, 0, 3); dashboardLayout.Controls.Add(tablaPedidos, 1, 3);
                    dashboardLayout.ResumeLayout(true);
                    modoLayout = nuevoModo;
                }
                alto = 1025;
            }
            else
            {
                alto = 1810;
                if (modoLayout != nuevoModo)
                {
                    dashboardLayout.SuspendLayout();
                    dashboardLayout.Controls.Clear();
                    dashboardLayout.ColumnCount = 1;
                    dashboardLayout.RowCount = 8;
                    dashboardLayout.ColumnStyles.Clear();
                    dashboardLayout.RowStyles.Clear();
                    dashboardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                    foreach (int fila in new[] { 190, 190, 220, 190, 240, 240, 270, 270 })
                        dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, fila));
                    dashboardLayout.Controls.Add(kInv, 0, 0); dashboardLayout.Controls.Add(kPed, 0, 1);
                    dashboardLayout.Controls.Add(kPre, 0, 2); dashboardLayout.Controls.Add(kIng, 0, 3);
                    dashboardLayout.Controls.Add(tarjetaLineas, 0, 4); dashboardLayout.Controls.Add(tarjetaBarras, 0, 5);
                    dashboardLayout.Controls.Add(tablaClientes, 0, 6); dashboardLayout.Controls.Add(tablaPedidos, 0, 7);
                    dashboardLayout.ResumeLayout(true);
                    modoLayout = nuevoModo;
                }
            }

            dashboardLayout.Width = ancho;
            dashboardLayout.Height = alto;
            if (dashboardLayout.Location != new Point(host.Padding.Left, host.Padding.Top))
                dashboardLayout.Location = new Point(host.Padding.Left, host.Padding.Top);
            Size minimoScroll = new Size(0, alto + host.Padding.Vertical);
            if (host.AutoScrollMinSize != minimoScroll) host.AutoScrollMinSize = minimoScroll;
        }

        Tarjeta CrearTarjetaLineas()
        {
            var c = new Tarjeta { Acento = Tema.Azul, Dock = DockStyle.Fill };
            lineas = new GraficoLineas { Dock = DockStyle.Fill };
            cRango = NuevoCombo("Últimos 30 días", "Últimos 15 días");
            cRango.Dock = DockStyle.Right;
            cRango.Width = 170;
            var cab = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = Color.Transparent };
            cab.Controls.Add(Tema.Etiqueta("Préstamos vs. devoluciones", Tema.Subtitulo, Tema.Texto, DockStyle.Fill, 0));
            cab.Controls.Add(cRango);
            c.Controls.Add(lineas);
            c.Controls.Add(cab);
            return c;
        }

        Tarjeta CrearTarjetaBarras()
        {
            var c = new Tarjeta { Acento = Tema.Ambar, Dock = DockStyle.Fill };
            barras = new GraficoBarras
            {
                Dock = DockStyle.Fill,
                Categorias = new string[0],
                Datos = new float[0][]
            };
            c.Controls.Add(barras);
            c.Controls.Add(Tema.Etiqueta("Libros por categoría", Tema.Subtitulo, Tema.Texto, DockStyle.Top, 32));
            return c;
        }

        // ---------- TABLAS ----------
        Tarjeta TarjetaTabla(string titulo, Color acento, string[] cols, bool verTodos, out TextBox q, out DataGridView g)
        {
            var c = new Tarjeta { Acento = acento, Dock = DockStyle.Fill };
            g = NuevaTabla(c.Relleno, cols);
            q = NuevaBusqueda();
            var cab = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(0, 0, 0, 6), BackColor = Color.Transparent };
            cab.Controls.Add(q);
            c.Controls.Add(g);
            if (verTodos)
            {
                var enlace = new LinkLabel { Text = "Ver todos", Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleRight, LinkColor = Tema.Verde, ActiveLinkColor = Tema.Cian, BackColor = Color.Transparent };
                enlace.LinkClicked += (sender, e) =>
                {
                    mostrarTodosLosMovimientos = !mostrarTodosLosMovimientos;
                    enlace.Text = mostrarTodosLosMovimientos ? "Ver recientes" : "Ver todos";
                    Aplicar();
                };
                c.Controls.Add(enlace);
            }
            c.Controls.Add(cab);
            c.Controls.Add(Tema.Etiqueta(titulo, Tema.Subtitulo, Tema.Texto, DockStyle.Top, 30));
            return c;
        }

        DataGridView NuevaTabla(Color fondo, string[] cols)
        {
            var d = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, AllowUserToResizeColumns = false,
                RowHeadersVisible = false, BorderStyle = BorderStyle.None, BackgroundColor = fondo, GridColor = Tema.Mezclar(fondo, Tema.Linea, 0.6f),
                EnableHeadersVisualStyles = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, ColumnHeadersHeight = 30,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            d.RowTemplate.Height = 28;
            d.ColumnHeadersDefaultCellStyle.BackColor = fondo;
            d.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(207, 217, 236);
            d.ColumnHeadersDefaultCellStyle.SelectionBackColor = fondo;
            d.ColumnHeadersDefaultCellStyle.Font = Tema.Negrita;
            d.DefaultCellStyle.BackColor = fondo;
            d.DefaultCellStyle.ForeColor = Tema.Texto;
            d.DefaultCellStyle.SelectionBackColor = Color.FromArgb(24, 48, 92);
            d.DefaultCellStyle.SelectionForeColor = Tema.Texto;
            foreach (var h in cols)
            {
                int i = d.Columns.Add(h.Replace("ó", "o"), h);
                d.Columns[i].FillWeight = h == "Acción" ? 45 : h == "ID" ? 28 : 60;
                d.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            d.CellFormatting += Colorear;
            d.CellPainting += PintarBadgeEstado;
            return d;
        }

        TextBox NuevaBusqueda()
        {
            var t = new TextBox { Dock = DockStyle.Top, BackColor = Color.FromArgb(11, 23, 48), ForeColor = Tema.Texto, BorderStyle = BorderStyle.FixedSingle };
            t.HandleCreated += (s, e) => SendMessage(t.Handle, 0x1501, 1, "Buscar"); // texto de ayuda (placeholder)
            return t;
        }

        static ComboBox NuevoCombo(params string[] items)
        {
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(15, 31, 58), ForeColor = Tema.Texto, Width = 150 };
            c.Items.AddRange(items);
            if (items.Length > 0) c.SelectedIndex = 0;
            return c;
        }

        void Colorear(object s, DataGridViewCellFormattingEventArgs e)
        {
            var g = (DataGridView)s;
            if (e.RowIndex < 0 || e.Value == null) return;
            string col = g.Columns[e.ColumnIndex].Name;
            if (col == "Accion") { e.CellStyle.ForeColor = Tema.Verde; return; }
            if (col != "Estatus") return;
            switch (e.Value.ToString())
            {
                case "Completo":
                case "Completado": e.CellStyle.ForeColor = Tema.Verde; break;
                case "Pendiente": e.CellStyle.ForeColor = Tema.Ambar; break;
                case "Procesado": e.CellStyle.ForeColor = Tema.Azul; break;
                case "Cancelado": e.CellStyle.ForeColor = Tema.Rojo; break;
            }
        }

        void PintarBadgeEstado(object s, DataGridViewCellPaintingEventArgs e)
        {
            var grid = (DataGridView)s;
            if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Estatus" || e.Value == null)
                return;

            Color fondo;
            switch (e.Value.ToString())
            {
                case "Completado":
                case "Completo": fondo = Color.FromArgb(22, 101, 68); break;
                case "Pendiente": fondo = Color.FromArgb(133, 77, 14); break;
                case "Cancelado": fondo = Color.FromArgb(153, 27, 27); break;
                case "Procesado": fondo = Color.FromArgb(30, 64, 120); break;
                default: return;
            }

            e.PaintBackground(e.CellBounds, true);
            using (var font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold))
            using (var text = new SolidBrush(Color.FromArgb(248, 250, 252)))
            using (var fill = new SolidBrush(fondo))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            {
                SizeF measured = e.Graphics.MeasureString(e.Value.ToString(), font);
                int width = Math.Min(e.CellBounds.Width - 12, (int)Math.Ceiling(measured.Width) + 20);
                var badge = new Rectangle(e.CellBounds.Left + (e.CellBounds.Width - width) / 2,
                    e.CellBounds.Top + 5, width, Math.Max(18, e.CellBounds.Height - 10));
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = Tema.Redondeado(badge, 7)) e.Graphics.FillPath(fill, path);
                e.Graphics.DrawString(e.Value.ToString(), font, text, badge, format);
            }
            e.Handled = true;
        }

        static bool Coincide(object[] fila, string q)
        {
            return q.Length == 0 || string.Join(" ", fila).ToLower().Contains(q);
        }

        void Llenar(DataGridView g, IEnumerable<object[]> datos, string filtro)
        {
            string q = filtro.Trim().ToLower();
            g.Rows.Clear();
            foreach (var r in datos) if (Coincide(r, q)) g.Rows.Add(r);
        }

        void LlenarTablas()
        {
            if (gMov == null || gCli == null || gPed == null) return;
            Llenar(gMov, dMov, qMov.Text);
            Llenar(gCli, dCli, qCli.Text);
            Llenar(gPed, dPed, qPed.Text);
        }

        // ---------- LÓGICA ----------
        void Aplicar()
        {
            try
            {
                DateTime hoy = DateTime.Today;
                DateTime desde = cFecha.SelectedIndex == 1 ? hoy.AddDays(-6) :
                    cFecha.SelectedIndex == 2 ? new DateTime(hoy.Year, hoy.Month, 1) : hoy;
                DateTime hasta = hoy.AddDays(1);
                int valorSucursal = cSuc.SelectedValue == null ? 0 : Convert.ToInt32(cSuc.SelectedValue);
                int? sucursal = valorSucursal == 0 ? (int?)null : valorSucursal;
                int dias = cRango.SelectedIndex == 1 ? 15 : 30;
                DashboardResumen resumen = dashboardNegocio.Consultar(desde, hasta, sucursal, dias, mostrarTodosLosMovimientos);

                string periodo = cFecha.SelectedIndex == 1 ? "últimos 7 días" : cFecha.SelectedIndex == 2 ? "este mes" : "hoy";
                kInv.Titulo.Text = "Libros en catálogo";
                kInv.Valor.Text = resumen.LibrosEnCatalogo.ToString("N0", CI);
                kPed.Titulo.Text = "Pedidos (" + periodo + ")";
                kPed.Valor.Text = resumen.PedidosEnPeriodo.ToString("N0", CI);
                kPre.Valor.Text = resumen.PrestamosActivos.ToString("N0", CI);
                kIng.Titulo.Text = "Operaciones (" + periodo + ")";
                kIng.Valor.Text = "Q " + resumen.IngresosEnPeriodo.ToString("N2", CI);
                aStock.Text = "N/D";
                aVenc.Text = resumen.PrestamosVencidos.ToString("N0", CI);
                if (kPre.Extra != null)
                    kPre.Extra.Text = resumen.ProximosVencimientos.Count == 0 ? "Sin próximos vencimientos" :
                        string.Join(Environment.NewLine, resumen.ProximosVencimientos.Select(f => "Vence: " + f.ToString("dd/MM/yyyy", CI)));

                miniIngresos.Valores = resumen.IngresosPorMes;
                miniIngresos.Etiquetas = resumen.EtiquetasMeses;
                miniIngresos.Invalidate();
                lineas.SerieA = resumen.PrestamosPorDia;
                lineas.SerieB = resumen.DevolucionesPorDia;
                lineas.Invalidate();
                barras.Categorias = resumen.Categorias.Select(x => x.Nombre).ToArray();
                barras.Datos = resumen.Categorias.Select(x => new float[] { x.Disponibles, x.NoDisponibles }).ToArray();
                barras.Invalidate();

                dMov = resumen.Movimientos.Select(x => new object[] { x.Id, x.Fecha.ToString("dd/MM/yyyy", CI), x.Cantidad, x.Tipo }).ToList();
                dCli = resumen.Clientes.Select(x => new object[] { x.Nombre, x.Telefono, x.Correo }).ToList();
                dPed = resumen.Pedidos.Select(x => new object[] { x.Id, x.Fecha.ToString("dd/MM/yyyy", CI), x.Cliente,
                    "Q " + x.Total.ToString("N2", CI), x.Estado }).ToList();
                titulo.Text = "Dashboard de Operaciones de Librería";
                LlenarTablas();
            }
            catch (Exception ex) { MostrarErrorDatos(ex); }
        }

        // Se adapta al tamaño de la ventana: oculta lo secundario y escala el título.
        void Adaptar()
        {
            int w = ClientSize.Width;
            lateral.Visible = w >= 1100;
            filtros.Visible = w >= 1100;
            float s = (float)Math.Round(Math.Max(13f, Math.Min(22f, w / 60f)));
            if (s != tamTitulo)
            {
                tamTitulo = s;
                var viejo = titulo.Font;
                titulo.Font = new Font("Segoe UI Semibold", s);
                if (viejo != Font) viejo.Dispose();
            }
            ReorganizarDashboard();
        }
    }

    /// <summary>Datos tipados usados como demostración mientras se conecta el dashboard a la base de datos.</summary>
    sealed class MovimientoDashboard
    {
        public int Id { get; private set; }
        public DateTime Fecha { get; private set; }
        public int Total { get; private set; }
        public string Accion { get; private set; }

        public MovimientoDashboard(int id, DateTime fecha, int total, string accion)
        { Id = id; Fecha = fecha; Total = total; Accion = accion; }

        public object[] ToRow()
        { return new object[] { Id, Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), Total, Accion }; }
    }

    sealed class ClienteDashboard
    {
        public string Nombre { get; private set; }
        public DateTime FechaRegistro { get; private set; }
        public string Estatus { get; private set; }
        public string Accion { get; private set; }

        public ClienteDashboard(string nombre, DateTime fechaRegistro, string estatus, string accion)
        { Nombre = nombre; FechaRegistro = fechaRegistro; Estatus = estatus; Accion = accion; }

        public object[] ToRow()
        { return new object[] { Nombre, FechaRegistro.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), Estatus, Accion }; }
    }

    sealed class PedidoDashboard
    {
        public int Id { get; private set; }
        public DateTime Fecha { get; private set; }
        public string Cliente { get; private set; }
        public string Total { get; private set; }
        public string Estatus { get; private set; }

        public PedidoDashboard(int id, DateTime fecha, string cliente, string total, string estatus)
        { Id = id; Fecha = fecha; Cliente = cliente; Total = total; Estatus = estatus; }

        public object[] ToRow()
        { return new object[] { Id, Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), Cliente, Total, Estatus }; }
    }

    // ======================== TEMA ========================
    static class Tema
    {
        public static readonly Color Fondo = Color.FromArgb(7, 16, 31);
        public static readonly Color Panel = Color.FromArgb(13, 26, 48);
        public static readonly Color Linea = Color.FromArgb(29, 44, 72);
        public static readonly Color Texto = Color.FromArgb(232, 238, 249);
        public static readonly Color Suave = Color.FromArgb(138, 155, 184);
        public static readonly Color Cian = Color.FromArgb(34, 211, 238);
        public static readonly Color Ambar = Color.FromArgb(245, 158, 11);
        public static readonly Color Azul = Color.FromArgb(59, 130, 246);
        public static readonly Color Dorado = Color.FromArgb(212, 165, 58);
        public static readonly Color Verde = Color.FromArgb(34, 197, 94);
        public static readonly Color Rojo = Color.FromArgb(248, 113, 113);

        public static readonly Font Normal = new Font("Segoe UI", 9f);
        public static readonly Font Pequena = new Font("Segoe UI", 8f);
        public static readonly Font Negrita = new Font("Segoe UI Semibold", 10f);
        public static readonly Font Subtitulo = new Font("Segoe UI Semibold", 11.5f);

        public static Color Mezclar(Color a, Color b, float t)
        {
            return Color.FromArgb((int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
        }

        public static GraphicsPath Redondeado(Rectangle r, int radio)
        {
            int d = radio * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static Label Etiqueta(string texto, Font f, Color c, DockStyle dock, int alto)
        {
            return new Label { Text = texto, Font = f, ForeColor = c, BackColor = Color.Transparent, Dock = dock, Height = alto, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        }
    }

    // ======================== TARJETA (panel redondeado con borde de color) ========================
    class Tarjeta : Panel
    {
        public Color Acento = Tema.Linea;
        public Color Relleno { get { return Tema.Mezclar(Tema.Panel, Acento, 0.08f); } }

        public Tarjeta()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Padding = new Padding(14);
            Margin = new Padding(6);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : Tema.Fondo);
            using (var path = Tema.Redondeado(new Rectangle(0, 0, Width - 1, Height - 1), 12))
            using (var fill = new SolidBrush(Relleno))
            using (var pen = new Pen(Color.FromArgb(170, Acento)))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }
        }
    }

    // ======================== TARJETA KPI (el tamaño del número se adapta al ancho) ========================
    class TarjetaKpi : Tarjeta
    {
        public readonly Label Titulo, Valor, Extra;
        float tam;
        Font fuente;

        public TarjetaKpi(string titulo, Color acento, string extra, Color colorExtra, int altoExtra, Control mini)
        {
            Acento = acento;
            Extra = null;
            Valor = new Label { Dock = DockStyle.Top, Height = 44, ForeColor = Tema.Texto, BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft };
            Titulo = Tema.Etiqueta(titulo, Tema.Negrita, Color.FromArgb(215, 226, 247), DockStyle.Top, 24);
            // En WinForms el ÚLTIMO control agregado se acopla primero (queda más arriba).
            if (mini != null) { mini.Dock = DockStyle.Bottom; Controls.Add(mini); }
            if (!string.IsNullOrEmpty(extra))
            {
                var ex = Tema.Etiqueta(extra, Tema.Pequena, colorExtra, DockStyle.Top, altoExtra);
                ex.TextAlign = ContentAlignment.TopLeft;
                Extra = ex;
                Controls.Add(ex);
            }
            Controls.Add(Valor);
            Controls.Add(Titulo);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Valor == null) return;
            float n = (float)Math.Round(Math.Max(18f, Math.Min(22f, Width / 12f)));
            if (n != tam)
            {
                tam = n;
                var viejo = fuente;
                fuente = new Font("Segoe UI Semibold", n);
                Valor.Font = fuente;
                if (viejo != null) viejo.Dispose();
            }
            // El margen adicional evita que las cifras se recorten con escalado DPI alto.
            Valor.Height = (int)Math.Ceiling((fuente == null ? n : fuente.GetHeight()) * 1.2f);
        }
    }

    // ======================== GRÁFICOS (GDI+, sin librerías externas) ========================
    abstract class GraficoBase : Control
    {
        protected GraficoBase()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected Rectangle Area { get { return new Rectangle(40, 8, Math.Max(0, Width - 52), Math.Max(0, Height - 66)); } }

        protected void Rejilla(Graphics g, Rectangle a, int max, int paso)
        {
            using (var lp = new Pen(Tema.Linea))
            using (var tb = new SolidBrush(Tema.Suave))
            using (var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
            {
                for (int v = 0; v <= max; v += paso)
                {
                    float y = a.Bottom - v / (float)max * a.Height;
                    g.DrawLine(lp, a.Left, y, a.Right, y);
                    g.DrawString(v.ToString(), Tema.Pequena, tb, new RectangleF(0, y - 8, a.Left - 6, 16), sf);
                }
            }
        }

        protected void Leyenda(Graphics g, string[] nombres, Color[] cols)
        {
            float x = Area.Left, y = Height - 20;
            using (var tb = new SolidBrush(Tema.Suave))
            {
                for (int i = 0; i < nombres.Length; i++)
                {
                    using (var b = new SolidBrush(cols[i])) g.FillRectangle(b, x, y + 3, 10, 10);
                    g.DrawString(nombres[i], Tema.Pequena, tb, x + 14, y);
                    x += 14 + g.MeasureString(nombres[i], Tema.Pequena).Width + 10;
                }
            }
        }

        protected static float Y(Rectangle a, float v, float max)
        {
            return a.Bottom - Math.Max(0, Math.Min(max, v)) / max * a.Height;
        }
    }

    class GraficoLineas : GraficoBase
    {
        public float[] SerieA = new float[0], SerieB = new float[0];

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var a = Area;
            if (a.Width < 40 || a.Height < 40 || SerieA.Length < 2) return;
            float max = Math.Max(10f, (float)Math.Ceiling(Math.Max(SerieA.Max(), SerieB.Length == 0 ? 0 : SerieB.Max()) / 5f) * 5f);
            Rejilla(g, a, (int)max, Math.Max(1, (int)(max / 5f)));

            int n = SerieA.Length;
            Func<float[], PointF[]> pts = s => s.Select((v, i) => new PointF(a.Left + i * (float)a.Width / (n - 1), Y(a, v, max))).ToArray();
            var pa = pts(SerieA);
            var pb = pts(SerieB);

            // Reserva al menos 34 px entre marcas para que los números del eje no se encimen.
            int paso = (int)Math.Ceiling(n * 34f / a.Width);
            using (var tb = new SolidBrush(Tema.Suave))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                for (int i = 0; i < n; i += Math.Max(1, paso))
                    g.DrawString((i + 1).ToString(), Tema.Pequena, tb, new RectangleF(pa[i].X - 15, a.Bottom + 4, 30, 18), sf);

            using (var path = new GraphicsPath())
            {
                path.AddCurve(pa, 0.4f);
                path.AddLine(pa[n - 1].X, a.Bottom, pa[0].X, a.Bottom);
                path.CloseFigure();
                using (var fill = new SolidBrush(Color.FromArgb(55, Tema.Azul))) g.FillPath(fill, path);
            }
            using (var p1 = new Pen(Tema.Azul, 2.5f)) g.DrawCurve(p1, pa, 0.4f);
            using (var p2 = new Pen(Tema.Cian, 2.5f)) g.DrawCurve(p2, pb, 0.4f);
            Leyenda(g, new[] { "Préstamos", "Devoluciones" }, new[] { Tema.Azul, Tema.Cian });
        }
    }

    class GraficoBarras : GraficoBase
    {
        public string[] Categorias = new string[0];
        public float[][] Datos = new float[0][];
        // Paleta coordinada con el tema oscuro de los formularios: cian, azul y turquesa.
        static readonly Color[] Cols = { Tema.Cian, Tema.Azul };

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var a = Area;
            if (a.Width < 40 || a.Height < 58 || Categorias.Length == 0) return;
            // Espacio superior reservado para las cantidades de cada barra.
            var grafica = new Rectangle(a.X, a.Y + 20, a.Width, a.Height - 20);
            float maxDatos = Datos.Where(x => x != null).Select(x => x.Sum()).DefaultIfEmpty(0).Max();
            int max = Math.Max(10, (int)Math.Ceiling(maxDatos / 5f) * 5);
            Rejilla(g, grafica, max, Math.Max(1, max / 5));
            float w = (float)grafica.Width / Categorias.Length;
            using (var tb = new SolidBrush(Tema.Suave))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            {
                for (int i = 0; i < Categorias.Length; i++)
                {
                    float x = grafica.Left + i * w + w * 0.2f, y = grafica.Bottom;
                    if (Datos[i] == null) continue;
                    for (int k = 0; k < Datos[i].Length && k < Cols.Length; k++)
                    {
                        float h = Datos[i][k] / max * grafica.Height;
                        y -= h;
                        using (var b = new SolidBrush(Cols[k])) g.FillRectangle(b, x, y, w * 0.6f, h);
                    }
                    // En ventanas estrechas se omiten las cantidades en vez de superponerlas.
                    if (w >= 38)
                    {
                        string cantidad = Datos[i].Sum().ToString("N0", CultureInfo.InvariantCulture);
                        g.DrawString(cantidad, Tema.Pequena, tb,
                            new RectangleF(grafica.Left + i * w, Math.Max(a.Top, y - 17), w, 15), sf);
                    }
                    g.DrawString(Categorias[i], Tema.Pequena, tb, new RectangleF(grafica.Left + i * w, grafica.Bottom + 4, w, 18), sf);
                }
            }
            Leyenda(g, new[] { "Disponibles", "No disponibles" }, Cols);
        }
    }

    class MiniGrafico : Control
    {
        public bool Barras;
        public float[] Valores = new float[0];
        public string[] Etiquetas;
        public Color Tinte = Tema.Cian;

        public MiniGrafico()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (Valores.Length < 2) return;
            float max = Math.Max(1f, Valores.Max());
            // Deja espacio suficiente bajo las barras para que las etiquetas
            // no se recorten en el borde inferior de la tarjeta.
            int pie = Etiquetas != null ? 21 : 0;
            var r = new RectangleF(0, 2, Width - 1, Math.Max(10, Height - pie - 2));
            if (Barras)
            {
                float w = r.Width / Valores.Length;
                using (var b = new SolidBrush(Tinte))
                using (var tb = new SolidBrush(Tema.Suave))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                    for (int i = 0; i < Valores.Length; i++)
                    {
                        float h = Valores[i] / max * r.Height;
                        g.FillRectangle(b, r.X + i * w + w * 0.15f, r.Bottom - h, w * 0.7f, h);
                        if (Etiquetas != null && w > 22) g.DrawString(Etiquetas[i], Tema.Pequena, tb, new RectangleF(r.X + i * w, r.Bottom + 1, w, 18), sf);
                    }
            }
            else
            {
                var p = Valores.Select((v, i) => new PointF(r.X + i * r.Width / (Valores.Length - 1), r.Bottom - v / max * r.Height * 0.9f)).ToArray();
                using (var path = new GraphicsPath())
                {
                    path.AddCurve(p, 0.4f);
                    path.AddLine(p[p.Length - 1].X, r.Bottom, p[0].X, r.Bottom);
                    path.CloseFigure();
                    using (var fill = new SolidBrush(Color.FromArgb(45, Tinte))) g.FillPath(fill, path);
                }
                using (var pen = new Pen(Tinte, 2f)) g.DrawCurve(pen, p, 0.4f);
            }
        }
    }

    /// <summary>
    /// Representación estática para que el diseñador de WinForms muestre la composición.
    /// En ejecución, FormDashboard reemplaza este control por sus controles funcionales.
    /// </summary>
    [System.ComponentModel.ToolboxItem(false)]
    public sealed class DashboardDesignPreview : Control
    {
        static readonly Color FondoVista = Color.FromArgb(8, 18, 34);
        static readonly Color PanelVista = Color.FromArgb(17, 31, 52);
        static readonly Color AzulVista = Color.FromArgb(18, 43, 72);
        static readonly Color TextoVista = Color.FromArgb(226, 234, 247);
        static readonly Color SuaveVista = Color.FromArgb(139, 160, 191);
        static readonly Color CianVista = Color.FromArgb(34, 211, 238);
        static readonly Color VerdeVista = Color.FromArgb(34, 197, 94);
        static readonly Color AmbarVista = Color.FromArgb(212, 165, 58);

        public DashboardDesignPreview()
        {
            BackColor = FondoVista;
            ForeColor = TextoVista;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Width < 10 || Height < 10) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(FondoVista);
            g.ScaleTransform(Width / 1350f, Height / 780f);

            using (var titleFont = new Font("Segoe UI", 24, FontStyle.Regular))
            using (var headFont = new Font("Segoe UI Semibold", 11, FontStyle.Bold))
            using (var normalFont = new Font("Segoe UI", 9.5f))
            using (var smallFont = new Font("Segoe UI", 8f))
            using (var valueFont = new Font("Segoe UI Semibold", 24, FontStyle.Bold))
            using (var brush = new SolidBrush(TextoVista))
            using (var muted = new SolidBrush(SuaveVista))
            using (var teal = new SolidBrush(CianVista))
            using (var line = new Pen(Color.FromArgb(42, 60, 87)))
            {
                using (var iconPen = new Pen(CianVista, 2.4f))
                using (var iconBg = new SolidBrush(Color.FromArgb(15, 42, 58)))
                using (var iconBorder = new Pen(Color.FromArgb(29, 75, 95)))
                {
                    var icon = new Rectangle(10, 8, 50, 50);
                    using (var path = Tema.Redondeado(icon, 10))
                    {
                        g.FillPath(iconBg, path);
                        g.DrawPath(iconBorder, path);
                    }
                    g.DrawRectangle(iconPen, 26, 17, 18, 31);
                    g.DrawLine(iconPen, 30, 25, 40, 25);
                    g.DrawLine(iconPen, 30, 32, 40, 32);
                    g.DrawLine(iconPen, 30, 39, 40, 39);
                }
                g.DrawString("Dashboard de Operaciones de Librería", titleFont, brush, 72, 12);
                g.DrawString("Filtros", smallFont, muted, 1060, 12);
                PreviewBox(g, 1058, 30, 100, 26, "Hoy", normalFont);
                PreviewBox(g, 1164, 30, 104, 26, "Todas", normalFont);
                g.DrawString("Admin", headFont, brush, 1280, 33);

                // Barra lateral
                PreviewCard(g, new Rectangle(8, 72, 210, 92), AmbarVista);
                g.DrawString("Alertas de operación", headFont, brush, 22, 82);
                g.DrawString("Títulos con stock crítico:", normalFont, brush, 22, 111);
                    using (var alertBrush = new SolidBrush(AmbarVista)) g.DrawString("18", headFont, alertBrush, 190, 109);
                g.DrawString("Préstamos vencidos:", normalFont, brush, 22, 137);
                    using (var alertBrush = new SolidBrush(Color.OrangeRed)) g.DrawString("12", headFont, alertBrush, 190, 135);
                PreviewCard(g, new Rectangle(8, 174, 210, 598), VerdeVista);
                g.DrawString("Movimientos recientes", headFont, brush, 22, 184);
                PreviewBox(g, 18, 209, 188, 24, "Buscar", normalFont);
                g.DrawString("ID     Fecha        Total     Acción", normalFont, brush, 22, 242);
                for (int i = 0; i < 9; i++)
                {
                    int y = 269 + i * 28;
                    if (i == 0) g.FillRectangle(new SolidBrush(Color.FromArgb(28, 54, 91)), 18, y - 2, 190, 25);
                    if (i % 3 == 0)
                    {
                        using (var actionBrush = new SolidBrush(VerdeVista))
                            g.DrawString((39 - (i % 4)).ToString() + "     05/02/...      " + (i % 3 + 1) + "         Editar", smallFont, actionBrush, 22, y);
                    }
                    else
                        g.DrawString((39 - (i % 4)).ToString() + "     05/02/...      " + (i % 3 + 1) + "         Editar", smallFont, brush, 22, y);
                }
                using (var linkBrush = new SolidBrush(VerdeVista)) g.DrawString("Ver todos", normalFont, linkBrush, 158, 738);

                // Indicadores principales
                int x0 = 224, gap = 8, w = 274, y0 = 72, h0 = 164;
                PreviewKpi(g, x0, y0, w, h0, "Libros en inventario", "14,580", CianVista, normalFont, headFont, valueFont);
                PreviewKpi(g, x0 + w + gap, y0, w, h0, "Pedidos nuevos (hoy)", "48", AmbarVista, normalFont, headFont, valueFont);
                PreviewCard(g, new Rectangle(x0 + 2 * (w + gap), y0, w, h0), Color.FromArgb(54, 112, 180));
                g.DrawString("Préstamos activos", headFont, brush, x0 + 2 * (w + gap) + 16, y0 + 14);
                g.DrawString("895", valueFont, brush, x0 + 2 * (w + gap) + 16, y0 + 44);
                g.DrawString("Próximos vencimientos:  3", smallFont, muted, x0 + 2 * (w + gap) + 16, y0 + 101);
                PreviewKpi(g, x0 + 3 * (w + gap), y0, w, h0, "Ingresos totales (mes)", "Q 11,250.75", AmbarVista, normalFont, headFont, valueFont);

                // Gráficas
                int chartY = 244, chartH = 244, chartW = 557;
                PreviewCard(g, new Rectangle(x0, chartY, chartW, chartH), Color.FromArgb(54, 112, 180));
                g.DrawString("Préstamos vs. devoluciones", headFont, brush, x0 + 14, chartY + 12);
                PreviewLineChart(g, new Rectangle(x0 + 28, chartY + 49, chartW - 48, chartH - 82), line);
                PreviewCard(g, new Rectangle(x0 + chartW + 8, chartY, chartW, chartH), AmbarVista);
                g.DrawString("Ventas por categoría de libro", headFont, brush, x0 + chartW + 22, chartY + 12);
                PreviewBars(g, new Rectangle(x0 + chartW + 34, chartY + 48, chartW - 56, chartH - 82), line);

                // Tablas inferiores
                int tableY = 496, tableH = 276;
                PreviewTable(g, new Rectangle(x0, tableY, chartW, tableH), VerdeVista,
                    "Últimos clientes registrados", "Nombre                 Fecha             Estatus            Acción",
                    new[] { "Sofía García             25/02/2023       Completo           Editar", "Pedro López              16/02/2023       Pendiente          Editar", "María Pérez              10/02/2023       Pendiente          Editar", "Luis Ramos               02/02/2023       Completo           Editar", "Ana Torres               28/01/2023       Completo           Editar" }, headFont, normalFont, smallFont);
                PreviewTable(g, new Rectangle(x0 + chartW + 8, tableY, chartW, tableH), AmbarVista,
                    "Últimos pedidos recibidos", "ID       Fecha              Cliente            Total              Estatus",
                    new[] { "39       05/02/2023      Alex Alowe        Q 31.35            Procesado", "28       16/02/2023      Alex Alurre       Q 5.00             Completado", "27       10/02/2023      Marta Gil          Q 18.90            Pendiente", "25       02/02/2023      Juan Díaz          Q 42.00            Cancelado", "24       28/01/2023      Eva Ruiz            Q 12.50            Completado" }, headFont, normalFont, smallFont);
            }
        }

        static void PreviewCard(Graphics g, Rectangle r, Color accent)
        {
            using (var path = Tema.Redondeado(r, 12))
            using (var fill = new SolidBrush(PanelVista))
            using (var edge = new Pen(Color.FromArgb(180, accent)))
            { g.FillPath(fill, path); g.DrawPath(edge, path); }
        }

        static void PreviewBox(Graphics g, int x, int y, int w, int h, string text, Font font)
        {
            using (var fill = new SolidBrush(Color.FromArgb(11, 25, 47)))
            using (var edge = new Pen(Color.FromArgb(65, 83, 111)))
            using (var textBrush = new SolidBrush(SuaveVista))
            { g.FillRectangle(fill, x, y, w, h); g.DrawRectangle(edge, x, y, w, h); g.DrawString(text, font, textBrush, x + 6, y + 3); }
        }

        static void PreviewKpi(Graphics g, int x, int y, int w, int h, string title, string value, Color accent, Font normal, Font heading, Font valueFont)
        {
            PreviewCard(g, new Rectangle(x, y, w, h), accent);
            using (var brush = new SolidBrush(TextoVista))
            using (var number = new SolidBrush(accent))
            using (var pen = new Pen(accent, 2))
            {
                g.DrawString(title, heading, brush, x + 16, y + 14);
                g.DrawString(value, valueFont, brush, x + 16, y + 44);
                g.DrawLine(pen, x + 16, y + h - 23, x + w - 16, y + h - 23);
                using (var note = new SolidBrush(SuaveVista)) g.DrawString("Tendencia reciente", normal, note, x + 16, y + h - 20);
            }
        }

        static void PreviewLineChart(Graphics g, Rectangle r, Pen grid)
        {
            for (int i = 0; i <= 4; i++)
            {
                int y = r.Top + i * r.Height / 4;
                g.DrawLine(grid, r.Left, y, r.Right, y);
            }
            Point[] a = { new Point(r.Left, r.Bottom - 20), new Point(r.Left + 44, r.Top + 25), new Point(r.Left + 96, r.Bottom - 48), new Point(r.Left + 158, r.Top + 35), new Point(r.Left + 220, r.Bottom - 30), new Point(r.Left + 288, r.Top + 20), new Point(r.Left + 354, r.Bottom - 40), new Point(r.Left + 420, r.Top + 14), new Point(r.Right, r.Bottom - 24) };
            Point[] b = { new Point(r.Left, r.Top + 20), new Point(r.Left + 50, r.Bottom - 18), new Point(r.Left + 120, r.Top + 54), new Point(r.Left + 185, r.Bottom - 26), new Point(r.Left + 255, r.Top + 18), new Point(r.Left + 320, r.Bottom - 22), new Point(r.Left + 390, r.Top + 44), new Point(r.Right, r.Top + 30) };
            using (var p1 = new Pen(Color.FromArgb(55, 130, 255), 2.5f))
            using (var p2 = new Pen(CianVista, 2.5f))
            { g.DrawCurve(p1, a); g.DrawCurve(p2, b); }
        }

        static void PreviewBars(Graphics g, Rectangle r, Pen grid)
        {
            for (int i = 0; i <= 4; i++)
            {
                int y = r.Top + i * r.Height / 4;
                g.DrawLine(grid, r.Left, y, r.Right, y);
            }
            Color[] colors = { CianVista, Color.FromArgb(59, 130, 246), Color.FromArgb(45, 212, 191) };
            int[] heights = { 118, 94, 68, 52, 42, 30, 18 };
            int[] totals = { 470, 370, 220, 165, 140, 95, 52 };
            int cell = r.Width / heights.Length;
            using (var labelFont = new Font("Segoe UI", 8f))
            using (var labelBrush = new SolidBrush(TextoVista))
            using (var format = new StringFormat { Alignment = StringAlignment.Center })
            for (int i = 0; i < heights.Length; i++)
            {
                int bw = cell / 2;
                int x = r.Left + i * cell + (cell - bw) / 2;
                int h1 = heights[i] * 48 / 100, h2 = heights[i] * 32 / 100, h3 = heights[i] - h1 - h2;
                using (var a = new SolidBrush(colors[0])) g.FillRectangle(a, x, r.Bottom - h1, bw, h1);
                using (var b = new SolidBrush(colors[1])) g.FillRectangle(b, x, r.Bottom - h1 - h2, bw, h2);
                using (var c = new SolidBrush(colors[2])) g.FillRectangle(c, x, r.Bottom - heights[i], bw, h3);
                g.DrawString(totals[i].ToString("N0", CultureInfo.CurrentCulture), labelFont, labelBrush,
                    new RectangleF(r.Left + i * cell, r.Bottom - heights[i] - 17, cell, 15), format);
            }
        }

        static void PreviewTable(Graphics g, Rectangle r, Color accent, string title, string columns, string[] rows, Font heading, Font normal, Font small)
        {
            PreviewCard(g, r, accent);
            using (var brush = new SolidBrush(TextoVista))
            using (var muted = new SolidBrush(SuaveVista))
            using (var selection = new SolidBrush(Color.FromArgb(28, 54, 91)))
            using (var edge = new Pen(Color.FromArgb(45, 65, 94)))
            {
                g.DrawString(title, heading, brush, r.Left + 14, r.Top + 12);
                PreviewBox(g, r.Left + 10, r.Top + 38, r.Width - 20, 22, "Buscar", small);
                g.DrawString(columns, normal, brush, r.Left + 14, r.Top + 68);
                for (int i = 0; i < rows.Length; i++)
                {
                    int y = r.Top + 94 + i * 27;
                    if (i == 0) g.FillRectangle(selection, r.Left + 10, y - 2, r.Width - 20, 24);
                    g.DrawLine(edge, r.Left + 10, y + 22, r.Right - 10, y + 22);
                    g.DrawString(rows[i], small, i == 0 ? brush : muted, r.Left + 14, y + 1);
                }
            }
        }
    }
}
