﻿using Castle.DynamicProxy;
using Core.CrossCuttingConcerns.Caching;
using Core.Utilities.Interceptors;
using Core.Utilities.IoC;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Aspects.Autofac.Caching
{
    public class CacheRemoveAspect:MethodInterception
    {
        private string _pattern;
        private ICacheManager _cacheManager;

        public CacheRemoveAspect(string pattern)
        {
            _pattern = pattern;
            _cacheManager = ServiceTool.ServiceProvider.GetService<ICacheManager>();
        }

        protected override void OnSuccess(IInvocation ınvocation)// başarılı işlemlerde like add, update gibi cache in yenilenmesi gerekiyor 
        {
            _cacheManager.RemoveByPattern(_pattern); // tutulan cache i sil ve yeni cache i al işleme
        }
    }
}