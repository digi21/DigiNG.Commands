using System;
using System.Linq;
using Digi21.Digi3D;
using Digi21.DigiNG.Plugin.Command;
using ng = Digi21.DigiNG.DigiNG;

namespace DigiNG.Commands.Operaciones_con_entidades
{
    [Command(Name=nameof(RecuperarGeometriasPorFecha))]
    public class RecuperarGeometriasPorFecha : Command
    {
        public RecuperarGeometriasPorFecha()
        {
            SetFocus += (_, e) => Digi3D.StatusBar.Text = "Selecciona geometría con la fecha de creación que quieres recuperar...";
            DataUp += (_, e) => ng.SelectEntity(e.Coordinates);
            EntitySelected += RecuperarGeometriasPorFecha_EntitySelected;
        }

        private void RecuperarGeometriasPorFecha_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            RecuperaGeometríasConFecha(e.Entity.CreationTime);
            Dispose();
        }

        private static void RecuperaGeometríasConFecha(DateTime fechaCreación)
        {
            var geometríasRecuperar =
                (from entidad in ng.DrawingFile
                    where entidad.Deleted
                    where entidad.CreationTime == fechaCreación
                    select entidad).ToList();

            if (geometríasRecuperar.Count > 0)
            {
                ng.DrawingFile.Undelete(geometríasRecuperar);
                Digi3D.Music(MusicType.EndOfLongProcess);
                Digi3D.ShowBallon("Recuperar geometrías por fecha",
                    $"Se han recuperado {geometríasRecuperar.Count} geometrías", 3);
            }
            else
            {
                Digi3D.Music(MusicType.Error);
                Digi3D.ShowBallon("Recuperar geometrías por fecha",
                    "No se ha recuperado ninguna geometría", 3);
            }
        }
    }
}
