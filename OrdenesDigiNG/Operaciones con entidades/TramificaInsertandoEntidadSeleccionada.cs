using System;
using System.Linq;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Digi3D;
using Digi21.Math;
using Digi21.DigiNG;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Topology;
using Digi21.Utilities;
using UtilidadesDigi;
using Digi21.DigiNG.Plugin.Shell;

namespace Ordenes.OperacionesConEntidades
{
    /// <summary>
    /// Esta orden solicita al usuario que se seleccione una entidad y localiza todas las intersecciones de esta entidad con el resto de entidades del archivo
    /// de dibujo. Insertará en la entidad seleccionada un vértice por cada vértice localizado en otras entidades.
    /// </summary>
    [Command(Name = "tram_inserta_vertices_entidad_sel")]
    [CommandInMenu("Línea seleccionada", MenuItemGroup.GeometricAnalysisGroup1Group1)]
    public class TramificaInsertandoEntidadSeleccionada : Command
    {
        public TramificaInsertandoEntidadSeleccionada()
        {
            this.Initialize += InvitaAlUsuarioASeleccionarLínea;
            this.SetFocus += InvitaAlUsuarioASeleccionarLínea;
            this.DataUp += new EventHandler<Digi21.Math.Point3DEventArgs>(TramificaInsertandoEntidadSeleccionada_DataUp);
            this.EntitySelected += new EventHandler<EntitySelectedEventArgs>(TramificaInsertandoEntidadSeleccionada_EntitySelected);
        }

        void TramificaInsertandoEntidadSeleccionada_DataUp(object sender, Point3DEventArgs e)
        {
            DigiNG.SelectEntity(e.Coordinates, entidad => entidad is ReadOnlyLine);
        }

        void InvitaAlUsuarioASeleccionarLínea(object sender, EventArgs e)
        {
            Digi3D.StatusBar.Text = "Selecciona la línea a tramificar";
        }

        void TramificaInsertandoEntidadSeleccionada_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            var líneaATramificar = e.Entity as ReadOnlyLine;

            // Seleccionamos las líneas visibles del archivo de dibujo (excluyendo la línea que vamos a tramificar)
            var líneasContraLasCualesTramificar = from entidad in DigiNG.DrawingFile.ExplotaAReadOnlyLine().Visibles()
                                                  where entidad != e.Entity
                                                  select entidad;

            // Obtenemos las intersecciones de la línea seleccionada con el reto de líneas visibles
            var intersecciones = líneaATramificar.DetectIntersections(líneasContraLasCualesTramificar);

            // Construimos una línea nueva con los códigos de la línea a tramificar
            Line líneaNueva = new Line(líneaATramificar.Codes);

            // Recorremos todos los segmentos de la línea a tramificar
            for (int vértice = 0; vértice < líneaATramificar.Points.Count - 1; vértice++)
            {
                // Añadimos el vértice de la línea original
                líneaNueva.Points.Add(líneaATramificar.Points[vértice]);

                // Ahora localizamos únicamente las intersecciones localizadas para el segmento actual en la línea a tramificar
                var vérticesAAñadirEnEsteSegmento = intersecciones.SoloDeSegmento(líneaATramificar, vértice);

                // Tenemos una lista de vértices, pero pueden venir desordenados. Vamos a ordenarlos calculando su distancia a la coordenada del primer vértice de este segmento
                Point2D vérticeComienzoSegmento = (Point2D)líneaATramificar.Points[vértice];

                var vérticesOrdenados = from v in vérticesAAñadirEnEsteSegmento
                                        let distancia = (v.Key - vérticeComienzoSegmento).Module
                                        orderby distancia
                                        select v.Key;

                // Ahora insertamos estos vértices en la línea nueva. Los vértices son 2D (los métodos de extensión proporcionados por el tipo IntersectionDetector trabajan con Point2D
                // así que tendremos que ínterpolar la coordenada Z
                foreach (var v in vérticesOrdenados)
                {
                    var segmento = new Segment(líneaATramificar.Points[vértice], líneaATramificar.Points[vértice + 1]);
                    double z = segmento.InterpolatedZ(v);

                    líneaNueva.Points.Add(new Point3D(v.X, v.Y, z));
                }
            }

            // Por último añadimos el último vértice
            líneaNueva.Points.Add(líneaATramificar.Points.Last());

            DigiNG.DrawingFile.Add(líneaNueva);
            DigiNG.DrawingFile.Delete(e.Entity);
            Dispose();
        }

    }
}
