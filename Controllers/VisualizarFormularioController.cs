﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Opiniometro_WebApp.Models;
using System.Data.Entity.Core.Objects;
using System.Text;
using System.Web.Helpers; //Para graficos, borrar despues
using Opiniometro_WebApp.ViewModels;

namespace Opiniometro_WebApp.Controllers
{
    public class VisualizarFormularioController : Controller
    {


        private Opiniometro_DatosEntities db;

        public VisualizarFormularioController()
        {
            db = new Opiniometro_DatosEntities();
        }

        public VisualizarFormularioController(Opiniometro_DatosEntities db)
        {
            this.db = db;
        }

        // GET: VisualizarFormulario | metodo HTTPGET por default (usado cuando se carga la pag la cual despliega todos los formularios disponibles)
        public ActionResult Index()
        {
            //lista de todos los codigos de formularios disponibles
            var codigos = (from cod in db.Formulario select cod.CodigoFormulario).AsEnumerable().Distinct();
            ViewBag.CodigoID = new SelectList(codigos);

            //lista de todos los semestres distintos en la base de datos
            var semestres = (from sem in db.Ciclo_Lectivo select sem.Semestre).AsEnumerable().Distinct();
            ViewBag.SemestreId = new SelectList(semestres);

            //lista de todos los años distintos en la base de datos
            var annos = (from ann in db.Ciclo_Lectivo select ann.Anno).AsEnumerable().Distinct();
            ViewBag.AnnoId = new SelectList(annos);

            //lista de todos los grupos distintos en la base de datos
            var grupos = (from grup in db.Grupo select grup.Numero).AsEnumerable().Distinct();
            ViewBag.GrupoID = new SelectList(grupos);

            //forma 1:
            var profes = (from prof in db.Profesor
                          join per in db.Persona on prof.CedulaProfesor equals per.Cedula
                          select new
                          {
                              id = prof.CedulaProfesor,
                              NombreProfe = per.Nombre + " " + per.Apellido1 + " " + per.Apellido2

                          }).AsEnumerable().Distinct();

            ViewBag.ProfesorID = new SelectList(profes, "id", "NombreProfe");

            var carreras = (from carrera in db.Carrera select carrera.Nombre).AsEnumerable().Distinct();
            ViewBag.CarreraNombre = new SelectList(carreras);

            //interfaz que proporciona funcionalidad para evaluar consultas
            IQueryable<Responde> formularios = from form in db.Responde select form;

            List<Curso> cursosConFormularios = new List<Curso>();

            //se agregan a la lista la sigla de cursos con formularios
            foreach (var c in formularios)
            {
                Curso nuevo = db.Curso.Find(c.SiglaGrupoResp);

                cursosConFormularios.Add(nuevo);
            }

            //se transfiere temporalmente datos desde este controlador a la vista que no están en el modelo
            ViewBag.CursoID = new SelectList(cursosConFormularios.Distinct(), "Sigla", "Nombre");

            List<Formulario> formulariosfiltrados = new List<Formulario>();

            //se cargan todos los formularis disponibles
            foreach (var f in formularios)
            {
                Formulario nuevo = db.Formulario.Find(f.CodigoFormularioResp);

                formulariosfiltrados.Add(nuevo);
            }

            return View(formulariosfiltrados);

        }

