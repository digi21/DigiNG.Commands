using System;
using System.Linq;
using System.Windows.Forms;
using Digi21.Digi3D;
using Digi21.DigiNG;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Math;
using Digi21.DigiNG.Plugin.Shell;

namespace OrdenesDigiNG.Códigos
{
    [LocalizableCommand(typeof(Recursos), "CopiarAtributosBBDDName")]
    [LocalizableCommandInMenu(typeof(Recursos), "CopiarAtributosBBDDTitle", MenuItemGroup.EditGroup6)]
    public class CopiarAtributosBBDD : Command
    {
        private Entity entidadOrigen = null;

        public CopiarAtributosBBDD()
        {
            this.Initialize += new EventHandler(CopiarAtributosBBDD_Initialize);
            this.SetFocus += new EventHandler(CopiarAtributosBBDD_SetFocus);
            this.DataUp += new EventHandler<Digi21.Math.Point3DEventArgs>(CopiarAtributosBBDD_DataUp);
            this.EntitySelected += new EventHandler<EntitySelectedEventArgs>(CopiarAtributosBBDD_EntitySelected);
        }

        private void CopiarAtributosBBDD_EntitySelected(object sender, EntitySelectedEventArgs e)
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

            // Copiamos los atributos de los códigos coincidentes
            var nombresCódigosEntidadOrigen = from código in entidadOrigen.Codes
                                              select código.Name;
            var nombresCodigosEntidadDestino = from código in e.Entity.Codes
                                               select código.Name;
            var códigosComunes = nombresCódigosEntidadOrigen.Intersect(nombresCodigosEntidadDestino).ToList();

            if (códigosComunes.Count == 0)
            {
                Digi3D.ShowBallon(
                    OrdenesDigiNG.Recursos.CopiarAtributosBBDDName, 
                    OrdenesDigiNG.Recursos.LaEntidadOrigenYDestinoNoTienenCódigosComunes, 
                    1000);
                Digi3D.Music(MusicType.Error);
            }
            else
            {
                Entity entidadClonada = e.Entity.Clone();

                EliminaEnlacesABBDD(entidadClonada);

                DigiNG.DrawingFile.Add(entidadClonada,
                    entidadOrigen.Owner.get_DatabaseAttributes(entidadOrigen));
                Digi3D.Music(MusicType.EndOfLongProcess);
                DigiNG.DrawingFile.Delete(e.Entity);
            }

            entidadOrigen = null;
            SolicitaSeleccionarEntidad();
        }

        private void EliminaEnlacesABBDD(Entity entidadClonada)
        {
            // Eliminamos los enlaces a BBDD de los distintos códigos para asegurarnos de que al añadir la entidad se generan altas nuevas. No queremos que los códigos 
            // apunten a los registros a los que están apuntando, queremos altas nuevas, porque puede incluso que la entidad de la cual estamos obteniendo los atributos 
            // ni siquiera pertenezca al archivo de dibujo activo.
            for (int i = 0; i < entidadClonada.Codes.Count; i++)
                entidadClonada.Codes[i] = new Code(entidadClonada.Codes[i].Name);
        }
        private void CopiarAtributosBBDD_DataUp(object sender, Point3DEventArgs e)
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

        private void CopiarAtributosBBDD_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionarEntidad();
        }

        private void CopiarAtributosBBDD_Initialize(object sender, EventArgs e)
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
