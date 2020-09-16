﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADS.Aplication.Extensions;
using ADS.Domain.Core;
using ADS.Domain.Helpers;
using ADS.Domain.ViewModels;
using ADS.Infrastructure;
using ADS.Infrastructure.Extensions;
using ADS.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADS.Aplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreetController : Controller
    {
        private ADCContext context;
        private readonly IMapper _mapper;

        public StreetController(ADCContext _context, IMapper mapper)
        {
            context = _context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> getStreets([FromQuery] Guid id, [FromQuery] QueryParameters queryParameters)
        {
            var city = await context.Cities
                .ProjectTo<CityViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(c => c.Id == id);

            var query = context.Streets
                .Include(x => x.Houses).Where(s => s.CityId == id)
                .ProjectTo<StreetViewModel>(_mapper.ConfigurationProvider);

            if (!String.IsNullOrEmpty(queryParameters.Active))
            {
                query = query.Sort(queryParameters.Active, queryParameters.Direction);
            }

            var paginationMetadata = query.GetPaginationViewModel(queryParameters);
            var streets = new StreetViewModelsWithPaginationModels
            {
                City = city,
                Streets = query.Skip(paginationMetadata.Skip).Take(queryParameters.PageCount),
                Pagination = paginationMetadata
            };

            return Ok(streets);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStreet([FromBody] StreetViewModel model)
        {
            try
            {
               // IsValidModel(model);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState.GetFullErrorMessage());
                }
                var street = new Street();
                street = _mapper.Map(model, street);
                context.Streets.Add(street);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPut]
        public async Task<IActionResult> EditStreet([FromBody] StreetViewModel model)
        {
           // IsValidModel(model);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.GetFullErrorMessage());
            }
            var street = context.Streets.FirstOrDefault(x => x.Id == model.Id);
            if (street == null)
                return NotFound();
            
            _mapper.Map(model, street);
            
            context.Streets.Update(street);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveStreet(string id)
        {
            try
            {
                var guid = Guid.Parse(id);
                var street = await context.Streets.FirstOrDefaultAsync(x => x.Id == guid);
                if (street == null)
                    return NotFound();
                context.Streets.Remove(street);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        //private void IsValidModel(StreetViewModel model)
        //{
        //    if (!model.IsValidateName(model.Name))
        //    {
        //        ModelState.AddModelError(nameof(model), $"Названия полей должны быть больше 100 или меньше 5");
        //    }
        //}
    }
}