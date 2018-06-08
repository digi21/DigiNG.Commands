using System;
using System.Linq;
using System.Windows.Forms;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG.Plugin.Shell;
using Digi21.Math;

namespace DigiNG.Commands.Códigos
{
    /// <summary>
    /// Esta orden solicita que se seleccione una entidad origen y una destino. Copiará el el número de tabla y registro del código de la entidad origen en la destino
    /// sin modificar el nombre del código de la entidad origen.
    /// </summary>
    [LocalizableCommand(typeof(Recursos), "CopiarTablaRegistroName")]
    [LocalizableCommandInMenu(typeof(Recursos), "CopiarTablaRegistroTitle", MenuItemGroup.EditGroup6)]
    public class CopiarTablaRegistro : Command
    {
        private Entity entidadOrigen;

        public CopiarTablaRegistro()
        {
            Initialize += CopiarTablaRegistro_Initialize;
            SetFocus += CopiarTablaRegistro_SetFocus;
            DataUp += CopiarTablaRegistro_DataUp;
            EntitySelected += CopiarTablaRegistro_EntitySelected;
        }

        private void CopiarTablaRegistro_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            if (e.Entity.Codes.Count > 1)
            {
                Digi3D.Music(MusicType.Error);
                MessageBox.Show(Recursos.HasSeleccionadoUnaEntidadConMasDeUnCodigoEstaOrdenNoEstaPreparada);
                SolicitaSeleccionarEntidad();
                return;
            }

            if (null == entidadOrigen)
            {
                if (!e.Entity.Codes[0].Table.HasValue || !e.Entity.Codes[0].Id.HasValue)
                    MessageBox.Show(Recursos.LaEntidadQueHasSeleccionadoNoTieneNingúnEnlaceDeBaseDatos);
                else
                    entidadOrigen = e.Entity;

                SolicitaSeleccionarEntidad();
                return;
            }

            var entidadClonada = e.Entity.Clone();
            entidadClonada.Codes[0] = new Code(entidadClonada.Codes[0].Name, entidadOrigen.Codes[0].Table, entidadOrigen.Codes[0].Id);
            Digi21.DigiNG.DigiNG.DrawingFile.Add(entidadClonada);
            Digi21.DigiNG.DigiNG.DrawingFile.Delete(e.Entity);
            entidadOrigen = null;
            Digi3D.Music(MusicType.EndOfLongProcess);
            SolicitaSeleccionarEntidad();
        }

        private void CopiarTablaRegistro_DataUp(object sender, Point3DEventArgs e)
        {
            // Aquí ordenamos a DigiNG que localice entidades en las coordenadas en las que el usuario ha hecho clic con el ratón.

            // Tenemos dos casos: Si estamos solicitando al usuario la entidad origen permitimos seleccionar todas las entidades posibles, pero si estamos seleccionando el destino
            // únicamente si la entidad pertenece al archivo de dibujo (y no a los archivos de referencia). Eso se resuelve con una expresión lambda que devuelve verdadero únicamente
            // si la entidad indicada pertenece al archivo de dibujo.
            if (entidadOrigen == null )
                Digi21.DigiNG.DigiNG.SelectEntity(e.Coordinates);
            else
                Digi21.DigiNG.DigiNG.SelectEntity(e.Coordinates, entidad => Digi21.DigiNG.DigiNG.DrawingFile.Contains(entidad));
        }

        private void CopiarTablaRegistro_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionarEntidad();
        }

        private void CopiarTablaRegistro_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionarEntidad();
        }

        private void SolicitaSeleccionarEntidad()
        {
            Digi3D.StatusBar.Text = entidadOrigen == null ? Recursos.SeleccionaLaEntidadOrigen : Recursos.SeleccionaLaEntidadDestino;
        }
    }
}
