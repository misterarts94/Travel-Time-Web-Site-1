﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopSpeed.Domain.Models;

namespace TopSpeed.Application.Contracts.Presistence
{
    public interface IBrandRepository : IGenericRepository<Brand>
    {
        Task<List<Brand>> GatAllAsync();
        Task Update (Brand brand);
    }
}
