using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Restaurante.Models;
using RestSharp;
using RestSharp.Authenticators;

namespace Restaurante.Controllers
{
    public class ReservasController : Controller
    {
        public string Mensaje { get; set; }
        public string Estilo { get; set; }


        private RestauranteEntities db = new RestauranteEntities();

        // GET: Reservas
        [Authorize]
        public async Task<ActionResult> Index()
        {
            var reserva = db.Reserva.Include(r => r.Sede);
            return View(await reserva.ToListAsync());
        }

        // GET: Reservas/Details/5
        [Authorize]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reserva reserva = await db.Reserva.FindAsync(id);
            if (reserva == null)
            {
                return HttpNotFound();
            }
            return View(reserva);
        }

        // GET: Reservas/Create
        [AllowAnonymous]
        public ActionResult Create()
        {
            ViewBag.IdSede = new SelectList(db.Sede, "IdSede", "NombreSede");
            return View();
        }

        // POST: Reservas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> Create([Bind(Include = "IdReserva,NombreUsuario,Fecha,IdSede,Cantidad,Correo")] Reserva reserva)
        {
            if (ModelState.IsValid)
            {
                db.Reserva.Add(reserva);
                await db.SaveChangesAsync();
                SendSimpleMessage(reserva);
                if (User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
                
            }

            ViewBag.IdSede = new SelectList(db.Sede, "IdSede", "NombreSede", reserva.IdSede);
            return View(reserva);
        }

        // GET: Reservas/Edit/5
        [Authorize]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reserva reserva = await db.Reserva.FindAsync(id);
            if (reserva == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdSede = new SelectList(db.Sede, "IdSede", "NombreSede", reserva.IdSede);
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Edit([Bind(Include = "IdReserva,NombreUsuario,Fecha,IdSede,Cantidad,Correo")] Reserva reserva)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reserva).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.IdSede = new SelectList(db.Sede, "IdSede", "NombreSede", reserva.IdSede);
            return View(reserva);
        }

        // GET: Reservas/Delete/5
        [Authorize]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reserva reserva = await db.Reserva.FindAsync(id);
            if (reserva == null)
            {
                return HttpNotFound();
            }
            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Reserva reserva = await db.Reserva.FindAsync(id);
            db.Reserva.Remove(reserva);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public static void SendSimpleMessage(Reserva reserva)
        {
            RestauranteEntities db = new RestauranteEntities();
            RestClient client = new RestClient();
            System.Uri uri = new System.Uri("https://api.mailgun.net/v3");
            client.BaseUrl = uri;
            client.Authenticator =
                   new HttpBasicAuthenticator("api",
                                              "key-05394996027f04202e30a49f7d6ea57a");
            RestRequest request = new RestRequest();
            request.AddParameter("domain",
                                "sandbox2a19a8beb07d48279eecf180fe4f0d62.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Equipo Restaurante <postmaster@sandbox2a19a8beb07d48279eecf180fe4f0d62.mailgun.org>");
            request.AddParameter("to", reserva.NombreUsuario + " " + "<" + reserva.Correo + ">");
            request.AddParameter("subject", "Reserva en el Restaurante para: " + reserva.NombreUsuario);
            request.AddParameter("text", "Has realizado una reserva en el restaurante\nDatos de la reserva:\n\tA nombre de: " + reserva.NombreUsuario + "\n\tCódigo reserva: " + reserva.IdReserva + "\n\tFecha reserva: " + reserva.Fecha + "\n\tSede: " + db.Sede.FirstOrDefault(x => x.IdSede.Equals(reserva.IdSede)).NombreSede + "\nRecuerde que el código de la reserva es indispensable para el ingreso.\nPara mayores informes llamar al 01-8000-117711");
            request.Method = Method.POST;
            client.Execute(request);
        }
    }
}
