using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MvcNetCorePaginacionRegistros.Data;
using MvcNetCorePaginacionRegistros.Models;

#region VISTAS Y PROCEDIMIENTOS
//alter view V_DEPARTAMENTOS_INDIVIDUAL

//as

//	select CAST(ROW_NUMBER() over (order by DEPT_NO) as int)AS POSICION,

//    DEPT_NO, DNOMBRE, LOC from DEPT

//go
 

//create procedure SP_GRUPO_DEPARTAMENTOS
//(@posicion int)
//as
//	select DEPT_NO, DNOMBRE, LOC from V_DEPARTAMENTOS_INDIVIDUAL
//	where POSICION >= @posicion and POSICION < (@posicion + 2)
//go

//exec SP_GRUPO_DEPARTAMENTOS 1

//create view V_GRUPO_EMPLEADOS
//as
//	select CAST(ROW_NUMBER() over (order by APELLIDO) as int)AS POSICION, EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO from EMP
//go

//create procedure SP_GRUPO_EMPLEADOS
//(@posicion int)
//as
//	select EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO from V_GRUPO_EMPLEADOS
//	where POSICION >= @posicion and POSICION < (@posicion + 3)
//go

//alter procedure SP_GRUPO_EMPLEADOS_OFICIO
//(@posicion int, @oficio nvarchar(50))
//as
//	select EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO from (
//	select CAST(ROW_NUMBER() over (order by APELLIDO) as int)AS POSICION, EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO from EMP
//	where OFICIO = @oficio
//	) query
//	where POSICION >= @posicion and POSICION < (@posicion + 3)
	
//go
#endregion
namespace MvcNetCorePaginacionRegistros.Repositories
{
    public class RepositoryHospital
    {
        private HospitalContext context;

        public RepositoryHospital(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<Departamento>> GetDepartamentosAsync()
        {
            var departamentos = await
                this.context.Departamentos.ToListAsync();
            return departamentos;
        }

        public async Task<List<Empleado>> GetEmpleadosDepartamentoAsync
            (int idDepartamento)
        {
            var empleados = this.context.Empleados
                .Where(x => x.IdDepartamento == idDepartamento);
            if (empleados.Count() == 0)
            {
                return null;
            }
            else
            {
                return await empleados.ToListAsync();
            }
        }
        public async Task<int> GetNumeroRegistrosVistaDepartamentosAsync()
        {
            return await this.context.VistaDepartamentos.CountAsync();
        }
        public async Task<VistaDepartamento> GetVistaDepartamentoAsync(int posicion)
        {
            VistaDepartamento departamento = await
            this.context.VistaDepartamentos
            .Where(z => z.Posicion == posicion).FirstOrDefaultAsync();
            return departamento;
        }
        public async Task<List<VistaDepartamento>> GetGrupoVistaDepartamentoAsync(int posicion)
        {
            var consulta = from datos in this.context.VistaDepartamentos
                           where datos.Posicion >= posicion
                           && datos.Posicion < (posicion + 2)
                           select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<Departamento>> GetGrupoDepartamentoAsync(int posicion)
        {
            string sql = "SP_GRUPO_DEPARTAMENTOS @posicion";
            var consulta = this.context.Departamentos
                .FromSqlRaw(sql, new SqlParameter("@posicion", posicion));

            return await consulta.ToListAsync();
        }





        public async Task<int> GetEmpleadosCountAsync()
        {
            return await this.context.Empleados.CountAsync();
        }

        public async Task<List<Empleado>> GetGrupoEmpleadosAsync(int posicion)
        {
            string sql = "SP_GRUPO_EMPLEADOS @posicion";
            var consulta = this.context.Empleados
                .FromSqlRaw(sql, new SqlParameter("@posicion", posicion));
            return await consulta.ToListAsync();
        }


        public async Task<int> GetEmpleadosOficioCountAsync(string oficio)
        {
            return await this.context.Empleados
                .Where(x => x.Oficio == oficio).CountAsync();
        }
        public async Task<List<Empleado>> GetGrupoEmpleadosOficioAsync(int posicion, string oficio)
        {
            string sql = "SP_GRUPO_EMPLEADOS_OFICIO @posicion, @oficio";
            var consulta = this.context.Empleados
                .FromSqlRaw(sql, new SqlParameter("@posicion", posicion), new SqlParameter("@oficio", oficio));
            return await consulta.ToListAsync();
        }

        public async Task<List<string>> GetOficiosAsync()
        {
            List<string> oficios = await this.context.Empleados
                .Select(x => x.Oficio).Distinct().ToListAsync();
            return oficios;
        }
    }
}
