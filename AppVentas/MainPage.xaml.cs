using Newtonsoft.Json;
using System.ComponentModel;
using System.Globalization;

namespace AppVentas
{
    // Clase Producto
    public class Producto : INotifyPropertyChanged
    {
        private bool _comprado;
        private string _rutaImagen = ""; // Inicializar el campo _rutaImagen con una cadena vacía

        public string Descripcion { get; set; }
        public double Precio { get; set; }

        // Hacer el evento PropertyChanged nulable
        public event PropertyChangedEventHandler? PropertyChanged;

        public string RutaImagen
        {
            get {
                if (string.IsNullOrEmpty(_rutaImagen))
                    return "Resources/Images/dotnet_bot.png";
                return _rutaImagen; 
            }
            set
            {
                if (_rutaImagen != value)
                {
                    _rutaImagen = value;
                    OnPropertyChanged(nameof(RutaImagen));
                }
            }
        }

        public bool Comprado
        {
            get { return _comprado; }
            set
            {
                if (_comprado != value)
                {
                    _comprado = value;
                    OnPropertyChanged(nameof(Comprado));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Clase convertidora booleana para negar el valor booleano
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }
            return value;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainPage : ContentPage
    {
        private List<Producto> listaProductos;

        public MainPage()
        {
            InitializeComponent();

            // Inicializar productos
            _ = InicializarProductos();
        }

        private async Task InicializarProductos()
        {
            // Obtener la lista completa de productos
            listaProductos = await ObtenerListaProductos();

            // Asignar la lista completa de productos como la fuente de datos del CollectionView
            productosGrid.ItemsSource = listaProductos;
        }

        private void BotonEliminar_Clicked(object sender, EventArgs e)
        {
            // Lógica para eliminar la compra
            var botonEliminar = (Button)sender;
            var producto = (Producto)botonEliminar.BindingContext;
            DisplayAlert("Compra Eliminada", $"¡Compra del producto '{producto.Descripcion}' eliminada con éxito!", "OK");

            // Ocultar el botón de eliminar compra
            producto.Comprado = false;
        }

        private void BotonComprar_Clicked(object sender, EventArgs e)
        {
            // Lógica para la compra exitosa
            var botonComprar = (Button)sender;
            var producto = (Producto)botonComprar.BindingContext;
            DisplayAlert("Compra Exitosa", $"¡Compra del producto '{producto.Descripcion}' realizada con éxito!", "OK");

            // Mostrar el botón de eliminar compra
            producto.Comprado = true;
        }

        private void BuscarEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Obtener el texto de búsqueda
            string textoBusqueda = e.NewTextValue.ToLowerInvariant();

            // Filtrar la lista de productos por la descripción que contenga el texto de búsqueda
            List<Producto> productosFiltrados = listaProductos.Where(p => p.Descripcion.ToLowerInvariant().Contains(textoBusqueda)).ToList();

            // Actualizar la fuente de datos del CollectionView con los productos filtrados
            productosGrid.ItemsSource = productosFiltrados;
        }

        // Esta función hace la obtención de la lista de productos
        private static async Task<List<Producto>> ObtenerListaProductos()
        {
            try
            {
                var Factory = MauiProgram.Services.GetService<IHttpClientFactory>()!;
                using var client = Factory.CreateClient();
                var Request = await client.GetAsync("https://localhost:7222/api/Product");

                var Content = await Request.Content!.ReadAsStringAsync();

                var Products = JsonConvert.DeserializeObject<List<Producto>>(Content)!;

                return Products;
            }
            catch(Exception ex)
            {
                return Enumerable.Empty<Producto>().ToList();
            }
        }

    }
}
