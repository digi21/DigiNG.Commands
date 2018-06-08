using System;
using System.Linq;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG.Plugin.Shell;
using Digi21.Math;

namespace DigiNG.Commands.Códigos
{
    /// <summary>
    /// Simula la orden CAMB_COD de DigiNG con la particuladidad de que si el nombre de tabla asignado en la tabla de códigos activa para el código
    /// destino coincide con el nombre de tabla del código original, se mantienen tanto el número de tabla como el número de registro.
    /// </summary>
    [LocalizableCommand(typeof(Recursos), "CambCodManteniendoAtributosName")]
    [LocalizableCommandInMenu(typeof(Recursos), "CambCodManteniendoAtributosTitle", MenuItemGroup.EditGroup6)]
    public class CambCodManteniendoAtributos : Command
    {
        public CambCodManteniendoAtributos()
        {
            Initialize += SustituyeCodManteniendoAtributos_Initialize;
            SetFocus += SustituyeCodManteniendoAtributos_SetFocus;
            DataUp += SustituyeCodManteniendoAtributos_DataUp;
            EntitySelected += SustituyeCodManteniendoAtributos_EntitySelected;
        }

        private static void SustituyeCodManteniendoAtributos_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            var entidadClonada = e.Entity.Clone();
            for (var i = 0; i<entidadClonada.Codes.Count(); i++)
            {
                var nombreCódigoActivo = Digi21.DigiNG.DigiNG.Codes.ActiveCodes.First().Name;
                var nuevoCódigo = Code.Compose(entidadClonada.Codes[i].Name, nombreCódigoActivo);

                // Si el nombre de tabla del código nuevo coincide con el nombre de tabla del código antiguo, mantenemos tanto el número de tabla
                // como el número de registro. Si no es así, creamos el nuevo código sin asignar ni tabla ni registro.
                if (Digi21.DigiNG.DigiNG.DigiTab[nuevoCódigo].Table == Digi21.DigiNG.DigiNG.DigiTab[entidadClonada.Codes[i].Name].Table)
                    entidadClonada.Codes[i] = new Code(nuevoCódigo, entidadClonada.Codes[i].Table, entidadClonada.Codes[i].Id);
                else
                    entidadClonada.Codes[i] = new Code(nuevoCódigo);
            }

            Digi21.DigiNG.DigiNG.DrawingFile.Add(entidadClonada);
            Digi21.DigiNG.DigiNG.DrawingFile.Delete(e.Entity);
            Digi3D.Music(MusicType.EndOfLongProcess);
            SolicitaSeleccionarEntidad();
        }

        private static void SustituyeCodManteniendoAtributos_DataUp(object sender, Point3DEventArgs e)
        {
            Digi21.DigiNG.DigiNG.SelectEntity(e.Coordinates, entidad => Digi21.DigiNG.DigiNG.DrawingFile.Contains(entidad));
        }

        private static void SustituyeCodManteniendoAtributos_SetFocus(object sender, EventArgs e)
        {
            VerificaCódigosActivos();
            SolicitaSeleccionarEntidad();
        }

        private static void SustituyeCodManteniendoAtributos_Initialize(object sender, EventArgs e)
        {
            VerificaCódigosActivos();
            SolicitaSeleccionarEntidad();
        }

        private static void VerificaCódigosActivos()
        {
            if (Digi21.DigiNG.DigiNG.Codes.ActiveCodes.Count() > 1)
                Digi3D.ShowBallon("camb_cod_manteniendo_atributos", "Tiene varios códigos activos, únicamente se sustiuirá el primero de ellos", 2);
        }

        private static void SolicitaSeleccionarEntidad()
        {
            Digi3D.StatusBar.Text = "Selecciona la entidad para copiar los atributos...";
        }
    }
}
