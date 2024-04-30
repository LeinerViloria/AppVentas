
using AppVentas.Entities;
using SQLite;
using AppVentas.Enums;
using Newtonsoft.Json;

namespace AppVentas.Services
{
    public class LocalDbService
    {
        const string DBName = "LocalDb_Tienda";
        readonly SQLiteAsyncConnection connection = null!;

        public LocalDbService()
        {
            try
            {
                var Route = Path.Combine(FileSystem.AppDataDirectory, DBName);
                connection = new SQLiteAsyncConnection(Route);
                connection.CreateTableAsync<Purchase>().Wait();
            }
            catch
            {

            }
        }

        public async Task AddToCart(Producto Item, int Cantidad)
        {
            try
            {
                var Purchase = new Purchase()
                {
                    RowidProducto = Item.Rowid,
                    Cantidad = Cantidad,
                    State = EnumStatePurchase.Inqueue,
                    Product = JsonConvert.SerializeObject(Item)
                };

                await connection.InsertAsync(Purchase);
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<List<Purchase>> GetItems()
        {
            try
            {
                return await connection.Table<Purchase>().ToListAsync();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Purchase>();
            }
        }

        public async Task DeleteItems(int[] Items)
        {
            try
            {
                await connection.Table<Purchase>()
                    .Where(x => Items.Contains(x.RowidProducto))
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<IEnumerable<int>> GetProductsInCar()
        {
            var Items = await connection.Table<Purchase>()
                .ToListAsync();

            return Items.Select(x => x.RowidProducto);
        }
    }
}
