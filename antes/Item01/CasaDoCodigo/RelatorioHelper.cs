using CasaDoCodigo.Models;
using Newtonsoft.Json;
using System;
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
    public async Task GerarRelatorio(Pedido pedido)
    {
      string linhaRelatorio = await GetLinhaRelatorio(pedido);
      using (HttpClient httpClient = new HttpClient())
      {
        var json = JsonConvert.SerializeObject(linhaRelatorio);
        HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        Uri uri = new Uri("http://localhost:5002/api/relatorio");
        await httpClient.PostAsync(uri, httpContent);
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