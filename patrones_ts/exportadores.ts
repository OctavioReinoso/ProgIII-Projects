// PRODUCTOS del Factory Method: lo que se crea.

export interface FilaInforme {
  alumno: string;
  materia: string;
  nota: number;
}

export interface InformeAcademico {
  titulo: string;
  filas: FilaInforme[];
}

// Producto abstracto: el cliente sólo conoce esta interfaz.
export interface Exportador {
  readonly extension: string;
  exportar(informe: InformeAcademico): string;
}

// Productos concretos. En un sistema real usarían librerías de PDF/Excel;
// acá generan un texto que muestra cómo quedaría cada formato.

export class ExportadorPDF implements Exportador {
  readonly extension = 'pdf';

  exportar(informe: InformeAcademico): string {
    const lineas = informe.filas.map((f) => `  ${f.alumno.padEnd(18)} ${f.materia.padEnd(12)} ${f.nota}`);
    return [`%PDF-1.7  [${informe.titulo}]`, ...lineas, '%%EOF'].join('\n');
  }
}

export class ExportadorCSV implements Exportador {
  readonly extension = 'csv';

  exportar(informe: InformeAcademico): string {
    const filas = informe.filas.map((f) => `${f.alumno};${f.materia};${f.nota}`);
    return ['alumno;materia;nota', ...filas].join('\n');
  }
}

export class ExportadorExcel implements Exportador {
  readonly extension = 'xlsx';

  exportar(informe: InformeAcademico): string {
    const celdas = informe.filas.map((f, i) => `A${i + 2}=${f.alumno} | B${i + 2}=${f.materia} | C${i + 2}=${f.nota}`);
    return [`[Hoja: ${informe.titulo}]`, 'A1=Alumno | B1=Materia | C1=Nota', ...celdas].join('\n');
  }
}
