using System;
using System.Collections.Generic;
using System.Linq;
using Digi21.Digi3D;
using Digi21.DigiNG;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.IO;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Math;
using Digi21.Utilities;
using ng = Digi21.DigiNG.DigiNG;

namespace DigiNG.Commands
{
    [Command(Name=nameof(SuperBininfo))]
    public class SuperBininfo : Command
    {
        public SuperBininfo()
        {
            Initialize += SuperBininfo_Initialize;
        }

        private void SuperBininfo_Initialize(object sender, EventArgs e)
        {
            Digi3D.OutputWindow.Show();

            MuestraParametrosArchivo(ng.DrawingFile);
            foreach (var archivo in ng.ReferenceFiles)
                MuestraParametrosArchivo(archivo);

            Dispose();
        }

        private void MuestraParametrosArchivo(IReadOnlyDrawingFile archivo)
        {
            Digi3D.OutputWindow.WriteLine(archivo.Path);

            var maxMin = new Window3D();
            foreach (var entity in archivo)
                maxMin.Union(entity);

            Digi3D.OutputWindow.WriteLine($"Las máximas y mínimas son: {maxMin}");

            Digi3D.OutputWindow.WriteLine($"El archivo tiene: {archivo.Count()} entidades.");
            Digi3D.OutputWindow.WriteLine($"El archivo tiene: {archivo.OfType<ReadOnlyLine>().Count()} líneas ({archivo.Eliminadas().OfType<ReadOnlyLine>().Count()} eliminadas).");
            Digi3D.OutputWindow.WriteLine($"El archivo tiene: {archivo.OfType<ReadOnlyPoint>().Count()} puntos ({archivo.Eliminadas().OfType<ReadOnlyPoint>().Count()} eliminadas).");
            Digi3D.OutputWindow.WriteLine($"El archivo tiene: {archivo.OfType<ReadOnlyText>().Count()} textos ({archivo.Eliminadas().OfType<ReadOnlyText>().Count()} eliminadas).");
            Digi3D.OutputWindow.WriteLine($"El archivo tiene: {archivo.OfType<ReadOnlyPolygon>().Count()} polígonos ({archivo.Eliminadas().OfType<ReadOnlyPolygon>().Count()} eliminadas).");
            Digi3D.OutputWindow.WriteLine($"El archivo tiene: {archivo.OfType<ReadOnlyComplex>().Count()} complejos ({archivo.Eliminadas().OfType<ReadOnlyComplex>().Count()} eliminadas).");


            var códigos = new Dictionary<string, int>();
            foreach (var entity in archivo)
            foreach (var código in entity.Codes)
            {
                if (!códigos.ContainsKey(código.Name))
                    códigos[código.Name] = 0;

                códigos[código.Name]++;
            }

            foreach (var clave in códigos)
                Digi3D.OutputWindow.WriteLine($"{clave.Key}: {clave.Value} entidades.");
        }
    }
}
