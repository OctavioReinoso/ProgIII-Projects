// Ejecutar desde la raíz del repo: node patrones_ts/main.ts
import type { InformeAcademico } from './exportadores.ts';
import { CreadorCSV, CreadorExcel, CreadorExportador, CreadorPDF } from './creadores.ts';
import { CreadorJSON } from './formato-json.ts';

const informe: InformeAcademico = {
  titulo: 'Notas 3A Primer Trimestre',
  filas: [
    { alumno: 'Fernández, Lucía', materia: 'Matemática', nota: 8 },
    { alumno: 'Fernández, Lucía', materia: 'Lengua', nota: 9 },
    { alumno: 'Gómez, Tomás', materia: 'Matemática', nota: 5 },
  ],
};

// El cliente sólo conoce CreadorExportador. Nunca hace "new ExportadorPDF()".
function exportarInforme(creador: CreadorExportador, datos: InformeAcademico): void {
  console.log(creador.generarInforme(datos));
  console.log('-'.repeat(60));
}

// El creador se elige por configuración (por ejemplo, lo que pide el usuario en la pantalla).
const creadores: Record<string, CreadorExportador> = {
  pdf: new CreadorPDF(),
  csv: new CreadorCSV(),
  excel: new CreadorExcel(),
  json: new CreadorJSON(), // formato nuevo: se agregó sin tocar exportarInforme()
};

for (const [formato, creador] of Object.entries(creadores)) {
  console.log(`>> Formato pedido: ${formato}`);
  exportarInforme(creador, informe);
}

// La validación común también la heredan todos los formatos.
try {
  new CreadorCSV().generarInforme({ titulo: 'Vacío', filas: [] });
} catch (error) {
  console.log(`Informe vacío -> ${(error as Error).message}`);
}
