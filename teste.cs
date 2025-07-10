using BOX3_ERP_API.Core.Data;
using BOX3_ERP_API.Core.DTOs.Advertencia;
using BOX3_ERP_API.Core.Models.AuxiliarModels;
using BOX3_ERP_API.Core.Models.DatabaseModels;
using BOX3_ERP_API.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
// using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dropbox.Api.Files.ListRevisionsMode;

namespace BOX3_ERP_API.Core.Repositories.Implementations
{
    public class AdvertenciaRepository : IAdvertenciaRepositoryj
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AdvertenciaRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> Adicionar(CreateAdvertenciaDto createDto, string usuario)
        {
            var colaborador = await _applicationDbContext.Colaborador.Where(x => x.Id == createDto.ColaboradorId).FirstOrDefaultAsync();

            if (colaborador == null)
                throw new Exception("Colaborador não encontrado.");

            if (string.IsNullOrWhiteSpace(createDto.TextoAdvertencia))
                throw new Exception("Preencha o texto da advertência.");

            var advertencia = new Advertencia
            {
                ColaboradorId = colaborador.Id,
                DataAdvertencia = createDto.DataAdvertencia,
                TextoAdvertencia = createDto.TextoAdvertencia,
                // ArquivoAnexoId = createDto.ArquivoAnexoId,
                // DataCadastro = DateTime.Now,
                // UsuarioCadastro = usuario
            };

            _applicationDbContext.Advertencia.Add(advertencia);
            await _applicationDbContext.SaveChangesAsync();

            return advertencia.Id;
        }

        public async Task<int> Editar(UpdateAdvertenciaDto updateDto, string usuario)
        {
            var colaborador = await _applicationDbContext.Colaborador.Where(x => x.Id == updateDto.ColaboradorId).FirstOrDefaultAsync();

            if (colaborador == null)
                throw new Exception("Colaborador não encontrado.");

            if (string.IsNullOrWhiteSpace(updateDto.TextoAdvertencia))
                throw new Exception("Preencha o texto da advertência.");

            var advertencia = await _applicationDbContext.Advertencia.Where(x => x.Id == updateDto.Id).FirstOrDefaultAsync();

            if (advertencia == null)
                throw new Exception("Advertência não encontrada.");

            // advertencia.ColaboradorId = colaborador.Id;
            advertencia.DataAdvertencia = updateDto.DataAdvertencia;
            advertencia.TextoAdvertencia = updateDto.TextoAdvertencia;
            // advertencia.ArquivoAnexoId = updateDto.ArquivoAnexoId;
            advertencia.DataCadastro = DateTime.Now;
            advertencia.UsuarioCadastro = usuario;

            await _applicationDbContext.SaveChangesAsync();

            return advertencia.Id;
        }

        public async Task Deletar(int id)
        {
            var advertencia = await _applicationDbContext.Advertencia.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (advertencia == null)
                throw new Exception("Advertência não encontrada.");

            _applicationDbContext.Advertencia.Remove(advertencia);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<AdvertenciaDto>> Listar()
        {
            var lista = await _applicationDbContext.Advertencia
                                                            .Select(x => new AdvertenciaDto
                                                            {
                                                                Id = x.Id,
                                                                ArquivoAnexoId = x.ArquivoAnexoId,
                                                                ColaboradorId = x.ColaboradorId,
                                                                ColaboradorNome = x.Colaborador.Nome,
                                                                DataAdvertencia = x.DataAdvertencia,
                                                                TextoAdvertencia = x.TextoAdvertencia,
                                                            })
                                                            .ToListAsync();

            return lista;
        }

        public async Task<AdvertenciaDto> Mostrar(int id)
        {
            var advertencia = await _applicationDbContext.Advertencia
                                                            .Where(x => x.Id == id)
                                                            .Select(x => new AdvertenciaDto
                                                            {
                                                                Id = x.Id,
                                                                ArquivoAnexoId = x.ArquivoAnexoId,
                                                                ColaboradorId = x.ColaboradorId,
                                                                ColaboradorNome = x.Colaborador.Nome,
                                                                DataAdvertencia = x.DataAdvertencia,
                                                                TextoAdvertencia = x.TextoAdvertencia,
                                                            })
                                                            .FirstOrDefaultAsync();

            if (advertencia == null)
                throw new Exception("Advertência não encontrada.");

            return advertencia;
        }

        public async Task<Select2Result> Select2(string term, int? id, string padrao, bool serverSide)
        {
            term = term ?? "";
            var query = _applicationDbContext.Advertencia.AsQueryable();

            if (id != null)
                query = query.Where(x => x.Id == id);
            else
                query = query.Where(x =>
                                    x.Colaborador.Nome.ToUpper().Contains(term.ToUpper())
                                 || x.TextoAdvertencia.ToUpper().Contains(term.ToUpper())
                            ).OrderBy(x => x.Colaborador.Nome);

            if (serverSide)
                query = query.Take(30);

            var dados = await query.Select(x => new Select2
            {
                Id = x.Id,
                Text = $"{x.Id} - Advertência para {x.Colaborador.Nome}"
            })
            .ToListAsync();

            if (serverSide)
            {
                dados.Add(new Select2
                {
                    Id = 0,
                    Text = padrao ?? "Todos"
                });
            }

            return new Select2Result
            {
                Results = dados
            };
        }

        public async Task<IEnumerable<AdvertenciaDto>> ListagemFiltrada(FiltrosAdvertenciaDto filtros)
        {
            var dbQuery = _applicationDbContext.Advertencia.AsQueryable();

            if (filtros.ColaboradorId != null)
                dbQuery = dbQuery.Where(x => x.ColaboradorId == filtros.ColaboradorId);

            if (filtros.DataInicio != null)
                dbQuery = dbQuery.Where(x => x.DataAdvertencia.Date >= filtros.DataInicio.Value.Date);

            if (filtros.DataFim != null)
                dbQuery = dbQuery.Where(x => x.DataAdvertencia.Date <= filtros.DataFim.Value.Date);

            var lista = await dbQuery
                                    .Select(x => new AdvertenciaDto
                                    {
                                        Id = x.Id,
                                        ArquivoAnexoId = x.ArquivoAnexoId,
                                        // ColaboradorId = x.ColaboradorId,
                                        ColaboradorNome = x.Colaborador.Nome,
                                        DataAdvertencia = x.DataAdvertencia,
                                        TextoAdvertencia = x.TextoAdvertencia,
                                    })
                                    .ToListAsync();

            return lista;
        }
    }
}
