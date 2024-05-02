using AppVentas.Entities;
using AppVentas.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace AppVentas;

public partial class ShoppingCarView : ContentPage
{
	readonly LocalDbService dbService;
    private List<Producto>? listaProductos { get; set; }
    private List<Purchase>? purchases { get; set; }

    public ShoppingCarView(LocalDbService dbService)
	{
		this.dbService = dbService;

        InitializeComponent();

		_ = LoadCar();
	}

	async Task LoadCar()
	{
        purchases = await dbService.GetItems();
		listaProductos = purchases.Select(x => {
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
        if (listaProductos is null || listaProductos.Count == 0)
        {
            _ = DisplayAlert("Aviso", "No tiene productos agregados al carrito", "Cerrar");
            return;
        }

        if (!Preferences.ContainsKey(nameof(App.UserToken)) || !Preferences.ContainsKey("CreationDateToken"))
        {
            FormStackLayout.IsVisible = true;
            DataStackLayout.IsVisible = false;
            BackButton.IsVisible = false;
            BuyButton.IsVisible = false;

            return;
        }

        var CreationDateToken = Preferences.Get("CreationDateToken", DateTime.UtcNow);

        var RightNow = DateTime.UtcNow.TimeOfDay.TotalMinutes - CreationDateToken.TimeOfDay.TotalMinutes;
        var TokenMinuteslife = 10;

        //Si el token se registró hace 40 minutos, se solicita que se vuelva a loguear
        if (RightNow >= TokenMinuteslife)
        {
            Preferences.Remove(nameof(App.UserToken));
            Preferences.Remove("CreationDateToken");

            FormStackLayout.IsVisible = true;
            DataStackLayout.IsVisible = false;
            BackButton.IsVisible = false;
            BuyButton.IsVisible = false;
            return;
        }

        _ = BuyAsync();
    }

    async Task BuyAsync()
    {
        try
        {
            var DataToSend = purchases!.Select(x => new
            {
                rowidProducto = x.RowidProducto,
                cantidad = x.Cantidad
            });

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Preferences.Get(nameof(App.UserToken), null));
            var content = new StringContent(JsonConvert.SerializeObject(DataToSend), Encoding.UTF8, "application/json");

            var Request = await client.PostAsync("https://tienda-maui-backend-fee2d021045f.herokuapp.com/api/Purchase", content);

            var Result = await Request.Content.ReadAsStringAsync();

            var Json = JsonConvert.DeserializeObject(Result);

            var SuccessProperty = Json!.GetType().GetProperty("success");

            var Success = false;

            if(SuccessProperty is not null)
                Success = (bool)SuccessProperty.GetValue(Json)!;
            else Success = true;

            if (!Success)
            {
                var ErrorProperty = Json!.GetType().GetProperty("message");
                var Error = (string)ErrorProperty!.GetValue(Json)!;
                _ = DisplayAlert("Tu compra no puedo ser generada", Error, "Cerrar");
                return;
            }

            await dbService.DeleteItems(purchases!.Select(x => x.RowidProducto).ToArray());

            _ = DisplayAlert("Tu compra fue generada correctamente", "Revisa tu correo", "Cerrar");

        }catch(Exception ex)
        {
            Console.WriteLine(ex);
            _ = DisplayAlert("Error interno", "Error al generar la compra", "Cerrar");
        }
    }

    public void Login(object sender, EventArgs e)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            _ = DisplayAlert("Sin conexion", "No tienes acceso a internet", "Cerrar");
            return;
        }

        _ = LoginAsync(sender, e);
    }

    private async Task LoginAsync(object sender, EventArgs e)
    {
        CancelButton.IsEnabled = false;
        LoginButton.IsEnabled = false;

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
            if(Preferences.ContainsKey(nameof(App.UserToken)))
            {
                Preferences.Remove(nameof(App.UserToken));
                Preferences.Remove("CreationDateToken");
            }

            var UserToken = (string)Json["data"]!;

            Preferences.Set(nameof(App.UserToken), UserToken);
            Preferences.Set("CreationDateToken", DateTime.UtcNow);
            App.UserToken = UserToken;

            _ = DisplayAlert("Inició sesión", "Ha iniciado exitosamente", "Cerrar");
            CancelPurchase(sender, e);
        }
        else
        {
            _ = DisplayAlert("Error", (string) Json["error"]!, "Cerrar");
        }

        CancelButton.IsEnabled = true;
        LoginButton.IsEnabled = true;
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