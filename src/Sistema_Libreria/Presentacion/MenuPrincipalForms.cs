using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Libreria;

namespace Presentacion
{
    public partial class MenuPrincipalForms : Form
    {
        public MenuPrincipalForms()
        {
            InitializeComponent();
            picIconoMenu.SizeChanged += (sender, e) => RedondearTarjetaLogo();
            RedondearTarjetaLogo();
            NavegacionPantallas.Configurar(this);
            btnCerrar.Click += (s, e) => Close();
            btnClientes.Click += (s, e) => Abrir(new FormsClientes());
            btnUsuarios.Click += (s, e) => Abrir(new UsuariosForms());
            btnSucursales.Click += (s, e) => Abrir(new SucursalesForms());
            btnPedidos.Click += (s, e) => Abrir(new PedidosForms());
            btnDetallePedido.Click += (s, e) => Abrir(new DetallePedidofomrs());
            btnPagos.Click += (s, e) => Abrir(new PagosForms());
            btnPrestamos.Click += (s, e) => Abrir(new PrestamosForms());
            btnDetallePrestamo.Click += (s, e) => Abrir(new DetallePrestamoForms());
            btnDevoluciones.Click += (s, e) => Abrir(new DevolucionesForms());
            btnDetalleDevolucion.Click += (s, e) => Abrir(new DetalleDevolucionForms());
            btnLibros.Click += (s, e) => Abrir(new LibreriaForms());
            btnAutores.Click += (s, e) => Abrir(new AutoresForms());
            btnEditoriales.Click += (s, e) => Abrir(new EditorialesForms());
            btnCategorias.Click += (s, e) => Abrir(new Categorias());
            button1.Click += (s, e) => Abrir(new FormDashboard());
        }

        private void RedondearTarjetaLogo()
        {
            int ancho = picIconoMenu.ClientSize.Width;
            int alto = picIconoMenu.ClientSize.Height;
            int diametro = Math.Min(24, Math.Min(ancho, alto));
            if (diametro == 0)
                return;

            using (var borde = new GraphicsPath())
            {
                borde.AddArc(0, 0, diametro, diametro, 180, 90);
                borde.AddArc(ancho - diametro, 0, diametro, diametro, 270, 90);
                borde.AddArc(ancho - diametro, alto - diametro, diametro, diametro, 0, 90);
                borde.AddArc(0, alto - diametro, diametro, diametro, 90, 90);
                borde.CloseFigure();
                picIconoMenu.Region = new Region(borde);
            }
        }

        private void Abrir(Form formulario)
        {
            NavegacionPantallas.Configurar(formulario);
            using (formulario) { formulario.ShowDialog(this); }
        }
    }

    internal static class NavegacionPantallas
    {
        public static void Configurar(Form formulario)
        {
            if (formulario == null)
                return;

            ConfigurarControles(formulario);
        }

        private static void ConfigurarControles(Control contenedor)
        {
            foreach (Control control in contenedor.Controls.Cast<Control>().ToArray())
            {
                Button boton = control as Button;
                if (boton != null)
                {
                    string texto = (boton.Text ?? string.Empty).Trim().ToLowerInvariant();
                    bool esCerrarSesion = texto.Contains("cerrar sesión") || texto.Contains("cerrar sesion");

                    if (!esCerrarSesion && texto.Contains("cerrar"))
                    {
                        if (!(contenedor.FindForm() is MenuPrincipalForms))
                        {
                            boton.Visible = false;
                            continue;
                        }
                    }

                    if (texto.Contains("menu principal") || texto.Contains("menú principal"))
                    {
                        boton.Click -= VolverAlMenu_Click;
                        boton.Click += VolverAlMenu_Click;
                    }
                    else if (esCerrarSesion)
                    {
                        boton.Click -= CerrarSesion_Click;
                        boton.Click += CerrarSesion_Click;
                    }
                }

                ConfigurarControles(control);
            }
        }

        private static void VolverAlMenu_Click(object sender, EventArgs e)
        {
            Button boton = sender as Button;
            if (boton != null)
                VolverAlMenuPrincipal(boton.FindForm());
        }

        public static void VolverAlMenuPrincipal(Form pantalla)
        {
            MenuPrincipalForms menu = BuscarMenuPropietario(pantalla)
                ?? Application.OpenForms.OfType<MenuPrincipalForms>().FirstOrDefault();
            Form actual = pantalla;

            while (actual != null && actual != menu)
            {
                Form propietario = actual.Owner;
                actual.Close();
                actual = propietario;
            }

            if (menu == null)
            {
                menu = new MenuPrincipalForms();
                menu.Show();
            }
            menu.Activate();
        }

        private static void CerrarSesion_Click(object sender, EventArgs e)
        {
            Button boton = sender as Button;
            if (boton != null)
                CerrarSesion(boton.FindForm());
        }

        public static void CerrarSesion(Form pantalla)
        {
            MenuPrincipalForms menu = BuscarMenuPropietario(pantalla);
            Form actual = pantalla;

            while (actual != null && actual != menu)
            {
                Form propietario = actual.Owner;
                actual.Close();
                actual = propietario;
            }

            if (menu != null)
            {
                menu.DialogResult = DialogResult.Retry;
                menu.Close();
                return;
            }

            LoginForms login = Application.OpenForms.OfType<LoginForms>().FirstOrDefault();
            if (login == null)
                login = new LoginForms();
            login.Show();
            login.Activate();
        }

        private static MenuPrincipalForms BuscarMenuPropietario(Form pantalla)
        {
            Form actual = pantalla;
            while (actual != null)
            {
                MenuPrincipalForms menu = actual as MenuPrincipalForms;
                if (menu != null)
                    return menu;
                actual = actual.Owner;
            }
            return null;
        }
    }
}