        //le indica al motor de enrutamiento que envie cualquier solicitud POST a este metodo de accion (excepto la primera vez)
        [HttpPost] 
        public ActionResult Index(int? x)
        {
            IQueryable<Responde> formularios = from form in db.Responde select form;
            List<Formulario> formulariosfiltrados = new List<Formulario>();
            List<Curso> cursosConFormularios = new List<Curso>();

            //se cargan todos los cursos que tiene formularios
            foreach (var c in formularios)
            {
                Curso nuevo = db.Curso.Find(c.SiglaGrupoResp);

                cursosConFormularios.Add(nuevo);           
            }

            //se envian los datos de codigo, semestre, año y grupos a la vista

            var codigos = (from cod in db.Formulario select cod.CodigoFormulario).AsEnumerable().Distinct();
            ViewBag.CodigoID = new SelectList(codigos);

            ViewBag.CursoID = new SelectList(cursosConFormularios.Distinct(), "Sigla", "Nombre");
            
            var semestres = (from sem in db.Ciclo_Lectivo select sem.Semestre).AsEnumerable().Distinct();
            ViewBag.SemestreId = new SelectList(semestres);

            var annos = (from ann in db.Ciclo_Lectivo select ann.Anno).AsEnumerable().Distinct();
            ViewBag.AnnoId = new SelectList(annos);

            var grupos = (from grup in db.Grupo select grup.Numero).AsEnumerable().Distinct();
            ViewBag.GrupoID = new SelectList(grupos);

            var profes = (from prof in db.Profesor
                          join per in db.Persona on prof.CedulaProfesor equals per.Cedula
                          select new
                          {
                              id = prof.CedulaProfesor,
                              NombreProfe = per.Nombre + " " + per.Apellido1 + " " + per.Apellido2
                          });

            ViewBag.ProfesorID = new SelectList(profes, "id", "NombreProfe");

            var carreras = (from carrera in db.Carrera select carrera.Nombre).AsEnumerable().Distinct();
            ViewBag.CarreraNombre = new SelectList(carreras);

            String selectcodigo = Request.Form["selectcodigo"];
            String selectcarrera = Request.Form["selectcarrera"];
            String selectcurso = Request.Form["selectcurso"];
            String selectsemestre = Request.Form["selectsemestre"];
            String selectanno = Request.Form["selectanno"];
            String selecgrupo = Request.Form["selecgrupo"];
            String selecprofe = Request.Form["selecprofe"];
            String selecid = Request.Form["selecid"];

            if (!String.IsNullOrEmpty(selectcarrera) || !String.IsNullOrEmpty(selectcodigo) || !String.IsNullOrEmpty(selectsemestre) || !String.IsNullOrEmpty(selectcurso) || !String.IsNullOrEmpty(selectanno) || !String.IsNullOrEmpty(selecgrupo) || !String.IsNullOrEmpty(selecid)|| !String.IsNullOrEmpty(selecprofe))
            {
                if (!String.IsNullOrEmpty(selectcodigo))
                {
                    formularios = formularios.Where(f => f.CodigoFormularioResp.Equals(selectcodigo));
                }
                if (!String.IsNullOrEmpty(selectcurso))
                {
                    formularios = formularios.Where(f => f.SiglaGrupoResp.Equals(selectcurso));
                }
                if (!String.IsNullOrEmpty(selectsemestre))
                {
                    byte semestre = byte.Parse(selectsemestre);
                    formularios = formularios.Where(f => f.SemestreGrupoResp.Equals( semestre ));
                }
                if (!String.IsNullOrEmpty(selectanno))
                {
                    short anno = short.Parse(selectanno);
                    formularios = formularios.Where(f => f.AnnoGrupoResp.Equals( anno ));
                }
                if (!String.IsNullOrEmpty(selecgrupo))
                {
                    byte grupo = byte.Parse(selecgrupo);
                    formularios = formularios.Where(f => f.NumeroGrupoResp.Equals( grupo ));
                }
                if (!String.IsNullOrEmpty(selecid))
                {
                    formularios = formularios.Where(f => f.CedulaProfesor.Equals(selecid));
                }

                //se se uso el filtro de carrera
                if (!String.IsNullOrEmpty(selectcarrera))
                {
                    ObjectResult<String> arregloResultado = db.SP_EncontrarFormularioPorNombreCarrera(selectcarrera);
                    foreach (String result in arregloResultado)
                    {
                        foreach (var f in formularios)
                        {
                            //si un formulario corresponde a la carrera que se uso para filtrar entonces se filtra
                            if (f.CodigoFormularioResp == result)
                            {
                                Formulario nuevo = db.Formulario.Find(f.CodigoFormularioResp);

                                formulariosfiltrados.Add(nuevo);
                            }
                        }
                    }
                }
                else
                {
                    //si no se uso el filtro de carrera
                    foreach (var f in formularios)
                    {
                        Formulario nuevo = db.Formulario.Find(f.CodigoFormularioResp);

                        formulariosfiltrados.Add(nuevo);
                    }
                }
                return View(formulariosfiltrados);
            }
            else
            {
                foreach (var f in formularios)
                {
                    Formulario nuevo = db.Formulario.Find(f.CodigoFormularioResp);

                    formulariosfiltrados.Add(nuevo);
                }
                return View(formulariosfiltrados);
            }
        }

