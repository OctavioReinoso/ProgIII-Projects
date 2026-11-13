// Nuevo formato agregado DESPUÉS, en un archivo aparte.
// Para sumarlo bastó con crear un producto y un creador nuevos:
// no se modificó ni exportadores.ts ni creadores.ts ni la operación generarInforme().
import type { Exportador, InformeAcademico } from './exportadores.ts';
import { CreadorExportador } from './creadores.ts';

export class ExportadorJSON implements Exportador {
  readonly extension = 'json';

  exportar(informe: InformeAcademico): string {
    return JSON.stringify(informe, null, 2);
  }
}

export class CreadorJSON extends CreadorExportador {
  protected crearExportador(): Exportador {
    return new ExportadorJSON();
  }
}
