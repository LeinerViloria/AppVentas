namespace AppVentas;

public partial class ShoppingCarView : ContentPage
{
	public ShoppingCarView()
	{
		InitializeComponent();
	}

    public void GoBack(object sender, EventArgs e)
	{
		_ = Navigation.PopAsync();
	}
}