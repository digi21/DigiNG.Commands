using System;
using System.Linq;
using System.Windows.Forms;
using Digi21.Digi3D;
using Digi21.DigiNG;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Math;
using Digi21.DigiNG.Plugin.Shell;

namespace Ordenes.Códigos
{
    /// <summary>
    /// Esta orden solicita que se seleccione una entidad origen y una destino. Copiará el nombre del código de la entidad origen en la destino sin sustituir
    /// los enlaces de BBDD que tuviera la entidad destino.
    /// </summary>
    [LocalizableCommand(typeof(OrdenesDigiNG.Recursos), "CopiarNombreCódigoName")]
    [LocalizableCommandInMenuAttribute(typeof(OrdenesDigiNG.Recursos), "CopiarNombreCódigoTitle", MenuItemGroup.EditGroup6)]
    public class CopiarNombreCódigo : Command
    {
        private Entity entidadOrigen = null;

        public CopiarNombreCódigo()
        {
            this.Initialize += new EventHandler(CopiarNombreCódigo_Initialize);
            this.SetFocus += new EventHandler(CopiarNombreCódigo_SetFocus);
            this.DataUp += new EventHandler<Digi21.Math.Point3DEventArgs>(CopiarNombreCódigo_DataUp);
            this.EntitySelected += new EventHandler<EntitySelectedEventArgs>(CopiarNombreCódigo_EntitySelected);
        }

        private void CopiarNombreCódigo_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            if (e.Entity.Codes.Count > 1)
            {
                Digi3D.Music(MusicType.Error);
                MessageBox.Show(OrdenesDigiNG.Recursos.HasSeleccionadoUnaEntidadConMasDeUnCodigoEstaOrdenNoEstaPreparada);
                SolicitaSeleccionarEntidad();
                return;
            }

            if (null == entidadOrigen)
            {
                entidadOrigen = e.Entity;
                SolicitaSeleccionarEntidad();
                return;
            }

            Entity entidadClonada = e.Entity.Clone();
            entidadClonada.Codes[0] = new Code(entidadOrigen.Codes[0].Name, entidadClonada.Codes[0].Table, entidadClonada.Codes[0].Id);
            DigiNG.DrawingFile.Add(entidadClonada);
            DigiNG.DrawingFile.Delete(e.Entity);
            entidadOrigen = null;
            Digi3D.Music(MusicType.EndOfLongProcess);
            SolicitaSeleccionarEntidad();
        }

        private void CopiarNombreCódigo_DataUp(object sender, Point3DEventArgs e)
        {
            // Aquí ordenamos a DigiNG que localice entidades en las coordenadas en las que el usuario ha hecho clic con el ratón.

            // Tenemos dos casos: Si estamos solicitando al usuario la entidad origen permitimos seleccionar todas las entidades posibles, pero si estamos seleccionando el destino
            // únicamente si la entidad pertenece al archivo de dibujo (y no a los archivos de referencia). Eso se resuelve con una expresión lambda que devuelve verdadero únicamente
            // si la entidad indicada pertenece al archivo de dibujo.
            if (entidadOrigen == null)
                DigiNG.SelectEntity(e.Coordinates);
            else
                DigiNG.SelectEntity(e.Coordinates, entidad => DigiNG.DrawingFile.Contains(entidad));
        }

        private void CopiarNombreCódigo_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionarEntidad();
        }

        private void CopiarNombreCódigo_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionarEntidad();
        }

        private void SolicitaSeleccionarEntidad()
        {
            if (entidadOrigen == null)
                Digi3D.StatusBar.Text = OrdenesDigiNG.Recursos.SeleccionaLaEntidadOrigen;
            else
                Digi3D.StatusBar.Text = OrdenesDigiNG.Recursos.SeleccionaLaEntidadDestino;
        }
    }
}
