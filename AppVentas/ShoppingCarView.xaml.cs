using AppVentas.Services;
using Newtonsoft.Json;

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
		FormStackLayout.IsVisible = true;
		DataStackLayout.IsVisible = false;
		BackButton.IsVisible = false;
		BuyButton.IsVisible = false;

    }

	public void CancelPurchase(object sender, EventArgs e)
    {
        FormStackLayout.IsVisible = false;
        DataStackLayout.IsVisible = true;
        BackButton.IsVisible = true;
        BuyButton.IsVisible = true;
    }
}