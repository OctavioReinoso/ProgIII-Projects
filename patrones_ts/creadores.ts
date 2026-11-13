// CREADORES del Factory Method: quién decide qué producto crear.
import type { Exportador, InformeAcademico } from './exportadores.ts';
import { ExportadorCSV, ExportadorExcel, ExportadorPDF } from './exportadores.ts';

export abstract class CreadorExportador {
  // EL FACTORY METHOD: cada subclase decide qué Exportador concreto devuelve.
  protected abstract crearExportador(): Exportador;

  // Operación común: arma el informe usando el exportador SIN saber cuál es.
  // Esta lógica se escribe una sola vez y sirve para todos los formatos, presentes y futuros.
  generarInforme(informe: InformeAcademico): string {
    if (informe.filas.length === 0) throw new Error('El informe no tiene datos para exportar.');

    const exportador = this.crearExportador();
    const nombreArchivo = `${informe.titulo.toLowerCase().replaceAll(' ', '-')}.${exportador.extension}`;
    const contenido = exportador.exportar(informe);
    return `Archivo: ${nombreArchivo}\n${contenido}`;
  }
}

// Un creador concreto por formato.
export class CreadorPDF extends CreadorExportador {
  protected crearExportador(): Exportador {
    return new ExportadorPDF();
  }
}

export class CreadorCSV extends CreadorExportador {
  protected crearExportador(): Exportador {
    return new ExportadorCSV();
  }
}

export class CreadorExcel extends CreadorExportador {
  protected crearExportador(): Exportador {
    return new ExportadorExcel();
  }
}
