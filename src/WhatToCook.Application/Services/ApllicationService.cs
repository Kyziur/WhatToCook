using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatToCook.Application.Services
{
    public static class ApllicationServicesDependencies
    {
       public static void RegisterApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<RecipeService>();
            services.AddScoped<RecipeServiceQuery>();
        }
    }
}