        // GET: VisualizarFormulario/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Formulario formulario = db.Formulario.Find(id);
            if (formulario == null)
            {
                return HttpNotFound();
            }

            return View(formulario);
        }

        // GET: VisualizarFormulario/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: VisualizarFormulario/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CodigoFormulario,Nombre")] Formulario formulario)
        {
            if (ModelState.IsValid)
            {
                db.Formulario.Add(formulario);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(formulario);
        }

        // GET: VisualizarFormulario/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Formulario formulario = db.Formulario.Find(id);
            if (formulario == null)
            {
                return HttpNotFound();
            }
            return View(formulario);
        }

        // POST: VisualizarFormulario/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CodigoFormulario,Nombre")] Formulario formulario)
        {
            if (ModelState.IsValid)
            {
                db.Entry(formulario).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(formulario);
        }

        // GET: VisualizarFormulario/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Formulario formulario = db.Formulario.Find(id);
            if (formulario == null)
            {
                return HttpNotFound();
            }
            return View(formulario);
        }

        // POST: VisualizarFormulario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Formulario formulario = db.Formulario.Find(id);
            db.Formulario.Remove(formulario);
            db.SaveChanges();
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

        //EFE: Devuelve un Int con la cantidad de respuestas por respuesta.
        //REQ: Que exista la conexion a la base de datos.
        //MOD:--
        [HttpGet]
        private ObjectResult<SP_ContarRespuestasPorGrupo_Result> ObtenerCantidadRespuestasPorPregunta(string codigoFormulario, string cedulaProfesor, short? annoGrupo, byte? semestreGrupo, byte? numeroGrupo, string siglaCurso, string itemId)
        {
            var result = db.SP_ContarRespuestasPorGrupo(codigoFormulario, cedulaProfesor, annoGrupo, semestreGrupo, numeroGrupo, siglaCurso, itemId);
            return result;
        }

       

