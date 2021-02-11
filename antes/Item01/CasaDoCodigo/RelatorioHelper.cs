using CasaDoCodigo.Models;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CasaDoCodigo
{
  public interface IRelatorioHelper
  {
    Task GerarRelatorio(Pedido pedido);
  }

  public class RelatorioHelper : IRelatorioHelper
  {
    private const string RelativeUri = "api/relatorio";
    private readonly IConfiguration configuration;
    private readonly HttpClient httpClient;
    public RelatorioHelper(IConfiguration configuration, HttpClient httpClient)
    {
      this.configuration = configuration;
      this.httpClient = httpClient;
    }
    public async Task GerarRelatorio(Pedido pedido)
    {
      string linhaRelatorio = await GetLinhaRelatorio(pedido);

      var json = JsonConvert.SerializeObject(linhaRelatorio);
      HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
      //descobrir o endereço (endpoint) do token de acesso
      var discoveryResponse = await httpClient.GetDiscoveryDocumentAsync(configuration["CasaDoCodigoIdentityServerUrl"]);
      if (discoveryResponse.IsError)
      {
        throw new ApplicationException(discoveryResponse.Error);
      }
      //solicitar o token de acesso
      var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(
        new ClientCredentialsTokenRequest { 
          Address = discoveryResponse.TokenEndpoint,
          ClientId = "CasaDoCodigo.MVC",
          ClientSecret = "511536EF-F270-4058-80CA-1C89C192F69A",
          Scope = "CasaDoCodigo.Relatorio",

        });
      if (tokenResponse.IsError)
      {
        Debug.WriteLine(tokenResponse.Error);
        return;
      }
      Uri baseUri = new Uri(configuration["RelatorioWebAPIURL"]);
      Uri uri = new Uri(baseUri, RelativeUri);
      HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(uri, httpContent);

      if (!httpResponseMessage.IsSuccessStatusCode)
      {
        throw new ApplicationException(httpResponseMessage.ReasonPhrase);
      }



    }

    private async Task<string> GetLinhaRelatorio(Pedido pedido)
    {
      StringBuilder sb = new StringBuilder();
      string templatePedido =
              await System.IO.File.ReadAllTextAsync("TemplatePedido.txt");

      string templateItemPedido =
          await System.IO.File.ReadAllTextAsync("TemplateItemPedido.txt");

      string linhaPedido =
          string.Format(templatePedido,
              pedido.Id,
              pedido.Cadastro.Nome,
              pedido.Cadastro.Endereco,
              pedido.Cadastro.Complemento,
              pedido.Cadastro.Bairro,
              pedido.Cadastro.Municipio,
              pedido.Cadastro.UF,
              pedido.Cadastro.Telefone,
              pedido.Cadastro.Email,
              pedido.Itens.Sum(i => i.Subtotal));

      sb.AppendLine(linhaPedido);

      foreach (var i in pedido.Itens)
      {
        string linhaItemPedido =
            string.Format(
                templateItemPedido,
                i.Produto.Codigo,
                i.PrecoUnitario,
                i.Produto.Nome,
                i.Quantidade,
                i.Subtotal);

        sb.AppendLine(linhaItemPedido);
      }
      sb.AppendLine($@"=============================================");

      return sb.ToString();
    }
  }
}