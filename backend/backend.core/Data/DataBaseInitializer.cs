﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.core.Data
{
    public static class DataBaseInitializer
    {
        public static void InitialDataBase<TDbContext>(this IHost host, Action<TDbContext, IServiceProvider> databaseSeeder) where TDbContext : DbContext
        {
            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                var dbContext = services.GetRequiredService<TDbContext>();

                var logger = services.GetRequiredService<ILogger<TDbContext>>();

                try
                {
                    logger.LogInformation($"初始化数据库{dbContext.GetType().Name}");
                    if (dbContext.Database.EnsureCreated())
                    {
                        databaseSeeder(dbContext, services);
                    }
                    logger.LogInformation($"初始化数据库{dbContext.GetType().Name}成功");
                }
                catch (Exception e)
                {
                    logger.LogError(e,$"初始化数据库{dbContext.GetType().Name}失败");
                }
            }
        }
    }
}