using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasaDoCodigo.Controllers
{
  public class ContaController : Controller
  {
    [Authorize]
    public async Task<ActionResult> Login()
    {
      return Redirect(Url.Content("~/"));
    }

    [Authorize]
    public async Task Logout()
    {
      //atualizar os cookies (autenticacao local)
      await HttpContext.SignOutAsync("Cookies");
      //faz o signout no identityserver
      await HttpContext.SignOutAsync("OpenIdConnect");
    }
  }
}
