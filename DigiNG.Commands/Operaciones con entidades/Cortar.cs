using System;
using System.Collections.Generic;
using System.Linq;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG.Plugin.Menu;
using Digi21.Math;
using Digi21.Utilities;

namespace DigiNG.Commands.Operaciones_con_entidades
{
    [LocalizableCommand(typeof(Recursos), "CortarName")]
    [LocalizableCommandInMenu(typeof(Recursos), "CortarTitle", MenuItemGroup.EditGroup4)]
    public class Cortar : Command
    {
        public Cortar()
        {
            Initialize += Cortar_Initialize;
            SetFocus += Cortar_SetFocus;
            DataUp += Cortar_DataUp;
            EntitySelected += Cortar_EntitySelected;
        }

        private void Cortar_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            try
            {
                var límite = e.Entity as ReadOnlyLine;

                var entidadesRecortables = (from entidad in Digi21.DigiNG.DigiNG.DrawingFile
                                            where entidad is IClippable
                                            where !entidad.Deleted
                                            where entidad.AlgúnCódigoVisible()
                                            where Window2D.Intersects(límite, entidad)
                                            select entidad as IClippable).ToList();

                var entidadesAAñadir = new List<Entity>();
                var entidadesAEliminar = new List<Entity>();
                var oldPorciento = -1;
                for (var i = 0; i < entidadesRecortables.Count; i++)
                {
                    var porciento = i * 100 / entidadesRecortables.Count;
                    if (porciento != oldPorciento)
                    {
                        oldPorciento = porciento;
                        Digi3D.StatusBar.ProgressBar.Value = porciento;
                    }
                    try
                    {
                        var partes = entidadesRecortables[i].Clip(límite);
                        entidadesAAñadir.AddRange(partes);
                        entidadesAEliminar.Add(entidadesRecortables[i] as Entity);

                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                var descripción = string.Format(Recursos.SePartieronXEntidadesYSeFormaronYEntidadesNuevas,
                    entidadesAEliminar.Count,
                    entidadesAAñadir.Count);
                Digi3D.Music(MusicType.EndOfLongProcess);
                Digi3D.ShowBallon("Cortar",
                    descripción,
                    2);

                Digi21.DigiNG.DigiNG.DrawingFile.Add(entidadesAAñadir);
                Digi21.DigiNG.DigiNG.DrawingFile.Delete(entidadesAEliminar);
            }
            finally
            {
                Dispose();
            }
        }

        private static void Cortar_DataUp(object sender, Point3DEventArgs e)
        {
            Digi21.DigiNG.DigiNG.SelectEntity(e.Coordinates, entidad => entidad is ReadOnlyLine);
        }

        private static void Cortar_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        private static void Cortar_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        private static void SolicitaSeleccionaEntidad()
        {
            Digi3D.StatusBar.Text = Recursos.SeleccionaLaLíneaLímite;
        }

    }
}
