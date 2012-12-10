﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using DotNetShipping.ShippingProviders;

namespace DotNetShipping
{
    public class RateManagerFactory
    {
        /// <summary>
        /// Builds a Rate Manager and adds the providers
        /// </summary>
        /// <returns></returns>
        public static RateManager Build()
        {
            var providers = Assembly.GetAssembly(typeof (IShippingProvider)).GetTypes().Where(x => x.BaseType == typeof (AbstractShippingProvider));

            var rateManager = new RateManager();

            foreach (var provider in providers)
            {
                var instance = Activator.CreateInstance(provider) as IShippingProvider;

                if (instance == null) continue;

                rateManager.AddProvider(instance);
            }

            return rateManager;
        }
    }
}
