using System;
using System.Collections.Generic;
using System.Linq;
using Digi21.Digi3D;
using Digi21.DigiNG.Plugin.TaskPanel;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG.Topology;
using Digi21.Math;
using Digi21.Tasks;
using Digi21.Utilities;

namespace DigiNG.Commands
{
    [LocalizableCommand(typeof(Recursos), "DetectarUndershootName")]
    public class DetectarUndershoot : Command
    {
        public DetectarUndershoot()
        {
            Initialize += DetectarUndershoot_Initialize;
        }

        private void DetectarUndershoot_Initialize(object sender, EventArgs e)
        {
            var tolerancia = double.Parse(Args[0]);

            var líneasDeInterés = Digi21.DigiNG.DigiNG.DrawingFile.NoEliminadas().OfType<ReadOnlyLine>().QueTenganAlgúnCódigoConComodín(Args.Skip(1)).ToArray();
            var nodos = líneasDeInterés.DetectNodes().ToArray();

            foreach (var nodo in nodos)
            {
                Digi3D.OutputWindow.WriteLine(nodo.Key.X.ToString());
            }

            foreach (var línea in líneasDeInterés)
            {
                var coordenadasCercanasAlOrigen = nodos.FindAll(
                    new Point2D(línea.FirstVertex.X, línea.FirstVertex.Y), 
                    tolerancia,
                    nodo => !nodo.LíneaLlegaANodo(línea)).ToArray();
                if (coordenadasCercanasAlOrigen.Any())
                    CreaTareas(línea, línea.FirstVertex, coordenadasCercanasAlOrigen);

                var coordenadasCercanasAlDestino = nodos.FindAll(
                    new Point2D(línea.LastVertex.X, línea.LastVertex.Y), 
                    tolerancia,
                    nodo => !nodo.LíneaLlegaANodo(línea)).ToArray();
                if (coordenadasCercanasAlDestino.Any())
                    CreaTareas(línea, línea.LastVertex, coordenadasCercanasAlDestino);
            }
        }

        private static void CreaTareas(ReadOnlyLine línea, Point3D puntoGotoTareaPrincipal, IEnumerable<Point2D> coordenadasCercanas)
        {
            var tareasPuntos = new List<ITask>();
            foreach (var coordenadaCercanaAlOrigen in coordenadasCercanas)
            {
                var mensaje = string.Format(Recursos.MagnitudX, (new Point2D(puntoGotoTareaPrincipal.X, puntoGotoTareaPrincipal.Y) - coordenadaCercanaAlOrigen).Module);
                tareasPuntos.Add(new TaskGotoPoint(new Point3D(coordenadaCercanaAlOrigen.X, coordenadaCercanaAlOrigen.Y, 0.0), mensaje, TaskSeverity.Error));
            }

            var mensajeTareaPrincipal = string.Format(Recursos.LocalizadoUndershootEnLaLíneaConCódigoX, línea.Codes[0].Name);
            var tareaPrincipal = new TaskEntityGotoPoint(
                puntoGotoTareaPrincipal,
                línea,
                2,
                mensajeTareaPrincipal,
                TaskSeverity.Error,
                Digi21.DigiNG.DigiNG.DrawingFile.Path,
                Recursos.DetectarUndershootName,
                tareasPuntos.ToArray());

            Digi3D.Tasks.Add(tareaPrincipal);
        }
    }
}
