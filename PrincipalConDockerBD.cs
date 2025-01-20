using Microsoft.Playwright;


[TestFixture]
public class BDTests
{
    private DatabaseManager _dbManager;
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _iphonePage;



    [SetUp]
    public async Task SetUp()
    {
        // Configura la cadena de conexión
        string connectionString = "Server=localhost;Port=3306;Database=testdb;User Id=testuser;Password=testpass;";

        _dbManager = new DatabaseManager(connectionString);

        // Inicializa la base de datos
        await _dbManager.InitializeDatabaseAsync();
        // Verifica si el usuario ya existe antes de insertarlo
        bool userExists = await _dbManager.ValidateUserAsync("standard_user", "secret_sauce");
        if (!userExists)
        {
            await _dbManager.InsertUserAsync("standard_user", "secret_sauce");
        }
    

    // Configura Playwright
    _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        _iphonePage = await _browser.NewPageAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _browser.CloseAsync(); // Cierra el navegador
    }

    [Test]
    public async Task LoginUsingDatabaseData()
    {
        // Obtiene credenciales desde la base de datos
        var (username, password) = await _dbManager.GetFirstUserAsync();

        // Navega al sitio y realiza el login
        await _iphonePage.GotoAsync("https://www.saucedemo.com/");
        await _iphonePage.FillAsync("#user-name", username); // Llena el campo de usuario CON LOS DATOS DE LA BASE
        await _iphonePage.FillAsync("#password", password); // Llena el campo de contraseña CON LOS DATOS DE LA BASE
        await _iphonePage.ClickAsync("#login-button"); // Hace clic en el botón de login

        // Verifica que se haya cargado la página de inventario
        var inventoryItems = await _iphonePage.Locator(".inventory_item").CountAsync();
        Assert.That(inventoryItems > 0, "Inventory items should be loaded.");
    }
}