        //EFE:Crea un gráfico con la información de los resultados en la base de datos.
        //REQ:Que exista una conexion a la base de datos.
        //MOD:--string cedulaProfesor, short annoGrupo, byte semestreGrupo, byte numeroGrupo, string siglaCurso,
        public JsonResult GraficoPie(string codigoFormulario, string cedulaProfesor, short annoGrupo, byte semestreGrupo, byte numeroGrupo, string siglaCurso, string itemId, byte tipoPregunta)
        {          
            var result = ObtenerCantidadRespuestasPorPregunta(codigoFormulario, cedulaProfesor, annoGrupo, semestreGrupo, numeroGrupo, siglaCurso, itemId).ToList();
            List<object> x = new List<object>();
            List<object> y = new List<object>();

            List<int?> listaOrdenada = new List<int?>();
            int size = 0;
            double? mediana = 0;
            double? tamanio = 0;
            int counter = 0;
            double? promedio = 0;
            double? desviacion = 0;
            double sumatoria = 0;
            double actual = 0;

            if (!Enumerable.Range(5, 6).Contains(tipoPregunta))
            {

                var labels = db.SP_RecuperarEtiquetas(tipoPregunta, itemId).ToList();
                bool encontrado;
                
                foreach (var label in labels)
                {
                    encontrado = false;
                    x.Add(label);
                    foreach (var resul in result)
                    {
                        if (resul.Respuesta == label)
                        {
                            y.Add(resul.cntResp);
                            listaOrdenada.Add(resul.cntResp);
                            tamanio = tamanio + listaOrdenada[counter];
                            counter++;
                            encontrado = true;
                        }
                    }
                    if (encontrado == false)
                    {
                        y.Add(0);
                        listaOrdenada.Add(0);
                        tamanio = tamanio + listaOrdenada[counter];
                        counter++;
                    }
                }
            }
            else
            {
                var rango = db.SP_RecuperarEtiquetasEscalar(tipoPregunta, itemId).ToList();
                int inicio = rango[0].Inicio;
                int final = rango[0].Fin;
                int incremento = rango[0].Incremento;
                bool encontrado;
                for (int i = inicio; i <= (final+1); i += incremento)
                {
                    encontrado = true;
                    if (i == final + 1)
                    {
                        x.Add("No se/No Responde/No Aplica");
                        foreach (var resul in result)
                        {
                            if (Int16.Parse(resul.Respuesta) == i)
                            {
                                encontrado = true;
                                y.Add(resul.cntResp);
                                listaOrdenada.Add(resul.cntResp);
                                tamanio = tamanio + listaOrdenada[counter];
                                counter++;
                            }
                        }
                        if(encontrado == false)
                        {
                            y.Add(0);
                            listaOrdenada.Add(0);
                            tamanio = tamanio + listaOrdenada[counter];
                            counter++;
                        }
                    }
                    else
                    {
                        
                        x.Add(i);
                        foreach (var resul in result)
                        {
                            if (Int16.Parse(resul.Respuesta) == i)
                            {
                                encontrado = true;
                                y.Add(resul.cntResp);
                                listaOrdenada.Add(resul.cntResp);
                                tamanio = tamanio + listaOrdenada[counter];
                                counter++;
                            }
                        }
                        if (encontrado == false)
                        {
                            y.Add(0);
                            listaOrdenada.Add(0);
                            tamanio = tamanio + listaOrdenada[counter];
                            counter++;
                        }
                    }
                }
            }

            listaOrdenada.Sort();
            size = listaOrdenada.Count();

            //Calcular el promedio

            promedio = tamanio / size;

            //Calcular la mediana

            if (size % 2 == 0)
            {
                mediana = (listaOrdenada[(size / 2) - 1] + listaOrdenada[(size / 2)]) / 2;
            }
            else
            {
                mediana = listaOrdenada[(size / 2)];
            }

            //Calcular la desviacion estandar

            for (int i = 0; i < size; i++)
            {
                actual = (double)listaOrdenada[i];
                sumatoria = sumatoria + Math.Pow(actual - (double)mediana, 2);
            }

            desviacion = Math.Sqrt(sumatoria / size);

            List<object> lista = new List<object> { x, y, promedio, mediana, desviacion};
            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        //EFE:Retorna las observaciones del item.
        //REQ:Que exista una conexion a la base de datos.
        //MOD:--
        [HttpGet]
        private ObjectResult<SP_DevolverObservacionesPorGrupo_Result> ObtenerObservacionesPorGrupo(string codigoFormulario, string cedulaProfesor, short? annoGrupo, byte? semestreGrupo, byte? numeroGrupo, string siglaCurso, string itemId)
        {
            var result = db.SP_DevolverObservacionesPorGrupo(codigoFormulario, cedulaProfesor, annoGrupo, semestreGrupo, numeroGrupo, siglaCurso, itemId);
            return result;
        }

        //EFE:Devuelve las observaciones asignadas a una pregunta en especifico.
        //REQ:Que exista una conexion a la base de datos.
        //MOD:--
        public JsonResult ObservacionesPorPregunta(string codigoFormulario, string cedulaProfesor, short annoGrupo, byte semestreGrupo, byte numeroGrupo, string siglaCurso, string itemId)
        {
            var result = ObtenerObservacionesPorGrupo(codigoFormulario, cedulaProfesor, annoGrupo, semestreGrupo, numeroGrupo, siglaCurso, itemId).ToList();

            List<object> obs = new List<object>();
            List<object> nom = new List<object>();
            List<object> ap1 = new List<object>();
            List<object> ap2 = new List<object>();

            foreach (var itemO in result)
            {
                obs.Add(itemO.Observacion);
                nom.Add(itemO.Nombre);
                ap1.Add(itemO.Apellido1);
                ap2.Add(itemO.Apellido2);
            }

            List<object> observaciones = new List<object> { obs, nom, ap1, ap2 };
            return Json(observaciones, JsonRequestBehavior.AllowGet);
        }
        
        //EFE:Devuelve las preguntas de tipo texto libre.
        //REQ:Que exista una conexion a la base de datos.
        //MOD:--
        public JsonResult ObtenerRespTexto(string codigoFormulario, string cedulaProfesor, short annoGrupo, byte semestreGrupo, byte numeroGrupo, string siglaCurso, string itemId)
        {
            var result = ObtenerCantidadRespuestasPorPregunta(codigoFormulario, cedulaProfesor, annoGrupo, semestreGrupo, numeroGrupo, siglaCurso, itemId);

            List<object> respuestas = new List<object>();

            foreach (var item in result)
            {
                respuestas.Add(item.Respuesta);
            }

            List<object> respuestasTexto = new List<object> { respuestas };
            return Json(respuestasTexto, JsonRequestBehavior.AllowGet);
        }
    }
}