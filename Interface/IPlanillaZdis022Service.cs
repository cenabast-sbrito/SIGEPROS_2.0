using Microsoft.Data.SqlClient;
using SIGEPROS.Models;
using SIGEPROS.Models.DTO;
using SIGEPROS.Services;

public interface IPlanillaZdis022Service
{
    ResultadoCargaDto Procesar(
        Stream stream,
        ParametrosCargaDto parametros);
    List<PlanillaZDIS022> getZDis022(string tabla);
    int truncDespachos(string tabla);
    int setDespachos(PlanillaZDIS022 despacho,string tabla);
    string consolidar();

}
