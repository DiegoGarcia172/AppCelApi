using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace AppCelApi;

public partial class MainPage : ContentPage
{
    private const string BaseUrl = "https://localhost:7140/api"; // Cambia por la URL de tu API
    private string Token { get; set; } // Propiedad para almacenar el token
    public MainPage()
    {
        InitializeComponent();
    }
    private async void OnRegistrarClicked(object sender, EventArgs e)
    {
        string url = $"{BaseUrl}/Acceso/Registrar";
        await EjecutarAccionLoginRegistro(url, false);
    }
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string url = $"{BaseUrl}/Acceso/Login";
        await EjecutarAccionLoginRegistro(url, true);
    }
    private async void OnConsultarClicked(object sender, EventArgs e)
    {
        string url = $"{BaseUrl}/Producto"; // Endpoint del controlador GetAll
        if (string.IsNullOrEmpty(Token))
        {
            await DisplayAlert("Error", "Primero inicie sesión para obtener el token.", "OK");
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var productos = JsonConvert.DeserializeObject<List<Producto>>(responseBody);
                cvDatos.ItemsSource = productos;
            }
            else
            {
                await DisplayAlert("Error", $"Error: {response.StatusCode}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
    }
    private async void OnConsultarPorIdClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtId.Text) || !int.TryParse(txtId.Text, out int id))
        {
            await DisplayAlert("Error", "Por favor, ingrese un ID válido.", "OK");
            return;
        }

        string url = $"{BaseUrl}/Producto/{id}"; // Endpoint para consultar por ID
        if (string.IsNullOrEmpty(Token))
        {
            await DisplayAlert("Error", "Primero inicie sesión para obtener el token.", "OK");
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var producto = JsonConvert.DeserializeObject<Producto>(responseBody);
                cvDatos.ItemsSource = new List<Producto> { producto }; // Mostrar un solo producto
            }
            else
            {
                await DisplayAlert("Error", $"Error: {response.StatusCode}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
    }
    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtId.Text) || !int.TryParse(txtId.Text, out int id))
        {
            await DisplayAlert("Error", "Por favor, ingrese un ID válido.", "OK");
            return;
        }

        string url = $"{BaseUrl}/Producto/{id}"; // Endpoint para eliminar por ID
        if (string.IsNullOrEmpty(Token))
        {
            await DisplayAlert("Error", "Primero inicie sesión para obtener el token.", "OK");
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            HttpResponseMessage response = await client.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Registro eliminado correctamente.", "OK");
            }
            else
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"Error: {response.StatusCode} - {errorBody}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
    }
    private async Task EjecutarAccionLoginRegistro(string url, bool esLogin)
    {
        string nombre = txtNombre.Text;
        string correo = txtCorreo.Text;
        string clave = txtClave.Text;

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
        {
            await DisplayAlert("Error", "Por favor, complete todos los campos.", "OK");
            return;
        }

        var datos = new { nombre, correo, clave };

        try
        {
            using var client = new HttpClient();
            string json = JsonConvert.SerializeObject(datos);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                if (esLogin)
                {
                    var respuesta = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    Token = respuesta?.token;

                    if (!string.IsNullOrEmpty(Token))
                    {
                        await DisplayAlert("Login", "Login exitoso.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo obtener el token.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Registro", "Usuario registrado exitosamente.", "OK");
                }
            }
            else
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"Error: {response.StatusCode} - {errorBody}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
    }
}
public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
}