using AppVentas.Services;

namespace AppVentas;

public partial class ShoppingCarView : ContentPage
{
	readonly LocalDbService dbService;

    public ShoppingCarView(LocalDbService dbService)
	{
		this.dbService = dbService;

        InitializeComponent();

		_ = LoadCar();
	}

	async Task LoadCar()
	{
		var Items = await dbService.GetItems();
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