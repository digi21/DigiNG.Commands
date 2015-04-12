using System;
using System.Collections.Generic;
using System.Linq;
using Digi21.Digi3D;
using Digi21.DigiNG;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG.Plugin.Shell;

namespace Ordenes.Códigos
{
    /// <summary>
    /// Simula la orden CAMB_COD de DigiNG con la particuladidad de que si el nombre de tabla asignado en la tabla de códigos activa para el código
    /// destino coincide con el nombre de tabla del código original, se mantienen tanto el número de tabla como el número de registro.
    /// </summary>
    [LocalizableCommand(typeof(OrdenesDigiNG.Recursos), "CambCodManteniendoAtributosName")]
    [LocalizableCommandInMenuAttribute(typeof(OrdenesDigiNG.Recursos), "CambCodManteniendoAtributosTitle", MenuItemGroup.EditGroup6)]
    public class CambCodManteniendoAtributos : Command
    {
        public CambCodManteniendoAtributos()
        {
            this.Initialize += new EventHandler(SustituyeCodManteniendoAtributos_Initialize);
            this.SetFocus += new EventHandler(SustituyeCodManteniendoAtributos_SetFocus);
            this.DataUp += new EventHandler<Digi21.Math.Point3DEventArgs>(SustituyeCodManteniendoAtributos_DataUp);
            this.EntitySelected += new EventHandler<EntitySelectedEventArgs>(SustituyeCodManteniendoAtributos_EntitySelected);
        }

        void SustituyeCodManteniendoAtributos_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            Entity entidadClonada = e.Entity.Clone();

            string nombreCódigoActivo = DigiNG.Codes.ActiveCodes.First().Name;

            List<Codes> códigosNuevos = new List<Codes>();
            for (int i = 0; i<entidadClonada.Codes.Count(); i++)
            {
                string nuevoCódigo = Code.Compose(entidadClonada.Codes[i].Name, nombreCódigoActivo);

                // Si el nombre de tabla del código nuevo coincide con el nombre de tabla del código antiguo, mantenemos tanto el número de tabla
                // como el número de registro. Si no es así, creamos el nuevo código sin asignar ni tabla ni registro.
                if (DigiNG.DigiTab[nuevoCódigo].Table == DigiNG.DigiTab[entidadClonada.Codes[i].Name].Table)
                    entidadClonada.Codes[i] = new Code(nuevoCódigo, entidadClonada.Codes[i].Table, entidadClonada.Codes[i].Id);
                else
                    entidadClonada.Codes[i] = new Code(nuevoCódigo);
            }

            DigiNG.DrawingFile.Add(entidadClonada);
            DigiNG.DrawingFile.Delete(e.Entity);
            Digi3D.Music(MusicType.EndOfLongProcess);
            SolicitaSeleccionarEntidad();
        }

        void SustituyeCodManteniendoAtributos_DataUp(object sender, Digi21.Math.Point3DEventArgs e)
        {
            DigiNG.SelectEntity(e.Coordinates, entidad => DigiNG.DrawingFile.Contains(entidad));
        }

        void SustituyeCodManteniendoAtributos_SetFocus(object sender, EventArgs e)
        {
            VerificaCódigosActivos();
            SolicitaSeleccionarEntidad();
        }

        void SustituyeCodManteniendoAtributos_Initialize(object sender, EventArgs e)
        {
            VerificaCódigosActivos();
            SolicitaSeleccionarEntidad();
        }

        private static void VerificaCódigosActivos()
        {
            if (DigiNG.Codes.ActiveCodes.Count() > 1)
                Digi3D.ShowBallon("camb_cod_manteniendo_atributos", "Tiene varios códigos activos, únicamente se sustiuirá el primero de ellos", 2);
        }

        private static void SolicitaSeleccionarEntidad()
        {
            Digi3D.StatusBar.Text = "Selecciona la entidad para copiar los atributos...";
        }
    }
}
