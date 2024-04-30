using AppVentas.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace AppVentas;

public partial class ShoppingCarView : ContentPage
{
	readonly LocalDbService dbService;
    private List<Producto>? listaProductos { get; set; }

    public ShoppingCarView(LocalDbService dbService)
	{
		this.dbService = dbService;

        InitializeComponent();

		_ = LoadCar();
	}

	async Task LoadCar()
	{
		var Items = await dbService.GetItems();
		listaProductos = Items.Select(x => {
				var Product = JsonConvert.DeserializeObject<Producto>(x.Product)!;
				Product.Stock = x.Cantidad;
				return Product;
            }).ToList();

		productosGrid.ItemsSource = listaProductos;

    }

    private void BotonEliminar_Clicked(object sender, EventArgs e)
    {
        // Lógica para eliminar la compra
        var botonEliminar = (Button)sender;
        var producto = (Producto)botonEliminar.BindingContext;

        _ = RemoveFromCar(producto.Rowid);
    }

    private async Task RemoveFromCar(int RowidProducto)
    {
        await dbService.DeleteItems([RowidProducto]);

        _ = LoadCar();
    }

    public void GoBack(object sender, EventArgs e)
	{
		_ = Navigation.PopAsync();
	}

    public void Buy(object sender, EventArgs e)
	{
        if(listaProductos is null ||  listaProductos.Count == 0)
        {
            _ = DisplayAlert("Aviso", "No tiene productos agregados al carrito", "Cerrar");
            return;
        }

		FormStackLayout.IsVisible = true;
		DataStackLayout.IsVisible = false;
		BackButton.IsVisible = false;
		BuyButton.IsVisible = false;

    }

    public void Login(object sender, EventArgs e)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            _ = DisplayAlert("Sin conexion", "No tienes acceso a internet", "Cerrar");
            return;
        }

        _ = LoginAsync();
    }

    private async Task LoginAsync()
    {
        CancelButton.IsEnabled = false;
        LoginButton.IsEnabled = false;
        RegisterButton.IsEnabled = false;

        var client = new HttpClient();

        var Body = new
        {
            Email = EmailEntry.Text,
            Password = PassEntry.Text
        };

        var content = new StringContent(JsonConvert.SerializeObject(Body), Encoding.UTF8, "application/json");

        var Request = await client.PostAsync("https://pruebax-091bcc393168.herokuapp.com/login", content);

        var Result = await Request.Content.ReadAsStringAsync();

        var Json = JObject.Parse(Result);

        var Success = (bool) Json["success"]!;

        if (Success)
        {
            _ = DisplayAlert("Inició sesión", "", "Cerrar");
        }
        else
        {
            _ = DisplayAlert("Error", (string) Json["error"]!, "Cerrar");
        }

        CancelButton.IsEnabled = true;
        LoginButton.IsEnabled = true;
        RegisterButton.IsEnabled = true;
    }


    public void CancelPurchase(object sender, EventArgs e)
    {
        FormStackLayout.IsVisible = false;
        DataStackLayout.IsVisible = true;
        BackButton.IsVisible = true;
        BuyButton.IsVisible = true;

        ClearForm();
    }

    void ClearForm()
    {
        EmailEntry.Text = string.Empty;
        PassEntry.Text = string.Empty;
    }
}